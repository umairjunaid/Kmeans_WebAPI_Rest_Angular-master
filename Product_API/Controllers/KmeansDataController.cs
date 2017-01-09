using Kmeans_Web_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AnomalyDetection.Interface;
using System.Web.Hosting;

namespace Kmeans_Web_API.Controllers
{
    //[EnableCors(origins: "http://localhost:3442", headers: "*", methods: "*")]
    public class KmeansDataController : ApiController
    {

        //System.Web.HttpContext.Current.Server.MapPath("File1.t‌​xt");
         //   HttpContext.Current.Server.MapPath("~/Data/data.html");
          //  HostingEnvironment.MapPath("~/Data/data.html");
         
        public static Lazy<List<KmeansData>> kmeansdata = new Lazy<List<KmeansData>>();//Static variable use only for demo, don’t use unless until require in project. 
        public static int PgaeLoadFlag = 1; // Page load count. 
        public static int ProductID = 4;
        public string checksample = "Null";
        public static int numofclstrs = 0;
        public KmeansDataController()
        {
            if (numofclstrs == 0)
            {
                numofclstrs = 3;
            }
            string pth= HostingEnvironment.MapPath("~/Data/data.html");
            int numberofclusters = 3;
            /// desired number of clusters
            int numberofattributes = 2;
            /// X,Y,Z components
            

            double[][] result = MockApi.MockApi.CSVtoDoubleJaggedArray(HostingEnvironment.MapPath("~/FileUpload/data.csv"));
            /// CSV to Array Conversion
            if (System.IO.File.Exists(HostingEnvironment.MapPath("~/FileUpload/data.csv")))
                numberofattributes = result[0].Length;
            /// Number of attributes are 3 for 3D and 2 for 2D
            IAnomalyDetectionApi X = new AnomalyDetectionApi.AnomalyDetectionAPI(result, numofclstrs);
            ClusteringSettings Y = new ClusteringSettings(result, 10, numofclstrs, numberofattributes, HostingEnvironment.MapPath("~/FileUpload/cluster.json"),Replace:true);
            /// Set number of clusters, max iterations and give data in result
            X.ImportNewDataForClustering(Y);
            /// CheckSample is a function that detect to which cluster the given sample belongs to.
            ClusteringResults[] r;
            /// All the Kmeans results needed for plotting
            AnomalyDetectionResponse Z = X.GetResults(HostingEnvironment.MapPath("~/FileUpload/cluster.json"), out r);
            /// Codes for Errors and Success
            //if (PgaeLoadFlag == 1) //use this only for first time page load
            //{
                if (r != null)// in case result contains some value we need to add results
                {
                    if (kmeansdata.IsValueCreated == true)//if already points are created we need to reinitiallize
                    {
                        checksample = kmeansdata.Value[0].CheckSampleResult;
                        kmeansdata = new Lazy<List<KmeansData>>();
                        
                    }
                    int i = 0; //Counts number of Samples we have
                    for (int j = 0; j < numofclstrs; j++)//Runs for numberofclusters defined earlier
                    {
                        for (int k = 0; k < r[j].ClusterData.Length; k++)// add the data in web Api
                        {
                            if (numberofattributes == 3)// add data for 2D
                                kmeansdata.Value.Add(new KmeansData { ID = i, ClustersDataLength = r[j].ClusterData.Length, TotalCentroid = r.Length, CentroidIdNumber = j, CentroidX = r[j].Centroid[0], CentroidY = r[j].Centroid[1], CentroidZ = r[j].Centroid[2], Xaxis = r[j].ClusterData[k][0], Yaxis = r[j].ClusterData[k][1], Zaxis = r[j].ClusterData[k][2], ClusterDataDistanceToCentroid = r[j].ClusterDataDistanceToCentroid[k], ClusterDataOriginalIndex = r[j].ClusterDataOriginalIndex[k], ClusterOfNearestForeignSample = r[j].ClusterOfNearestForeignSample, DistanceToNearestClusterCentroid = r[j].DistanceToNearestClusterCentroid, DistanceToNearestForeignSample = r[j].DistanceToNearestForeignSample, DistanceToNearestForeignSampleInNearestCluster = r[j].DistanceToNearestForeignSampleInNearestCluster, InClusterFarthestSampleX = r[j].InClusterFarthestSample[0], InClusterFarthestSampleY = r[j].InClusterFarthestSample[1], InClusterFarthestSampleZ = r[j].InClusterFarthestSample[2], InClusterFarthestSampleIndex = r[j].InClusterFarthestSampleIndex, InClusterMaxDistance = r[j].InClusterMaxDistance, MeanX = r[j].Mean[0], MeanY = r[j].Mean[1], MeanZ = r[j].Mean[2], NearestCluster = r[j].NearestCluster, NearestForeignSampleX = r[j].NearestForeignSample[0], NearestForeignSampleY = r[j].NearestForeignSample[1], NearestForeignSampleZ = r[j].NearestForeignSample[2], NearestForeignSampleInNearestClusterX = r[j].NearestForeignSampleInNearestCluster[0], NearestForeignSampleInNearestClusterY = r[j].NearestForeignSampleInNearestCluster[1], NearestForeignSampleInNearestClusterZ = r[j].NearestForeignSampleInNearestCluster[2], NumberOfAttribs= numberofattributes });
                            if (numberofattributes == 2)// add data for 3D
                                kmeansdata.Value.Add(new KmeansData { ID = i, ClustersDataLength = r[j].ClusterData.Length, TotalCentroid = r.Length, CentroidIdNumber = j, CentroidX = r[j].Centroid[0], CentroidY = r[j].Centroid[1], Xaxis = r[j].ClusterData[k][0], Yaxis = r[j].ClusterData[k][1], ClusterDataDistanceToCentroid = r[j].ClusterDataDistanceToCentroid[k], ClusterDataOriginalIndex = r[j].ClusterDataOriginalIndex[k], ClusterOfNearestForeignSample = r[j].ClusterOfNearestForeignSample, DistanceToNearestClusterCentroid = r[j].DistanceToNearestClusterCentroid, DistanceToNearestForeignSample = r[j].DistanceToNearestForeignSample, DistanceToNearestForeignSampleInNearestCluster = r[j].DistanceToNearestForeignSampleInNearestCluster, InClusterFarthestSampleX = r[j].InClusterFarthestSample[0], InClusterFarthestSampleY = r[j].InClusterFarthestSample[1], InClusterFarthestSampleIndex = r[j].InClusterFarthestSampleIndex, InClusterMaxDistance = r[j].InClusterMaxDistance, MeanX = r[j].Mean[0], MeanY = r[j].Mean[1], NearestCluster = r[j].NearestCluster, NearestForeignSampleX = r[j].NearestForeignSample[0], NearestForeignSampleY = r[j].NearestForeignSample[1], NearestForeignSampleInNearestClusterX = r[j].NearestForeignSampleInNearestCluster[0], NearestForeignSampleInNearestClusterY = r[j].NearestForeignSampleInNearestCluster[1], NumberOfAttribs = numberofattributes });
                            i++;
                        }
                    }

                //   PgaeLoadFlag++;
                //}
                kmeansdata.Value[0].CheckSampleResult = checksample;
            }
        }
        
        // GET api/product
        public List<KmeansData> GetAllKmeansData() //get method
        {
            //Instedd of static variable you can use database resource to get the data and return to API
            return kmeansdata.Value; //return all the product list data
        }

        // GET api/product/5
        public IHttpActionResult GetKmeansData(int id)
        {
            KmeansData data = kmeansdata.Value.FirstOrDefault((p) => p.ID == id);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

       // POST api/product
       /*
        public void KmeansDataAdd(KmeansData data) //post method
        {
            
            data.ID = ProductID;
            kmeansdata.Value.Add(data); //add the post product data to the product list
            ProductID++;
            //instead of adding product data to the static product list you can save data to the database.
        }
       /*/
        public void KmeansDataCheck(KmeansData data) //post method
        {
            if (data.NumberOfClstrs != 0 && data.NumberOfClstrs != null) {
                numofclstrs = data.NumberOfClstrs;
              //  if (System.IO.File.Exists(@"F:\ProData\cluster.json"))// in case some files exist already delete them and upload the current one
              //  {
              //      System.IO.File.Delete(@"F:\ProData\cluster.json");
              //      System.IO.File.Delete(@"F:\ProData\Result\cluster.json");
              //  }
                int numberofattributes = 2;
                /// X,Y,Z components


                double[][] result = MockApi.MockApi.CSVtoDoubleJaggedArray(HostingEnvironment.MapPath("~/FileUpload/data.csv"));
                /// CSV to Array Conversion
                if (System.IO.File.Exists(@"F:\ProData\data.csv"))
                    numberofattributes = result[0].Length;
                /// Number of attributes are 3 for 3D and 2 for 2D
                IAnomalyDetectionApi X = new AnomalyDetectionApi.AnomalyDetectionAPI(result, numofclstrs);
                ClusteringSettings Y = new ClusteringSettings(result, 10, numofclstrs, numberofattributes, HostingEnvironment.MapPath("~/FileUpload/cluster.json"), Replace:true);
                /// Set number of clusters, max iterations and give data in result
                AnomalyDetectionResponse AB = X.ImportNewDataForClustering(Y);
                /// CheckSample is a function that detect to which cluster the given sample belongs to.
                ClusteringResults[] r;
                /// All the Kmeans results needed for plotting
                AnomalyDetectionResponse Z = X.GetResults(HostingEnvironment.MapPath("~/FileUpload/cluster.json"), out r);
                /// Codes for Errors and Success
                //if (PgaeLoadFlag == 1) //use this only for first time page load
                //{
            //    if (r != null)// in case result contains some value we need to add results
             //   {
                    if (kmeansdata.IsValueCreated == true)//if already points are created we need to reinitiallize
                    {
                        checksample = kmeansdata.Value[0].CheckSampleResult;
                        kmeansdata = new Lazy<List<KmeansData>>();

                    }
                    int i = 0; //Counts number of Samples we have
                    for (int j = 0; j < numofclstrs; j++)//Runs for numberofclusters defined earlier
                    {
                        for (int k = 0; k < r[j].ClusterData.Length; k++)// add the data in web Api
                        {
                            if (numberofattributes == 3)// add data for 2D
                                kmeansdata.Value.Add(new KmeansData { ID = i, ClustersDataLength = r[j].ClusterData.Length, TotalCentroid = r.Length, CentroidIdNumber = j, CentroidX = r[j].Centroid[0], CentroidY = r[j].Centroid[1], CentroidZ = r[j].Centroid[2], Xaxis = r[j].ClusterData[k][0], Yaxis = r[j].ClusterData[k][1], Zaxis = r[j].ClusterData[k][2], ClusterDataDistanceToCentroid = r[j].ClusterDataDistanceToCentroid[k], ClusterDataOriginalIndex = r[j].ClusterDataOriginalIndex[k], ClusterOfNearestForeignSample = r[j].ClusterOfNearestForeignSample, DistanceToNearestClusterCentroid = r[j].DistanceToNearestClusterCentroid, DistanceToNearestForeignSample = r[j].DistanceToNearestForeignSample, DistanceToNearestForeignSampleInNearestCluster = r[j].DistanceToNearestForeignSampleInNearestCluster, InClusterFarthestSampleX = r[j].InClusterFarthestSample[0], InClusterFarthestSampleY = r[j].InClusterFarthestSample[1], InClusterFarthestSampleZ = r[j].InClusterFarthestSample[2], InClusterFarthestSampleIndex = r[j].InClusterFarthestSampleIndex, InClusterMaxDistance = r[j].InClusterMaxDistance, MeanX = r[j].Mean[0], MeanY = r[j].Mean[1], MeanZ = r[j].Mean[2], NearestCluster = r[j].NearestCluster, NearestForeignSampleX = r[j].NearestForeignSample[0], NearestForeignSampleY = r[j].NearestForeignSample[1], NearestForeignSampleZ = r[j].NearestForeignSample[2], NearestForeignSampleInNearestClusterX = r[j].NearestForeignSampleInNearestCluster[0], NearestForeignSampleInNearestClusterY = r[j].NearestForeignSampleInNearestCluster[1], NearestForeignSampleInNearestClusterZ = r[j].NearestForeignSampleInNearestCluster[2], NumberOfAttribs = numberofattributes });
                            if (numberofattributes == 2)// add data for 3D
                                kmeansdata.Value.Add(new KmeansData { ID = i, ClustersDataLength = r[j].ClusterData.Length, TotalCentroid = r.Length, CentroidIdNumber = j, CentroidX = r[j].Centroid[0], CentroidY = r[j].Centroid[1], Xaxis = r[j].ClusterData[k][0], Yaxis = r[j].ClusterData[k][1], ClusterDataDistanceToCentroid = r[j].ClusterDataDistanceToCentroid[k], ClusterDataOriginalIndex = r[j].ClusterDataOriginalIndex[k], ClusterOfNearestForeignSample = r[j].ClusterOfNearestForeignSample, DistanceToNearestClusterCentroid = r[j].DistanceToNearestClusterCentroid, DistanceToNearestForeignSample = r[j].DistanceToNearestForeignSample, DistanceToNearestForeignSampleInNearestCluster = r[j].DistanceToNearestForeignSampleInNearestCluster, InClusterFarthestSampleX = r[j].InClusterFarthestSample[0], InClusterFarthestSampleY = r[j].InClusterFarthestSample[1], InClusterFarthestSampleIndex = r[j].InClusterFarthestSampleIndex, InClusterMaxDistance = r[j].InClusterMaxDistance, MeanX = r[j].Mean[0], MeanY = r[j].Mean[1], NearestCluster = r[j].NearestCluster, NearestForeignSampleX = r[j].NearestForeignSample[0], NearestForeignSampleY = r[j].NearestForeignSample[1], NearestForeignSampleInNearestClusterX = r[j].NearestForeignSampleInNearestCluster[0], NearestForeignSampleInNearestClusterY = r[j].NearestForeignSampleInNearestCluster[1], NumberOfAttribs = numberofattributes });
                            i++;
                        }
                    }

                    //   PgaeLoadFlag++;
                    //}
                    kmeansdata.Value[0].CheckSampleResult = checksample;
                }

         //   }
            else
            {
                numofclstrs = 3;
                int numberofclusters = 3;
            /// desired number of clusters
            int numberofattributes = 3;
            /// X,Y,Z components
            double[][] result = MockApi.MockApi.CSVtoDoubleJaggedArray(HostingEnvironment.MapPath("~/FileUpload/data.csv"));
            /// CSV to Array Conversion
            if (System.IO.File.Exists(HostingEnvironment.MapPath("~/FileUpload/data.csv")))
                numberofattributes = result[0].Length;
            if (numberofattributes == data.ClustersDataLength)
            {
                /// Number of attributes are 3 for 3D and 2 for 2D
                IAnomalyDetectionApi X = new AnomalyDetectionApi.AnomalyDetectionAPI(result, numofclstrs);
                ClusteringSettings Y = new ClusteringSettings(result, 10, numofclstrs, numberofattributes, HostingEnvironment.MapPath("~/FileUpload/cluster.json"));
                /// Set number of clusters, max iterations and give data in result
                X.ImportNewDataForClustering(Y);
                /// CheckSample is a function that detect to which cluster the given sample belongs to.
                ClusteringResults[] r;
                /// All the Kmeans results needed for plotting
                AnomalyDetectionResponse Z = X.GetResults(HostingEnvironment.MapPath("~/FileUpload/cluster.json"), out r);
                
                if (numberofattributes == 3) {
                    double[] sample = new double[3] { data.XaxisCheck, data.YaxisCheck, data.ZaxisCheck };
                    CheckingSampleSettings A = new CheckingSampleSettings(HostingEnvironment.MapPath("~/FileUpload/cluster.json"), sample, 10);
                    int N;
                    Z = X.CheckSample(A, out N);
                    data.CheckSampleResult = Z.Message;
                    kmeansdata.Value[0].CheckSampleResult = Z.Message;
                }
                if (numberofattributes == 2)
                {
                    double[] sample = new double[2] { data.XaxisCheck, data.YaxisCheck };
                    CheckingSampleSettings A = new CheckingSampleSettings(@"F:\ProData\cluster.json", sample, 10);
                    int N;
                    Z = X.CheckSample(A, out N);
                    data.CheckSampleResult = Z.Message;
                    kmeansdata.Value[0].CheckSampleResult = Z.Message;
                }

                
            }
            else {
                kmeansdata.Value[0].CheckSampleResult = "Your Data attributes are invalid ! Please Enter number of Cordinates according to the File Uploaded !";
            }
            }
            //kmeansdata.Value.Add.();
            //data.XaxisCheck = ProductID;
            //kmeansdata.Value.Add(data); //add the post product data to the product list
            //ProductID++;
            //instead of adding product data to the static product list you can save data to the database.
        }


    }
}