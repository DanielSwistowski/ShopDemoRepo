var photosTempArray = [];
var xhr = null;
$(document).ready(function () {
    resolvePictures();
    resolveSelectedCategories();

    var target = '';
    $('.change').click(function (e) {
        e.preventDefault();
        target = e.target.dataset.target;
        $('#txtUploadFile').trigger('click');
    });

    $('.remove').click(function (e) {
        target = e.target.dataset.target;
        removePhotoIfExistsInPhotoTempArray(target);
        cleanPhotoFileData(target);
        changeToDefaultPhoto(target);
        organiseGallery(target);
    });

    $('#txtUploadFile').on('change', function (e) {
        startLoader();
        var files = e.target.files;
        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                var filesCount = files.length;
                for (var i = 0; i < filesCount; i++) {
                    data.append("file" + i, files[i]);
                }
                var url = $('#txtUploadFile').data('url');
                xhr = $.ajax({
                    type: "POST",
                    headers: { '__RequestVerificationToken': getAntiForgeryToken() },
                    url: url,
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (result) {
                        addPhotosToTempArray(result);
                        removePhotoIfExistsInPhotoTempArray(target);
                        cleanPhotoFileData(target);
                        changeToDefaultPhoto(target);
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
});

function deleteUnsavedGallery() { 
    if (xhr != null) {
        xhr.abort();
        xhr = null;
    }

    if (photosTempArray.length > 0) {
        var url = $('#deleteUnsavedGalleryUrl').val();
        $.ajax({
            type: "POST",
            url: url,
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            data: JSON.stringify(photosTempArray),
            success: function (result) {
                return result;
            }
        });
    }
}

function removePhotoIfExistsInPhotoTempArray(target) {
    var photo = {
        PhotoPath: $('#photo' + target).val(),
        PhotoThumbPath: $('#thumb' + target).val()
    };

    var photoIndex = -1;
    for (var i = 0; i < photosTempArray.length; i++) {
        if (photosTempArray[i].PhotoPath == photo.PhotoPath && photosTempArray[i].PhotoThumbPath == photo.PhotoThumbPath) {
            photoIndex = i;
            break;
        }
    }

    if (photoIndex > -1) {
        deletePhoto(photo, photoIndex);
    }
}

function deletePhoto(photo, photoIndex) {
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
                photosTempArray.splice(photoIndex, 1);
            } else {
                alertError('Błąd! Nie można usunąć zdjęcia');
            }
        },
        error: function () {
            alertError('Błąd! Nie można usunąć zdjęcia');
        }
    });
}

function populateProductAttributes(attributesList) {
    var html = '';
    for (var i = 0; i < attributesList.length; i++) {
        html += '<div class="form-group">';
        html += '<label class="control-label col-md-2 product-attribute-label" id="attribute-label-' + i + '">' + attributesList[i].Name + '</label>';
        html += '<div class="col-md-10">';
        html+='<input type="hidden" id="attribute-name-' + i + '" name="ProductDetails[' + i + '].DetailName" value ="' + attributesList[i].Name + '" />';
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

    resolveProductAttributes();
}

function resolveProductAttributes(){
    $('#removedProductAttributes').html('');
    var productAttributes = $('#productAttributes').data('selectedattributes');
    var productAttributesCount = productAttributes.length;
    var headerOfRemovedAttributesExists = false;
    if(productAttributesCount > 0){
        var attributesCount = $('#productAttributes .form-group .product-attribute-label').length;
        for (var i = 0; i < productAttributesCount; i++) {
            var productDetailName = productAttributes[i].DetailName;
            var productDetailValue = productAttributes[i].DetailValue;
            var attributeExistsInSelectedCategory = false;
            for (var j = 0; j < attributesCount; j++) {
                var attributeName = $('#attribute-name-' + j).val();
                if(productDetailName == attributeName){
                    var attributeExistsDropDownList = attributeExistsOnList(j, productDetailValue);
                    if (attributeExistsDropDownList == true) {
                        $('#attribute-input-' + j).val(productDetailValue);
                    } else {
                        $('#attribute-input-' + j).val(0);
                    }
                    attributeExistsInSelectedCategory = true;
                    break;
                }
            }
            if(attributeExistsInSelectedCategory == false){
                var html = '';
                if (headerOfRemovedAttributesExists == false) {
                    html = '<br /><h4>Cechy produktu, które zostaną usunięte</h4>';
                    headerOfRemovedAttributesExists = true;
                }
                html += '<div class="form-group">';
                html += '<label class="control-label col-md-2 product-attribute-label">'+productDetailName+'</label>';
                html += '<div class="col-md-10">';
                html += '<input type="text" class="form-control lg-control" value="'+productDetailValue+'" disabled="disabled" />';
                html += '</div>';
                html += '</div>';
                $('#removedProductAttributes').append(html);
            }
        }
    }
}

function attributeExistsOnList(dropDownNumber, attribute) {
    var exists = false;
    var dropDownItems = $('#attribute-input-' + dropDownNumber);
    var dropDownItemsCount = $('#attribute-input-' + dropDownNumber + ' option').length;
    if (dropDownItemsCount > 1) {
        $('#attribute-input-' + dropDownNumber + ' option').each(function () {
            var option = $(this).val();
            if (option == attribute) {
                exists = true;
                return;
            }
        });
    } else {//to nie drop down
        exists = true;
    }
    return exists;
}

function addPhotosToTempArray(photos) {
    $.each(photos, function () {
        photosTempArray.push(this);
    });
}

function clearForm() {
    var divnumber = targetDropDown;
    while ($('#ddlDiv_' + divnumber).length > 0) {
        $('#ddlDiv_' + divnumber).remove();
        divnumber++;
    }
}

function cleanPhotoFileData(target) {
    $('#photoFileData' + target).html('');
}

function changeToDefaultPhoto(target) {
    var noImageSrc = $('#noImageSrc').val();
    if ($('#img' + target).attr('src') != noImageSrc) {
        $('#img' + target).attr("src", noImageSrc);
    }
}

function startLoader() {
    $('#ajaxLoader').show();
}

function stopLoader() {
    $('#ajaxLoader').hide();
}