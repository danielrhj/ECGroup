/**
* 模块名：mms viewModel
* 程序名: mms.MaterialType.js
**/
var mms = mms || {};

mms.ARList = function (data) {
    var self = this;

    this.tabConst = { grid: 'grid', form: 'form', tab: 'tab', edit: 'gridEdit' };
    this.urls = data.urls;
    this.resx = data.resx;
    this.idField = data.idField
    this.dataSource = data.dataSource;
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    this.form = ko.mapping.fromJS(data.form);
    delete this.form.__ko_mapping__;

    this.grid = {
        //idField: 'ARID',
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true,
        singleSelect: false,
        checkOnSelect: false, autoRowHeight: false,
        onLoadSuccess: function (data) {    //根據checkbox綁定的值禁用checkbox

            for (var i = 0; i < data.rows.length; i++) {
                //根据ck的值让某些行不可选
                if (data.rows[i].ck == "N") {
                    $("input[type='checkbox']")[i + 1].disabled = true;
                }
            }

            $('input:checkbox[name!="ck"]').unbind();//解除單頭的checkbox全選事件,重写DataGrid全选复选框事件
            $('input:checkbox[name!="ck"]').bind("click", function () {

                if ($('input:checkbox[name!="ck"]').prop("checked")) {
                    $('input:checkbox[name="ck"]').each(function (index, el) {
                        //先选中所有复选框                      
                        el.checked = true;

                    });

                    //遍历复选框组，符合条件的不选中
                    $('input:checkbox[name="ck"]').each(function (index, el) {
                        if (el.value == "N") { el.checked = false; }
                    });
                }
                else {
                    $('input:checkbox[name="ck"]').each(function (index, el) {
                        el.checked = false;
                    });

                }
            });
            com.mergeGridCopy(self.grid, ['ShipNo'], ['Customer', 'RcvAmount', 'Currency', 'DueDate', 'ARStatus', 'BatchNo', 'BatchDate', 'Remarks', 'RcvNo', 'RcvDate', 'RcvDetail']);
        },
        onClickRow: function (rowIndex, rowData) {      //單選時不能選中禁用的checkbox
            //加载完毕后获取所有的checkbox遍历
            $("input[type='checkbox']").each(function (index, el) {
                //如果当前的复选框不可选，则不让其选中
                if (el.disabled == true) {
                    $("#gridlist").datagrid('unselectRow', index - 1);
                }               

            })
        },
        onCheck: function (index, row) {        
            var shipNo = row.ShipNo;
            var rows = $("#gridlist").datagrid('getRows');
            for (var i = 0; i < rows.length; i++) {                
                if (shipNo == rows[i].ShipNo && (i != index)) {
                    $("input[type='checkbox']")[i + 1].checked = true;// = row.checked;
                }
            }
        },
        onUncheck: function (index, row) {
            var shipNo = row.ShipNo;
            var rows = $("#gridlist").datagrid('getRows');
            for (var i = 0; i < rows.length; i++) {
                if (shipNo == rows[i].ShipNo && (i != index)) {
                    $("input[type='checkbox']")[i + 1].checked = false;// = row.checked;
                }
            }
        }
    };

    this.grid.queryParams(data.form);

    this.gridEdit = new com.editGridViewModel(self.grid);

    //this.grid.OnAfterCreateEditor = function (editors, row) {        
    //     com.readOnlyHandler('input')(editors.ck.target, true);
    //};

    //this.grid.onDblClickRow = this.gridEdit.begin;    
    this.grid.toolbar = "#gridtb"; 

    this.grid.onClickRow = function (rowIndex, rowData) { //this.grid.onDblClickRow
        if (rowData.ck=='N') return;        
        self.gridEdit.begin();
    };

    this.searchClick = function () {
        var param = ko.toJS(this.form);
        com.setFirstPageWhenSearchGrid(self.grid);
        this.grid.queryParams(param);
    };
    this.clearClick = function () {
        $.each(self.form, function () { this(''); });
        this.searchClick();
    };
    this.refreshClick = function () {
        window.location.reload();
    };
    this.CreateBatchNoClick = function () {
        var checkedItems = self.grid.datagrid('getChecked');
        if (checkedItems.length==0) return com.message('warning', self.resx.noneSelect);

        var array = [];
        $.each(checkedItems, function (index, item) {
            array.push(item.ARID);
        });
        var str = array.join(',');
        com.message('confirm', self.resx.batchConfirm, function (b) {
            if (b) {
                post = { value: str };
                com.ajax({
                    data: ko.toJSON(post),
                    url: self.urls.createbatch,
                    success: function (d) {
                        self.form.BatchNo(d);
                        self.searchClick();
                        com.message('success', self.resx.BatchNoSuccess);
                    }
                });
            }
        });
        //com.openTab('新增明細資料', "/mms/BA_Payee/Edit/");
    };

    this.ExportBatchClick = function () {
        var batchNo = self.form.BatchNo();

        if (batchNo == '') { com.message('warning', '请录入对账单号,然后在点击导出對賬单'); return; }
        com.ajax({            
            url: self.urls.createBatchExcel + batchNo,
            success: function (w) {  
                if (w.Msg.length > 0) {
                    com.message('warning', w.Msg);
                }
                else {
                    com.message('success', self.resx.exportBatchExcelSuccess);
                    if (w.url)
                    { com.openFile(w.url); }
                }
            }
        });
    }
   
    this.saveClick = function () {
        self.gridEdit.ended(); //结束grid编辑状态
        var post = {};
        post.list = self.gridEdit.getChanges(self.setting.postListFields);
        post.form = self.form;
        if (self.gridEdit.ended() && post.list._changed) {
            com.ajax({
                url: self.urls.edit,
                data: ko.toJSON(post),
                success: function (d) {
                    com.message('success', self.resx.editSuccess);
                    self.gridEdit.accept();
                    self.grid.queryParams(self.form);//重新查詢表格數據
                }
            });
        }
    };   

    this.canShow = function (item) {
        var buttons = self.dataSource.buttonsList;
        var flag = false;

        for (var k = 0; k < buttons.length; k++) {
            if (buttons[k].ButtonIcon == item) {
                flag = true; break;
            }
        };
        return flag;
    };
};

mms.Detail = function (data) {
    var self = this;
    this.urls = data.urls;
    this.resx = data.resx;
    this.idField = data.idField
    this.dataSource = data.dataSource;
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    this.form = ko.mapping.fromJS(data.form);
    delete this.form.__ko_mapping__;

    this.grid = {
        size: { w: 4, h: 40 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true, pageSize: 10,
        singleSelect: false
    };

    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.queryParams({ ARID: self.form.ARID() });

    this.grid.onDblClickRow = this.gridEdit.begin;
    this.grid.onClickRow = this.gridEdit.ended;
   
    this.refreshClick = function () {
        window.location.reload();
    };
    this.addClick = function () {
        var row = self.defaultRow;
        self.gridEdit.addnew(row);
    };
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelections');  //注意getSelected只能選到一行
        if (!row) return com.message('warning', self.resx.noneSelect);

        var array = [], arrayTypeCode = [];

        $.each(row, function (index, item) {
            array.push(item.SNO); arrayTypeCode.push(item.RcvNo);
        });
        var str = array.join(',');
        com.message('confirm', "确定要删除 " + arrayTypeCode.join(',') + " 的資料吗？", function (b) {
            if (b) {
                com.ajax({
                    data: ko.toJSON({ value: str }),
                    url: self.urls.remove,
                    success: function () {
                        com.message('success', self.resx.deleteSuccess);
                        self.grid.queryParams({ ARID: self.form.ARID() });
                    }
                });
            }
        });
    };


    this.editClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        self.gridEdit.begin();       

    };
    this.grid.onDblClickRow = this.editClick;

    this.saveClick = function () {
        self.gridEdit.ended(); //结束grid编辑状态
        var post = {};
        post.list = self.gridEdit.getChanges(self.setting.postListFields);
        post.form = self.form;
        if (self.gridEdit.ended() && post.list._changed) {
            com.ajax({
                url: self.urls.edit,
                data: ko.toJSON(post),
                success: function (d) {
                    com.message('success', self.resx.editSuccess);
                    self.gridEdit.accept();
                    self.grid.queryParams(self.form);//重新查詢表格數據
                }
            });
        }
    };

    this.canShow = function (item) {
        var buttons = self.dataSource.buttonsList;
        var flag = false;

        for (var k = 0; k < buttons.length; k++) {
            if (buttons[k].ButtonIcon == item) {
                flag = true; break;
            }
        };
        return flag;
    };
};

var openURL = function (data0, data1) { 
    com.openTab('出库明细--' + data1, "/mms/shiping/Edit/" + data0);
}

var openURLDetail = function (data) {    
    var param = {BizID:data,BizTable:'ARSub'};
    mms.com.ARAPDetail(param);
};

var SaveAROnceClear = function (edit) {
    var arid=edit;
    var row = $('#gridlist').datagrid('getSelections')[0];  
    if (!row) { return; }
    var rowIndex = $('#gridlist').datagrid('getRowIndex', row);
    var editor = $('#gridlist').datagrid('getEditors', rowIndex); 

    var DueDate = $(editor[0].target).datebox('getValue');
    var ARStatus = $(editor[1].target).combobox('getValue');
    var RcvNo = $(editor[2].target).val();
    var RcvDate=$(editor[3].target).datebox('getValue');
    var Remarks = $(editor[4].target).val();

    if (!RcvNo || !RcvDate) { com.message('error', '一次结清时必须录入收款单据号码和实际收款日期!'); }

    var result = "";
    post = { key: arid, duedate: DueDate, arstatus: ARStatus, rcvno: RcvNo, rcvdate: RcvDate,remarks:Remarks };

    com.ajax({
        async: false,   
        url: data.urls.saveClearOnce,
        data: ko.toJSON(post), 
        success: function (d) {
            if (d.success) {
                if (d.Msg)
                { com.message('warning', d.Msg); }
                else
                { com.message('success', '保存成功!'); }
            }
        },
        error: function (e) {
            com.message('error', e.responseText);
        }
    });

};

