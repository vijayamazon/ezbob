$(document).ready(function () {
    $('table.table').dataTable({
        "paging": false,
        "info": false,
        "bFilter": false,
        "aaSorting": []
    });
    
    setInterval(function () {
        var url = "/Dashboard/IsSomethingChanged";
        $.getJSON(url, null, function(res) {
            if (res.changed) {
                location.reload(true);
            }
        });
    }, 10000);
});