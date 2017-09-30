/// <reference path="index.js" />
/**
* 模块名：mms viewModel
* 程序名: mms.supplierEdit.js
**/
var mms = mms || {};
var rows = JSON.parse(localStorage.getItem('keySuplier'));

mms.supplierEdit = function (data) {
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
    
    //重新構造scrollKeys數據源    
    this.currentIndex = ko.observable(0);
    this.currentKey = self.pageData.form.SuppID();
    this.keyIndex = function () {
        if (!rows) { self.currentIndex(0); return; }
        $.each(rows, function (i, value) {
            if (self.currentKey == value.key) {
                self.currentIndex(i);                
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
            self.currentKey=rows[0].key;
            self.currentIndex(0);
        }
        else if (id == 9) {
            if (self.currentIndex() == rows.length - 1) return;
            self.currentKey=rows[rows.length - 1].key;
            self.currentIndex(rows.length - 1);
        }
        else {
            if ((self.currentIndex() == 0 && id == -1) || (self.currentIndex() == rows.length - 1 && id == 1)) { return; }
            self.currentIndex(self.currentIndex()+id);
            self.currentKey = rows[self.currentIndex()].key;
        };

        com.setLocationHashId(self.currentKey);
        com.ajax({
            type: 'GET',
            url: self.urls.getdata + self.currentKey,
            success: function (d) {
                data.dataSource.pageData = d;
                ko.mapping.fromJS(d, self.pageData);
            }
        });
    };
  
    //撤销
    this.rejectClick = function () {
        ko.mapping.fromJS(data.dataSource.pageData, {}, self.pageData);
        com.message('success', self.resx.rejected);        
    };   

    //保存
    this.saveClick = function () {
        //数据验证
        //alert(self.pageData.form.IS_RELATED_COMPANY());
        //alert(self.IS_RELATED_COMPANY());
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

        if (post.form._changed) { post.form = self.pageData.form; post.form._changed = true; }  //為了使用SP，当单头資料有改動時要將所有欄位資料發送到後台處理

        //数据提交,保存時執行APIController的方法
        com.ajax({
            url: self.urls.edit,
            data: ko.toJSON(post),
            success: function (d) {
                com.message('success', self.resx.editSuccess);
                if (post.form.SuppID() == '0')
                    self.pageData.form.SuppCode(d.form.SuppCode);
                self.pageData.form.SuppID(d.form.SuppID);
            }
        });
    };

    this.fnIsPageChanged = function () {
        var result = { form: {}, tabs: [], _changed: false };

        result.form = com.formChanges(self.pageData.form, data.dataSource.pageData.form, self.form.primaryKeys);
        result._changed = result._changed || result.form._changed;

        for (var i in self.tabs) {
            var tab = self.tabs[i], tabData;
            if (tab.type == self.tabConst.grid) {
                var edit = self[self.tabConst.edit + i];
                edit.ended();
                tabData = edit.getChanges(tab.postFields);
            }
            else if (tab.type == self.tabConst.form) {
                var name = self.tabConst.tab + i;
                tabData = com.formChanges(self.pageData[name], data.dataSource.pageData[name], tab.primaryKeys);
            }
            result.tabs.push(tabData);
            result._changed = result._changed || tabData._changed;
        }

        return result;
    };

    this.fnIsPageValid = function () {

        //var kk = com.formValidate(document.getElementById("txtFullName"));

        var formValid = com.formValidate();
        if (!formValid)
        { return '验证不通过，数据未保存！'; }
            // LEGAL_ENTITY,ERP_CODE,LEGAL_FULL,IS_RELATED_COMPANY,CURRENCY,PayerType,SystemType
        
        //else {
        //    var myform = self.pageData.form;
        //    //var dsIR = self.dataSource.ISRelatedList;            

        //    //console.log(self.dataSource.ISRelatedList); //alert(myform.CURRENCY());
        //    if (!myform.LEGAL_ENTITY())
        //    { return 'ideas編碼不能為空！'; }
        //    else if (!myform.ERP_CODE())
        //    { return '付款法人不能為空！'; }           
        //    else if (!myform.CURRENCY())
        //    {                
        //        return '付款幣種不能為空！';
        //    }

        //    else if (myform.PayerType()!= '內部客戶' && myform.PayerType() != '外部客戶' && myform.PayerType() != '準時達法人') {
        //        return '請選擇正確的客戶類型!';
        //    }
        //    else if (myform.PayerType() == '內部客戶' && (!myform.LEGAL_CODE() || !myform.COSTCODE() || !myform.CUSTOMER_CODE() || !myform.LEGAL_TYPE())) {
        //        return '內部客戶必須填寫法人檔、費用代碼、客戶代碼、中文簡稱!';
        //    }
        //    else if (myform.PayerType() == '外部客戶' && (!myform.CUSTOMER_CODE())) {
        //        return '外部客戶必須填寫客戶代碼!';
        //    }
        //    else if (myform.PayerType()== '準時達法人' && (!myform.VendorCode))
        //    { return '準時達法人必須填寫-Jusda收款代碼!'; }
        //    else if (myform.PayerType()== '準時達法人' && (!myform.LEGAL_TYPE))
        //    { return '準時達法人必須填寫中文簡稱!'; }
            
        //    else if (myform.SystemType().toUpperCase() != 'TIPTOP' && myform.SystemType().toUpperCase() != 'SAP')
        //    { return '系統類型只能選TIPTOP/SAP！'; }
        //    else
        //    {                
        //        var msg = com.myValid(self.dataSource.ISRelatedList, myform.IS_RELATED_COMPANY(), { required: true, msgTip: '內部客戶' });
        //        if (msg)
        //        { return msg; }
        //        else {
        //            return '';
        //        }
        //    }
        //}

        for (var i in self.tabs) {
            var tab = self.tabs[i], tabData;
            if (tab.type == self.tabConst.grid) {
                var edit = self[self.tabConst.edit + i];
                if (!edit.ended())
                    return '第' + i + '个页签中验证不通过';
            }
        }
        return '';
    };


    //审核
    //this.auditClick = function () {
    //    var changes = self.fnIsPageChanged();
    //    if (changes._changed) {
    //        com.message('warning', '数据有修改，请保存后再审核！');
    //        return;
    //    }

    //    com.auditDialogForEditVM(self.pageData.form, function (d) {
    //        com.ajax({
    //            type: 'POST',
    //            url: self.urls.audit + self.pageData.scrollKeys.current(),
    //            data: JSON.stringify(d),
    //            success: function () {
    //                com.message('success', d.status == "passed" ? self.resx.auditPassed : self.resx.auditReject);
    //                ko.mapping.fromJS(self.pageData.form, {}, data.dataSource.pageData.form);
    //            },
    //            error: function (e) {
    //                com.message('error', e.responseText);
    //                ko.mapping.fromJS(data.dataSource.pageData.form, {}, self.pageData.form);
    //            }
    //        });
    //    });
    //};

    //打印
    //this.printClick = function () {
    //    com.openTab('打印报表', '/report?area=mms&rpt=' + self.urls.report + '&BillNo=' + self.form.data.BillNo(), 'icon-printer_color');
    //};

    //初始化tabs
    this.init = function () {
        //tabs
        for (var i in self.tabs) {
            var tab = self.tabs[i];
            if (tab.type == self.tabConst.grid) {
                var grid = this[self.tabConst.grid + i] = {};
                var edit = this[self.tabConst.edit + i] = new com.editGridViewModel(grid);
                $.extend(true, grid, new self.fnEditGrid(tab, grid, edit, i));
            }
            else if (tab.type == self.tabConst.form) {
                if (self.fnIsNew()) //主表新增時清空所有綁定欄位
                    self.pageData[self.tabConst.tab + i] = ko.mapping.fromJS(tab.defaults);
            }

        }
        self.keyIndex();

        //pageData
        if (self.fnIsNew()) {
            self.pageData.form = ko.mapping.fromJS(self.form.defaults);
        }
    };

    //取得grid参数对象
    this.fnEditGrid = function (tab, grid, edit, i) {
        this.size = { w: 6, h: 177 };
        this.pagination = false;
        this.remoteSort = false;
        this.data = self.pageData["tab" + i];//注意此时才给grid绑定数据
        this.onClickRow = function () {
            edit.begin();
        };
        this.toolbar = "#gridtb" + i;
        this.addRowClick = function () {
            var row = $.extend(true, {}, tab.defaults);
            row[tab.rowId] = 0;
            row[tab.relationId] = self.pageData.form.idField;
            edit.addnew(row);
        };
        this.removeRowClick = function () {
            edit.deleterow();
        };
    };

    this.fnIsNew = function () { return data.dataSource.pageData.SuppID == 0; };
    this.init();
    //this.fnIsAudit = function () { return self.pageData.form["ApproveState"]() == "passed"; };
    //this.readonly = ko.computed(function () { return self.pageData.form.PayerType() == "準時達法人" ? false : true; });

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


    //var updateCombox = function (newValue, oldValue) {
    //    var kk = newValue;
    //};