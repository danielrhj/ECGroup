$(function () {
    using(['layout', 'datagrid', 'tree'], function () {
        //获取window信息
        var iframe = getThisIframe();
        var thiswin = parent.$(iframe).parent();
        var options = thiswin.window("options");
        var param = options.paramater;

        //初始化layout
        var box = $("#layoutbox"), right = $('#right').layout();
        box.width($(window).width()).height($(window).height()).layout();
        $(window).resize(function () { box.width($(window).width()).height($(window).height()).layout('resize'); });

        //调整layout时自动调整grid
        var panels = $('#right').data('layout').panels;
        panels.north.panel({
            onResize: function (w, h) {
                $('#list').datagrid('resize', { width: w, height: h - 38 });
            }
        });
        panels.center.panel({
            onResize: function (w, h) {
                $('#selectlist').datagrid('resize', { width: w, height: h - 36 });
            }
        });

        //设置grid列  , 适用于param.LookupType='LookupSuppPNForBuyOrder'  
        var cols = [[
                { title: '制造商料号', field: 'SuppPN', sortable: true, align: 'left', width: 200},
                { title: '品名', field: 'CDesc', sortable: true, align: 'left', width: 100 },
                { title: '規格', field: 'CSpec', sortable: true, align: 'left', width: 220 },
                { title: '品牌', field: 'Brand', sortable: true, align: 'left', width: 80 },
                { title: '采购单余量', field: 'RemainNum', sortable: true, align: 'right', width: 70 },
                { title: '最新采购价', field: 'BuyPrice', sortable: true, align: 'right', width: 80 }
        ]];
        if (param.LookupType == 'LookupSuppPNForRFQ') //RFQ
        {
            var cols = [[
                    { title: '制造商料号', field: 'SuppPN', sortable: true, align: 'left', width: 200 },
                    { title: '客户料号', field: 'CustPN', sortable: true, align: 'left', width: 200 },
                    { title: '品名', field: 'CDesc', sortable: true, align: 'left', width: 100 },
                    { title: '規格', field: 'CSpec', sortable: true, align: 'left', width: 220 },
                    { title: '品牌', field: 'Brand', sortable: true, align: 'left', width: 80 },
                    { title: 'MOQ', field: 'MOQ', sortable: true, align: 'right', width: 60 },
                    { title: 'SPQ', field: 'MinQty', sortable: true, align: 'right', width: 60 },
                    { title: 'SPQ单位', field: 'MinQtyUnit', sortable: true, align: 'center', width: 60 },
                    { title: 'LT(d)', field: 'LeadTime', sortable: true, align: 'center', width: 60 },
                    { title: '最新采购价', field: 'BuyPrice', sortable: true, align: 'right', width: 80 }
            ]];
            $("#CustPNTitle").attr('style', 'display:block'); $("#divCustPNValue").attr('style', 'display:block');
        }

        if (param.LookupType == 'LookupSuppPNForReceiving') //入库单新增明细
        {
            cols[0].unshift({ title: '代理商', field: 'SuppAbbr', sortable: true, align: 'left', width: 60 });
            cols[0].unshift({ title: '采购单号', field: 'BuyNo', sortable: true, align: 'left', width: 100 });
            cols[0].pop();
            cols[0].push({ title: '采购单价', field: 'BuyPrice', sortable: true, align: 'right', width: 60 });
            cols[0].push({ title: '税率', field: 'TaxRate', sortable: true, align: 'right', width: 60 });
            $("#divHasBuyNo").attr('style', 'display:block'); document.getElementById("divHasBuyNo").checked=true;
        }

        if (param.LookupType == 'LookupCustPNForSaleQuote' || param.LookupType == 'LookupCustPNForSaleOrder') {       //销售订单新增明细
            //cols[0].pop();  //删除数组最后一个元素并返回新的数组
            cols[0].unshift({ title: '客户料号', field: 'CustPN', sortable: true, align: 'left', width: 200 });
            $("#CustPNTitle").attr('style', 'display:block'); $("#divCustPNValue").attr('style', 'display:block');
            $("#textTitle").html('品名');
            cols[0].pop();
            cols[0].pop();

            if (param.LookupType == 'LookupCustPNForSaleQuote')
            {
                cols[0].push({ title: 'MOQ', field: 'MOQ', sortable: true, align: 'right', width: 60 });
                cols[0].push({ title: 'SPQ', field: 'SPQ', sortable: true, align: 'right', width: 60 });
                cols[0].push({ title: 'LT(d)', field: 'LeadTime', sortable: true, align: 'center', width: 60 });
                cols[0].push({ title: '最新报价', field: 'ReplyPrice', sortable: true, align: 'right', width: 80 });
            }
            if (param.LookupType == 'LookupCustPNForSaleOrder') {
                cols[0].push({ title: '最新报价', field: 'UnitPrice', sortable: true, align: 'right', width: 80 });
            }
        }

        if (param.LookupType == 'LookupCustPNForShiping') {     //出库单新增明细
            cols[0] = [{ title: 'PO', field: 'PO', sortable: true, align: 'left', width: 80 },
                //{ title: '客户', field: 'Customer', sortable: true, align: 'left', width: 100 },
            { title: '制造商料号', field: 'SuppPN', sortable: true, align: 'left', width: 150 },
            { title: '客户料号', field: 'CustPN', sortable: true, align: 'left', width: 150 },
            { title: '品名', field: 'CDesc', sortable: true, align: 'left', width: 100 },
            { title: '規格', field: 'CSpec', sortable: true, align: 'left', width: 220 },
            { title: 'PO单价', field: 'UnitPrice', sortable: true, align: 'right', width: 80 },
            { title: 'PO数量', field: 'OrderQty', sortable: true, align: 'right', width: 60 },
            { title: 'PO已出', field: 'ShipQty', sortable: true, align: 'right', width: 60 },
            { title: 'PO剩余', field: 'OrderRemain', sortable: true, align: 'right', width: 60 },
            { title: '库存剩余', field: 'WHQty', sortable: true, align: 'right', width: 60 },            
            { title: '入库单价', field: 'RcvPrice', sortable: true, align: 'right', width: 80 },
            { title: '入库税率', field: 'RcvTaxRate', sortable: true, align: 'right', width: 50 }];

            $("#CustPNTitle").attr('style', 'display:block'); $("#divCustPNValue").attr('style', 'display:block');
            $("#PO").attr('style', 'display:block'); $("#divPO").attr('style', 'display:block');

            //PO,CustPN,SuppPN,CDesc,CSpec,OrderQty,ShipQty,OrderRemain,WHQty
        }

        if (param._xml == "mms.material_check") {
            cols[0][3] = { title: '账面数量', field: 'BookNum', sortable: true, align: 'right', width: 60 };
        }

        if (param._xml == "mms.material_batches" || param._xml == "mms.material_send" || param._xml == "mms.material_rentin") {
            cols[0].push({ title: '来源类型', field: 'SrcBillType', sortable: true, align: 'left', width: 60 ,formatter:mms.com.formatSrcBillType});
            cols[0].push({ title: '来源单号', field: 'SrcBillNo', sortable: true, align: 'left', width: 85 });
        }

        //定义返回值
        var lookType=['LookupSuppPNForRFQ','LookupSuppPNForBuyOrder','LookupSuppPNForReceiving','LookupCustPNForSaleQuote','LookupCustPNForSaleOrder','LookupCustPNForShiping'];
        var selected = { total: 0, rows: [] };
        var grid1 = $('#list');
        var grid2 = $('#selectlist');

        var defaults = {
            iconCls: 'icon icon-list',
            nowrap: true,           //折行
            rownumbers: true,       //行号
            striped: true,          //隔行变色
            singleSelect: true,     //单选
            remoteSort: true,       //后台排序
            pagination: false,      //翻页
            pageSize: com.getSetting("gridrows", 20),
            contentType: "application/json",
            method: "GET"
        };

        //设置明细表格的属性
        var opt = $.extend({},defaults,{
            height: 315,
            pagination: true,
            url: '/api/mms/home/GetMaterial',
            queryParams: param,
            pageSize: 10,
            columns: cols,
            onDblClickRow: function (index, row) {
                if (row.RemainNum == 0 && param.LookupType == 'LookupSuppPNForReceiving') { com.message('warning', '采购单收货余量为0,不能继续收货!'); return; }

                if (row.WHQty * row.OrderRemain == 0 && param.LookupType == 'LookupCustPNForShiping') { com.message('warning', '客户订单余量或库存为0,不能出货!'); return; }

                for (var i in selected.rows) {
                    if (param.LookupType == 'LookupSuppPNForReceiving' && row.SuppPN == selected.rows[i].SuppPN && row.BuyNo == selected.rows[i].BuyNo) {
                        grid2.datagrid('selectRow', i);
                        return;
                    }
                    else if (row.SuppPN == selected.rows[i].SuppPN && param.LookupType.indexOf('LookupSuppPN') > -1 && param.LookupType != 'LookupSuppPNForReceiving') {
                        grid2.datagrid('selectRow', i);
                        return;
                    }
                    else if (row.CustPN == selected.rows[i].CustPN && param.LookupType.indexOf('LookupCustPN') > -1 && param.LookupType != 'LookupCustPNForShiping') {
                        grid2.datagrid('selectRow', i);
                        return;
                    }
                }
                selected.total = selected.rows.push(row);
                grid2.datagrid('loadData', selected);
                grid2.datagrid('selectRow', grid2.datagrid('getRowIndex', row));
                $('#total').html(selected.total);
            }
        });

        //已选择的grid
        var opt2 = $.extend({}, defaults, {
            pagination: false,
            remoteSort: false,
            columns: cols,
            height: panels.center.panel('options').height - 36,
            onDblClickRow: function (index, row) {
                for (var i in selected.rows) {
                    if (row.SuppPN == selected.rows[i].SuppPN) {
                        selected.rows.pop(row);
                        selected.total -= 1;
                        grid2.datagrid('loadData', selected);
                        $('#total').html(selected.total);
                        break;
                    }
                }
            }
        });

        grid2.datagrid(opt2).datagrid('loaded');
        grid1.datagrid(opt);

        var typeid = '';
        //var clickType = function (node) {
        //    typeid = node.id;
        //    search();
        //};

        //此處擴展查詢參數
        var search = function () {
            var queryParams = $.extend({},param,{
                MaterialType: typeid,
                SuppPN: $('#SuppPN').val(),
                CDesc: $('#CDesc').val(),
                CustPN: $('#CustPNValue').val(),
                PO: $('#CustPOValue').val(),
                BuyNoFlag: $("#chkHasBuyNo")[0].checked?"Y":"N"
            });
            com.setFirstPageWhenSearchGrid(grid1);
            grid1.datagrid('reload', queryParams);
        };

        var paramStr="";
        for (var key in param)
            paramStr += (paramStr ? "&" : "?") + key + "=" + param[key];

        $('#btnSearch').click(search);
        $('#btnClear').click(function () { $('#master').find("input").val(""); search(); });
        //$('#typetree').tree({
        //    method:'GET',
        //    url: '/api/mms/home/GetMaterialType' + paramStr,
        //    onClick: clickType,
        //    loadFilter:function(d){
        //        var data = utils.toTreeData(d.rows || d, 'id', 'pid', "children");
        //        return [{ id: '', text: '所有类别', children: data }];
        //    },
        //});
  
        $('#btnConfirm').click(function () {
            options.onSelect(selected);
            destroyIframe(iframe);
            thiswin.window('destroy');
        });

        $('#btnCancel').click(function () {
            destroyIframe(iframe);
            thiswin.window('destroy');
        });
    });
});

function getThisIframe() {
    if (!parent) return null;
    var iframes = parent.document.getElementsByTagName('iframe');
    if (iframes.length == 0) return null;
    for (var i = 0; i < iframes.length; ++i) {
        var iframe = iframes[i];
        if (iframe.contentWindow === self) {
            return iframe;
        }
    }
    return null;
}

function destroyIframe(iframeEl) {
    if (!iframeEl) return;
    iframeEl.parentNode.removeChild(iframeEl);
    iframeEl = null;
};