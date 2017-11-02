/**
* 模块名：mms viewModel
* 程序名: mms.Supplier.js
**/
var mms = mms || {};
var user = $.cookie('UserCode');
var SupplierCodeList = {};

mms.PartNo = function (data) {
    var self = this;
    this.urls = data.urls;
    this.resx = data.resx;
    this.dataSource = data.dataSource; SupplierCodeList = data.dataSource.SupplierCodeList;
    this.form = ko.mapping.fromJS(data.form);
    this.defaultRow = data.defaultRow;
    this.setting = data.setting;
    delete this.form.__ko_mapping__;

    this.grid = {
        size: { w: 4, h: 94 },
        url: self.urls.query,
        queryParams: ko.observable(),
        pagination: true,pageSize:10
    };

    this.grid.queryParams(data.form); //這一句是確保在頁面首次加載查詢表格時使用默認條件，不寫這一句也會查詢，但會出現request找不到參數的錯誤

    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onDblClickRow = this.gridEdit.begin;
    this.grid.onClickRow = this.gridEdit.ended;
    this.grid.OnAfterCreateEditor = function (editors, row) {
        if (!row._isnew) {
            //com.readOnlyHandler('input')(editors.LEGAL_ENTITY.target, true);
            com.readOnlyHandler('input')(editors.SuppPN.target, true);
        }
    };
    this.searchClick = function () {
        var suppPN = this.form.SuppPN();
        suppPN=suppPN.replace(/[\'\"\\\/\b\f\n\r\t]/g, '');
        suppPN=suppPN.replace(/[\@\#\$\%\^\&\*\(\)\{\}\:\"\L\<\>\?\[\]]/);

        this.form.SuppPN(suppPN);
        // 去掉特殊字符

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
        com.ajax({
            type: 'GET',
            url: self.urls.add,
            success: function (d) {
                var row = $.extend( self.defaultRow,{ PNID: d });
                self.gridEdit.addnew0(row);
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
        var flag = true;
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
                    com.message('success', '料号清单生成OK！');
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

    this.typeClick = function () {
        com.dialog({
            title: "&nbsp;料号类别",
            iconCls: 'icon-report_add',
            width: 516,
            height: 410,
            html: "#type-template",
            viewModel: function (w) {
                var that = this;
                this.grid = {
                    width: 500,
                    height: 340,
                    pagination: true,
                    pageSize: 20,
                    url: "/api/mms/materialtype/get",
                    queryParams: {TypeName:''}
                };
                this.gridEdit = new com.editGridViewModel(this.grid);
                this.grid.OnAfterCreateEditor = function (editors, row) {
                    com.readOnlyHandler('input')(editors["AutoID"].target, true);
                    com.readOnlyHandler('input')(editors["TypeCode"].target, true);
                };
                this.grid.onClickRow = that.gridEdit.ended;
                this.grid.onDblClickRow = that.gridEdit.begin;
                this.grid.toolbar = [
                    {
                        text: '新增', iconCls: 'icon-add1', handler: function () {                           
                            var row = { AutoID:'0',TypeCode:'',TypeName:'' };
                            that.gridEdit.addnew(row);
                        }
                    }, '-',
                    { text: '编辑', iconCls: 'icon-edit', handler: that.gridEdit.begin }, '-',
                    { text: '删除', iconCls: 'icon-cross', handler: that.gridEdit.deleterow }
                ];
                this.confirmClick = function () {
                    if (that.gridEdit.isChangedAndValid()) {
                        var list = that.gridEdit.getChanges(['AutoID', 'TypeCode', 'TypeName']);
                        com.ajax({
                            url: '/api/mms/materialtype/edit/',
                            data: ko.toJSON({ list: list }),
                            success: function (d) {
                                that.cancelClick();
                                //that.grid.queryParams({ TypeName: '' })
                                //self.tree.$element().tree('reload');
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

    this.brandClick = function () {
        com.dialog({
            title: "&nbsp;品牌清单",
            iconCls: 'icon-pictures',
            width: 516,
            height: 410,
            html: "#brand-template",
            viewModel: function (w) {
                var that = this;
                this.grid = {
                    width: 500,
                    height: 340,
                    pagination: true,
                    pageSize: 10,
                    url: "/api/mms/materialtype/getBrand",
                    queryParams: { Brand: '' }
                };
                this.gridEdit = new com.editGridViewModel(this.grid);
                this.grid.OnAfterCreateEditor = function (editors, row) {
                    com.readOnlyHandler('input')(editors["AutoID"].target, true);
                    if (row.AutoID != '0')
                    { com.readOnlyHandler('input')(editors["Brand"].target, true); }
                };
                this.grid.onClickRow = that.gridEdit.ended;
                this.grid.onDblClickRow = that.gridEdit.begin;
                this.grid.toolbar = [
                    {
                        text: '新增', iconCls: 'icon-add1', handler: function () {
                            var row = { AutoID: '0', Brand: '', Remark: '' };
                            that.gridEdit.addnew(row);
                        }
                    }, '-',
                    { text: '编辑', iconCls: 'icon-edit', handler: that.gridEdit.begin }, '-',
                    { text: '删除', iconCls: 'icon-cross', handler: that.gridEdit.deleterow }
                ];
                this.confirmClick = function () {
                    if (that.gridEdit.isChangedAndValid()) {
                        var list = that.gridEdit.getChanges(['AutoID', 'Brand', 'Remark']);
                        com.ajax({
                            url: '/api/mms/materialtype/editBrand/',
                            data: ko.toJSON({ list: list }),
                            success: function (d) {                                
                                //that.grid.queryParams({ TypeName: '' })
                                //self.tree.$element().tree('reload');
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

    this.unitClick = function () {
        com.dialog({
            title: "&nbsp;数量单位维护",
            iconCls: 'icon-vector',
            width: 516,
            height: 410,
            html: "#unit-template",
            viewModel: function (w) {
                var that = this;
                this.grid = {
                    width: 500,
                    height: 340,
                    pagination: true,
                    pageSize: 10,
                    url: "/api/mms/materialtype/getUnit",
                    queryParams: { Unit: '' }
                };
                this.gridEdit = new com.editGridViewModel(this.grid);
                this.grid.OnAfterCreateEditor = function (editors, row) {
                    com.readOnlyHandler('input')(editors["AutoID"].target, true);
                    if (row.AutoID != '0')
                    { com.readOnlyHandler('input')(editors["Unit"].target, true); }
                };
                this.grid.onClickRow = that.gridEdit.ended;
                this.grid.onDblClickRow = that.gridEdit.begin;
                this.grid.toolbar = [
                    {
                        text: '新增', iconCls: 'icon-add1', handler: function () {
                            var row = { AutoID: '0', Brand: '', Remark: '' };
                            that.gridEdit.addnew(row);
                        }
                    }, '-',
                    { text: '编辑', iconCls: 'icon-edit', handler: that.gridEdit.begin }, '-',
                    { text: '删除', iconCls: 'icon-cross', handler: that.gridEdit.deleterow }
                ];
                this.confirmClick = function () {
                    if (that.gridEdit.isChangedAndValid()) {
                        var list = that.gridEdit.getChanges(['AutoID', 'Unit', 'Chiname']);
                        com.ajax({
                            url: '/api/mms/materialtype/editUnit/',
                            data: ko.toJSON({ list: list }),
                            success: function (d) {
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

    this.taxrateClick = function () {
        com.dialog({
            title: "&nbsp;税率维护",
            iconCls: 'icon-calendar_edit',
            width: 516,
            height: 410,
            html: "#taxrate-template",
            viewModel: function (w) {
                var that = this;
                this.grid = {
                    width: 500,
                    height: 340,
                    pagination: true,
                    pageSize: 10,
                    url: "/api/mms/materialtype/getTaxRate",
                    queryParams: {}
                };
                this.gridEdit = new com.editGridViewModel(this.grid);
                this.grid.OnAfterCreateEditor = function (editors, row) {
                    com.readOnlyHandler('input')(editors["AutoID"].target, true);                   
                };
                this.grid.onClickRow = that.gridEdit.ended;
                this.grid.onDblClickRow = that.gridEdit.begin;
                this.grid.toolbar = [
                    {
                        text: '新增', iconCls: 'icon-add1', handler: function () {
                            var row = { AutoID: '0', Brand: '', Remark: '' };
                            that.gridEdit.addnew(row);
                        }
                    }, '-',
                    { text: '编辑', iconCls: 'icon-edit', handler: that.gridEdit.begin }, '-',
                    { text: '删除', iconCls: 'icon-cross', handler: that.gridEdit.deleterow }
                ];
                this.confirmClick = function () {
                    if (that.gridEdit.isChangedAndValid()) {
                        var list = that.gridEdit.getChanges(['AutoID', 'TaxRate', 'Remark']);
                        com.ajax({
                            url: '/api/mms/materialtype/editTaxRate/',
                            data: ko.toJSON({ list: list }),
                            success: function (d) {
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
};

mms.Relate = function (data) {
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
        pagination: false, pageSize: 10,
        singleSelect: false
    };

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
        var row = self.defaultRow;
        self.gridEdit.addnew(row);       
    };
    this.deleteClick = function () {
        var row = self.grid.datagrid('getChecked');  //注意getSelected只能選到一行
        if (!row) return com.message('warning', self.resx.noneSelect);

        var array = [], arrayTypeCode = [];;

        $.each(row, function (index, item) {
            array.push(item.AutoID); arrayTypeCode.push(item.CustPN);
        });
        var str = array.join(',');
        com.message('confirm', "确定要删除 " + arrayTypeCode.join(',') + " 的資料吗？", function (b) {
            if (b) {
                com.ajax({
                    data: ko.toJSON({value:str}),
                    url: self.urls.remove,
                    success: function () {
                        com.message('success', self.resx.deleteSuccess);
                        self.grid.queryParams(self.form);
                    }
                });
            }
        });
    };


    this.editClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        self.gridEdit.begin()
        //com.openTab(row.ERP_CODE + '明細資料', "/mms/BA_Payee/Edit/" + row[self.idField]);

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

mms.EPN=function(data) {
    var self = this;

    this.dataSource = data.dataSource;
    //this.defaultRow = data.defaultRow;

    this.grid = {
        size: { w: 4, h: 40 }, pageSize: 10,
        url: '/api/mms/partno/GetEPNAll',
        idField: '_id',
        queryParams: ko.observable(),
        treeField: 'PN',
        loadFilter: function (d) {
            d = utils.copyProperty(d.rows || d, ["PNID", "IconClass"], ["_id", "iconCls"], false);
            return utils.toTreeData(d, '_id', 'ParentCode', "children");
        }
    };
    this.refreshClick = function () {
        window.location.reload();
    };
    this.addClick = function () {
        if (self.grid.onClickRow()) {
            var row = { _id: utils.uuid(), PNID: '', PN: '',Qty:'1',Currency:'RMB',Price:'1',OrderBy:'1' };
            self.grid.treegrid('append', { parent: '', data: [row] });
            self.grid.treegrid('select', row._id);
            self.grid.$element().data("datagrid").insertedRows.push(row);
            self.editClick();
        }
    };
    this.editClick = function () {
        var row = self.grid.treegrid('getSelected');
        if (row) {
            //取得父节点数据
            var treeData = JSON.parse(JSON.stringify(self.grid.treegrid('getData')).replace(/_id/g, "id").replace(/PN/g, "text"));
            treeData.unshift({ "id": 0, "text": "" });//在數組頭部條件新元素

            //设置上级菜单下拉树
            var gridOpt = $.data(self.grid.$element()[0], "datagrid").options;
            var col = $.grep(gridOpt.columns[0], function (n) { return n.field == 'ParentCode' })[0];//篩選出列名為ParentCode的數組
            col.editor = { type: 'combotree', options: { data: treeData } };//設置樹形下拉框數據源
            col.editor.options.onBeforeSelect = function (node) {
                var isChild = utils.isInChild(treeData, row._id, node.id);
                com.messageif(isChild, 'warning', '不能将自己或下级设为上级节点');
                return !isChild;
            };

            //开始编辑行数据
            self.grid.treegrid('beginEdit', row._id);
            self.edit_id = row._id;
            //var eds = self.grid.treegrid('getEditors', row._id);
            //var edt = function (field) { return $.grep(eds, function (n) { return n.field == field })[0]; };
            //self.afterCreateEditors(edt);
        }
    };
    this.afterCreateEditors = function (editors) {
        var iconInput = editors("IconClass").target;
        var onShowPanel = function () {
            iconInput.lookup('hidePanel');
            com.dialog({
                title: "&nbsp;选择图标",
                iconCls: 'icon-node_tree',
                width: 800,
                height: 550,
                url: "/Content/page/icon.html",
                viewModel: function (w) {
                    w.find('#iconlist').css("padding", "10px");
                    w.find('#iconlist li').attr('style', 'float:left;border:1px solid #fff; line-height:20px; margin-right:4px;width:16px;cursor:pointer')
                     .click(function () {
                         iconInput.lookup('setValue', $(this).find('span').attr('class').split(" ")[1]);
                         w.dialog('close');
                     }).hover(function () {
                         $(this).css({ 'border': '1px solid red' });
                     }, function () {
                         $(this).css({ 'border': '1px solid #fff' });
                     });
                }
            });
        };
        iconInput.lookup({ customShowPanel: true, onShowPanel: onShowPanel, editable: true });
        iconInput.lookup('resize', iconInput.parent().width());
        iconInput.lookup('textbox').unbind();
    };
    this.grid.OnBeforeDestroyEditor = function (editors, row) {
        row.ParentName = editors['ParentCode'].target.combotree('getText');
        //row.IconClass = editors["IconClass"].target.lookup('textbox').val();
    };
    this.deleteClick = function () {
        var row = self.grid.treegrid('getSelected');
        if (row) {
            self.grid.$element().treegrid('remove', row._id);
            self.grid.$element().data("datagrid").deletedRows.push(row);
        }
    };
    this.grid.onDblClickRow = self.editClick;
    this.grid.onClickRow = function () {
        var edit_id = self.edit_id;
        if (!!edit_id) {
            if (self.grid.treegrid('validateRow', edit_id)) { //通过验证
                self.grid.treegrid('endEdit', edit_id);
                self.edit_id = undefined;
            }
            else { //未通过验证
                self.grid.treegrid('select', edit_id);
                return false;
            }
        }
        return true;
    };
    this.saveClick = function () {
        self.grid.onClickRow();
        var post = {};
        post.list = new com.editTreeGridViewModel(self.grid).getChanges(['_id', 'PN', 'PNID', 'ParentCode','CDesc', 'Qty','Price','Currency', 'OrderBy']);
        if (self.grid.onClickRow() && post.list._changed) {
            com.ajax({
                url: '/api/mms/partno/editpn',
                data: ko.toJSON(post),
                success: function (d) {
                    com.message('success', '保存成功！');
                    self.grid.treegrid('acceptChanges');
                    self.grid.queryParams({});
                }
            });
        }

    };
}

var openRelateInfo = function (PNID, SuppPN, CDesc, type) {
    if (type == 'Attach')
    {
        var param = { BizID: PNID, BizCode: SuppPN, BizTable: 'DG_PartNo' }
        mms.com.upload(param);
    }
    else if (type == 'EPNAttach') {
        var param = { BizID: PNID, BizCode: SuppPN, BizTable: 'DG_PartNoEcom' }
        mms.com.upload(param);
    }
    else {
        var param = { RelateType: 'Relate' + type, pnid: PNID, supppn: SuppPN, cdesc: CDesc };
        mms.com.relateMaterial(param);
    }
}

