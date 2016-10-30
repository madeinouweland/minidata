// Made with ❤ in Berlin by Loek van den Ouweland
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniData.Test.Entities;
using System.Linq;

namespace MiniData.Test
{
    [TestClass]
    public class SerializeTest
    {
        private string employee_minidata = "1|Vera|25|Expert|[1,2,3]\n2|Chuck|21|Novice|[4,5]\n|Dave|35|Expert|";
        private string employee_minidata_broken_vera = "1|Vera********,2,3]\n2|Chuck|21|Novice|[4,5]";

        [TestMethod]
        public void DeserializeSucceeds()
        {
            var ser = new Serializer<Employee>();
            var stream = ser.FromText(employee_minidata);
            var employees = ser.DeserializeFromStream(stream).ToList();
            Assert.IsTrue(employees.Count == 3);
        }

        [TestMethod]
        public void DeserializeBrokenDataReturnsChuck()
        {
            var ser = new Serializer<Employee>();
            var stream = ser.FromText(employee_minidata_broken_vera);
            var employees = ser.DeserializeFromStream(stream).ToList();
            Assert.IsTrue(employees.Count == 1);
        }

        [TestMethod]
        public void DeserializeBrokenDataReturnsEmptyList()
        {
            var mini = "pietje_puk";
            var ser = new Serializer<Employee>();
            var stream = ser.FromText(mini);
            var employees = ser.DeserializeFromStream(stream).ToList();
            Assert.IsTrue(employees.Count == 0);
        }
    }
}
