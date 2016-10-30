using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniData.Test.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MiniData.Test
{
    [TestClass]
    public class StreamTest
    {
        [TestMethod]
        public async Task ReturnNullForNotExistingFile()
        {
            var streamer = new Streamer();
            var stream = await streamer.StreamForReadAsync("Employee.mini_notexist");
            Assert.IsNull(stream);
        }

        [TestMethod]
        public async Task ReturnEmptyListForNonexistingDocument()
        {
            var database = new Database();
            var all = await database.GetAll<NonExistingDocument>();
            Assert.IsTrue(all.Count == 0);
        }

        [TestMethod]
        public async Task SaveEmployees()
        {
            var database = new Database();

            var vera = new Employee { Name = "Vera", Age = 40, Level = Level.Novice };
            var saved = await database.Save(vera);
            Assert.IsTrue(saved.Id == 1);

            var chuck = new Employee { Name = "Chuck", Age = 42, Sprints = new List<int>() };
            var saved2 = await database.Save(chuck);
            Assert.IsTrue(saved2.Id == 2);

            // save again
            var saved3 = await database.Save(chuck);
            Assert.IsTrue(saved3.Id == 2);

            var dave = new Employee { Name = "Dave", Age = 26, Sprints = new List<int> { 4, 5 } };
            var saved4 = await database.Save(dave);
            Assert.IsTrue(saved4.Id == 3);

            var streamer = new Streamer();
            var stream = await streamer.StreamForReadAsync("Employee.mini");

            var sr = new StreamReader(stream);
            var text = sr.ReadToEnd();

            var result = "1|Vera|40|Novice|\n2|Chuck|42|Expert|[]\n3|Dave|26|Expert|[4,5]";
            Assert.AreEqual(text, result);
        }
    }
}
