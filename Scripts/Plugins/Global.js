//js获取网站根路径(站点及虚拟目录)，获得网站的根目录或虚拟目录的根地址
var g_WebSiteRootURL = "";
var g_Ajax_LoadingDiv_ID = "AjaxLoadingDiv";
$(document).ready(function () {
    /*加載頁面遮罩層*/
    $(window).scroll(function () {
        SetAjaxLoadingDiv(g_Ajax_LoadingDiv_ID);
    });
    $('body').prepend("<div id='" + g_Ajax_LoadingDiv_ID + "' class='loadingdiv'></div>");

    $("#AjaxLoadingDiv").ajaxStart(function () {
        ShowAjaxLoadingDiv(g_Ajax_LoadingDiv_ID, '', true);
    });
    $("#AjaxLoadingDiv").ajaxComplete(function (event, request, settings) {
        ShowAjaxLoadingDiv(g_Ajax_LoadingDiv_ID, '', false);
    });

    /*隱藏/顯示*/
    $("img[id*='Show_Hide_Img_']").bind("click", null, Show_Hide);
    /*控制表格風格$('.ctrl_list').mytable();*/
    //全選/取消
    $("#chkSelectAll").click(function () {
        if (($(this).attr("checked") == undefined) || ($(this).attr("checked") == false)) {
            $("input[name='chkSelectItem']").attr("checked", false);
        }
        else
        { $("input[name='chkSelectItem']").attr("checked", true); }
    });
    //分頁
    //上一頁
    $("#btnPrePager").click(function () {
        var pager = $('#hidePager').val();
        Action('query', parseInt(pager) - 1);
    });
    //下一頁
    $("#btnNextPager").click(function () {
        var pager = $('#hidePager').val();
        Action('query', parseInt(pager) + 1);
    });
    //跳轉
    $("#txtPager").keydown(function (event) {
        if (event.keyCode == 13) {
            var pager = $('#txtPager').val();
            Action('query', pager);
        }
    });
    /* 打印申請單 */
    $("#btnPrint").click(function () {
        var tmp = $("#iframe_Container").html();
        $("#iframe_Container").html($("#iframe_MainRight").html());
        $(':button').hide();
        window.print();
        $("#iframe_Container").html(tmp);
        ShowButton($("#IsShowButton").val());
    });
});

/*加載頁面遮罩層*/
function ShowAjaxLoadingDiv(divID, content, show) {
    var divLoading = $("#" + divID);
    if (show) {
        divLoading.text(content);
        SetAjaxLoadingDiv();
        divLoading.fadeTo("slow", 0.66);
    }
    else {
        divLoading.fadeTo("slow", 0);
        divLoading.hide();
    }
}
function SetAjaxLoadingDiv(divID) {
    var divLoading = $("#" + divID);
    var top1 = $(window).scrollTop();
    var left1 = $(window).scrollLeft();
    var width1 = $(window).width();
    var height1 = $(window).height();
    divLoading.width(left1 + width1);
    divLoading.height(top1 + height1);
}
/*
*功能：點擊圖標，隱藏 / 顯示
*參數：img id=Show_Hide_Img_
*           div id=Show_Hide_Div_
*/
function Show_Hide() {
    var imgid = $(this).attr("id");
    var divid = imgid.replace('Show_Hide_Img_', '#Show_Hide_Div_');
    var contextPath = "/Content/themes/default/images/";
    var state = "";
    state = ($(divid).css("display") == "block") ? "none" : "";
    if (state == "") {
        $(imgid).attr("src", contextPath + "show.gif");
    }
    else {
        $(imgid).attr("src", contextPath + "hide.gif");
    }
    $(divid).css("display", state);
    $.cookie(divid, state);
}

function ShowDialogByColorBox(url, initEvent, closeEvent) {
    $.colorbox({
        href: g_WebSiteRootURL + url
    , iframe: true
    , width: "500px"
    , height: "350px"
    , onLoad: function () {
        if (initEvent != undefined && initEvent != null) { initEvent(); }
    }
    , onCleanup: function () {
        if (closeEvent != undefined && closeEvent != null) { closeEvent(); }
    }
    });
}

function ShowDialogByColorBox1(url, initEvent, closeEvent) {
    $.colorbox({
        href: g_WebSiteRootURL + url
    , iframe: true
    , width: "900px"
    , height: "350px"
    , onLoad: function () {
        if (initEvent != undefined && initEvent != null) { initEvent(); }
    }
    , onCleanup: function () {
        if (closeEvent != undefined && closeEvent != null) { closeEvent(); }
    }
    });
}

function ShowDialogByColorBox_auto(url, initEvent, closeEvent,width,height) {
    $.colorbox({
        href: g_WebSiteRootURL + url
    , iframe: true
    , width: width
    , height: height
    , onLoad: function () {
        if (initEvent != undefined && initEvent != null) { initEvent(); }
    }
    , onCleanup: function () {
        if (closeEvent != undefined && closeEvent != null) { closeEvent(); }
    }
    });
}

function AjaxExec(url, param, dataType, successEvent) {
    $.ajax({
        type: "POST"
        , contentType: "application/x-www-form-urlencoded; charset=utf-8"
        , url: g_WebSiteRootURL + url
        , data: param
        , dataType: dataType
        , success: function (data, textStatus) {
            if (successEvent != undefined && successEvent != null) {
                successEvent(data, textStatus);
            }
        }
    });
}

/*
功能：顯示附件列表
參數：strTableID：附件列表ID
            data：附件JSON數據(附件ID，附件名稱，附件路徑，附件備註，所屬分類，分類代碼)
            strURL：刪除附件的功能頁面
*/
function AddAttachTable(strTableID, strURL, data) {
    $.each(data, function (i) {
        var item = data[i];
        //max_line_num = $("#" + strTableID + " tr:last-child").prev().children("td").html(); form_attach
        max_line_num = $("#form_attach tr:last-child").prev().children("td").html();
        if (max_line_num == null) {
            max_line_num = 1;
        }
        else {
            max_line_num = parseInt(max_line_num);
            max_line_num += 1;
        }
        //$('#' + strTableID).append("<tr id='tr_" + item.AttachID + "'><td>" + max_line_num + "</td><td><a target='_blank' href='" + item.SavePath.replace('\\', '/') + "/" + item.ServerFileName + "'>" + item.FileName + "</a></td><td>" + item.Remarks + "</td><td><a href=\"javascript:DeleteAttach('" + item.AttachID + "','" + item.FormID + "','" + item.AppType + "','" + strURL + "');\">删除</a></td></tr>");
        $('#' + strTableID).before("<tr id='tr_" + item.AttachID + "'><td>" + max_line_num + "</td><td><a target='_blank' href='" + item.SavePath.replace('\\', '/') + "/" + item.ServerFileName + "'>" + item.FileName + "</a></td><td>" + item.Remarks + "</td><td><a href=\"javascript:DeleteAttach('" + item.AttachID + "','" + item.FormID + "','" + item.AppType + "','" + strURL + "');\">删除</a></td></tr>");
        $(".form_attach_no").html(parseInt(max_line_num + 1));
    });
}

/*
功能：顯示附件列表
參數：strTableID：附件列表ID
            strTypeID：分類ID
            strTypeCode：分類代碼
            strURL：刪除附件的功能頁面
*/
function ShowAttachTable( strTableID, strAppID, strAppType,strURL) {
    if ((strAppID != undefined) && (strAppID != '')) {
        $.ajax({
            type: "POST",
            url: strURL,
            data: "op=list&hid=" + strAppID + "&htype=" + strAppType,
            dataType: 'json', 
            success: function (data, textStatus) {
                AddAttachTable(strTableID, strURL, data.AttachList);
            }
        });
    }
}

/*
功能：顯示附件列表
參數：strFileID：文件ID
            strTypeID：分類ID
            strTypeCode：分類代碼
            strURL：刪除附件的功能頁面
*/
function DeleteAttach(strFileID, strAppID, strAppType, strURL) {

    var formID = $("#Form_ID").val();
    var userID = $("#LogOnUser").val();
    $.ajax({
        type: "POST"
        , contentType: "application/x-www-form-urlencoded; charset=utf-8"
        , url: g_WebSiteRootURL + "/SysUser/JudgeRight"
        , data: "right=attach&user=" + userID + "&formid=" + formID
        , success: function (data, textStatus) {
            if (data == 'True') {
                if (window.confirm("您確定要刪除該附件嗎？")) {
                    $.ajax({
                        type: "POST"
                        , url: strURL
                        , data: "op=del&hid=" + strAppID + "&htype=" + strAppType + "&fid=" + strFileID
                        , success: function (data, textStatus) {
                            if (!(data == '' || data == undefined)) {
                                $("#tr_" + strFileID).remove();
                                
                                var i = 0;
                                $("#form_attach tr").each(function () {                                    
                                    var AA = $(this).children("TD").eq(1).html();
                                    if (!(AA == null || AA == '' || AA == undefined)) {
                                        $(this).children("TD").eq(0).html(i);
                                    }
                                    i++;
                                }
                                );


                                alert("成功刪除了一條附件！");
                            }
                        }
                    });
                }
            }
            else {
                alert("您沒有權限删除该附件！");
            }
        }
    });

}

/*
功能：顯示附件列表
參數：strUploadID：上傳控件ID
            strTableID：附件列表ID
            strTypeID：分類ID
            strTypeCode：分類代碼
            strRemark：備註
            strURL：刪除附件的功能頁面
*/
function ajaxFileUpload(strUploadID, strTableID, strTypeID, strTypeCode, strRemark, strURL) {
    $.ajaxFileUpload({
        url: strURL,
        secureuri: false,
        fileElementId: strUploadID,
        dataType: 'json',
        data: { 'op': 'add', 'UploadID': strUploadID, 'hid': strTypeID, 'hcode': strTypeCode, 'cm': strRemark },
        beforeSend: function () {
            $("#loading").show();
        },
        complete: function () {
            $("#loading").hide();
        },
        success: function (data, status) {
            if (typeof (data.Error) != 'undefined') {
                if (data.Error != '') {
                    alert(data.Error);
                } else {
                    AddAttachTable(strTableID, strURL, data.AttachList);
                }
            }
        },
        error: function (data, status, e) {
            alert(e);
        }
    });
}

//Ajax刷新簽核人員ListBox
function RefreshSignUser(formID, signState, listID) {
    AjaxExec("/Sign/GetList"
            , "formid=" + formID + "&signstate=" + signState
            , "json"
            , function (data, textStatus) {
                var ctrList = $("#" + listID);
                if (data == null || data == undefined || data == '') { ctrList.empty(); return; }
                ctrList.empty();
                $.each(data, function (i) {
                    ctrList.append("<option value='" + data[i].UserID + "'>" + data[i].FNameCN + "</option>");
                });
            });
}
//Ajax刷新簽核人員TextBox
function RefreshSignUser2(formID, signState, txtID) {
       $("#" + txtID).val('');
       AjaxExec("/Sign/GetList"
        , "formid=" + formID + "&signstate=" + signState
        , "json"
        , function (data, textStatus) {
            if (data == null || data == undefined || data == '') { return; }
            if (data.length > 0) {
                $("#" + txtID).val(data[0].FNameCN);
            }
        });
}

/* 拋轉申請單 */
function SendForm(id,sendType) {
    $.ajax({
        type: "POST",
        url: g_WebSiteRootURL + "/FormHead/Send",
        data: "id=" + id + "&sendtype=" + sendType,
        success: function (data, textStatus) {
            alert(data);
        }
    });
}