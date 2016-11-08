using System;
using System.Data;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel.DataAnnotations;
namespace Mysoft.Project.Entity
{
	/// <summary>
	///��ҵ��λ
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
		///��λGUID
		/// </summary>
		[ID]
		[DbType(SqlDbType.UniqueIdentifier)]
		public String BUGUID { get; set; }
		
		/// <summary>
		///��λ���
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String BUName { get; set; }
		
		/// <summary>
		///��λȫ��
		/// </summary>
		[StringLength(100)]
		[DbType(SqlDbType.VarChar)]
		public String BUFullName { get; set; }
		
		/// <summary>
		///��λ����
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String BUCode { get; set; }
		
		/// <summary>
		///�㼶����
		/// </summary>
		[StringLength(500)]
		[DbType(SqlDbType.VarChar)]
		public String HierarchyCode { get; set; }
		
		/// <summary>
		///����GUID
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ParentGUID { get; set; }
		
		/// <summary>
		///��ַ
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String WebSite { get; set; }
		
		/// <summary>
		///����
		/// </summary>
		[StringLength(20)]
		[DbType(SqlDbType.VarChar)]
		public String Fax { get; set; }
		
		/// <summary>
		///��˾��ַ
		/// </summary>
		[StringLength(100)]
		[DbType(SqlDbType.VarChar)]
		public String CompanyAddr { get; set; }
		
		/// <summary>
		///Ӫҵִ��
		/// </summary>
		[StringLength(50)]
		[DbType(SqlDbType.VarChar)]
		public String Charter { get; set; }
		
		/// <summary>
		///���˴���
		/// </summary>
		[StringLength(20)]
		[DbType(SqlDbType.VarChar)]
		public String CorporationDeputy { get; set; }
		
		/// <summary>
		///����ʱ��
		/// </summary>
		[DbType(SqlDbType.DateTime)]
		public DateTime? CreatedOn { get; set; }
		
		/// <summary>
		///�޸�ʱ��
		/// </summary>
		[DbType(SqlDbType.DateTime)]
		public DateTime? ModifiedOn { get; set; }
		
		/// <summary>
		///������
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String CreatedBy { get; set; }
		
		/// <summary>
		///˵��
		/// </summary>
		[StringLength(500)]
		[DbType(SqlDbType.VarChar)]
		public String Comments { get; set; }
		
		/// <summary>
		///�޸���
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ModifiedBy { get; set; }
		
		/// <summary>
		///�Ƿ�ĩ����˾
		/// </summary>
		[DbType(SqlDbType.Bit)]
		public Byte IsEndCompany { get; set; }
		
		/// <summary>
		///�Ƿ�˾
		/// </summary>
		[DbType(SqlDbType.Bit)]
		public Byte IsCompany { get; set; }
		
		/// <summary>
		///�㼶��
		/// </summary>
		[DbType(SqlDbType.Int)]
		public Int32 Level { get; set; }
		
		/// <summary>
		///��֯����
		/// </summary>
		[DbType(SqlDbType.TinyInt)]
		public Byte BUType { get; set; }
		
		/// <summary>
		///������Ŀ
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String ProjGUID { get; set; }
		
		/// <summary>
		///�������
		/// </summary>
		[StringLength(20)]
		[DbType(SqlDbType.VarChar)]
		public String OrderCode { get; set; }
		
		/// <summary>
		///����㼶����
		/// </summary>
		[StringLength(500)]
		[DbType(SqlDbType.VarChar)]
		public String OrderHierarchyCode { get; set; }
		
		/// <summary>
		///��ǰ��˾
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String CompanyGUID { get; set; }
		
		/// <summary>
		///��λ���·��
		/// </summary>
		[StringLength(1000)]
		[DbType(SqlDbType.VarChar)]
		public String NamePath { get; set; }
		
		/// <summary>
		///����������
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String FyStationGUID { get; set; }
		
		/// <summary>
		///Ԥ������˸�λ��
		/// </summary>
		[StringLength(1000)]
		[DbType(SqlDbType.VarChar)]
		public String RefStationName { get; set; }
		
		/// <summary>
		///�عɱ���
		/// </summary>
		[DbType(SqlDbType.Money)]
		public Decimal KGRate { get; set; }
		
		/// <summary>
		///��ϵͳ������˾guid
		/// </summary>
		[DbType(SqlDbType.UniqueIdentifier)]
		public String mainbuguid { get; set; }
		
		[DbType(SqlDbType.Int)]
		public Int32 IsFc { get; set; }
		
	}
}
