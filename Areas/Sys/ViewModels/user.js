/**
* 模块名：mms viewModel
* 程序名: SearchEdit.js
* Copyright(c) 2013-2015 liuhuisheng [ liuhuisheng.xm@gmail.com ] 
**/
//var user = user || {};
//mms.viewModel = mms.viewModel || {};

user = function (data) {
    var self = this;
    this.urls = data.urls;
    this.resx = data.resx;
    this.roleid = data.roleid;
    this.dataSource = data.dataSource;
    this.form = ko.mapping.fromJS(data.form);
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    delete this.form.__ko_mapping__;

    this.grid = {
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true,
        onLoadSuccess: function (data) {
            if (self.roleid != 'SuperAdmin')
                $("#gridlist").datagrid('hideColumn', 'Permit');
        }        
    };

    this.grid.queryParams(data.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤

    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onDblClickRow = this.gridEdit.begin;
    this.grid.onClickRow = this.gridEdit.ended;
    this.grid.OnAfterCreateEditor = function (editors,row) {
        if (!row._isnew) com.readOnlyHandler('input')(editors.UserAccount.target, true);
        //var Role = editors.Role.target;
        //$(Role).autocomplete('/api/sys/user/GetRoleListForInsert',
        //    {
        //        extraParams: { ua: function () { return row.UserAccount; } }
        //    }
        //);
        
    };

    this.searchClick = function () {
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
                    //$.messager.alert('Save', self.resx.editSuccess, 'info');萬一不行，可以用這個
                    com.message('success', self.resx.editSuccess);//这个notify样式有问题，导致没有提示，原因不明(已解决:發現是index.js頁面注釋了這個控件的初始化代碼)。                    
                    self.gridEdit.accept();
                    self.grid.queryParams(self.form);//重新查詢表格數據
                }
            });
        }
    };
    this.downloadClick = function (vm, event) {
        com.exporter(self.grid).download($(event.currentTarget).attr("suffix"));
    };    
};

var permissionTab = function (row) {
    com.dialog({
        title: "用戶權限",
        width: 1040,
        height: 600,
        html: "#permission-template",
        viewModel: function (win) {
            var self = this;
            this.role = ko.mapping.fromJS(row);
            this.tab = {
                onSelect: function (title, index) {
                    if (title == '按钮权限') {
                        //取得菜单权限中的选中行，并重新加开到按钮权限列表中
                        var temp = {}, data = [], panel = self.grid2.treegrid('getPanel');
                        utils.eachTreeRow(self.grid.treegrid('getData'), function (node) {
                            if (node.checked) {
                                data.push(utils.filterProperties(node, ['children', 'Description'], true));
                                temp[node.MenuCode] = node;
                            }
                        });
                        self.grid2.treegrid('loadData', data);

                        //checkbox点击处理函数
                        var checkHandler = function (obj, value) {
                            if (!obj.length) return;
                            var map = { "0": "/Content/images/checknomark.gif", "1": "/Content/images/checkmark.gif" };
                            obj.attr("src", map[value]).attr("value", value);
                            temp[obj.attr("MenuCode")]["btn_" + obj.attr("ButtonCode")] = parseInt(obj.attr("value"));
                        };

                        //注册checkbox点击事件
                        panel.find("td[field]").unbind("click").click(function () {
                            var img = $(this).find("img"), value = img.attr("value") == "1" ? "0" : "1";
                            checkHandler(img, value);

                            if (img.attr("ButtonCode") == "_checkall")
                                panel.find("img[MenuCode=" + img.attr("MenuCode") + "]").each(function () {
                                    checkHandler($(this), value);
                                });
                        });

                        //注册全选checkbox的事件
                        panel.find(".datagrid-header .icon-chk_unchecked").unbind("click").click(function () {
                            var chk = $(this),
                                value = chk.hasClass("icon-chk_checked") ? "0" : "1",
                                iconcls = chk.hasClass("icon-chk_checked") ? "icon-chk_unchecked" : "icon-chk_checked";
                            chk.removeClass("icon-chk_unchecked").removeClass("icon-chk_checked").addClass(iconcls);

                            panel.find("img").each(function () {
                                checkHandler($(this), value);
                            });
                        });
                    }
                    //else if (title == '字段权限') {
                    //    var temp = {}, data = [];
                    //    utils.eachTreeRow(self.grid.treegrid('getData'), function (node) {
                    //        if (node.checked) {
                    //            data.push(utils.filterProperties(node, ['children', 'Description'], true));
                    //            temp[node.MenuCode] = node;
                    //        }
                    //    });

                    //    self.grid4.OnBeforeDestroyEditor(function (editors, row) {
                    //        temp[row.MenuCode]["AllowColumns"] = editors["AllowColumns"].target.val();
                    //        temp[row.MenuCode]["RejectColumns"] = editors["RejectColumns"].target.val();
                    //    });
                    //    self.grid4.treegrid('loadData', data);
                    //}
                }
            };

            this.grid = {
                height: 460,
                width:1010,
                url: '/api/sys/menu/getenabled/' + row.UserAccount,
                idField: 'MenuCode',
                queryParams: ko.observable(),
                treeField: 'MenuName',
                singleSelect: false,
                onCheck: function (node) {
                    node.checked = true;
                },
                onUncheck: function (node) {
                    node.checked = false;
                },
                onCheckAll: function (rows) {
                    utils.eachTreeRow(rows, function (node) { node.checked = true; });
                },
                onUncheckAll: function (rows) {
                    utils.eachTreeRow(rows, function (node) { node.checked = false; });
                },
                loadFilter: function (d) {
                    var formatterChk = function (ButtonCode) {
                        return function (value, row) {
                            if (value >= 0)
                                return '<img MenuCode="' + row.MenuCode + '" ButtonCode="' + ButtonCode + '" value="' + value + '" src="/Content/images/' + (value ? "checkmark.gif" : "checknomark.gif") + '"/>';
                        };
                    }
                    var cols = [[]];
                    var colwidth = 50;

                    for (var i in d.buttons) {
                        //if (d.buttons[i].ButtonCode == "delERPNo" || d.buttons[i].ButtonCode == "getERPNo") { colwidth = 100; }
                        //else {colwidth = 50; }
                        cols[0].push({ field: 'btn_' + d.buttons[i].ButtonCode, width: d.buttons[i].ColWidth, align: 'center', title: utils.formatString('<span class="icon {1}">{0}</span>', d.buttons[i].ButtonName, d.buttons[i].ButtonIcon), formatter: formatterChk(d.buttons[i].ButtonCode) });
                    }
                    self.grid2.columns(cols);

                    return utils.toTreeData(d.menus, 'MenuCode', 'ParentCode', "children");
                }
            };

            this.grid2 = {
                height: 460,
                width: 1010,
                idField: 'MenuCode',
                treeField: 'MenuName',
                frozenColumns: [[
                    { field: 'MenuName', width: 150, title: '菜单' },
                    {
                        field: 'btn__checkall',
                        width: 50,
                        align: 'center',
                        title: '<span class="icon icon-chk_unchecked">全选</span>',
                        formatter: function (v, r) {
                            for (var i in r) {
                                if (i.indexOf("btn_") > -1 && r[i] > -1) {
                                    return '<img MenuCode="' + r.MenuCode + '" ButtonCode="_checkall" src="/Content/images/' + (v ? "checkmark.gif" : "checknomark.gif") + '"/>';
                                }
                            }
                        }
                    }
                ]],
                columns: ko.observableArray(),
                loadFilter: function (d) {
                    return utils.toTreeData(d, 'MenuCode', 'ParentCode', "children");
                }
            };

            //this.grid3check = function (node, value) {
            //    node.checked = value;
            //    var img = self.grid3.treegrid('getPanel').find('img[PermissionCode=' + node.PermissionCode + ']');
            //    value ? img.show() : img.hide();
            //    img.val(node.IsDefault);
            //};
            //this.grid3 = {
            //    height: 460,
            //    width: 774,
            //    url: '/api/sys/permission/GetRolePermission/' + row.RoleCode,
            //    idField: 'PermissionCode',
            //    queryParams: ko.observable(),
            //    treeField: 'PermissionName',
            //    singleSelect: false,
            //    columns: [[
            //        { field: 'chk', checkbox: true },
            //        { field: 'PermissionName', width: 150, title: '授权名称' },
            //        { field: 'PermissionCode', width: 100, title: '授权代码' },
            //        {
            //            field: 'IsDefault', width: 60, title: '是否默认', align: 'center', formatter: function (v, r) {
            //                return '<img value="' + r.IsDefault + '" style="display:' + (r.checked ? '' : 'none') + '" PermissionCode="' + r.PermissionCode + '" src="/Content/images/' + (v ? "checkmark.gif" : "checknomark.gif") + '"/>';
            //            }
            //        }
            //    ]],
            //    onCheck: function (node) {
            //        self.grid3check(node, true);
            //    },
            //    onUncheck: function (node) {
            //        self.grid3check(node, false);
            //    },
            //    onCheckAll: function (rows) {
            //        utils.eachTreeRow(rows, function (node) { self.grid3check(node, true); });
            //    },
            //    onUncheckAll: function (rows) {
            //        utils.eachTreeRow(rows, function (node) { self.grid3check(node, false); });
            //    },
            //    onLoadSuccess: function (r, d) {
            //        self.grid3.treegrid('getPanel').find("td[field=IsDefault]").unbind('click').click(function (event) {
            //            var img = $(this).find("img"), value = img.attr("value") == "1" ? "0" : "1";
            //            var map = { "0": "/Content/images/checknomark.gif", "1": "/Content/images/checkmark.gif" };
            //            if (value == "1")
            //                self.grid3.treegrid('getPanel').find("img[PermissionCode]").attr("src", map["0"]).val(0);
            //            img.attr("src", map[value]).val(value);
            //            event.stopPropagation();
            //        });
            //    },
            //    loadFilter: function (d) {
            //        return utils.toTreeData(d, 'PermissionCode', 'ParentCode', "children");
            //    }
            //};

            //this.grid4 = {
            //    height: 460,
            //    width: 774,
            //    idField: 'MenuCode',
            //    treeField: 'MenuName',
            //    columns: [[
            //        { field: 'MenuName', width: 150, title: '菜单' },
            //        { field: 'AllowColumns', width: 270, title: '允许', editor: 'text' },
            //        { field: 'RejectColumns', width: 300, title: '拒绝', editor: 'text' }
            //    ]],
            //    loadFilter: function (d) {
            //        return utils.toTreeData(d, 'MenuCode', 'ParentCode', "children");
            //    }
            //};
            //this.grid4Edit = new com.editTreeGridViewModel(this.grid4);
            //this.grid4.onDblClickRow = this.grid4Edit.begin;
            //this.grid4.onClickRow = this.grid4Edit.ended;
            //this.grid4.OnBeforeDestroyEditor = ko.observable();

            this.confirmClick = function () {
                var post = { menus: [], buttons: [], permissions: [], columns: [] };
                utils.eachTreeRow(self.grid.treegrid('getData'), function (node) {
                    if (node.checked) {
                        //1 取得菜单权限数据  
                        post.menus.push({ MenuCode: node.MenuCode });

                        //2 取得按钮权限数据
                        for (var btn in node)
                            if (btn.substr(0, 4) == 'btn_' && node[btn] == '1' && btn != 'btn__checkall')
                                post.buttons.push({ MenuCode: node.MenuCode, ButtonCode: btn.split('_')[1] });

                        //3取得列权限数据
                        if (node.AllowColumns || node.RejectColumns)
                            post.columns.push({ MenuCode: node.MenuCode, AllowColumns: node.AllowColumns, RejectColumns: node.RejectColumns });
                    }
                });

                //4 取得授权代码数据
                //var panel3 = self.grid3.treegrid('getPanel');
                //utils.eachTreeRow(self.grid3.treegrid('getData'), function (node) {
                //    if (node.checked) {
                //        var img = panel3.find("img[PermissionCode=" + node.PermissionCode + "][value=1]");
                //        post.permissions.push({ PermissionCode: node.PermissionCode, IsDefault: img.length });
                //    }
                //});

                com.ajax({
                    url: '/api/sys/user/editpermission/' + row.UserAccount, //混合傳參
                    data: ko.toJSON(post),
                    success: function (d) {
                        self.cancelClick();
                        com.message('success', '保存成功！');
                    }
                });

            };
            this.cancelClick = function () {
                win.dialog('close');
            };

        }
    });
};
 