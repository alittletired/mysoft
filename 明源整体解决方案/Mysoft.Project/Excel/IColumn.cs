using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mysoft.Project.Core
{
  
   public class ExcelColumn
   {
      

       public string Title
       {
          get;set;
       }

       public string Bind
       {
          get;set;
       }

       public int Width
       {
          get;set;
       }
       public string TreeCode { get; set; }

       public bool IsLock { get; set; }
       public bool IsHide { get; set; }
   }
}
