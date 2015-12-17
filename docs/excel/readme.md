+ [Excel](Excel.md)

在Excel导入导出中将会生成excel的元数据，并隐藏在生成的的excel文档中
，可以通过Datatable或者实体集合来导出导入

模版支持语法，
模版规则：

1. 模版绑定都包裹在#{}中

2. 支持列表绑定，在需要循环的列开头模版中添加 each:table，
数据源是可遍历的数据集合，如datatable，list等

3. 绑定字段直接使用#{字段名}即可，也可以使用模版的完整语法#{value:字段名}

## 模版语法：

toJSONString : function(filter){return JSON.stringify(this,filter)},
	parseJSON : function(filter){return JSON.parse(this,filter)}

| 属性     | 说明     |值
| :------------- | :------------- |:----|
| each |  绑定表格  |数据源中的列表字段，如果绑定数据源本身，则使用$root
| bind |  设置单元格绑定的字段   | null
| treeCode |  设置树型code绑定字段，在each节点中有效  | null
| islock |  设置是否锁定单元格  | 0,1
| ishide |  设置是否隐藏单元格,值  | 0,1

![exceltemplate image](exceltemplate.png)

##Quick start

+ 导出用法
后台定义数据源,
```C#  
public DataTable ExportDateExcel(string filter, string xml)
{
	var sql = @" SELECT  * FROM		 ep_room		
		 WHERE	  (1=1)		 AND (2=2)		 ORDER BY  RoomCode";
	var filtersql = Mysoft.Map.Utility.General.XML2Filter(filter, "ep_Room", "RoomGUID");
	sql = sql.Replace("2=2", filtersql);
	return DBHelper.GetDataTable(sql);
}
```

```javascript
var exceloptions = {
    serviceMethod: 'Mysoft.Kfxt.Services.JFFJGLService.ExportDateExcel' //后台数据提供方法
      , params: getExcelParam  //传递给serviceMethod后台方法的参数,可以是对象或者函数,函数必须返回对象
      , fileName: '20151211.xls'//导出的文件名称
      , templateFile: '/Kfxt/ZSJF/JFFJGL_Grid_template.xls' //模版文件地址
}
function getExcelParam() {
    return { xml: $('#__xml').val(), filter: $('#__filter').val() }
}
    function ExportDateExcel() {
        seajs.use('Excel', function(excel) {

        excel.exporttExcel(exceloptions);
        });
    }

```


+ 导入

```C#
public string ImportDateExcel(List<JFFJGLDateDTO> data)
{
	StringBuilder sb = new StringBuilder();
	foreach (var item in data)
	{
		var CFHDate = string.IsNullOrEmpty(item.CFHDate) ? "null" : "'" + item.CFHDate + "'";			 
		sb.AppendLine("update s_Contract set CFHDate=" + CFHDate + " where ContractGUID='" + item.ContractGUID + "';");


	}
	DBHelper.Execute(sb.ToString());
	return string.Empty;
}

 ```

 ```javascript
 var importoptions = {
	   serviceMethod: 'Mysoft.Kfxt.Services.JFFJGLService.ImportDateExcel' //后台导入提供方法
	 , params: {}  //传递给serviceMethod后台方法的参数,可以是对象或者函数,函数必须返回对象
	 , docType: '交付房间日期' //上传文档的类型,p_document表中docType
	 , id: '' //可选,p_document表中refguid
   }
   function ImportDateExcel() {
	   seajs.use('Excel', function(excel) {
		   var err = excel.importExcel(importoptions);
		   if (err)
			   return alert(err);
		   else {
			   alert("导入成功")
		   }
		   appGrid.frameElement.Query();
	   });
   }

 ```
