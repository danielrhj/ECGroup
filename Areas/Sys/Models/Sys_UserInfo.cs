
using ECGroup.Areas.Mms.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using Zephyr.Core;
using Zephyr.Utils;

namespace ECGroup.Models
{
    [Module("EC")]
    public class SYS_UserInfoService : ServiceBase<SYS_UserInfo>
    {

        internal IList<SYS_UserInfo> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<SYS_UserInfo> myList = db.Select<SYS_UserInfo>(strSQL).QueryMany();

            return myList;
        }

        internal object Login(JObject request)
        {   
            var UserCode = request.Value<string>("usercode");
            var Password = request.Value<string>("password");
            var VerifiCode = request.Value<string>("verificode");
            //用戶名密碼檢查
            if (String.IsNullOrEmpty(VerifiCode))
                return new { status = "error", message = "驗證碼不能為空！" };
            if (String.IsNullOrEmpty(UserCode) || String.IsNullOrEmpty(Password))
                return new { status = "error", message = "用戶名或密碼不能為空！" };

            var verificode = System.Web.HttpContext.Current.Session["VerificationCode"] as string;
            if (verificode.ToLower() != VerifiCode.ToLower())
            {
                return new { status = "error", message = "驗證碼不正確！" };
            }

            //用戶名密碼驗證
            var result = this.GetModel(ParamQuery.Instance()
                            .AndWhere("UserAccount", UserCode)
                            .AndWhere("Password", Password)
                            .AndWhere("is_valid", true));

            if (result == null || String.IsNullOrEmpty(result.UserAccount))
                return new { status = "error", message = "用戶名或密碼不正確！" };

            //調用框架中的登陸機制
            var loginer = new LoginerBase { UserCode = result.UserAccount, UserName = result.UserName };

            var effectiveHours = ZConfig.GetConfigInt("LoginEffectiveHours");
            FormsAuth.SignIn(loginer.UserCode, loginer, effectiveHours*60); // *

            //登陸後處理
            this.UpdateUserLoginCountAndDate(UserCode); //更新用戶登陸次數及時間
            this.AppendLoginHistory(request);           //添加登陸履歷
            MmsService.LoginHandler(request);           //MMS系統的其它的業務處理

            //返回登陸成功
            return new { status = "success", message = "登陸成功！" };
        }

        public void UpdateUserLoginCountAndDate(string UserCode)
        {
            int intA=db.Sql("update sys_userInfo set LoginCount = isnull(LoginCount,0) + 1,LastLoginDate = getdate() where UserAccount = @0", UserCode).Execute();
        }

        public void AppendLoginHistory(JObject request)
        {
            var lanIP = ZHttp.ClientIP;
            var hostName = ZHttp.IsLanIP(lanIP) ? ZHttp.ClientHostName : string.Empty; //如果是內網就獲取，否則出錯獲取不到，且影響效率

            var UserCode = request.Value<string>("usercode");
            var UserName = MmsHelper.GetUserName();
            var IP = request.Value<string>("ip");
            var City = request.Value<string>("city");
            if (IP != lanIP)
                IP = string.Format("{0}/{1}", IP, lanIP).Trim('/').Replace("::1", "localhost");

            var item = new sys_loginHistory();
            item.UserCode = UserCode;
            item.UserName = UserName;
            item.HostName = hostName;
            item.HostIP = IP;
            item.LoginCity = City;
            item.LoginDate = DateTime.Now;

            db.Insert<sys_loginHistory>("sys_loginHistory", item).AutoMap(x => x.ID).Execute();
        }

        public static List<dynamic> GetRoleList(string RoleName)
        {
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ps.Parameter("ActionType", "GetRoleList");
            ps.Parameter("Role", RoleName);
            List<dynamic> resultA = new SYS_UserInfoService().GetDynamicList(ps);
            return resultA;
        }

        public static List<dynamic> GetRoleListForInsert(string RoleName)
        {
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ps.Parameter("ActionType", "GetRoleList");
            ps.Parameter("Role", RoleName);
            ps.Parameter("UserID", FormsAuth.GetUserData().UserCode);
            List<dynamic> resultA = new SYS_UserInfoService().GetDynamicList(ps);
            return resultA;
        }

        public static List<dynamic> GetUserCodeList(string UserCode)
        {
            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            ps.Parameter("ActionType", "GetUserCodeList");
            ps.Parameter("Account", UserCode);
            List<dynamic> resultA = new SYS_UserInfoService().GetDynamicList(ps);
            return resultA;
        }

        public static List<dynamic> GetAreaNameList()
        {
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ps.Parameter("ActionType", "GetSiteList");
            List<dynamic> resultA = new SYS_UserInfoService().GetDynamicList(ps);
            return resultA;
        }

        public static List<dynamic> GetYN()
        {
            //GGGG
            var result = new List<dynamic>();
            result.Add(new { value = "", text = "全部" });    //全部
            result.Add(new { value = "1", text = "Y" });    //有效,對應DB為1
            result.Add(new { value = "0", text = "N" });    //無效,對應DB為0

            return result;
        }

        public static List<dynamic> GetYON()
        {
            //GGGG
            var result = new List<dynamic>();
            result.Add(new { value = "", text = "全部" });
            result.Add(new { value = "Y", text = "Y" });
            result.Add(new { value = "N", text = "N" });

            return result;
        }

        public SYS_UserInfo GetUserModel(string UserCode="")
        {
            string UserAccount = string.IsNullOrEmpty(UserCode) ? FormsAuth.GetUserData().UserCode : UserCode;
            var user = db.Sql("select * from sys_userInfo where UserAccount=@0", UserAccount).QuerySingle<SYS_UserInfo>();
            return user;
        }

        public bool updateSignMap(string UserID, string strSignMapName)
        {
            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            ps.Parameter("ActionType", "updateSignMap");
            ps.Parameter("Account", UserID);
            ps.Parameter("SignMap", strSignMapName);
            bool result = new SYS_UserInfoService().StoredProcedureNoneQuery(ps);
            return result;
        }

        public bool ModifyUserByModel(SYS_UserInfo model)
        {
            ParamSP ps = new ParamSP().Name("usp_UserAccount");

            ps.Parameter("ActionType", "ModifyUserByModel");
            ps.Parameter("Account", model.UserAccount);
            ps.Parameter("UserName", model.UserName);
            
            ps.Parameter("Tel", model.Tel);
            ps.Parameter("Email", model.Email);

            ps.Parameter("Role", model.Role);
            ps.Parameter("is_valid", model.is_valid);
            ps.Parameter("SignMap", model.SignatureLine);
            ps.Parameter("Department", model.Department);
            bool result = new SYS_UserInfoService().StoredProcedureNoneQuery(ps);           
            return result;
        }

        public static bool Login(SYS_UserInfo ui)
        {
            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            ps.Parameter("ActionType", "login");
            ps.Parameter("Account", ui.UserAccount);
            ps.Parameter("Pwd", ui.Password);
            var mUser = new SYS_UserInfoService().StoredProcedureDT(ps);

            if (mUser.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string ChangePWD(string strUserAccount, string strPwdOld, string strPwdNew)
        {
            string Rs = string.Empty;
           
            try
            {
                ParamSP ps = new ParamSP().Name("usp_UserAccount");
                ps.Parameter("ActionType", "ChangePWD");
                ps.Parameter("Account", strUserAccount);
                ps.Parameter("Pwd", strPwdOld);
                ps.Parameter("NPwd", strPwdNew);

                string result = new SYS_UserInfoService().StoredProcedureScalar(ps);                
                return result;
            }
            catch
            {
                return "修改密碼失敗！請聯繫IT！";
            }
        }

        public static string GetUserMenu(string UserID)
        {
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ps.Parameter("ActionType", "GetUserMenuList");
            ps.Parameter("UserId", UserID);
            
            DataTable dt = new SYS_UserInfoService().StoredProcedureDT(ps);


            return new SYS_UserInfoService().JsonFromDataTable(dt);
        }

        #region DataTable及DataSet轉Json
        public string JsonFromDynamic(dynamic list)
        {
            return JsonConvert.SerializeObject(list);
        }


        public string JsonFromList(IEnumerable<dynamic> list)
        {
            return JsonConvert.SerializeObject(list);
        }

        public string JsonFromDataTable(DataTable dt)
        {
            var dyn = DynamicFromDataTable(dt);
            return JsonConvert.SerializeObject(dyn);
        }

        public dynamic DynamicFromDataTable(DataTable dt)
        {
            var dynamicDt = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                foreach (DataColumn column in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
                dynamicDt.Add(dyn);
            }

            return dynamicDt;
        }

        public List<dynamic> DynamicFromDataSet(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0) { return null; }

            List<dynamic> myList = new List<dynamic>();
            foreach (DataTable dt in ds.Tables)
            {
                myList.Add(DynamicFromDataTable(dt));
            }
            return myList;
        }

        #endregion

    }

    public class SYS_UserInfo : ModelBase
    {     
        [PrimaryKey]
        public string UserAccount { get; set; }
        public string Password { get; set; }
        private string VerifiCode { get; set; }
        public string UserName { get; set; }
        public string UnionCode { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public byte is_valid { get; set; }    
        public string Tel { get; set; }    
        public string CreateBy { get; set; }
        public DateTime? CreateDt { get; set; }
        public string AreaName { get; set; }
        public string Role { get; set; }
        public string Supervisor { get; set; }
        public string SignatureLine { get; set; }
        public int LoginCount { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }   
}