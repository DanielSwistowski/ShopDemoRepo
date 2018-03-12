using NUnit.Framework;
using ShopDemo.CustomHelpers;

namespace ShopDemo.Tests.CustomHelpers
{
    [TestFixture]
    public class DateHelperTest
    {
        [Test]
        public void GetMonthNumber_returns_correct_number()
        {
            Assert.AreEqual(1, DateHelper.GetMonthNumber("styczeń"));
            Assert.AreEqual(2, DateHelper.GetMonthNumber("luty"));
            Assert.AreEqual(3, DateHelper.GetMonthNumber("marzec"));
            Assert.AreEqual(4, DateHelper.GetMonthNumber("kwiecień"));
            Assert.AreEqual(5, DateHelper.GetMonthNumber("maj"));
            Assert.AreEqual(6, DateHelper.GetMonthNumber("czerwiec"));
            Assert.AreEqual(7, DateHelper.GetMonthNumber("lipiec"));
            Assert.AreEqual(8, DateHelper.GetMonthNumber("sierpień"));
            Assert.AreEqual(9, DateHelper.GetMonthNumber("wrzesień"));
            Assert.AreEqual(10, DateHelper.GetMonthNumber("październik"));
            Assert.AreEqual(11, DateHelper.GetMonthNumber("listopad"));
            Assert.AreEqual(12, DateHelper.GetMonthNumber("grudzień"));
        }
    }
}
