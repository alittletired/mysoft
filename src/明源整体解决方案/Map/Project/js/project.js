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
                else
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


    project.showPopup = function(src, html, popupWidth, popupHeight, offsetx, offsety) {
        offsetx = offsetx || 0;
        offsety = offsety || 0;
        var x = src.clientLeft;
        var y = src.clientTop + $(src).height()
        src._oPopUp = src._oPopUp || window.createPopup();
        var popup = src._oPopUp;
        popup.document.body.innerHTML = html
        //获取浏览器窗口可视范围的高度和宽度
        var iWidth, iHeight;
        try {
            iWidth = top.document.documentElement.offsetWidth;
            iHeight = top.document.documentElement.offsetHeight;
        }
        catch (e) {
            iWidth = window.screen.availWidth - 10;
            iHeight = window.screen.availHeight - 60;
        }

        //获取当前点击元素所在窗口在浏览器中的位置
        var frameLeft = 0;
        var frameTop = 0;
        var oWin = window;
        var pos;
        try {
            while (oWin.frameElement != null) {
                pos = oWin.frameElement.getBoundingClientRect();
                frameLeft += pos.left;
                frameTop += pos.top;

                oWin = oWin.parent;
            }
        }
        catch (e) { }
        //获取当前点击元素的位置
        pos = src.getBoundingClientRect();
        var xPos = pos.left;
        var yPos = pos.top;

        //对当前点击元素的位置进行判断,是否需要调整popup的位置
        if (frameLeft + xPos + x + popupWidth > iWidth) {
            x = iWidth - frameLeft - xPos - popupWidth;
        }
        if (frameLeft + xPos + x < 0) {
            x = 0 - frameLeft - xPos;
        }

        if (frameTop + yPos + y + popupHeight > iHeight) {
            y = iHeight - frameTop - yPos - popupHeight;
        }
        if (frameTop + yPos + y < 0) {
            y = 0 - frameTop - yPos;
        }

        // 显示 Popup 窗口
        popup.document.iLeft = frameLeft + xPos + x;
        popup.document.iTop = frameTop + yPos + y;
        //二级菜单定位
        if (src.ownerDocument.iLeft) {
            xPos = src.ownerDocument.iLeft;
            yPos = yPos + src.ownerDocument.iTop;
            if (frameLeft + xPos + x + popupWidth > iWidth) {
                x = 0 - popupWidth;
            }
            if (frameTop + yPos + y + popupHeight > iHeight) {
                y = yPos - popupHeight;
            }
            if (frameTop + yPos + y < 0) {
                y = 0;
            }
        }

        popup.show(x + offsetx, y + offsety, popupWidth, popupHeight, src);
        setTimeout(function() { popup.document.focus() }, 0);

        return popup;

    }
    project.hidePopup = function(ele) {
        if (ele._oPopUp && ele._oPopUp.hide)
            ele._oPopUp.hide();
    }

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

