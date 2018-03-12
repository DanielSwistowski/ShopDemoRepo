$(document).ready(function () {
    $('.btnDeleteProductFromOffer').click(function (e) {
        e.preventDefault();
        var targetLink = $(this).attr('href');
        var itemName = $(this).data('item');
        $('#confirmModalMessage').html('Czy chcesz wycofać ze sprzedaży wybrany produkt?');
        $('#confirmModalItemName').html('Produkt: ' + itemName);
        $('#modalBtnConfirm').data('targetUrl', targetLink);
        $('#confirmModal').modal('show');
    });

    $('#modalBtnConfirm').click(function () {
        var url = $('#modalBtnConfirm').data('targetUrl');
        $('#confirmModalLoader').show();
        $.ajax({
            type: "POST",
            url: url,
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                $('#confirmModalLoader').hide();
                $('#confirmModal').modal('hide');
                if (result.success == true) {
                    alertSuccess('Produkt został wycofany ze sprzedaży');
                    var rowId = '#' + result.productId;
                    $(rowId).fadeOut();
                } else {
                    alertError(result.message);
                }
            },
            error: function () {
                $('#confirmModalLoader').hide();
                $('#confirmModal').modal('hide');
                alertError('Błąd! Proszę spróbować ponownie');
            }
        });
    });

    getPreviousSelectedCategories("Index", "Admin");
    getSearchFilters("Index", "Admin");

    $('.btnActualizeProductQuantity').click(function () {
        $('#modalProductId').val($(this).data('productid'));
        $('#modalProductName').html($(this).data('productname'));
        $('#modalProductQuantity').val('');
        $('#modalErrorMessage').html('');
        $('#updateProductQuantityModal').modal('show');
    });

    $('#modalConfirmActualizeBtn').click(function () {
        var productId = $('#modalProductId').val();
        var quantity = $('#modalProductQuantity').val();

        if (quantity.trim() == "") {
            $('#modalErrorMessage').html('Wpisz ilość produktu');
        } else {
            if (quantity <= 0 || quantity > 100) {
                $('#modalErrorMessage').html('Niepoprawna ilość (zakres 1-100)');
            }
            else {
                $('#modalErrorMessage').html('');
                $('#updateProductQuantityLoader').show();
                actualizeProductQuantity(productId, quantity);
            }
        }
    });
});

function getAntiForgeryToken() {
    var token = $('#ajaxAntiForgeryTokenForm input[name=__RequestVerificationToken]').val();
    return token;
}

function actualizeProductQuantity(productId, quantity) {
    var url = $('#actualizeProductQuantityUrl').val();
    $.ajax({
        url: url,
        type: 'POST',
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: { 'productId': productId, 'quantity': quantity },
        success: function (result) {
            $('#updateProductQuantityLoader').hide();
            $('#updateProductQuantityModal').modal('hide');
            if (result.success == true) {
                $('#actualProductQuantity_' + productId).addClass('tag-updated');
                $('#actualProductQuantity_' + productId).html(result.updatedQuantity);
                alertSuccess('Ilość produktu została zaktualizowana');
            } else {
                alertError('Nie można zaktualizować ilości produktu. Proszę spróbować ponownie.');
            }
        },
        error: function () {
            alertError('Nie można zaktualizować ilości produktu. Proszę spróbować ponownie.');
        }
    });
}