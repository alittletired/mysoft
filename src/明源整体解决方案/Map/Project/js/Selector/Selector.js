define(function(require) {
    var project = require('../project')
    var Selector = {}
    Selector.open = function(options) {
        options = $.extend({}, Selector.DEFAULTS, options);

        var ret = window.showModalDialog('/project/js/Selector/Selector.htm',
       options, "dialogWidth:" + options.width + "px;dialogHeight:" + options.height + "px;help:0;status:0;scroll:0;center:1");
        if (ret)
            return JSON.parse(ret).result;
        return null;
    }

    var SelectorType = { grid: 0, treeGrid: 1 }
    //设置默认值
    Selector.DEFAULTS = {
        title: '' //弹出的选择窗口名称
            , valueCtrl: ''//绑定的值控件
            , textCtrl: ''  //绑定的文本控件
            , searchField: []
            , columns: []
            , dataMethod: 'Mysoft.Project.Control.DDTreeService.GetDDTreeData'
            , params: function() { return { buguid: 123 }; } //调用服务器方法前传递的参数
            , selectorType: SelectorType.grid
            , showClearButton: false
            , height: 340,
        width: 500

    };



    return Selector;


});



