// JavaScript Document
var flag = true;
$('#ca-container').contentcarousel();
function ShowAllContent(flag) {
    var content = $("#content");
    if (flag == "unfold") {
        content.removeClass("introducep");
        $("#unfold").attr("style", "display:none");
        $("#fold").attr("style", "display:inline");
    } else {
        content.addClass("introducep");
        $("#unfold").attr("style", "display:inline");
        $("#fold").attr("style", "display:none");
    }
}
function ShowAllContentj(flag) {
    var content = $("#contentj");
    if (flag == "unfold") {
        content.removeClass("introducep");
        $("#unfoldj").attr("style", "display:none");
        $("#foldj").attr("style", "display:inline");
    } else {
        content.addClass("introducep");
        $("#unfoldj").attr("style", "display:inline");
        $("#foldj").attr("style", "display:none");
    }
}
$(document).ready(function(){


	$('#myCarousel').carousel({
		interval: 2000
	});
	$('#myCarouse2').carousel({
		interval: 2000
	});
	$('#myCarouse3').carousel({
		interval: 2000
	});
});