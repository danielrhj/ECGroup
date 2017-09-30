/********************************
author: tiger
function: change table style.
********************************/
//所有Jquery的控件的第一步都是搭这个架子，兼容JQuery和$避免闭包，避免和其他类库冲突，接受一个参数（是个对象）
(function ($) {
    //也可以使用$.fn.extend(mytable:function(setting){}) 
    // 给控件加一些参数默认参数，同时能调用方法$.extend让最终调用时的参数覆盖默认的（如果没有则使用默认）
    $.fn.mytable = function (settings) {
        var dfop =
            {
                datatype: "json"
            };
        $.extend(dfop, settings);
        var me = $(this);
        var id = me.attr("id");
        if (id == null || id == "") {
            id = "myTable" + new Date().getTime();
            me.attr("id", id);
        }
        var meRowCount = me.find('tr').size();
        var meEven = me.find('td:even');
        var meOdd = me.find('td:odd');
        var widthTable = me.width();
        var widthEven = 90;
        var widthOdd = (widthTable - meEven.size() * widthEven / meRowCount) / meOdd.size() * meRowCount;
        meEven.width(widthEven);
        meOdd.width(widthOdd); //$(".ctrl_list td:odd").width(200);       
    }
})(jQuery);