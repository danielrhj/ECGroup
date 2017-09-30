/**
* 模块名：mms viewModel
* 程序名: mms.payee.js
**/
var mms = mms || {};
mms.Shiping = mms.Shiping || {};

mms.Shiping.Search = function (data) {
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
        singleSelect: true, autoRowHeight: false,
        pagination: true,showFooter:true,
        onLoadSuccess: function (data) {
            localStorage.removeItem('keyShipID');
            localStorage.setItem('keyShipID', JSON.stringify(data.keyRows));
            
            for (var i = 0; i < data.rows.length; i++) {
                //根据ck的值让某些行不可选,ck=ISNULL(omaconf,'N')--经管確認
                if (data.rows[i].ck == "Y" || data.rows[i].AccountStatus == "已生成預結報") {
                    $("input[type='checkbox']")[i + 1].disabled = true;
                }
            }

            var rows = $('#gridlist').datagrid('getRows');
            if (rows.length > 1) {
                var atotal = 0, aqty = 0;
                for (var i = 0; i < rows.length; i++) {
                    atotal += rows[i]['Amount'];
                }
                $('#gridlist').datagrid('reloadFooter', [{ ShipNo: '', CSpec: '<label style="color:blue;"><b>Total：</b></label>', Amount: atotal, Currency: rows[0]['Currency'] }]);
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
                        if (el.value == "Y") { el.checked = false; }
                    });
                }
                else {
                    $('input:checkbox[name="ck"]').each(function (index, el) {
                        el.checked = false;
                    });

                }
            });
            com.mergeGrid(self.grid, ['ShipNo']);
        },
        onClickRow: function (rowIndex, rowData) {      //注意這兩個事件執行的先後順序：先執行Select,再執行ClickRow
            //加载完毕后获取所有的checkbox遍历
            $("input[type='checkbox']").each(function (index, el) {
                //如果当前的复选框不可选，则不让其选中
                if (el.disabled == true) {
                    $("#gridlist").datagrid('unselectRow', index - 1);
                }
            })
        },
        onSelect: function (rowIndex, rowData) {
            $("input[type='checkbox']").each(function (index, el) {
                //如果当前的复选框不可选，则不让其选中
                if (el.disabled == true) {
                    $("#gridlist").datagrid('unselectRow', index - 1);
                }
            })
        },
        rowStyler: function (index, row) {
            if (row.ShipStatus == '录入') {
                return 'background-color:#CAFFFF;color:blue;font-weight:bold;';
            }
        }
    };

    this.grid.queryParams(data.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤
      
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
        com.openTab('新增出库单', "/mms/shiping/Edit/0");
    };
    this.deleteClick = function () {
        var checkedItems = self.grid.datagrid('getChecked');
        if (!checkedItems) return com.message('warning', self.resx.noneSelect);
        var array = [], arrayQuoteNo = [];
        $.each(checkedItems, function (index, item) {
            array.push(item.ShipID);    
            arrayQuoteNo.push(item.ShipNo);
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
        { com.openTab('出库明细--' + row.ShipNo, "/mms/shiping/Edit/" + row.ShipID); }

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
        com.ajax({
            data: ko.toJSON(self.form),
            url: self.urls.excel,
            success: function (w) {
                if (w.Msg.length > 0) {
                    com.message('warning', w.Msg);
                }
                else {
                    com.message('success', '出货清单导出OK！');
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
    com.openTab('出库明细--' + data1, "/mms/shiping/Edit/" + data0);
}


mms.Shiping.Edit = function (data) {
    var rows = JSON.parse(localStorage.getItem('keyShipID'))||[];

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
     
    this.updateInventory = function (data, event) {
        if (event.type == 'load' || self.currentKey == 0) { return false; }

        var oldValue = data.dataSource.pageData.form.ShipStatus;
        var newValue = data.pageData.form.ShipStatus();
        var action = '',Msg='';

        if (self.currentKey>0)
        {
            if (oldValue == '录入' && newValue == '确认')
            { action = 'ConfirmShiping'; Msg = '扣减'; }
            else if (oldValue == '确认' && newValue == '取消')
            { action = 'CancelShiping'; Msg = '还原'; }
            else { action = ''; }

            if (action) {
                com.message('confirm', '您確定要'+newValue+'出货嗎?此操作将立即改变库存!', function (b) {
                    if (b) {
                        var post = { ShipID: self.currentKey, Action: action };
                        com.ajax({
                            data: ko.toJSON(post),
                            url: self.urls.updateInventory,
                            success: function (d) {
                                if (d.error)
                                { com.message('error', d.error); return false; }
                                else
                                {
                                    com.message('success', '库存' + Msg + '成功');
                                }
                            }
                        });
                    }
                });
            }
        }
    };

    //重新構造scrollKeys數據源    
    this.currentIndex = ko.observable(0);
    this.currentKey = self.pageData.form.ShipID();
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
                self.grid.queryParams({ ShipID: self.currentKey });
            }
        });
    };

    this.refreshClick = function () {
        //window.location.reload();
    };
    
    this.grid = {
        pagination: true,
        remoteSort: false,showFooter:true,
        url: self.urls.getShipDetail,
        queryParams: ko.observable(),
        onLoadSuccess: function (data) {
            var rows = $('#gridlist').datagrid('getRows');
            if (rows.length > 1) {
                var atotal = 0, aqty = 0;
                for (var i = 0; i < rows.length; i++) {
                    atotal += rows[i]['Amount']; aqty += rows[i]['Qty'];
                }
                $('#gridlist').datagrid('reloadFooter', [{ SNO: -1, CSpec: '<label style="color:blue;"><b>Total：</b></label>', Amount: atotal, Qty: aqty }]);
            }
        }
    };

    this.grid.queryParams({ ShipID: self.currentKey });

    this.gridEdit = new com.editGridViewModel(self.grid);

    this.addRowClick = function () {
        if (self.readonly()) return;
        var param = { LookupType: 'LookupCustPNForShiping',CustCode: self.pageData.form.CustCode() };
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

    this.ShipFileClick = function () {
        mms.com.upload({ BizID: self.currentKey, BizCode: self.pageData.form.ShipNo(), BizTable: 'DG_ShipHeader' });
    };

    this.searchCustomerClick = function () {
        var CustCode = self.pageData.form.CustCode();
        var CustAbbr = self.pageData.form.Customer();
        mms.com.selectSupplier(self, { SuppCode: CustCode, SuppAbbr: CustAbbr, type: 'Shiping' });
    };    

    this.CreatHubOutClick = function () {
        com.ajax({
            url: self.urls.createShipingExcel + self.currentKey,
            success: function (w) {
                if (w.Msg.length > 0) {
                    com.message('warning', w.Msg);
                }
                else {
                    com.message('success', '出货单生成OK！');
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

        //检查inserted和updated的无效值:BuyPrice>0,Qty>0,Unit!=''
        var flag = true;
        $.each(post.tabs[0].inserted.concat(post.tabs[0].updated), function (i, item) {
            if (item.UnitPrice == 0 || item.Qty == 0 || item.Unit == '') {
                com.message('error', '销售单价,数量和单位无效！');
                flag = false;
                return false;
            }
        });

        if(!flag){return;}
      
        if (post.form._changed) {   
            post.form = self.pageData.form; post.form._changed = true;
        }

        com.ajax({
            url: self.urls.edit,
            data: ko.toJSON(post),
            success: function (d) {
                if (self.currentKey == '0') { self.currentKey = d.form.ShipID;rows.push({ key: d.form.ShipID }); self.keyIndex(); } 
                com.message('success', self.resx.editSuccess);
                data.dataSource.pageData = d;
                ko.mapping.fromJS(d, self.pageData);
                self.grid.queryParams({ ShipID: self.currentKey });
                
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
        return self.pageData.form.ShipStatus() != '录入';
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