/**
* 模块名：mms viewModel
* 程序名: mms.payee.js
**/
var mms = mms || {};
mms.Profit = mms.Profit || {};

mms.Profit.Search = function (data) {
    var self = this;
    this.urls = data.urls;
    this.resx = data.resx;
    this.idField = data.idField
    this.dataSource = data.dataSource;
    this.form = ko.mapping.fromJS(data.form);
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    delete this.form.__ko_mapping__;

    this.grid = {
        idField: '_id',
        treeField: 'PO',
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        singleSelect:false,
        pagination: true,
        loadFilter: function (d) {
            d = utils.copyProperty(d.rows || d, ["PO", "IconClass"], ["_id", "iconCls"], false);
            return utils.toTreeData(d, '_id', 'ParentCode', "children");
        }
    };

    this.grid.queryParams(data.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤
       
    this.searchClick = function () {
        //com.setFirstPageWhenSearchGrid(self.grid);
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
   
    this.downloadClick = function (vm, event) {
        var excelType = $(event.currentTarget).attr("suffix");
        var parm = $.extend({}, self.form, { type: excelType });//写this.form也可以
        //com.message('warning', '导出功能正在建设中');
        com.ajax({
            data: ko.toJSON(parm),
            url: self.urls.excel,
            success: function (w) {
                if (w.Msg.length > 0) {
                    com.message('warning', w.Msg);
                }
                else {
                    com.message('success', '利润表生成OK！');
                    if (w.url)
                    { com.openFile(w.url); }
                }
            }
        });
    };

    this.canShow = function (item) {
        var buttons = self.dataSource.buttonsList;
        var flag = false;

        for (var k = 0; k < buttons.length; k++) {
            if (buttons[k].ButtonIcon == item) {
                flag = true; break;
            }
        };
        return flag;;
    };
};

mms.Profit.Stock = function (data) {
    var self = this;
    this.urls = data.urls;
    this.resx = data.resx;
    this.idField = data.idField
    this.dataSource = data.dataSource;
    this.form = ko.mapping.fromJS(data.form);
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    delete this.form.__ko_mapping__;

    this.grid = {
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        singleSelect: false,
        pagination: true,
        showFooter: true, autoRowHeight: false,
        onLoadSuccess: function (d) {
            var rows = $('#gridlist').datagrid('getRows');
            if (rows.length > 1) {
                var atotal = 0,bq=0,sq=0,bl=0;    
                $.each(rows, function (i, v) {
                    atotal += v['Amount'];
                    bq += v['BuyQty']; sq += v['ShipQty']; bl += v['Balance'];
                });
                $('#gridlist').datagrid('reloadFooter', [{ BuyQty: bq, ShipQty: sq, Balance: bl, BuyNo: '', Unit: '<label style="color:blue"><b>Total：</b></label>', Amount: atotal }]);
            }

            com.mergeGridCopy(self.grid, ['RcvNo'], ['Supplier']);
        },
        rowStyler: function (index, row) {
            if (row.Balance==0) {
                return 'background-color:pink;color:blue;font-weight:bold;';
            }
}
    };

    this.grid.queryParams(data.form); 

    this.searchClick = function () {
        com.setFirstPageWhenSearchGrid(self.grid);
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

    this.downloadClick = function (vm, event) {
        var excelType = $(event.currentTarget).attr("suffix");
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
        return flag;;
    };
};

var openURL = function (data0, data1,type) { //注意這個方法要寫在最外面
    if (type == 'SaleOrder') {
        localStorage.setItem('keyPOID', data0);
        com.openTab('客户订单--' + data1, "/mms/saleorder/Edit/" + data0);
    }
    if (type == 'BuyOrder') {
        localStorage.setItem('keyBuyID', data0);
        com.openTab('采购订单--' + data1, "/mms/buyorder/Edit/" + data0);
    }
}

var openURLShipQty = function (po, custPN, suppPN) {
    com.dialog({
        title: "&nbsp;采购明细",
        iconCls: 'icon-user',
        width: 1120,
        height: 600,
        html: "#Cost-template",
        viewModel: function (w) {
            var that = this;
            //this.form = this.form || {};
            //this.form.PO = ko.observable(po);
            //this.form.CustPN = ko.observable(custPN);
            //this.form.SuppPN = ko.observable(suppPN);

            this.grid = {
                width: 1100,
                height: 530,
                pagination: true,
                pageSize: 20,
                url: "/api/mms/profit/getBuyCostDetail/",
                queryParams: ko.observable()
            };
               
            this.grid.queryParams({ PO: po, CustPN: custPN,SuppPN:suppPN });           
            this.cancelClick = function () {
                w.dialog('close');
            };           
        }
    });
};

