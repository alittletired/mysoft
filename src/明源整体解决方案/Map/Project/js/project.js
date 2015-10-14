(function() {
    var project = {};
    project.invoke = function(method, option, callback) {
        var invokeMethod = method;
        var data = option;
        if (typeof invokeMethod !== "string") {
            invokeMethod = method.serviceInfo;
            data = method.data || method
            callback = option;
        } else if ($.isFunction(option)) {
            callback = option;
            data = {}
        }
        serverUrl = data.serverUrl || '/project/ajax.aspx';
        var async = callback ? true : false;
        var returnValue;
        var ajaxdone = function(json) {
            if (json.__error__) {
                var parentWin = window.parent;
                while (parentWin && parentWin != parentWin.parent) {
                    parentWin.__error__ = json.__error__;
                    parentWin = parentWin.parent;
                }
                parentWin.__error__ = json.__error__;
                if (window.debug || window.location.host.indexOf('localhost') > -1)
                    alert(json.__error__);
                else if (!window.hiddenServiceError)
                    alert('操作出错，请联系系统管理员！');
                return;

            }
            if (callback) {
                returnValue = callback(json.result);
            }
            returnValue = json.result;
        }
        $.ajax({ url: serverUrl + '?invokeMethod=' + invokeMethod, contentType: 'application/x-www-form-urlencoded; charset=UTF-8', data: { postdata: JSON.stringify(data) }, async: async, type: 'POST', cache: false, dataType: 'json' }).done(ajaxdone);
        return returnValue;

    };  

    project.addStyle = function(styleText) {
        style = document.createElement('style');
        style.setAttribute("type", "text/css");
        if (style.styleSheet)
            style.styleSheet.cssText = styleText;
        else
            style.appendChild(document.createTextNode(styleText));
        document.getElementsByTagName('head')[0].appendChild(style);
    }


    //兼容小平台传参调用
    window.my = window.my || {};
    window.my.project = project;
    if (typeof define === 'function') {
        define(function(require) {
            require('jquery')
            return project;
        });
    }

})();

