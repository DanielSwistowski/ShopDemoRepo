var nickNameIsAvailable = false;
$(document).ready(function () {
    $(".fancybox").fancybox({ type: 'image' });

    getComments();

    $('#btnAddComment').click(function () {
        $('#commentForm').show();
    });

    $('#btnSendOpinion').click(function () {
        var formIsValid = validateForm();
        if (formIsValid == false)
            return false;
        addComment();
    });

    $('#btnAddToCart').click(function () {
        var productId = $('#ProductId').val();
        var productCount = $('#productCount').val();
        
        if (productCountIsValid(productCount)){
            var productQuantity = $('#productQuantity').data('quantity');
            if (productCount > productQuantity) {
                $('#productCount').addClass('border-error');
                alertError("Niepoprawna ilość produktu!")
            } else {
                showLoader('#ajaxLoader');
                $.when(getProductCountFromCart(productId)).done(function (result) {
                    if (result.success == true) {
                        var total = +result.productCount + +productCount
                        if (total > productQuantity) {
                            hideLoader('#ajaxLoader');
                            alertError('Błąd! W sklepie znajduje się tylko ' + productQuantity + 'szt. tego produktu. W koszyku posiadasz już ' + result.productCount + 'szt. tego produktu.');
                        } else {
                            $('#productCount').removeClass('border-error');
                            addToCart(productCount, productId);
                        }
                    } else {
                        hideLoader('#ajaxLoader');
                        alertError('Błąd! Proszę odświeżyć stronę i spróbowac ponownie.');
                    }
                });
            }
        }
        else {
            $('#productCount').addClass('border-error');
            alertError("Niepoprawna ilość produktu!")
        }
        
    });

    $('#productCount').blur(function () {
        var productCountValue = $(this).val();
        var isValid = productCountIsValid(productCountValue);
        if (!isValid) {
            $(this).addClass('border-error');
            alertError('Niepoprawna wartość');
        } else {
            $(this).removeClass('border-error');
        }
    });
});

function getAntiForgeryToken() {
    var token = $('#ajaxAntiForgeryTokenForm input[name=__RequestVerificationToken]').val();
    return token;
}

function validateForm() {
    var formIsValid = true;

    if ($('#ProductId').val().trim() == "") {
        formIsValid = false;
    }
    if ($('#rateValue').val() == 0) {
        $('#selectRateErrorMessage').show();
        formIsValid = false;
    } else {
        $('#selectRateErrorMessage').hide();
    }
    if ($('#comment').val().trim() == "") {
        $('#comment').css('border-color', 'Red');
        formIsValid = false;
    }
    else {
        $('#comment').css('border-color', 'lightgrey');
    }

    if ($('#nickName').val().trim() == "") {
        $('#nickName').css('border-color', 'Red');
        formIsValid = false;
    }
    else {
        if (nickNameIsAvailable == false) {
            $('#nickName').css('border-color', 'Red');
            formIsValid = false;
        }
        else {
            $('#nickName').css('border-color', 'lightgrey');
        }
    }
    return formIsValid;
}

function checkNickName() {
    var nickName = $('#nickName').val();
    var productId = $('#ProductId').val();
    var url = $('#nickName').data('url');
    $.ajax({
        url: url,
        type: "GET",
        data: { 'productId': productId, 'nickName': nickName },
        success: function (result) {
            if (result != true) {
                $('#nickUnavailableError').show();
                nickNameIsAvailable = false;
            } else {
                $('#nickUnavailableError').hide();
                nickNameIsAvailable = true;
            }
        },
        error: function () { nickNameIsAvailable = false; }
    });
}

function addComment() {
    var rate = {
        ProductId: $('#ProductId').val(),
        Comment: $('#comment').val(),
        NickName: $('#nickName').val(),
        Rate: $('#rateValue').val()
    };

    var url = $('#btnSendOpinion').data('url');

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: JSON.stringify(rate),
        success: function (result) {
            if (result == true) {
                $('#commentForm').hide();
                clearAddCommentForm();
                $('#message').css('color', 'green');
                $('#message').html('Dziękujemy za dodanie opini!');
                $('#btnAddComment').hide();
                getComments();
            }
        },
        error: function () {
            $('#message').css('color', 'red');
            $('#message').html('Błąd! Nie można dodać opini!');
        }
    });
}

function clearAddCommentForm() {
    $('#comment').val('');
    $('#nickName').val('');
    $('#rateValue').val(0);
}

function selectRate(s) {
    $('#rateValue').val(s);
    for (var i = 1; i <= s; i++) {
        $('#star' + i).css('color', 'orange');
    }
    for (var i = s + 1; i <= 5; i++) {
        $("#star" + i).css('color', '');
    }
}

function highlightStar(s) {
    for (var i = 1; i <= s; i++) {
        $('#star' + i).css('color', 'orange');
    }
}

function UnHighlightStar(s) {
    for (var i = 1; i <= s; i++) {
        $('#star' + i).css('color', '');
    }
}

function highlightSelectedRate() {
    var rateValue = $('#rateValue').val();
    if (rateValue != 0) {
        for (var i = 1; i <= rateValue; i++) {
            $("#star" + i).css('color', 'orange');
        }
    }
}

function getComments() {
    var productId = $('#ProductId').val();
    var url = $('#comments').data('url');
    $.ajax({
        type: 'GET',
        url: url,
        data: { 'productId': productId, },
        success: function (result) {
            $('#comments').html(result);
        },
        error: function () {
            alertError('Błąd! Nie można wczytać opinii o produkcie. Proszę odświeżyć stronę.');
        }
    });
}