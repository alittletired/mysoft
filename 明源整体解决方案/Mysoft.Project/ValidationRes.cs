using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mysoft.Project.Validation
{
  public static  class ValidationRes
    {
      public static readonly string email_error = "'{PropertyName}' 不是有效的电子邮件地址。";
      public static readonly string equal_error = "'{PropertyName}' 应该和 '{ComparisonValue}' 相等。";
      public static readonly string exact_length_error = "'{PropertyName}' 必须是 {MaxLength} 个字符，您已经输入了 {TotalLength} 字符。";
      public static readonly string exclusivebetween_error = "'{PropertyName}' 必须在 {From} 和 {To} 之外， 您输入了 {Value}。";
      public static readonly string greaterthan_error = "'{PropertyName}' 必须大于 '{ComparisonValue}'。";
      public static readonly string greaterthanorequal_error = "'{PropertyName}' 必须大于或等于 '{ComparisonValue}'。";
      public static readonly string inclusivebetween_error = "'{PropertyName}' 必须在 {From} 和 {To} 之间， 您输入了 {Value}。";
      public static readonly string length_error = "'{PropertyName}' 的长度必须在 {MinLength} 到 {MaxLength} 字符，您已经输入了 {TotalLength} 字符。";
      public static readonly string lessthan_error = "'{PropertyName}' 必须小于 '{ComparisonValue}'。";
      public static readonly string lessthanorequal_error = "'{PropertyName}' 必须小于或等于 '{ComparisonValue}'。";
      public static readonly string notempty_error = "请填写 '{PropertyName}'。";
      public static readonly string regex_error = "'{PropertyName}' 的格式不正确。";
    }
}
