function getCategories(targetActionName, areaName, filtr) {
    var categoryId = $('#selectedCategoryId').val();
    var url = $('#categories').data('url');
    var req = $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId, 'targetActionName': targetActionName, 'areaName': areaName, 'filtr':filtr },
        success: function (result) {
            $('#categories').html(result);
        },
        error: function () {
            hideLoader('#categories');
            alertError('Nie można pobrać kategorii. Proszę odświeżyć stronę.');
        }
    });
}

function getPreviousSelectedCategories(targetActionName, areaName) {
    var categoryId = $('#selectedCategoryId').val();
    var url = $('#previousCategories').data('url');
    $.ajax({
        url: url,
        type: 'GET',
        data: { 'categoryId': categoryId, 'targetActionName': targetActionName, 'areaName': areaName },
        success: function (result) {
            $('#previousCategories').html(result);
        },
        error: function () {
            alertError('Nie można pobrać wcześniej wybranych kategorii. Proszę odświeżyć stronę.');
        }
    });
}