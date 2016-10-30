// Made with ❤ in Berlin by Loek van den Ouweland
using System.Collections.Generic;

namespace MiniData.Test.Entities
{
    public class Employee : Document<Employee>
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Level Level { get; set; }
        public List<int> Sprints { get; set; }
    }
}
