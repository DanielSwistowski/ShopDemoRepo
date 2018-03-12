$(document).ready(function () {
    getCategories("ProductsOnPromotion","Admin",'');
    getPreviousSelectedCategories("ProductsOnPromotion", "Admin");

    $('.btnDeleteDiscount').click(function (e) {
        e.preventDefault();
        var itemName = $(this).data('item');
        var targetLink = $(this).attr('href');
        $('#confirmModalMessage').html('Czy chcesz usunąć wybraną promocję?');
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
                if (result.success == true)
                    window.location.href = result.redirectUrl;
            },
            error: function () {
                $('#confirmModalLoader').hide();
                $('#confirmModal').modal('hide');
                alertError('Błąd! Proszę spróbować ponownie');
            }
        });
    });
});

function getAntiForgeryToken() {
    var token = $('#ajaxAntiForgeryTokenForm input[name=__RequestVerificationToken]').val();
    return token;
}