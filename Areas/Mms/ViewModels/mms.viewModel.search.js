/**
* 模块名：mms viewModel
* 程序名: mms.viewModel.search.js
* Copyright(c) 2013-2015 liuhuisheng [ liuhuisheng.xm@gmail.com ] 
**/
var mms = mms || {};
var urls = ""; var resx = "";
mms.viewModel = mms.viewModel || {};

mms.viewModel.search = function (data) {
    var self = this;
    this.idField = data.idField || "BillNo";
    this.urls = data.urls; urls = data.urls;
    this.resx = data.resx; resx = data.resx;
    this.dataSource = data.dataSource;
    this.form = ko.mapping.fromJS(data.form);
    delete this.form.__ko_mapping__;

    this.grid = {
        size: { w: 4, h: 94 },        
        url: self.urls.query,
        //url: "/api/sys/user",
        queryParams: ko.observable(),
        pagination: true,
        customLoad: false
    };

    this.grid.queryParams(data.form);
    

    this.searchClick = function () {
        var param = ko.toJS(this.form);
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
            url: self.urls.billno, //注意這個路由獲取新的BillNo,方法在繼承的ApiController：MmsBaseApi裏面
            success: function (d) {//d就是獲取的新單號
                com.openTab(self.resx.detailTitle, self.urls.edit + d); //注意新單號作為此路由的參數傳入,注意此路由不能使用ApiController的路径
            }
        });
    };

    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        com.message('confirm', self.resx.deleteConfirm, function (b) {
            if (b) {
                com.ajax({
                    type: 'DELETE', url: self.urls.remove + escape(row[self.idField]), success: function () {
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
        com.openTab(self.resx.detailTitle, self.urls.edit + row[self.idField]);
    };

    this.grid.onDblClickRow = this.editClick;
    
    this.auditClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        com.auditDialog(function (d) {
            com.ajax({
                type: 'POST',
                url: self.urls.audit + row[self.idField],
                data: JSON.stringify(d),
                success: function () {
                    com.message('success', self.resx.auditSuccess);
                }
            });
        });
    };

    this.downloadClick = function (vm, event) {
        com.exporter(self.grid).download($(event.currentTarget).attr("suffix"));
    };
};

var openURL = function (data) { //注意這個方法要寫在最外面
    com.openTab(resx.detailTitle, urls.edit + data);
}
 