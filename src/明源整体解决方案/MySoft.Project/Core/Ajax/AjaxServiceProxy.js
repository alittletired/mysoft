function AjaxServiceProxy(window, type, serverUrl) {   
    var service = {};
    service._typeName = type;
    service._serverUrl = serverUrl;
    service.parseParam = function(data, paramName, paramVal) {
        if (!window.$ && window.location.host.indexOf('localhost') > 0) { alert('人生苦短，我用jquery '); };
//        if ($.isArray(paramVal)) {
//            data[paramName] = JSON.stringify(paramVal);
//        }
//        else 
        if ($.isFunction(paramVal)) { }
        else if (typeof paramVal === 'object') {
            $.extend(data, paramVal);
        }
        else if (typeof data[paramName] === 'undefined') {
            data[paramName] = paramVal;
        }

    }
    service.registerMethod = function(methodName, methodParams) {
        var arrParams = methodParams.split(",");
        service[methodName] = function() {
            var data = {};
            var callback = null;
            for (var i = 0; i < arrParams.length; i++) {
                service.parseParam(data, arrParams[i], arguments[i]);
            }
            if (arguments.length > 0 && $.isFunction(arguments[arguments.length - 1])) {
                callback = arguments[arguments.length - 1];
            }
            var invokeMethod = this._typeName + '.' + methodName;
            data.serverUrl = service._serverUrl;
            var sRtn = my.project.invoke(invokeMethod, data, callback);
            return sRtn;
        }
    }

    if (typeof module === 'object' && typeof module.exports === 'object') { module.exports = service; }
    if (typeof define === 'function') { define(function() { return service; }); }
    var serviceNames = type.split('.');
    window[serviceNames[serviceNames.length - 1]] = service;
    return service;
}


//兼容小平台传参调用
window.my = window.my || {};
my.project = my.project || {};
my.project.invoke = function(method, option, callback) {
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