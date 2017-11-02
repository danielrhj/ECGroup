/// <reference path="common.js" />

/**
* 模块名：共通脚本
* 程序名: 材料模块通用方法mms.com.js
* Copyright(c) 2013-2015 liuhuisheng [ liuhuisheng.xm@gmail.com ] 
**/

var mms = mms || {};
mms.com = {};

mms.com.getCurrentProject = function () {
    return window!=parent?parent.$.cookie('CurrentProject'):"";
};

mms.com.formatSrcBillType = function (value) {
    var dict = {
        'receive': '收料单',
        'send': '发料单',
        'adjust':'库存调整',
        'direct':'直入直出单',
        'rentin': '租赁单',
        'return': '退货单',
        'refund': '退库单',
        'transfer': '调拨单',
        'lossReport': '报损单'
    };
    return dict[value]||value;
};

//计算总金额 用法：
//this.grid.OnAfterCreateEditor = com.psi.calcTotalMoney(self, "Num", "UnitPrice", "Money", "TotalMoney");
mms.com.bindCalcTotalMoney = function (self, fieldNum, fieldUnitPrice, fieldRowTotal, fieldAllTotal) {
    return function (editors) {
        var RowTotal = editors[fieldRowTotal].target;   //Money
        var Num = editors[fieldNum].target;             //Num
        var UnitPrice = editors[fieldUnitPrice].target; //UnitPrice

        com.readOnlyHandler('input')(editors[fieldRowTotal].target, true);
        var calc = function () {
            var rowTotalMoney = Num.numberbox('getValue') * UnitPrice.numberbox('getValue');
            RowTotal.numberbox('setValue', rowTotalMoney);
            var allMoney = rowTotalMoney - Number(editors[fieldRowTotal].oldHtml.replace(',', '') * 100) / 100;
            $.each(self.grid.datagrid('getData').rows, function () {
                var addMoney = (Number(this[fieldRowTotal] * 100) / 100) || 0;
                allMoney += addMoney
            });
            self.form[fieldAllTotal](allMoney);
        };
        Num.blur(calc);
        UnitPrice.blur(calc);
    };
};

mms.com.calcTotalMoneyWhileRemoveRow = function (self, fieldRowTotal, fieldAllTotal) {
    var allMoney = 0, fieldRowTotal = fieldRowTotal || "Money", fieldAllTotal = fieldAllTotal || "TotalMoney";
    $.each(self.grid.datagrid('getData').rows, function () {
        var addMoney = (Number(this[fieldRowTotal] * 100) / 100) || 0;
        allMoney += addMoney
    });
    self.form[fieldAllTotal](allMoney);
};

mms.com.calcTotalMoneyWhileRemoveRowForMoreGrid = function (self,grid, fieldRowTotals, fieldAllTotals) {   
    var allMoney = [0,0,0]; //new Array(0,0,0);注意數組必須初始化為數字

    $.each(grid.datagrid('getData').rows, function (k,row) {
        $.each(fieldRowTotals, function (i, value) {
            allMoney[i] += (Number(row[value] * 100) / 100) || 0;
        }); 
    });

    $.each(fieldAllTotals, function (i,value) {
        self.form[value](allMoney[i]);
    });
};

mms.com.bindCalc1From2 = function (self,grid, fieldNum, fieldUnitPrice, fieldRowTotal, fieldAllTotal) {
    return function (editors) {
        var RowTotal = editors[fieldRowTotal].target;   //Money
        var Num = editors[fieldNum].target;             //Num
        var UnitPrice = editors[fieldUnitPrice].target; //UnitPrice

        com.readOnlyHandler('input')(editors[fieldRowTotal].target, true);
        var calc = function () {
            var rowTotalMoney = Num.numberbox('getValue') - UnitPrice.numberbox('getValue');
            RowTotal.numberbox('setValue', rowTotalMoney);
            var allMoney = rowTotalMoney - Number(editors[fieldRowTotal].oldHtml.replace(',', '') * 100) / 100;
            var allNum = 0, allUnitPrice = 0;

            allNum = Num.numberbox('getValue') - Number(editors[fieldNum].oldHtml.replace(',', '') * 100) / 100;
            allUnitPrice = UnitPrice.numberbox('getValue') - Number(editors[fieldUnitPrice].oldHtml.replace(',', '') * 100) / 100;

            $.each(grid.datagrid('getData').rows, function () {
                var addMoney = (Number(this[fieldRowTotal] * 100) / 100) || 0;
                allMoney += addMoney;

                addMoney = (Number(this[fieldNum] * 100) / 100) || 0;
                allNum += addMoney;

                addMoney = (Number(this[fieldUnitPrice] * 100) / 100) || 0;
                allUnitPrice += addMoney;

                addMoney = 0;
            });
            self.form[fieldAllTotal[0]](allNum);
            self.form[fieldAllTotal[1]](allUnitPrice);
            self.form[fieldAllTotal[2]](allMoney);
        };
        Num.blur(calc);
        UnitPrice.blur(calc);
    };
};

mms.com.auditDialog = function () {
    var query = parent.$;
    var winAudit = query('#w_audit_div'), args = arguments;
    if (winAudit.length == 0) {
        var html = utils.functionComment(function () {/*
            <div id="w_audit_wrapper">
                <div id="w_audit_div" class="easyui-dialog"  title="审核" data-options="modal:true,closed:true,iconCls:'icon-user-accept'" style="width:400px;height:210px;" buttons="#w_audit_div_button"> 
                    <div class="container_16" style="width:90%;margin:5%;">  
                        <div class="grid_3 lbl" style="font-weight: bold;color:#7e7789">审核状态</div>  
                        <div class="grid_13 val">
                            通过审核<input type="radio" name="AuditStatus" value="passed" data-bind="checked:form.status,disable:disabled" style="margin-right:10px;" /> 
                            取消审核<input type="radio" name="AuditStatus" value="reject" data-bind="checked:form.status,disable:disabled" />
                        </div>
                        <div class="grid_3 lbl" style="margin-top:5px;font-weight: bold;color:#7e7789" style="font-weight: bold;">审核意见</div>  
                        <div class="grid_13 val"><textarea style="width:272px;height:60px;" class="z-text" data-bind="value:form.comment" ></textarea></div>
                        <div class="clear"></div>
                    </div> 
                </div> 
                <div id="w_audit_div_button" class="audit_button">  
                    <a href="javascript:void(0)" data-bind="click:confirmClick" class="easyui-linkbutton" iconCls="icon-ok" >确定</a>  
                    <a href="javascript:void(0)" data-bind="click:cancelClick" class="easyui-linkbutton" iconCls="icon-cancel" >取消</a>  
                </div> 
            </div>
            */});
        var wrapper = query(html).appendTo("body");
        wrapper.find(".easyui-linkbutton").linkbutton();
        winAudit = wrapper.find(".easyui-dialog").dialog();
    }

    var viewModel = function () {
        var self = this;
        this.disabled = ko.observable(true);
        this.form = {
            status: args[0].ApproveState()=="passed"?"reject":"passed",
            comment: args[0].ApproveRemark()
        };
        this.confirmClick = function () {
            winAudit.dialog('close');
            if (typeof args[1] === 'function') {
                args[0].ApproveState(this.form.status);
                args[0].ApproveRemark(this.form.comment);
                args[1].call(this, ko.toJS(self.form));
            }
        };
        this.cancelClick = function () {
            winAudit.dialog('close');
        };
    }

    var node = winAudit.parent()[0];
    winAudit.dialog('open');
    ko.cleanNode(node);
    ko.applyBindings(new viewModel(), node);
};
 
//弹出选择材料窗口
mms.com.selectMaterial = function (vm, param) {
    var grid = vm.grid;
    var addnew = vm.gridEdit.addnew;
    //var defaultRow = vm.defaultRow;
    var defaultRow = vm.tabs[0].defaults;
    var url = vm.urls.getrowid;
    var iframeWidth = 800;

    //var isExist = {}, existData = grid.datagrid('getData').rows;
    //for (var j in existData)
    //    isExist[existData[j].MaterialCode] = true;
    var orgRows = grid.datagrid('getData').rows;
    var comapreArray = [];

    if (param.LookupType == 'LookupSuppPNForRFQ')
    { comapreArray = ['SuppPN','Qty']; iframeWidth = 1100; }
    else if (param.LookupType == 'LookupSuppPNForBuyOrder')
    { comapreArray = ['SuppPN']; iframeWidth = 810; }
    else if (param.LookupType == 'LookupSuppPNForReceiving')
    { comapreArray = ['BuyNo', 'SuppPN']; iframeWidth = 1050; }
else if (param.LookupType == 'LookupCustPNForSaleQuote'||param.LookupType == 'LookupCustPNForSaleOrder')
    { comapreArray = ['CustPN','Qty']; iframeWidth =1030; }
    else if (param.LookupType == 'LookupCustPNForShiping')
    { comapreArray = ['CustPN', 'PO']; iframeWidth = 1250; } //出货时比较订单号码和客户料号
    else
    { comapreArray = ['MaterialCode', 'SrcBillType', 'SrcBillNo']; }

    param._xml == comapreArray;
    var fnEqual = function (row1, row2) {
        for (var key in comapreArray) 
            if (row1[comapreArray[key]] != row2[comapreArray[key]])
                return false;
        return true;
    }
    var fnExist = function (row) {
        for (var i in orgRows)
            if (fnEqual(orgRows[i], row))
                return true;
        return false;
    };

    var target = parent.$('#selectMaterial').length ? parent.$('#selectMaterial') : parent.$('<div id="selectMaterial"></div>').appendTo('body');
    utils.clearIframe(target);

    var opt = { title: '选择料号', width: iframeWidth, height: 610, modal: true, collapsible: false, minimizable: false, maximizable: true, closable: true };
    opt.content = "<iframe id='frm_win_material' src='/mms/home/lookupmaterial' style='height:100%;width:100%;border:0;' frameborder='0'></iframe>";  //frameborder="0" for ie7
    opt.paramater = param;      //可传参数
    opt.onSelect = function (data) {                //可接收选择数据
        var total = data.total;
        var rows = data.rows;       

        for (var i in rows) {
            if (!fnExist(rows[i])) {
                var newRow = {};
                    if (param.LookupType == 'LookupSuppPNForRFQ') { defaultRow.RFQID = vm.currentKey; defaultRow.RFQPriceT = rows[i].BuyPrice; }
                    if (param.LookupType == 'LookupSuppPNForBuyOrder') { defaultRow.BuyID = vm.currentKey; }
                    if (param.LookupType == 'LookupSuppPNForReceiving') { defaultRow.RcvID = vm.currentKey; defaultRow.Qty = rows[i].RemainNum; defaultRow.RcvPrice = rows[i].BuyPrice; }

                    if (param.LookupType == 'LookupCustPNForSaleQuote') { defaultRow.QuoteID = vm.currentKey; }
                    if (param.LookupType == 'LookupCustPNForSaleOrder') { defaultRow.POID = vm.currentKey; }
                    if (param.LookupType == 'LookupCustPNForShiping')
                    { defaultRow.ShipID = vm.currentKey; defaultRow.Qty = Math.min(rows[i].OrderRemain, rows[i].WHQty); }

                    newRow = $.extend(newRow, defaultRow, rows[i]);
                    addnew(newRow);     //注意连续新增时,grid里面设置的必填栏位可能会阻止新增
                
            }
        }
    };
    target.window(opt);
};

//弹出选择代理商/客户窗口
mms.com.selectSupplier = function (vm, param) {
    var form = vm.pageData.form;    
    var iframeWidth = 628;    
    var LookupType = param.type;
    var wtitle = '选择' + ((LookupType == 'SaleQuote' || LookupType == 'SaleOrder' || LookupType == 'Shiping') ? '客户' : '代理商');

    var target = parent.$('#selectSupplier').length ? parent.$('#selectSupplier') : parent.$('<div id="selectSupplier"></div>').appendTo('body');
    utils.clearIframe(target);

    var opt = { title: wtitle, width: iframeWidth, height: 520, modal: true, collapsible: false, minimizable: false, maximizable: true, closable: true };
    opt.content = "<iframe id='frm_win_partner' src='/mms/home/lookupSupplier' style='height:100%;width:100%;border:0;' frameborder='0'></iframe>";  //frameborder="0" for ie7
    opt.paramater = param;      //可传参数
    opt.onSelect = function (data) {                //可接收选择数据       
        if (LookupType == 'BuyOrder' || LookupType == 'RFQ' || LookupType == 'Receive') {
            form.SuppCode(data.value);
            form.SuppAbbr(data.text);
            form.Contact(data.Contact);
            form.Tel(data.Tel);
        }
        else if (LookupType == 'SaleQuote')
        {
            form.CustCode(data.value);
            form.CustAbbr(data.text);
            form.Contact(data.Contact);
            form.Tel(data.Tel);
        }       
        else if (LookupType == 'SaleOrder'||LookupType == 'Shiping') {
            form.CustCode(data.value);
            form.Customer(data.text);
            form.Contact(data.Contact + ' ' + data.Tel);
        }
    };
    target.window(opt);
};

//弹出关联料号窗口
mms.com.relateMaterial = function (param) {
    var relateType = param.RelateType;
    var iframeWidth = 800;
    var titleName = '';

    titleName = relateType == 'RelateCustPN' ? '关联客户料号' : '关联代理商';
    iframeWidth = relateType == 'RelateCustPN' ? 800 : 1000;
    //移到視圖vm中驗證
    //var comapreArray = [];

    //if (relateType == 'RelateCustPN')
    //{ comapreArray = ['PNID','CustPN']; iframeWidth = 810; }
    //else if (relateType == 'RelateProxy')
    //{ comapreArray = ['PNID','SupplierCode']; iframeWidth = 830; }    
    //else
    //{ comapreArray = ['MaterialCode', 'SrcBillType', 'SrcBillNo']; }

    //param._xml == comapreArray;
    //var fnEqual = function (row1, row2) {
    //    for (var key in comapreArray)
    //        if (row1[comapreArray[key]] != row2[comapreArray[key]])
    //            return false;
    //    return true;
    //}
    //var fnExist = function (row) {
    //    for (var i in orgRows)
    //        if (fnEqual(orgRows[i], row))
    //            return true;
    //    return false;
    //};

    var target = parent.$('#relatePN').length ? parent.$('#relatePN') : parent.$('<div id="relatePN"></div>').appendTo('body');
    utils.clearIframe(target);
    
    var opt = { iconCls: 'icon-node_tree', title: titleName, width: iframeWidth, height: 520, modal: true, collapsible: false, minimizable: false, maximizable: true, closable: true };
    opt.content = "<iframe id='frm_win_relate' src='/mms/home/" + relateType+"/?para="+ko.toJSON(param) + "' style='height:100%;width:100%;border:0;' frameborder='0'></iframe>";  //frameborder="0" for ie7
    
    target.window(opt);
};

//弹出上传文件的窗口
mms.com.upload = function (param) {
    var BizID = param.BizID;
    var BizTable = param.BizTable;
    var iframeWidth = 640;
    var titleName = '';

    if (BizTable == "DG_PartNo")
        titleName = "物料附件";
    else if (BizTable == "DG_PartNoProxy")
        titleName = "物料规格书";
    else if (BizTable == "DG_PartNoCustomerPN")
        titleName = "物料确认书";
    else if (BizTable == "DG_RFQHeader")
        titleName = "供应商报价单";
    else if (BizTable == "DG_BuyHeader")
        titleName = "采购单";
    else
        titleName = "上传附件";

    iframeWidth = BizTable == 'DG_PartNo' ? 640 : 660;

    var target = parent.$('#Attach').length ? parent.$('#Attach') : parent.$('<div id="Attach"></div>').appendTo('body');
    utils.clearIframe(target);

    var opt = { iconCls: 'icon-upload', title: titleName, width: iframeWidth, height: 520, modal: true, collapsible: false, minimizable: false, maximizable: true, closable: true };
    opt.content = "<iframe id='frm_win_attach' src='/mms/home/UploadFile/?para=" + ko.toJSON(param) + "' style='height:100%;width:100%;border:0;' frameborder='0'></iframe>";  //frameborder="0" for ie7

    target.window(opt);
};

mms.com.ARAPDetail = function (param) {
    var BizID = param.BizID;
    var BizTable = param.BizTable;
    var iframeWidth = 640;
    var titleName = '',controller="";

    if (BizTable == "ARSub")
    { titleName = "应收明细"; controller = "ARList"; }
    else if (BizTable == "APSub")
    { titleName = "应付明细"; controller = "APList";  }
    else
        titleName = "";

    var target = parent.$('#ARAP').length ? parent.$('#ARAP') : parent.$('<div id="ARAP"></div>').appendTo('body');
    utils.clearIframe(target);

    var opt = { iconCls: 'icon-node_tree', title: titleName, width: iframeWidth, height: 430, modal: true, collapsible: false, minimizable: false, maximizable: true, closable: true };
    opt.content = "<iframe id='frm_win_arap' src='/mms/" + controller + "/Detail/" + BizID + "' style='height:100%;width:100%;border:0;' frameborder='0'></iframe>";  //frameborder="0" for ie7

    target.window(opt);
};
