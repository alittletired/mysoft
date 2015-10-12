
# 项目小平台
项目小平台是项目开发中常用的场景进行提炼和封装

目标：简化开发人员的日常工作，让开发人员能专注与业务开发，代码应该是

+ 简单灵活，学习成本低

+ 可理解，约定大于配置

+ 可维护，代码零依赖或少依赖，满足单一职责，代码耦合度低

+ 可调错，出错信息对开发人员友好，尽量还原错误现场


## API

+ [Core](/docs/core)

+ [Control](/docs/control)

## Quick start
+ 定义后台服务类和方法

```C#
namespace Mysoft.Cbgl.Services
{
    public class MonthPlanService
    {
        public string GetWorkflowProcessGUID(string planMonth, string buguid)
        {
            string sql = @"select ProcessGUID from myWorkflowProcessEntity where  IsHistory=0 and BusinessGUID in
                        ( select top 1 MpProcessGUID from  cb_MonthPlan  where planmonth=@0 and buguid=@1)";
            string mpProcessGUID = DBHelper.ExecuteScalarString(sql, planMonth, buguid);
            return mpProcessGUID;
        }
    }
    //more...
}
```

+ 在头部添加jquery和需要调用的后台服务类型
`<head>`:

```html
<script type="text/javascript" src="/Project/js/jquery.js" ></script>
<script type="text/javascript" src="/Project/ajax.aspx?type=Mysoft.Cbgl.Services.MonthPlanService"></script>
```

+ 在前台脚步调用服务

```javascript
function doSendProcess() {
		var mpProcessGUID = MonthPlanService.GetMpProcessGUID($('#__planMonth').val(), $('#txtBUGUID').val());
		initiateBusinessProcess(mpProcessGUID, '资金计划审批');
	}
```
