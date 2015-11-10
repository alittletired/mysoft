using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;
namespace Mysoft.Project.Core
{
    public class IBusiness
    {
        public IBusiness(string funcCode, string funcName)
        {
            FuncCode = funcCode;
            FuncName = funcName;
        }
        public string FuncCode { get; private set; }
        public string FuncName { get; private set; }
        public virtual bool HasRight(string actionCode) {
            return false;
        }      
    }
    public class CurrentUser
    {
        public string UserName { get; private set; }
        public string UserGUID { get; private set; }
        public string BUGUID { get; private set; }
        public string IPAddress { get; private set; }
        private static string GetUserIP()
        {
            string ipList =HttpContext.Current. Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipList))
            {
                return ipList.Split(',')[0];
            }

            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }
        public static CurrentUser Current
        {
            get
            {
                var context = HttpContext.Current;
                CurrentUser user = context.Items["CurrentUser"] as CurrentUser;
                if (user == null)
                {                  
                    user =new CurrentUser();
                    user.UserName = context.Session["UserName"].ToString();
                    user.UserGUID = context.Session["UserGUID"].ToString();
                    user.BUGUID = context.Session["BUGUID"].ToString();
                    user.IPAddress = GetUserIP();
                    context.Items["CurrentUser"] = user;

                }
                return user;
            }
        }
    }
}


