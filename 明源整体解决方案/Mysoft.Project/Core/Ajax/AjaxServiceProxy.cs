using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Collections;
using Mysoft.Project.Core;
using System.IO;
using Mysoft.Project.Core.DataAnnotations;
using Mysoft.Project.Json;
using Mysoft.Project.Json.Linq;

namespace Mysoft.Project.Ajax
{

    public static class AjaxServiceProxy
    {
        static BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Static;
        static Dictionary<Type, string> _cacheScript = new Dictionary<Type, string>();


        public static void Run()
        {
            HttpContext context = HttpContext.Current;
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            object mess = null;
            try
            {

                mess = Handle(context);
            }
            catch (Exception ex)
            {

                var innerEx = ex;
                while (innerEx.InnerException != null) {
                    innerEx = innerEx.InnerException;
                }              
                if (context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    mess = "alert('" + innerEx.Message + "')";
                }
                else
                    mess = new { __error__ = innerEx.Message + "\n调用堆栈：" + innerEx.StackTrace };
            }
            if (mess is string)
            {
                context.Response.Write(mess);
            }
            else
                context.Response.Write(JsonHelper.SerializeObject(mess));

        }

        private static bool IsAllowServiceMethod(MemberInfo method){
            if (method.DeclaringType.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase))
               return true;
            if (method.GetCustomAttributes(typeof(ServiceAttribute), true).Length > 0)
                return true;
            return false;
        }


        private static object Handle(HttpContext context)
        {

            HttpRequest request = context.Request;
            var assbemlyName = request.QueryString["assbemly"];
            var invokeMethod = request.QueryString["invokeMethod"];
            var typeName = request.QueryString["type"];
            Type type;

            //请求前端脚本
            if (!string.IsNullOrEmpty(typeName) && request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                try
                {

                    type = ReflectionHelper.GetType(typeName, assbemlyName);


                    context.Response.AddHeader("Content-Type", "application/x-javascript");

                    return GetProxyScript(type, context.Request.Path);
                }
                catch (Exception ex)
                {
                    throw new Exception("无法加载类型，需要传入正确的type，和 assbemly参数！\n出错信息：" + ex.Message, ex);
                }
            }

            object mess = null;
            MethodInfo methodInfo = null;
            try
            {
                methodInfo = ReflectionHelper.GetMethod(invokeMethod, assbemlyName);
            }
            catch (Exception ex)
            {
                throw new Exception("无法调用方法'" + invokeMethod + "'，请检查后台是否存在此方法！\n出错信息：" + ex.Message, ex);

            }

            if (!IsAllowServiceMethod(methodInfo))
            {
                throw new Exception("无法调用方法'" + invokeMethod + "',不满足约定，申明方法的类型需以Service结尾或方法添加ServiceAttribute特性！");
            }
            if(!methodInfo.IsPublic)
                throw new Exception("无法调用方法'" + invokeMethod + "'，方法非public公共方法！");

            //是否开启事务
            var transAttribute = methodInfo.GetCustomAttributes(typeof(TransactionAttribute), true);
            var isOpenTrans = true;
            if (transAttribute.Length > 0)
            {
                isOpenTrans = ((TransactionAttribute)transAttribute[0]).IsOpen;
            }
            else if (methodInfo.Name.StartsWith("get", StringComparison.OrdinalIgnoreCase))
            {
                isOpenTrans = false;
            }

            var postdata = request.Form["postdata"];
            if (isOpenTrans)
            {
                using (var trans = DBHelper.BeginTransaction())
                {
                    mess =new {result= ReflectionHelper. Invoke(methodInfo, postdata)};
                    trans.Complete();
                }
            }
            else
            {
                mess = new { result = ReflectionHelper.Invoke(methodInfo, postdata) };
            }

            return mess;
        }

      
        /// <summary>
        /// 创建类型代理脚本
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xmlHttpUrl"></param>
        /// <returns></returns>
        static string GetProxyScript(Type type, string xmlHttpUrl)
        {
            string script;
            if (_cacheScript.TryGetValue(type, out script)) { return script; }
            StringBuilder sb = new StringBuilder();
            var resName = typeof(AjaxServiceProxy).Name + ".js";          
            var serviceJS = ReflectionHelper.GetResourceString(resName, typeof(AjaxServiceProxy));
            sb.AppendLine(serviceJS);
            sb.AppendLine("var service=new AjaxServiceProxy(window,'" + type.FullName + "','" + xmlHttpUrl + "');");
            object instance = null;
            foreach (PropertyInfo propertyInfo in type.GetProperties(_bindingFlags))
            {
                if (propertyInfo.GetGetMethod().IsStatic)
                    sb.AppendLine("service." + propertyInfo.Name + "='" + propertyInfo.GetValue(null, null) + "';");
                else
                {
                    instance = instance ?? Activator.CreateInstance(type, new object[] { });
                    sb.AppendLine("service." + propertyInfo.Name + "='" + propertyInfo.GetValue(instance, null) + "';");
                }
            }
            foreach (MethodInfo methodInfo in type.GetMethods(_bindingFlags))
            {                
                if (methodInfo.DeclaringType == typeof(object))
                    continue;
                ParameterInfo[] paramterInfos = methodInfo.GetParameters();
                var paramterNames = paramterInfos.Select(o => o.Name).ToArray();
                sb.AppendLine("service.registerMethod('" + methodInfo.Name + "','" + string.Join(",", paramterNames) + "')");

            }
            script = sb.ToString();
            _cacheScript[type] = script;
            return script;
        }
    }
}
