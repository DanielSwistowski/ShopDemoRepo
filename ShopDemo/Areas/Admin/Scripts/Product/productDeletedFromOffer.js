$(document).ready(function () {
    getCategories("ProductsDeletedFromOffer", "Admin",'');
    getPreviousSelectedCategories("ProductsDeletedFromOffer", "Admin");

    $('.btnDeleteProduct').click(function (e) {
        e.preventDefault();
        var targetLink = $(this).attr('href');
        var itemName = $(this).data('item');
        $('#confirmModalMessage').html('Czy chcesz usunąć wybrany produkt?');
        $('#confirmModalItemName').html('Nazwa produktu: ' + itemName);
        $('#modalBtnConfirm').data('targetUrl', targetLink);
        $('#modalBtnConfirm').data('callbackFunction', 'deleteProduct');
        $('#confirmModal').modal('show');
    });

    $('#modalBtnConfirm').click(function () {
        var url = $('#modalBtnConfirm').data('targetUrl');
        var callbackFunction = $('#modalBtnConfirm').data('callbackFunction');
        functions[callbackFunction](url);
    });

    $('.btnAddProductToOffer').click(function (e) {
        e.preventDefault();
        var targetLink = $(this).attr('href');
        var itemName = $(this).data('item');
        $('#confirmModalMessage').html('Czy chcesz przywrócić do sprzedaży wybrany produkt?');
        $('#confirmModalItemName').html('Nazwa produktu: ' + itemName);
        $('#modalBtnConfirm').data('targetUrl', targetLink);
        $('#modalBtnConfirm').data('callbackFunction', 'addProductToOffer');
        $('#confirmModal').modal('show');
    });
});

function getAntiForgeryToken() {
    var token = $('#ajaxAntiForgeryTokenForm input[name=__RequestVerificationToken]').val();
    return token;
}

var functions = {
    deleteProduct: function (url) {
        $('#confirmModalLoader').show();
        $.ajax({
            type: "POST",
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            url: url,
            success: function (result) {
                $('#confirmModalLoader').hide();
                $('#confirmModal').modal('hide');
                if (result == 0) {
                    alertError('Nie możesz usunąć wybranego produktu, ponieważ istnieje on na liście zamówień');
                }
                else {
                    alertSuccess('Produkt został usunięty');
                    var rowId = '#' + result;
                    $(rowId).fadeOut();
                }
            },
            error: function () {
                $('#confirmModalLoader').hide();
                $('#confirmModal').modal('hide');
                alertError('Błąd! Proszę spróbować ponownie');
            }
        });
    },
    addProductToOffer: function (url) {
        $('#confirmModalLoader').show();
        $.ajax({
            type: "POST",
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            url: url,
            success: function (result) {
                $('#confirmModalLoader').hide();
                $('#confirmModal').modal('hide');
                alertSuccess('Produkt został przywrócony do sprzedaży');
                var rowId = '#' + result;
                $(rowId).fadeOut();
            },
            error: function () {
                $('#confirmModalLoader').hide();
                $('#confirmModal').modal('hide');
                alertError('Błąd! Proszę spróbować ponownie');
            }
        });
    }
}