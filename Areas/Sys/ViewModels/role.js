/**
* 模块名：mms viewModel
* 程序名: bms.bgbu.js
* Copyright(c) 2013-2015 liuhuisheng [ liuhuisheng.xm@gmail.com ] 
**/
//var RoleMenu = RoleMenu || {};

//這兩行必須寫，否則視圖引用viewModel將會報錯。
// var viewModel = bms.bgbu.viewModel;
//ko.bindingViewModel(new viewModel());

var viewModel = function (data) {
    var self = this;
    this.roletree = function () {
        var d = data.dataSource.MenuTreeData; //取出所有菜單列表
        //以下四行構造樹形菜單
        d = utils.copyProperty(d.rows || d, ["MenuCode", "IconClass"], ["_id", "iconCls"], false);//複製列MenuCode和IconClass，領取名為_id和iconCls,false表示不刪除原有列
        d = utils.toTreeData(d, '_id', 'ParentCode', "children");//將列表轉為樹形結構

        var treeData = JSON.parse(JSON.stringify(d).replace(/_id/g, "id").replace(/MenuName/g, "text"));
        treeData.unshift({ "id": 0, "text": "" });//在數組頭部添加空的新元素
        return treeData;
    };

    this.grid = {
        size: { w: 239, h: 40 },
        idField: 'AutoID',
        url: "/api/sys/menu/GetRoleMenuList",
        queryParams: ko.observable(),
        pagination: true
    };
    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onClickRow = function () {
        //self.gridEdit.begin;
        self.editClick();//注意：此处不能用this.editClick(),this此时还是空的。也不能縮減為this.grid.onClickRow=self.editClick()
    }
    this.grid.OnAfterCreateEditor = function (editors) {
        com.readOnlyHandler('input')(editors["RoleID"].target, true);  //注意此處設定關聯欄位RoleID不可修改。應該是可編輯的欄位，readonly:true的不能寫，否則報錯,另外這句將把RoleID變為只讀欄位
        //因為RoleID來源于左邊樹選中的值，所以無論增加新行還是修改已有數據，此時都不能改。
    };

    this.tree = {
        method:'GET',
        url: '/api/sys/menu/GetRoleTree',
        queryParams: ko.observable(),
        loadFilter: function (d) {  //d代表url返回的内容(ExpandoObject)：            
            var filter = utils.filterProperties(d.rows || d, ['RoleID as id', 'RoleName as text', 'ParentId as pid']);//此處映射的id必須是數字，否則結果將為undefined
            var data = utils.toTreeData(filter, 'id', 'pid', "children");            
            return [{ id: '', text: '所有Role',parentId:data.pid, children: data }];
        },
        onSelect: function (node) {
            self.RoleID(node.text);
            //self.PID=ko.observable(node.pid);
        }
    };

    //点节点時執行一下兩行，并執行ApiController裡面的GetBUList方法
    this.RoleID = ko.observable();
    this.RoleID.subscribe(function (value) {
        self.grid.queryParams({ RoleID: value });
    });  

    this.refreshClick = function () {
        window.location.reload();
    };
    this.addClick = function () {
        if (!self.RoleID()) return com.message('warning', '请先在左边选择要添加的BG！');
        var row = { RoleID: self.RoleID() };
        self.gridEdit.addnew(row);
        //com.ajax({
        //    type: 'GET',
        //    url: '/api/sys/menu/GetNewBillNo/' + self.RoleID(),
        //    success: function (d) {
        //        var row = { RoleID: self.RoleID() };
        //        self.gridEdit.addnew(row);
        //    }
        //});
    };
    this.editClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (row) {
           var treeData = self.roletree();//注意調用時使用self，定義時使用this.

            //设置上级菜单下拉树
            var gridOpt = $.data(self.grid.$element()[0], "datagrid").options;
            var col = $.grep(gridOpt.columns[0], function (n) { return n.field == 'MenuCode' })[0];//取出列名為MenuCode的列
            col.editor = { type: 'combotree', options: { data: treeData } };//設置該列的樹形下拉框數據源
            col.editor.options.onBeforeSelect = function (node) {
                var isChild = utils.isInChild(treeData, row._id, node.id);
                com.messageif(isChild, 'warning', '不能将自己或下级设为上级节点');//待完成：檢查選中的節點node.id是否為子節點，因為只有子節點才可以作為菜單保存。
                return !isChild;
            };

            //开始编辑行数据
            self.gridEdit.begin(row);
            //self.grid.datagrid('beginEdit', row._id);
            //self.edit_id = row._id;
            //var eds = self.grid.datagrid('getEditors', row._id);
            //var edt = function (field) { return $.grep(eds, function (n) { return n.field == field })[0]; };
            //self.afterCreateEditors(edt);
        }
    };
    this.deleteClick = self.gridEdit.deleterow;
    this.saveClick = function () {
        self.gridEdit.ended();
        var post = { list: self.gridEdit.getChanges(), RoleID: this.RoleID() };//注意此處綁定的欄位并不限於視圖中的欄位，而是表格url返回的所有欄位
        if (self.gridEdit.isChangedAndValid()) {
            com.ajax({
                url: '/api/sys/menu/EditRoleMenu',
                data: ko.toJSON(post),
                success: function (d) {
                    com.message('success', '保存成功！');
                    self.gridEdit.accept();
                    self.grid.queryParams({ RoleID: post.RoleID });//重新查詢表格數據
                }
            });
        }
    };

    this.typeClick = function () {
        com.dialog({
            title: "&nbsp;Role清單",
            iconCls: 'icon-node_tree',
            width: 516,
            height: 410,
            html: "#role-template",
            viewModel: function (w) {
                var that = this;
                this.grid = {
                    width: 500,
                    height: 340,
                    pagination: true,
                    pageSize: 20,
                    url: "/api/sys/menu/Getrolelist",
                    queryParams: ko.observable()
                };
                this.gridEdit = new com.editGridViewModel(this.grid);
                //this.grid.OnAfterCreateEditor = function (editors, row) {
                //    if (!row._isnew) com.readOnlyHandler('input')(editors["AutoID"].target, true);
                //};
                this.grid.onClickRow = that.gridEdit.ended;
                this.grid.onDblClickRow = that.gridEdit.begin;
                this.grid.toolbar = [
                    {
                        text: '新增', iconCls: 'icon-add1', handler: function () {                            
                            var t = com.formatDateTime(new Date());
                            var row = { Creator: 'admin',CreateDate:t };
                            that.gridEdit.addnew(row);
                        }
                    }, '-',
                    { text: '编辑', iconCls: 'icon-edit', handler: that.gridEdit.begin }, '-',
                    { text: '删除', iconCls: 'icon-cross', handler: that.gridEdit.deleterow }
                ];
                this.confirmClick = function () {
                    //com.message('Test','TestAAA');
                    if (that.gridEdit.isChangedAndValid()) {
                        var list = that.gridEdit.getChanges(['AutoID', 'RoleID']);
                        com.ajax({
                            url: '/api/sys/menu/editroletype/',
                            data: ko.toJSON({ list: list }),
                            success: function (d) {
                                that.cancelClick();
                                that.grid.queryParams(ko.observable());
                                self.tree.$element().tree('reload');                                
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