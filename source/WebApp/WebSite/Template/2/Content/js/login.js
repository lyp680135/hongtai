
layui.use('layer', function () {
    var $ = layui.jquery, layer = layui.layer;
});

$(document).ready(function () {

    //钢厂人员登录
    $("#btnLogin_ByUserName").click(function () {

        var username = $("#UserName").val();
        var pwd = $("#Pwd").val();

        if (username == undefined || username == "")
        {
            layer.msg("请输入手机号或者用户名");
            $("#UserName").focus();
            return false;
        }

        if (pwd == undefined || pwd == "") {
            layer.msg("请输入密码");
            $("#Pwd").focus();
            return false;
        }

        var index = layer.load(2, {
            shade: [0.1, '#fff']
        });

        $.ajax({
            type: "POST",
            url: "/Login/LoginByUserName",
            data: "UserName=" + username  + "&Pwd=" + pwd,
            success: function (re) {
                if (re.status) {
                    var reData = JSON.parse(re.data);
                    window.location.href = reData.redirect + "SetAuth?s=" + reData.access_token;
                } else {
                    layer.msg(re.msg, { icon: 2 });
                }
                layer.close(index);
            }, error: function () {
                layer.msg("服务器出现错误", { icon: 2 });
                layer.close(index);
            }
        });

    });

    ///发送短信验证码
    $("#sendcode").click(function () {
        var moblie = $("#UserName");
        var systemMemberType = $("#SystemMemberType");//系统会员类别
        if (isPoneAvailable(moblie.val())) {

            var index = layer.load(2, {
                shade: [0.1, '#fff']
            });
            $.ajax({
                type: "POST",
                url: "/Login/SendMobileCode",
                data: "mobile=" + moblie.val() + "&systemMemberType=" + systemMemberType.val(),
                success: function (re) {
                    if (re.status) {
                        settime($("#sendcode")); //禁用发送按钮
                        layer.msg("短信验证码发送成功");
                    } else {
                        layer.alert(re.msg, { icon: 2 });
                    }
                    layer.close(index);
                }, error: function () {
                    layer.msg("服务器出现错误", { icon: 2 });
                    layer.close(index);
                }
            });

        } else {
            layer.msg("请输入正确的手机号", { icon: 2 });
            moblie.focus();
        }
    });

    //经销商登录 、 钢厂人员手机验证码登录
    $("#btnLogin_ByPhone").click(function () {
        var moblie = $("#UserName");
        var code = $("#Code");
        var systemMemberType = $("#SystemMemberType");

        if (isPoneAvailable(moblie.val())) {
            if (code.val() != '') {
                var index = layer.load(2, {
                    shade: [0.1, '#fff']
                });
                $.ajax({
                    type: "POST",
                    url: "/Login/LoginByPhone",
                    data: "mobile=" + moblie.val() + "&code=" + code.val() + "&systemMemberType=" + systemMemberType.val(),
                    success: function (re) {
                        if (re.status) {
                            var reData = JSON.parse(re.data);
                             
                            if (systemMemberType.val() == 0) {

                                var redUrl = '/Login/SetPwd';
                                window.location.href = redUrl + "?s=" + reData.access_token;
                            } else {
                                window.location.href = reData.redirect + "SetAuth?s=" + reData.access_token;
                            }
                        } else {
                            layer.msg(re.msg, { icon: 2 });
                        }
                        layer.close(index);
                    }, error: function () {
                        layer.msg("服务器出现错误", { icon: 2 });
                        layer.close(index);
                    }
                });
            } else {
                layer.msg("请输入短信验证码", { icon: 2 });
                code.focus();
            }
        } else {
            layer.msg("请输入正确的手机号", { icon: 2 });
            moblie.focus();
        } 
    });

    //设置密码
    $("#btn_SetPwd").click(function () {
        var SetPwd = $("#SetPwd");
        var SetPwd2 = $("#SetPwd2");

        if (SetPwd.val().length < 6)
        {
            layer.msg("密码长度应不少于6位");
            SetPwd.focus();
            return false;
        }
        if (SetPwd.val() != SetPwd2.val())
        {
            layer.msg("两次输入的密码不一致");
            return false;
        }

        $("#form1").submit();

    });
});
 

//手机号验证
function isPoneAvailable($poneInput) {
    var myreg = /^[1][3,4,5,7,8,9][0-9]{9}$/;
    if (!myreg.test($poneInput)) {
        return false;
    } else {
        return true;
    }
}  

var countdown = 60; 
function settime(obj) { //发送验证码倒计时
    if (countdown == 0) {
        obj.attr('disabled', false);
        obj.html("发送验证码");
        obj.css("background", "");
        countdown = 60;
        return;
    } else {
        obj.attr('disabled', true);
        obj.css("background", "#9d9d9d");
        obj.html("重新发送(" + countdown + ")");
        countdown--;
    }
    setTimeout(function () {
        settime(obj)
    }, 1000)
}