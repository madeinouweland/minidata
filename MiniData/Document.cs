// Made with ❤ in Berlin by Loek van den Ouweland
namespace MiniData
{
    public class Document<T> where T : new()
    {
        public Document() { }

        public int Id { get; set; }
    }
}
