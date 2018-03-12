function getCart() {
    var url = $('#cartLink').attr('href');
    window.location.href = url;
}

function addToCart(productCount, productId) {
    var url = $('#addToCartUrl').val();
    $.ajax({
        type: 'POST',
        url: url,
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: { 'productId': productId, 'productCount': productCount },
        success: function (result) {
            if (result.success == true) {
                getCart();
            }
        },
        error: function () {
            alertError('Nie można dodać do koszyka.');
        }
    });
}

function getProductCountFromCart(productId) {
    var url = $('#getProductCountFromCartUrl').val();
    return $.ajax({
        type: 'GET',
        url: url,
        data: { 'productId': productId }
    });
}

function productCountIsValid(productCountValue) {
    var isValid = false;
    var isNumber = isNumeric(productCountValue);
    if (isNumber) {
        if (isInt(productCountValue)) {
            if (productCountValue > 0 && productCountValue <= 100)
                isValid = true;
        }
    }
    return isValid;
}

function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

function isInt(n) {
    return n % 1 === 0;
}