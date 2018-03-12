using DataAccessLayer.Models;
using ShopDemo.CustomValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ShopDemo.ViewModels
{
    public class BasicProductDataViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int ProductId { get; set; }

        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }

        [Display(Name = "Opis")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Cena")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }

    public class AddProductViewModel
    {
        [Required(ErrorMessage = "Pole wymagane")]
        [StringLength(40, ErrorMessage = "Nazwa produktu jest za długa. Maksymalna ilość to 40 znaków")]
        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "Pole wymagane")]
        [Display(Name = "Opis")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Pole wymagane")]
        [Display(Name = "Cena")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        [RegularExpression(@"(\d+?\,\d{1,2}|\d+?\.\d{1,2})|(\d+)|(\.\d{1,2})|(\,\d{1,2})", ErrorMessage = "Nieprawidłowa wartość")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Pole wymagane")]
        [RegularExpression(@"\d+", ErrorMessage = "Nieprawidłowa wartość")]
        [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowa wartość")]
        [Display(Name = "Ilość")]
        public int Quantity { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool IsInOffer { get { return true; } }

        [HiddenInput(DisplayValue = false)]
        public DateTime CreatedAt { get { return DateTime.Now; } }

        public List<PhotoViewModel> ProductGallery { get; set; }

        [CannotContainsEmptyElements(ErrorMessage ="Wybierz kategorie")]
        public List<int> SelectedCategories { get; set; }

        [CannotContainSpecialCharacters(ErrorMessage = "Specyfikacja techniczna produktu zawiera niedozwolone znaki (dozwolone cyfry, litery oraz przecinek i nawiasy kwadratowe)")]
        public List<ProductDetailViewModel> ProductDetails { get; set; }
    }

    public class EditProductViewModel : ViewModelBase
    {
        [HiddenInput(DisplayValue = false)]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Pole wymagane")]
        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "Pole wymagane")]
        [Display(Name = "Opis")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Pole wymagane")]
        [Display(Name = "Cena")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        [RegularExpression(@"(\d+?\,\d{1,2}|\d+?\.\d{1,2})|(\d+)|(\.\d{1,2})|(\,\d{1,2})", ErrorMessage = "Nieprawidłowa wartość")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Pole wymagane")]
        [RegularExpression(@"\d+", ErrorMessage = "Nieprawidłowa wartość")]
        [Range(0, int.MaxValue, ErrorMessage = "Nieprawidłowa wartość")]
        [Display(Name = "Ilość")]
        public int Quantity { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool IsInOffer { get; set; }

        [HiddenInput(DisplayValue = false)]
        public DateTime CreatedAt { get; set; }

        public List<PhotoViewModel> ProductGallery { get; set; }

        [CannotContainsEmptyElements(ErrorMessage = "Wybierz kategorie")]
        public List<int> SelectedCategories { get; set; }

        [CannotContainSpecialCharacters(ErrorMessage = "Specyfikacja techniczna produktu zawiera niedozwolone znaki (dozwolone cyfry, litery oraz przecinek i nawiasy kwadratowe)")]
        public List<ProductDetailViewModel> ProductDetails { get; set; }
    }

    public class ProductDeletedFromOfferViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int ProductId { get; set; }

        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }

        [Display(Name = "Cena")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Ilość")]
        public int Quantity { get; set; }

        [Display(Name = "Data wycofania")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime DeletedFromOfferDate { get; set; }
    }

    public class AdminIndexProductViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int ProductId { get; set; }

        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }

        [Display(Name = "Cena")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Ilość")]
        public int Quantity { get; set; }

        [Display(Name = "Data dodania")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Zdjęcie")]
        public string PreviewThumbPath { get; set; }

        public bool IsInPromotion { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal NewPrice { get; set; }
    }

    public class ProductOnPromotionViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int ProductId { get; set; }

        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }

        [Display(Name = "Cena")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Currency)]
        public decimal OldPrice { get; set; }

        [Display(Name = "Rabat(%)")]
        public int DiscountQuantity { get; set; }

        [Display(Name = "Cena po rabacie")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Currency)]
        public decimal NewPrice { get; set; }

        [Display(Name = "Status promocji")]
        public ProductDiscountStatusViewModel Status { get; set; }
    }

    public class AdminProductDetailsViewModel : CustomerProductDetailsViewModel
    {
        [Display(Name = "Data dodania")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Data wycofania")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime DeletedFromOfferDate { get; set; }
    }

    public class CustomerProductDetailsViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int ProductId { get; set; }

        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }

        [Display(Name = "Opis")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Cena")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal OldPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal NewPrice { get; set; }

        [Display(Name = "Ilość")]
        public int Quantity { get; set; }

        public bool IsInPromotion { get; set; }

        [Display(Name = "Galeria")]
        public List<Photo> ProductGallery { get; set; }

        public List<ProductDetailViewModel> ProductDetails { get; set; }

        [Display(Name = "Kategorie")]
        public IEnumerable<BaseCategoryViewModel> ProductCategories { get; set; }
    }

    public class CustomerProductViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int ProductId { get; set; }

        [Display(Name = "Nazwa produktu")]
        public string Name { get; set; }

        [Display(Name = "Cena")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal OldPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal NewPrice { get; set; }

        public int Quantity { get; set; }

        [Display(Name = "Ocena produktu")]
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public double ProductRate { get; set; }

        public bool IsInPromotion { get; set; }

        public string PreviewThumbPath { get; set; }
    }

    public class ProductThumbnailViewModel
    {
        public int ProductId { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal OldPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal NewPrice { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public double ProductRate { get; set; }

        public string PreviewThumbPath { get; set; }
    }
}