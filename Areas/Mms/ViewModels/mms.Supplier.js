/**
* 模块名：mms viewModel
* 程序名: mms.Supplier.js
**/
var mms = mms || {};
var urls = "";
var user = $.cookie('UserCode');

mms.Supplier = function (data) {
    var self = this;
    this.urls = data.urls; urls = data.urls;
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
        pagination: true, pageSize: 10,
        singleSelect: false,
        onLoadSuccess: function (data) {   
        localStorage.removeItem('keySuplier');
        localStorage.setItem('keySuplier', JSON.stringify(data.keyRows));
    }           
    };

    this.grid.queryParams(data.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤

    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onDblClickRow = this.gridEdit.begin;
    this.grid.onClickRow = this.gridEdit.ended;
    this.grid.OnAfterCreateEditor = function (editors, row) {
        if (!row._isnew) {
            //com.readOnlyHandler('input')(editors.LEGAL_ENTITY.target, true);
            com.readOnlyHandler('input')(editors.PAYEE_CODE.target, true);
            com.readOnlyHandler('input')(editors.CURRENCY.target, true);
        }
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
    this.addClick = function () {       
        com.openTab('新增供应商', "/mms/supplier/Edit/");
    };
    this.deleteClick = function () {
        var array = [],arrayCustCode = [];;

        var checkedItems = self.grid.datagrid('getChecked');
        if (!checkedItems) return com.message('warning', self.resx.noneSelect);

        $.each(checkedItems, function (index, item) {
            array.push(item.SuppID); arrayCustCode.push(item.SuppCode);
        });
        var str = array.join(',');

        com.message('confirm', "确定要删除" + arrayCustCode.join(',') + "的供应商資料吗？", function (b) {
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
        com.openTab('供应商明细資料', "/mms/supplier/Edit/" + row.SuppID);
    };
    this.grid.onDblClickRow = this.editClick;

    this.saveClick = function () {
        self.gridEdit.ended(); //结束grid编辑状态
        var post = {};
        post.list = self.gridEdit.getChanges(self.setting.postListFields);
        post.form = self.form;
        if (post.list._changed) {
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
                    com.message('success', '供应商清单生成OK！');
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
        var flag = false;

        for (var k = 0; k < buttons.length; k++) {
            if (buttons[k].ButtonIcon == item) {
                flag = true; break;
            }
        };
        return flag;
    };
};

var openURL = function (data) {
    com.openTab(data + '明細資料', urls.edit + data);
}

var openContactInfo = function (data) {
        com.dialog({
            title: "&nbsp;開票聯繫人明細--" + data,
            iconCls: 'icon-user',
            width: 920,
            height: 420,
            html: "#contact-template",
            viewModel: function (w) {
                var that = this;
                this.grid = {
                    width: 900,
                    height: 340,
                    pagination: true,
                    pageSize: 20,
                    url: "/api/mms/ba_supplier/GetSupplierContact/",
                    queryParams: { VendorCode: data }
                };

                this.vendorcode = ko.observable(data);
                this.gridEdit = new com.editGridViewModel(this.grid);
                this.grid.OnAfterCreateEditor = function (editors, row) {
                    com.readOnlyHandler('input')(editors["VendorCode"].target, true);
                };
                this.grid.onClickRow = that.gridEdit.ended;
                this.grid.onDblClickRow = that.gridEdit.begin;
                this.grid.toolbar = [
                    {
                        text: '新增', iconCls: 'icon-add1', handler: function () {
                            var t = com.formatDateTime(new Date());
                            var row = { VendorCode: data, CreateDate: t };
                            that.gridEdit.addnew(row);
                        }
                    }, '-',
                    { text: '编辑', iconCls: 'icon-edit', handler: that.gridEdit.begin }, '-',
                    { text: '删除', iconCls: 'icon-cross', handler: that.gridEdit.deleterow }
                ];
                this.confirmClick = function () {
                    if (that.gridEdit.isChangedAndValid()) {
                        var list = that.gridEdit.getChanges(['AutoID', 'VendorCode', 'Contact', 'Tel', 'Mail', 'ProjectName', 'Remark']);
                        com.ajax({
                            url: '/api/mms/ba_supplier/EditSupplierContact/',
                            data: ko.toJSON({ list: list }),
                            success: function (d) {
                                //that.grid.queryParams({ VendorCode: data }); 這一句報錯，原因不明
                                that.gridEdit.ended;
                                com.message('success', '保存成功！');
                            }
                        });
                    }
                };
                this.cancelClick = function () {
                    w.dialog('close');
                };

            }
        });
    };
