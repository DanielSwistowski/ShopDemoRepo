var columnNumber = 1;
$(document).ready(function () {
    var baseCategory = 'Kategorie';
    var categoryId = null;
    var target = '#column_' + columnNumber;
    getCategoriesJson(categoryId, baseCategory, target);
});

function getNumber(target) {
    var number = target.split('_').pop();
    return number;
}

function getAntiForgeryToken() {
    var token = $('#ajaxAntiForgeryTokenForm input[name=__RequestVerificationToken]').val();
    return token;
}

var parentCategoryId = null;
function getCategoriesJson(categoryId, baseCategory, target) {
    var html = '';
    var column = getNumber(target);
    var url = $('#categories').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId },
        success: function (result) {
            html += '<table class="table">';
            html += '<th>' + baseCategory + '</th><th></th><th></th>';
            html += '<input type="hidden" id="currentColumn_' + column + '" value ="' + column + '"/>';
            if (result.length > 0) {
                $.each(result, function (key, category) {
                    html += '<tr>';
                    html += '<td><a class="categoryLabel' + column + '" href="#" data-categoryid="' + category.CategoryId + '" data-basecategory="' + category.Name + '">' + category.Name + '</a></td>';
                    html += '<td style="text-align:right">';
                    html += '<button type="button" class="btn btn-warning btn-sm btnSearchFilters' + column + '" data-categoryid="' + category.CategoryId + '" data-toggle="tooltip" title="Filtry wyszukiwania"><span class="glyphicon glyphicon-filter"></span></button>';
                    html += '<button style="margin-left:5px" type="button" class="btn btn-warning btn-sm btnCategorySettings' + column + '" data-categoryid="' + category.CategoryId + '" data-toggle="tooltip" title="Ustawienia"><span class="glyphicon glyphicon-info-sign"></span></button>';
                    html += '<button style="margin-left:5px" type="button" class="btn btn-warning btn-sm btnEdit' + column + '" data-categoryid="' + category.CategoryId + '" data-basecategory="' + category.Name + '" data-target="column_' + column + '" data-toggle="tooltip" title="Edytuj"><span class="glyphicon glyphicon-edit"></span></button>';
                    html += '<button style="margin-left:5px" type="button" class="btn btn-danger btn-sm btnDelete' + column + '" data-categoryid="' + category.CategoryId + '" data-basecategory="' + category.Name + '" data-target="column_' + column + '" data-toggle="tooltip" title="Usuń kategorię"><span class="glyphicon glyphicon-trash"></span></button>';
                    html += '</td>';
                    html += '</tr>';
                });
            } else {
                html += '<tr>';
                html += '<td>Brak kategorii</td>';
                html += '</tr>';
            }
            html += '<tr><td colspan="2"><span id="categoryMessage' + column + '" style="color:red"></span></td></tr>';

            html += '<tr>';
            html += '<td  style="border-top:none" class="form-inline" colspan="2"><input id="txtNewCategory" type="text" placeholder="Nowa kategoria" class="form-control" />';
            html += '<button id="btnSave' + column + '" type="button" data-target="column_' + column + '" class="btn btn-success" style="margin-left:5px">Dodaj</button>';
            html += '<button id="btnSaveChanges' + column + '" data-target="column_' + column + '" type="button" class="btn btn-success hidden" style="margin-left:5px">Zapisz</button>';
            html += '<button id="btnDeleteConfirm' + column + '" data-target="column_' + column + '" type="button" class="btn btn-success hidden" style="margin-left:5px">Usuń</button>';
            html += '<button id="btnCancel' + column + '" data-target="column_' + column + '" type="button" class="btn btn-danger hidden" style="margin-left:5px">Anuluj</button></td>';
            html += '</tr>';
            html += '</table>';

            clearColumns(column);
            $('#column_' + column).html(html);

            $('.categoryLabel' + column).on('click', function (e) {
                e.preventDefault();
                var categoryId = $(this).data('categoryid');
                var baseCategory = $(this).data('basecategory');
                var currentColumn = parseInt($('#currentColumn_' + column).val(), 10);
                var newColumn = currentColumn + 1;
                createColumns(newColumn);
                var target = '#column_' + newColumn;
                getCategoriesJson(categoryId, baseCategory, target);
            });

            $('#btnSave' + column).on('click', function () {
                var target = $(this).data('target');
                var newCategory = $('#' + target + ' ' + '#txtNewCategory').val().replace('.',',');
                if ($.trim(newCategory) == '') {
                    $('#' + target + ' ' + '#txtNewCategory').css('border', '1px solid red');
                    $('#categoryMessage' + column).text('Uzupełnij dane');
                } else if(!validateInputText(newCategory)){
                    $('#' + target + ' ' + '#txtNewCategory').css('border', '1px solid red');
                    $('#categoryMessage' + column).text('Dozwolone znaki to cyfry i litery');
                }else {
                    $('#' + target + ' ' + '#txtNewCategory').css('border', '');
                    $('#categoryMessage' + column).text('');
                    addCategory(target, newCategory, categoryId, baseCategory);
                }
            });

            var updatedCategoryId;
            $('.btnEdit' + column).on('click', function () {
                $('#btnCancel' + column).click();
                $('#btnSave' + column).addClass('hidden');
                $('#btnSaveChanges' + column).removeClass('hidden');
                $('#btnCancel' + column).removeClass('hidden');
                updatedCategoryId = $(this).data('categoryid');
                var baseCategory = $(this).data('basecategory');
                var target = $(this).data('target');
                $('#' + target + ' ' + '#txtNewCategory').val(baseCategory);
            });

            $('#btnSaveChanges' + column).each(function () {
                $(this).on('click', function () {
                    var target = $(this).data('target');
                    var changedCategory = $('#' + target + ' ' + '#txtNewCategory').val().replace('.', ',');
                    if ($.trim(changedCategory) == '') {
                        $('#categoryMessage' + column).text('Uzupełnij dane');
                        $('#' + target + ' ' + '#txtNewCategory').css('border', '1px solid red');
                    } else {
                        updateCategory(changedCategory, baseCategory, updatedCategoryId, categoryId, target);
                        $('#' + target + ' ' + '#txtNewCategory').css('border', '');
                        $('#btnSave' + column).removeClass('hidden');
                        $('#btnSaveChanges' + column).addClass('hidden');
                        $('#btnCancel' + column).addClass('hidden');
                        $('#' + target + ' ' + '#txtNewCategory').val('');
                        $('#categoryMessage' + column).text('');
                    }
                });
            });

            $('#btnCancel' + column).each(function () {
                $(this).on('click', function () {
                    $('#btnSave' + column).removeClass('hidden');
                    $('#btnSaveChanges' + column).addClass('hidden');
                    $('#btnCancel' + column).addClass('hidden');
                    $('#btnDeleteConfirm' + column).addClass('hidden');
                    $('#categoryMessage' + column).text('');
                    var target = $(this).data('target');
                    $('#' + target + ' ' + '#txtNewCategory').val('');
                    $('#' + target + ' ' + '#txtNewCategory').css('border', '');
                });
            });

            $('.btnCategorySettings' + column).on('click', function () {
                var catId = $(this).data('categoryid');
                getProductAttributes(catId, categoryId);
                getParentAttributes(catId, categoryId);
                $('#productAttributesModal').modal('show');
            });

            $('.btnSearchFilters' + column).on('click', function () {
                var catId = $(this).data('categoryid');
                getSearchFilters(catId);
                $('#searchFiltersModal').modal('show');
            });

            var deletedCategoryId;
            $('.btnDelete' + column).click(function () {
                $('#btnCancel' + column).click();
                deletedCategoryId = $(this).data('categoryid');
                var baseCategory = $(this).data('basecategory');
                var target = $(this).data('target');
                $('#' + target + ' ' + '#txtNewCategory').val(baseCategory);
                $('#categoryMessage' + column).text('Usunąć wybraną kategorię?');
                $('#btnSave' + column).addClass('hidden');
                $('#btnDeleteConfirm' + column).removeClass('hidden');
                $('#btnCancel' + column).removeClass('hidden');
            });

            $('#btnDeleteConfirm' + column).on('click', function () {
                var target = $(this).data('target');
                deleteCategory(deletedCategoryId, categoryId, baseCategory, target);
            });
        },
        error: function () {
            alertError('Nie można pobrać kategorii! Proszę odświeżyć stronę.');
        }
    });
}

var columnsCount = 1;
var rowNumber = 1;
function createColumns(newColumnNumber) {
    if ($('#column_' + newColumnNumber).length == 0) {
        if (columnsCount % 3 === 0) {
            rowNumber++;
            var rowHtml = '<hr /><div class="row" id="row_' + rowNumber + '"</div>';
            $('#categories').append(rowHtml);
        }
        var html = '<div id="column_' + newColumnNumber + '" class="col-md-4"></div>';
        $('#categories #row_' + rowNumber).append(html);
        columnsCount++;
    }
}

function clearColumns(currentColumn) {
    for (var i = currentColumn; i <= columnsCount; i++) {
        $('#column_' + i).html('');
    }
}

function addCategory(target, newCategory, parentId, baseCategory) {
    var category = {
        ParentCategoryId: parentId,
        Name: newCategory
    };

    var url = $('#addCategoryUrl').val();

    $.ajax({
        url: url,
        type: 'POST',
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: JSON.stringify(category),
        success: function (result) {
            if (result.success == true) {
                getCategoriesJson(parentId, baseCategory, target);
            } else {
                alertError(result.message);
            }
        },
        error: function () {
            alertError('Nie można dodać nowej kategorii! Proszę odświeżyć stronę.');
        }
    });
}

function deleteCategory(deletedCategoryId, parentCategoryId, baseCategory, target) {
    var url = $('#deleteCategoryUrl').val();
    $.ajax({
        url: url,
        type: 'POST',
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: { 'categoryId': deletedCategoryId },
        success: function (result) {
            if (result.success == true)
                getCategoriesJson(parentCategoryId, baseCategory, target)
            else
                alertError(result.message);
        },
        error: function () {
            alertError('Nie można usunąć kategorii! Proszę odświeżyć stronę i spróbować ponownie.');
        }
    });
}

function updateCategory(changedCategory, baseCategory, updatedCategoryId, categoryId, target) {
    var category = {
        CategoryId: updatedCategoryId,
        Name: changedCategory
    };

    var url = $('#editCategoryUrl').val();

    $.ajax({
        url: url,
        type: 'POST',
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: JSON.stringify(category),
        success: function (result) {
            if (result.success == true) {
                getCategoriesJson(categoryId, baseCategory, target);
            } else {
                alertError(result.message);
            }
        },
        error: function () {
            alertError('Nie można zapisać zmian! Proszę odświeżyć stronę.');
        }
    });
}

function getProductAttributes(categoryId, parentCategoryId) {
    var url = $('.modal-body #attributes').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId },
        success: function (result) {
            $('.modal-body #attributes').html(result);
            $('#productAttributeModalMessage').html('');
            $('#btnAddProductAttribute').on('click', function () {
                var attribute = [];
                var att = $('#productAttribute').val().replace('.', ',');
                var isValid = validateInputText(att);
                if (isValid) {
                    $('#productAttribute').removeClass('border-error');
                    $('#productAttributeModalMessage').html('');
                    attribute.push(att);
                    addProductAttributes(attribute, categoryId, parentCategoryId);
                } else {
                    $('#productAttribute').addClass('border-error');
                    $('#productAttributeModalMessage').html('Pole jest puste lub zawiera niedozwolone znaki (dozwolone cyfry, litery oraz przecinek i nawiasy kwadratowe)!');
                }
            });
            var attributeId = 0;
            $('.btnRemoveAttribute').click(function () {
                attributeId = $(this).data('attributeid');
                var attribute = $(this).data('attribute');
                $('#productAttribute').addClass('border-error');
                $('#productAttributeModalMessage').html('Usunąć wybraną cechę produktu?');
                $('#productAttribute').val(attribute);
                $('#btnAttributeSaveChanges').addClass('hidden');
                $('#btnAddProductAttribute').addClass('hidden');
                $('#btnConfirmDeleteAttribute').removeClass('hidden');
                $('#btnCancelEditAttribute').removeClass('hidden');
            });
            $('#btnConfirmDeleteAttribute').click(function () {
                removeProductAttribute(attributeId, categoryId, parentCategoryId);
            });

            $('.btnEditAttribute').click(function () {
                attributeId = $(this).data('attributeid');
                var attribute = $(this).data('attribute');
                $('#productAttribute').val(attribute);
                $('#btnAddProductAttribute').addClass('hidden');
                $('#btnConfirmDeleteAttribute').addClass('hidden');
                $('#btnAttributeSaveChanges').removeClass('hidden');
                $('#btnCancelEditAttribute').removeClass('hidden');
            });
            $('#btnAttributeSaveChanges').click(function () {
                var attribute = $('#productAttribute').val().replace('.', ',');
                var isValid = validateInputText(attribute);
                if (isValid) {
                    $('#productAttribute').removeClass('border-error');
                    $('#productAttributeModalMessage').html('');
                    updateProductAttribute(attributeId, attribute, categoryId, parentCategoryId);
                } else {
                    $('#productAttribute').addClass('border-error');
                    $('#productAttributeModalMessage').html('Pole jest puste lub zawiera niedozwolone znaki (dozwolone cyfry, litery oraz przecinek i nawiasy kwadratowe)!');
                }
            });
            $('#btnCancelEditAttribute').click(function () {
                $('#productAttribute').val('');
                $('#productAttribute').removeClass('border-error');
                $('#productAttributeModalMessage').html('');
                $('#btnAddProductAttribute').removeClass('hidden');
                $('#btnAttributeSaveChanges').addClass('hidden');
                $('#btnCancelEditAttribute').addClass('hidden');
                $('#btnConfirmDeleteAttribute').addClass('hidden');
            });
            $('.btnEditAttributeValues').click(function () {
                var attributeId = $(this).data('attributeid');
                var attributeName = $(this).data('attribute-name');
                $('#attributeValuesModal').modal('show');
                getProductAttributeValues(attributeId);
                getParentProductAttributeValues(parentCategoryId, attributeName, attributeId)
            });
        },
        error: function () {
            $('.modal-body #attributes').html('<p style="color:red;">Nie można pobrać cech produktów! Proszę odświeżyć stronę.</p>');
        }
    });
}

function getParentAttributes(categoryId, parentCategoryId) {
    var url = $('.modal-body #parentAttributes').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId, 'parentCategoryId': parentCategoryId },
        success: function (result) {
            $('.modal-body #parentAttributes').html(result);
            $('#productAttributesModal #btnSaveCheckboxChanges').on('click', function () {
                var $boxes = $('input[name=selectedAttributes]:checked');
                if ($boxes.length > 0) {
                    var selectedValues = [];
                    $boxes.each(function () {
                        var txt = $(this).val();
                        var isDisabled = $(this).is(':disabled');
                        if (txt != 0 && isDisabled == false) {
                            selectedValues.push(txt);
                            $(this).attr('disabled', true);
                            $(this).val(0);
                        }
                    });
                    if (selectedValues.length > 0) {
                        addProductAttributes(selectedValues, categoryId, parentCategoryId);
                    }
                } else {
                    $('#productAttributeModalMessage').html('Wybierz cechy produktu!');
                }
            });
        },
        error: function () {
            $('.modal-body #parentAttributes').html('<p style="color:red;">Nie można pobrać cech produktów z nadrzędnej kategorii! Proszę odświeżyć stronę.</p>');
        }
    });
}

function addProductAttributes(attributes, attributeCategoryId, parentCategoryId) {
    var attributes = {
        CategoryId: attributeCategoryId,
        Attributes: attributes
    };
    var url = $('#btnAddProductAttribute').data('url');
    $.ajax({
        url: url,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: JSON.stringify(attributes),
        type: 'POST',
        success: function (result) {
            if (result.success == true) {
                $('#productAttributeModalMessage').html('');
                getProductAttributes(attributeCategoryId, parentCategoryId);
            } else {
                $('#productAttributeModalMessage').html(result.message);
            }
        },
        error: function () {
            $('#productAttributeModalMessage').html('Błąd przy dodawaniu nowych cech produktu!');
        }
    });
}

function removeProductAttribute(attributeId, attributeCategoryId, parentCategoryId) {
    var url = $('#removeProductAttributeUrl').val();
    $.ajax({
        url: url,
        data: { 'attributeId': attributeId },
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        type: 'POST',
        success: function (result) {
            if (result.success == true) {
                $('#productAttributeModalMessage').html('');
                getProductAttributes(attributeCategoryId, parentCategoryId);
            }
            else {
                $('#productAttributeModalMessage').html(result.message);
            }
        },
        error: function () {
            $('#productAttributeModalMessage').html('Nie można usunąć wybranej cechy produktu!');
        }
    });
}

function updateProductAttribute(attributeId, attribute, attributeCategoryId, parentCategoryId) {
    if ($.trim(attribute) == '') {
        $('#productAttribute').css('border', '1px solid red');
    } else {
        $('#productAttribute').css('border', '');
        var attribute = {
            ProductAttributeId: attributeId,
            Name: attribute
        };
        var url = $('#btnAttributeSaveChanges').data('url');
        $.ajax({
            url: url,
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            data: JSON.stringify(attribute),
            type: 'POST',
            success: function (result) {
                if (result.success == true) {
                    $('#productAttributeModalMessage').html('');
                    getProductAttributes(attributeCategoryId, parentCategoryId);
                }
                else {
                    $('#productAttributeModalMessage').html(result.message);
                }
            },
            error: function () {
                $('#productAttributeModalMessage').html('Nie można edytować cechy produktu!');
            }
        });
    }
}

function getProductAttributeValues(attributeId) {
    var url = $('.modal-body #availableValues').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'attributeId': attributeId },
        success: function (result) {
            $('#productAttributeValueModalMessage').html('');
            $('.modal-body #availableValues').html(result);
            $('#btnAddProductAttributeValue').click(function () {
                var attributeValues = [];
                var attributeValue = $('#productAttributeValue').val().replace('.', ',');
                var isValid = validateInputText(attributeValue);
                if (isValid) {
                    attributeValues.push(attributeValue);
                    $('#productAttributeValue').removeClass('border-error');
                    $('#productAttributeValueModalMessage').html('');
                    addProductAttributeValues(attributeId, attributeValues);
                } else {
                    $('#productAttributeValue').addClass('border-error');
                    $('#productAttributeValueModalMessage').html('Pole jest puste lub zawiera niedozwolone znaki (dozwolone cyfry, litery oraz przecinek i nawiasy kwadratowe)!');
                }
            });
            var attributeValueId;
            $('.btnRemoveAttributeValue').click(function () {
                attributeValueId = $(this).data('attributevalueid');
                var attributeValue = $(this).data('attributevalue');
                $('#productAttributeValue').val(attributeValue);
                $('#productAttributeValue').addClass('border-error');
                $('#productAttributeValueModalMessage').html('Usunąć wybraną wartość?');
                $('#btnAddProductAttributeValue').addClass('hidden');
                $('#btnConfirmDeleteAttributeValue').removeClass('hidden');
                $('#btnCancelDeleteAttributeValue').removeClass('hidden');
            });
            $('#btnConfirmDeleteAttributeValue').click(function () {
                removeProductAttributeValue(attributeValueId, attributeId);
            });
            $('#btnCancelDeleteAttributeValue').click(function () {
                $('#productAttributeValue').val('');
                $('#productAttributeValue').removeClass('border-error');
                $('#productAttributeValueModalMessage').html('');
                $('#btnAddProductAttributeValue').removeClass('hidden');
                $('#btnConfirmDeleteAttributeValue').addClass('hidden');
                $('#btnCancelDeleteAttributeValue').addClass('hidden');
            });
        },
        error: function () {
            $('.modal-body #availableValues').html('<p style="color:red;">Nie można pobrać ustawionych wartości wybranej cechy produktu! Proszę odświeżyć stronę.</p>');
        }
    });
}

function getParentProductAttributeValues(categoryId, attributeName, attributeId) {
    var url = $('.modal-body #parentAttributeValues').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId, 'attributeName': attributeName, 'attributeId': attributeId },
        success: function (result) {
            $('#productAttributeValueModalMessage').html('');
            $('.modal-body #parentAttributeValues').html(result);
            $('#attributeValuesModal #btnSaveCheckboxChanges').on('click', function () {
                var $boxes = $('input[name=selectedValues]:checked');
                if ($boxes.length > 0) {
                    var selectedValues = [];
                    $boxes.each(function () {
                        var txt = $(this).val();
                        var isDisabled = $(this).is(':disabled');
                        if (txt != 0 && isDisabled == false) {
                            selectedValues.push(txt);
                            $(this).attr('disabled', true);
                            $(this).val(0);
                        }
                    });
                    if (selectedValues.length > 0) {
                        addProductAttributeValues(attributeId, selectedValues);
                    }
                } else {
                    $('#attributeValuesModal').html('Wybierz wartości cechy produktu!');
                }
            });
        },
        error: function () {
            $('.modal-body #parentAttributeValues').html('<p style="color:red;">Nie można pobrać ustawionych wartości wybranej cechy produktu! Proszę odświeżyć stronę.</p>');
        }
    });
}

function addProductAttributeValues(attributeId, values) {
    var attributeValues = {
        ProductAttributeId: attributeId,
        AttributeValues: values
    };
    var url = $('#btnAddProductAttributeValue').data('url');
    $.ajax({
        url: url,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: JSON.stringify(attributeValues),
        type: 'POST',
        success: function (result) {
            if (result.success == true) {
                getProductAttributeValues(attributeId);
            } else {
                $('#productAttributeValueModalMessage').html(result.message);
            }
        },
        error: function () {
            $('#productAttributeValueModalMessage').html('Błąd przy dodawaniu nowych wartości cechy produktu! Proszę spróbować ponowanie.');
        }
    });
}

function removeProductAttributeValue(attributeValueId, productAttributeId) {
    var url = $('#removeProductAttributeValueUrl').val();
    $.ajax({
        url: url,
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: { 'attributeValueId': attributeValueId },
        type: 'POST',
        success: function (result) {
            if (result.success == true) {
                getProductAttributeValues(productAttributeId);
            }
            else {
                $('#productAttributeValueModalMessage').html(result.message);
            }
        },
        error: function () {
            $('#productAttributeValueModalMessage').html('Błąd przy usuwaniu wartości cechy produktu! Proszę spróbować ponowanie.');
        }
    });
}

var categoryIdForFilterDelete;
function getSearchFilters(categoryId) {
    $('#searchFiltersModalMessage').html('');
    var url = $('.modal-body #availableFilters').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId },
        success: function (result) {
            $('.modal-body #availableFilters').html(result);

            $('#btnAddFilter').click(function () {
                var productAttriuteId = $('#AddSearchFilterModel_ProductAttributeId').val();
                var filterTypeId = $('#AddSearchFilterModel_FilterType').val();
                addSearchFilter(productAttriuteId, filterTypeId, categoryId);
            });

            $('.btnRemoveFilter').click(function () {
                categoryIdForFilterDelete = categoryId;
                var searchFilterId = $(this).data('filterid');
                var attribute = $(this).data('filterattribute');
                var url = $('#removeSearchFilterUrl').val();
                $('#modalBtnConfirm').data('targetUrl', url);
                $('#confirmModalMessage').html('Czy chcesz usunąć filtr wyszukiwania dla wybranej cechy?');
                $('#confirmModalItemName').html('Cecha produktu: ' + attribute);
                $('#modalBtnConfirm').data('id', searchFilterId);
                $('#confirmModal').modal('show');
            });
        },
        error: function () {
            $('.modal-body #availableFilters').html('<p style="color:red;">Nie można pobrać ustawionych filtrów wyszukiwania! Proszę odświeżyć stronę.</p>');
        }
    });
}

$('#modalBtnConfirm').click(function () {
    var searchFilterId = $('#modalBtnConfirm').data('id');
    var url = $('#modalBtnConfirm').data('targetUrl');
    $('#confirmModalLoader').show();
    $.ajax({
        url: url,
        type: 'POST',
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: { 'searchFilterId': searchFilterId },
        success: function (result) {
            $('#confirmModalLoader').hide();
            $('#confirmModal').modal('hide');
            if (result.success = true) {
                getSearchFilters(categoryIdForFilterDelete);
            } else {
                alertError.html(result.message);
            }
        },
        error: function () {
            $('#confirmModalLoader').hide();
            $('#confirmModal').modal('hide');
            alertError('Błąd! Nie można usunąć filtra wyszukiwania. Proszę spróbować ponownie.');
        }
    });
});

function addSearchFilter(productAttriuteId, filterTypeId, categoryId) {
    var filter = {
        CategoryId: categoryId,
        ProductAttributeId: productAttriuteId,
        FilterType: filterTypeId
    };
    var url = $('#btnAddFilter').data('url');
    $.ajax({
        url: url,
        type: 'POST',
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: JSON.stringify(filter),
        success: function (result) {
            if (result.success == true) {
                getSearchFilters(categoryId);
            } else {
                $('#searchFiltersModalMessage').html(result.message);
            }
        },
        error: function () {
            $('#searchFiltersModalMessage').html('Nie można zapisać nowego filtra wyszukiwania! Proszę odświeżyć stronę.');
        }
    });
}

function validateInputText(value) {
    var isValid = true;
    if (value.trim() == '') {
        isValid = false;
    } else {
        var pattern = /[^A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ\d\s\[\]\,\+]/;
        if (value.match(pattern)) {
            isValid = false;
        }
    }
    return isValid;
}