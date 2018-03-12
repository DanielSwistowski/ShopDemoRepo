function alertSuccess(message) {
    $(window).scrollTop(0);
    $('#alert').addClass('alert alert-success');
    $('#alert').html(message).show();
    $('#alert').delay(3000).fadeOut(2000);
}

function alertError(message) {
    setTimeout(function () {
        $(window).scrollTop(0);
        $('#alert').addClass('alert alert-danger');
        $('#alert').html(message).show();
        $('#alert').delay(3000).fadeOut(2000);
    }, 1000);
}

function showLoader(targetSelector) {
    var loaderSrc = $('#ajaxLoaderSrc').val();
    $(targetSelector).html('<img src="' + loaderSrc + '" style="margin-top:5px" alt="Wczytywanie..." />');
}

function hideLoader(targetSelector) {
    $(targetSelector).html('');
}