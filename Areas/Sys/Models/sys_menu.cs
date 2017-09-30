using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Zephyr.Core;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using System.Web;

namespace ECGroup.Models
{
    [Module("EC")]
    public class sys_menuService : ServiceBase<sys_menu>
    {
        protected override bool OnBeforEditDetail(EditEventArgs arg)
        {
            var MenuCode = arg.row["_id"].ToString();
            string strSQL = "";

            if (arg.type == OptType.Del)
            {
                strSQL = string.Format("delete sys_roleMenuColumnMap where id in (select sys_roleMenuColumnMap.id from sys_roleMenuColumnMap left join sys_menu on sys_menu.menucode=sys_roleMenuColumnMap.menucode where sys_menu.menucode={0});",MenuCode);

                strSQL+=string.Format("delete sys_roleMenuMap where id in (select sys_roleMenuMap.id from sys_roleMenuMap left join sys_menu on sys_menu.menucode=sys_roleMenuMap.menucode where sys_menu.menucode={0});",MenuCode);

                strSQL+=string.Format("delete sys_menuButtonMap where sys_menuButtonMap.id in (select id from sys_menuButtonMap left join sys_menu on sys_menu.menucode=sys_menuButtonMap.menucode where sys_menu.menucode={0})", MenuCode);

                db.Sql(strSQL).Execute();
            }

            return base.OnBeforEditDetail(arg);
        }

        public void EditUserMenuButton(string UserCode, JToken MenuButtonList)
        {
            db.UseTransaction(true);
            Logger("保存用戶按鈕權限", () =>
            {
                db.Delete("sys_userMenuButtonMap").Where("UserAccount", UserCode).Execute();
                foreach (JToken item in MenuButtonList.Children())
                {
                    var MenuCode = item["MenuCode"].ToString();
                    var ButtonCode = item["ButtonCode"].ToString();
                    db.Insert("sys_userMenuButtonMap").Column("UserAccount", UserCode).Column("MenuCode", MenuCode).Column("ButtonCode", ButtonCode).Execute();
                }
                db.Commit();
            }, e => db.Rollback());
        }

        //用戶菜單(已廢)
        public dynamic GetUserMenu(string UserCode)
        {            
            var sql = String.Format(@" select distinct B.* from sys_roleMenuMap A
                inner join sys_menu  B on B.MenuCode = A.MenuCode where B.IsEnable='1'  and RoleCode in (
                  select RoleCode from sys_userRoleMap where UserCode = '{0}' 
                union all
                  select RoleCode from sys_organizeRoleMap where OrganizeCode in  
                  (
	                select OrganizeCode from sys_userOrganizeMap where UserCode = '{0}'
                  )  
                )
                order by B.MenuSeq,B.MenuCode", UserCode);

            var result = db.Sql(sql).QueryMany<dynamic>();
            return result;
        }

        public dynamic GetEnabledMenusAndButtons(string UserCode)
        {
            string RoleCode = db.Sql("select Role from SYS_UserInfo where UserAccount=@0", UserCode).QuerySingle<string>();
            var buttons = db.Sql("select * from sys_button order by ButtonSeq").QueryMany<sys_button>();

            var sql = "select A.Menu_Name as MenuName,A.Menu_id as MenuCode,A.Menu_pid as ParentCode,A.IconClass as iconCls,B.Menu_Name as ParentName";
            sql += String.Format(",(select 1 from sys_roleMenu tb_role where tb_role.RoleID='{0}' and tb_role.MenuID=A.Menu_id) as checked", RoleCode);
            foreach (var button in buttons)
                sql += String.Format(@",(
                select case when max(tb1_{0}.ID) is null then -1 
                            when max(tb2_{0}.ID) is null then 0 
                            else 1 end 
                from  sys_menuButtonMap AS tb1_{0}
                left join sys_userMenuButtonMap AS tb2_{0} ON tb2_{0}.MenuCode=tb1_{0}.MenuCode AND tb2_{0}.ButtonCode=tb1_{0}.ButtonCode AND tb2_{0}.UserAccount='{1}'
                where tb1_{0}.MenuCode = A.Menu_id and  tb1_{0}.ButtonCode = '{0}'  
                )as 'btn_{0}' ", button.ButtonCode, UserCode);

                    sql += @"
                from sys_menulist as A
                left join sys_menulist B on B.Menu_id = A.Menu_pid
                where A.Is_valid = 1
                order by A.OrderBy,A.Menu_id";

            var result = db.Sql(sql).QueryMany<dynamic>();

            //var columns = db.Sql("select * from sys_MenuColumnMap where RoleCode = @0", RoleCode).QueryMany<sys_roleMenuColumnMap>();

            //foreach (var item in result)
            //{
            //    string MenuCode = item.MenuCode;
            //    item.AllowColumns = string.Join(",", columns.Where(x => x.MenuCode == MenuCode && x.IsReject == false).Select(x => x.FieldName));
            //    item.RejectColumns = string.Join(",", columns.Where(x => x.MenuCode == MenuCode && x.IsReject == true).Select(x => x.FieldName));
            //}

            return new { menus = result, buttons = buttons };
        }

        public List<dynamic> GetButtons()
        {
            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            ps.Parameter("ActionType", "GetButtons");
            List<dynamic> resultA = new sys_menuService().GetDynamicList(ps);
            return resultA;
        }

        public string SaveButtons(dynamic data)
        {
            var menuService = new sys_menuService();
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveButtons");

                foreach (var item in gridData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ps.Parameter("ActionItem", itemKey);
                        ps.Parameter("Creator", FormsAuth.GetUserData().UserCode);
                        JArray ActionData = item.Value as JArray; //注意這個數組的每個元素對應表格的一行編輯的記錄
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject;
                            foreach (var drField in dr)
                            {
                                ps.Parameter(drField.Key, drField.Value.ToString());
                            }
                            menuService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }

            return "OK";
        }

        public List<dynamic> GetMenuButtons(string MenuCode)
        {
            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            ps.Parameter("ActionType", "GetMenuButtonsAvailable");
            ps.Parameter("MenuCode", MenuCode);
            List<dynamic> resultA = new sys_menuService().GetDynamicList(ps);
            return resultA;
        }

        public void SaveMenuButtons(string MenuCode, JToken ButtonList)
        {
            db.UseTransaction(true);
            Logger("保存頁面按鈕", () =>
            {
                db.Delete("sys_menuButtonMap").Where("MenuCode", MenuCode).Execute();
                foreach (JToken item in ButtonList.Children())
                {
                    var ButtonCode = item["ButtonCode"].ToString();
                    db.Insert("sys_menuButtonMap").Column("MenuCode", MenuCode).Column("ButtonCode", ButtonCode).Execute();
                }
                db.Commit();
            }, e => db.Rollback());
        }

        public string GetCurrentMenuCode()
        {
            var url = HttpContext.Current.Request.RawUrl;
           
            if (HttpContext.Current.Request.Url.Segments.Length > 4)
            {
                if (url != "/mms/APEal/Index/AP" && url != "/mms/APEal/Index/AR" && url != "/mms/APSign/Index/AP" && url != "/mms/APSign/Index/AR")
                {
                    string[] arrUrl = HttpContext.Current.Request.Url.Segments;
                    url = url.Replace(arrUrl[arrUrl.Length - 1], "");
                }
            }

            var result = db.Sql("select Menu_id as MenuCode from sys_menulist where URL = @0", url).QuerySingle<string>();
            return result;
        }

        //public List<sys_button> GetCurrentUserMenuButtons()
        public List<dynamic> GetCurrentUserMenuButtons()
        {
            var MenuCode = GetCurrentMenuCode();
            var UserCode = FormsAuth.GetUserData().UserCode;
            var sql = @"
            select A.*
            from sys_button A
            inner join sys_roleMenuButtonMap B on B.MenuCode = @0 and B.ButtonCode = A.ButtonCode
            where RoleCode in (
            select RoleCode
            from sys_userRoleMap
            where userCode = @1
            union
            select A.RoleCode
            from sys_organizeRoleMap A
            inner join sys_userOrganizeMap B on B.OrganizeCode = A.OrganizeCode
            where B.UserCode = @1
            )
            order by ButtonSeq";

            //var result = db.Sql(sql, MenuCode, UserCode).QueryMany<sys_button>();
            var result = new List<dynamic>();
            return result;
        }

        public List<dynamic> GetCurrentUserMenuButtonsNew()
         {
            var MenuCode = GetCurrentMenuCode();
            var UserCode = FormsAuth.GetUserData().UserCode;
            var sql = @"select A.ButtonCode,B.ButtonIcon from sys_MenuButtonMap A,sys_button B where A.ButtonCode=B.ButtonCode and A.MenuCode=@0
             and not exists(select 1 from sys_userMenuButtonMap where MenuCode=A.MenuCode and ButtonCode=A.ButtonCode and UserAccount=@1)";
            var result = db.Sql(sql, MenuCode, UserCode).QueryMany<dynamic>();            
            return result;
        }
        public List<dynamic> GetUserButtons()
        {
            string UserCode = FormsAuth.GetUserData().UserCode;
            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            ps.Parameter("ActionType", "GetUserButtonRightsTemp");
            ps.Parameter("UserAccount", UserCode);
            List<dynamic> resultA = new sys_menuService().GetDynamicList(ps);
            return resultA;
        }


    
    }


    public class sys_menu : ModelBase
    {

        [PrimaryKey]
        public string MenuCode
        {
            get;
            set;
        }

        public string ParentCode
        {
            get;
            set;
        }

        public string MenuName
        {
            get;
            set;
        }

        public string URL
        {
            get;
            set;
        }

        public string IconClass
        {
            get;
            set;
        }

        public string IconURL
        {
            get;
            set;
        }

        public string MenuSeq
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public bool? IsVisible
        {
            get;
            set;
        }

        public bool? IsEnable
        {
            get;
            set;
        }

        public string CreatePerson
        {
            get;
            set;
        }

        public DateTime? CreateDate
        {
            get;
            set;
        }

        public string UpdatePerson
        {
            get;
            set;
        }

        public DateTime? UpdateDate
        {
            get;
            set;
        }

    }
}
