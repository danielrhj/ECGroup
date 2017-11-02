/**
* 模块名：mms viewModel
* 程序名: mms.payee.js
**/
var mms = mms || {};
mms.customer = mms.customer || {};
var urls = "";
var user = $.cookie('UserCode');

mms.customer.list = function (data) {
    var self = this;
    this.urls = data.urls; urls = data.urls;
    this.resx = data.resx;
    this.idField = data.idField;
    this.dataSource = data.dataSource;
    this.form = ko.mapping.fromJS(data.form);
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    delete this.form.__ko_mapping__;

    this.grid = {        
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true,pageSize:10,
        singleSelect:false,
        onLoadSuccess: function (data) {   
            localStorage.removeItem('keyPayee');
            localStorage.setItem('keyPayee', JSON.stringify(data.keyRows));
        }
    };

    this.grid.queryParams(data.form);
 
    this.searchClick = function () {
        var param = ko.toJS(this.form); com.setFirstPageWhenSearchGrid(self.grid);
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
        com.openTab('新增客户', "/mms/customer/Edit/");
    };
    this.deleteClick = function () {
        var array = [],arrayCustCode = [];;

        var checkedItems = self.grid.datagrid('getChecked');
        if (!checkedItems) return com.message('warning', self.resx.noneSelect);

        $.each(checkedItems, function (index, item) {
            array.push(item.CustID); arrayCustCode.push(item.CustCode);
        });
        var str = array.join(',');

        com.message('confirm', "确定要删除" + arrayCustCode.join(',') + "的客户資料吗？", function (b) {
            if (b) {
                post = { value: escape(str) };
                com.ajax({
                    data:ko.toJSON(post),
                    url: self.urls.remove,
                    success: function () {
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
        com.openTab(row.CustAbbr + '明細資料', self.urls.edit + row[self.idField]);
        
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
        com.ajax({
            data: ko.toJSON(self.form),
            url: self.urls.excel,
            success: function (w) {
                if (w.Msg.length > 0) {
                    com.message('warning', w.Msg);
                }
                else {
                    com.message('success', '客户清单生成OK！');
                    if (w.url)
                    { com.openFile(w.url); }
                }
            }
        });
    };

    this.openUpload = function () {
        $('#idfile').window("open");
    };   

    this.canShow = function (item) {     
        var buttons = self.dataSource.buttonsList;       
        var flag=false;        
       
        for(var k=0;k<buttons.length;k++) {
            if (buttons[k].ButtonIcon == item) {
                flag = true; break;
            }
        };
        return flag;
    };
};

mms.customer.relate = function (data) {
    var self = this;
    this.urls = data.urls; urls = data.urls;
    this.resx = data.resx;
    this.idField = data.idField;
    this.dataSource = data.dataSource;
    this.form = ko.mapping.fromJS(data.form);
    this.form.action = 'ListCustPNRelate';
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    delete this.form.__ko_mapping__;

    this.grid = {
        //size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true, pageSize: 10,
        singleSelect: false
    };

    this.grid.queryParams(self.form);

    this.searchClick = function () {
        var param = ko.toJS(this.form); com.setFirstPageWhenSearchGrid(self.grid);
        this.grid.queryParams(param);
    };
    this.clearClick = function () {
        $.each(self.form, function () { if (this != self.form.action) this(''); });
        this.searchClick();
    };
    this.refreshClick = function () {
        window.location.reload();
    };   
    
    this.downloadClick = function (vm, event) {
        com.ajax({
            data: ko.toJSON(self.form),
            url: self.urls.excel,
            success: function (w) {
                if (w.Msg.length > 0) {
                    com.message('warning', w.Msg);
                }
                else {
                    com.message('success', '客户料号关联清单生成OK！');
                    if (w.url)
                    { com.openFile(w.url); }
                }
            }
        });
    };

    //this.openUpload = function () {
    //    $('#idfile').window("open");
    //};

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

var openURL = function (data,code) { 
    com.openTab(code + '明細資料', urls.edit + data);
}