using NUnit.Framework;
using ShopDemo.CustomHelpers;
using System.Linq;

namespace ShopDemo.Tests.CustomHelpers
{
    [TestFixture]
    public class DictionaryHelperTest
    {
        [Test]
        public void ToDictionary_returns_null_if_provided_query_string_is_null_or_empty()
        {
            string queryString = "";
            string queryString2 = null;

            Assert.IsNull(queryString.ToDictionary());
            Assert.IsNull(queryString2.ToDictionary());
        }

        [Test]
        public void ToDictionary_returns_key_invalidFiltr_with_query_parameters_as_value_if_queryString_is_invalid_and_exception_is_thrown()
        {
            string queryString = "taktowanie.liczba+rdzeni-4_6.socket-s775_lga2011";

            var result = queryString.ToDictionary();

            Assert.IsNotNull(result["invalidFiltr"]);
        }

        [Test]
        public void ToDictionary_returns_dictionary_with_search_parameters()
        {
            string queryString = "taktowanie-1000_2000.liczba+rdzeni-4_6.socket-s775_lga2011";

            var result = queryString.ToDictionary();
            var values1 = result["taktowanie"].ToList();
            var values2 = result["liczba+rdzeni"].ToList();
            var values3 = result["socket"].ToList();

            Assert.AreEqual("1000", values1[0]);
            Assert.AreEqual("2000", values1[1]);

            Assert.AreEqual("4", values2[0]);
            Assert.AreEqual("6", values2[1]);

            Assert.AreEqual("s775", values3[0]);
            Assert.AreEqual("lga2011", values3[1]);
        }
    }
}
