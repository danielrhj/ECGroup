var mms = mms || {};

mms.Company = function (data) {
    var self = this;
    this.urls = data.urls;
    this.resx = data.resx;
    this.idField = data.idField
    this.dataSource = data.dataSource;
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    this.form = ko.mapping.fromJS(data.form);
    delete this.form.__ko_mapping__;

    this.grid = {
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true, pageSize: 10,
        onLoadSuccess: function (data) {
            localStorage.removeItem('keyOwner');
            localStorage.setItem('keyOwner', JSON.stringify(data.keyRows));
        }
    };

    this.grid.queryParams(data.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤

    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onDblClickRow = this.gridEdit.begin;
    this.grid.onClickRow = this.gridEdit.ended;

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
        com.openTab('新增公司資料', "/mms/company/Edit/");
    };
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        com.message('confirm', "确定要删除吗？", function (b) {
            if (b) {
                com.ajax({
                    type: 'DELETE',
                    url: self.urls.remove + row[self.idField],
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
        //self.gridEdit.begin()
        com.openTab('公司明细資料', "/mms/company/Edit/"+row.OwnerID);

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
                    com.message('success', '公司信息表生成OK！');
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
        return flag;
    };
}


mms.companyEdit = function (data) {
    var rows = JSON.parse(localStorage.getItem('keyOwner'));

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
    this.currentKey = self.pageData.form.OwnerID();
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
                if (post.form.AutoID() == '0')                    
                self.pageData.form.AutoID(d.form.AutoID);
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
    this.readonly = ko.computed(function () {
        return self.pageData.form.AutoID() == "0" ? false : true;
    });

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