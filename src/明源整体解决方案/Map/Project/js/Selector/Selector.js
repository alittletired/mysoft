define(function(require) {
    require('jquery')
    var project = require('../project')


    //下拉树
    function Selector(element, options) {
        if (typeof element === "string" && element.indexOf('#') < 0) {
            element = '#' + element;
        }
        options = $.extend({}, DDTree.DEFAULTS, typeof options == 'object' && options)
        var that = this;
        var me = that.$element = $(element);

        that.options = options;
        if (!that.options.data) {
            var data = project.invoke(that.options.serviceMethod, { applySys: that.options.applySys, treeType: that.options.treeType });
            data = data || {}
            that.options.data = data.data || [];
            that.options.value = data.value || '';
            that.init()
        } else {
            that.init();
        }
    }



    //设置默认值
    Selector.DEFAULTS = {
        title: '' //弹出的选择窗口名称
            , valueCtrl: true//绑定的值控件
            , textCtrl: false  //绑定的文本控件
            , searchField: []
            , columns: []
            , dataMethod: 'MySoft.Project.Control.DDTreeService.GetDDTreeData'
            , params: function() { return { buguid: 123 }; }//调用服务器方法前传递的参数

    };
    var data = project.invoke(tdataMethod, params);
    DDTree.prototype.addStyle = function() {
        var css = []
        css.push(".ddtreeWarp{  padding:0px 0px ;display:inline-block;text-align:left; }  table.ddtree{TABLE-LAYOUT: fixed; WIDTH: 100%; BACKGROUND-COLOR: white; }")
        css.push(" .ddtree  span.spanOut {  CURSOR: default;  BORDER: #7b9ebd 1px solid; HEIGHT: 19px;FONT-SIZE: 9pt; FONT-FAMILY: 宋体, Tahoma, Verdana, Arial; BORDER-RIGHT: medium none; PADDING-LEFT: 5px; PADDING-top: 1px;  WIDTH: 100% }")
        css.push(" .ddtree span.text{ CURSOR: hand; COLOR: blue; TEXT-DECORATION: underline } ")
        css.push("  .ddtree span.readonly{ CURSOR: hand; } ")
        project.addCss(css.join(''));
    }

    DDTree.prototype.init = function() {

        var that = this;
        if (that.options.selectType > that.options.treeType && window.location.host.indexOf('localhost') > 0) {
            alert("注意了，配置的selectType大于treeType将无法选取任何数据");
        }
        var me = that.$element;
        that.addStyle();
        that._rootItems = [];
        that._treeKey = {};
        that._searchHit = {}
        that._searchShow = {};
        this._popup = {}
        NodeType = that.options.NodeType
        nodeBgColor = that.options.nodeBgColor
        $.each(that.options.data, function() {
            var item = this;
            var code = item.code;
            if (!that.options.showGroup && item.type == NodeType.Group)
                return;

            if (!that.options.showCompany && item.type == NodeType.Company)
                return;
            if (item.type > that.options.treeType)
                return;
            that._treeKey[code] = item;
            that._treeKey[item.id] = item;
            item.expand = that.options.showType >= item.type;
            var parentCode = code.substr(0, code.lastIndexOf('.'));
            var parentItem = that._treeKey[parentCode];
            while (parentItem == null) {
                var lastIndex = parentCode.lastIndexOf('.');
                if (lastIndex < 0)
                    break;
                parentCode = parentCode.substr(0, lastIndex);
                parentItem = that._treeKey[parentCode];

            }
            if (parentItem) {
                parentItem.children = parentItem.children || [];
                parentItem.children.push(item);
                item.parentItem = parentItem;
            } else {
                that._rootItems.push(item)
            }

        });

        var headerHtml = that.renderHeader();
        me.addClass("ddtreeWarp").html(headerHtml);

        //添加appfind支持
        for (var i = 0; i < 20; i++) {
            _oCtl = document.getElementById("#appQueryCtl" + i.toString());
            if (!_oCtl) {
                this._oCtl = $('<input type="hidden" id="appQueryCtl' + i + '" />');
                this._oCtl.prop('queryxml', '<filter type="and"><condition attribute="1" value="2" operator="eq"/></filter>')
                me.append(this._oCtl);
                break;
            }
        }

        if (that.options.value)
            that.setValue(that.options.value, 1);

        me.click(function(e) {
            that.showDropDown(e);
        });
        me.find('#ddtree_text').on('click', '.text', function() {
            var span = $(this);
            var code = span.prop('code');

            that.setValue(code);
            return false;
        })

    }


    return Selector;


});



