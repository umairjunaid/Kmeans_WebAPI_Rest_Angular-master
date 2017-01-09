
    var jsonData = {};
var data = [];
var xdata2d = [];
var ydata2d = [];
var zdata2d = [];

$.ajax({
    url: "http://localhost:2222/api/api/kmeansdata",
    async: false,
    dataType: 'json',
    success: function (data) {
        jsonData = data;
    }
});
if (jsonData[0].NumberOfAttribs == 3) {
    for (i = 0; i < jsonData.length; i++) {

        xdata2d[i] = jsonData[i].Xaxis
        ydata2d[i] = jsonData[i].Yaxis
        zdata2d[i] = jsonData[i].Zaxis

    }
    var data = [{
        x: xdata2d,
        y: ydata2d,
        z: zdata2d,
        mode: 'markers',
        name: `Raw Data`,
        type: 'scatter3d',
        marker: {
            color: 'rgb(23, 190, 207)',
            size: 8
        }
    }, {
        alphahull: 10,
        opacity: 0.1,
        type: 'mesh3d',
        x: xdata2d,
        y: ydata2d,
        z: zdata2d,
    }];

    var layout = {
        autosize: true,
        height: 700,
        showlegend: true,
        scene: {
            aspectratio: {
                x: 1,
                y: 1,
                z: 1
            },
            camera: {
                center: {
                    x: 0,
                    y: 0,
                    z: 0
                },
                eye: {
                    x: 1.25,
                    y: 1.25,
                    z: 1.25
                },
                up: {
                    x: 0,
                    y: 0,
                    z: 1
                }
            },
            xaxis: {
                type: 'linear',
                zeroline: false
            },
            yaxis: {
                type: 'linear',
                zeroline: false
            },
            zaxis: {
                type: 'linear',
                zeroline: false
            }
        },
        width: 1000
    };
    Plotly.newPlot('raw3d', data, layout);
}
