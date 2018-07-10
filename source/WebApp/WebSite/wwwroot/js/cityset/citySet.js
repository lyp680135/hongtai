(function ($, undefined) {
    $.fn.xycity = function (data) {
        var ths = $(this);
        $(ths).attr("readonly", "readonly");

        var jsondata = {
            province: "",
            provinceid: "",
            city: "",
            cityid: "",
            area: "",
            areaid: "",
        };

        if(data!=undefined && data !=''){
            jsondata = data;
        }

        var hprovince = $('<input>', {
            type: 'hidden', name: "hprovince", id: "hprovince", val: ''
        });
        var hprovinceid = $('<input>', {
            type: 'hidden', name: "hprovinceid", id: "hprovinceid", val: ''
        });
        var hcity = $('<input>', {
            type: 'hidden', name: "hcity", id: "hcity", val: ''
        });
        var hcityid = $('<input>', {
            type: 'hidden',name: "hcityid",id: "hcityid",val: ''
        });
        var harea = $('<input>', {
            type: 'hidden',name: "harea",id: "harea",val: ''
        });
        var hareaid = $('<input>', {
            type: 'hidden', name: "hareaid", id: "hareaid", val: ''
        });
        $(ths).after(hprovince);
        $(ths).after(hprovinceid);
        $(ths).after(hcity);
        $(ths).after(hcityid);
        $(ths).after(harea);
        $(ths).after(hareaid);

        if (jsondata.province != undefined && jsondata.provinceid != undefined && jsondata.province != "") {
            $("#hprovince").val(jsondata.province);
            $("#hprovinceid").val(jsondata.provinceid);
            $(ths).val(jsondata.province);

            if (jsondata.city != undefined && jsondata.cityid != undefined && jsondata.city != "") {
                $("#hcity").val(jsondata.city);
                $("#hcityid").val(jsondata.cityid);
                $(ths).val(jsondata.province + '-'+ jsondata.city);

                if (jsondata.area != undefined && jsondata.areaid != undefined && jsondata.area != "") {
                    $("#harea").val(jsondata.area);
                    $("#hareaid").val(jsondata.areaid);

                    $(ths).val(jsondata.province + '-' + jsondata.city + '-' + jsondata.area);
                }
            }
        }

        $(this).click(function (e) {
            var dal = '<div class="_citys"><span title="清空" id="cColse" >清空</span><ul id="_citysheng" class="_citys0"><li class="citySel">省份</li><li>城市</li><li>区县</li></ul><div id="_citys0" class="_citys1"></div><div style="display:none" id="_citys1" class="_citys1"></div><div style="display:none" id="_citys2" class="_citys1"></div></div>';
            Iput.show({ id: $(ths)[0], event: e, content: dal, width: "470" });
            $("#cColse").click(function () {
                $("#hprovince,#hcity,#harea").remove();
                $("#hprovinceid,#hcityid,#hareaid").remove();
                $(ths).val('');

                jsondata.province = "";
                jsondata.provinceid = "";
                jsondata.city = "";
                jsondata.cityid = "";
                jsondata.area = "";
                jsondata.areaid = "";
                $(ths).trigger("change", jsondata);

                Iput.colse();
            });
            var tb_province = [];
            var b = province;
            for (var i = 0, len = b.length; i < len; i++) {
                tb_province.push('<a data-level="0" data-id="' + b[i]['id'] + '" data-name="' + b[i]['name'] + '">' + b[i]['name'] + '</a>');
            }
            $("#_citys0").append(tb_province.join(""));
            $("#_citys0 a").click(function () {
                var g = getCity($(this));
                $("#_citys1 a").remove();
                $("#_citys1").append(g);
                $("._citys1").hide();
                $("._citys1:eq(1)").show();
                $("#_citys0 a,#_citys1 a,#_citys2 a").removeClass("AreaS");
                $(this).addClass("AreaS");
                var lev = $(this).data("name");
                var levid = $(this).data("id");
                $(ths).val($(this).data("name"));

                jsondata.province = lev;
                jsondata.provinceid = levid;
                $(ths).trigger("change", jsondata);

                if (document.getElementById("hprovince") == null) {
                    var hcitys = $('<input>', {
                        type: 'hidden',
                        name: "hprovince",
                        "data-id": levid,
                        id: "hprovince",
                        val: lev
                    });
                    $(ths).after(hcitys);
                }
                else {
                    $("#hprovince").val(lev);
                    $("#hprovince").attr("data-id", levid);
                    $("#hcity,#harea").val('');
                    $("#hcity,#harea").attr('data-id', '');
                }

                if (document.getElementById("hprovinceid") == null) {
                    var hcitys = $('<input>', {
                        type: 'hidden',
                        name: "hprovinceid",
                        id: "hprovinceid",
                        val: levid
                    });
                    $(ths).after(hcitys);
                }
                else {
                    $("#hprovinceid").val(levid);
                    $("#hcityid,#hareaid").val('');
                }

                $("#_citys1 a").click(function () {
                    $("#_citys1 a,#_citys2 a").removeClass("AreaS");
                    $(this).addClass("AreaS");
                    var lev = $(this).data("name");
                    var levid = $(this).data("id");
                    if (document.getElementById("hcity") == null) {
                        var hcitys = $('<input>', {
                            type: 'hidden',
                            name: "hcity",
                            "data-id": $(this).data("id"),
                            id: "hcity",
                            val: lev
                        });
                        $(ths).after(hcitys);
                    }
                    else {
                        $("#hcity").attr("data-id", $(this).data("id"));
                        $("#hcity").val(lev);
                        $("#harea").val('');
                        $("#harea").attr('data-id', '');
                    }


                    if (document.getElementById("hcityid") == null) {
                        var hcitys = $('<input>', {
                            type: 'hidden',
                            name: "hcityid",
                            id: "hcityid",
                            val: levid
                        });
                        $(ths).after(hcitys);
                    }
                    else {
                        $("#hcityid").val(levid);
                        $("#hareaid").val('');
                    }

                    var bc = $("#hprovince").val();
                    $(ths).val(bc + "-" + $(this).data("name"));

                    jsondata.city = lev;
                    jsondata.cityid = levid;
                    $(ths).trigger("change", jsondata);

                    var ar = getArea($(this));

                    $("#_citys2 a").remove();
                    $("#_citys2").append(ar);
                    $("._citys1").hide();
                    $("._citys1:eq(2)").show();

                    $("#_citys2 a").click(function () {
                        $(ths).trigger("change", $(ths).val());

                        $("#_citys2 a").removeClass("AreaS");
                        $(this).addClass("AreaS");
                        var lev = $(this).data("name");
                        var levid = $(this).data("id");
                        if (document.getElementById("harea") == null) {
                            var hcitys = $('<input>', {
                                type: 'hidden',
                                name: "harea",
                                "data-id": $(this).data("id"),
                                id: "harea",
                                val: lev
                            });
                            $(ths).after(hcitys);
                        }
                        else {
                            $("#harea").val(lev);
                            $("#harea").attr("data-id", $(this).data("id"));
                        }

                        if (document.getElementById("hareaid") == null) {
                            var hcitys = $('<input>', {
                                type: 'hidden',
                                name: "hareaid",
                                id: "hareaid",
                                val: levid
                            });
                            $(ths).after(hcitys);
                        }
                        else {
                            $("#hareaid").val(levid);
                        }

                        var bc = $("#hprovince").val();
                        var bp = $("#hcity").val();
                        $(ths).val(bc + "-" + bp + "-" + $(this).data("name"));

                        jsondata.area = lev;
                        jsondata.areaid = levid;
                        $(ths).trigger("change", jsondata);

                        Iput.colse();
                    });

                });
            });
            $("#_citysheng li").click(function () {
                $("#_citysheng li").removeClass("citySel");
                $(this).addClass("citySel");
                var s = $("#_citysheng li").index(this);
                $("._citys1").hide();
                $("._citys1:eq(" + s + ")").show();
            });
        }
    );
    }
})(jQuery)

function getCity(obj) {
    var c = obj.data('id');
    var e = province;
    var f;
    var g = '';
    for (var i = 0, plen = e.length; i < plen; i++) {
        if (e[i]['id'] == parseInt(c)) {
            f = e[i]['city'];
            break
        }
    }
    for (var j = 0, clen = f.length; j < clen; j++) {
        g += '<a data-level="1" data-id="' + f[j]['id'] + '" data-name="' + f[j]['name'] + '" title="' + f[j]['name'] + '">' + f[j]['name'] + '</a>'
    }
    $("#_citysheng li").removeClass("citySel");
    $("#_citysheng li:eq(1)").addClass("citySel");
    return g;
}
function getArea(obj) {
    var c = obj.data('id');
    var e = area;
    var f = [];
    var g = '';
    for (var i = 0, plen = e.length; i < plen; i++) {
        if (e[i]['pid'] == parseInt(c)) {
            f.push(e[i]);
        }
    }
    for (var j = 0, clen = f.length; j < clen; j++) {
        g += '<a data-level="1" data-id="' + f[j]['id'] + '" data-name="' + f[j]['name'] + '" title="' + f[j]['name'] + '">' + f[j]['name'] + '</a>'
    }

    $("#_citysheng li").removeClass("citySel");
    $("#_citysheng li:eq(2)").addClass("citySel");
    return g;
}