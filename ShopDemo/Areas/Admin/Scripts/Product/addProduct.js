var modelStateIsValid;
var xhr = null;
$(document).ready(function () {
    var hiddenModelStateValue = $('#modelState').val();
    modelStateIsValid = hiddenModelStateValue == "true" ? true : false;

    if (modelStateIsValid == false) {
        resolveSelectedCategories();
        resolvePictures();
    } else {
        getCategoriesForDropdownJson(null);
    }

    $('#btnBrowse').click(function (e) {
        e.preventDefault();
        var freeSpace = checkFreeSpace();
        if (freeSpace != 0) {
            $('#txtUploadFile').trigger('click');
        } else {
            alertError('Brak wolnych miejsc! Możesz dodać tylko 5 zdjęć produktu');
        }
    });

    $('#txtUploadFile').on('change', function (e) {
        var space = checkFreeSpace();
        if (space == 0) {
            alertError('Brak wolnych miejsc! Możesz dodać tylko 5 zdjęć produktu');
            return false;;
        }
        startLoader();
        var files = e.target.files;
        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                var counter;
                var filesCount = files.length;
                if (filesCount > space) {
                    counter = space;
                } else {
                    counter = filesCount;
                }
                for (var i = 0; i < counter; i++) {
                    data.append("file" + i, files[i]);
                }
                var url = $('#txtUploadFile').data('url');
                xhr = $.ajax({
                    type: "POST",
                    url: url,
                    headers: { '__RequestVerificationToken': getAntiForgeryToken() },
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (result) {
                        populateThumbsCollection(result);
                        stopLoader();
                    },
                    error: function () {
                        stopLoader();
                        alertError('Dodawanie zdjęć nie działa!');
                    }
                });
            }
            else {
                alertError('Błąd! Przeglądarka nie wspiera HTML5 File uploads api');
            }
        }
    });

    $('#formFieldset button').click(function (e) {
        e.preventDefault();
        target = e.target.dataset.target;
        deletePhoto(target);
    });

    $('#Name').blur(function () {
        findCategoryBasedOnProductName();
    });
});

function findCategoryBasedOnProductName() {
    var productName = $('#Name').val();
    var url = $('#findCategoryByProductNameUrl').val();
    $.ajax({
        url: url,
        type: 'GET',
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        data: { 'productName': productName },
        success: function (result) {
            if (result.data != false) {
                targetDropDown = 1;
                clearForm();
                populateSelectedCategories(result.data);
            } else {
                targetDropDown = 1;
                clearForm();
                getCategoriesForDropdownJson(null);
            }
        },
        error: function () {
            alertError('Błąd pobierania! Proszę odświeżyć stronę.');
        }
    });
}

function deleteUnsavedGallery() {
    if (xhr != null) {
        xhr.abort();
        xhr = null;
    }

    var photos = [];
    for (var i = 1; i <= 5; i++) {
        var photoNotExists = isEmpty($('#photoFileData' + i));
        if (!photoNotExists) {
            var photo = {
                PhotoPath: $('#photo' + i).val(),
                PhotoThumbPath: $('#thumb' + i).val()
            };
            photos.push(photo);
        }
    }

    if (photos.length > 0) {
        var url = $('#deleteUnsavedGalleryUrl').val();
        $.ajax({
            type: "POST",
            url: url,
            contentType: "application/json;charset=utf-8",
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            dataType: "json",
            data: JSON.stringify(photos),
            success: function (result) {
                return result;
            }
        });
    }
}

function checkFreeSpace() {
    var freeSpace = 0;
    var noImageSrc = $('#noImageSrc').val();
    $('.productPhoto').each(function () {
        if (($(this).attr('src') == noImageSrc)) {
            freeSpace++;
        }
    });
    return freeSpace;
}

function clearForm() {
    var divnumber = targetDropDown;
    while ($('#ddlDiv_' + divnumber).length > 0) {
        $('#ddlDiv_' + divnumber).remove();
        divnumber++;
    }
    if ($('#productAttributes').length > 0) {
        $('#productAttributes').html('');
    }
}

function deletePhoto(target) {
    var photoNotExists = isEmpty($('#photoFileData' + target));
    if (!photoNotExists) {
        var photo = {
            PhotoPath: $('#photo' + target).val(),
            PhotoThumbPath: $('#thumb' + target).val()
        };
        var url = $('#deletePhotoUrl').val();
        $.ajax({
            type: "POST",
            url: url,
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            data: JSON.stringify(photo),
            success: function (result) {
                if (result.success == true) {
                    $('#photoFileData' + target).html('');
                    var noImageSrc = $('#noImageSrc').val();
                    $('#img' + target).attr("src", noImageSrc);
                    organiseGallery(target);
                } else {
                    alertError('Błąd! Nie można usunąć zdjęcia');
                }
            },
            error: function () {
                alertError('Błąd! Nie można usunąć zdjęcia');
            }
        });
    }
}

function populateProductAttributes(attributesList) {
    var html = '';
    for (var i = 0; i < attributesList.length; i++) {
        html += '<div class="form-group">';
        html += '<label class="control-label col-md-2 product-attribute-label" id="attribute-label-' + i + '">' + attributesList[i].Name + '</label>';
        html += '<div class="col-md-10">';
        html += '<input type="hidden" id="attribute-name-' + i + '" name="ProductDetails[' + i + '].DetailName" value ="' + attributesList[i].Name + '" />';
        if (attributesList[i].AttributeValues.length > 0) {
            html += '<select class="form-control lg-control attributeDropdown" name="ProductDetails[' + i + '].DetailValue" id="attribute-input-' + i + '">';
            html += '<option value="0">Wybierz...</option>';
            for (var j = 0; j < attributesList[i].AttributeValues.length; j++) {
                html += '<option value="' + attributesList[i].AttributeValues[j] + '">' + attributesList[i].AttributeValues[j] + '</option>';
            }
            html += '</select>'
        } else {
            html += '<input type="text" name="ProductDetails[' + i + '].DetailValue" class = "productAttributeValue form-control lg-control" id="attribute-input-' + i + '" />';
        }
        html += '</div>';
        html += '</div>';
    }
    $('#productAttributes').html(html);

    $('input[type=text]').each(function () {
        $(this).blur(function () {
            validateTextbox();
        });
    });


    $('.attributeDropdown').each(function () {
        $(this).change(function () {
            validateAttributesDropdowns();
        });
    });

    if (!modelStateIsValid) {
        resolveProductAttributes();
    }
}

function resolveProductAttributes() {
    var productAttributes = $('#productAttributes').data('selectedattributes');
    var productAttributesCount = productAttributes.length;
    if (productAttributesCount > 0) {
        var attributesCount = $('#productAttributes .form-group .product-attribute-label').length;
        for (var i = 0; i < attributesCount; i++) {
            var attributeName = $('#attribute-name-' + i).val();
            for (var j = 0; j < productAttributesCount; j++) {
                if (productAttributes[j].DetailName == attributeName) {
                    $('#attribute-input-' + i).val(productAttributes[j].DetailValue);
                    break;
                }
            }
        }
    }
}

function startLoader() {
    $('#ajaxLoader').show();
    $('#btnBrowse').hide();
}

function stopLoader() {
    $('#ajaxLoader').hide();
    $('#btnBrowse').show();
}