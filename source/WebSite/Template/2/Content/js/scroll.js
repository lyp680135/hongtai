//flash��ʽ���÷���
//<EMBED src='flash.swf'/*tpa=http://www.hnmytg.com/Public/Home/js/flash.swf*/ quality=high  WIDTH=100 HEIGHT=300 TYPE='application/x-shockwave-flash' id=ad wmode=opaque></EMBED>

lastScrollY = 0;

function heartBeat(){
var diffY;
if (document.documentElement && document.documentElement.scrollTop)
diffY = document.documentElement.scrollTop;
else if (document.body)
diffY = document.body.scrollTop
else
{/*Netscape stuff*/}
//alert(diffY);
percent=.1*(diffY-lastScrollY);
if(percent>0)percent=Math.ceil(percent);
else percent=Math.floor(percent);
document.getElementById("rightDiv").style.top = parseInt(document.getElementById("rightDiv").style.top)+percent+"px";
lastScrollY=lastScrollY+percent;
//alert(lastScrollY);
}
//�������ɾ��󣬶�������������Ļ���ƶ���
window.setInterval("heartBeat()",1);
//-->

document.writeln("<style type=\"text\/css\">");
document.writeln("body{height:1000px;}");
document.writeln("#rightDiv{position:absolute;}");
document.writeln(".itemFloat{height:auto;line-height:5px}");
document.writeln("<\/style>");
document.writeln("<div id=\"rightDiv\" style=\"top:250px;right:5px\">");
document.writeln("<ul id='scrollbox' class='itemFloat'>");
document.writeln("<li class='wx'><span></span></li><li class='qq'><span>"+qq+"</span></li><li class='tel'><span>"+mobile+"</span></li><li class='gotop'></li>");
document.writeln("<\/ul>");
document.writeln("<\/div>");