
//Ajax刷新簽核人員ListBox
function RefreshSignUser(formID, signState, listID) {
    $.ajax({
        type: "POST"
                , contentType: "application/x-www-form-urlencoded; charset=utf-8"
                , url: g_WebSiteRootURL + "/Sign/GetList"
                , data: "formid=" + formID + "&signstate=" + signState
                , success: function (data, textStatus) {
                    if (data == null || data == undefined || data == '') { return; }
                    var jsonData = eval(data);
                    var ctrList = $("#" + listID);
                    ctrList.empty();
                    $.each(jsonData, function (i) {
                        ctrList.append("<option value='" + jsonData[i].UserID + "'>" + jsonData[i].FNameCN + "</option>");
                    });
                }
    });
}

//申請單項次--表格增加一行數據
function gridFormItemAppend(dateFormItem) {
    var tr = "<tr>";
    tr = tr + "<td><input name='chkSelectItem' type='checkbox' value='" + dateFormItem.ItemID + "' /></td>";
    var max_line_num = $("#gridFormItem  tr:last-child").children("td").eq(1).html();
    max_line_num = max_line_num == null ? 0 : parseInt(max_line_num) + 1;
    tr = tr + "<td>" + max_line_num + "</td>";
    tr = tr + "<td>" + dateFormItem.OutSubjectID + "," + dateFormItem.OutSubjectName + "</td>";
    tr = tr + "<td>" + dateFormItem.InSubjectID + "," + dateFormItem.InSubjectName + "</td>";
    tr = tr + "<td>" + dateFormItem.Abstract + "</td>";
    tr = tr + "<td>" + dateFormItem.CurrencyID + "</td>";
    tr = tr + "<td>" + dateFormItem.Amount + "</td>";
    tr = tr + "<td>" + dateFormItem.Remarks + "</td>";
    tr = tr + "</tr>";
    $('#gridFormItem').append(tr);
}

//申請單項次--表格刪除所有勾選數據
function gridFormItemDelete(id) {
    var trlist = $('#gridFormItem').find("tr");
    if (trlist.length < 2) return;
    for (var i = 2, j = 1; i < trlist.length; i++) {
        var tr = $(trlist[i]);
        var input = tr.find("INPUT[type='checkbox']");
        if (input.val() == id) {
            tr.remove();
        }
        else {
            tr.find('td').eq(1).html(j);
            j++;
        }
    }
}

//獲取法人代碼信息
function AjaxGetCorporationInfo(corpID, logonUserID) {
    $.ajax({
        type: "POST"
                , contentType: "application/x-www-form-urlencoded; charset=utf-8"
                , url: g_WebSiteRootURL + "/Corporation/GetModel"
                , data: "range=1&corpid=" + corpID + "&user=" + logonUserID
                , success: function (data, textStatus) {
                    if (data == null || data == undefined || data == '') {
                        $("#Corp_FName").val('');
                        $("#hideCorpID").val('');
                        alert('@CT.Resources.Views.FormHead_Edit_GetCorpAlert');
                        return;
                    }
                    var jsonData = eval(data)[0];
                    //Ajax更新
                    $("#Corp_ID").val(jsonData.CorpID);
                    $("#hideCorpID").val(jsonData.CorpID);
                    $("#Corp_FName").val(jsonData.FName);
                }
    });
}
//獲取費用代碼信息
function AjaxGetDeptInfo(corpID, deptID, io) {
    $.ajax({
        type: "POST"
                , contentType: "application/x-www-form-urlencoded; charset=utf-8"
                , url: g_WebSiteRootURL + "/Department/GetModel"
                , data: "deptid=" + deptID + "&corpid=" + corpID
                , success: function (data, textStatus) {
                    if (data == null || data == undefined || data == '') {
                        alert('@CT.Resources.Views.FormHead_Edit_GetDeptAlert');
                        if (io == 'in') {
                            $("#hideInDeptID").val('');
                            $("#InDeptName").val('');
                            $("#InDeptAttribute").val('');
                        }
                        if (io == 'out') {
                            $("#hideOutDeptID").val('');
                            $("#OutDeptName").val('');
                            $("#OutDeptAttribute").val('');
                        }
                        return;
                    }
                    var jsonData = eval(data)[0];
                    //Ajax更新
                    if (io == 'in') {
                        $("#InDeptID").val(jsonData.DeptID);
                        $("#hideInDeptID").val(jsonData.DeptID);
                        $("#InDeptName").val(jsonData.FName);
                        $("#InDeptAttribute").val(jsonData.DeptAttr);
                        return;
                    }
                    if (io == 'out') {
                        $("#OutDeptID").val(jsonData.DeptID);
                        $("#hideOutDeptID").val(jsonData.DeptID);
                        $("#OutDeptName").val(jsonData.FName);
                        $("#OutDeptAttribute").val(jsonData.DeptAttr);
                        return;
                    }
                }
    });
}
//獲取科目信息
function AjaxGetSubjectInfo(corpID, subID, io, attr) {
    var isout = 1;
    if (io == 'in') { isout = 2; }
    $.ajax({
        type: "POST"
                , contentType: "application/x-www-form-urlencoded; charset=utf-8"
                , url: g_WebSiteRootURL + "/FSubject/GetModel"
                , data: "corpid=" + corpID + "&subid=" + subID + "&attr=" + attr + "&io=" + isout
                , success: function (data, textStatus) {
                    if (data == null || data == undefined || data == '') {
                        alert('@CT.Resources.Views.FormHead_Edit_GetSubjectAlert');
                        if (isout == 1) {
                            $("#hideItemOutSubjectID").val('');
                            $("#txtItemOutSubjectID").val('');
                        }
                        else {
                            $("#hideItemInSubjectID").val('');
                            $("#txtItemInSubjectID").val('');
                        }
                        return;
                    }
                    var jsonData = eval(data)[0];
                    //Ajax更新
                    if (isout == 1) {
                        $("#hideItemOutSubjectID").val(jsonData.SubjectID);
                        $("#txtItemOutSubjectID").val(jsonData.SubjectID + jsonData.FNameCN);
                    }
                    else {
                        $("#hideItemInSubjectID").val(jsonData.SubjectID);
                        $("#txtItemInSubjectID").val(jsonData.SubjectID + jsonData.FNameCN);
                    }
                }
    });
}