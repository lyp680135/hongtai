
$(function () {
    $('#nav-collapse')
        .on('show.bs.collapse', function () {
            $('.triangle_border_top').show();
        })
        .on('hidden.bs.collapse', function () {
            $('.triangle_border_top').hide();
        });
});
