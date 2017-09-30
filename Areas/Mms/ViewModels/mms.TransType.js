/**
* 模块名：mms viewModel
* 程序名: mms.TransType.js
**/
var mms = mms || {};
var user = $.cookie('UserCode');

mms.TransType = function (data) {
    var self = this;
    this.urls = data.urls;
    this.resx = data.resx;
    this.dataSource = data.dataSource;
    this.form = ko.mapping.fromJS(data.form);
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    delete this.form.__ko_mapping__;

    this.grid = {
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true
    };

    this.grid.queryParams(data.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤

    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onDblClickRow = this.gridEdit.begin;
    this.grid.onClickRow = this.gridEdit.ended;
    //this.grid.OnAfterCreateEditor = function (editors, row) {
    //    if (!row._isnew) {
    //        com.readOnlyHandler('input')(editors.LEGAL_ENTITY.target, true);
    //        com.readOnlyHandler('input')(editors.PAYEE_CODE.target, true);
    //        com.readOnlyHandler('input')(editors.CURRENCY.target, true);
    //    }
    //};
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
        com.ajax({
            type: 'GET',
            url: self.urls.add,
            success: function (d) {
                var row = $.extend({ ID: d }, self.defaultRow);
                self.gridEdit.addnew(row);
            }
        });

    };
    this.deleteClick = self.gridEdit.deleterow;
    this.editClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        self.gridEdit.begin()
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