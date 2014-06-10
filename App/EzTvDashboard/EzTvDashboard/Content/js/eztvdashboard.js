$(document).ready(function () {
    var refreshIntervalId = setInterval(function () {
        var url = "/Dashboard/IsSomethingChanged";
        $.getJSON(url, null, function(res) {
            if (res.changed) {
                clearInterval(refreshIntervalId);
                window.location.href = "/Dashboard/Redirect";
            }
            $('.last-updated').text('last updated ' + moment.utc(res.lastChanged).fromNow());
        });
    }, 10000);
});