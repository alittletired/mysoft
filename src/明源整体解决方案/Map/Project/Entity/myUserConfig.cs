using System;
using Mysoft.Project.Core.DataAnnotations;
using System.ComponentModel.DataAnnotations;
namespace Mysoft.BusinessPortal.Entity
{
	/// <summary>
	///用户设置
	/// </summary>
	public partial class myUserConfig
	{
		public myUserConfig()
		{
		}
		
		/// <summary>
		///用户GUID
		/// </summary>
		[ID]
		public String  UserGUID { get; set; }
		
		/// <summary>
		///部件默认汇总方式
		/// </summary>
		[StringLength(10)]
		public String  SummaryKind { get; set; }
		
	}
}
