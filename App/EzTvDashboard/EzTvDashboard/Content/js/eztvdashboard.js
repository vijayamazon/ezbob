$(document).ready(function () {
    var refreshIntervalId = setInterval(function () {
        var url = "/Dashboard/IsSomethingChanged";
        $.getJSON(url, null, function(res) {
            if (res.changed) {
                //clearInterval(refreshIntervalId);
                $.ajax({
                    url: "/Dashboard/Dashboard",
                    type: "GET",
                })
                .done(function (partialViewResult) {
                    $("#main-content").html(partialViewResult);
                });
            }
            $('.last-updated').text('last updated ' + moment.utc(res.lastChanged).fromNow());
        });
    }, 10000);
});