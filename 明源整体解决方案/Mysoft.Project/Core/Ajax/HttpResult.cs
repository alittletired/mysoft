using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mysoft.Project.Core
{
  
    public class HttpResult
    {

        public object data { get; set; }
        public string err { get; set; }
        public static HttpResult Ok(object data)
        {
            var res = new HttpResult();
            res.data = data;          
            return res;
        }

        public static HttpResult Error(string error)
        {
            var res = new HttpResult();
            res.err = error;
            return res;
        }
      
        protected HttpResult() { }
    }
}
