
/*框架配置*/
var g_iFrame_Header_Height = 65;
var g_iFrame_MainLeft_Menu_Width = 180;
var g_iFrame_MainLeft_Splitter_Width = 7;
var g_iFrame_MainLeft_Width = g_iFrame_MainLeft_Menu_Width + g_iFrame_MainLeft_Splitter_Width;
var g_iFrame_Footer_Height = 22;
var g_iFrame_MainLeft_Menu_Show = false;

$(document).ready(function () {
    /*窗體大小調整*/
//    WindowMax();
    MainLeftMenuShow();
    $(window).resize(function () {
        WindowResize();
    });
    /*分隔條樣式控制*/
    $('#iframe_MainLeft_Splitter').click(function () {
        MainLeftMenuShow();
    });
});

/*最大化屏幕*/
function WindowMax() {
    self.moveTo(0, 0);
    self.resizeTo(screen.availWidth, screen.availHeight);
    WindowResize();
 }
/*窗體大小變化*/
 function WindowResize() {
     $('#iframe_Header').height(g_iFrame_Header_Height);
     $('#iframe_Main').height($(window).height() - g_iFrame_Header_Height - g_iFrame_Footer_Height);
     $('#iframe_MainLeft').width(g_iFrame_MainLeft_Width);
     $('#iframe_MainLeft_Menu').width(g_iFrame_MainLeft_Width - g_iFrame_MainLeft_Splitter_Width);
     $('#iframe_MainRight').width($('#iframe_Main').width() - g_iFrame_MainLeft_Width);
     $('#iframe_Footer').height(g_iFrame_Footer_Height);
}
/*左边菜单栏的展开与关闭*/
function MainLeftMenuShow() {
    var menuID = '#iframe_MainLeft_Menu';
    g_iFrame_MainLeft_Menu_Show = !g_iFrame_MainLeft_Menu_Show;
    if (g_iFrame_MainLeft_Menu_Show) {
        $(menuID).css("display", 'block');
        $('#iframe_MainLeft_Splitter').addClass('iframe_MainLeft_Splitter_Open');
        $('#iframe_MainLeft_Splitter').removeClass('iframe_MainLeft_Splitter_Close');
        //$('#iframe_MainLeft_Splitter').css("background-image", "url('/Content/themes/default/images/SplitterRight1.gif')");
        g_iFrame_MainLeft_Width = g_iFrame_MainLeft_Menu_Width + g_iFrame_MainLeft_Splitter_Width;
    }
    else {
        $(menuID).css("display", 'none');
        $('#iframe_MainLeft_Splitter').addClass('iframe_MainLeft_Splitter_Close');
        $('#iframe_MainLeft_Splitter').removeClass('iframe_MainLeft_Splitter_Open');
        //$('#iframe_MainLeft_Splitter').css("background-image", "url('/Content/themes/default/images/SplitterLeft1.gif')");
        g_iFrame_MainLeft_Width = g_iFrame_MainLeft_Splitter_Width;
    }
    WindowResize();
}