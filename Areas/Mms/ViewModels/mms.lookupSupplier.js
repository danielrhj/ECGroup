$(function () {
    using(['layout', 'datagrid', 'tree'], function () {
        //获取window信息
        var iframe = getThisIframe();
        var thiswin = parent.$(iframe).parent();
        var options = thiswin.window("options");
        var param = options.paramater;
        var LookupType = param.type;
        $('#txtValue').val(param.SuppCode);
        $('#txtText').val(param.SuppAbbr);

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
        //设置grid列  , @*select SuppID,SuppAbbr,SuppCode,Contact,Tel from DG_Supplier*@
        var cols = [[
                { title: '代码', field: 'value', sortable: true, align: 'left', width: 50 },
                { title: '简称', field: 'text', sortable: true, align: 'left', width: 60 },
                { title: '全称', field: 'SuppName', sortable: true, align: 'left', width: 210 },
                { title: '联系人', field: 'Contact', sortable: true, align: 'left', width: 120 },
                { title: '电话', field: 'Tel', sortable: true, align: 'left', width: 125 },
        ]];        

        //定义返回值
        var lookType=['RFQ','BuyOrder','Receive','SaleQuote','SaleOrder','Shiping'];
        var selected = {};
        var grid1 = $('#list');

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
            height: 422,
            pagination: true,
            url: '/api/mms/home/getSupplierInfoList',
            queryParams: param,
            pageSize: 10,
            columns: cols,
            onDblClickRow: function (index, row) {
                selected = row;
                $('#btnConfirm').click();
            },
            onClickRow: function (index, row) {
                selected = row;
            }
        });

        grid1.datagrid(opt);

        //此處擴展查詢參數
        var search = function () {
            var queryParams = $.extend({},param,{
                SuppCode: $('#txtValue').val(),
                SuppAbbr: $('#txtText').val()
            });

            grid1.datagrid('reload', queryParams);
        };

        var paramStr="";
        for (var key in param)
            paramStr += (paramStr ? "&" : "?") + key + "=" + param[key];

        $('#btnSearch').click(search);
        $('#btnClear').click(function () { $('#master').find("input").val(""); search(); });        
  
        $('#btnConfirm').click(function () {
            var msg = (LookupType == 'SaleQuote' || LookupType == 'SaleOrder' || LookupType == 'Shiping') ? '客户' : '代理商';
            if (selected.value == undefined) { com.message('warning', '选择一条'+msg+'的资料.'); return; }
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