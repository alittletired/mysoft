
var c = { charset: 'gb2312' }
seajs.config(c);
define(function(require) {
    require('/_controls/upfile/UpFile.js')
    var project = require('project')
    var Excel = {};
    Excel.exporttExcel = function(options) {
        options = options || {}
        if (!options.fileName) {
            return alert("必须定义fileName文件名");
        }
        if (!options.serviceMethod) {
            return alert("必须定义serviceMethod后台导出方法")
        }
        var CustomerParam;
        if (typeof options.params === 'function')
            CustomerParam = options.params();
        else
            CustomerParam = options.params;
        var sPageURL = window.location.search.substring(1);

        //将页面url的参数传递到后台
        var urlOption = {};
        var sURLVariables = sPageURL.split('&');
        for (var i = 0; i < sURLVariables.length; i++) {
            var p = sURLVariables[i];
            var index = p.indexOf('=');
            if (index > 0) {
                urlOption[p.substr(0, index)] = p.substr(index + 1)
            }


        }

        CustomerParam = JSON.stringify($.extend({}, urlOption, CustomerParam));
        var dto = $.extend({}, options, { CustomerParam: CustomerParam });
        var res = project.invoke('Mysoft.Project.Excel.ExcelService.ExportExcel', { dto: dto });
        if (res.err)
            return alert(res.err);
        //      options.fileName += '.xls';
        OpenDownloadWin(options.fileName, res.data)
    }

    Excel.importExcel = function(options) {
        options = options || {}
        if (!options.serviceMethod) {
            return alert("必须定义serviceMethod后台导入方法")
        }
        if (!options.docType) {
            return alert("必须定义docType")
        }
        options.id = options.id || project.generateUUID();
        //  OpenUpFileWinMap
        __UpFile(options.id, options.docType, "");
        var res = project.invoke('Mysoft.Project.Excel.ExcelService.ImportExcel', { dto: options });
        if (res.err)
            return alert(res.err);

        return res.data;
    }

    return Excel;
});



