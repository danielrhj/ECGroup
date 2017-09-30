using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Zephyr.Core;

namespace ECGroup.Models
{
    [Module("EC")]
    public class CustomerService : ServiceBase<Customer>
    {
        public static string strSP = "DG_BasicInfo";
        internal IList<Customer> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<Customer> myList = db.Select<Customer>(strSQL).QueryMany();

            return myList;
        }    

     public static List<dynamic> GetRegistryList()
     {
        var result = new List<dynamic>();
        result.Add(new { value = "Abroad", text = "Abroad" });
        result.Add(new { value = "Domestic", text = "Domestic" });

        return result;
     }

     public static List<dynamic> GetCurrencyList()
     {
         var result = new List<dynamic>();
         result.Add(new { value = "RMB", text = "RMB" });
         result.Add(new { value = "外幣", text = "外幣" });
         result.Add(new { value = "全部", text = "全部" });
         return result;
     }

     public static List<dynamic> GetLegalTypeList()
     {
         ParamSP ps = new ParamSP().Name(strSP);
         ParamSPData psd = ps.GetData();
         ps.Parameter("ActionType", "GetLegalTypeList");
         List<dynamic> resultA = new CustomerService().GetDynamicList(ps);
         var result = new List<dynamic>();
         resultA.ForEach(delegate(dynamic item)
         {
             result.Add(new { value = item.value, text = item.text });
         });

         return result;      
     }

     public static List<dynamic> GetPayerTypeList()
     {
         ParamSP ps = new ParamSP().Name(strSP);
         ParamSPData psd = ps.GetData();
         ps.Parameter("ActionType", "GetPayerTypeList");
         List<dynamic> resultA = new CustomerService().GetDynamicList(ps);
         var result = new List<dynamic>();
         resultA.ForEach(delegate(dynamic item)
         {
             result.Add(new { value = item.value, text = item.text });
         });

         return result;
     }

     public static List<dynamic> GetSystemTypeList()
     {
         var result = new List<dynamic>();
         result.Add(new { value = "TIPTOP", text = "TIPTOP" });
         result.Add(new { value = "SAP", text = "SAP" });
         return result;
     }

     public static dynamic GetPayerByCustomerCode(string q)
     {
         ParamSP ps = new ParamSP().Name("usp_BasisData");
         ParamSPData psd = ps.GetData();
         ps.Parameter("ActionType", "GetPayerByCustomerCode");
         ps.Parameter("data", q);
         dynamic resultA = new CustomerService().GetModel(ps);

         return resultA;
     }

     public static List<dynamic> getPayeeList(string q)
     {
         ParamSP ps = new ParamSP().Name("usp_BasisData");
         ParamSPData psd = ps.GetData();
         ps.Parameter("ActionType", "GetPayeeList");
         ps.Parameter("data", q);
         List<dynamic> resultA = new CustomerService().GetDynamicList(ps);

         return resultA; 
     }

     public static List<dynamic> getCustomerCodeListForCombobox()
     {
         ParamSP ps = new ParamSP().Name(strSP);
         ParamSPData psd = ps.GetData();
         ps.Parameter("ActionType", "getCustomerCodeList");
         List<dynamic> resultA = new CustomerService().GetDynamicList(ps);

         return resultA;
     }


     public static List<dynamic> getCustomerAbbr(string q)
     {
         ParamSP ps = new ParamSP().Name(strSP);
         ParamSPData psd = ps.GetData();
         ps.Parameter("ActionType", "GetCustomerListA");
         ps.Parameter("CustAbbr", q);
         List<dynamic> resultA = new CustomerService().GetDynamicList(ps);

         return resultA;
     }

     internal static string ExportExcel(ParamSP ps)
     {
        DataTable dt = new CompanyService().StoredProcedureDS(ps).Tables[0];
        dt.Columns.Remove("CustID");
        dt.Columns["CustCode"].ColumnName = "客户代码"; dt.Columns["CustAbbr"].ColumnName = "客户简称"; dt.Columns["CustName"].ColumnName = "全称";
        dt.Columns["CustAdd"].ColumnName = "地址"; dt.Columns["Contact"].ColumnName = "联系人";
        dt.Columns["Tel"].ColumnName = "电话"; dt.Columns["CellNo"].ColumnName = "手机";
        dt.Columns["AccountNo"].ColumnName = "银行账号"; dt.Columns["AccountName"].ColumnName = "账号持有人";
        dt.Columns["Currency"].ColumnName = "结算币别"; dt.Columns["BankName"].ColumnName = "开户行"; dt.Columns["PayTerms"].ColumnName = "结算条件";

        EPPlusNew myExcel = new EPPlusNew("");

        string filename = "Customer.xlsx";
        string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

        myExcel.ExportExcel(dt, path);
        myExcel = null;
        return "/FJLBS/TempFiles/" + filename;
     }

    }
    public class Customer : ModelBase
    {
        [PrimaryKey]
        public int CustID { get; set; }
        public string CustAbbr { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string CustAdd { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
        public string CellNo { get; set; }
        public string Email { get; set; }
        public string SWIFICode { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public string Currency { get; set; }
        public string PayTerms { get; set; }
        public string BankName { get; set; } 
    }
}