(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.customerGrid = function(settings) {
    var $div, $grid, checkbox, list, model, names, options, pagerId, url;

    settings.$el = $(settings.el);
    list = "#" + (settings.$el.find("table").attr("id") || "");
    pagerId = "#" + (settings.$el.find("div").attr("id") || "");
    url = settings.url;
    model = settings.model;
    names = settings.names;
    if (list === "#") {
      list = "" + (settings.$el.attr('id')) + "-table";
      $("<table id='" + list + "'></table>").appendTo(settings.$el);
      list = "#" + list;
    }
    if (pagerId === "#") {
      pagerId = "" + (settings.$el.attr('id')) + "-pager";
      $("<div id='" + pagerId + "'/>").appendTo(settings.$el);
      pagerId = "#" + pagerId;
    }
    options = {
      sortable: true,
      datatype: "json",
      mtype: "GET",
      sortname: "Id",
      sortorder: "desc",
      viewrecords: true,
      hidegrid: false,
      gridview: true,
      hoverrows: false,
      colModel: model,
      colNames: names,
      url: url,
      pager: pagerId,
      autowidth: true,
      height: "auto",
      rowNum: 100,
      shrinkToFit: true,
      loadError: function(xhr, status, error) {
        BlockUi("off", $(list));
        return console.log(status);
      },
      loadComplete: function(data) {
        return BlockUi("off", $(list));
      },
      gridComplete: function() {
        (settings.$el.find("[data-toggle='tooltip']")).tooltip();
        return BlockUi("off", $(list));
      },
      loadBeforeSend: function() {
        return BlockUi("on", $(list));
      }
    };
    $grid = $(list).jqGrid(options);
    $grid.jqGrid("filterToolbar", {
      searchOnEnter: true
    });
    $grid.jqGrid("navGrid", pagerId, {
      del: false,
      add: false,
      edit: false,
      search: true
    }, {}, {}, {}, {
      multipleSearch: true
    });
    EzBob.App.vent.on("uw:grids:refresh", function() {
      return $("" + list + ":visible").trigger("reloadGrid");
    });
    EzBob.App.vent.on("uw:grids:performsearch", function() {
      return $(list)[0].triggerToolbar();
    });
    checkbox = $("<input type='checkbox'>Show test customers</input>").on("change", function() {
      var checkboxes, isTest;

      isTest = checkbox.is(":checked");
      EzBob.Config.isTest = isTest;
      checkboxes = $(".show-test-customers input");
      if (isTest) {
        checkboxes.attr("checked", "checked");
      } else {
        checkboxes.removeAttr("checked");
      }
      return checkboxes.trigger("reload");
    });
    checkbox.on("reload", function() {
      var isTest;

      isTest = checkbox.is(":checked");
      EzBob.Config.isTest = isTest;
      $(list).jqGrid("setGridParam", {
        postData: {
          IsTest: isTest
        }
      });
      return $(list).trigger("reloadGrid");
    });
    $div = $("<div class='show-test-customers'></div>");
    checkbox.appendTo($div);
    $div.appendTo(settings.$el);
    return settings.$el.on("dblclick", "tr", function(ev) {
      var href;

      href = $(ev.currentTarget).find("a").first().attr("href");
      if (href) {
        window.location.href = href;
      }
      return false;
    });
  };

  $.fn.fmatter.profileWithTypeLink = function(cellval, opts) {
    var href, text;

    href = "#profile/" + (cellval.id || cellval) + "/" + (opts.gid.replace('-table', ''));
    text = cellval.text || cellval;
    return $.fn.fmatter.withScrollbar("<a class='profileLink' href='" + href + "'>" + text + "</a>");
  };

  $.fn.fmatter.showMedalIcon = function(cellval) {
    var text;

    text = cellval.text || cellval;
    return "<i data-toggle='tooltip' title='" + text + "' class='" + (text.toLowerCase().replace(/\s/g, '')) + "'></i>";
  };

  $.fn.fmatter.showMPIcon = function(cellval, aryCGAccounts) {
    var className, text;

    text = cellval.text || cellval;
    className = text.replace(/\s|\d/g, '');
    className = aryCGAccounts[className] ? 'cgaccount' : className.toLowerCase();
    return "<i data-toggle='tooltip' title='" + text + "' class='" + className + "'></i>";
  };

  $.fn.fmatter.showMPsIcon = function(cellval, opt) {
    var mps, retVal;

    if (!$.fn.fmatter.showMPsIcon.prototype.CGAccounts) {
      $.fn.fmatter.showMPsIcon.prototype.CGAccounts = $.parseJSON($('div#cg-account-list').text());
    }
    mps = cellval.text || cellval;
    mps = mps.split(',').clean("");
    retVal = "";
    _.each(mps, function(val) {
      return retVal += $.fn.fmatter.showMPIcon(val, $.fn.fmatter.showMPsIcon.prototype.CGAccounts);
    });
    return "<div style='overflow: auto; width: 102%'>" + (retVal + ' ') + "</div>";
  };

  $.fn.fmatter.profileLink = function(cellval, opts) {
    var href, text;

    href = "#profile/" + (cellval.id || cellval);
    text = cellval.text || cellval;
    return $.fn.fmatter.withScrollbar("<a class='profileLink' href='" + href + "'>" + text + "</a>");
  };

  $.fn.fmatter.datetimeNative = function(cellval, opts, row) {
    var date, q;

    if (!cellval) {
      return "";
    }
    date = moment(cellval);
    if (isNaN(date)) {
      q = cellval;
      date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)), parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)));
      date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)) - 1, parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)));
      return date.format("dd.MM.yyyy HH:mm");
    }
    return date.format("DD.MM.YYYY HH:mm");
  };

  $.fn.fmatter.dateNative = function(cellval, opts, row) {
    var date, q;

    if (!cellval) {
      return "";
    }
    date = moment(cellval);
    if (isNaN(date)) {
      q = cellval;
      date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)), parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)));
      date = new Date(parseInt(q.substring(0, 4)), parseInt(q.substring(5, 7)) - 1, parseInt(q.substring(8, 10)), parseInt(q.substring(11, 13)), parseInt(q.substring(14, 16)));
      return date.format("dd.MM.yyyy");
    }
    return date.format("DD.MM.YYYY");
  };

  $.fn.fmatter.CheckDateWithNow = function(cellval, opts, row) {
    var date, el, isPast;

    date = moment(cellval);
    isPast = moment(new Date) > date;
    el = $.fn.fmatter.dateNative(cellval, opts, row);
    if (!isPast) {
      el = "<span class='blue'>" + el + "</span>";
    }
    return el;
  };

  $.fn.fmatter.withScrollbar = function(cellval) {
    var text;

    text = cellval.text || cellval;
    return "<div style='overflow: auto; width: auto'>" + text + "</div>";
  };

}).call(this);
