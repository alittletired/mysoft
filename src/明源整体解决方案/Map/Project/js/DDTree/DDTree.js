define(function(require) {
    require('jquery')
    var project = require('../project')

    var NodeType = {}
    var nodeBgColor = {};
    //下拉树
    function DDTree(element, options) {
        if (typeof element === "string" && element.indexOf('#') < 0) {
            element = '#' + element;
        }
        options = $.extend({}, DDTree.DEFAULTS, typeof options == 'object' && options)
        var that = this;
        var me = that.$element = $(element);
        me.data('_ddtree', that);
        that.options = options;
        //添加appfind支持
        for (var i = 0; i < 20; i++) {
            var _oCtl = document.getElementById("#appQueryCtl" + i.toString());
            if (!_oCtl) {
                this._oCtl = $('<input type="hidden" id="appQueryCtl' + i + '" />');
                this._oCtl.prop('queryxml', '<filter type="and"><condition attribute="1" value="2" operator="eq"/></filter>')
                $('body').append(this._oCtl);
                break;
            }
        }

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
    DDTree.DEFAULTS = {
        data: null //数据源    
        , text: ''
            , showFullText: true//显示完整的路径名
            , onchange: false  //选择元素后的回调函数
            , showSearch: true
             , serviceMethod: 'MySoft.Project.Control.DDTreeService.GetDDTreeData'
             , treeType: NodeType.EndProject //数据加载类型
             , selectType: NodeType.EndProject //可以选择的类型
             , showType: NodeType.None //默认展开类型，-1不展开
             , showGroup: true  //是否显示集团
             , showCompany: true //是否显示区域公司
             , applySys: '0201'
             , NodeType: { None: -1, Group: 0, Company: 10, EndCompany: 20, Dept: 30, Team: 40, ProjectTeam: 50, Project: 60, EndProject: 70 }
             , nodeBgColor: { "0": "#DDE0E5", "10": "#E4E7EC", "20": "#EEF0F2", "30": "#F4F5F8", "60": "#F4F5F8", "70": "#F8F9FC" }
, autoSwitchCompany: true //是否自动切换公司
    };

    DDTree.prototype.addStyle = function() {
        var styles = []
        styles.push(".ddtreeWarp{  padding:0px 0px ;display:inline-block;text-align:left; }  table.ddtree{TABLE-LAYOUT: fixed; WIDTH: 100%; BACKGROUND-COLOR: white; }")
        styles.push(" .ddtree  span.spanOut {  CURSOR: default;  BORDER: #7b9ebd 1px solid; HEIGHT: 19px;FONT-SIZE: 9pt; FONT-FAMILY: 宋体, Tahoma, Verdana, Arial; BORDER-RIGHT: medium none; PADDING-LEFT: 5px; PADDING-top: 1px;  WIDTH: 100% }")
        styles.push(" .ddtree span.text{ CURSOR: hand; COLOR: blue; TEXT-DECORATION: underline } ")
        styles.push("  .ddtree span.readonly{ CURSOR: hand; } ")
        project.addStyle(styles.join(''));
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

    //显示下拉的div
    DDTree.prototype._expand = function(obj, ev) {
        var $li = $(obj).parents('li:eq(0)');
        var code = $li.prop('code');
        var item = this._treeKey[code];
        obj.innerText = (item.expand) ? '[+]' : '[-]';
        if (item.expand) {
            item.expand = false;
            $li.find('ul:eq(0)').hide();

        } else {
            item.expand = true;
            $li.find('ul:eq(0)').show();
        }

        if (ev) {
            ev.cancelBubble = true
            ev.returnValue = false;
        }
        return false;

    }
    DDTree.prototype._search = function(obj) {
        var that = this;
        var $tr = $(obj).parents('tr');
        var searchText = $tr.find('#txtSearch').val();
        if (searchText != that._lastSearchText) {
            that._lastSearchText = searchText;
        }
        else {
            return;
        }
        if (!searchText) {
            var c = "1";
        }
        that._searchHit = {}
        that._searchShow = {};
        var data = that.options.data
        if (!data || data.length == 0) return;
        if (searchText) {
            for (var i = data.length - 1; i >= 0; i--) {
                var item = data[i];
                if (item.name.indexOf(searchText) > -1) {

                    that._searchHit[item.code] = true;
                    that._searchShow[item.code] = true;
                    var parentItem = item.parentItem;
                    while (parentItem) {
                        that._searchShow[parentItem.code] = true;

                        parentItem = parentItem.parentItem;
                    }
                }
            }
        }
        var arr = [];
        var html = [];
        if (searchText) {
            that._addColorReg = new RegExp(searchText, "gm");
            that._searchReplace = '<b style="color:red">' + searchText + '</b>';
            this.buildSearchDropDownItem(this._rootItems, arr, 0)
            html = arr.join('')

        } else {
            this.buildDropDownItem(this._rootItems, arr, 0)
            html = arr.join('')

        }
        if (that._popup) {
            var treeUL = that._popup.document.parentWindow.document.getElementById('treeUL');

            treeUL.innerHTML = html;
        }

    }


    //ie popup有bug，无法输入字符字母
    DDTree.prototype._input = function(o, keyCode) {
        var that = this;

        //只能输入大小字母和数字
        var c = keyCode;
        if (c == 8) { //Back 
            o.value = o.value.slice(0, -1);
        } else if (c == 46) { //Del 
            o.value = o.value.slice(1);
        } else if (c < 255) {

            o.value += String.fromCharCode(c);
        }

    }

    DDTree.prototype._select = function(obj) {
        var $li = $(obj).parents('li');
        var code = $li.prop('code');
        options = this.options;
        var item = this._treeKey[code];
        if (!item) return;

        this.setValue(code)


    }

    DDTree.prototype.hidePopup = function() {
        if (this._searchTimer) {
            clearInterval(this._searchTimer);
        }
        if (this._txtSearch) {
            this._txtSearch.value = '';
            this._lastSearchText = '';
        }
        project.hidePopup(this.$element[0])

    }
    DDTree.prototype.showPopup = function(html) {
        var width = this.options.width || this.$element.width();
        var height = this.options.height || 300;

        return project.showPopup(this.$element[0], html, width, height)
    }

    DDTree.prototype.buildSearchDropDownItem = function(data, arr, indent) {
        var that = this;
        var options = that.options
        if (!data || data.length == 0) return;
        var indentHtml = ''
        for (var i = 0; i < indent; i++) {
            indentHtml += '&nbsp;&nbsp;&nbsp;'
        }
        $.each(data, function() {
            var item = this;
            if (!that._searchShow[item.code]) return;
            var canSelect = item.type >= options.selectType;
            var selectEvent = 'onclick="_dropdown._select(this)""';
            var highline = ";color:black;";
            var mouseover = ""
            if (that._selItem == item) {
                highline += ";background: #CAD3E4;"
            } else {
                highline += ";background: " + nodeBgColor[item.type]
                mouseover = 'onmouseover=\'style.backgroundColor="#E3E9F4"\'   onmouseout=\'  style.backgroundColor="' + nodeBgColor[item.type] + '"\'';
            }
            if (!canSelect) {
                highline += ";color:grey;"
                selectEvent = ""
            }
            // '" title="' + item.name +
            arr.push('<li  code="' + item.code + '"  title="' + item.name + '"   style="CURSOR: hand; height:20px;line-height:20px; margin-bottom:1px;">');

            arr.push('<div href="javascript:void(0)" ' + selectEvent + ' style="vertical-align:middle; CURSOR: hand; height:20px; ' + highline + '"' + mouseover + ' ><span>' + indentHtml
                 + '</span><span  class="icon"  ');
            var expandHandler = '';
            var icon = ''
            if (item.hasChilden || (item.children && item.children.length > 0)) {
                icon = (that._searchShow[item.code] || item.expand) ? '[-]' : '[+]'
                expandHandler = ' onclick="return _dropdown._expand(this,event); "'
            } else if (options.icon) {

                icon = '<img style="display:inline-block; margin-right:6px;vertical-align:middle;" src="' + options.icon + '">';
            }
            arr.push(expandHandler + '>' + icon)
            arr.push('</span><span style="display:inline-block;overflow:hidden;text-overflow:clip ;margin-top:2px;"  ');

            var name = item.name;
            if (that._searchHit[item.code]) {
                name = name.replace(that._addColorReg, that._searchReplace);
            }
            arr.push('>' + name + '</span></div>');


            if (item.children) {
                var display = (that._searchShow[item.code] || item.expand) ? "block" : "none";
                arr.push('<ul style="list-style: none;padding:0 0 ;margin:1px 0px 0px 0px; display:' + display + ';">')
                if (that._searchHit[item.code]) {
                    that.buildDropDownItem(item.children, arr, indent + 1);

                } else {
                    var showItems = [];
                    for (var j = 0; j < item.children.length; j++) {
                        var showitem = item.children[j];
                        if (that._searchShow[showitem.code])
                            showItems.push(showitem);
                    }
                    that.buildSearchDropDownItem(showItems, arr, indent + 1);
                }
                arr.push('</ul>')
            }
            arr.push('</li>');

        });

    }
    DDTree.prototype.buildDropDownItem = function(data, arr, indent) {
        var that = this;
        var options = that.options
        if (!data || data.length == 0) return;
        var indentHtml = ''
        for (var i = 0; i < indent; i++) {
            indentHtml += '&nbsp;&nbsp;&nbsp;'
        }
        $.each(data, function() {
            var item = this;
            var highline = ";color:black;";
            var canSelect = item.type >= options.selectType;
            var selectEvent = 'onclick="_dropdown._select(this)"';
            var mouseover = "";
            if (that._selItem == item) {
                highline += ";background: #CAD3E4;"
            }
            else {
                highline += ";background: " + nodeBgColor[item.type]
                mouseover = 'onmouseover=\'style.backgroundColor="#E3E9F4"\'   onmouseout=\'  style.backgroundColor="' + nodeBgColor[item.type] + '"\'';
            }

            if (!canSelect) {
                highline += ";color:grey;"
                selectEvent = ""
            }
            // '" title="' + item.name +
            arr.push('<li  code="' + item.code + '"  title="' + item.name + '"   style="CURSOR: hand; height:20px;line-height:20px; margin-bottom:1px;">');

            arr.push('<div href="javascript:void(0)" ' + selectEvent + ' style="vertical-align:middle; CURSOR: hand; height:20px; ' + highline + '" ' + mouseover + '><span>' + indentHtml
                 + '</span><span  class="icon"  ');
            var expandHandler = '';

            var icon = ''
            if (item.hasChilden || (item.children && item.children.length > 0)) {
                icon = (that._searchShow[item.code] || item.expand) ? '[-]' : '[+]'
                expandHandler = ' onclick="return _dropdown._expand(this,event); "'
            } else if (options.icon) {

                icon = '<img style="display:inline-block; margin-right:6px;vertical-align:middle;" src="' + options.icon + '">';
            }
            arr.push(expandHandler + '>' + icon)
            arr.push('</span><span style="display:inline-block;overflow:hidden;text-overflow:clip ;margin-top:2px;"  ');

            var name = item.name;
            if (that._searchHit[item.code]) {
                name = name.replace(that._addColorReg, that._searchReplace);
            }
            arr.push('>' + name + '</span></div>');

            if (item.children) {
                var display = (that._searchShow[item.code] || item.expand) ? "block" : "none";
                arr.push('<ul style="list-style: none;padding:0 0 ;margin:0 0; display:' + display + ';">')
                that.buildDropDownItem(item.children, arr, indent + 1);
                arr.push('</ul>')
            }
            arr.push('</li>');

        });

    }


    DDTree.prototype.wrapDropdown = function(html) {
        var height = this.options.height || 300;
        var warp = '<span style="width:100%; height:' + height + 'px; border:1px solid #8F9DC0; padding:1px; overflow-y:auto; overflow-x:hidden;">';
        var endwarp = '</span>';
        return warp + html + endwarp
    }
    DDTree.prototype.renderHeader = function() {
        var html = [];
        html.push('<TABLE class="ddtree"  cellSpacing=0 cellPadding=0><COLGROUP><COL><COL width=18><TBODY><TD>');
        html.push('<SPAN class="spanOut" name="_spanOut">')
        html.push('<SPAN id="ddtree_text" ></SPAN></SPAN></TD>');
        html.push('<TD><IMG  ');
        if (!this.options.disabled) {
            html.push('onmouseover="this.src=\'/_imgs/selectOn.gif\';" onmouseout="this.src=\'/_imgs/selectOff.gif\';"')
        }
        html.push(' style="CURSOR: hand"  align=absMiddle src="/_imgs/selectOff.gif"></TD></TR></TBODY></TABLE>');
        return html.join('')
    }
    //设置选中的值,将夫元素展开
    DDTree.prototype.setValue = function(code, isinit) {
        var item = this._treeKey[code];
        if (!item) return;
        if (this.options.selectType > item.type) return;
        text = '<span class="text ddt_text  code="' + item.code + '">' + item.name + '</span>'
        if (!this.options.showFullText)
            text = '<span class="readonly" code="' + item.code + '">' + item.name + '</span>'
        this._selItem = item;
        var parentItem = item.parentItem;
        while (parentItem) {
            parentItem.expand = true;
            var cls = this.options.selectType <= parentItem.type ? 'text' : 'readonly';

            if (this.options.showFullText)
                text = '<span class="' + cls + '" code="' + parentItem.code + '">' + parentItem.name + '</span><SPAN>－</SPAN>' + text;
            parentItem = parentItem.parentItem;
        }
        this.$element.find('#ddtree_text').html(text);
        var filter = "<filter />"
        var values = [];
        switch (this.options.treeType) {
            case NodeType.EndProject:
            case NodeType.Project:
                if (item.type == NodeType.EndProject || item.type == NodeType.Project) {
                    this._getEndIds(item, NodeType.EndProject, values);
                    filter = ' <filter type="and"><condition operator="in" attribute=\"ProjGUID" value="' + values.join(",") + '"/></filter>'

                }
                else if (item.type <= NodeType.EndCompany) {
                    filter = '<filter type="and"><condition operator="api" attribute="ProjGUID" value="' + item.id + '" datatype="buprojectfilter" application="' + this.options.applySys + '"/></filter>'
                }
                break;
            case NodeType.Group:
            case NodeType.Company:
            case NodeType.EndCompany:
                this._getEndIds(item, NodeType.EndCompany, values);
                filter = ' <filter type="and"><condition operator="in" attribute=\"BUGUID" value="' + values.join(",") + '"/></filter>'
                break;
            case NodeType.Dept:
                filter = '<filter><condition attribute="replace" operator="replace" value=" JbDeptCode=\'' + item.code + '\' or JbDeptCode like \'' + item.code + '.%\' "/></filter>';
        }
        this._oCtl.prop('queryxml', filter)
        var data = { code: item.code, id: item.id, type: item.type, name: item.name, isend: item.isend };
        if (this.options.autoSwitchCompany && !isinit) {

            var error = project.invoke('MySoft.Project.Control.DDTreeService.SetValue', data);
            if (error) return alert(error);
        }
        if (this.options.onchange) {
            if (this.options.onchange(data) === false) {
                return;
            }
        }
        this.hidePopup();
    }

    DDTree.prototype._getEndIds = function(item, nodeType, arr) {
        if (item.type == nodeType) {
            arr.push(item.id)
            return arr;
        }
        var children = item.children || [];
        for (var i = 0; i < children.length; i++) {
            this._getEndIds(children[i], nodeType, arr);
        }

    }
    DDTree.prototype.showDropDown = function(e) {

        var that = this;
        that._searchHit = {}
        if (that._searchTimer) {
            clearInterval(that._searchTimer);
            that._searchTimer = null;
        }
        that._searchShow = {};
        var arr = [];
        if (this.options.showSearch) {
            arr.push('<TABLE  style="TABLE-LAYOUT: fixed" cellSpacing=0 cellPadding=0 width="100%">');
            arr.push('<TR><TD>');
            arr.push('<input style="width:100%" type="text" value style="border:1px solid #CAD3E4"  id="txtSearch" onkeypress="_dropdown._input(this,event.keyCode)" />');
            arr.push('</TD></TR></TBODY></TABLE>');
        }
        arr.push('<ul id="treeUL"  style="list-style: none;padding:0 0 ;margin:0 0;font-size: 9pt; font-family: 宋体, Tahoma, Verdana, Arial; width:100%;"  >');
        this.buildDropDownItem(this._rootItems, arr, 0, true)
        arr.push('</ul>');

        // 3、生成下拉菜单
        var html = this.wrapDropdown(arr.join(''));

        var popup = this.showPopup(html)
        this._popup = popup
        popup.document.parentWindow._dropdown = this;

        var txtSearch = popup.document.parentWindow.document.getElementById('txtSearch');
        if (txtSearch) {
            that._txtSearch = txtSearch;
            that._txtSearch.value = ''
            that._lastSearchText = '';
            //popbug，无法获取中文输入
            that._searchTimer = setInterval(function() {
                that._search(txtSearch);

            }, 100)

        }


    }
    return DDTree;


});



