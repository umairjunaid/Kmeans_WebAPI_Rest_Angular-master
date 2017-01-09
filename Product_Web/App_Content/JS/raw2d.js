
                       var jsonData = {};
var data = [];
var xdata2d = [];
var ydata2d = [];

$.ajax({
    url: "http://localhost:2222/api/api/kmeansdata",
    async: false,
    dataType: 'json',
    success: function (data) {
        jsonData = data;
    }
});
if (jsonData[0].NumberOfAttribs == 2) {
    for (i = 0; i < jsonData.length; i++) {

        xdata2d[i] = jsonData[i].Xaxis
        ydata2d[i] = jsonData[i].Yaxis

    }
    var trace1 = {

        x: xdata2d,
        y: ydata2d,
        mode: 'markers',
        name: `Raw Data`,
        marker: {
            size: 8
        }
    };
    ////console.log(trace1);
    var data = [trace1];

    var layout = {

        showlegend: true,
        hovermode: 'closest',
        height: 600,
        width: 1000,

    };

    Plotly.newPlot('raw2d', data, layout);
}
