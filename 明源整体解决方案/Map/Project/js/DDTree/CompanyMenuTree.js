define(function(require) {
    require('jquery')
    var DDTree = require('./DDTree')
    var project = require('../project')
    function CompanyMenuTree(option) {
        var styles = []
        styles.push("  .ddtree_span{ color:#fff;border:1px solid  #758bb1; font-weight:normal;  display:inline-block} ")
        styles.push("  .ddtree_span_active{border-color:#00377a;background:#64799c} ")
        styles.push("  .ddtree_span_click{border-color:#00377a;background:#889DC2 } ")
        styles.push("   table.stdTable{ margin:0px; padding:0px;  }");
        styles.push("  nobr.mnuTitle{ height:100%; } .mnuTitle  span{ height:100%;  padding-top:4px;}")
        styles.push(" .mnuTitle   a{ height:100%; padding-top:4px; } .mnuTitle span.ddtreeWarp  { height:100%; padding-top:0px;}");
        styles.push(" .mnuTitle .ddtreeWarp  span{ height:100%; padding-top:0px; } #spnCompany { margin-right:10px;}")
        project.addStyle(styles.join(''));
        var options = {};
        options.icon = "/_imgs/ico_16_10.gif";
        options.showFullText = false;


        var lblCurCompany = $('#lblCurCompany').hide();
        lblCurCompany.prev().hide();
        var table = $(lblCurCompany.parents('table')[0]);
        table.prop('cellpadding', '0');
        table.prop('cellspacing', '0');


        var span = $('<span id="spnCompany"></span>')

        lblCurCompany.parent().prepend(span);

        DDTree.prototype.renderHeader = function() {
            var html = [];
            //   html.push('<TABLE class="ddtree"  cellSpacing=0 cellPadding=0><TD>');
            html.push('<SPAN class="ddtree_span "  style=" padding:4px 5px 0px 5px;height:100%; "  >')

            html.push('<span ><img style="margin:-1px 6px 0px 8px;vertical-align:top;padding-right:2px" width="16" height="16" src="/Project/js/DDTree/img/ico_16_10.gif" /></span>')
            html.push('<span id="ddtree_text" style="padding:0px 1px 0px 0px"></span>')
            html.push('<span ><img style="margin:4px 6px 0px 6px;vertical-align:top;" width="7" height="5" src="/Project/js/DDTree/img/icon_arrow_down.png" /></span>')
            html.push('</SPAN  >')
            //    html.push('</TD></TR></TABLE>');
            return html.join('')
        };
        DDTree.prototype.showPopup = function(html) {
            var width = this.options.width || this.$element.width();
            var height = this.options.height;
            return project.showPopup(this.$element[0], html, width + 120, height, -120)
        }
        var getMenuIfr = function() {
            var win = window;
            while (true) {
                if (win.refreshByBU)
                    return win;
                var menuIfr;
                if (win.frames && win.frames['menu']) {
                    menuIfr = win.frames['menu'];
                } else {
                    menuIfr = win.parent;
                }
                if (win == menuIfr) { return null; }
                win = menuIfr;
            }
        }



        var defaults = { treeType: 20, selectType: 0, showType: 0, showSearch: false, autoSwitchCompany: true, showSearch: false, height: 400 }
        options = $.extend(defaults, options, typeof option == 'object' && option)
        options.onchange = function(item) {
            $('.ddtreeWarp').find('.ddtree_span').removeClass('ddtree_span_click').removeClass('ddtree_span_active');
            try {
                var menuIfr = getMenuIfr();
                if (menuIfr && menuIfr.refreshByBU) {
                    menuIfr.refreshByBU(item.name);

                }
            }
            catch (e) { }
        }

        options.nodeBgColor = { "0": "#FFFFFF", "10": "#FFFFFF", "20": "#FFFFFF", "30": "#FFFFFF" };
       var ddtree= new  DDTree(span,options)      
        var interval;
        $('.ddtreeWarp').find('.ddtree_span')
        .hover(function() {
            var span1 = $(this);
            span1.addClass('ddtree_span_click');
            if (interval) {
                clearInterval(interval);
            }

            interval = setInterval(function() {
                if (!ddtree._popup.isOpen) {
                    span1.removeClass('ddtree_span_click');
                    if (interval) {
                        clearInterval(interval);
                    }
                }

            }, 100);

        }, function() {
            //   $(this).removeClass('ddtree_span_active');


        });

        $('.ddtreeWarp').on('mouseover', function() {

            ddtree.showDropDown();
        });
    }

    return CompanyMenuTree;
    //  $.fn.CompanyMenuTree = CompanyMenuTree
});
