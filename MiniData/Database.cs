// Made with ❤ in Berlin by Loek van den Ouweland
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniData
{
    public class Database : IDatabase
    {
        private Streamer _streamer = new Streamer();
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private string GetFileNameFromType(Type type)
        {
            return $"{type.Name}.mini";
        }

        public async Task<bool> DocumentExists<T>() where T : Document<T>, new()
        {
            await _semaphoreSlim.WaitAsync();
            var stream = await _streamer.StreamForReadAsync(GetFileNameFromType(typeof(T)));
            _semaphoreSlim.Release();
            return stream != null;
        }

        public async Task<List<T>> GetAll<T>() where T : Document<T>,new()
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                var stream = await _streamer.StreamForReadAsync(GetFileNameFromType(typeof(T)));
                if (stream != null)
                {
                    var serializer = new Serializer<T>();
                    return serializer.DeserializeFromStream(stream).OrderBy(x => x.Id).ToList();
                }
                return new List<T>();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<T> Save<T>(T document) where T : Document<T>, new()
        {
            var list = await GetAll<T>();
            await _semaphoreSlim.WaitAsync();
            var exists = list.FirstOrDefault(x => x.Id == document.Id);
            if (exists != null)
            {
                list.Remove(exists);
                list.Add(document);
            }
            else
            {
                if (list.Count == 0)
                {
                    document.Id = 1;
                }
                else
                {
                    document.Id = list.Max(x => x.Id) + 1;
                }
                list.Add(document);
            }

            try
            {
                var serializer = new Serializer<T>();
                using (var input = serializer.SerializeAsStream(list.OrderBy(x => x.Id)))
                {
                    using (var output = await _streamer.StreamForWriteAsync(GetFileNameFromType(typeof(T))))
                    {
                        await input.CopyToAsync(output);
                        await output.FlushAsync();
                    }
                }
                return document;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<T> Delete<T>(T document) where T : Document<T>, new()
        {
            var list = await GetAll<T>();
            await _semaphoreSlim.WaitAsync();
            var exists = list.FirstOrDefault(x => x.Id == document.Id);
            if (exists != null)
            {
                list.Remove(exists);
            }
            else
            {
                return null; // could not find and thus not delete
            }

            try
            {
                var serializer = new Serializer<T>();
                using (var json = serializer.SerializeAsStream(list))
                {
                    using (var stream = await _streamer.StreamForWriteAsync(GetFileNameFromType(typeof(T))))
                    {
                        await json.CopyToAsync(stream);
                        await stream.FlushAsync();
                    }
                }
                return document;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
