using System;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel.DataAnnotations;
namespace Mysoft.BusinessPortal.Entity
{
	/// <summary>
	///我关注的公司
	/// </summary>
	public partial class myAttentionCompany
	{
		public myAttentionCompany()
		{
		}
		
		/// <summary>
		///主键
		/// </summary>
		public String  Id { get; set; }
		
		/// <summary>
		///用户GUID
		/// </summary>
		public String  UserGUID { get; set; }
		
		/// <summary>
		///公司GUID
		/// </summary>
		[StringLength(8000)]
		public String  CompanyGUID { get; set; }
		
	}
}
