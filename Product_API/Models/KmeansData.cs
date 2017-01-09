using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
/// <summary>
///  Product_API.Models is a class that contains the results per cluster of a clustering instance with additional statistics. This class members are:
/// - ClusterDataOriginalIndex: the original index of this cluster's samples before clustering.
/// - ClusterData: the samples that belong to this cluster
/// - ClusterDataDistanceToCentroid: distance between eanch sample of this cluster and it's cetroid.
/// - Centroid: the centroid of the cluster
/// - Mean : the mean of the cluster
/// - StandardDeviation: the standard deviation in the cluster
/// - InClusterFarthestSampleIndex: the index of the farthest sample from centroid (not original index)
/// - InClusterFarthestSample: the farthest sample
/// - InClusterMaxDistance: distance between the centroid and the farthest sample
/// - NearestCluster: nearest cluster number
/// - DistanceToNearestClusterCentroid: distance between the centroid of this cluster and that of nearest cluster
/// - NearestForeignSampleInNearestCluster: nearest sample belonging to the nearest cluster to this cluster's centroid
/// - DistanceToNearestForeignSampleInNearestCluster: distance between the nearest sample of the nearest cluster and this cluster's centroid
/// - NearestForeignSample: nearest sample not belonging to this cluster and this cluster's centroid
/// - DistanceToNearestForeignSample: distance between the nearest foreign sample and this cluster's centroid
/// - ClusterOfNearestForeignSample: the cluster to which the nearest foreign sample belongs
/// </summary>
namespace Kmeans_Web_API.Models
{
    public class KmeansData
    {
        public int ID { get; set; }
        //public string Name { get; set; }
        //public string Category { get; set; }
        //public decimal Price { get; set; }

        public double XaxisCheck { get; set; }
        public double YaxisCheck { get; set; }
        public double ZaxisCheck { get; set; }
        public string CheckSampleResult { get; set; }

        public double TotalCentroid { get; set; }
        public int ClustersDataLength { get; set; }
        public int NumberOfAttribs { get; set; }
        public int NumberOfClstrs { get; set; }
        public int CentroidIdNumber { get; set; }
        public double CentroidX { get; set; }
        public double CentroidY { get; set; }
        public double CentroidZ { get; set; }
        public double Xaxis { get; set; }
        public double Yaxis { get; set; }
        public double Zaxis { get; set; }
        public double ClusterDataDistanceToCentroid { get; set; }
        public int ClusterDataOriginalIndex { get; set; }
        public int ClusterOfNearestForeignSample { get; set; }
        public double DistanceToNearestClusterCentroid { get; set; }
        public double DistanceToNearestForeignSample { get; set; }
        public double DistanceToNearestForeignSampleInNearestCluster { get; set; }
        public double InClusterFarthestSampleX { get; set; }
        public double InClusterFarthestSampleY { get; set; }
        public double InClusterFarthestSampleZ { get; set; }
        public int InClusterFarthestSampleIndex { get; set; }
        public double InClusterMaxDistance { get; set; }
        public double MeanX { get; set; }
        public double MeanY { get; set; }
        public double MeanZ { get; set; }
        public int NearestCluster { get; set; }
        public double NearestForeignSampleX { get; set; }
        public double NearestForeignSampleY { get; set; }
        public double NearestForeignSampleZ { get; set; }
        public double NearestForeignSampleInNearestClusterX { get; set; }
        public double NearestForeignSampleInNearestClusterY { get; set; }
        public double NearestForeignSampleInNearestClusterZ { get; set; }
        public double StandardDeviation { get; set; }


    }
}