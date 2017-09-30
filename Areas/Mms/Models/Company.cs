using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Zephyr.Core;

namespace ECGroup.Models
{
    [Module("EC")]
    public class CompanyService : ServiceBase<Company>
    {
        public static string strSP = "DG_BasicInfo";
        internal IList<Company> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<Company> myList = db.Select<Company>(strSQL).QueryMany();

            return myList;
        }
        public static List<dynamic> getcompanyCode(string q)
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetOwnerIDList");
            ps.Parameter("OwnerID", q);
            List<dynamic> resultA = new CompanyService().GetDynamicList(ps);

            return resultA;
        }

        internal static string ExportExcel(ParamSP ps)
        {
            //AutoID	OwnerID	CName	CAdd	Contact	Tel	Fax	Email	Remarks	AccountNo	BankName	Currency

            DataTable dt = new CompanyService().StoredProcedureDS(ps).Tables[0];
            dt.Columns.Remove("AutoID");
            dt.Columns["OwnerID"].ColumnName = "简称"; dt.Columns["CName"].ColumnName = "全称";
            dt.Columns["CAdd"].ColumnName = "地址"; dt.Columns["Contact"].ColumnName = "联系人";
            dt.Columns["Tel"].ColumnName = "电话"; dt.Columns["Fax"].ColumnName = "传真";
            dt.Columns["AccountNo"].ColumnName = "银行账号"; dt.Columns["BankName"].ColumnName = "开户行全称";
            dt.Columns["Currency"].ColumnName = "结算币别"; 

            EPPlusNew myExcel = new EPPlusNew("");

            string filename = "Company.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            myExcel.ExportExcel(dt, path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;
        }
    }
    public class Company : ModelBase
    {
        //SELECT   AutoID, OwnerID, CName, CAdd, Contact, Tel, Fax, Email, Remarks, AccountNo, BankName, Currency FROM      DG_OwnerInfo
        public int AutoID { get; set; }
        public string OwnerID { get; set; }
        public string CName { get; set; }
        public string CAdd { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Remarks { get; set; }
        public string AccountNo { get; set; }
        public string BankName { get; set; }
        public string Currency { get; set; }
    }
}