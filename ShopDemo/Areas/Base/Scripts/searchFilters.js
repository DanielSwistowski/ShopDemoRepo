$(document).ready(function () {
    $('#btnSearch').click(function (e) {
        e.preventDefault;
        createSearchParameters();
    });

    $('#btnFilterProducts').click(function () {
        $('#btnSearch').click();
    });
});

function getSearchFilters(targetActionName, areaName) {
    showLoader('#categories');
    showLoader('#searchFilters');
    $.when(getSearchFiltersAjax()).done(function (result) {
        $('#searchFilters').html(result);
        var filtr = createFilterStringParameter();
        getCategories(targetActionName, areaName, filtr)
    }).fail(function () {
        hideLoader('#categories');
        hideLoader('#searchFilters');
        alertError('Nie można pobrać filtrów wyszukiwania oraz kategorii produktów. Proszę odświeżyć stronę.');
    });
}

function getSearchFiltersAjax() {
    var categoryId = $('#selectedCategoryId').val();
    var currentSelectedParams = $('#searchFilters').data('currentselectedparameters');
    var url = $('#searchFilters').data('url');
    return $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId, 'currentSelectedParams': currentSelectedParams }
    });
}

function createSearchParameters() {
    var html = '';

    var search = $('#search').val();
    if (search != '') {
        html += '<input name="szukaj" type="hidden" value="' + search + '" />'
    }

    var priceFrom = $('#priceFrom').val();
    if (priceFrom != '') {
        html += '<input name="cena_od" type="hidden" value="' + priceFrom.replace(',', '.') + '" />'
    }

    var priceTo = $('#priceTo').val();
    if (priceTo != '') {
        html += '<input name="cena_do" type="hidden" value="' + priceTo.replace(',', '.') + '" />'
    }

    $('#searchParams').html(html);

    var stringParameter = createFilterStringParameter();
    if (stringParameter.length > 0) {
        var html = '<input type="hidden" name="filtr" value="' + stringParameter + '"/>';
        $('#searchParams').append(html);
    }
}

function createFilterStringParameter() {
    var filters = getValuesFromSelectedCheckboxes();
    var filterString = '';
    for (var i = 0; i < filters.length; i++) {
        var property = '';
        if (i == 0) {
            property = filters[i].attribute + '-';
        } else {
            property = '.' + filters[i].attribute + '-';
        }

        var value = '';
        for (var j = 0; j < filters[i].values.length; j++) {
            if (j == filters[i].values.length - 1) {
                value += filters[i].values[j];
            } else {
                value += filters[i].values[j] + '_';
            }
        }
        filterString += property + value;
    }
    return filterString.toLowerCase();
}

function getValuesFromSelectedCheckboxes() {
    var filters = [];
    $('#searchFilters .attributeSearchFilter').each(function () {
        var attribute = $(this).data('attributename');
        var $boxes = $(this).find('input[name=selectedFilters]:checked');
        if ($boxes.length > 0) {
            var values = [];
            $boxes.each(function () {
                var txt = $(this).val();
                if (txt != 0) {
                    values.push(txt);
                }
            });
            var filter = {
                attribute: attribute,
                values: values
            };
            filters.push(filter);
        }
    });
    return filters;
}