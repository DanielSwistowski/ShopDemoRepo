using NUnit.Framework;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System.Collections.Generic;

namespace ShopDemo.Tests.CustomValidationAttributes
{
    [TestFixture]
    public class CannotContainSpecialCharactersAttributeTest
    {
        [Test]
        public void IsValid_returns_false_if_any_DetailValue_from_collection_contains_not_valid_characters() //regex @"[^A-Za-zćęłńóśźżĄĘŁŃÓŚŹŻ0-9,\s\[\]]"
        {
            List<ProductDetailViewModel> testValues = new List<ProductDetailViewModel>();
            testValues.Add(new ProductDetailViewModel { DetailName="Name1", DetailValue = "Value1" });
            testValues.Add(new ProductDetailViewModel { DetailName = "Name2", DetailValue = "Val//ue2" });

            CannotContainSpecialCharactersAttribute validator = new CannotContainSpecialCharactersAttribute();

            Assert.IsFalse(validator.IsValid(testValues));
        }

        [Test]
        public void IsValid_returns_true_if_all_DetailValue_items_are_valid()
        {
            List<ProductDetailViewModel> testValues = new List<ProductDetailViewModel>();
            testValues.Add(new ProductDetailViewModel { DetailName = "Name1", DetailValue = "Value1" });
            testValues.Add(new ProductDetailViewModel { DetailName = "Name2", DetailValue = "Valuąęśże2" });

            CannotContainSpecialCharactersAttribute validator = new CannotContainSpecialCharactersAttribute();

            Assert.IsTrue(validator.IsValid(testValues));
        }
    }
}
