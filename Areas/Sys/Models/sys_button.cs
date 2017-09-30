using System;
using System.Collections.Generic;
using System.Text;
using Zephyr.Core;
using System.ComponentModel.DataAnnotations;

namespace ECGroup.Models
{
    [Module("EC")]
    public class sys_buttonService : ServiceBase<sys_button>
    {
        protected override bool OnBeforEditDetail(EditEventArgs arg)
        {
            var ButtonCode = arg.row["ButtonCode"].ToString();

            if (arg.type == OptType.Del)
            {
                db.Sql(@"delete sys_userMenuButtonMap where ButtonCode = @0
                         delete sys_menuButtonMap where ButtonCode =@0 ", ButtonCode).Execute();
            }

            return base.OnBeforEditDetail(arg);
        }
    }

    public class sys_button : ModelBase
    {

        [PrimaryKey]
        public string ButtonCode
        {
            get;
            set;
        }

        public string ButtonName
        {
            get;
            set;
        }

        public int? ButtonSeq
        {
            get;
            set;
        }

        public int? ColWidth
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string ButtonIcon
        {
            get;
            set;
        }

        public string Creator
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
