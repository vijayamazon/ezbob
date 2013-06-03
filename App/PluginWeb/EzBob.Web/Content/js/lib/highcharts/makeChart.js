function drawChart(shopId) {
    $.get(window.gRootPath + "Underwriter/MarketPlaces/GetTeraPeakOrderItems/",
        { customerMarketPlaceId: shopId },
            function (data) {
                makeChart(data);
            }
    );
}

function makeChart(data) {

    $('#terapeakChart').highcharts({

        title: {
            text: 'Terapeak Sales'
        },

        xAxis: {
            type: 'datetime'
        },

        yAxis: {
            title: {
                text: '£ Revenue'
            }
        },

        tooltip: {
            crosshairs: true,
            shared: true,
            valueSuffix: '£',
            shadow: true,
            animation: true
        },

        legend: {
        },

        series: [{
            name: 'Revenue',
            data: data,
            zIndex: 1,
            marker: {
                fillColor: 'white',
                lineWidth: 2,
                lineColor: Highcharts.getOptions().colors[0]
            }
        }]
    });
}

