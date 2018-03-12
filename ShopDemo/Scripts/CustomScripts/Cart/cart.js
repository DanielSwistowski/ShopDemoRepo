$(document).ready(function () {
    $('#btnEmptyCart').click(function () {
        var url = $(this).data('url');
        $.ajax({
            type: 'POST',
            url: url,
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                if (result.success == true) {
                    window.location.reload();
                }
            }
        });
    });

    $('.btnRemoveProductFromCart').click(function () {
        var url = $(this).data('url');
        var urlReferrer = $('#urlReferrer').attr('href');
        var productId = $(this).data('productid');
        $.ajax({
            type: 'POST',
            url: url,
            headers: { '__RequestVerificationToken': getAntiForgeryToken() },
            data: { 'productId': productId, 'urlReferrer': urlReferrer },
            success: function (result) {
                if (result.success == true) {
                    window.location.reload();
                }
            }
        });
    });

    $('.productCount').blur(function () {
        var productCountValue = $(this).val();
        var isValid = productCountIsValid(productCountValue);
        if (!isValid) {
            $(this).addClass('border-error');
            alertError('Niepoprawna wartość');
        } else {
            $(this).removeClass('border-error');
            var productId = $(this).data('productid');
            $.when(getProductCount(productId)).done(function (result) {
                if (result.success == true) {
                    if (+productCountValue > result.productCount) {
                        alertError('Błąd! W sklepie znajduje się tylko ' + result.productCount + 'szt. tego produktu.');
                        setTimeout(function () { getCart(); }, 3000);
                    } else {
                        updateProductCount(productCountValue, productId);
                    }
                } else {
                    alertError('Błąd! Proszę odświeżyć stronę.');
                }
            });
        }
    });
});

function getProductCount(productId) {
    var url = $('#getProductCountUrl').val();
    return $.ajax({
        type: 'GET',
        url: url,
        data: { 'productId': productId }
    });
}

function updateProductCount(productCountValue, productId) {
    var url = $('#updateCartUrl').val();
    var urlReferrer = $('#urlReferrer').attr('href');
    $.ajax({
        type: 'POST',
        url: url,
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: { 'productCount': productCountValue, 'productId': productId, 'urlReferrer': urlReferrer },
        success: function (result) {
            if (result.success == true) {
                window.location.reload();
            }
        }
    });
}

function getAntiForgeryToken() {
    var token = $('#ajaxAntiForgeryTokenForm input[name=__RequestVerificationToken]').val();
    return token;
}