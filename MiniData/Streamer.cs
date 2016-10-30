// Made with ❤ in Berlin by Loek van den Ouweland
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace MiniData
{
    public class Streamer
    {
        private StorageFolder DataFolder => ApplicationData.Current.LocalFolder;

        public async Task<Stream> StreamForReadAsync(string fileName)
        {
            try
            {
                var file = await DataFolder.GetFileAsync(fileName);
                return await file.OpenStreamForReadAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<Stream> StreamForWriteAsync(string fileName)
        {
            try
            {
                var file = await DataFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                return await file.OpenStreamForWriteAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task Delete(string fileName)
        {
            try
            {
                var file = await DataFolder.GetFileAsync(fileName);
                await file.DeleteAsync();
            }
            catch
            {
            }
        }
    }
}