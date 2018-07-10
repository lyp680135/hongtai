(function ($, ele, id) {
    $.fn.selall = function (name) {
        $(this).click(function () {
            var val = $(this).prop('checked');
            var inputname = 'input[name=' + name + ']';
            if (val) {
                $(inputname).each(function (index, element) {
                    $(element).attr("checked", true)
                });
            } else {
                $(inputname).each(function (index, element) {
                    $(element).attr("checked", false)
                });
            }
        });
    };

    //默认生效
    $(function(){
        $(ele).selall(id);
    });
})(jQuery, "#chkall", "id")

function checkValid(obj) {
    var obj = $(obj);
    var buttonid = obj.context.id;
    var formId = document.getElementById(buttonid).form.id;
    var valid = $("#" + formId).validationEngine("validate");
    if (valid) {
        layer.load(1);
    }
    return valid;
}