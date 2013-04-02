var jqgridHelpers = {
    scortoDateInit: function (el) {
        var options = {
            dateFormat: 'dd.mm.yy',
            changeYear: true,
            changeMonth: true,
            yearRange: '1900:' + new Date().getFullYear(),
            showButtonPanel: true
        };
        var $el = $(el), $parent = $el.parent();
        $el.datepicker(options).addClass('datepickerCol').change(function () {
            $("#list")[0].triggerToolbar();
        });
        return;
    },

    beforeClear: function () {
        $('.ui-th-column input').val('');
        // Очистка выпадающего списка, который содержит Стандарт/Не стандарт значения. Колонка Стандарт заявки
        $('.ui-th-column select').val($('.ui-th-column option[value="0"]').val());
    },

    // options.menuItemId - menu item id
    // options.showStatus - show status col
    // options.url - applications url
    // options colNames
    // options colModel
    // options.pool
    // options.secappid
    // options.userid
    // options.bntContainer
    initGrid: function (options) {
        var settings = { "strings": {
            "setupColumns": "Setup columns",
            "editFilter": "Edit Filter",
            "clearFilter": "Clear Filter",
            "cancelAllSettings": "Cancel all settings",
            "refresh": "Refresh"
        }
        };

        $.extend(settings, options);

        var jqOptions = {
            sortable: true,
            url: options.url || window.location.pathname + "/GetApplications",
            datatype: 'json',
            mtype: 'GET',
            colNames: options.colNames,
            colModel: options.colModel,
            height: 'auto',
            pager: '#pager',
            rowNum: window.localStorage['select.ui-pg-selbox'] || 10,
            rowList: [10, 20, 30, 40],
            sortname: 'Id',
            sortorder: 'asc',
            viewrecords: true,
            hidegrid: false,
            gridview: true,
            hoverrows: false,

            loadBeforeSend: function(xhr) {
                if (!options.headers) return;
                for (var header in options.headers) {
                    xhr.setRequestHeader(header, options.headers[header]);
                }
            },
            beforeSelectRow: function(rowid, e) {
                return false;
            }
        };

        $.extend(jqOptions, options.jqOptions);

        $("#list").jqGrid(jqOptions)
            .jqGrid('filterToolbar', { searchOnEnter: true })
            .jqGrid('navGrid', '#pager', { searchtext: settings.strings.editFilter, searchtitle: settings.strings.editFilter, del: false, add: false, edit: false, search: true, refreshtitle: settings.strings.clearFilter, refreshtext: settings.strings.clearFilter, beforeRefresh: jqgridHelpers.beforeClear }, {}, {}, {}, { multipleSearch: true });


        if (options.bntContainer) {
            $('<button/>').text(settings.strings.setupColumns).appendTo(options.bntContainer).click(columntReorderClick).button();
        } else {
            $("#list").jqGrid('navButtonAdd', '#pager', {
                caption: settings.strings.setupColumns,
                title: settings.strings.setupColumns,
                onClickButton: columntReorderClick
            });
        }


        function columntReorderClick() {
            $("#list").jqGrid('columnChooser', {
                msel_opts: {
                    dividerLocation: 0.5,
                    searchable: false
                },
                resizable: 'false',
                modal: true,
                width: 600,
                done: function (perm) {
                    if (perm) {
                        $("#list").jqGrid("remapColumns", perm, true);
                        saveLayout();
                    };
                }
            });
            return false;
        };

        if (options.bntContainer) {
            $('<button/>').text(settings.strings.cancelAllSettings).appendTo(options.bntContainer).click(clearLayout).button();
        } else {
            $("#list").jqGrid('navButtonAdd', '#pager', {
                caption: settings.strings.cancelAllSettings,
                title: settings.strings.cancelAllSettings,
                onClickButton: clearLayout
            });
        }

        if (options.bntContainer) {
            $('<button/>').text(settings.strings.refresh).appendTo(options.bntContainer).css('margin-left', '10px').click(function () {
                $("#list").trigger("reloadGrid");
                return false;
            }).button();
        } else {
            $("#list").jqGrid('navButtonAdd', '#pager', {
                caption: settings.strings.refresh,
                title: settings.strings.refresh,
                css: { "margin-left": '10px' },
                onClickButton: function () {
                    $("#list").trigger("reloadGrid");
                    return false;
                }
            });
        }

        $("#tableHolder .ui-sortable").bind("sortupdate", function () {
            setTimeout(saveLayout, 10);
        });

        function clearLayout() {
            localStorage.removeItem(getPersistentKey());
            document.location.href = document.location.href;
        }

        function saveLayout() {
            var colModel = $("#list").jqGrid("getGridParam", "colModel");
            localStorage.setItem(getPersistentKey(), JSON.stringify(colModel));
        };

        function getPersistentKey() {
            return "appgrid" + options.userid + '-' + options.menuitemid;
        }

        function restoreLayout() {
            var i = 0, j = 0, perm = [];
            var model = JSON.parse(localStorage.getItem(getPersistentKey()));
            if (!model) return;
            var modelCurrent = $("#list").jqGrid("getGridParam", "colModel");

            for (i = 0; i < modelCurrent.length; i++) {
                var col = modelCurrent[i];
                for (j = 0; j < model.length; j++) {
                    if (model[j].name === col.name) {
                        perm[j] = i;
                        break;
                    }
                }
            }

            for (i = 0; i < model.length; i++) {
                $("#list").jqGrid(model[i].hidden ? "hideCol" : "showCol", model[i].name);
            }

            $("#list").jqGrid("remapColumns", perm, true);
        };

        restoreLayout();

        $(window).bind('resize', function () {
            var e = $('#menuTabs');
            var w = e.length ? e.width() - 30 : $('#content').width();

            $("#list").setGridWidth(w);
        }).trigger('resize');

        $('.ui-th-column').click(function (e) {
            if (!e.srcElement || e.srcElement.tagName != 'TH') return;
            $(this).children().click();
            return false;
        });

        $('select.ui-pg-selbox').change(function () {
            window.localStorage['select.ui-pg-selbox'] = this.value;
        });

        if (window.localStorage['select.ui-pg-selbox']) {
            $('select.ui-pg-selbox').val(window.localStorage['select.ui-pg-selbox']);
        }
        return $("#list");
    }
};

$.fn.fmatter.showlinkid = function (cellval, opts, row) {
    if (!cellval.Enabled) return cellval.Text;
    return "<a href='" + cellval.Href + "' onclick=\"" + cellval.OnClick + "\">" + cellval.Text + "</a>";
};

$.fn.fmatter.datetime = function(cellval, opts, row) {
    return cellval.replace(" ", "<br/>");
};

$.fn.fmatter.rejectapp = function(cellval, opts, row) {
    return "<a href='#' onclick='RejectApplication (" + cellval + "); return false;'>" + 'Відмовити' + "</a>";
};

$.fn.fmatter.tmpfile = function(cellval, opts, row) {
    return "<a href='#' onclick='GetTmpFile (" + cellval + "); return false;'>" + 'TMP' + "</a>";
};

$.fn.fmatter.showhistory = function(cellval, opts, row) {
    return "<a href='#' onclick='ShowHistory (" + cellval + "); return false;'>" + 'Історія заявки' + "</a>";
};

$.fn.fmatter.tty = function(cellval, opts, row) {
    return "<span class='TTYCounter' alt=" + cellval.alt + ">" + cellval.text + "</span>";
};

$.fn.fmatter.protocol = function(cellval, opts, row) {
    if (!cellval || cellval.length == 0) return "Документи відсутні";
    var r = "<ul>";
    for (var i = 0; i < cellval.length; i++) {
        r += "<li>";
        r += "<a href='#' onclick='" + cellval[i].OnClick + "' class='" + cellval[i].Css + "'>" + cellval[i].Text + "</a>";
        r += "</li>";
    }
    r += "</ul>";
    return r;
};

$.fn.fmatter.attachments = function(cellval, opts, row) {
    if (cellval.length == 0) return "Документи відсутні";
    var r = "<ul>";
    for (var i = 0; i < cellval.length; i++) {
        r += "<li>";
        r += "<a href=../ExportAttachment.ashx?appid=" + row[0] + "&fileId=" + cellval[i].detailid + "&fileName=" + cellval[i].filename + "&hash=" + cellval[i].hash + ">" + cellval[i].filename + "</a>";
        r += "</li>";
    }
    r += "</ul>";
    return r;
};

$.extend($.ui.multiselect, {
    locale: {
        addAll: 'Додати всі',
        removeAll: 'Видалити всі',
        itemsCount: 'обрано'
    }
});