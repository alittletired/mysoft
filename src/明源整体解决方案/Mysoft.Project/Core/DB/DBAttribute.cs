using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mysoft.Project.Core.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {

        public string Name { get; set; }
        public TableAttribute(string tableName) { Name = tableName; }
    }
    public enum RelationType { HasOne, HasMany, HasMany2Many }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class RelationAttribute : Attribute
    {
        public RelationAttribute(RelationType relationType)
        {

            RelationType = relationType;
        }

        public RelationAttribute(string table, string foreginKey, string childKey, RelationType relationType)
        {
            Table = table;
            ParentKey = foreginKey;
            ChildKey = childKey;
            RelationType = relationType;
        }

        public string Table { get; set; }
        public string ParentKey { get; set; }
        public string ChildKey { get; set; }
        public RelationType RelationType { get; private set; }

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HasManyAttribute : RelationAttribute
    {
        public HasManyAttribute(string foreginKey)
            : base(RelationType.HasMany)
        {

            ParentKey = foreginKey;
        }
        public HasManyAttribute(string childTable,string childKey)
            : base(RelationType.HasMany)
        {
            Table = childTable;
            ChildKey = childKey;
        }

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HasMany2ManyAttribute : RelationAttribute
    {
        public HasMany2ManyAttribute(string midTable)
            : base( RelationType.HasMany2Many)
        {
            Table = midTable;
        }

    }

    public class HasOneAttribute : RelationAttribute
    {

        public HasOneAttribute(string parentKey)
            : base(RelationType.HasOne)
        {
            ParentKey = parentKey;
        }


    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IDAttribute : ColumnAttribute
    {

        public IDAttribute(string columnName)
            : base(columnName)
        {

        }
        public IDAttribute() { }
       bool _autoIncrement=true;
       public bool AutoIncrement { get { return _autoIncrement; } set { _autoIncrement = value; } }

    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
     public class IgnoreAttribute : ColumnAttribute
    {

    }
   


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute()
        { }
        public ColumnAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }

 
   
    // Poco's marked [Explicit] require all column properties to be marked
    [AttributeUsage(AttributeTargets.Class)]
    public class ExplicitColumnsAttribute : Attribute
    {
    }



    // For explicit pocos, marks property as a result column and optionally supplies column name
    [AttributeUsage(AttributeTargets.Property)]
    public class ResultColumnAttribute : ColumnAttribute
    {
        public ResultColumnAttribute() { }
        public ResultColumnAttribute(string name) : base(name) { }
    }



    [AttributeUsage(AttributeTargets.Property)]
    public class AutoJoinAttribute : Attribute
    {
        public AutoJoinAttribute() { }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class TransactionAttribute : Attribute{
        public bool IsOpen { get; set; }
        public TransactionAttribute() {
            IsOpen = true;
        }
        
       
    }
    // Results from paged request
    public class Page<T>
    {
        public long CurrentPage { get; set; }
        public long TotalPages { get; set; }
        public long TotalItems { get; set; }
        public long ItemsPerPage { get; set; }
        public List<T> Items { get; set; }
        public object Context { get; set; }
    }

    // Pass as parameter value to force to DBType.AnsiString
    public class AnsiString
    {
        public AnsiString(string str)
        {
            Value = str;
        }
        public string Value { get; private set; }
    }

    // Used by IMapper to override table bindings for an object
    public class TableInfo
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public bool AutoIncrement { get; set; }
        public string SequenceName { get; set; }
    }

    // Optionally provide an implementation of this to Database.Mapper
    public interface IMapper
    {
        void GetTableInfo(Type t, TableInfo ti);
        bool MapPropertyToColumn(PropertyInfo pi, ref string columnName, ref bool resultColumn);
        Func<object, object> GetFromDbConverter(PropertyInfo pi, Type SourceType);
        Func<object, object> GetToDbConverter(Type SourceType);
    }

    // This will be merged with IMapper in the next major version
    public interface IMapper2 : IMapper
    {
        Func<object, object> GetFromDbConverter(Type DestType, Type SourceType);
    }


}
