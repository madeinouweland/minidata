// Made with ❤ in Berlin by Loek van den Ouweland
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MiniData
{
    public class Tokenizer
    {
        public List<CollectionToken> Parse(string text)
        {
            var list = new List<CollectionToken>();
            var lines = text.Split('\n');

            foreach (var line in lines)
            {
                var lt = new CollectionToken(line, @"(?<!(?<!\\)*\\)\|"); // split on pipe
                list.Add(lt);
            }

            return list;
        }
    }

    public class ValueToken : IToken
    {
        private string _value;

        public ValueToken(string value)
        {
            _value = value
                .Replace("\\\\n", "\n")
                .Replace("\\|", "|")
                .Replace("\\,", ",")
                .Replace("\\[", "[")
                .Replace("\\]", "]");
        }

        public override string ToString()
        {
            return _value;
        }

        public bool HasValue => !string.IsNullOrEmpty(_value);
    }

    public class CollectionToken : IToken, IEnumerable<IToken>
    {
        public CollectionToken(string collection, string regEx)
        {
            if (collection == "" || collection == "[]") return; // empty collection, no need to add values;

            if (collection.StartsWith("[") && collection.EndsWith("]"))
            {
                collection = collection.Substring(1, collection.Length - 2); // strip [ and ]
            }

            var values = Regex.Split(collection, regEx);
            foreach (var value in values)
            {
                if (value.StartsWith("[") && value.EndsWith("]"))
                {
                    _tokens.Add(new CollectionToken(value, @"(?<!(?<!\\)*\\)\,(?=[^\]]*(?:\[|$))")); // split on commas outside [ and ]
                }
                else
                {
                    _tokens.Add(new ValueToken(value));
                }
            }
        }

        public IToken this[int index] => _tokens[index];

        private List<IToken> _tokens = new List<IToken>();

        public IEnumerator<IToken> GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        public bool HasValue => _tokens != null;
    }
}
