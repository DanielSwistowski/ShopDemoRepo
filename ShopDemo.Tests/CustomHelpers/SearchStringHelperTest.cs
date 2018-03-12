using NUnit.Framework;
using ShopDemo.CustomHelpers;
using System.Collections.Generic;

namespace ShopDemo.Tests.CustomHelpers
{
    [TestFixture]
    public class SearchStringHelperTest
    {
        [Test]
        public void ToSearchString_returns_empty_string_if_provided_dictionary_is_null()
        {
            Dictionary<string, IEnumerable<string>> testDictionary = null;

            var result = testDictionary.ToSearchString();

            Assert.AreEqual("", result);
        }

        [Test]
        public void ToSearchString_returns_correct_search_string()
        {
            string expectedSearchString = "taktowanie-1000_2000.liczba+rdzeni-4_6.socket-s775_lga2011";

            Dictionary<string, IEnumerable<string>> testDictionary = new Dictionary<string, IEnumerable<string>>();
            testDictionary.Add("taktowanie", new List<string> { "1000", "2000" });
            testDictionary.Add("liczba+rdzeni", new List<string> { "4", "6" });
            testDictionary.Add("socket", new List<string> { "s775", "lga2011" });

            var result = testDictionary.ToSearchString();

            Assert.AreEqual(expectedSearchString, result);
        }
    }
}