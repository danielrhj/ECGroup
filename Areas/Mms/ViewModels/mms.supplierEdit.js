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
                self.grid0.datagrid('options').queryParams = { action: 'getSuppPNListBySuppCode', SuppCode: self.pageData.form.SuppCode() };   //不能写self.grid0.queryParams={};
                self.grid0.datagrid('reload');
            }
        });
    };
  
    this.refreshClick = function () {
        window.location.reload();
    };

    //保存
    this.saveClick = function () {
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
                if (post.form.SuppID == '0') {
                    self.pageData.form.SuppCode(d.form.SuppCode);
                    self.pageData.form.SuppID(d.form.SuppID);
                }
                
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
        //this.size = { w: 6, h: 177 };
        this.pagination = true;
        this.remoteSort = false; this.url = self.urls.query; this.queryParams = { action: 'getSuppPNListBySuppCode', suppCode: self.pageData.form.SuppCode() };
        this.data = self.pageData["tab" + i];//注意此时才给grid绑定数据
        this.onClickRow = function () {
            edit.begin();
        };
        this.toolbar = "#gridtb" + i;
        this.addRowClick = function () {
            var row = $.extend(true, {}, tab.defaults);
            row[tab.rowId] = 0;
            row[tab.relationId] = self.pageData.form.SuppCode();
            row.SupplierCode = self.pageData.form.SuppCode();
            row.SuppAbbr = self.pageData.form.SuppAbbr();
            edit.addnew(row);
        };
        this.removeRowClick = function () {
            edit.deleterow();
        };

        this.OnAfterCreateEditor = function (editors, row) {
            com.readOnlyHandler('input')(editors["SupplierCode"].target, true);
            com.readOnlyHandler('input')(editors["SuppAbbr"].target, true);
            com.readOnlyHandler('input')(editors["CDesc"].target, true);
            com.readOnlyHandler('input')(editors["CSpec"].target, true);
            com.readOnlyHandler('input')(editors["TypeName"].target, true);
            com.readOnlyHandler('input')(editors["Brand"].target, true);
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


var updatePNInfo = function (d) {
    var pnid = d.value;
    var row = $('#gridlist').datagrid('getSelections')[0];  //第一步:取得選中行,也就是編輯行
    var rowIndex = $('#gridlist').datagrid('getRowIndex', row);//第二步:根據選中行取得行索引
    var editors = $('#gridlist').datagrid('getEditors', rowIndex);

    com.ajax({
        url: '/api/mms/partno/getPNInfoByPNID', 
        data: ko.toJSON({pnid:pnid}), 
        success: function (info) {
            if (info) {
                $(editors[3].target).val(info.CDesc);
                $(editors[4].target).val(info.CSpec);
                $(editors[5].target).val(info.Brand);
                $(editors[6].target).val(info.TypeName);
            }
        },
        error: function (e) {
            com.message('error', e.responseText);
        }
    });
};
