$(document).ready(function () {
    $(".fancybox").fancybox({ type: 'image' });
    getComments();
});

function getComments() {
    var productId = $('#ProductId').val();
    var url = $('#comments').data('url');
    $.ajax({
        type: 'GET',
        url: url,
        data: { 'productId': productId, },
        success: function (result) {
            $('#comments').html(result);

            $('.btnDeleteComment').click(function () {
                var commentId = $(this).data('commentid');
                var url = $(this).data('url');
                var comment = $('#comment_' + commentId).html();
                $('#confirmModalMessage').html('Czy chcesz usunąć wybrany komentarz?');
                $('#confirmModalItemName').html('Treść komentarza: ' + comment);
                $('#modalBtnConfirm').data('targetUrl', url);
                $('#modalBtnConfirm').data('id', commentId);
                $('#confirmModal').modal('show');
            });
        },
        error: function () {
            alertError('Błąd! Nie można wczytać opinii o produkcie. Proszę odświeżyć stronę.');
        }
    });
}

$('#modalBtnConfirm').click(function () {
    var url = $('#modalBtnConfirm').data('targetUrl');
    var commentId = $('#modalBtnConfirm').data('id');
    $('#confirmModalLoader').show();

    $.ajax({
        type: 'POST',
        url: url,
        headers: { '__RequestVerificationToken': getAntiForgeryToken() },
        data: { 'productRateId': commentId, },
        success: function (result) {
            $('#confirmModalLoader').hide();
            $('#confirmModal').modal('hide');
            if (result.success == true) {
                alertSuccess(result.message);
                getComments();
            } else {
                alertError(result.message);
            }
        },
        error: function () {
            $('#confirmModalLoader').hide();
            $('#confirmModal').modal('hide');
            alertError('Błąd! Nie można usunąć komentarza. Proszę spróbować ponownie.');
        }
    });
});

function getAntiForgeryToken() {
    var token = $('#ajaxAntiForgeryTokenForm input[name=__RequestVerificationToken]').val();
    return token;
}