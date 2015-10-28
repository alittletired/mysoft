using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Web;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using System.IO;

using System.Reflection.Emit;
using System.Linq.Expressions;
namespace Mysoft.Project.Core
{

    public class PocoColumn
    {
        public string ColumnName;
        public PropertyInfo PropertyInfo;
        public bool ResultColumn;
        public virtual void SetValue(object target, object val) { PropertyInfo.SetValue(target, val, null); }
        public virtual object GetValue(object target) { return PropertyInfo.GetValue(target, null); }
        public virtual object ChangeType(object val) { return Convert.ChangeType(val, PropertyInfo.PropertyType); }
    }
    public class ExpandoColumn : PocoColumn
    {
        public override void SetValue(object target, object val) { (target as IDictionary<string, object>)[ColumnName] = val; }
        public override object GetValue(object target)
        {
            object val = null;
            (target as IDictionary<string, object>).TryGetValue(ColumnName, out val);
            return val;
        }
        public override object ChangeType(object val) { return val; }
    }
    public class PocoData
    {
        public static PocoData ForObject(object o)
        {
            var t = o.GetType();
            //#if !PETAPOCO_NO_DYNAMIC
            //                if (t == typeof(System.Dynamic.ExpandoObject))
            //                {
            //                    var pd = new PocoData();
            //                    pd.TableInfo = new TableInfo();
            //                    pd.Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
            //                    pd.Columns.Add(primaryKeyName, new ExpandoColumn() { ColumnName = primaryKeyName });
            //                    pd.TableInfo.PrimaryKey = primaryKeyName;
            //                    pd.TableInfo.AutoIncrement = true;
            //                    foreach (var col in (o as IDictionary<string, object>).Keys)
            //                    {
            //                        if (col != primaryKeyName)
            //                            pd.Columns.Add(col, new ExpandoColumn() { ColumnName = col });
            //                    }
            //                    return pd;
            //                }
            //                else
            //#endif
            return ForType(t);
        }
        static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();
        public static PocoData ForType(Type t)
        {
            //#if !PETAPOCO_NO_DYNAMIC
            //                if (t == typeof(System.Dynamic.ExpandoObject))
            //                    throw new InvalidOperationException("Can't use dynamic types with this method");
            //#endif
            // Check cache
            RWLock.EnterReadLock();
            PocoData pd;
            try
            {
                if (m_PocoDatas.TryGetValue(t, out pd))
                    return pd;
            }
            finally
            {
                RWLock.ExitReadLock();
            }


            // Cache it
            RWLock.EnterWriteLock();
            try
            {
                // Check again
                if (m_PocoDatas.TryGetValue(t, out pd))
                    return pd;

                // Create it
                pd = new PocoData(t);

                m_PocoDatas.Add(t, pd);
            }
            finally
            {
                RWLock.ExitWriteLock();
            }

            return pd;
        }

        public PocoData()
        {
        }

        public PocoData(Type t)
        {
            type = t;
            TableInfo = new TableInfo();

            // Get the table name
            var a = t.GetCustomAttributes(typeof(TableAttribute), true);
            TableInfo.TableName = a.Length == 0 ? t.Name : (a[0] as TableAttribute).Name;


            // Call column mapper
            if (Database.Mapper != null)
                Database.Mapper.GetTableInfo(t, TableInfo);

            // Work out bound properties
            bool ExplicitColumns = t.GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Length > 0;
            Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
            IgnoreColumns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
            foreach (var pi in t.GetProperties())
            {
                a = pi.GetCustomAttributes(typeof(IDAttribute), true);
                if (a.Length > 0)
                {
                    IDAttribute idAttri = a[0] as IDAttribute;
                    TableInfo.PrimaryKey = idAttri.Name ?? pi.Name;
                    TableInfo.SequenceName = idAttri.Name ?? pi.Name;
                    TableInfo.AutoIncrement = idAttri.AutoIncrement;
                }


                // Work out if properties is to be included
                var ColAttrs = pi.GetCustomAttributes(typeof(ColumnAttribute), true);

                if (ExplicitColumns)
                {
                    if (ColAttrs.Length == 0)
                        continue;
                }
                else
                {

                }

                var pc = new PocoColumn();
                pc.PropertyInfo = pi;

                // Work out the DB column name
                if (ColAttrs.Length > 0)
                {
                    var colattr = (ColumnAttribute)ColAttrs[0];
                    pc.ColumnName = colattr.Name;
                    if ((colattr as ResultColumnAttribute) != null)
                        pc.ResultColumn = true;
                }
                if (pc.ColumnName == null)
                {
                    pc.ColumnName = pi.Name;
                    if (Database.Mapper != null && !Database.Mapper.MapPropertyToColumn(pi, ref pc.ColumnName, ref pc.ResultColumn))
                        continue;
                }
                if (pi.GetCustomAttributes(typeof(IgnoreAttribute), true).Length != 0)
                { IgnoreColumns[pc.ColumnName] = pc; }
                // Store it
                Columns.Add(pc.ColumnName, pc);
            }
            TableInfo.PrimaryKey = TableInfo.PrimaryKey ?? "ID";
            TableInfo.SequenceName = TableInfo.SequenceName ?? "ID";
            // Build column list for automatic select
            QueryColumns = (from c in Columns where !c.Value.ResultColumn select c.Key).ToArray();

        }

        static bool IsIntegralType(Type t)
        {
            var tc = Type.GetTypeCode(t);
            return tc >= TypeCode.SByte && tc <= TypeCode.UInt64;
        }

        // Create factory function that can convert a IDataReader record into a POCO
        public Delegate GetFactory(string sql, string connString, bool ForceDateTimesToUtc, int firstColumn, int countColumns, IDataReader r)
        {

            var key = string.Format("{0}:{1}:{2}:{3}:{4}", sql, connString, ForceDateTimesToUtc, firstColumn, countColumns).GetHashCode().ToString();
            RWLock.EnterReadLock();
            try
            {
                // Have we already created it?
                Delegate factory;
                if (PocoFactories.TryGetValue(key, out factory))
                    return factory;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            // Take the writer lock
            RWLock.EnterWriteLock();

            try
            {

                // Check again, just in case
                Delegate factory;
                if (PocoFactories.TryGetValue(key, out factory))
                    return factory;
                PocoFactories.Clear();
                // Create the method
                var m = new DynamicMethod("petapoco_factory_" + PocoFactories.Count.ToString(), type, new Type[] { typeof(IDataReader) }, true);
                var il = m.GetILGenerator();

                //#if !PETAPOCO_NO_DYNAMIC
                //                    if (type == typeof(object))
                //                    {
                //                        // var poco=new T()
                //                        il.Emit(OpCodes.Newobj, typeof(System.Dynamic.ExpandoObject).GetConstructor(Type.EmptyTypes));			// obj

                //                        MethodInfo fnAdd = typeof(IDictionary<string, object>).GetMethod("Add");

                //                        // Enumerate all fields generating a set assignment for the column
                //                        for (int i = firstColumn; i < firstColumn + countColumns; i++)
                //                        {
                //                            var srcType = r.GetFieldType(i);

                //                            il.Emit(OpCodes.Dup);						// obj, obj
                //                            il.Emit(OpCodes.Ldstr, r.GetName(i));		// obj, obj, fieldname

                //                            // Get the converter
                //                            Func<object, object> converter = null;
                //                            if (Database.Mapper != null)
                //                                converter = Database.Mapper.GetFromDbConverter(null, srcType);
                //                            if (ForceDateTimesToUtc && converter == null && srcType == typeof(DateTime))
                //                                converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };

                //                            // Setup stack for call to converter
                //                            AddConverterToStack(il, converter);

                //                            // r[i]
                //                            il.Emit(OpCodes.Ldarg_0);					// obj, obj, fieldname, converter?,    rdr
                //                            il.Emit(OpCodes.Ldc_I4, i);					// obj, obj, fieldname, converter?,  rdr,i
                //                            il.Emit(OpCodes.Callvirt, fnGetValue);		// obj, obj, fieldname, converter?,  value

                //                            // Convert DBNull to null
                //                            il.Emit(OpCodes.Dup);						// obj, obj, fieldname, converter?,  value, value
                //                            il.Emit(OpCodes.Isinst, typeof(DBNull));	// obj, obj, fieldname, converter?,  value, (value or null)
                //                            var lblNotNull = il.DefineLabel();
                //                            il.Emit(OpCodes.Brfalse_S, lblNotNull);		// obj, obj, fieldname, converter?,  value
                //                            il.Emit(OpCodes.Pop);						// obj, obj, fieldname, converter?
                //                            if (converter != null)
                //                                il.Emit(OpCodes.Pop);					// obj, obj, fieldname, 
                //                            il.Emit(OpCodes.Ldnull);					// obj, obj, fieldname, null
                //                            if (converter != null)
                //                            {
                //                                var lblReady = il.DefineLabel();
                //                                il.Emit(OpCodes.Br_S, lblReady);
                //                                il.MarkLabel(lblNotNull);
                //                                il.Emit(OpCodes.Callvirt, fnInvoke);
                //                                il.MarkLabel(lblReady);
                //                            }
                //                            else
                //                            {
                //                                il.MarkLabel(lblNotNull);
                //                            }

                //                            il.Emit(OpCodes.Callvirt, fnAdd);
                //                        }
                //                    }
                //                    else
                //#endif
                if (type.IsValueType || type == typeof(string) || type == typeof(byte[]))
                {
                    // Do we need to install a converter?
                    var srcType = r.GetFieldType(0);
                    var converter = GetConverter(ForceDateTimesToUtc, null, srcType, type);

                    // "if (!rdr.IsDBNull(i))"
                    il.Emit(OpCodes.Ldarg_0);										// rdr
                    il.Emit(OpCodes.Ldc_I4_0);										// rdr,0
                    il.Emit(OpCodes.Callvirt, fnIsDBNull);							// bool
                    var lblCont = il.DefineLabel();
                    il.Emit(OpCodes.Brfalse_S, lblCont);
                    il.Emit(OpCodes.Ldnull);										// null
                    var lblFin = il.DefineLabel();
                    il.Emit(OpCodes.Br_S, lblFin);

                    il.MarkLabel(lblCont);

                    // Setup stack for call to converter
                    AddConverterToStack(il, converter);

                    il.Emit(OpCodes.Ldarg_0);										// rdr
                    il.Emit(OpCodes.Ldc_I4_0);										// rdr,0
                    il.Emit(OpCodes.Callvirt, fnGetValue);							// value

                    // Call the converter
                    if (converter != null)
                        il.Emit(OpCodes.Callvirt, fnInvoke);

                    il.MarkLabel(lblFin);
                    il.Emit(OpCodes.Unbox_Any, type);								// value converted
                }
                else
                {
                    // var poco=new T()
                    il.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));

                    // Enumerate all fields generating a set assignment for the column
                    for (int i = firstColumn; i < firstColumn + countColumns; i++)
                    {
                        // Get the PocoColumn for this db column, ignore if not known
                        PocoColumn pc;
                        if (!Columns.TryGetValue(r.GetName(i), out pc))
                            continue;

                        // Get the source type for this column
                        var srcType = r.GetFieldType(i);
                        var dstType = pc.PropertyInfo.PropertyType;

                        // "if (!rdr.IsDBNull(i))"
                        il.Emit(OpCodes.Ldarg_0);										// poco,rdr
                        il.Emit(OpCodes.Ldc_I4, i);										// poco,rdr,i
                        il.Emit(OpCodes.Callvirt, fnIsDBNull);							// poco,bool
                        var lblNext = il.DefineLabel();
                        il.Emit(OpCodes.Brtrue_S, lblNext);								// poco

                        il.Emit(OpCodes.Dup);											// poco,poco

                        // Do we need to install a converter?
                        var converter = GetConverter(ForceDateTimesToUtc, pc, srcType, dstType);

                        // Fast
                        bool Handled = false;
                        if (converter == null)
                        {
                            var valuegetter = typeof(IDataRecord).GetMethod("Get" + srcType.Name, new Type[] { typeof(int) });
                            if (valuegetter != null
                                    && valuegetter.ReturnType == srcType
                                    && (valuegetter.ReturnType == dstType || valuegetter.ReturnType == Nullable.GetUnderlyingType(dstType)))
                            {
                                il.Emit(OpCodes.Ldarg_0);										// *,rdr
                                il.Emit(OpCodes.Ldc_I4, i);										// *,rdr,i
                                il.Emit(OpCodes.Callvirt, valuegetter);							// *,value

                                // Convert to Nullable
                                if (Nullable.GetUnderlyingType(dstType) != null)
                                {
                                    il.Emit(OpCodes.Newobj, dstType.GetConstructor(new Type[] { Nullable.GetUnderlyingType(dstType) }));
                                }

                                il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true));		// poco
                                Handled = true;
                            }
                        }

                        // Not so fast
                        if (!Handled)
                        {
                            // Setup stack for call to converter
                            AddConverterToStack(il, converter);

                            // "value = rdr.GetValue(i)"
                            il.Emit(OpCodes.Ldarg_0);										// *,rdr
                            il.Emit(OpCodes.Ldc_I4, i);										// *,rdr,i
                            il.Emit(OpCodes.Callvirt, fnGetValue);							// *,value

                            // Call the converter
                            if (converter != null)
                                il.Emit(OpCodes.Callvirt, fnInvoke);

                            // Assign it
                            il.Emit(OpCodes.Unbox_Any, pc.PropertyInfo.PropertyType);		// poco,poco,value
                            il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true));		// poco
                        }

                        il.MarkLabel(lblNext);
                    }

                    var fnOnLoaded = RecurseInheritedTypes<MethodInfo>(type, (x) => x.GetMethod("OnLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));
                    if (fnOnLoaded != null)
                    {
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Callvirt, fnOnLoaded);
                    }
                }

                il.Emit(OpCodes.Ret);

                // Cache it, return it
                var del = m.CreateDelegate(Expression.GetFuncType(typeof(IDataReader), type));
                PocoFactories.Add(key, del);
                return del;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
        }

        private static void AddConverterToStack(ILGenerator il, Func<object, object> converter)
        {
            if (converter != null)
            {
                // Add the converter
                int converterIndex = m_Converters.Count;
                m_Converters.Add(converter);

                // Generate IL to push the converter onto the stack
                il.Emit(OpCodes.Ldsfld, fldConverters);
                il.Emit(OpCodes.Ldc_I4, converterIndex);
                il.Emit(OpCodes.Callvirt, fnListGetItem);					// Converter
            }
        }

        private static Func<object, object> GetConverter(bool forceDateTimesToUtc, PocoColumn pc, Type srcType, Type dstType)
        {
            Func<object, object> converter = null;

            // Get converter from the mapper
            if (Database.Mapper != null)
            {
                if (pc != null)
                {
                    converter = Database.Mapper.GetFromDbConverter(pc.PropertyInfo, srcType);
                }
                else
                {
                    var m2 = Database.Mapper as IMapper2;
                    if (m2 != null)
                    {
                        converter = m2.GetFromDbConverter(dstType, srcType);
                    }
                }
            }

            // Standard DateTime->Utc mapper
            if (forceDateTimesToUtc && converter == null && srcType == typeof(DateTime) && (dstType == typeof(DateTime) || dstType == typeof(DateTime?)))
            {
                converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };
            }
            if ((srcType == typeof(Guid?) || srcType == typeof(Guid)) && dstType == typeof(string))
            {
                converter = delegate(object src)
                {
                    if (src == null) return null;
                    return src.ToString();
                };
            }

            // Forced type conversion including integral types -> enum
            if (converter == null)
            {
                if (dstType.IsEnum && IsIntegralType(srcType))
                {
                    if (srcType != typeof(int))
                    {
                        converter = delegate(object src) { return Convert.ChangeType(src, typeof(int), null); };
                    }
                }
                else if (!dstType.IsAssignableFrom(srcType))
                {
                    converter = delegate(object src) { return Convert.ChangeType(src, dstType, null); };
                }
            }
            return converter;
        }


        static T RecurseInheritedTypes<T>(Type t, Func<Type, T> cb)
        {
            while (t != null)
            {
                T info = cb(t);
                if (info != null)
                    return info;
                t = t.BaseType;
            }
            return default(T);
        }


        static Dictionary<Type, PocoData> m_PocoDatas = new Dictionary<Type, PocoData>();
        static List<Func<object, object>> m_Converters = new List<Func<object, object>>();
        static MethodInfo fnGetValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });
        static MethodInfo fnIsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
        static FieldInfo fldConverters = typeof(PocoData).GetField("m_Converters", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
        static MethodInfo fnListGetItem = typeof(List<Func<object, object>>).GetProperty("Item").GetGetMethod();
        static MethodInfo fnInvoke = typeof(Func<object, object>).GetMethod("Invoke");
        public Type type;
        public string[] QueryColumns { get; private set; }
        public TableInfo TableInfo { get; private set; }
        public Dictionary<string, PocoColumn> Columns { get; private set; }
        public Dictionary<string, PocoColumn> IgnoreColumns { get; private set; }
        Dictionary<string, Delegate> PocoFactories = new Dictionary<string, Delegate>();
    }
}
