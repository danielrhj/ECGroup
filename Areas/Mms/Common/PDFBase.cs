using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ECGroup.Areas.Mms.Common
{
    public class PDFBase
    {
        //定义字体 颜色 用法：iTextSharp.text.Font font = BaseFontAndSize("黑体", 10, Font.NORMAL, BaseColor.BLACK); 



        public static Font BaseFontAndSize(string font_name, int size, int style, BaseColor baseColor)
        {
            BaseFont baseFont;
            BaseFont.AddToResourceSearch("iTextAsian.dll");
            BaseFont.AddToResourceSearch("iTextAsianCmaps.dll");
            Font font = null;
            string file_name = "";
            int fontStyle;
            switch (font_name)
            {
                case "黑体":
                    file_name = "SIMHEI.TTF";
                    break;
                case "华文中宋":
                    file_name = "STZHONGS.TTF";
                    break;
                case "宋体":
                    file_name = "SIMYOU.TTF";
                    break;
                case "新宋体":
                    file_name = "SIMSUN.TTC,0";
                    break;
                case "Times New Roman":
                    file_name = "times.ttf";
                    break;
                default:
                    file_name = "SIMYOU.TTF";
                    break;
            }
            baseFont = BaseFont.CreateFont(@"c:/windows/fonts/" + file_name, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);//字体：黑体 
            if (style < -1)
            {
                fontStyle = Font.NORMAL;
            }
            else
            {
                fontStyle = style;
            }
            font = new Font(baseFont, size, fontStyle, baseColor);
            return font;
        }

    }

    public class PDFPara
    {            
        public string fileName;
        public DataSet reportDataSet;
    }
}