using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Expressions;
using System.Reflection;
using System.IO;
namespace Mysoft.Project.Core
{
    /// <summary>
    /// Type related helper methods
    /// </summary>
    public static class ReflectionHelper
    {
        static Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();
        static Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();


        /// <summary>
        /// 获取资源文件的流
        /// </summary>
        /// <param name="resourceName">包括资源的命名空间 </param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Stream GetResourceStream(string resourceName, Type type)
        {
            Assembly assembly = Assembly.GetAssembly(type);
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                var resName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(resourceName, StringComparison.CurrentCultureIgnoreCase));
                if (resName != null)
                    stream = assembly.GetManifestResourceStream(resName);
            }
            return stream;
        }
        /// <summary>
        /// 获取资源文件的文本
        /// </summary>
        /// <param name="resourceName">包括资源的命名空间 </param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetResourceString(string resourceName, Type type)
        {
            using (StreamReader reader = new StreamReader(GetResourceStream(resourceName, type), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }


        public static bool TryInvokeMethod(string methodName, string assbemly,out object value, params object[] paramArr)
        {
            value = null;
            try
            {
                value = InvokeMethod(methodName, assbemly, paramArr);
                return true;
            }
            catch { return false; }
        }

        public static MethodInfo GetMethod(string callMethod, string assbemlyName, params object[] paramArr)
        {
            MethodInfo methodInfo = null;
            if (_methodCache.TryGetValue(callMethod, out methodInfo))
            {
                return methodInfo;
            }

            var lastDotIndex = callMethod.LastIndexOf(".");
            var typeName = callMethod.Substring(0, lastDotIndex);
            var type = GetType(typeName, assbemlyName);
            var methodName = callMethod.Substring(lastDotIndex + 1);

            var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var n in methodInfos)
            {
                if (n.Name.Equals(methodName) &&(paramArr.Length==0|| n.GetParameters().Length == paramArr.Length))
                {
                    methodInfo = n;
                    break;
                }

            }

            if (methodInfo == null)
            {
                throw new InvalidProgramException("未找到" + callMethod + "方法!");
            }
            _methodCache[callMethod] = methodInfo;
            return methodInfo;
        }

        public static Type GetType(string typeName, string assbemlyName)
        {
            Type type = null;
            if (_typeCache.TryGetValue(typeName, out type))
            {
                return type;
            }
            if (string.IsNullOrEmpty(assbemlyName))
            {
                assbemlyName = typeName.Substring(0, typeName.LastIndexOf("."));
            }
            var assembly = GetAssembly(assbemlyName);
            if (assembly == null)
                throw new FileNotFoundException(assbemlyName + ".dll文件不存在!");
            type = assembly.GetType(typeName);
            if (type == null)
            {
                throw new InvalidProgramException("未找到" + typeName + "类型!");

            }
            _typeCache[typeName] = type;
          
            return type;
        }

        static Assembly GetAssembly(string assemblyName)
        {
            Assembly assembly = null;          
            while (assemblyName.LastIndexOf('.') > 0)
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch { }
                if (assembly != null)
                {                  
                    return assembly;
                }
                assemblyName = assemblyName.Substring(0, assemblyName.LastIndexOf('.'));
            }
            return assembly;

        }

        public static object InvokeMethod(string callMethod, string assbemlyName, params object[] paramArr)
        {

            var methodInfo = GetMethod(callMethod, assbemlyName, paramArr);            
            object instance = null;
            if (!methodInfo.IsStatic)
            {               
                instance = Activator.CreateInstance(methodInfo.DeclaringType, new object[] { });
            }
            return methodInfo.Invoke(instance, paramArr);

        }
    }
}
