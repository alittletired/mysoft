+ [Excel](Excel.md)

## ExcelHelper


ExcelHelper是一个通用的导出Excel解决方案

**设计通用Excel的目的就是为了避免重复工作，
也就是说不必因为Excel的样式、
数据等变化而重新从零做起、重复劳动。
因此我们就必须抽取一个通用的东西出来，
使用时只需要关注相关的业务而不必过度关注相关excel操作和存储。**

##Quick start

+ 代码调用
```C#  

	ExcelHelper excel = new ExcelHelper();
	/// <summary>
	/// 导出Excel
	/// </summary>
	/// <param name="templateFilePath">Excel模版文件路径:如D:\map\upfiles\temp.xls</param>
	/// <param name="data">匹配Exceml模版的数据源</param>
	/// <returns>导出Excel的相对路径</returns>
	string exportedFilePath = excel.ExportExcel(templateFilePath[string], data[DataSet]);
	返回的是生成的文件路径exportedFilePath
```


+ 模版填写    
   正则支持$table:表序号或表名,column:字段名#
           $each:循环模式#（循环模式row,col）
           $lock:是否锁定字段名，值为1时锁定，值为0时不锁定#或直接指定$（符号）lock:1#锁定
   所有正则关键字只支持小写
   如：

   <table style="width:200px;border:1px solid #e1e1e1;">
	<tr>
		<td colspan="3">课程：$table:0,column:CourseName# 课时：$table:0,column:Period#</td>
	</tr>
	<tr>
		<th>报名学生</th>
		<th>班级</th>
		<th>专业</th>
	</tr>
	<tr>
		<td>$table:1,column:StudantName#$each:row#</td>
		<td>$table:1,column:Class#$each:row#</td>
		<td>$table:1,column:Major#$each:row#</td>
	</tr>
	<tr>
		<td>使用教室：$table:2,column:ClassRoom# 教师：$table:2,column:Teacher#$each:col#</td>
		<td></td>
		<td></td>
	<tr>
	</table>   
