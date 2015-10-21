
var c = { charset: 'gb2312' }
seajs.config(c);
/*
undo:
启用自定义分页：CustomerPager
背景样式须调整
隐藏数据未处理 newrow
done:格式化数据，number,datetime

    
*/

// 对Date的扩展，将 Date 转化为指定格式的String   
// 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符，   
// 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字)   
// 例子：   
// (new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423   
// (new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18
Date.prototype.Format = function(fmt) {
    if (this.toString() == "NaN")
        return "";
    //author: meizz   
    var o = {
        "M+": this.getMonth() + 1,                 //月份   
        "d+": this.getDate(),                    //日   
        "h+": this.getHours(),                   //小时   
        "m+": this.getMinutes(),                 //分   
        "s+": this.getSeconds(),                 //秒   
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度   
        "S": this.getMilliseconds()             //毫秒   
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
        fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}

function IsNotNull(val) {
    if (val == null || val == undefined) {
        return false;
    }
    return true;
}
define(function(require) {
    require('jquery');

    require('/_controls/number/number.js');
    require('/_controls/datetime/date.js');
    require('/_controls/datetime/time.js');
    require('/_controls/lookup/map_lookup.js');
    require('/_controls/util/util.js');
    require('/_common/scripts/global.js');

    var project = require('project');

    //repeater列表
    function myRepeater(element, options) {
        if (typeof element === "string" && element.indexOf('#') < 0) {
            element = '#' + element;
        }
        options = $.extend({}, myRepeater.DEFAULTS, typeof options == 'object' && options)
        var that = this;
        var me = that.$element = $(element);
        that.options = options;
        that.options.defaultsort = that.options.sortField;


        that.init();

    }

    //设置默认值
    myRepeater.DEFAULTS = {
        //properties
        columns: []//列集合
            , idField: ''//主键
            , enablePager: true//是否启用分页
            , pageindex: 1//页码
            , pagesize: 20//页大小
            , queryParams: {} //查询参数
            , mutiSelect: false//是否多选
            , serviceMethod: ''
            , sortField: ''//field1 asc/field desc
            , isEdit: false
            , height: "300px"
            , deleterows: []
            , isChange: false

        //event
            , onClickRow: false//行点击事件
            , onDblClickRow: false//双击事件

    };
    //列对象封装
    myRepeater.ColumnDEFAULTS = {
        title: ""//标题
        , field: ""//字段
        , width: ""//宽度
        , align: "center"//对齐方式
        , sortable: true//是否可排序
        , datatype: "text"//number,datetime
        , format:""
        , hidden: false
        , req:false
    };

    //Methods

    //加载数据
    myRepeater.prototype.init = function() {

        //加载标题
        var me = this.$element;
        //me.append(this._renderHead());
        this.loadData();
    }

    /*
    myRepeater.prototype._renderHead = function() {
    var that = this;
    var table = '<table id="gridBarHead" width="100%" cellpadding="0" cellspacing="0" border="0"   style="table-layout: fixed; padding-left: 2px; padding-right: 2px">';
    table += '<tr id="trHeader" align="center" height="23" style="cursor: hand">';
    if (that.options.mutiSelect) {
    table += '<td class="gridBar" align="center"><INPUT id="chkAll" title="选择本页所有记录" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none" type=checkbox value=""></td>';
    }
    else {
    table += '<td class="gridBar" align="center">序号</td>';
    }

        $(that.options.columns).each(function() {
    table += ' <td class="gridBar ' + (this.req ? "req" : "") + '" fieldname="' + this.field + '" align="center" sortable="' + this.sortable + '" hidden="' + this.hidden + '" title="' + this.title + '">' + this.title + '</td>';
    });
    table += "</tr>";
    table+="</table>";
    return table;
    }
    */
    myRepeater.prototype.setData = function(data) {
        var that = this;
        if (that.options.isChange) {
            if (!confirm("页面存在数据修改，是否继续页面刷新?"))
                return;
        }

        that.LastEditRow = null;
        $.extend(that.options, data);
        var cols = [];
        $(that.options.columns).each(function() {
            cols.push($.extend({}, myRepeater.ColumnDEFAULTS, this));
        });
        that.options.columns = cols;
        //新增样式
        project.addStyle('.repTitle td{border-right: 2px solid #dbdac9; padding-left: 5px;padding-right: 5px;} .gridSelectOver{background-color:rgb(230, 230, 230)}');
        //展示数据
        var me = that.$element;


        that.options.width = me.width();
        var table = '<div style="overflow: auto; width: ' + that.options.width + '; height: ' + that.options.height + '"><table id="gridBar" width="100%" cellpadding="0" cellspacing="0" border="0"   style="table-layout: fixed; padding-left: 2px; padding-right: 2px">';


        table += '<tr id="trHeader" class="repTitle" align="center" height="23" style="cursor: hand">';
        if (that.options.mutiSelect) {
            table += '<td class="gridBar" width="30px" align="center" name="noSort"><INPUT id="chkAll" title="选择本页所有记录" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none" type=checkbox value=""></td>';
        }

        table += '<td class="gridBar" width="40px" align="center" id="tdNO" name="noSort">序号</td>';

        $(that.options.columns).each(function() {
            var tdTitle = $(' <td width="' + this.width + '" ' + (this.hidden ? "style='display:none'" : "") + ' class="gridBar ' + (this.req ? "req" : "") + '" fieldname="' + this.field + '" align="center" sortable="' + this.sortable + '" title="' + this.title + '"> <NOBR>' + this.title + '</NOBR></td>');
            if (("#" + that.options.sortField).indexOf("#" + this.field + " ") > -1) {

                if (that.options.sortField.indexOf("asc") > -1) {
                    tdTitle.html("<NOBR>" + this.title + "<img class='asc' data-field='" + this.field + "' src='/Project/js/myRepeater/_imgs/ico_arrow_U.gif' /></NOBR>");
                }
                else {
                    tdTitle.html("<NOBR>" + this.title + "<img class='desc' data-field='" + this.field + "' src='/Project/js/myRepeater/_imgs/ico_arrow_D.gif' /></NOBR>");
                }
            }

            table += tdTitle.prop("outerHTML");

        });
        table += "</tr>";


        //加载数据
        var mydr = [];
        $(that.options.items).each(function(i) {
            var item = this;
            //item.rowoptype = "edit";
            mydr.push('<tr height="24" data-id="' + item[that.options.idField] + '" rowtype="datarow">');
            //是否多选决定是否显示下拉框
            if (that.options.mutiSelect) {
                mydr.push('<td class="gridBorder" align="center"><INPUT name="rowChk" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none" type=checkbox value="' + item[that.options.idField] + '"></td>');
            }

            //序号
            var no = i + 1 + (that.options.pagesize * (that.options.pageindex - 1))
            mydr.push('<td class="gridBorder" align="center" name="rowno">' + (no) + '</td>');
            $(that.options.columns).each(function(j) {
                var col = this;
                mydr.push('<td ' + (this.hidden ? "style='display:none'" : "") + ' class="gridBorder" align="' + col.align + '" fieldname="' + col.field + '" data-type="' + col.datatype + '">');
                var val = item[col.field];
                val = that.dataFormat(col, val);

                //处理datatype,格式化数据
                mydr.push("<NOBR title='" + val + "'>" + val + "</NOBR>");

                mydr.push('</td>');
            });
            mydr.push('</tr>');
        });

        table += mydr.join("");

        table += "</table></div>";

        //加载分页控件
        var arrPager = [];
        if (that.options.enablePager) {

            var pagecount = Math.ceil(that.options.total / that.options.pagesize);


            arrPager.push('<TABLE id="' + me.attr("id") + '_PagerBar" class="gridBar" height="22" cellSpacing="0" cellPadding="0" width="100%">');
            arrPager.push('<TBODY><TR> <TD style="PADDING-LEFT: 8px"><NOBR>');
            //页调整
            arrPager.push('页次：<INPUT id="' + me.attr("id") + '_ToPage" class="pagenum" style="WIDTH: 24px" maxLength="4" value="' + that.options.pageindex + '" name="' + me.attr("id") + ':ToPage" max="' + pagecount + '" min="1">');
            arrPager.push("/");
            arrPager.push('<SPAN id="' + me.attr("id") + '_PageCount">' + pagecount + '</SPAN>&nbsp;&nbsp;每页<INPUT id="' + me.attr("id") + '_PageSize" class="pagenum" style="WIDTH: 24px" maxLength="3" value="' + that.options.pagesize + '" name="' + me.attr("id") + ':PageSize" onvalidchange="" max="100" min="1">               条/共<SPAN id="' + me.attr("id") + '_RowsCount">' + that.options.total + '</SPAN>条');
            arrPager.push('</NOBR></TD><TD width="170" align="right"><NOBR>');
            //前后分页
            var isFist = that.options.pageindex == 1 ? "pager1" : "pager";
            var isLast = that.options.pageindex == pagecount ? "pager1" : "pager";
            arrPager.push('<A onclick="" id="' + me.attr("id") + '_FirstPage" class="' + isFist + '" style="CURSOR: hand" href="#">[首页]</A>&nbsp;<A onclick="" id="' + me.attr("id") + '_PrevPage" class="' + isFist + '" style="CURSOR: hand" href="#">[上页]</A>&nbsp;<A onclick="" id="' + me.attr("id") + '_NextPage" class="' + isLast + '" style="CURSOR: hand" href="#">[下页]</A>&nbsp;<A onclick="" id="' + me.attr("id") + '_LastPage" class="' + isLast + '" style="CURSOR: hand" href="#">[末页]</A>&nbsp;');
            arrPager.push('</NOBR></TD></TR></TBODY></TABLE>');

        }
        table += arrPager.join("");
        me.html(table);


        //绑定分页事件
        me.find('#' + me.attr("id") + "_ToPage")[0].onvalidchange = function(e) {

            that.options.pageindex = $(this).val();
            that.loadData();

        }

        me.find('#' + me.attr("id") + "_PageSize")[0].onvalidchange = function(e) {

            that.options.pagesize = $(this).val();
            that.loadData();
        }

        me.find('#' + me.attr("id") + "_FirstPage").on("click", function() {
            if (that.options.pageindex == 1)
                return false;
            else
                that.options.pageindex = 1;
            that.loadData();
        });
        me.find('#' + me.attr("id") + "_PrevPage").on("click", function() {
            if (that.options.pageindex <= 1)
                return false;
            else
                that.options.pageindex -= 1;
            that.loadData();
        });
        me.find('#' + me.attr("id") + "_NextPage").on("click", function() {
            if (that.options.pagesize == 0) that.options.pageindex = 20;
            var pagecount = Math.ceil(that.options.total / that.options.pagesize);
            if (that.options.pageindex >= pagecount)
                return false;
            else
                that.options.pageindex += 1;
            that.loadData();
        });
        me.find('#' + me.attr("id") + "_LastPage").on("click", function() {
            if (that.options.pagesize == 0) that.options.pagesize = 20;
            var pagecount = Math.ceil(that.options.total / that.options.pagesize);
            if (that.options.pageindex == pagecount)
                return false;
            else
                that.options.pageindex = pagecount;
            that.loadData();
        });

        //绑定控件事件
        //行事件
        me.find("#gridBar tr[id!=trHeader]").on("click", $.proxy(that.rowClick, that))
        me.find("#gridBar tr[id!=trHeader]").on("click", "nobr", $.proxy(that.rowClick, that))
        me.find("#gridBar tr[id!=trHeader]").on("mouseover", function() {
            $(this).addClass("gridSelectOver");
        })
        me.find("#gridBar tr[id!=trHeader]").on("mouseout", function() {
            $(this).removeClass("gridSelectOver");
        })
        me.find("#gridBar tr[id!=trHeader]").on("click", "input[name=rowChk]", function() {
            if (this.checked) {
                $(this).parents("tr[rowtype=datarow]").addClass("gridSelectOn");
                $(this).parents("tr[rowtype=datarow]").removeClass("gridSelectOff");
                if (that.options.items.length == me.find("#gridBar .gridSelectOn").length) {
                    me.find("#chkAll").attr("checked", true);
                }
            }
            else {
                $(this).parents("tr[rowtype=datarow]").addClass("gridSelectOff");
                $(this).parents("tr[rowtype=datarow]").removeClass("gridSelectOn");
                me.find("#chkAll").attr("checked", false);
            }
        });

        me.find("#gridBar tr[id!=trHeader]").on("dbclick", function() {
            var row = that.options.items[$(this).index()];
            if (that.options.onDblClickRow) {
                if (that.options.onDblClickRow(row) === false) {
                    return;
                }
            }
        })

        //列头点击事件

        me.find("#trHeader NOBR").bind("click", function() {
            var me = $(this).parent();
            var sortimg = me.find("img");
            if (sortimg.length == 0) {
                that.options.sortField = me.attr("fieldname") + " asc";

            }
            else {
                if (sortimg.hasClass("asc") == true) {
                    that.options.sortField = me.attr("fieldname") + " desc";

                }

                if (sortimg.hasClass("desc") == true) {
                    that.options.sortField = that.options.defaultsort;

                }
            }

            that.loadData();
        })

        //全选点击

        me.find("#chkAll").bind("click", function() {
            var tbl = $(this).closest("table").find("tr[id!=trHeader]");
            if (this.checked) {
                //tbl.removeClass("gridSelectOn");
                tbl.removeClass("gridSelectOff");
                tbl.addClass("gridSelectOn");
                tbl.find("input[name=rowChk]").attr("checked", true);
            }
            else {
                tbl.removeClass("gridSelectOn");
                //tbl.removeClass("gridSelectOff");
                tbl.addClass("gridSelectOff");
                tbl.find("input[name=rowChk]").attr("checked", false);
            }
        })

        //处理列宽调整
        //if (that.options.resizeCol) {
        SetColResize(me.find("#gridBar")[0]);
        //}

        //加载完成后选择第一行
        if (that.options.items.length > 0)
            me.find("#gridBar tr[id!=trHeader]:first").click();

        //页面自适应

        //beg lpf
        myRepeater.prototype.LastEditRow = null;
        myRepeater.prototype.SetRowEdit = function(row, data) {

            var cols = that.options.columns;
            if (that.LastEditRow != null) {
                if (that.LastEditRow.rowIndex == -1) {
                    that.LastEditRow = null;
                }
                else {
                    var lastRow = that.LastEditRow;
                    var lastCells = $(lastRow).find("td[fieldname]");
                    for (var n = 0; n < cols.length; n++) {
                        var editor = cols[n].editor;
                        if (IsNotNull(editor)) {

                            var control = this.editors[cols[n].editor.type];
                            control.destroy(lastCells[n], cols[n]);
                        }
                    }
                    lastRow.isEditing = false;
                }
            }


            var cells = $(row).find("td[fieldname]")
            for (var n = 0; n < cols.length; n++) {
                var editor = cols[n].editor;
                if (IsNotNull(editor)) {
                    var control = this.editors[cols[n].editor.type];
                    control.init(cells[n], cols[n]);
                }
            }
            that.LastEditRow = row;
            row.isEditing = true;
        }



        myRepeater.prototype.editors =
    {
        textbox:
                {
                    init: function(target, col) {
                        var options = col.editor.option;
                        var input = $("<input class=\"txt\" type=\"text\" style=\"text-align:left\" />");
                        if (IsNotNull(options)) {
                            if (IsNotNull(options.forbiddenchars)) {
                                $(input).attr("forbiddenchars", options.forbiddenchars);
                            }
                            if (IsNotNull(options.maxlength)) {
                                $(input).attr("maxlength", options.maxlength);
                            }
                            if (IsNotNull(options.ro)) {
                                $(input).attr("ro", options.ro);
                            }
                            if (IsNotNull(options.onblur)) {
                                $(input).bind("blur", options.onblur);
                            }
                            if (IsNotNull(options.onchange)) {
                                $(input).bind("change", options.onchange);
                            }
                            $(input).val($(target).text());
                        }
                        $(input).bind("change", function() {
                            //that.options.items
                            var rowIndex = target.parentNode.rowIndex;
                            var cellIndex = target.cellIndex;
                            that.options.items[rowIndex - 1][col.field] = $(input).val();
                            that.options.isChange = true;
                        });
                        $(target).html("");
                        $(input).appendTo(target);
                        return input;
                    },
                    setValue: function(target, value) {
                        var row = target.parentNode;
                        if (row.isEditing != true) {
                            $(target).text(value);
                        }
                        else {
                            $(target).find("input").val(value);
                        }
                        var rowIndex = row.rowIndex;
                        that.options.items[rowIndex - 1][target.fieldname] = value;
                    },
                    getValue: function(target) {
                        return that.options.items[target.parentNode.rowIndex - 1][target.fieldname];
                    },
                    destroy: function(target, col) {
                        var row = target.parentNode;
                        if (row.isEditing != true) {
                            return;
                        }
                        else {
                            var rowIndex = row.rowIndex;
                            $(target).text(that.options.items[rowIndex - 1][target.fieldname]);
                            $(target).find("input").remove();
                        }
                    }
                },
        numbox: {
            init: function(target, col) {
                var options = col.editor.option;
                var input = $("<input class=\"num\" type=\"text\" style=\"text-align:right\" returnValue=\"\" />");
                $(input).bind("change", function() {
                    //that.options.items
                    var rowIndex = target.parentNode.rowIndex;
                    var cellIndex = target.cellIndex;
                    var value = $(input).val().replace(/,/g, "");
                    input.returnValue = value;
                    that.options.items[rowIndex - 1][col.field] = value;
                });
                if (IsNotNull(options)) {
                    if (IsNotNull(options.max)) {
                        $(input).attr("max", options.max);
                    }
                    if (IsNotNull(options.min)) {
                        $(input).attr("min", options.min);
                    }
                    if (IsNotNull(options.grp)) {
                        $(input).attr("grp", options.grp);
                    }
                    if (IsNotNull(options.acc)) {
                        $(input).attr("acc", options.acc);
                    }
                    if (IsNotNull(options.dt)) {
                        $(input).attr("dt", options.dt);
                    }
                    if (IsNotNull(options.maxlength)) {
                        $(input).attr("maxlength", options.maxlength);
                    }
                    if (IsNotNull(options.ro)) {
                        $(input).attr("ro", options.ro);
                    }
                    if (IsNotNull(options.onchange)) {
                        $(input).bind("change", options.onchange);
                    }

                    $(input).bind("change", function() {
                        that.options.isChange = true;
                    });

                    $(input).val($(target).text());
                }
                $(target).html("");
                $(input).appendTo(target);
                return input;
            },
            setValue: function(target, value) {
                var row = target.parentNode;
                if (row.isEditing != true) {
                    $(target).text(value);
                }
                else {
                    $(target).find("input").val(value);
                }
                var rowIndex = row.rowIndex;
                that.options.items[rowIndex - 1][target.fieldname] = value;
            },
            getValue: function(target) {
                return that.options.items[target.parentNode.rowIndex - 1][target.fieldname];
            },
            destroy: function(target, col) {
                var row = target.parentNode;
                if (row.isEditing != true) {
                    return;
                }
                else {

                    var rowIndex = row.rowIndex;
                    var value = that.options.items[rowIndex - 1][target.fieldname];
                    value = that.dataFormat(col,value);

                    $(target).text(value);
                    $(target).find("input").remove();
                }
            }
        },
        combobox: {
            init: function(target, col) {
                var options = col.editor.option;
                var select = $("<select style=\"width: 97%\"></select>");

                $(select).bind("change", function() {
                    var rowIndex = target.parentNode.rowIndex;
                    var cellIndex = target.cellIndex;
                    var value = this.options[this.selectedIndex].innerText;
                    that.options.items[rowIndex - 1][col.field] = value;
                    //记录数据修改
                    that.options.isChange = true;
                });
                if (IsNotNull(options)) {
                    if (IsNotNull(options.data)) {
                        var data = options.data;
                        if (typeof (data) == "function") {
                            data = data();
                        }
                        $(data).each(function() {
                            var option = $("<option></option>");
                            $(option).attr("value", this.value);
                            if (this.text == $(target).text()) {
                                $(option).attr("selected", "selected");
                            }
                            $(option).text(this.text);
                            $(select).append(option);
                        });
                    }
                }
                $(target).html("");
                $(select).appendTo(target);
                return select;
            },
            setValue: function(target, value) {
                var row = target.parentNode;
                if (row.isEditing != true) {
                    $(target).text(value);
                }
                else {
                    $(target).find("select").val(value);
                }
                var rowIndex = row.rowIndex;
                that.options.items[rowIndex - 1][target.fieldname] = value;
            },
            getValue: function(target) {
                return that.options.items[target.parentNode.rowIndex - 1][target.fieldname];
            },
            destroy: function(target, col) {
                var row = target.parentNode;
                if (row.isEditing != true) {
                    return;
                }
                else {
                    var rowIndex = row.rowIndex;
                    $(target).text(that.options.items[rowIndex - 1][target.fieldname]);
                    $(target).find("select").remove();
                }
            }
        },
        datetimebox: {
            init: function(target, col) {
                var options = col.editor.option;
                var table = $('<table style="table-layout:fixed;display:inline;width:95%" cellspacing="0" cellpadding="0"><colgroup><col /><col width="40" /><tr><td><input type="datetime" maptype="dtm" time="0" class="dtm" maxlength="10" value="" returnValue="" /></td><td style="padding-left:2px;"><img class="dtm" src="/_imgs/btn_off_cal.gif" align="absMiddle" /></td></tr></table>');
                var input = $(table).find("input");
                if (IsNotNull(options)) {
                    if (IsNotNull(options.time)) {
                        $(input).attr("time", options.time);
                    }
                    if (IsNotNull(options.maxlength)) {
                        $(input).attr("maxlength", options.maxlength);
                    }
                    if (IsNotNull(options.onchange)) {
                        $(input).bind("change", options.onchange);
                    }
                    $(input).val($(target).text());
                }
                $(input).bind("change", function() {
                    //that.options.items
                    var rowIndex = target.parentNode.rowIndex;
                    var cellIndex = target.cellIndex;
                    that.options.items[rowIndex - 1][col.field] = $(input).val();
                    //记录数据修改
                    that.options.isChange = true;
                });
                $(target).html("");
                $(table).appendTo(target);
                return table;
            },
            setValue: function(target, value) {
                var row = target.parentNode;
                if (row.isEditing != true) {
                    $(target).text(value);
                }
                else {
                    $(target).find("input").val(value);
                }
                var rowIndex = row.rowIndex;
                that.options.items[rowIndex - 1][target.fieldname] = value;
            },
            getValue: function(target) {
                return that.options.items[target.parentNode.rowIndex - 1][target.fieldname];
            },
            destroy: function(target, col) {
                var row = target.parentNode;
                if (row.isEditing != true) {
                    return;
                }
                else {
                    var rowIndex = row.rowIndex;
                    var val = that.options.items[rowIndex - 1][target.fieldname];
                    val = that.dataFormat(col, val);
                    $(target).text(val);
                    $(target).find("table").remove();
                }
            }
        },
        lookupbox:
                {
                    init: function(target, col) {
                        var options = col.editor.option;
                        var table = $('<table style="table-layout:fixed;display:inline;width:97%" cellspacing="0" cellpadding="0" ><colgroup><col /><col width="26" /></colgroup><tr><td><input type="text" class="txt2" id="" readOnly value="" /></td><td style="padding-left: 2px;"><img style="cursor:hand" class="txt2" src="/_imgs/btn_off_lookup.gif" align="absMiddle" onclick="" /></td></tr></table>');
                        var input = $(table).find("input");
                        var img = $(table).find("img");
                        if (IsNotNull(options)) {
                            if (IsNotNull(options.onclick)) {
                                $(img).bind("click", options.onclick);
                            }
                            $(input).val($(target).text());
                        }
                        $(input).bind("change", function() {
                            var rowIndex = target.parentNode.rowIndex;
                            var cellIndex = target.cellIndex;
                            that.options.items[rowIndex - 1][col.field] = $(input).val();
                            //记录数据修改
                            that.options.isChange = true;
                        });
                        $(target).html("");
                        $(table).appendTo(target);
                        return table;
                    },
                    setValue: function(target, value) {
                        var row = target.parentNode;
                        if (row.isEditing != true) {
                            $(target).text(value);
                        }
                        else {
                            $(target).find("input").val(value);
                        }
                        var rowIndex = row.rowIndex;
                        that.options.items[rowIndex - 1][target.fieldname] = value;
                    },
                    getValue: function(target) {
                        return that.options.items[target.parentNode.rowIndex - 1][target.fieldname];
                    },
                    destroy: function(target, col) {
                        var row = target.parentNode;
                        if (row.isEditing != true) {
                            return;
                        }
                        else {
                            var rowIndex = row.rowIndex;
                            $(target).text(that.options.items[rowIndex - 1][target.fieldname]);
                            $(target).find("table").remove();
                        }
                    }
                }
    };
        //end
        //数据重新加载后，重置状态
        that.options.isChange = false;
    }
    myRepeater.prototype.loadData = function() {
        var that = this;
        //自定义参数
        var param = { pagesize: that.options.pagesize, pageindex: that.options.pageindex, sortfield: that.options.sortField, servicemethod: that.options.serviceMethod };
        if (typeof (that.options.queryParams) == "function") {

            that.options.queryParams();
        }
        else {
            param.customerParam = that.options.queryParams || {};
        }
        //获取数据
        var data = project.invoke("Mysoft.Project.Control.myRepeaterService.GetPageData", param);

        return that.setData(data)


    }
    function SetColResize(table) {
        var tTD; //用来存储当前更改宽度的Table Cell,避免快速移动鼠标的问题 
        for (j = 0; j < table.rows[0].cells.length; j++) {
            table.rows[0].cells[j].onmousedown = function() {
                //记录单元格
                tTD = this;
                if (event.offsetX > tTD.offsetWidth - 10) {
                    tTD.mouseDown = true;
                    tTD.oldX = event.x;
                    tTD.oldWidth = tTD.offsetWidth;
                }
                //记录Table宽度
                //table = tTD; while (table.tagName != ‘TABLE’) table = table.parentElement;
                //tTD.tableWidth = table.offsetWidth;
            };
            table.rows[0].cells[j].onmouseup = function() {
                //结束宽度调整
                if (tTD == undefined) tTD = this;

                tTD.mouseDown = false;
                tTD.style.cursor = 'default';
            };
            table.rows[0].cells[j].onmousemove = function() {
                //更改鼠标样式
                if (event.offsetX > this.offsetWidth - 10)
                    this.style.cursor = 'col-resize';
                else
                    this.style.cursor = 'default';

                //取出暂存的Table Cell
                if (tTD == undefined) tTD = this;

                //调整宽度
                if (tTD.mouseDown != null && tTD.mouseDown == true) {
                    tTD.style.cursor = 'default';
                    if (tTD.oldWidth + (event.x - tTD.oldX) > 0)
                        tTD.width = tTD.oldWidth + (event.x - tTD.oldX);
                    //调整列宽
                    tTD.style.width = tTD.width;
                    tTD.style.cursor = 'col-resize';
                    //调整该列中的每个Cell
                    table = tTD; while (table.tagName != 'TABLE') table = table.parentElement;
                    for (j = 0; j < table.rows.length; j++) {
                        table.rows[j].cells[tTD.cellIndex].width = tTD.width;
                    }
                    //调整整个表
                    //table.width = tTD.tableWidth + (tTD.offsetWidth – tTD.oldWidth);
                    //table.style.width = table.width;
                }
            };
        }
    }

    myRepeater.prototype.dataFormat = function(col, val) {

        switch (col.datatype) {
            case "datetime":
                val = val.replace("T", " ").replace(/-/g, "/");
                var d = new Date(val);
                if (col.format && col.format != "") {
                    val = d.Format(col.format);
                }

                break;
            case "number":
                if (col.format && col.format != "") {

                    val = __formatNumber(val.toString(), col.format);
                }
                break;
        }

        return val;
    }

    //使用新的条件重新加载数据
    myRepeater.prototype.reload = function(param) {
        this.options.queryParam = param || {};
        this.loadData();
    };

    //获取加载的数据
    myRepeater.prototype.getData = function() {
        var that = this;
        return that.options.items;
    };
    //获取行索引
    myRepeater.prototype.getRowIndex = function(row) {
        return $(row).index() - 1;
    };
    //获取选中行
    myRepeater.prototype.getSelected = function() {
        var that = this;
        var arrSelected = [];
        var me = that.$element;
        me.find("#gridBar .gridSelectOn").each(function() {
            var item = that.options.items[$(this).index() - 1];

            item.index = $(this).index();
            arrSelected.push(item);
        });

        return arrSelected;
    };
    //获取指定索引行
    myRepeater.prototype.selectRow = function(index) {
        return this.options.data.items[index];
    };
    //开始编辑
    myRepeater.prototype.beginEdit = function(index) {
        //beg lpf
        this.options.isEdit = true;
        //end
    };
    //结束编辑
    myRepeater.prototype.endEdit = function(index) {
        //beg lpf
        this.options.isEdit = false;
        //end
    };
    //中止编辑
    myRepeater.prototype.cancelEdit = function(index) { };
    //验证数据
    myRepeater.prototype.validateData = function() {
    };
    //新增行
    myRepeater.prototype.newRow = function(item) {

        var item = $.extend({}, myRepeater.ColumnDEFAULTS, item);
        var that = this;
        //增加变量确定数据是否改变
        that.options.isChange = true;
        var me = that.$element;
        //item.rowoptype = "new";
        var mydr = [];
        mydr.push('<tr height="24" data-id="" rowtype="datarow">');
        //是否多选决定是否显示下拉框
        if (that.options.mutiSelect) {
            mydr.push('<td class="gridBorder" align="center"><INPUT name="rowChk" style="BORDER-TOP: medium none; BORDER-RIGHT: medium none; BORDER-BOTTOM: medium none; BORDER-LEFT: medium none" type=checkbox value=""></td>');
        }

        //序号
        var no = that.options.items.length + 1 + (that.options.pagesize * (that.options.pageindex - 1))
        mydr.push('<td class="gridBorder" align="center" name="rowno">' + (no) + '</td>');
        $(that.options.columns).each(function(j) {
            var col = this;
            mydr.push('<td ' + (this.hidden ? "style='display:none'" : "") + ' class="gridBorder" align="' + col.align + '" fieldname="' + col.field + '" data-type="' + col.datatype + '">');

            var val = item[col.field];
            if (typeof (col.datatype) == "object") {
                switch (col.datatype.type) {
                    case "datetime":
                        var d = new Date(val.replace("T", " ").replace(/-/g, "/"));
                        var format = col.datatype.option.format == "" ? "yyyy-MM-dd" : col.datatype.option.format;
                        val = d.Format(format);
                        break;
                    case "number":
                        if (col.datatype.option.format && col.datatype.option.format != "") {

                            val = __formatNumber(val.toString(), col.datatype.option.format);
                        }
                        break;
                }
            }
            //处理datatype,格式化数据
            mydr.push("<NOBR title='" + val + "'>" + val + "</NOBR>");
            mydr.push('</td>');
            // item[col.field] = col.defaultvalue;
        });
        mydr.push('</tr>');
        //增加数据
        that.options.items.push(item);
        me.find("#gridBar").append(mydr.join(""));

        me.find("#gridBar tr:last").bind("click", $.proxy(that.rowClick, that));
        me.find("#gridBar tr:last").on("click", "nobr", $.proxy(that.rowClick, that));
        me.find("#gridBar tr:last").click();
    };
    myRepeater.prototype.rowClick = function(e) {
        var that = this;
        var me = that.$element;
        var tr = [];
        if ($(e.target).attr("rowtype") == "datarow")
            tr = $(e.target)
        else
            tr = $(e.target).parents("tr[rowtype=datarow]");
        if (tr.length != 1)
            return;
        var i = tr.index();
        tr.removeClass("gridSelectOff");
        tr.addClass("gridSelectOn");

        if (that.options.mutiSelect) {
            var chk = tr.find("input[name=rowChk]");
            if (chk.attr("checked") == "checked") {
                chk.attr("checked", false);
                tr.removeClass("gridSelectOn");
                me.find("#chkAll").attr("checked", false);
            }
            else {
                chk.attr("checked", true);
                tr.addClass("gridSelectOn");
                if (that.options.items.length == me.find("#gridBar .gridSelectOn").length) {
                    me.find("#chkAll").attr("checked", true);
                }
            }
        }
        else {
            tr.parent().find("tr[rowtype=datarow]").each(function() {
                if (i != $(this).index()) {
                    $(this).removeClass("gridSelectOn");
                    $(this).addClass("gridSelectOff");
                }
            }

            );
        }
        var row = that.options.items[tr.index() - 1];
        //beg lpf
        //当编辑模式需要进入编辑模式
        if (that.options.isEdit) {
            if (tr[0].isEditing != true) {
                that.SetRowEdit(tr[0], row);
            }
        }
        //end 
        if (that.options.onClickRow) {
            if (that.options.onClickRow(row) === false) {
                return;
            }
        }
    }

    //删除行
    myRepeater.prototype.deleteRow = function(arrSelected) {
        if (typeof (arrSelected) == undefined || typeof (arrSelected) == null) {
            return;
        }
        var that = this;
        var me = that.$element;
        var del = [];
        $(arrSelected.reverse()).each(function() {
            //删除dom
            me.find("#gridBar tr").eq(this.index).remove();
            //维护data数据
            //delete that.options.items[this.index - 1];

            //缓存数据,标识为删除
            del.push(this.index - 1);
            if (this[that.options.idField] && this[that.options.idField] != "") {
                delete this["index"];
                that.options.deleterows.push(this);
                that.options.isChange = true;
            }

        });

        //重建序号

        me.find("#gridBar tr[id!=trHeader]").each(function(i) {
            var no = i + 1 + (that.options.pagesize * (that.options.pageindex - 1))
            $(this).find("td[name=rowno]").html(no);
        });

        //重建Data索引

        var data = $(that.options.items).filter(function(index) {
            if (("," + del.join(",") + ",").indexOf("," + index + ",") == -1) {
                return this;
            }
        });
        that.options.items = data;
    };
    //显示列
    myRepeater.prototype.showColumn = function(field) {
        var that = this;
        var me = that.$element;
        var col = [];
        $(that.options.columns).each(function() {
            if (this.field == field) {
                this.hidden = false;
                col = this;
            }
        });
        if (!col.width) {
            return;
        }
        me.find("#gridBar td[fieldname='" + field + "']").show();
        me.find("#gridBar td[fieldname='" + field + "']").attr("width", col.width);

    };
    //隐藏列
    myRepeater.prototype.hideColumn = function(field) {
        var that = this;
        var me = that.$element;
        me.find("#gridBar td[fieldname='" + field + "']").hide();
        me.find("#gridBar td[fieldname='" + field + "']").attr("width", "0px");
        $(that.options.columns).each(function() {
            if (this.field == field) {
                this.hidden = true;
            }
        });
    };
    //获取行指定字段值
    myRepeater.prototype.getRowFieldValue = function(row, field) {
        return row[field];
    };



    return myRepeater;
});
