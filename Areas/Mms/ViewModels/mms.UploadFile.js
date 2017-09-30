
var mms = mms || {};

mms.Upload = function (data) {
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
        pagination: false,
        singleSelect: false
    };

    this.grid.queryParams({BizID:self.form.BizID(),BizTable:self.form.BizTable()});
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
    
    this.deleteClick = function () {
        var row = self.grid.datagrid('getChecked');  //注意getSelected只能選到一行
        if (!row) return com.message('warning', self.resx.noneSelect);

        var array = [], arrayTypeCode = [];

        $.each(row, function (index, item) {
            array.push(item.AutoID); arrayTypeCode.push(item.FileName);
        });
        var str = array.join(',');
        com.message('confirm', "确定要删除 " + arrayTypeCode.join(',') + " 的資料吗？", function (b) {
            if (b) {
                com.ajax({
                    data: ko.toJSON({ value: str }),
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
                    self.grid.queryParams({ BizID: self.form.BizID(), BizTable: self.form.BizTable() });;//重新查詢表格數據
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

    this.clickUpload = function () {
        var strUploadID = "AjaxFileUploader";
        var post = { 'UploadID': strUploadID, BizID: self.form.BizID(), BizTable: self.form.BizTable(), BizType: self.form.BizType() };

        if (!post.BizType) { com.message('warning', '请选择文件类型!'); return false; }

        $.ajaxFileUpload({
            url: self.urls.save,
            secureuri: false,
            fileElementId: strUploadID,
            dataType: 'json',
            data: post,
            success: function (d, status) {
                if (d.error) {
                    com.message('warning', d.error);
                }
                else {
                    com.message('success', "上傳成功!");                    
                    self.grid.queryParams({ BizID: self.form.BizID(), BizTable: self.form.BizTable()}); //BizID,BizTable,BizType
                    //var param = ko.toJS({ previewNo: previewNo });
                    //self.grid1.queryParams(param);                    
                }
            },
            error: function (d, status, e) {
                alert(e);
            }
        });
    }
};