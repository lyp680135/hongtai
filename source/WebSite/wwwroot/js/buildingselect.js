$(document).ready(function (e) {
    BuildingSelect.init();
});

var BuildingSelect = {};

BuildingSelect.buildingInfoList = [];

BuildingSelect.init = function () { 
        $("#area").change(function (e, val) {
            BuildingSelect.reloadbuild();
        });

        //根据城市/区域初始化楼盘列表
        $("#build").select2({
            data: {},
            placeholder: "选择楼盘",
            width: "150px",
          //  minimumResultsForSearch: -1,
            language: "zh-CN",
        });

        $("#stage").select2({
            data: {},
            placeholder: "期数",
            width: "80px",
            //minimumResultsForSearch: -1,
            language: "zh-CN",
        });

        $("#stage").parent().hide();

        $("#number").select2({
            data: {},
            placeholder: "楼栋",
            width: "80px",
           // minimumResultsForSearch: -1,
            language: "zh-CN",
        })

        $("#unit").select2({
            data: {},
            placeholder: "单元",
            width: "80px",
           // minimumResultsForSearch: -1,
            language: "zh-CN",
        })

        $("#layer").select2({
            data: {},
            placeholder: "楼层",
            width: "80px",
           // minimumResultsForSearch: -1,
            language: "zh-CN",
        })

        $("#room").select2({
            data: {},
            placeholder: "房号",
            width: "80px",
          //  minimumResultsForSearch: -1,
            language: "zh-CN",
        })

        $("#build").on("change", function (e) {

            var curbuild = "";
            for (var key in BuildingSelect.buildingInfoList) {
                var info = BuildingSelect.buildingInfoList[key];

                if (info.id == $(this).val()) {
                    curbuild = info;
                    break;
                }
            }

            $("#stage").empty();//先清空期号
            $("#number").empty();//先清空楼栋号
            $("#unit").empty();//先清空单元号
            $("#layer").empty();//先清空楼层号

            if (curbuild != undefined) {

                var list = new Array();

                var d = {
                    id: "",
                    text: "",
                };

                list.push(d);

                if (curbuild.stage != undefined && curbuild.stage != "") {
                    var stagearr = curbuild.stage.split(',');

                    if (stagearr != undefined) {
                        for (var i = 0; i < stagearr.length; i++) {
                            var d = {
                                id: stagearr[i],
                                text: stagearr[i],
                            }

                            list.push(d);
                        }
                    }
                }

                //if (list.length <= 1) {
                //    //默认显示第0期-第7期
                //    for (var i = 0; i < 8; i++) {
                //        var tempState = "";
                //        if (i == 0) {
                //            tempState = "无期数";
                //        } else {
                //            tempState = "第" + i + "期";
                //        }
                //        var d = {
                //            id: i,
                //            text: tempState,
                //        }

                //        list.push(d);
                //    }
                //}

                $("#stage").select2({
                    data: list,
                    placeholder: "期数",
                    width: "80px",
                    minimumResultsForSearch: -1,
                    language: "zh-CN",
                })
                if (list.length > 1) {
                    $("#stage").parent().show();
                } else {
                    $("#stage").parent().hide();
                }

                //初始化栋数列表
                var list = new Array();

                var d = {
                    id: "",
                    text: "",
                };

                list.push(d);

                var bnum = parseInt(curbuild.number);
                if (bnum > 0) {
                    for (var i = 1; i <= bnum; i++) {
                        var d = {
                            id: i,
                            text: i.toString() + "号楼",
                        }

                        list.push(d);
                    }
                }

                if (list.length <= 1) {
                    for (var i = 0; i < 1001; i++) {

                        var tempNumber = "";
                        if (i == 0) {
                            tempNumber = "无栋数";
                        } else {
                            tempNumber =  i + "号楼";
                        }

                        var d = {
                            id: i,
                            text: tempNumber,
                        }

                        list.push(d);
                    }
                }

                $("#number").select2({
                    data: list,
                    placeholder: "楼栋",
                    width: "80px",
                    minimumResultsForSearch: 1,
                    language: "zh-CN",
                })

                if (list.length > 0) {
                    $("#number").parent().show();
                } else {
                    $("#number").parent().hide();
                }

                //初始化单元列表
                var list = new Array();

                var d = {
                    id: "",
                    text: "",
                };

                list.push(d);

                var unum = parseInt(curbuild.unit);
                if (unum > 0) {
                    for (var i = 1; i <= unum; i++) {
                        var d = {
                            id: i,
                            text: i.toString() + "单元",
                        }

                        list.push(d);
                    }
                }

                if (list.length <= 1) {
                    //默认显示第0单元-第10单元
                    for (var i = 0; i < 11; i++) {

                        var tempDanYuan = "";
                        if (i == 0) {
                            tempDanYuan = "无单元";
                        } else {
                            tempDanYuan = i + "单元";
                        }

                        var d = {
                            id: i,
                            text: tempDanYuan,
                        }

                        list.push(d);
                    }
                }

                $("#unit").select2({
                    data: list,
                    placeholder: "单元",
                    width: "80px",
                  //  minimumResultsForSearch: -1,
                    language: "zh-CN",
                })

                if (list.length > 0) {
                    $("#unit").parent().show();
                } else {
                    $("#unit").parent().hide();
                }

                $("#layer").on("change", function (e) {
                    
                    $("#room").empty();//先清空房号
                    //初始化房间号列表
                    var list = new Array();

                    var d = {
                        id: "",
                        text: "",
                    };

                    list.push(d);

                    var rnum = parseInt(curbuild.room);
                    if (rnum > 0 && $(this).val() != undefined && $(this).val() != "") {
                        for (var i = 1; i <= rnum; i++) {
                            var text = "";
                            if (i > 10) {
                                text = $(this).val().toString() + i.toString() + "室";
                            } else {
                                text = $(this).val().toString() + "0" + i.toString() + "室";
                            }

                            var d = {
                                id: i,
                                text: text,
                            }

                            list.push(d);
                        }
                      
                    }else{

                        //默认显示0室-20室
                        for (var i = 1; i < 22; i++) {
                            var text = "";
                            if (i > 10) {
                                text = $(this).val().toString() + i.toString() + "室";
                            } else {
                                if (i == 0) {
                                    text ="无房号";
                                } else {
                                    text = $(this).val().toString() + "0" + i.toString() + "室";
                                }
                            }

                            var d = {
                                id: i,
                                text: text,
                            }

                            list.push(d);
                        }
                    }


                    $("#room").select2({
                        data: list,
                        placeholder: "房号",
                        width: "80px",
                        //minimumResultsForSearch: -1,
                        language: "zh-CN",
                    })

                    if (list.length > 0) {
                        $("#room").parent().show();
                    } else {
                        $("#room").parent().hide();
                    }

                });

                //初始化楼层列表
                var list = new Array();
                var d = {
                    id: "",
                    text: "",
                };

                list.push(d);

                var lnum = parseInt(curbuild.layer);
                if (lnum > 0) {
                    for (var i = 1; i <= lnum; i++) {
                        var d = {
                            id: i,
                            text: i.toString() + "楼",
                        }

                        list.push(d);
                    }

                }

                if (list.length <= 1) {
                    //默认显示0楼-40楼
                    for (var i = 1; i < 101; i++) {
                        
                        var tempLayer = "";
                        if (i == 0) {
                            tempLayer = "无楼层";
                        } else {
                            tempLayer = i + "楼";
                        }


                        var d = {
                            id: i,
                            text: tempLayer,
                        }

                        list.push(d);
                    }
                }

                $("#layer").select2({
                    data: list,
                    placeholder: "楼层",
                    width: "80px",
                   // minimumResultsForSearch: 1,
                    language: "zh-CN",
                })

                if (list.length > 0) {
                    $("#layer").parent().show();
                    $("#layer").trigger("change");
                } else {
                    $("#layer").parent().hide();
                    $("#room").parent().hide();
                }


                if (curbuild.customfiled != undefined && curbuild.customfiled != "") {
                    $("#number").parent().hide();
                    $("#unit").parent().hide();
                    $("#layer").parent().hide();
                    $("#room").parent().hide();

                    $(".customselect").remove();

                    var json_customfiled = JSON.parse(curbuild.customfiled);

                    for (var customi = 0; customi < json_customfiled.length; customi++) {

                        if (json_customfiled[customi].value != undefined && json_customfiled[customi].value != "") {

                            var customHtml = '<div class="cityselect customselect"><select id="customFiled' + customi + '" name="customFiled" class="validate[required] select2"></select></div>';
                            $("#stage").parent().parent().append(customHtml);

                            var customlist = new Array();
                            var customd = {
                                id: "",
                                text: "",
                            };
                            customlist.push(customd);

                            var values = json_customfiled[customi].value.split(',');
                            for (var valuesi = 0 ; valuesi < values.length; valuesi++) {
                                customlist.push({ id: values[valuesi], text: values[valuesi] });
                            }

                            $("#customFiled" + customi).select2({
                                data: customlist,
                                placeholder: json_customfiled[customi].name,
                                width: "100px",
                                language: "zh-CN",
                            });
                        } else {
                            var customHtml = '<div class="cityselect customselect"><input placeholder="' + json_customfiled[customi].name  + '" id="customFiled' + customi + '" name="customFiled" class="validate[required] scinput" style="width:85px;" /></div>';
                            $("#stage").parent().parent().append(customHtml);
                        }
                    }
                } else {
                    $(".customselect").remove();
                }
            }
        });
    };

BuildingSelect.reloadbuild = function (value) {
    $.ajax({
        type: 'POST',
        url: "/Address/BuildingInfoAjax",
        data: {
            areaid: $("#hareaid").val()
        },
        dataType: 'json',
        success: function (data) {
            if (data != undefined) {
                BuildingSelect.buildingInfoList = data;

                $("#build").empty();//先清空

                var blist = new Array();
                var d = {
                    id: "",
                    text: "",
                };

                blist.push(d);

                for (var key in BuildingSelect.buildingInfoList) {
                    var info = BuildingSelect.buildingInfoList[key];
                    var d = {
                        id: info.id,
                        text: info.name,
                        locked: info.locked,
                    }
                    blist.push(d);
                };

                $("#build").select2({
                    data: blist,
                    placeholder: "选择楼盘",
                    width: "150px",
                   // minimumResultsForSearch: -1,
                    language: "zh-CN",
                });

                $("#build").select2("val", value);

                if ($("#build").val() == "") {
                    $("#stage").empty();
                    //$("#stage").parent().hide();
                    $("#number").empty();
                    //$("#number").parent().hide();
                    $("#unit").empty();
                    //$("#unit").parent().hide();
                    $("#layer").empty();
                    //$("#layer").parent().hide();
                    $("#room").empty();
                    //$("#room").parent().hide();
                }
            }
        }
    })
};
