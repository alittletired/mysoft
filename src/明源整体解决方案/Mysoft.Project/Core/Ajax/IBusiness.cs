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
        public string UserName { get { return HttpContext.Current.Session["UserName"].ToString(); } }
        public string UserGUID { get  { return HttpContext.Current.Session["UserGUID"].ToString(); } }
        public string BUGUID { get { return HttpContext.Current.Session["BUGUID"].ToString(); } }
        public static CurrentUser Current
        {
            get
            {
                var context = HttpContext.Current;
                CurrentUser user = context.Items["CurrentUser"] as CurrentUser;
                if (user == null)
                {                  
                    user =new CurrentUser();
                    context.Items["CurrentUser"] = user;
                }
                return user;
            }
        }
    }
}


