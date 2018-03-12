using NUnit.Framework;
using ShopDemo.CustomValidationAttributes;

namespace ShopDemo.Tests.CustomValidationAttributes
{
    [TestFixture]
    public class MustBeTrueAttributeTest
    {
        [Test]
        public void IsValid_returns_true_if_value_is_valid()
        {
            var testValue = true;

            MustBeTrueAttribute validator = new MustBeTrueAttribute();

            Assert.IsTrue(validator.IsValid(testValue));
        }

        [Test]
        public void IsValid_returns_false_if_value_is_not_valid()
        {
            var testValue = false;

            MustBeTrueAttribute validator = new MustBeTrueAttribute();

            Assert.IsFalse(validator.IsValid(testValue));
        }

        [Test]
        public void IsValid_returns_false_if_value_is_not_boolean_type()
        {
            var testValue = "test";

            MustBeTrueAttribute validator = new MustBeTrueAttribute();

            Assert.IsFalse(validator.IsValid(testValue));
        }
    }
}