+ [DDTree](ddtree.md)

## DDTree


DDTree是一个通用的选择公司，部门，项目控件

**由于平台只提供末级公司有过滤权限的末级项目，如要使用集团级或者区域公司的末级项目，
需要改造存储过程，存储过程的文件路径/project/ddtree/usp_myProjFilterBase.sql**

##Quick start

+ 在页面上使用位置添加div

```html
<div id="ddtree"></div>
<script type="text/javascript" src="/project/js/sea.js"></script>
```

+ 在页面底部添加调用
```javascript  

    var TreeType = { None: -1, Group: 0, Company: 10, EndCompany: 20, Dept: 30, Project: 60, EndProject: 70 }    
    var option = {
              onchange: false  //选择元素后的回调函数
             , treeType: TreeType.EndProject //数据加载类型
             , selectType: TreeType.EndCompany //可以选择的类型
             , showType: TreeType.EndProject //默认展开类型，-1不展开            
              , applySys: '0201' //业务系统
    }
    option.onchange = function(item) {
      //ddtree._selItem //当前选择的值
     //   alert(item.type + "----" + item.id + "-----" + item.name);

}
var ddtree=null;
seajs.use('DDTree', function(DDTree) {
    ddtree = new ddtree('ddtree', option);

});

```


options
====

```javascript
var TreeType = { None: -1, Group: 0, Company: 10, EndCompany: 20, Dept: 30, Project: 60, EndProject: 70 }  
var option= {
            onchange: false  //选择元素后的回调函数
           , showSearch: true     //是否显示搜索框
            , treeType: TreeType.EndProject //数据加载类型
            , selectType: TreeType.EndCompany //可以选择的类型
            , showType: TreeType.EndProject //默认展开类型，-1不展开
            , showGroup: true  //是否显示集团
            , showCompany: true //是否显示区域公司
             , applySys: '0201' //业务系统
            , serviceMethod: 'MySoft.Project.Control.DDTreeService.GetDDTreeData' //后台数据提供方法
            , autoSwitchCompany: true //是否自动切换公司

}


```

method&properties
=====
\_selItem
________
当前控件选择的item


setValue(codeorid)
________
设置控件的值，传入code或者id
