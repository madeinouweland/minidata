// Made with ❤ in Berlin by Loek van den Ouweland
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniData
{
    public interface IDatabase
    {
        Task<bool> DocumentExists<T>() where T : Document<T>, new();
        Task<List<T>> GetAll<T>() where T : Document<T>, new();
        Task<T> Save<T>(T document) where T : Document<T>, new();
        Task<T> Delete<T>(T document) where T : Document<T>, new();
    }
}
