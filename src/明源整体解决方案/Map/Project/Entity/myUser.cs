using System;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel.DataAnnotations;
namespace Mysoft.BusinessPortal.Entity
{
	/// <summary>
	///用户表
	/// </summary>
	public partial class myUser
	{
		public myUser()
		{
			this.IsDisabeld  =  0 ;
			this.IsAdmin  =  0 ;
			this.IsSaler  =  0 ;
			this.IsAdvanceUser  =  0 ;
			this.UserKind  =  0 ;
			this.IsUserChangePWD  =  0 ;
			this.IsLocked  =  0 ;
			this.IsMobileUser  =  0 ;
		}
		
		/// <summary>
		///用户GUID
		/// </summary>
		[ID]
		public String  UserGUID { get; set; }
		
		/// <summary>
		///用户代码
		/// </summary>
		[StringLength(16)]
		public String  UserCode { get; set; }
		
		/// <summary>
		///用户名称
		/// </summary>
		[StringLength(20)]
		public String  UserName { get; set; }
		
		/// <summary>
		///所属公司
		/// </summary>
		public String  BUGUID { get; set; }
		
		/// <summary>
		///照片地址
		/// </summary>
		[StringLength(50)]
		public String  PhotoUrl { get; set; }
		
		/// <summary>
		///职位
		/// </summary>
		[StringLength(50)]
		public String  JobTitle { get; set; }
		
		/// <summary>
		///电子邮箱
		/// </summary>
		[StringLength(50)]
		public String  Email { get; set; }
		
		/// <summary>
		///公司电话
		/// </summary>
		[StringLength(20)]
		public String  OfficePhone { get; set; }
		
		/// <summary>
		///移动电话
		/// </summary>
		[StringLength(20)]
		public String  MobilePhone { get; set; }
		
		/// <summary>
		///家庭电话
		/// </summary>
		[StringLength(20)]
		public String  HomePhone { get; set; }
		
		/// <summary>
		///AD登陆帐号
		/// </summary>
		[StringLength(50)]
		public String  ADAccount { get; set; }
		
		/// <summary>
		///创建时间
		/// </summary>
		public DateTime?  CreatedOn { get; set; }
		
		/// <summary>
		///修改时间
		/// </summary>
		[StringLength(50)]
		public String  ModifiedOn { get; set; }
		
		/// <summary>
		///创建人
		/// </summary>
		public String  CreatedBy { get; set; }
		
		/// <summary>
		///修改人
		/// </summary>
		public String  ModifiedBy { get; set; }
		
		/// <summary>
		///是否禁用
		/// </summary>
		public Int16  IsDisabeld { get; set; }
		
		/// <summary>
		///禁用原因
		/// </summary>
		[StringLength(200)]
		public String  DisabledReason { get; set; }
		
		/// <summary>
		///备注
		/// </summary>
		[StringLength(200)]
		public String  Comments { get; set; }
		
		/// <summary>
		///所属公司2
		/// </summary>
		public String  ParentGUID { get; set; }
		
		/// <summary>
		///密码
		/// </summary>
		[StringLength(50)]
		public String  Password { get; set; }
		
		/// <summary>
		///是否超级用户
		/// </summary>
		public Int16  IsAdmin { get; set; }
		
		/// <summary>
		///是否销售员
		/// </summary>
		public Int16  IsSaler { get; set; }
		
		/// <summary>
		///工号
		/// </summary>
		[StringLength(50)]
		public String  JobNumber { get; set; }
		
		/// <summary>
		///所属部门
		/// </summary>
		public String  DepartmentGUID { get; set; }
		
		/// <summary>
		///用户项目过滤
		/// </summary>
		[StringLength(16)]
		public String  UserProject { get; set; }
		
		/// <summary>
		///是否高级用户
		/// </summary>
		public Int16  IsAdvanceUser { get; set; }
		
		/// <summary>
		///默认岗位
		/// </summary>
		public String  DefaultStation { get; set; }
		
		/// <summary>
		///用户类型
		/// </summary>
		public Byte  UserKind { get; set; }
		
		/// <summary>
		///密码修改时间
		/// </summary>
		public DateTime?  PSWModifyTime { get; set; }
		
		/// <summary>
		///是否用户自己更改密码
		/// </summary>
		public Byte  IsUserChangePWD { get; set; }
		
		/// <summary>
		///是否被锁定
		/// </summary>
		public Byte  IsLocked { get; set; }
		
		/// <summary>
		///是否移动用户
		/// </summary>
		public Byte  IsMobileUser { get; set; }
		
		public DateTime?  LockTime { get; set; }
		
	}
}
