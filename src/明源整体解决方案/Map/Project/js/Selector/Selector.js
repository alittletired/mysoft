define(function(require) {
    var project = require('../project')
    var Selector = {}
    Selector.open = function(options) {
        options = $.extend({}, Selector.DEFAULTS, options);

        var arrItem = window.showModalDialog('/project/js/Selector/Selector.htm?t=4',
       options, "dialogWidth:" + options.width + "px;dialogHeight:" + options.height + "px;help:0;status:0;scroll:0;center:1");
        if (arrItem) {
            var bindValues = {};
            for (var field in options.bindCtrls) {
                bindValues[field] = [];
            }
            $.each(arrItem, function() {
                for (var field in options.bindCtrls) {
                    bindValues[field].push(this[field]);
                }

            });

            for (var field in options.bindCtrls) {
                var ctrl = options.bindCtrls[field];
                if (typeof ctrl === 'string') {
                    ctrl = document.getElementById(ctrl) || appForm[ctrl];
                }
                ctrl.value = bindValues[field].join(',');
            }
            return arrItem;
        }
        return null;
    }

    var SelectorType = { grid: 0, treeGrid: 1 }
    //设置默认值
    Selector.DEFAULTS = {
        title: '' //弹出的选择窗口名称
            , bindCtrls: { 'id': 'oid'}//绑定的值控件
            , searchField: []
            , columns: []
            , dataMethod: 'Mysoft.Project.Control.DDTreeService.GetDDTreeData'
            , params: function() { return { buguid: 123 }; } //调用服务器方法前传递的参数
            , selectorType: SelectorType.grid
            , showClearButton: false
            , height: 400,
        width: 600

    };



    return Selector;


});



