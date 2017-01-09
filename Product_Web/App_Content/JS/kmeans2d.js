
var jsonData = {};
var pdata = [];
var xdata2d = [];
var ydata2d = [];

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
if (jsonData[0].NumberOfAttribs == 2) {
    //TotalNumberofCentorids = jsonData[0].TotalCentroid
    var xdata = [];
    var ydata = [];
    var zdata = [];
    var colorp = [];
    var textdata = [];
    var checkcenteroid = [];
    for (j = 0; j < jsonData[0].TotalCentroid; j++) {

        r = Math.floor(Math.random() * (255 - 0 + 1)) + 0
        g = Math.floor(Math.random() * (255 - 0 + 1)) + 0
        b = Math.floor(Math.random() * (255 - 0 + 1)) + 0
        colorp[j] = `rgb(${r},${g},${b})`
    }
    var center = 0;
    for (j = 0; j < jsonData[0].TotalCentroid; j++) {
        xdata = [];
        ydata = [];
        zdata = [];
        xmeandata = [];
        ymeandata = [];
        textdata = [];
        for (i = 0; i < jsonData.length; i++) {
            if (jsonData[i].ClusterDataDistanceToCentroid != 0) {
                if (jsonData[i].CentroidIdNumber === j) {
                    xdata.push(jsonData[i].Xaxis)
                    ydata.push(jsonData[i].Yaxis)
                    textdata.push(`Distance To Nearest Centroid: ${jsonData[i].ClusterDataDistanceToCentroid}<br>Original Sample Index: ${jsonData[i].ClusterDataOriginalIndex}`)
                    center = i;
                }
            }
        }
        xmeandata.push(jsonData[center].CentroidX)
        ymeandata.push(jsonData[center].CentroidY)
        //console.log(textdata);
        var trace1 = {
            x: xdata,
            y: ydata,
            mode: 'markers',
            marker: {
                color: `${colorp[j]}`,
                size: 8
            },
            name: `Cluster ${j}`,
            text: textdata,

        };
        var mean1 = {
            x: xmeandata,
            y: ymeandata,
            mode: 'markers',
            marker: {
                color: `${colorp[j]}`,
                line: { color: 'black' },
                opacity: 0.7,
                size: 16
            },
            name: `Centroid ${j}`,
            text: `Distance To Nearest Centroid: ${jsonData[center].DistanceToNearestClusterCentroid}<br>Distance To Nearest Foreign Sample: ${jsonData[center].DistanceToNearestForeignSample}<br>Nearest Cluster: ${jsonData[center].NearestCluster}<br>Farthest Sample in Cluster: ${jsonData[center].InClusterFarthestSampleX}:${jsonData[center].InClusterFarthestSampleY}<br>Max Distance In Cluster: ${jsonData[center].InClusterMaxDistance}`,

        };


        var data1 = [trace1, mean1];
        var layout = {
            showlegend: true,
            hovermode: 'closest',
            height: 600,
            width: 1000
        };
        // console.log(data1);
        Plotly.plot('kmeans2d', data1, layout);
    }
}
