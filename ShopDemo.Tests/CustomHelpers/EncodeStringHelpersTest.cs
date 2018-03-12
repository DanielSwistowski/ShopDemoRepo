using NUnit.Framework;
using ShopDemo.CustomHelpers;

namespace ShopDemo.Tests.CustomHelpers
{
    [TestFixture]
    public class EncodeStringHelpersTest
    {
        [Test]
        public void ConvertStringToUtf8_returns_correct_string()
        {
            string testValue = "abcdef";

            string expectedValue = "=?UTF-8?B?" + "YWJjZGVm" + "?=";

            var result = EncodeStringHelpers.ConvertStringToUtf8(testValue);

            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public void ToBase64_returns_correct_string()
        {
            string testValue = "abcdef";

            string expectedValue = "YWJjZGVm";

            var result = EncodeStringHelpers.ToBase64(testValue);

            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public void FromBase64_returns_correct_string()
        {
            string testValue = "YWJjZGVm";

            string expectedValue = "abcdef";

            var result = EncodeStringHelpers.FromBase64(testValue);

            Assert.AreEqual(expectedValue, result);
        }
    }
}