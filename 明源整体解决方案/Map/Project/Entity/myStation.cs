using System;
using System.Data;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel.DataAnnotations;
namespace Mysoft.Project.Entity
{
	/// <summary>
	///岗位
	/// </summary>
	public partial class myStation
	{
		public myStation()
		{
			this.IsProjManager = 0 ;
		}
		
		/// <summary>
		///岗位GUID
		/// </summary>
		[ID]
		[DbType(SqlDbType.UniqueIdentifier)]
		public String StationGUID { get; set; }
		
		/// <summary>
		///岗位名称
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String StationName { get; set; }
		
		/// <summary>
		///岗位类型
		/// </summary>
		[DbType(SqlDbType.TinyInt)]
		public Byte? StationType { get; set; }
		
		/// <summary>
		///备注
		/// </summary>
		[StringLength(200)]
		[DbType(SqlDbType.VarChar)]
		public String Memo { get; set; }
		
		/// <summary>
		///岗位代码
		/// </summary>
		[StringLength(16)]
		[DbType(SqlDbType.VarChar)]
		public String StationCode { get; set; }
		
		/// <summary>
		///排序代码
		/// </summary>
		[StringLength(16)]
		[DbType(SqlDbType.VarChar)]
		public String OrderCode { get; set; }
		
		/// <summary>
		///上级岗位
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ParentStationGUID { get; set; }
		
		/// <summary>
		///岗位层级编码
		/// </summary>
		[StringLength(500)]
		[DbType(SqlDbType.VarChar)]
		public String HierarchyCode { get; set; }
		
		/// <summary>
		///所属公司
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String BUGUID { get; set; }
		
		/// <summary>
		///相关项目
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ProjGUID { get; set; }
		
		/// <summary>
		///是否项目管理员
		/// </summary>
		[DbType(SqlDbType.TinyInt)]
		public Byte IsProjManager { get; set; }
		
		/// <summary>
		///通用岗位GUID
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String GlobalStationGUID { get; set; }
		
		/// <summary>
		///所属公司
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String CompanyGUID { get; set; }
		
	}
}
