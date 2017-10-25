using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zephyr.Core;

namespace ECGroup.Models
{
    [Module("EC")]
    public class MaterialTypeService : ServiceBase<MaterialType>
    {
        public static string strSP = "DG_BasicInfo";
        internal IList<MaterialType> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<MaterialType> myList = db.Select<MaterialType>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getTypeCodeList(bool showBlank)
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetTypeCodeList");
            List<dynamic> resultA = new MaterialTypeService().GetDynamicList(ps);

            if (showBlank)
            { resultA.Insert(0, new { value = "", text = "全部" }); }
            return resultA; 
        }

        internal static List<dynamic> getBrandList(bool showBlank)
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetBrandListForAC");
            List<dynamic> resultA = new MaterialTypeService().GetDynamicList(ps);

            if (showBlank)
            { resultA.Insert(0, new { value = "", text = "全部" }); }
            return resultA;
        }
    }

    public class MaterialType : ModelBase
    {
        [PrimaryKey]
        public int AutoID { get; set; }
        public string TypeCode { get; set; }
        public string TypeNam { get; set; }       
    }
}