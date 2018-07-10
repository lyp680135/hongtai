/* xyHelper为辅助方法类  , 方法名为小写开头
xyHelper.QueryString(key)  为获取当前页面参数传值，如： a.html?id=1 ，获取方法为 xyHelper.QueryString('id')
xyHelper.isNumber
xyHelper.isNullOrEmpty
xyHelper.isPhone
*/
 /* eslint-disable */
var xyHelper = new Object()

xyHelper.queryString = function (key) {
  let LocString = window.document.location.href
  var rs = new RegExp('(^|)' + key + '=([^&]*)(&|$)', 'gi').exec(LocString),
    tmp
  if (tmp = rs) {
    return tmp[2]
  }
  return ''
}
xyHelper.isNumber = function (obj) {
  return typeof obj === 'number' && isFinite(obj)
}
xyHelper.isAmt = function (amt) {
  return /^(\d*)(.[0-9]{1,2})?$/.test(amt)
}
xyHelper.isNullOrEmpty = function (str) {
  if (str == null || str === undefined || str.length === 0) {
    return true
  }
  return false
}
xyHelper.isPhone = function (phone) {
  var myreg = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1})|(17[0-9]{1}))+\d{8})$/
  return myreg.test(phone)
}
xyHelper.isEmail = function (email) {
  var myreg = /^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/
  return myreg.test(email)
}
xyHelper.isBankCard = function (cardno) {
  return /^(\d{16}|\d{19}|\d{23})$/.test(xyHelper.removeSpace(cardno))
}
/*
银行卡号格式化
格式化前：6212261202022550975
格式化后：6212 2612 0202 2550 975
*/
xyHelper.fmtBankCard = function (s) {
  if (!s) { return '' }
  return xyHelper.removeSpace(s).replace(/(\d{4})(?=\d)/g, '$1 ')
}
/**
 *去除所有空格
 *去空格前：6212 2612 0202 2550 975
 *去空格后：6212261202022550975
 */
xyHelper.removeSpace = function (s) {
  if (!s) { return '' }
  return s.replace(/\s/g, '')
}

xyHelper.tryParseJson = function(obj){
  var json = obj;
  if (typeof obj == 'string') {
      try{
          json = JSON.parse(obj);
      }catch(e){
          console.warn("tryParseJson faild:"+e.Message);
          return null;
      }
  }else{
      var isObject = (typeof obj == "object");
      var isObjects = Object.prototype.toString.call(obj).toLowerCase() == "[object object]";
      var isArray = Object.prototype.toString.call(obj).toLowerCase() == "[object array]";

      //判断本身是否是JSON对象
      if ((isObject  && isObjects && !obj.length) || (isObject && isArray)){
      }else{
          console.warn("tryParseJson faild invalid obj:"+obj);
          return null;
      }
  }
  return json
}


Array.prototype.contains = function () {
	var arg0 = arguments[0];
	var arg1 = arguments[1];
  	if(arg1){
  		for (var i in this) {
    		if (this[i][arg0] == arg1) return true;
  	  }		
  	}else{
	  	for (var i in this) {
	    	if (this[i] == arg0) return true;
	  	}
  	}
  	return false;
}

String.prototype.changeTwoDecimal = function (digits,defaultvalue){
  var f_x = parseFloat(this);
  if (isNaN(f_x))
  {
      console.error('function:changeTwoDecimal->parameter error:'+this);
      if(defaultvalue == undefined){
          return '--';
      }else{
          return defaultvalue;
      }
  }

  if(digits == undefined){
      f_x = Math.round(f_x * 100)/100;
      f_x = f_x.toFixed(2);
  }else{
      try{
          var num=parseInt(digits);
  
          f_x = Math.round(f_x * Math.pow(10,num))/Math.pow(10,num);
          f_x = f_x.toFixed(num);
      }catch(e){
          console.error('invalid digits args:'+digits);
          f_x = f_x.toFixed(2);
      }
  }

  return f_x;
}


export default xyHelper
