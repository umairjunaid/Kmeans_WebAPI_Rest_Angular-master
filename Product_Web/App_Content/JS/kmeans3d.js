
var jsonData = {};
var pdata = [];
var xdata2d = [];
var ydata2d = [];
var zdata2d = [];
var datar = [];
var data = [];
$.ajax({
    url: "http://localhost:2222/api/api/kmeansdata",
    async: false,
    dataType: 'json',
    success: function (pdata) {
        jsonData = pdata;
    }
});
if (jsonData[0].NumberOfAttribs == 3) {
    //TotalNumberofCentorids = jsonData[0].TotalCentroid
    var xdata = [];
    var ydata = [];
    var zdata = [];
    var colorp = [];
    var textdata = [];
    for (j = 0; j < jsonData[0].TotalCentroid; j++) {

        r = Math.floor(Math.random() * (255 - 0 + 1)) + 0
        g = Math.floor(Math.random() * (255 - 0 + 1)) + 0
        b = Math.floor(Math.random() * (255 - 0 + 1)) + 0
        colorp[j] = `rgb(${r},${g},${b})`

    }
    //console.log(colorp);
    for (j = 0; j < jsonData[0].TotalCentroid; j++) {
        xdata = [];
        ydata = [];
        zdata = [];
        xmeandata = [];
        ymeandata = [];
        zmeandata = [];
        textdata = [];
        var center = 0;
        for (i = 0; i < jsonData.length; i++) {

            if (jsonData[i].ClusterDataDistanceToCentroid != 0) {
                if (jsonData[i].CentroidIdNumber === j) {
                    xdata.push(jsonData[i].Xaxis)
                    ydata.push(jsonData[i].Yaxis)
                    zdata.push(jsonData[i].Zaxis)
                    textdata.push(`Distance To Nearest Centroid: ${jsonData[i].ClusterDataDistanceToCentroid}<br>Original Sample Index: ${jsonData[i].ClusterDataOriginalIndex}`)
                    center = i;
                }
            }
        }
        xmeandata.push(jsonData[center].CentroidX)
        ymeandata.push(jsonData[center].CentroidY)
        zmeandata.push(jsonData[center].CentroidZ)
        var trace1 = {
            x: xdata,
            y: ydata,
            z: zdata,
            marker: {
                color: `${colorp[j]}`,
                size: 8
            },
            name: `Cluster ${j}`,
            text: textdata,
            mode: "markers",
            type: "scatter3d",
            uid: `96a0b${j}`
        };
        var mean1 = {
            x: xmeandata,
            y: ymeandata,
            z: zmeandata,
            marker: {
                color: `${colorp[j]}`,
                line: { color: 'black' },
                opacity: 0.7,
                size: 16
            },

            name: `Centroid ${j}`,
            text: `Distance To Nearest Centroid: ${jsonData[center].DistanceToNearestClusterCentroid}<br>Distance To Nearest Foreign Sample: ${jsonData[center].DistanceToNearestForeignSample}<br>Nearest Cluster: ${jsonData[center].NearestCluster}<br>Farthest Sample in Cluster: ${jsonData[center].InClusterFarthestSampleX}:${jsonData[center].InClusterFarthestSampleY}:${jsonData[center].InClusterFarthestSampleZ}<br>Max Distance In Cluster: ${jsonData[center].InClusterMaxDistance}`,
            mode: "markers",
            type: "scatter3d",
            uid: `96a0b${j}`
        };
        var data1 = [trace1, mean1];
        var layout = {
            autosize: true,
            height: 700,
            width: 1000,
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
                        x: 1.3655883731,
                        y: 1.46671332417,
                        z: 0.81940247739
                    },
                    up: {
                        x: 0,
                        y: 0,
                        z: 1
                    }
                },
                xaxis: {
                    showspikes: false,
                    type: "linear"
                },
                yaxis: {
                    showspikes: false,
                    type: "linear"
                },
                zaxis: {
                    showspikes: false,
                    type: "linear"
                }
            },
            showlegend: true,
            xaxis: {
                showgrid: true,
                title: "Xaxis",
                type: "linear"
            },
            yaxis: {
                showgrid: true,
                title: "Yaxis",
                type: "linear"
            },
            zaxis: {
                showgrid: true,
                title: "Zaxis",
                type: "linear"
            }
        };
        Plotly.plot('kmeans3d', data1, layout);
    }
}