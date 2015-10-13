using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Project.Core
{
    public class HttpResult<T> : HttpResult
    {
        public T Result { get; set; }
    }
    public class HttpResult
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }

        public static HttpResult<T> CreateOk<T>(T data)
        {
            var res = new HttpResult<T>();
            res.Result = data;
            res.IsSuccess = true;
            return res;
        }

        public static HttpResult CreateError(string error)
        {
            var res = new HttpResult();
            res.IsSuccess = false;
            res.Error = error;
            return res;
        }
        public static HttpResult<T> CreateError<T>(string error, T data)
        {
            var res = new HttpResult<T>();
            res.IsSuccess = false;
            res.Error = error;
            res.Result = data;
            return res;
        }
        protected HttpResult() { }
    }
}
