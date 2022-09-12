// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
    if ($("#simple-form")[0]) {
        initFormValue($("#simple-form"));
        $("#simple-form").on('submit', function (e) {
            e.preventDefault();
            var form = $(this);
            var url = form.attr('action');
            console.log(this);
            $.ajax({
                type: "GET",
                url: `${url}?` + form.serialize(),
                success: function (data) {
                    $("#result").text(JSON.stringify(data, null, 4));
                }
            });
        });
    }
});

function initFormValue($ele) {
    const now = new Date();
    const start = new Date();
    start.setMonth(now.getMonth() - 1);
    $($ele).find('input[name="start"]').val(start.toISOString().slice(0, 10));
    $($ele).find('input[name="end"]').val(now.toISOString().slice(0, 10));
    $($ele).find('input[name="region"]').val('Hong Kong');
}