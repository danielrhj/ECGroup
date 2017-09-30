var Sys = Sys || {};
var urls = "",editUrls=""; 

Sys.Request = function (data) {
    var self = this;
    this.urls = data.urls; urls = data.urls;
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
        pagination: true
    };

    this.grid.queryParams(data.form);

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
        com.openTab('新增需求單', "/Sys/Request/Edit/");
    };
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        com.message('confirm', "确定要删除吗？", function (b) {
            if (b) {
                post = { value: escape(row[self.idField]) };
                com.ajax({
                    data: ko.toJSON(post),
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
        com.openTab('需求明細資料', self.urls.edit + row[self.idField]);

    };
    this.grid.onDblClickRow = this.editClick;
    
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

    this.downloadClick = function (vm, event) {
        com.exporter(self.grid).download($(event.currentTarget).attr("suffix"));
    };
}

Sys.RequestEdit = function (data) {
    var self = this;
    //常量
    this.tabConst = { grid: 'grid', form: 'form',  edit: 'gridEdit' };

    //设置
    this.urls = data.urls; editUrls = data.urls;                                   //api服务地址
    this.resx = data.resx;                                      //中文资源
    this.form = ko.mapping.fromJS(data.form);
    this.tabs = data.tabs;
    this.setting = data.setting;

    //数据
    this.dataSource = data.dataSource;                          //下拉框的数据源
    this.pageData = ko.mapping.fromJS(data.dataSource.pageData);//页面数据
    delete this.form.__ko_mapping__;

    this.fnIsPageChanged = function () {
        var result = { form: {}, _changed: false };

        result.form = com.formChanges(self.pageData.form, data.dataSource.pageData.form, self.form.primaryKeys);
        result._changed = result._changed || result.form._changed;

        return result;
    };

    this.refreshClick = function () {
        window.location.reload();
    };

    this.grid = {
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: false,
        customLoad: false
    };

    this.grid.queryParams(this.pageData.form);

    //保存
    this.saveClick = function () {
        //取得数据
        var post = self.fnIsPageChanged(); 
        if (!post._changed) {
            com.message('success', '页面没有数据被修改！');
            return;
        }

        if (post.form._changed) { post.form = self.pageData.form; post.form._changed = true; }

        com.ajax({
            url: self.urls.edit,
            data: ko.toJSON(post),
            success: function (d) {
                com.message('success', self.resx.editSuccess);
            }
        });
    };

    this.auditClick = function () {
        com.dialog({
            title: "&nbsp;需求單審核",
            iconCls: 'icon-transmit',
            width: 450,
            height: 250,
            html: "#audit-template",
            viewModel: function (w) {
                var that = this;
                that.form = self.pageData.form;

                var signAction = function (action) {
                    var url = '', msg = '';
                    var post = { AutoId: that.form.AutoID(), ITComments: that.form.ITComments(), ActionName: action };
                    if (action == 'accept') {
                        msg = '簽核成功！';
                    }
                    if (action == 'reject') {
                        msg = '駁回成功！';
                    }
                    if (action == 'close') {
                        msg = '結案成功！';
                    }

                    com.ajax({
                        data: ko.toJSON(post),
                        url: self.urls.sign,
                        success: function (s) {
                            com.message('success', msg);
                            w.dialog('close'); self.refreshClick();
                        },
                        error: function (e) {
                            com.message('error', e.responseText);
                        }
                    });
                };

                this.approveClick = function () {
                    return signAction('accept');
                };

                this.rejectClick = function () {
                    return signAction('reject');
                };

                this.closeClick = function () {
                    return signAction('close');
                };
            }
        });
    };    
   
    //初始化tabs
    this.init = function () {

        //pageData
        if (self.fnIsNew()) {
            self.pageData.form = ko.mapping.fromJS(self.form.defaults);
        }        
    };
    this.fnIsNew = function () { return data.dataSource.pageData.form == null; };
    this.init();
    this.fnIsSuperAdmin = ko.computed(function () {
        var kk = self.setting.role == "superadmin" ? false : true;
        return kk;
    });

    this.canShow = function (item) {
        var buttons = self.dataSource.buttonsList;
        var flag = false;

        for (var k = 0; k < buttons.length; k++) {
            if (buttons[k].ButtonIcon == item) {
                flag = true; break;
            }
        };

        if (item == "icon-edit" && (self.pageData.form.AutoID()=="0")) {
            return true;
        }
        return flag;
    };
    
}

var openURL = function (data) {
    com.openTab('需求明細資料', urls.edit + data);
}

var delFile = function (data,file) {
    com.ajax({
        type: 'DELETE',
        data: ko.toJSON({autoID:data,fileid:file}),
        url: editUrls.remove + data,
        success: function (d) {
            if (d.msg)
            { com.message('warning', d.msg); }
            else {
                com.message('success', '刪除成功.');
                $("#gridlist").datagrid('reload');
            }
        }
    });
}


var openUserInfo = function (data) {

    com.dialog({
        title: "&nbsp;用戶信息",
        iconCls: 'icon-node_tree',
        width: 330,
        height: 280,
        html: "#UserInfo-template",
        viewModel: function (w) {
            var that = this;
            this.grid = {
                width: 300,
                height: 200,
                url: self.urls.userinfo,
                queryParams: ko.observable({ key: data })
            };
        }
    });
};