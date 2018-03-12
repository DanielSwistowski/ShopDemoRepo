$(document).ready(function () {
    getPreviousSelectedCategories("Index", "");
    getSearchFilters("Index", "");

    $('.addToCart').click(function () {
        var productCount = $('#productCount').val();
        var productId = $(this).data('productid');
        var availableProductQuantity = $(this).data('productcount');
        showLoader('#ajaxLoader_'+productId);

        $.when(getProductCountFromCart(productId)).done(function (result) {
            if (result.success == true) {
                var total = +result.productCount + +productCount;
                if (total > availableProductQuantity) {
                    hideLoader('#ajaxLoader_'+productId);
                    alertError('Błąd! W sklepie znajduje się tylko ' + availableProductQuantity + 'szt. tego produktu. W koszyku posiadasz już ' + result.productCount +'szt. tego produktu.');
                } else {
                    addToCart(productCount, productId);
                }
            } else {
                hideLoader('#ajaxLoader_'+productId);
                alertError('Błąd! Proszę odświeżyć stronę i spróbowac ponownie.');
            }
        });
    });
});

function getAntiForgeryToken() {
    var token = $('#ajaxAntiForgeryTokenForm input[name=__RequestVerificationToken]').val();
    return token;
}