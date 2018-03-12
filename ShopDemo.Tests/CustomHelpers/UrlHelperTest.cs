using NUnit.Framework;
using ShopDemo.CustomHelpers;

namespace ShopDemo.Tests.CustomHelpers
{
    [TestFixture]
    public class UrlHelperTest
    {
        [Test]
        public void RemoveAccent_removes_polish_dialect_signs()
        {
            string testString = "ąęłćś";

            string expected = "aelcs";

            var result = UrlHelper.RemoveAccent(testString);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToSeoUrl_returns_string_without_url_special_characters() //regex  @"[^0-9a-zA-Z:/]+"
        {
            string testString = "Nowość Procesor intel core+i7";

            string expected = "nowosc-procesor-intel-core-i7";

            var result = UrlHelper.ToSeoUrl(testString);

            Assert.AreEqual(expected, result);
        }
    }
}