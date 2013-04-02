_V_.options.flash.swf = gRootPath + "Content/Video/video-js.swf";
_V_.options.techOrder = ["flash", "html5"];

var EzBob = EzBob || {};

$('body').on('click', '.play-link', function (e) {
    var el = $(e.currentTarget),
        divId = "#" + el.attr("data-videoDivId"),
        videoId = el.attr("data-videoId");

    $(divId).show().dialog({
        width: 675,
        height: 430,
        modal: true,
        resizable: false,
        draggable: false,
        title: "EZBOB",
        close: function () {
            _V_(videoId).pause();
        }
    });
    _V_(videoId).play();
});