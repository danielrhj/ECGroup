using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.IO;
using System.Collections;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Zephyr.Utils;
using System.Text;
using System.Threading;


namespace ECGroup.Models
{
    /// <summary>
    ///Daniel 2017/05/18,源自NPOI2011
    ///利用EPPlus導出及讀入Excel資料
    /// </summary>
    public class EPPlusNew
    {
        private ExcelPackage _myExcelWB;
        private ExcelWorksheet _myExcelST;
        private ExcelStyles myStyle;
        private ExcelStyle myGridStyle;
        private ExcelFont myFont2;

        public ExcelPackage myExcelWBA;
        public ExcelWorksheet myExcelST;

        private int _intST=0;
        public int intST
        {
            get
            {
                _intST = _myExcelWB.Workbook.View.ActiveTab;
                return _intST; 
            }
            set
            {
                _intST = value;
                _myExcelST = _myExcelWB.Workbook.Worksheets[_intST];
                _myExcelWB.Workbook.Worksheets[_intST].Select("A1");
                myExcelST = _myExcelST;
                //return _intST;

            }
        }

        

        private bool _AutoNoFlag=false;
        public bool AutoNoFlag
        {
            get { return _AutoNoFlag; }
            set { _AutoNoFlag = value; }
        }

        public EPPlusNew(string strPath)
        {
            if (strPath == "")
            {
                _myExcelWB = new ExcelPackage();        ///如果strPath="",表示创建一个空的WorkBook对象
                _myExcelST =_myExcelWB.Workbook.Worksheets.Add("sheet1");
            }

            else
            {
                _myExcelWB = new ExcelPackage(new FileStream(strPath, FileMode.Open, FileAccess.Read));                
                _myExcelST = _myExcelWB.Workbook.Worksheets[1];
            }

            _myExcelWB.Workbook.Properties.Company = "EPPlus";
            _myExcelWB.Workbook.Properties.Subject = "EPPlus Sample";  

            myExcelST = _myExcelST;
            myExcelWBA = _myExcelWB;

            ExcelStyle myStyle = _myExcelWB.Workbook.Styles.CreateNamedStyle("GeneralStyle").Style;
            myStyle.Fill.PatternType=ExcelFillStyle.LightGray;
            myStyle.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);  // HSSFColor.AUTOMATIC.BLUE.index;

            myFont2 = myStyle.Font;
            myFont2.Italic = true;
            myFont2.Name = "Times New Roman";
            myFont2.Size = 9;
            myFont2.Color.SetColor(System.Drawing.Color.Blue);


        }

        public void CreateSheets(int intCount)
        {
            if (intCount > 1)
            {
                for (int i = 1; i < intCount; i++)
                {
                    if (_myExcelWB.Workbook.Worksheets["ST" + i.ToString()].Index<=0)
                    _myExcelWB.Workbook.Worksheets.Add("ST"+i.ToString());
                }               
            }
        }
        public Boolean SaveXls(string strPath)
        {
            try
            {
                FileStream file = new FileStream(strPath, FileMode.Create);
                _myExcelWB.SaveAs(file);
                
                file.Close(); file = null;

                return true;
            }
            catch (Exception ex)
            { return false; }

        }

        public ExcelWorksheet GetSheetByIndex(int SheetIndex)
        {
            return _myExcelWB.Workbook.Worksheets[SheetIndex];    
        }

        public string GetCellValue(string strCell,string DataType)
        {           
            ExcelCellAddress myCR = new ExcelCellAddress(strCell);
            
            object myCell = _myExcelST.GetValue(myCR.Row,myCR.Column);

            try
            {
                if (myCell == null) { return ""; }

                if (DataType.ToLower() == "datetime")
                {
                    return _myExcelST.Cells[strCell].GetValue<DateTime>().ToString("yyyy/MM/dd");                    
                }
                else if (DataType.ToLower() == "numeric")
                {
                    return _myExcelST.Cells[strCell].GetValue<Double>().ToString();
                }

                else
                {
                    return _myExcelST.Cells[strCell].Value.ToString().Trim();
                }
            }
            catch { return ""; }

        }

        public string GetCellValue(string strCell)
        {
            try
            {
                string myCell = _myExcelST.Cells[strCell].GetValue<String>().ToString();
                return myCell = string.IsNullOrEmpty(myCell)? "" : myCell.ToString().Trim();
            }
            catch
            { return ""; }

        }

        public void AddLink(string strCell, string URL)
        {            
            ExcelCellAddress myCR = new ExcelCellAddress(strCell);
            object myCell = _myExcelST.GetValue(myCR.Row, myCR.Column);

            int cellstyleNow = _myExcelST.Cells[strCell].Style.XfId;
            _myExcelST.Cells[strCell].Hyperlink=new ExcelHyperLink(URL,myCell.ToString());            
            
            _myExcelST.Cells[strCell].StyleID = cellstyleNow;           //此处需要保留原有样式，但没有成功

        }

        public void ExportExcelWithCaption(DataTable dt, string strCaption)
        {
            try
            {
                WriteExcelBody(dt);              
                int KK = _myExcelST.Dimension.End.Column;
                _myExcelST.InsertRow(1,1);
                _myExcelST.Cells[1, 1, 1, KK].Merge = true;
                _myExcelST.Cells[1, 1, 1, KK].Style.HorizontalAlignment=ExcelHorizontalAlignment.Center;
                _myExcelST.SetValue(1,1,strCaption);                

                HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                HttpContext.Current.Response.AddHeader("content-disposition", "attachment;  filename=myExcel.xlsx");
                HttpContext.Current.Response.BinaryWrite(_myExcelWB.GetAsByteArray());

            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write(ex.Message.ToString());
            }   
        }

        //public void OpenExcelFile()
        //{
        //    MemoryStream myMS = new MemoryStream();
        //    _myExcelWB.SaveAs(myMS);

        //    HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment;filename=myExcel.xls"));
        //    HttpContext.Current.Response.BinaryWrite(myMS.GetBuffer());

        //    myMS = null;
        
        //}

        public void OpenExcelFile(string strPath)
        {
            FileInfo FI = new FileInfo(strPath);
            _myExcelWB=new ExcelPackage(FI);

            HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment;filename=" + FI.Name + ""));
            HttpContext.Current.Response.BinaryWrite(_myExcelWB.GetAsByteArray());

            FI = null;
        }

        public bool ExportExcel(DataTable dt, string strPath)
        {
            WriteExcelBody(dt);
            Boolean bk= SaveXls(strPath);

            return bk;
        }

        private void WriteExcelBody(DataTable dt)
        {
            int i, j;
            Hashtable myHT = new Hashtable();
            double myInt;
            string strDataItem;
            DateTime myDT;       

            ExcelStyle myStyle= _myExcelWB.Workbook.Styles.CreateNamedStyle("bodyStyle").Style;

            myStyle.Fill.PatternType = ExcelFillStyle.Solid;            
            myStyle.Fill.BackgroundColor.SetColor(Color.Aqua);

            ExcelFont myFont = myStyle.Font;
            myFont.Italic = true;
            myFont.Name = "Times New Roman";
            myFont.Size = 10;
            myFont.Bold = true;
            myFont.Color.SetColor(Color.Blue);
            myGridStyle = _myExcelWB.Workbook.Styles.CreateNamedStyle("GridStyle").Style;

            ExcelStyle cellStyle7 = _myExcelWB.Workbook.Styles.CreateNamedStyle("cellStyle7").Style;
            cellStyle7.Numberformat.Format = "yyyy/MM/dd";

            ExcelStyle cellStyle8 = _myExcelWB.Workbook.Styles.CreateNamedStyle("cellStyle8").Style;
            cellStyle8.Numberformat.Format = "HH:mm";

            if (_AutoNoFlag)
            {
                dt.Columns.Add("NO", Type.GetType("System.String"));
                dt.Columns["NO"].SetOrdinal(0);
            }
            
            for (j = 0; j < dt.Columns.Count; j++)
            {
                _myExcelST.Cells[1, j + 1].Value = dt.Columns[j].ColumnName.ToString();
                _myExcelST.Cells[1, j + 1].StyleName = "bodyStyle";//未驗證的寫法：_myExcelST.Cells[1, j + 1].StyleID = myStyle.XfId;
                _myExcelST.Cells[1, j + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (_AutoNoFlag) { dt.Rows[i]["NO"] = (i + 1).ToString(); }

                for (j = 0; j < dt.Columns.Count; j++)
                {
                    ExcelCellAddress myCell = new ExcelCellAddress(i+2,j+1);
                    string CellAddress = myCell.Address;
                    strDataItem = dt.Rows[i][j].ToString();

                    if (DateTime.TryParse(strDataItem, out myDT))
                    {
                        if (strDataItem.Contains('/') || strDataItem.Contains('-'))
                        {
                            _myExcelST.Cells[CellAddress].Value = Convert.ToDateTime(strDataItem);
                            _myExcelST.Cells[CellAddress].StyleName = "cellStyle7";

                            if (!myHT.ContainsKey(myCell.Column.ToString())) { myHT.Add(myCell.Column.ToString(), 1); }
                        }
                        else if (!strDataItem.Contains('/') && !strDataItem.Contains('-') && strDataItem.Contains(':') && strDataItem.Length == 5)
                        {
                            _myExcelST.Cells[CellAddress].Value = strDataItem;
                            _myExcelST.Cells[CellAddress].StyleName = "cellStyle8";
                            if (!myHT.ContainsKey(myCell.Column.ToString())) { myHT.Add(myCell.Column.ToString(), 1); }
                        }
                        else
                        {
                            _myExcelST.Cells[CellAddress].StyleName = "GridStyle";
                            _myExcelST.Cells[CellAddress].Value = Convert.ToDouble(dt.Rows[i][j].ToString());
                        }
                        
                    }
                    else if (double.TryParse(strDataItem, out myInt))
                    {
                        if(myInt.ToString().Contains("E+")||strDataItem.StartsWith("0"))
                        {
                            _myExcelST.Cells[CellAddress].StyleName = "GridStyle";
                            _myExcelST.Cells[CellAddress].Value = dt.Rows[i][j].ToString();
                        }
                        else{
                            _myExcelST.Cells[CellAddress].StyleName = "GridStyle";
                            _myExcelST.Cells[CellAddress].Value = Convert.ToDouble(dt.Rows[i][j].ToString());       }                 
                    }
                    else
                    {
                        _myExcelST.Cells[CellAddress].StyleName = "GridStyle";
                        _myExcelST.Cells[CellAddress].Value = dt.Rows[i][j].ToString();
                    }
                    _myExcelST.Cells[CellAddress].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }
           
            for (j = 0; j < dt.Columns.Count; j++)
                { _myExcelST.Column(j+1).AutoFit();}

            if (myHT.Count > 0)
            {
                for (j = 0; j < dt.Columns.Count; j++)
                {
                    if (myHT.ContainsKey(j))
                    {
                        if (_myExcelST.Column(j+1).Width < 3000)
                        { _myExcelST.Column(j+1).Width=3000; }

                    }
                }
            }
        }

        public string ExportExcel(DataTable dt)
        {
            try
            {
                string filename=Guid.NewGuid().ToString()+".xlsx";
                string path=Zephyr.Utils.ZHttp.RootPhysicalPath+@"FJLBS\TempFiles\"+filename;

                WriteExcelBody(dt);
                SaveXls(path);
                return "/FJLBS/TempFiles/" + filename;
                //HttpContext.Current.Response.BinaryWrite(_myExcelWB.GetAsByteArray());
                //_myExcelWB.SaveAs(HttpContext.Current.Response.OutputStream);
                //HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //HttpContext.Current.Response.AddHeader("Content-Disposition","attachment;filename=myExcel01.xlsx");

                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=Sample2.xlsx");
                ////HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                return "";
                //HttpContext.Current.Response.Write(ex.Message.ToString());
            }            
            
        }

        public Stream ExportExcelNew(DataTable dt)
        {
            try
            {
                string filename = Guid.NewGuid().ToString() + ".xlsx";
                string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

                WriteExcelBody(dt);
                //SaveXls(path);
                //return "/FJLBS/TempFiles/" + filename;
                HttpContext.Current.Response.ClearHeaders();
                return _myExcelWB.Stream;
                //HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=myExcel01.xlsx");
                ////HttpContext.Current.Response.BinaryWrite(_myExcelWB.GetAsByteArray());
                //HttpContext.Current.Response.Flush();

               // DownloadFile(HttpContext.Current, _myExcelWB.Stream, "myExcel01.xls", 1024 * 1024 * 10);
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=Sample2.xlsx");
                ////HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write(ex.Message.ToString());
                return null;
            }

        }

        public string ExportExcel(DataSet ds,string FileName)
        {
            try
            {
                string filenameBK = Guid.NewGuid().ToString() + ".xlsx";
                string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + FileName;
                try
                {
                    File.Delete(path);
                }
                catch (Exception)
                {
                    FileName = filenameBK;
                    path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filenameBK;
                }
                int i = 0, dtCount = ds.Tables.Count;
                if (_myExcelWB.Workbook.Worksheets.Count == 0)
                {
                    for (int k = 0; k < dtCount; k++)
                    { _myExcelWB.Workbook.Worksheets.Add("ST" + (i + 1).ToString()); }
                }  

                foreach (DataTable dt in ds.Tables)
                {
                    _myExcelST = _myExcelWB.Workbook.Worksheets[i];
                    WriteExcelBody(dt);
                    i++;
                }      

                SaveXls(path);
                return "/FJLBS/TempFiles/" + FileName;
                //HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment;filename="+FileName));
                //HttpContext.Current.Response.BinaryWrite(_myExcelWB.GetAsByteArray());

            }
            catch (Exception ex)
            {
                return "";
                //HttpContext.Current.Response.Write(ex.Message.ToString());
            }

        }
       
        public void SetRangeValue(string strRegion, string strValue, string strFormat)
        {
            switch (strFormat)
            {
                case "yyyy/MM/dd":
                case "yyyy/M/d":
                case "yy/MM/dd":
                case "MM/dd":
                    { _myExcelST.Cells[strRegion].Value=Convert.ToDateTime(strValue); break; }
                case "#,#.00":
                case "#,#.0":
                    { _myExcelST.Cells[strRegion].Value=Convert.ToDouble(strValue); break; }
                case "#,#":
                    { _myExcelST.Cells[strRegion].Value=Convert.ToInt16(strValue); break; }
                case "":
                    { _myExcelST.Cells[strRegion].Value=strValue; break; }
                default: break;
            }

            if (strFormat != "")
            {
                _myExcelST.Cells[strRegion].Style.Numberformat.Format = strFormat;

            }
        }

        public void SetRangeValue(string strRegion, string strValue)
        {
            SetRangeValue(strRegion, strValue, "");
        }

        public void SetRangeValue(string strRegion,int RowOffset,int ColOffset, string strValue,string strFormat)
        {
            string strNewCell = RangeOffset(strRegion, RowOffset, ColOffset);
            SetRangeValue(strNewCell, strValue, strFormat);
        }

        public string RangeOffset(string pCell, int intRow, int intCol,string DataType)
        {
            string strCell = RangeOffsetAddress(pCell,intRow,intCol);
            return GetCellValue(strCell,DataType);  //返回偏移后的地址内容
        }

        public string RangeOffset(string pCell, int intRow, int intCol)
        {           
            string strCell = RangeOffsetAddress(pCell, intRow, intCol);
            return GetCellValue(strCell);  //返回偏移后的地址内容

        }

        public string RangeOffsetAddress(string pCell, int intRow, int intCol)
        {
            string strCell = _myExcelST.Cells[pCell].Offset(intRow, intCol).Address;           
            return strCell;  //返回偏移后的地址字符串

        }

        public void InsertRows(int fromRowIndex, int rowCount)
        {
            //int rowIndex, colIndex;
            //IRow rowSource, rowInsert;
            //ICell cellSource, cellInsert;

            _myExcelST.InsertRow(fromRowIndex, rowCount,fromRowIndex);

            //for (rowIndex = fromRowIndex; rowIndex < fromRowIndex + rowCount; rowIndex++)
            //{
            //    rowSource = _myExcelST.GetRow(rowIndex + rowCount);
            //    rowInsert = _myExcelST.CreateRow(rowIndex);
            //    rowInsert.Height = rowSource.Height;
            //    for (colIndex = 0; colIndex < rowSource.LastCellNum + 1; colIndex++)
            //    {
            //        cellSource = rowSource.GetCell(colIndex);
            //        cellInsert = rowInsert.CreateCell(colIndex);

            //        if (cellSource != null)
            //        { cellInsert.CellStyle = cellSource.CellStyle; }
            //    }
            //}
        }

        public int MergeRegion(string strCell1, string strCell2, ref ExcelStyle myCellStyle)
        {
            //ExcelCellAddress FromCell = new ExcelCellAddress(strCell1);
            //ExcelCellAddress ToCell = new ExcelCellAddress(strCell2);

            _myExcelST.Cells[strCell1 + ":" + strCell2].Merge = true;
            _myExcelST.Cells[strCell1 + ":" + strCell2].StyleID = myCellStyle.XfId;
            //CellReference cellRef1 = new CellReference(strCell1);
            //CellReference cellRef2 = new CellReference(strCell2);
            //int KK = 0;

            //CellRangeAddress region = new CellRangeAddress(cellRef1.Row, cellRef1.Col, cellRef2.Row, cellRef2.Col);
            //KK = _myExcelST.AddMergedRegion(region);

            //int i, j;
            //for (i = cellRef1.Row; i < cellRef2.Row + 1; i++)
            //{
            //    for (j = cellRef1.Col; j < cellRef2.Col + 1; j++)
            //    {
            //        try
            //        { _myExcelST.GetRow(i).GetCell(j).CellStyle = myCellStyle; }
            //        catch (Exception ex)
            //        { _myExcelST.CreateRow(i).CreateCell(j).CellStyle = myCellStyle; }
            //    }
            //}

            return 0;
        }

        //public void GridRegion(int KK)
        //{
        //    CellRangeAddress myRegion = _myExcelST.GetMergedRegion(KK);
        //    int intRow1, intCol1, intRow2, intCol2;

        //    intRow1 = myRegion.FirstRow; intCol1 = myRegion.FirstColumn;
        //    intRow2 = myRegion.LastRow; intCol2 = myRegion.LastColumn;

        //    AddGridLines(intRow1, intCol1, intRow2, intCol2);
        //}

        public void GridRegion(string strCell1, string strCell2)
        {            
            _myExcelST.Cells[strCell1 + ":" + strCell2].StyleName = "GridStyle";
        }

        private void AddGridLines(int intRow1, int intCol1, int intRow2, int intCol2)
        {
            _myExcelST.Cells[intRow1,intCol1,intRow2,intCol2].StyleName = "GridStyle";
        }

        private void AddGridLines()
        {
            string Cell1 = _myExcelST.Dimension.Start.Address;
            string Cell2 = _myExcelST.Dimension.End.Address;          
            GridRegion(Cell1, Cell2);
            

        }
        #region 上傳Excel文件 獲取DataTabale
        /// <summary>
        /// 上傳Excel文件 獲取DataTabale
        /// </summary>
        /// <param name="file"></param>
        /// <param name="startCell">起始位置</param>
        /// <param name="ReadHeader">是否需要讀取單頭</param>
        /// <returns></returns>
        public DataTable GetDataTable(System.Web.UI.WebControls.FileUpload file)
        {
            if (file.HasFile)
            {
                _myExcelWB = new ExcelPackage(file.FileContent);
                _myExcelST = _myExcelWB.Workbook.Worksheets[1];
                int rowCount = _myExcelST.Dimension.End.Row;
                object CellValue = "";string format = "";
                DateTime myDT;            
                DataTable dt = new DataTable();
                if (_myExcelST != null && rowCount > 0)
                {
                    ExcelRow headerRow = _myExcelST.Row(0);
                    int cellCount = _myExcelST.Dimension.End.Column;
                    if (headerRow != null && cellCount > 0)
                    {
                        //表頭
                        for (int i = 0; i < cellCount; i++)
                        {
                            string ColName = _myExcelST.Cells[1, i + 1].Value.ToString().Replace(" ", "");
                            DataColumn column = new DataColumn(ColName);
                            dt.Columns.Add(column);
                        }

                        //表身
                        for (int i = 1; i <= rowCount; i++)
                        {
                            DataRow dataRow = dt.NewRow();
                            for (int j = 1; j <= cellCount; j++)
                            {
                                CellValue = _myExcelST.Cells[i, j].Value;
                                format = _myExcelST.Cells[i, j].Style.Numberformat.Format;
                                try
                                {
                                    if (format.Contains("#") || format.Contains("0"))
                                    {dataRow[j] = double.Parse(CellValue.ToString());}
                                    else if (format.Contains("Y") || format.Contains("M") || format.Contains("d"))
                                    {
                                        if (DateTime.TryParse(CellValue.ToString(), out myDT))
                                        {dataRow[j] =(myDT.Year > 1900 && myDT.Year < 2020)?myDT.ToString("yyyy/MM/dd HH:mm"):CellValue.ToString();}
                                    }
                                    else
                                    {dataRow[j] = CellValue.ToString();}
                                }
                                catch
                                {dataRow[j] = CellValue.ToString();}
                            }
                            dt.Rows.Add(dataRow);
                        }
                    }
                    _myExcelWB = null;
                    _myExcelST = null;
                    return dt;
                }
                else {return null; }
            }
            else
            {return null;}
        }

        //public DataTable GetDataTable(System.Web.UI.WebControls.FileUpload file, string StartCell)
        //{
        //    int rowCount, cellCount;
        //    if (file.HasFile)
        //    {
        //       try
        //       {
        //        _myExcelWB = new ExcelPackage(file.FileContent);
        //        _myExcelST = _myExcelWB.Workbook.Worksheets[1];}
        //       catch{return null;}
               
        //        CellReference myCell = new CellReference(StartCell);

        //        if (_myExcelST != null)
        //        { rowCount = _myExcelST.Dimension.End.Row; }
        //        else { return null; }

        //        IRow headerRow = _myExcelST.GetRow(myCell.Row);

        //        if (headerRow != null)
        //        {
        //            cellCount = headerRow.LastCellNum;
        //        }
        //        else { return null; }

        //        DataTable dt = new DataTable();
        //        if (_myExcelST != null && rowCount > 0)
        //        {  
        //            if (headerRow != null && cellCount > 0)
        //            {
        //                //表頭
        //                for (int i = headerRow.FirstCellNum; i < cellCount; i++)
        //                {
        //                    DataColumn column = new DataColumn("Col"+i.ToString());
        //                    dt.Columns.Add(column);
        //                }

        //                //表身
        //                for (int i = (headerRow.RowNum + 1); i <= _myExcelST.LastRowNum; i++)
        //                {
        //                    HSSFRow row = (HSSFRow)_myExcelST.GetRow(i);
        //                    DataRow dataRow = dt.NewRow();

        //                    for (int j = row.FirstCellNum; j < cellCount; j++)
        //                    {
        //                        if (row.GetCell(j) != null)
        //                        {
        //                            try
        //                            {
        //                                if (row.GetCell(j).CellType == CellType.NUMERIC)
        //                                {
        //                                    DateTime myDT = row.GetCell(j).DateCellValue;
        //                                    if (myDT.Year > 1900 && myDT.Year < 2020)
        //                                    {
        //                                        dataRow[j] = myDT.ToString("yyyy/MM/dd HH:mm");
        //                                    }
        //                                    else
        //                                    {
        //                                        dataRow[j] = row.GetCell(j).ToString();
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    dataRow[j] = row.GetCell(j).ToString();
        //                                }
        //                            }
        //                            catch
        //                            {
        //                                dataRow[j] = row.GetCell(j).ToString();
        //                            }
        //                        }
        //                    }
        //                    dt.Rows.Add(dataRow);
        //                }

        //            }

        //            _myExcelWB = null;
        //            _myExcelST = null;
        //        }
        //        return dt;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        #endregion


        public static bool DownloadFile(HttpContext context, Stream myFile, string fileName, long speed)
        {
            bool ret = true;
            try
            {
                #region 定义局部变量
                long startBytes = 0;
                int packSize = 1024 * 10; //分块读取，每块10K bytes

                //FileStream myFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(myFile);
                long fileLength = myFile.Length;

                int sleep = (int)Math.Ceiling(1000.0 * packSize / speed);//毫秒数：读取下一数据块的时间间隔
                //string lastUpdateTiemStr = File.GetLastWriteTimeUtc(filePath).ToString("r");
                //string eTag = HttpUtility.UrlEncode(fileName, Encoding.UTF8) + lastUpdateTiemStr;//便于恢复下载时提取请求头;
                #endregion

                #region--验证：文件是否太大，是否是续传，且在上次被请求的日期之后是否被修改过--------------
                if (myFile.Length > Int32.MaxValue)
                {//-------文件太大了-------
                    context.Response.StatusCode = 413;//请求实体太大
                    return false;
                }

                //if (context.Request.Headers["If-Range"] != null)//对应响应头ETag：文件名+文件最后修改时间
                //{
                //    //----------上次被请求的日期之后被修改过--------------
                //    if (context.Request.Headers["If-Range"].Replace("\"", "") != eTag)
                //    {//文件修改过
                //        context.Response.StatusCode = 412;//预处理失败
                //        return false;
                //    }
                //}
                #endregion

                try
                {
                    #region -------添加重要响应头、解析请求头、相关验证-------------------
                    context.Response.Clear();
                    context.Response.Buffer = false;
                    //context.Response.AddHeader("Content-MD5", GetHashMD5(myFile));//用于验证文件
                    context.Response.AddHeader("Accept-Ranges", "bytes");//重要：续传必须
                    //context.Response.AppendHeader("ETag", "\"" + eTag + "\"");//重要：续传必须
                    //context.Response.AppendHeader("Last-Modified", lastUpdateTiemStr);//把最后修改日期写入响应              
                    context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";//MIME类型：匹配任意文件类型
                    context.Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName, Encoding.UTF8).Replace("+", "%20"));
                    context.Response.AddHeader("Content-Length", (fileLength - startBytes).ToString());
                    context.Response.AddHeader("Connection", "Keep-Alive");
                    context.Response.ContentEncoding = Encoding.UTF8;
                    if (context.Request.Headers["Range"] != null)
                    {
                        //------如果是续传请求，则获取续传的起始位置，即已经下载到客户端的字节数------
                        context.Response.StatusCode = 206;//重要：续传必须，表示局部范围响应。初始下载时默认为200
                        string[] range = context.Request.Headers["Range"].Split(new char[] { '=', '-' });//"bytes=1474560-"
                        startBytes = Convert.ToInt64(range[1]);//已经下载的字节数，即本次下载的开始位置
                        if (startBytes < 0 || startBytes >= fileLength)
                        {
                            //无效的起始位置
                            return false;
                        }
                    }
                    if (startBytes > 0)
                    {
                        //------如果是续传请求，告诉客户端本次的开始字节数，总长度，以便客户端将续传数据追加到startBytes位置后----------
                        context.Response.AddHeader("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                    }
                    #endregion


                    #region -------向客户端发送数据块-------------------
                    br.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                    int maxCount = (int)Math.Ceiling((fileLength - startBytes + 0.0) / packSize);//分块下载，剩余部分可分成的块数
                    for (int i = 0; i < maxCount && context.Response.IsClientConnected; i++)
                    {//客户端中断连接，则暂停
                        context.Response.BinaryWrite(br.ReadBytes(packSize));
                        context.Response.Flush();
                        if (sleep > 1) Thread.Sleep(sleep);
                    }
                    #endregion
                }
                catch
                {
                    ret = false;
                }
                finally
                {
                    br.Close();
                    myFile.Close();
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }
    }
}