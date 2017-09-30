using System;
using System.Collections.Generic;
using System.Text;
using Zephyr.Core;
using System.ComponentModel.DataAnnotations;
using Zephyr.Utils;
using Newtonsoft.Json.Linq;
using System.Data;

namespace ECGroup.Models
{
    [Module("EC")]
    public class Sys_RequestService : ServiceBase<Sys_Request>
    {

        internal IList<Sys_Request> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<Sys_Request> myList = db.Select<Sys_Request>(strSQL).QueryMany();

            return myList;
        }
        public List<dynamic> GetTaskGrade()
        {
            //GGGG
            var result = new List<dynamic>();
            result.Add(new { value = "一般", text = "一般" });
            result.Add(new { value = "緊急", text = "緊急" });
            result.Add(new { value = "非常緊急", text = "非常緊急" });

            return result;
        }

        public List<dynamic> GetRequestTypeList()
        {
            ParamSP ps = new ParamSP().Name("ups_Sys_Request");
            ps.Parameter("ActionType", "GetRequestType");
            var result = new Sys_RequestService().GetDynamicList(ps);

            return result;
        }

        internal List<dynamic> GetUserInfo(string UserCode)
        {
            ParamSP ps = new ParamSP().Name("ups_Sys_Request");
            ps.Parameter("ActionType", "GetUserInfo");
            ps.Parameter("Creator", UserCode);

            DataTable dt = new Sys_RequestService().StoredProcedureDT(ps);
            var result = new List<dynamic>();
            foreach (DataColumn dc in dt.Columns)
            {
                result.Add(new { ItemNo = dc.ColumnName, Content = dt.Rows[0][dc.ColumnName].ToString() });
            }
            dt = null;
            return result;
        }

        internal void InsertFileRecord(dynamic fileInfo)
        {
            string UserCode=fileInfo.UserCode;
            string OldFileName = fileInfo.OldFileName;
            string FileID = fileInfo.FileID;

            ParamSP ps = new ParamSP().Name("ups_Sys_Request");
            ps.Parameter("ActionType", "InsertFileRecord");
            ps.Parameter("RequestID", fileInfo.RequestID);
            ps.Parameter("Creator", UserCode);
            ps.Parameter("FileName",OldFileName);
            ps.Parameter("FileID", FileID);

            bool bk = new Sys_RequestService().StoredProcedureNoneQuery(ps);

        }
    }
    public class Sys_Request:ModelBase
    {
        [Identity]
        [PrimaryKey]
        public int AutoID	{ get; set; }
        public string CatID	{ get; set; }
        public string Subject	{ get; set; }
        public string Request	{ get; set; }
        public string ITPerson	{ get; set; }
        public string ITComments	{ get; set; }
        public DateTime? CloseDate	{ get; set; }
        public string TaskGrade	{ get; set; }
        public string RejectFlag	{ get; set; }
        public string Creator	{ get; set; }
        public DateTime? CreateDate	{ get; set; }
        public string Editor	{ get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string STATUS { get; set; }
    }
}