using System;
using System.Data;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel.DataAnnotations;
namespace Mysoft.Project.Entity
{
	/// <summary>
	///商业单位
	/// </summary>
	public partial class myBusinessUnit
	{
		public myBusinessUnit()
		{
			this.IsEndCompany = 0 ;
			this.IsCompany = 0 ;
			this.Level = 0 ;
			this.BUType = 0 ;
			this.KGRate = 0 ;
			this.IsFc = 0 ;
		}
		
		/// <summary>
		///单位GUID
		/// </summary>
		[ID]
		[DbType(SqlDbType.UniqueIdentifier)]
		public String BUGUID { get; set; }
		
		/// <summary>
		///单位简称
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String BUName { get; set; }
		
		/// <summary>
		///单位全称
		/// </summary>
		[StringLength(100)]
		[DbType(SqlDbType.VarChar)]
		public String BUFullName { get; set; }
		
		/// <summary>
		///单位代码
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String BUCode { get; set; }
		
		/// <summary>
		///层级代码
		/// </summary>
		[StringLength(500)]
		[DbType(SqlDbType.VarChar)]
		public String HierarchyCode { get; set; }
		
		/// <summary>
		///父级GUID
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ParentGUID { get; set; }
		
		/// <summary>
		///网址
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String WebSite { get; set; }
		
		/// <summary>
		///传真
		/// </summary>
		[StringLength(20)]
		[DbType(SqlDbType.VarChar)]
		public String Fax { get; set; }
		
		/// <summary>
		///公司地址
		/// </summary>
		[StringLength(100)]
		[DbType(SqlDbType.VarChar)]
		public String CompanyAddr { get; set; }
		
		/// <summary>
		///营业执照
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String Charter { get; set; }
		
		/// <summary>
		///法人代表
		/// </summary>
		[StringLength(20)]
		[DbType(SqlDbType.VarChar)]
		public String CorporationDeputy { get; set; }
		
		/// <summary>
		///创建时间
		/// </summary>
		[DbType(SqlDbType.DateTime)]
		public DateTime? CreatedOn { get; set; }
		
		/// <summary>
		///修改时间
		/// </summary>
		[DbType(SqlDbType.DateTime)]
		public DateTime? ModifiedOn { get; set; }
		
		/// <summary>
		///创建人
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String CreatedBy { get; set; }
		
		/// <summary>
		///说明
		/// </summary>
		[StringLength(500)]
		[DbType(SqlDbType.VarChar)]
		public String Comments { get; set; }
		
		/// <summary>
		///修改人
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ModifiedBy { get; set; }
		
		/// <summary>
		///是否末级公司
		/// </summary>
		[DbType(SqlDbType.Bit)]
		public Byte IsEndCompany { get; set; }
		
		/// <summary>
		///是否公司
		/// </summary>
		[DbType(SqlDbType.Bit)]
		public Byte IsCompany { get; set; }
		
		/// <summary>
		///层级数
		/// </summary>
		[DbType(SqlDbType.Int)]
		public Int32 Level { get; set; }
		
		/// <summary>
		///组织类型
		/// </summary>
		[DbType(SqlDbType.TinyInt)]
		public Byte BUType { get; set; }
		
		/// <summary>
		///关联项目
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ProjGUID { get; set; }
		
		/// <summary>
		///排序代码
		/// </summary>
		[StringLength(20)]
		[DbType(SqlDbType.VarChar)]
		public String OrderCode { get; set; }
		
		/// <summary>
		///排序层级代码
		/// </summary>
		[StringLength(500)]
		[DbType(SqlDbType.VarChar)]
		public String OrderHierarchyCode { get; set; }
		
		/// <summary>
		///当前公司
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String CompanyGUID { get; set; }
		
		/// <summary>
		///单位简称路径
		/// </summary>
		[StringLength(1000)]
		[DbType(SqlDbType.VarChar)]
		public String NamePath { get; set; }
		
		/// <summary>
		///费用责任人
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String FyStationGUID { get; set; }
		
		/// <summary>
		///预算编制人岗位名
		/// </summary>
		[StringLength(1000)]
		[DbType(SqlDbType.VarChar)]
		public String RefStationName { get; set; }
		
		/// <summary>
		///控股比例
		/// </summary>
		[DbType(SqlDbType.Money)]
		public Decimal KGRate { get; set; }
		
		/// <summary>
		///主系统关联公司guid
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String mainbuguid { get; set; }
		
		[DbType(SqlDbType.Int)]
		public Int32 IsFc { get; set; }
		
	}
}
