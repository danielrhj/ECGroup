/**
* 模块名：mms viewModel
* 程序名: mms.payee.js
**/
var mms = mms || {};
mms.Ecom = mms.Ecom || {};

mms.Ecom.Search = function (data) {
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
        //size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        singleSelect:false,
        pagination: true,showFooter:true,
        onLoadSuccess: function (data) {
            localStorage.removeItem('keyEcomID');
            localStorage.setItem('keyEcomID', JSON.stringify(data.keyRows));

            var rows = $('#gridlist').datagrid('getRows');
            if (rows.length > 1) {
                var saleCNCY = rows[0]['SaleCNCY'], settleCNCY = rows[0]['SettleCNCY'];
                var aSaleTotal = 0, aSaleNet = 0, aSaleTotalNew = 0, aSaleNetNew = 0, aMainCost = 0, aLogCost = 0, aAdsCost = 0, aManCost = 0, aOverHead = 0, aFixedCost = 0, aOtherCost = 0, aOffSet = 0, aGrossProfit = 0, aGPRate = 0;
                $.each(rows,function(i,v) {
                    aMainCost += v['MainCost']; aLogCost += v['LogCost'];aSaleTotal += v['SaleTotal']; aSaleNet += v['SaleNet']; 
                    aAdsCost += v['AdsCost']; aManCost += v['ManCost']; aOverHead += v['OverHead']; aFixedCost += v['FixedCost']; aOtherCost += v['OtherCost'];
                    if (v['SaleCNCY'] == v['SettleCNCY'])
                    {
                        aSaleTotalNew += v['SaleTotal']; aSaleNetNew += v['SaleNet'];
                        aOffSet += v['SaleTotal'] * v["OffSet"] * 0.01;
                    }
                    else
                    {
                        aSaleTotalNew += v['SaleTotal'] / v["ExRate"]; aSaleNetNew += v['SaleNet'] / v["ExRate"];
                        aOffSet += (v['SaleTotal'] / v["ExRate"]) * v["OffSet"] * 0.01;
                    }
                });

                aGrossProfit = aSaleNetNew - aOffSet - aAdsCost - aManCost - aOtherCost - aMainCost - aLogCost - aOverHead - aFixedCost;
                aGPRate = aGrossProfit / aSaleTotalNew;

                $('#gridlist').datagrid('reloadFooter', [{ EcomID: -1, SaleNo: '', SaleMonth: '<label style="color:blue;"><b>Total：</b></label>', SaleTotal: aSaleTotal, SaleNet: aSaleNet, MainCost: aMainCost, LogCost: aLogCost, AdsCost: aAdsCost, ManCost: aManCost, OverHead: aOverHead, FixedCost: aFixedCost, OtherCost: aOtherCost, GrossProfit: aGrossProfit, GPRate: aGPRate, SaleCNCY: '<label style="color:blue;">' + saleCNCY + '</label>', SettleCNCY: '<label style="color:green;">' + settleCNCY + '</label>' }]);
            }
            else
            { $('#gridlist').datagrid({ showFooter: false }); }
        }
    };

    this.grid.queryParams(data.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤

    //this.gridEdit = new com.editGridViewModel(this.grid);
    //this.grid.onDblClickRow = this.gridEdit.begin;
    //this.grid.onClickRow = this.gridEdit.ended;
    //this.grid.OnAfterCreateEditor = function (editors, row) {
    //    if (!row._isnew) com.readOnlyHandler('input')(editors.LEGAL_ENTITY.target, true);
    //};
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
    this.addClick = function () {
        com.openTab('新增营收', "/mms/ecom/Edit/0");

    };
    this.deleteClick = function () {
        var checkedItems = self.grid.datagrid('getChecked');
        if (!checkedItems) return com.message('warning', self.resx.noneSelect);
        var array = [], arrayQuoteNo = [];
        $.each(checkedItems, function (index, item) {
            array.push(item.EcomID);
            arrayQuoteNo.push(item.SaleNo);
        });
        var str = array.join(',');
        com.message('confirm', self.resx.deleteConfirm + "\r\n" + arrayQuoteNo.join(','), function (b) {
            if (b) {
                post = { value: str };
                com.ajax({                    
                    url: self.urls.remove,
                    data: ko.toJSON(post),
                    success: function (d) {
                        if (d) {
                            com.message('success', self.resx.deleteSuccess);
                            self.searchClick();
                        }
                        else { com.message('error', '刪除出現錯誤！'); }
                    },
                    error: function (e) {
                        com.message('error', '刪除出現錯誤！');
                    }
                });
            }
        });
    };

    this.editClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        { com.openTab('电商营收--' + row.SaleNo, "/mms/ecom/Edit/" + row.EcomID); }

    };
    this.grid.onDblClickRow = this.editClick;
    
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
        return flag;;
    };
};
var openURL = function (data0, data1) { //注意這個方法要寫在最外面
    com.openTab('电商营收--' + data1, "/mms/ecom/Edit/" + data0);
}


mms.Ecom.Edit = function (data) {
    var rows = JSON.parse(localStorage.getItem('keyEcomID'));

    var self = this;

    //常量
    this.tabConst = { grid: 'grid', form: 'form', tab: 'tab', edit: 'gridEdit' };

    //设置
    this.urls = data.urls;                                      //api服务地址
    this.resx = data.resx;                                      //中文资源
    this.form = data.form;
    this.tabs = data.tabs;

    //数据
    this.dataSource = data.dataSource;                          //下拉框的数据源
    this.pageData = ko.mapping.fromJS(data.dataSource.pageData);//页面数据
    this.setting = data.setting;
    delete this.form.__ko_mapping__;
   

    //重新構造scrollKeys數據源    
    this.currentIndex = ko.observable(0);
    this.currentKey = self.pageData.form.EcomID();
    this.keyIndex = function () {
        if (!rows) { self.currentIndex(0); return; }
        $.each(rows, function (i, value) {
            if (self.currentKey == value.key) {
                self.currentIndex(i);
                return;
            };
        });

    };

    this.firstClick = function () {
        self.scrollTo(0);
    };
    this.previousClick = function () {
        self.scrollTo(-1);
    };
    this.nextClick = function () {
        self.scrollTo(1);
    };
    this.lastClick = function () {
        self.scrollTo(9);
    };

    this.firstEnable = ko.computed(function () {
        return self.currentIndex() > 0;
    });

    this.previousEnable = ko.computed(function () {
        return self.currentIndex() >= 1;
    });

    this.nextEnable = ko.computed(function () {
        if (!rows) return false;
        return self.currentIndex() <= rows.length - 2;
    });

    this.lastEnable = ko.computed(function () {
        if (!rows) return false;
        return self.currentIndex() < rows.length - 1;
    });

    this.scrollTo = function (id) {
        if (!rows) { return; }

        if (id == 0) {
            if (self.currentIndex() == 0) return;
            self.currentKey = rows[0].key;
            self.currentIndex(0);
        }
        else if (id == 9) {
            if (self.currentIndex() == rows.length - 1) return;
            self.currentKey = rows[rows.length - 1].key;
            self.currentIndex(rows.length - 1);
        }
        else {
            if ((self.currentIndex() == 0 && id == -1) || (self.currentIndex() == rows.length - 1 && id == 1)) { return; }
            self.currentIndex(self.currentIndex() + id);
            self.currentKey = rows[self.currentIndex()].key;
        };

        com.setLocationHashId(self.currentKey);
        com.ajax({
            type: 'GET',
            url: self.urls.getdata + self.currentKey,
            success: function (d) {
                data.dataSource.pageData = d;
                ko.mapping.fromJS(d, self.pageData);
                self.grid.queryParams({ EcomID: self.currentKey });
            }
        });
    };

    this.refreshClick = function () {
        com.ajax({
            type: 'GET',
            url: self.urls.getdata + self.currentKey,
            success: function (d) {
                data.dataSource.pageData = d;
                ko.mapping.fromJS(d, self.pageData);
                self.grid.queryParams({ EcomID: self.currentKey });
            }
        });
    };
    
    this.grid = {
        pagination: true,
        remoteSort: false,showFooter:true,
        url: self.urls.getEcomDetail,
        queryParams: ko.observable(),
        onLoadSuccess: function (data) {

            var rows = $('#gridlist').datagrid('getRows');
            if (rows.length > 1) {
                var aTotalSum = 0, aLogSum = 0, aComSum=0;
                $.each(rows,function(i,v){
                    aTotalSum += v['TotalSum']; 
                    aLogSum += v['Qty'] * v['LogCost'];
                    aComSum += v['Qty'] * (v['Price'] + v['PKGCost'] + v['OtherCost']);
                });
                $('#gridlist').datagrid('reloadFooter', [{ SNO: -1, Currency: '<label style="color:blue;"><b>Total：</b></label>', TotalSum: aTotalSum, LogSum: '<label style="color:blue;">'+com.formatMoney2(aLogSum)+'</label>', ComSum: '<label style="color:blue;">'+com.formatMoney2(aComSum)+'</label>' }]);
            }
            else {
                $('#gridlist').datagrid({showFooter:false});
            }

        }
    };

    this.grid.queryParams({ EcomID: self.currentKey });

    this.gridEdit = new com.editGridViewModel(self.grid);

    this.addRowClick = function () {        
        self.gridEdit.addnew(self.tabs[0].defaults);
    };

    this.onClickRow = function () { //this.grid.onDblClickRow
        if (self.readonly()) return;
        self.gridEdit.begin();
    };
    this.grid.toolbar = "#gridtb";
   
    this.removeRowClick = function () {
        if (self.readonly()) return;
        self.gridEdit.deleterow();
        //if (self.form.TotalMoney)
        //    mms.com.calcTotalMoneyWhileRemoveRow(self, "Money", "TotalMoney");
    };

    this.EcomFileClick = function () {
        mms.com.upload({ BizID: self.currentKey, BizCode: self.pageData.form.SaleNo(), BizTable: 'DG_EcomHeader' });
    };    

    this.excelClick = function () {
        com.ajax({
            url: self.urls.getExcel + self.pageData.form.EcomID(),
            success: function (w) {
                if (w.Msg.length > 0) {
                    com.message('warning', w.Msg);
                }
                else {
                    com.message('success', '电商月报表生成OK！');
                    if (w.url)
                    { com.openFile(w.url); }
                }
            }
        });
    };
    //保存
    this.saveClick = function () {
        //数据验证
        var validMessage = self.fnIsPageValid();
        if (validMessage) {
            com.message('warning', validMessage);
            return;
        }

        //取得数据
        var post = self.fnIsPageChanged(); 
        if (!post._changed) {
            com.message('success', '页面没有数据被修改！');
            return;
        }       
       
        if (post.form._changed) {   
            post.form = self.pageData.form; post.form._changed = true;
        }

        com.ajax({
            url: self.urls.edit,
            data: ko.toJSON(post),
            success: function (d) {
                if (self.currentKey == '0') { self.currentKey = d.form.EcomID; }
                com.message('success', self.resx.editSuccess);
                data.dataSource.pageData = d;
                ko.mapping.fromJS(d, self.pageData);
                self.grid.queryParams({ EcomID: self.currentKey });
                
            }
        });
    };

    this.fnIsPageChanged = function () {
        var result = { form: {}, tabs: [], _changed: false };

        result.form = com.formChanges(self.pageData.form, data.dataSource.pageData.form, self.form.primaryKeys);
        result._changed = result._changed || result.form._changed;

        var tab = self.tabs[0];
        if (tab.type == self.tabConst.grid) {
            var edit = self[self.tabConst.edit];
            edit.ended();
            tabData = edit.getChanges(tab.postFields);
        }
        else if (tab.type == self.tabConst.form) {
            var name = self.tabConst.tab;
            tabData = com.formChanges(self.pageData[name], data.dataSource.pageData[name], tab.primaryKeys);
        }

        result.tabs.push(tabData);
        result._changed = result._changed || tabData._changed;        

        return result;
    };

    this.fnIsPageValid = function () {
        var formValid = com.formValidate();
        if (!formValid)
        {return '验证不通过，数据未保存！';}

        var tab = self.tabs[0];
        if (tab.type == self.tabConst.grid) {
            var edit = self[self.tabConst.edit];
            if (!edit.ended())
                return '第' + i + '个页签中验证不通过';
        }
        else if (tab.type == self.tabConst.form) {
            //formValid = com.formValidate(document.getElementById("xxx"));
        }        

        return '';
    };




    //初始化tabs
    this.init = function () {
        //tabs

        //for (var i in self.tabs) {
        //    var tab = self.tabs[i];
        //    if (tab.type == self.tabConst.grid) {
        //        var grid = this[self.tabConst.grid + i] = {};

        //        //grid.pagination = true;
        //        //grid.url = self.urls.getorder;
        //        //grid.queryParams = { ReceiveInvoiceId: self.form.idField };

        //        var edit = this[self.tabConst.edit + i] = new com.editGridViewModel(grid);
        //        $.extend(true, grid, new self.fnEditGrid(tab, grid, edit, i));
        //    }
        //    else if (tab.type == self.tabConst.form) {
        //        if (self.fnIsNew()) //主表新增時清空所有綁定欄位
        //            self.pageData[self.tabConst.tab + i] = ko.mapping.fromJS(tab.defaults);
        //    }

        //}
      

        //this.grid0.url = self.urls.getorder;
        //this.grid0.fit = true;
        //this.grid0.pagination = true;
        //this.grid0.pageSize = 10;
        ////this.grid0.size = { w: 6, h: 10 };
        //this.grid0.queryParams = ko.observable();
        //this.grid0.queryParams(self.pageData.form);

        self.keyIndex();
        //pageData
        if (self.fnIsNew()) {
            self.pageData.form = ko.mapping.fromJS(self.form.defaults);
        }
    };

    //取得grid参数对象
    this.fnEditGrid = function (tab, grid, edit, i) {
        //this.size = { w: 6, h: 180 };
        //this.remoteSort = false;

        this.data = self.pageData["tab" + i];//注意此时才给grid绑定数据
        this.onClickRow = function () {
            //if (self.readonly()) return;
            edit.begin();
        };
        this.toolbar = "#gridtb" + i;
        

        this.removeRowClick = function () {
            edit.deleterow();
            //mms.com.calcTotalMoneyWhileRemoveRowForMoreGrid(self.pageData, grid, ['Cost', 'PreInvoiceNoTax', 'PreInvoiceTax'], ['ReceiveCost', 'PreInvoiceNoTax', "PreInvoiceTax"]);
        };
        //this.OnAfterCreateEditor = function (editors, row) {
        //    if (!row._isnew) {
        //        com.readOnlyHandler('input')(editors.PaymentInvNo.target, true);
        //        com.readOnlyHandler('input')(editors.PreInvoiceTax.target, true);
        //    }
        //};

    };

    this.fnIsNew = function () { return data.dataSource.pageData.form == null; };
    this.init();
    this.readonly = ko.computed(function () {
        return false;
        //return self.pageData.form.ShipStatus() != '录入';
    });//data.dataSource.pageData.form.CFMFlag == 'N';
    this.canShow = function (item) {
        var buttons = self.dataSource.buttonsList;
        var flag = false;

        for (var k = 0; k < buttons.length; k++) {
            if (buttons[k].ButtonIcon == item) {
                flag = true; break;
            }
        };
        return (flag == false) ? self.readonly() : flag;;
    };
};

var getPrice = function (cb) {
    if (event.type == 'click') {
        var row = $('#gridlist').datagrid('getSelected');
        var rowIndex = $('#gridlist').datagrid('getRowIndex', row);//第二步:根據選中行取得行索引
        var editors = $('#gridlist').datagrid('getEditors', rowIndex); //第三步:取得該行所有欄位的editors        
      
        post = { pnid: cb.value };
        com.ajax({
            async: false,   
            url: data.urls.getEPNPrice,  
            data: ko.toJSON(post),  
            success: function (d) {
                $(editors[2].target).numberbox('setValue', d.Price); //注意这里不能写成简单赋值:$(editors[2].target).val(d.Price),否则后面save时无法获取新值
                $(editors[4].target).numberbox('setValue',d.PKGCost); 
            },
            error: function (e) {
                com.message('error', e.responseText);
            }
        });        
    }
}