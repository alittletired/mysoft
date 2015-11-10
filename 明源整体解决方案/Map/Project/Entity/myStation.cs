using System;
using System.Data;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel.DataAnnotations;
namespace Mysoft.Project.Entity
{
	/// <summary>
	///��λ
	/// </summary>
	public partial class myStation
	{
		public myStation()
		{
			this.IsProjManager = 0 ;
		}
		
		/// <summary>
		///��λGUID
		/// </summary>
		[ID]
		[DbType(SqlDbType.UniqueIdentifier)]
		public String StationGUID { get; set; }
		
		/// <summary>
		///��λ����
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String StationName { get; set; }
		
		/// <summary>
		///��λ����
		/// </summary>
		[DbType(SqlDbType.TinyInt)]
		public Byte? StationType { get; set; }
		
		/// <summary>
		///��ע
		/// </summary>
		[StringLength(200)]
		[DbType(SqlDbType.VarChar)]
		public String Memo { get; set; }
		
		/// <summary>
		///��λ����
		/// </summary>
		[StringLength(16)]
		[DbType(SqlDbType.VarChar)]
		public String StationCode { get; set; }
		
		/// <summary>
		///�������
		/// </summary>
		[StringLength(16)]
		[DbType(SqlDbType.VarChar)]
		public String OrderCode { get; set; }
		
		/// <summary>
		///�ϼ���λ
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ParentStationGUID { get; set; }
		
		/// <summary>
		///��λ�㼶����
		/// </summary>
		[StringLength(500)]
		[DbType(SqlDbType.VarChar)]
		public String HierarchyCode { get; set; }
		
		/// <summary>
		///������˾
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String BUGUID { get; set; }
		
		/// <summary>
		///�����Ŀ
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ProjGUID { get; set; }
		
		/// <summary>
		///�Ƿ���Ŀ����Ա
		/// </summary>
		[DbType(SqlDbType.TinyInt)]
		public Byte IsProjManager { get; set; }
		
		/// <summary>
		///ͨ�ø�λGUID
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String GlobalStationGUID { get; set; }
		
		/// <summary>
		///������˾
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String CompanyGUID { get; set; }
		
	}
}
