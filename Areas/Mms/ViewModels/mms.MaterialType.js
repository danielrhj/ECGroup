/**
* 模块名：mms viewModel
* 程序名: mms.MaterialType.js
**/
var mms = mms || {};

mms.MaterialType = function (data) {
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
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true,
        singleSelect: false
    };

    this.grid.queryParams(data.form);

    this.gridEdit = new com.editGridViewModel(this.grid);

    this.grid.OnAfterCreateEditor = function (editors, row) {        
         com.readOnlyHandler('input')(editors.TypeCode.target, true);
    };

    this.grid.onDblClickRow = this.gridEdit.begin;
    this.grid.onClickRow = this.gridEdit.ended;
   
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
    this.addClick = function () {
        com.ajax({
            type: 'GET',
            url: self.urls.add,
            success: function (data) {
                var row = { AutoID: '0', TypeCode: data, TypeName: ''};
                self.gridEdit.addnew(row);
            }
        });
        //com.openTab('新增明細資料', "/mms/BA_Payee/Edit/");
    };
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelections');  //注意getSelected只能選到一行
        if (!row) return com.message('warning', self.resx.noneSelect);

        var array = [], arrayTypeCode = [];;

        $.each(row, function (index, item) {
            array.push(item.AutoID); arrayTypeCode.push(item.TypeCode);
        });
        var str = array.join(',');
        com.message('confirm', "确定要删除 " + arrayTypeCode.join(',') + " 的資料吗？", function (b) {
            if (b) {
                com.ajax({
                    type: 'DELETE', url: self.urls.remove + escape(str), success: function () {
                        com.message('success', self.resx.deleteSuccess);
                        self.searchClick();
                    }
                });
            }
        });
    };

       
    this.editClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        self.gridEdit.begin()
        //com.openTab(row.ERP_CODE + '明細資料', "/mms/BA_Payee/Edit/" + row[self.idField]);

    };
    this.grid.onDblClickRow = this.editClick;
    this.auditClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        com.auditDialog(function (d) {
            com.ajax({
                type: 'POST',
                url: self.urls.audit + row.BillNo,
                data: JSON.stringify(d),
                success: function () {
                    com.message('success', self.resx.auditSuccess);
                }
            });
        });
    };
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
    this.downloadClick = function (vm, event) {
        com.exporter(self.grid).download($(event.currentTarget).attr("suffix"));
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

