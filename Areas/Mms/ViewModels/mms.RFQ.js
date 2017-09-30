/**
* 模块名：mms viewModel
* 程序名: mms.payee.js
**/
var mms = mms || {};
mms.RFQ = mms.RFQ || {};

mms.RFQ.Search = function (data) {
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
        treeField: 'RFQNo',
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        singleSelect:false,
        //pagination: true,
        onLoadSuccess: function (row,data) {
            localStorage.removeItem('keyRFQID');
            var keyid = [];//重整key
            $.each(data, function (i, v) {
                keyid.push({ key: v.RFQID.toString() });
            });

            localStorage.setItem('keyRFQID', JSON.stringify(keyid));
        },
        loadFilter: function (d) {
            d = utils.copyProperty(d.rows || d, ["RFQNo", "IconClass"], ["_id", "iconCls"], false);
            return utils.toTreeData(d, '_id', 'ParentCode', "children");
        },
        rowStyler: function (row) {
            if (isNaN(row.RFQNo)) {
                return 'background-color:#CAFFFF;color:blue;font-weight:bold;';
            }
        }
        
    };

    this.grid.queryParams(self.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤

    //this.gridEdit = new com.editGridViewModel(this.grid);
    //this.grid.onDblClickRow = this.gridEdit.begin;
    //this.grid.onClickRow = this.gridEdit.ended;
    //this.grid.OnAfterCreateEditor = function (editors, row) {
    //    if (!row._isnew) com.readOnlyHandler('input')(editors.LEGAL_ENTITY.target, true);
    //};
    this.searchClick = function () {
        //com.setFirstPageWhenSearchGrid(self.grid);
        var param = ko.toJS(this.form);
        this.grid.queryParams(param);
    };
    this.clearClick = function () {
        $.each(self.form, function () { this(''); });
        this.searchClick();
    };

    this.CreatReceiveOrderClick= function () {
        var checkedItems = this.grid.datagrid('getChecked');
        if (checkedItems.length == 0) { com.message('warning', self.resx.noneSelect); return; }
        if (checkedItems.length > 1) { com.message('warning', self.resx.moreSelect); return; }

        var oma043 = checkedItems[0].oma043;
        post = { key: checkedItems[0].oma01 };
        com.ajax({
            url: self.urls.createEal,
            data: ko.toJSON(post),
            success: function (d) {
                if (d.length == 0) {
                    com.openTab(oma043 + '預結報明細', '/mms/apeal/edit/' + oma043);
                    com.message("success", "轉預結報成功");
                }
                else
                    com.message("success", "轉預結報失敗:" + d);
            },
            error: function (e) {
                com.message('error', e.responseText);
            }
        });
    };

    this.refreshClick = function () {
        window.location.reload();
    };
    this.addClick = function () {    
        com.openTab('新增代理商报价单', "/mms/rfq/Edit/0");
    };
    this.deleteClick = function () {
        var checkedItems = self.grid.treegrid('getSelections');
        if (!checkedItems) return com.message('warning', self.resx.noneSelect);
        var array = [], arrayQuoteNo = [];
        $.each(checkedItems, function (index, item) {
            if (array.indexOf(item.RFQID)==-1) array.push(item.RFQID);    //BuyID
            if (arrayQuoteNo.indexOf(item.RFQNo) == -1&&item.ParentCode=='0') arrayQuoteNo.push(item.RFQNo);
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
                            self.grid.queryParams(self.form);
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
        var row = self.grid.treegrid('getSelected');
        if (!row) { return com.message('warning', self.resx.noneSelect); }
        else if(row.ParentCode=='0')
        {
            com.openTab('代理商报价--' + row.RFQNo, "/mms/rfq/Edit/" + row.RFQID);
        }

    };
    this.grid.onDblClickRow = this.editClick;
  
    this.downloadClick = function (vm, event) {
        var parm = $.extend({}, self.form, { type: 'excel' });//写this.form也可以        
        com.ajax({
            data: ko.toJSON(parm),
            url: self.urls.excel,
            success: function (w) {
                if (w.Msg.length > 0) {
                    com.message('warning', w.Msg);
                }
                else {
                    com.message('success', '代理商报价导出OK！');
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
var openURL = function (data0, data1) { //注意這個方法要寫在最外面
    com.openTab('代理商报价单--' + data1, "/mms/rfq/Edit/" + data0);
}


mms.RFQ.Edit = function (data) {
    var rows = JSON.parse(localStorage.getItem('keyRFQID'));

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
    this.currentKey = self.pageData.form.RFQID();
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
                ko.mapping.fromJS(d, self.pageData); self.grid.queryParams({ RFQID: self.currentKey });
            }
        });
    };

    this.refreshClick = function () {
        window.location.reload();
    };
    
    this.grid = {
        pagination: true,
        remoteSort: false,
        showFooter:true,
        url: self.urls.getRFQDetail,
        queryParams:ko.observable()
    };

    this.grid.queryParams({ RFQID: self.currentKey });

    this.gridEdit = new com.editGridViewModel(self.grid);

    this.addRowClick = function () {
        if (self.readonly()) return;
        var param = { LookupType: 'LookupSuppPNForRFQ',SuppCode: self.pageData.form.SuppCode() };
        mms.com.selectMaterial(self, param);
    };

    this.gridEdit = new com.editGridViewModel(self.grid);
    this.grid.onClickRow = function () { //this.grid.onDblClickRow
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

    this.searchSupplierClick = function () {
        var suppCode = self.pageData.form.SuppCode();
        var suppAbbr = self.pageData.form.SuppAbbr();
        mms.com.selectSupplier(self, { SuppCode: suppCode, SuppAbbr: suppAbbr, type: 'RFQ' });
    };

    this.RFQFileClick = function () {
        mms.com.upload({ BizID: self.currentKey, BizCode: self.pageData.form.RFQNo(), BizTable: 'DG_RFQHeader' });
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
        var post = self.fnIsPageChanged(); //注意此时的post包含了单头和下面所有tabs修改的内容,所以下面的tab没有save按钮
        if (!post._changed) {
            com.message('success', '页面没有数据被修改！');
            return;
        }

        //检查inserted和updated的无效值:BuyPrice>0,Qty>0,Unit!=''
        var flag = true;
        $.each(post.tabs[0].inserted.concat(post.tabs[0].updated), function (i, item) {
            if (item.BuyPrice == 0 || item.Qty == 0 || item.Unit == '') {
                com.message('error', '采购单价,数量和单位无效！');
                flag = false;
                return false;
            }
        });

        if(!flag){return;}
      
        if (post.form._changed) {   //為了使用SP，当单头資料有改動時要將所有欄位資料發送到後台處理            
            post.form = self.pageData.form; post.form._changed = true;
        }

        com.ajax({
            url: self.urls.edit,
            data: ko.toJSON(post),
            success: function (d) {
                if (self.currentKey == '0') { self.currentKey = d.form.RFQID;  }
                com.message('success', self.resx.editSuccess);
                data.dataSource.pageData = d;
                ko.mapping.fromJS(d, self.pageData);

                //for (var i in self.tabs)
                //    var k = i;
                //k = i == '0' ? "" : k;
                //    if (self.tabs[i].type == self.tabConst.grid)
                //        self[self.tabConst.edit + k].accept();
                //    else if (self.tabs[i].type == self.tabConst.form)
                //        ko.mapping.fromJS(self.pageData[[self.tabConst.tab + k]], {}, data.dataSource.pageData[self.tabConst.tab + k]);
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
    this.readonly = ko.computed(function () { return data.dataSource.pageData.form.CFMFlag == 'Y'; });//data.dataSource.pageData.form.CFMFlag == 'N';
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
