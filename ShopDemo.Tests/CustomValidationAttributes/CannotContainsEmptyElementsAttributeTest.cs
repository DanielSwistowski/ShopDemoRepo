using NUnit.Framework;
using ShopDemo.CustomValidationAttributes;
using System.Collections.Generic;

namespace ShopDemo.Tests.CustomValidationAttributes
{
    [TestFixture]
    public class CannotContainsEmptyElementsAttributeTest
    {
        [Test]
        public void IsValid_returns_false_if_collection_is_null()
        {
            CannotContainsEmptyElementsAttribute validator = new CannotContainsEmptyElementsAttribute();

            Assert.IsFalse(validator.IsValid(null));
        }

        [Test]
        public void IsValid_returns_false_if_value_of_any_item_in_collection_is_zero()
        {
            CannotContainsEmptyElementsAttribute validator = new CannotContainsEmptyElementsAttribute();

            List<int> testCollection = new List<int>();
            testCollection.Add(10);
            testCollection.Add(20);
            testCollection.Add(0);

            Assert.IsFalse(validator.IsValid(testCollection));
        }
        
        [Test]
        public void IsValid_returns_true_if_all_items_values_from_collection_are_not_zero()
        {
            CannotContainsEmptyElementsAttribute validator = new CannotContainsEmptyElementsAttribute();

            List<int> testCollection = new List<int>();
            testCollection.Add(10);
            testCollection.Add(20);
            testCollection.Add(30);

            Assert.IsTrue(validator.IsValid(testCollection));
        }
    }
}
