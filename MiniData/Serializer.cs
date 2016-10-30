// Made with ❤ in Berlin by Loek van den Ouweland
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MiniData
{
    public class Serializer<T> where T : new()
    {
        private PropertyInfo[] _propInfoT => typeof(T).GetProperties();

        private bool IsIEnumerable(PropertyInfo prop)
        {
            return ((typeof(string) != prop.PropertyType) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType));
        }

        public Stream FromText(string text)
        {
            var ms = new MemoryStream();
            ms.Write(new ASCIIEncoding().GetBytes(text), 0, text.Length);
            ms.Position = 0;
            return ms;
        }

        public Stream SerializeAsStream(IEnumerable<T> list)
        {
            var text = String.Join("\n", list.Select(x => GetValue(_propInfoT, x)));
            return FromText(text);
        }

        private string GetValue(PropertyInfo[] props, object raw)
        {
            var values = new List<object>();
            foreach (var prop in props.OrderByDescending(x => x.Name == "Id"))
            {
                if (IsIEnumerable(prop))
                {
                    // ienumerable
                    var e = (IEnumerable)prop.GetValue(raw, null);
                    if (e == null)
                    {
                        values.Add("");
                    }
                    else
                    {
                        values.Add($"[{String.Join(",", e.Cast<object>())}]");
                    }
                }
                else
                {
                    var val = prop.GetValue(raw);
                    values.Add(Escape(prop.GetValue(raw) + ""));
                }
            }
            var line = String.Join("|", values);
            return line;
        }

        private string Escape(string value)
        {
            return value
                .Replace("\n", "\\\\n")
                .Replace("|", "\\|")
                .Replace(",", "\\,")
                .Replace("[", "\\[")
                .Replace("]", "\\]");
        }

        public IEnumerable<T> DeserializeFromStream(Stream stream)
        {
            var sr = new StreamReader(stream);
            var text = sr.ReadToEnd();
            var list = new List<T>();
            var lines = new Tokenizer().Parse(text);

            foreach (var line in lines.Where(x => x.Count() > 0))
            {
                try
                {
                    var fieldCounter = 0;
                    var entity = new T();
                    foreach (var prop in _propInfoT.OrderByDescending(x => x.Name == "Id"))
                    {
                        SetValue(line[fieldCounter++], entity, prop);
                    }
                    list.Add(entity);
                }
                catch
                {
                    // Ignore broken line
                }
            }

            return list;
        }

        private void SetValue(object value, T entity, PropertyInfo prop)
        {
            var token = (IToken)value;
            if (token.HasValue)
            {
                if (IsIEnumerable(prop))
                {
                    var args = prop.PropertyType.GetGenericArguments();
                    Type typedList = typeof(List<>).MakeGenericType(args);
                    object result = Activator.CreateInstance(typedList);
                    foreach (var item in (CollectionToken)value)
                    {
                        var val = Convert.ChangeType(item.ToString(), args[0]);
                        result.GetType().GetMethod("Add").Invoke(result, new[] { val });
                    };
                    prop.SetValue(entity, result, null);
                }
                else
                {
                    var valueToken = (ValueToken)value;
                    if (IsEnum(prop))
                    {
                        try
                        {
                            var val = Enum.Parse(prop.PropertyType, value.ToString());
                            prop.SetValue(entity, val, null);
                        }
                        catch
                        {
                            // unable to create enum from string. Ignore
                        }
                    }
                    else
                    {
                        prop.SetValue(entity, Convert.ChangeType(value.ToString(), prop.PropertyType), null);
                    }
                }
            }
        }

        private bool IsEnum(PropertyInfo prop)
        {
            return typeof(Enum).IsAssignableFrom(prop.PropertyType);
        }
    }
}
