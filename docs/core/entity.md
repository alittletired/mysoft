## API

+ [Ajax](ajax.md)

+ [DBHelper](dbhelper.md)

+ [Entity](entity.md)



## entity
实体类使用的T4模版进行实体类的自动生成

安装dist/lib中的t4toolbox,将[Entity.tt](Entity.tt)文件拷贝到项目中（如Mysoft.Cbgl.Entity）

编辑[Entity.tt](Entity.tt)，指定 数据库连接字符串,所属命名空间,表名数组 后,ctrl+s看效果吧:)

生成的实体只是简单的poco，无基类继承


**数据库中的guid类型将转换成string，如需修改，编辑MySoft.Project.Core.Entity.EntityGenerator中GetFiledType方法**
