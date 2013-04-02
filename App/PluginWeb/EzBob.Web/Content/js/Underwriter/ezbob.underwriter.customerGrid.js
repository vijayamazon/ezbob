var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.customerGrid = function (settings) {
    settings.el = $(settings.el);
    var list = '#' + (settings.el.find('table').attr('id') || ''),
                pagerId = '#' + (settings.el.find('div').attr('id') || ''),
                url = settings.url,
                model = settings.model,
                names = settings.names;
    if (list == "#") {
        list = settings.el.attr('id') + "-table";
        $("<table id=" + list + "></table>").appendTo(settings.el);
        list = '#' + list;
    }
    if (pagerId == "#") {
        pagerId = settings.el.attr('id') + "-pager";
        $("<div id=" + pagerId + "/>").appendTo(settings.el);
        pagerId = '#' + pagerId;
    }
    var options = {
        sortable: true,
        datatype: 'json',
        mtype: 'GET',
        sortname: 'Id',
        sortorder: 'desc',
        viewrecords: true,
        hidegrid: false,
        gridview: true,
        hoverrows: false,
        colModel: model,
        colNames: names,
        url: url,
        pager: pagerId,
        autowidth: true,
        height: 'auto',
        rowNum: 100,
        shrinkToFit: true
    };
    $(list)
            .jqGrid(options)
            .jqGrid('filterToolbar', { searchOnEnter: true })
            .jqGrid('navGrid', pagerId, { del: false, add: false, edit: false, search: true }, {}, {}, {}, { multipleSearch: true });

    EzBob.App.vent.on('uw:grids:refresh', function () {
        $(list).trigger("reloadGrid");
    });
    
    EzBob.App.vent.on('uw:grids:performsearch', function () {
        $(list)[0].triggerToolbar();
    });
    
    var checkbox = $("<input type='checkbox'>Show test customers</input>").on('change', function () {
        var isTest = checkbox.is(':checked'),
            checkboxes = $('.show-test-customers input');
        
        if (isTest){
            checkboxes.attr('checked', 'checked');
        } else{
            checkboxes.removeAttr('checked');
        }

        checkboxes.trigger('reload');
    });


    checkbox.on('reload', function(){
        var isTest = checkbox.is(':checked');
        $(list).jqGrid('setGridParam', { postData: { 'IsTest': isTest } });
        $(list).trigger("reloadGrid");
    });

    var div = $("<div class='show-test-customers'></div>");
    checkbox.appendTo(div);
    div.appendTo(settings.el);

    settings.el.on('dblclick', 'tr', function (ev) {
        var href = $(ev.currentTarget).find('a').first().attr('href');
        if (href) {
            window.location.href = href;
        }
        return false;
    });
    
    $(window).bind('resize', function () {
        settings.el.setGridWidth($(window).width()*0.8);
    }).trigger('resize');
};

$.fn.fmatter.profileWithTypeLink = function (cellval, opts) {
    var href = "#profile/{0}/{1}".f(cellval.id || cellval, opts.gid.replace('-table',''));
    var text = cellval.text || cellval;
    var link = "<a class='profileLink' href='" + href + "'>" + text + "</a>";

    return link;
};

$.fn.fmatter.profileLink = function (cellval, opts) {
    var href = "#profile/" + (cellval.id || cellval);
    var text = cellval.text || cellval;
    var link = "<a class='profileLink' href='" + href + "'>" + text + "</a>";

    return link;
};

$.fn.fmatter.datetimeNative = function (cellval, opts, row) {
    if (!cellval) return "";
    var date = moment(cellval);
    //IE8 fix
    if (isNaN(date)) {
        var q = cellval;
        date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)), parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)));
        date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)) - 1, parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)));
        return date.format("dd.MM.yyyy HH:mm");
    }
    return date.format("DD.MM.YYYY HH:mm");
};

$.fn.fmatter.dateNative = function (cellval, opts, row) {
    if (!cellval) return "";
    var date = moment(cellval);
    //IE8 fix
    if (isNaN(date)) {
        var q = cellval;
        date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)), parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)));
        date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)) - 1, parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)));
        return date.format("dd.MM.yyyy");
    }
    return date.format("DD.MM.YYYY");
};