$(document).ready(function () {
    $('#Quantity').attr('data-val-number', 'Nieprawidłowa wartość');
    $('#Price').attr('data-val-number', 'Nieprawidłowa wartość');

    var shouldCleanGallery = true;
    $('#btnSave').click(function (e) {
        var existsAsParent = $('#categoryExistsAsParent').val();
        if (existsAsParent == 1 || validateDropdowns() == false) {
            e.preventDefault();
            alertError('Wybierz kategorię!');
        } else {
            if (validateTextbox() == true && validateAttributesDropdowns() == true) {
                shouldCleanGallery = false;
            } else {
                e.preventDefault();
                alertError('Uzupełnij dane!');
            }
        }
    });

    window.onunload = function () {
        if (shouldCleanGallery) {
            $.when(deleteUnsavedGallery()).done(function () {
                return;
            });
        }
    }

    window.onbeforeunload = function () {
        if (shouldCleanGallery) {
            $.when(deleteUnsavedGallery()).done(function () {
                return;
            });
        }
    }
});

function resolvePictures() {
    var pictures = $('#selectedPictures').data('selectedpictures');
    populateThumbsCollection(pictures);
}

function populateThumbsCollection(thumbs) {
    var noImageSrc = $('#noImageSrc').val();
    $.each(thumbs, function (key, thumb) {
        for (var i = 1; i <= 5; i++) {
            var selectedImg = "#img" + i;
            if ($(selectedImg).attr('src') == noImageSrc) {
                $(selectedImg).attr("src", getFullPath(thumb.PhotoThumbPath));
                var arrayIndex = i - 1;
                var html = '';
                html += '<input type="hidden" id="photo' + i + '" name="ProductGallery[' + arrayIndex + '].PhotoPath" value="' + thumb.PhotoPath + '"/>';
                html += '<input type="hidden" id="thumb' + i + '" name="ProductGallery[' + arrayIndex + '].PhotoThumbPath" value="' + thumb.PhotoThumbPath + '"/>';
                $('#photoFileData' + i).html(html);
                break;
            }
        }
    });
}

function getAntiForgeryToken() {
    var token = $('#productForm input[name=__RequestVerificationToken]').val();
    return token;
}

function organiseGallery(target) {
    var photos = [];
    for (var i = 1; i <= 5; i++) {
        if (!isEmpty($('#photoFileData' + i))) {
            var photo = {
                PhotoPath: $('#photo' + i).val(),
                PhotoThumbPath: $('#thumb' + i).val()
            };
            photos.push(photo);
        }
    }
    clearGallery();
    populateThumbsCollection(photos);
}

function clearGallery() {
    var noImageSrc = $('#noImageSrc').val();
    for (var i = 1; i <= 5; i++) {
        var selectedImg = "#img" + i;
        if ($(selectedImg).attr('src') != noImageSrc) {
            $('#img' + i).attr("src", noImageSrc);
            $('#photoFileData' + i).html('');
        }
    }
}

function isEmpty(el) {
    return !$.trim(el.html())
}

function getCategoriesForDropdownJson(categoryId) {
    var url = $('#categoriesDropdowns').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId },
        success: function (result) {
            populateCategoriesDropDown(result);
            validateDropdowns();
        },
        error: function () {
            alertError('Nie można pobrać dostępnych kategorii! Proszę odświeżyć stronę.');
        }
    });
}

var targetDropDown = 1;
function populateCategoriesDropDown(categories) {
    var html = '';
    html += '<div id="ddlDiv_' + targetDropDown + '" class="form-group">';
    if (targetDropDown == 1) {
        html += '<label class="control-label col-md-2">Kategoria</label>';
    } else {
        html += '<label class="control-label col-md-2"></label>';
    }
    html += '<div class="col-md-10">';
    var index = targetDropDown - 1;
    html += '<select class="form-control lg-control category-dropdown" name="SelectedCategories[' + index + ']" data-targetddlnumber="' + targetDropDown + '">';
    html += '<option value="0">Wybierz kategorię</option>';
    for (var i = 0; i < categories.length; i++) {
        html += '<option value="' + categories[i].CategoryId + '">' + categories[i].Name + '</option>';
    }
    html += '</select></div></div>'
    $('#categoriesDropdowns').append(html);

    $('#ddlDiv_' + targetDropDown + ' select').on('change', function () {
        validateDropdowns();
        var selectedCategoryId = $(this).val();
        var targetDdlNumber = $(this).data('targetddlnumber');
        setCategoryDropDownOnChangeAction(selectedCategoryId, targetDdlNumber);
    });
}

function populateSelectedCategories(categories) {
    var lastCategoryId;
    for (var i = 0; i < categories.length; i++) {
        var selectedValue = categories[i].SelectedCategoryId;
        var html = '';
        html += '<div id="ddlDiv_' + targetDropDown + '" class="form-group">';
        if (targetDropDown == 1) {
            html += '<label class="control-label col-md-2">Kategoria</label>';
        } else {
            html += '<label class="control-label col-md-2"></label>';
        }
        html += '<div class="col-md-10">';
        html += '<select class="form-control lg-control category-dropdown" name="SelectedCategories[' + i + ']" data-targetddlnumber="' + targetDropDown + '">';

        for (var j = 0; j < categories[i].Categories.length; j++) {
            if (categories[i].Categories[j].CategoryId == selectedValue) {
                html += '<option value="' + categories[i].Categories[j].CategoryId + '" selected="selected">' + categories[i].Categories[j].Name + '</option>';
                if (i == categories.length - 1) {
                    lastCategoryId = categories[i].Categories[j].CategoryId;
                }
            } else {
                html += '<option value="' + categories[i].Categories[j].CategoryId + '">' + categories[i].Categories[j].Name + '</option>';
            }
        }
        html += '</select></div></div>'
        $('#categoriesDropdowns').append(html);

        $('#ddlDiv_' + targetDropDown + ' select').on('change', function () {
            validateDropdowns();
            var selectedCategoryId = $(this).val();
            var targetDdlNumber = $(this).data('targetddlnumber');
            setCategoryDropDownOnChangeAction(selectedCategoryId, targetDdlNumber);
        });

        if (i == categories.length - 1) {
            setCategoryDropDownOnChangeAction(lastCategoryId, targetDropDown);
        } else {
            targetDropDown++;
        }
    }
}

function setCategoryDropDownOnChangeAction(categoryId, targetDdlNumber) {
    var url = $('#categoryExistsAsParent').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId },
        success: function (result) {
            if (result == true) {
                $('#categoryExistsAsParent').val(1);
                targetDropDown = targetDdlNumber + 1;
                clearForm();
                getCategoriesForDropdownJson(categoryId);
            } else {
                $('#categoryExistsAsParent').val(0);
                targetDropDown = targetDdlNumber + 1;
                clearForm();
                getProductAttributes(categoryId);
            }
        },
        error: function () {
            alertError('Błąd pobierania! Proszę odświeżyć stronę.');
        }
    });
}

function getProductAttributes(categoryId) {
    var url = $('#productAttributes').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId },
        success: function (result) {
            populateProductAttributes(result);
        },
        error: function () {
            alertError('Nie można pobrać specyfikacji technicznej produktu! Proszę odświeżyć stronę.');
        }
    });
}

function resolveSelectedCategories() {
    var selectedCategories = $('#categoriesDropdowns').data('selectedcategories');
    if (selectedCategories.length > 1) {
        var categories = [];
        $.each(selectedCategories, function () {
            categories.push(this);
        });
        var url = $('#categoriesDropdowns').data('resolveselectedcategoriesurl');
        $.ajax({
            url: url,
            type: 'GET',
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            data: { 'selectedCategories': categories },
            success: function (result) {
                populateSelectedCategories(result);
            },
            error: function () {
                alertError('Błąd pobierania! Proszę odświeżyć stronę.');
            }
        });
    } else {
        getCategoriesForDropdownJson(null);
    }
}

function validateDropdowns() {
    var isValid = true;
    $('#categoriesDropdowns .category-dropdown').each(function () {
        if ($(this).val() == 0) {
            $(this).addClass('border-error');
            isValid = false;
        } else {
            $(this).removeClass('border-error');
        }
    });
    return isValid;
}

function validateTextbox() {
    var isValid = true;
    var displayValidationSpecialCharctersError = false;
    $('input[type=text]').each(function () {
        if ($(this).val().trim() == '') {
            $(this).addClass('border-error');
            isValid = false;
        } else {
            var inputValue = $(this).val();
            $(this).val(inputValue.replace('.', ','));
            if ($(this).hasClass('productAttributeValue')) {
                if (validateSpecialCharacters($(this).val())) {
                    $(this).removeClass('border-error');
                } else {
                    displayValidationSpecialCharctersError = true;
                    $(this).addClass('border-error');
                }
            } else {
                $(this).removeClass('border-error');
            }
        }
    });

    if (displayValidationSpecialCharctersError)
        $('#productDetailsError').text('Specyfikacja techniczna produktu zawiera niedozwolone znaki (dozwolone cyfry, litery oraz przecinek i nawiasy kwadratowe).');
    else
        $('#productDetailsError').text('');

    return isValid;
}

function validateSpecialCharacters(value) {
    var isValid = true;
    var pattern = /[^A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ\d\s\[\]\,\+]/;

    if (value.match(pattern))
        isValid = false;

    return isValid;
}

function validateAttributesDropdowns() {
    var isValid = true;
    $('#productAttributes .attributeDropdown').each(function () {
        if ($(this).val() == 0) {
            $(this).addClass('border-error');
            isValid = false;
        } else {
            $(this).removeClass('border-error');
        }
    });
    return isValid;
}