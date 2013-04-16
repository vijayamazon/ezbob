root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

EzBob.Underwriter.customerGrid = (settings) ->
    settings.$el = ( $ settings.el )
    list = "#" + (settings.$el.find("table").attr("id") or "")
    pagerId = "#" + (settings.$el.find("div").attr("id") or "")
    url = settings.url
    model = settings.model
    names = settings.names
    if list is "#"
        list = "#{settings.$el.attr('id')}-table"
        $("<table id='#{list}'></table>").appendTo settings.$el
        list = "##{list}"
    if pagerId is "#"
        pagerId = "#{settings.$el.attr('id')}-pager"
        $("<div id='#{pagerId}'/>").appendTo settings.$el
        pagerId = "##{pagerId}"
    options =
        sortable: true
        datatype: "json"
        mtype: "GET"
        sortname: "Id"
        sortorder: "desc"
        viewrecords: true
        hidegrid: false
        gridview: true
        hoverrows: false
        colModel: model
        colNames: names
        url: url
        pager: pagerId
        autowidth: true
        height: "auto"
        rowNum: 100
        shrinkToFit: true
        loadError: (xhr,status,error)->
            BlockUi "off"
            EzBob.ShowMessage error, status
        loadComplete: ->
            BlockUi "off"
        gridComplete: ->
            ( settings.$el.find "[data-toggle='tooltip']" ).tooltip()
            BlockUi "off"
        loadBeforeSend: ->
            BlockUi "on"

    $grid = $(list).jqGrid(options)
    $grid.jqGrid "filterToolbar",
        searchOnEnter: true
    $grid.jqGrid "navGrid", pagerId,
        del: false
        add: false
        edit: false
        search: true
    , {}, {}, {},
        multipleSearch: true

    EzBob.App.vent.on "uw:grids:refresh", ->
        $(list).trigger "reloadGrid"

    EzBob.App.vent.on "uw:grids:performsearch", ->
        $(list)[0].triggerToolbar()

    checkbox = $("<input type='checkbox'>Show test customers</input>").on("change", ->
        isTest = checkbox.is(":checked")
        checkboxes = $(".show-test-customers input")
        if isTest
            checkboxes.attr "checked", "checked"
        else
            checkboxes.removeAttr "checked"
        checkboxes.trigger "reload"
    )
    checkbox.on "reload", ->
        isTest = checkbox.is(":checked")
        $(list).jqGrid "setGridParam",
            postData:
                IsTest: isTest
        $(list).trigger "reloadGrid"

    $div = $("<div class='show-test-customers'></div>")
    checkbox.appendTo $div
    $div.appendTo settings.$el
    settings.$el.on "dblclick", "tr", (ev) ->
        href = $(ev.currentTarget).find("a").first().attr("href")
        window.location.href = href if href
        false

# --- forrmaters ---
$.fn.fmatter.profileWithTypeLink = (cellval, opts) ->
    href = "#profile/#{cellval.id or cellval}/#{opts.gid.replace('-table', '')}"
    text = cellval.text or cellval
    $.fn.fmatter.withScrollbar "<a class='profileLink' href='#{href}'>#{text}</a>"

$.fn.fmatter.showMedalIcon = (cellval) ->
    text = cellval.text or cellval
    "<i data-toggle='tooltip' title='#{text}' class='#{text.toLowerCase().replace(/\s/g, '')}'></i>"

$.fn.fmatter.showMPsIcon = (cellval, opt) ->
    mps = cellval.text or cellval
    retVal = ""
    _.each mps, (val) ->
        retVal += $.fn.fmatter.showMedalIcon(val)
    "<div style='overflow: auto; width: auto'>#{retVal}</div>"

$.fn.fmatter.profileLink = (cellval, opts) ->
    href = "#profile/#{(cellval.id or cellval)}"
    text = cellval.text or cellval
    $.fn.fmatter.withScrollbar "<a class='profileLink' href='#{href}'>#{text}</a>"

$.fn.fmatter.datetimeNative = (cellval, opts, row) ->
    return "" unless cellval
    date = moment(cellval)
    #IE8 fix
    if isNaN(date)
        q = cellval
        date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)), parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)))
        date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)) - 1, parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)))
        return date.format("dd.MM.yyyy HH:mm")
    date.format "DD.MM.YYYY HH:mm"

$.fn.fmatter.dateNative = (cellval, opts, row) ->
    return "" unless cellval
    date = moment(cellval)
    #IE8 fix
    if isNaN(date)
        q = cellval
        date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)), parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)))
        date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)) - 1, parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)))
        return date.format("dd.MM.yyyy")
    date.format "DD.MM.YYYY"

$.fn.fmatter.CheckDateWithNow = (cellval, opts, row) ->
    date = moment(cellval)
    isPast = moment(new Date) > date
    el = $.fn.fmatter.dateNative(cellval, opts, row)
    el = "<span class='blue'>#{el}</span>" if not isPast
    el

$.fn.fmatter.withScrollbar = (cellval) ->
    text = cellval.text or cellval
    "<div style='overflow: auto; width: auto'>#{text}</div>"