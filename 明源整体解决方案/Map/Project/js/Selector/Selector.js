define(function(require) {

    var project = require('../project')
    var Selector = {}
    Selector.open = function(options) {
        options = $.extend({}, Selector.DEFAULTS, options);
        var height = options.height
    
        options.height = options.height - 120;
        var arrItem = window.showModalDialog('/project/js/Selector/Selector.htm?t=' + Math.random(),
       options, "dialogWidth:" + options.width + "px;dialogHeight:" + height + "px;help:0;status:0;scroll:0;center:1");

        if (arrItem) {
            var bindValues = {};
            $.each(options.bindCtrls, function() {
                bindValues[this["field"]] = [];
            })

            $.each(arrItem, function() {
                var item = this;
                $.each(options.bindCtrls, function() {
                    var field = this["field"];
                    bindValues[field].push(item[field]);

                });

            });
            
            $.each(options.bindCtrls, function() {
                var item = this;
                var field = item["field"];
                var ctrl = item["control"];
                if (typeof ctrl === 'string') {
                    ctrl = document.getElementById(ctrl) || appForm[ctrl];
                }
                ctrl.value = bindValues[field].join(',');
            })

            return arrItem;
        }
        return null;
    }

    var SelectorType = { grid: 0, treeGrid: 1 }
    //设置默认值
    Selector.DEFAULTS = {
        title: '', //弹出的选择窗口名称
        bindCtrls: [{ field: "Room", control: 'txtid' },
                { field: "RoomCode", control: 'txtRoomCode' },
                { field: "HuXing", control: 'txtHuXing' }
                ], //绑定的值控件
        searchField: []
            , columns: []
            , serviceMethod: 'Mysoft.Project.Control.DDTreeService.GetDDTreeData'
            , queryParams: function() { return { buguid: 123 }; } //调用服务器方法前传递的参数
            , selectorType: SelectorType.grid
            , showClearButton: false
            , height: 400,
        width: 600
    };
    return Selector;


});



