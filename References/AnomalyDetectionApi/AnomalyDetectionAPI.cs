using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using  AnomalyDetection.Interface;


namespace AnomalyDetectionApi
{
    /// <summary>
    /// AnomalyDetectionAPI is a class containing basic information about a clustering instance.
    /// </summary>
    [DataContract]
    public class AnomalyDetectionAPI : IAnomalyDetectionApi
    {

        /// <summary>
        /// Constructor for creating AnomalyDetectionAPI object
        /// </summary>
        /// <param name="RawData">data to be clustered</param>
        /// <param name="NumberOfClusters">desired number of clusters</param>
        public AnomalyDetectionAPI(double[][] RawData, int NumberOfClusters)
        {
            this.RawData = RawData;
            this.NumberOfClusters = NumberOfClusters;
        }

        /// <summary>
        /// data to be clustered
        /// </summary>
        [DataMember]
        private double[][] RawData { get; set; }
        /// <summary>
        /// desired number of clusters
        /// </summary>
        [DataMember]
        private int NumberOfClusters { get; set; }
        /// <summary>
        /// the resulting centroids
        /// </summary>
        [DataMember]
        private double[][] Centroids { get; set; }
        /// <summary>
        /// distance between the centroid and the farthest smaple in each cluster
        /// </summary>
        [DataMember]
        private double[] InClusterMaxDistance { get; set; }
        /// <summary>
        /// contains the assigned cluster number for each sample of the RawData
        /// </summary>
        [DataMember]
        private int[] DataToClusterMapping { get; set; }

        //Main functions
        //-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->

        /// <summary>
        /// ImportNewDataForClustering is a function that start a new clustering instance or add to an existing one. It saves the results automatically.
        /// </summary>
        /// <param name="Settings">contains the desired settings for the clustering process</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "Clustering Complete. K-means stopped at the maximum allowed iteration: " + Maximum_Allowed_Iteration </li>
        /// <li> or </li>
        /// <li> - Code: 0, "Clustering Complete. K-means converged at iteration: " + Iteration_Reached </li>
        /// </ul>
        /// </returns>
        public AnomalyDetectionResponse ImportNewDataForClustering(ClusteringSettings Settings)
        {
            try
            {
                string CheckedSavePath, CheckedLoadPath;
                // does some checks on the passed parameters by the user
                AnomalyDetectionResponse ADResponse = PreproccessingOfParameters(Settings.RawData,/* Settings.KmeansAlgorithmImplementation,*/ Settings.KmeansMaxIterations, Settings.NumberOfClusters, Settings.NumberOfAttributes, Settings.SavePath, Settings.LoadPath, Settings.Replace, out CheckedSavePath, out CheckedLoadPath);
                if (ADResponse.Code != 0)
                {
                    return ADResponse;
                }       
                AnomalyDetectionAPI Instance;
                
                //in case of adding to an existing clustering instance
                if (!CheckedLoadPath.Equals(""))
                {
                    Tuple<AnomalyDetectionAPI, AnomalyDetectionResponse> LoadJSON;
                    //load the previous clustering instance
                    LoadJSON = LoadJSON_AnomalyDetectionAPI(CheckedLoadPath);
                    if (LoadJSON.Item2.Code != 0)
                    {
                        return LoadJSON.Item2;
                    }
                    AnomalyDetectionAPI LoadedInstance = LoadJSON.Item1;

                    //some additional checks on the passed parameters by the user
                    if (Settings.NumberOfClusters != LoadedInstance.NumberOfClusters)
                    {
                        return new AnomalyDetectionResponse(112, "Function <ImportNewDataForClustering>: Mismatch between old and new cluster numbers");
                    }
                    if (Settings.NumberOfAttributes != LoadedInstance.RawData[0].Length)
                    {
                        return new AnomalyDetectionResponse(113, "Function <ImportNewDataForClustering>: Mismatch between old and new number of atributes");
                    }
                    
                    Tuple<double[][], AnomalyDetectionResponse> PCSResponse;
                    //get rid of outliers in the new RawData
                    PCSResponse = PrivateCheckSamples(Settings.RawData, LoadedInstance.Centroids, LoadedInstance.InClusterMaxDistance);
                    if (PCSResponse.Item2.Code != 0)
                    {
                        return PCSResponse.Item2;
                    }
                    double[][] AcceptedSamples = PCSResponse.Item1;

                    //concatinate the old data with the accepted data of the new clustering request
                    double[][] RawData = new double[LoadedInstance.RawData.Length + AcceptedSamples.Length][];
                    int PreviousSamplesCount = LoadedInstance.RawData.Length;
                    for (int i = 0; i < PreviousSamplesCount; i++)
                    {
                        RawData[i] = LoadedInstance.RawData[i];
                    }
                    for (int i = 0; i < AcceptedSamples.Length; i++)
                    {
                        RawData[i + PreviousSamplesCount] = AcceptedSamples[i];
                    }
                    Instance = new AnomalyDetectionAPI(RawData, Settings.NumberOfClusters);
                }
                //in case of a new clustering instance
                else
                {
                    Instance = new AnomalyDetectionAPI(Settings.RawData, Settings.NumberOfClusters);
                }

                double[][] Centroids;
                int IterationReached = -1;
                Tuple<int[], AnomalyDetectionResponse> KMeansResponse;
                //initiate the clustering process
                KMeansResponse = KMeansClusteringAlg(Instance.RawData, Instance.NumberOfClusters, Settings.NumberOfAttributes, Settings.KmeansMaxIterations, out Centroids, out IterationReached);
                if (KMeansResponse.Item2.Code != 0)
                {
                    return KMeansResponse.Item2;
                }
                Instance.DataToClusterMapping = KMeansResponse.Item1;
                Instance.Centroids = Centroids;

                Tuple<ClusteringResults[], AnomalyDetectionResponse> CCRResponse;
                //create the clusters' result & statistics
                CCRResponse = ClusteringResults.CreateClusteringResult(Instance.RawData, Instance.DataToClusterMapping, Centroids, Instance.NumberOfClusters);
                if (CCRResponse.Item2.Code != 0)
                {
                    return CCRResponse.Item2;
                }
                ClusteringResults[] Results = CCRResponse.Item1;

                Instance.InClusterMaxDistance = new double[Instance.NumberOfClusters];
                for (int i = 0; i < Instance.NumberOfClusters; i++)
                {
                    Instance.InClusterMaxDistance[i] = Results[i].InClusterMaxDistance;
                }

                //save the clustering instance (in the desired save path)
                ADResponse = SaveToJSON(CheckedSavePath, Instance);
                if (ADResponse.Code != 0)
                {
                    return ADResponse;
                }

                //save the clustering results (in a folder called Result in the desired save path)
                string ResultPath = Path.GetDirectoryName(CheckedSavePath).ToString() + "\\Result\\" + Path.GetFileNameWithoutExtension(CheckedSavePath).ToString() + ".json";
                ADResponse = SaveToJSON(ResultPath, Results);
                if (ADResponse.Code != 0)
                {
                    return ADResponse;
                }

                this.Centroids = Instance.Centroids;
                this.DataToClusterMapping = Instance.DataToClusterMapping;
                this.InClusterMaxDistance = Instance.InClusterMaxDistance;

                if ( Settings.KmeansMaxIterations <= IterationReached)
                {
                    return new AnomalyDetectionResponse(0, "Clustering Complete. K-means stopped at the maximum allowed iteration: " + Settings.KmeansMaxIterations);
                }
                else
                {
                    return new AnomalyDetectionResponse(0, "Clustering Complete. K-means converged at iteration: " + IterationReached);
                }
            } 
            catch (Exception Ex)
            {
                return new AnomalyDetectionResponse(400, "Function <ImportNewDataForClustering>: Unhnadled exception:\t" + Ex.ToString());
            }       
            
        }

        /// <summary>
        /// CheckSample is a function that detects to which cluster the given sample belongs to.
        /// </summary>
        /// <param name="Settings">contains the desired settings for detecting to which, if any, cluster the sample belongs</param>
        /// <param name="ClusterIndex">the cluster number to which the sample belongs (-1 if the sample doesn't belong to any cluster or if an error was encountered).</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "This sample belongs to cluster: " + Cluster_Number </li>
        /// <li> or </li>
        /// <li> - Code: 1, "This sample doesn't belong to any cluster" </li>
        /// </ul>       
        /// </returns>
        public AnomalyDetectionResponse CheckSample(CheckingSampleSettings Settings, out int ClusterIndex)
        {
            try
            {
                //some checks on the passed parameters by the user
                if (Settings.tolerance < 0)
                {
                    ClusterIndex = -1;
                    return new AnomalyDetectionResponse(110, "Function <CheckSample>: Unacceptable tolerance value");
                }
                string CheckedProjectPath;
                AnomalyDetectionResponse ADResponse = CheckPath(Settings.ProjectPath, out CheckedProjectPath);
                if (ADResponse.Code != 0)
                {
                    ClusterIndex = -1;
                    return ADResponse;
                }
                if (!File.Exists(CheckedProjectPath))
                {
                    ClusterIndex = -1;
                    return new AnomalyDetectionResponse(200, "Function <CheckSample>: File not found");
                }
                Tuple<AnomalyDetectionAPI, AnomalyDetectionResponse> LoadJSON;
                //load the clustering project containing the clusters to one of which, if any, the sample will be assigned to
                LoadJSON = LoadJSON_AnomalyDetectionAPI(Settings.ProjectPath);
                if (LoadJSON.Item2.Code != 0)
                {
                    ClusterIndex = -1;
                    return LoadJSON.Item2;
                }
                AnomalyDetectionAPI Project = LoadJSON.Item1;
                //returns error if the new sample has different number of attributes compared to the samples in the desired project
                if (Project.Centroids[0].Length != Settings.Sample.Length)
                {
                    ClusterIndex = -1;
                    return new AnomalyDetectionResponse(114, "Function <CheckSample>: Mismatch in number of attributes");
                }
                double CalculatedDistance;
                double MinDistance = double.MaxValue;
                int ClosestCentroid = -1;
                Tuple<double, AnomalyDetectionResponse> CDResponse;
                //determines to which centroid the sample is closest and the distance
                for (int j = 0; j < Project.NumberOfClusters; j++)
                {
                    CDResponse = CalculateDistance(Settings.Sample, Project.Centroids[j]);
                    if (CDResponse.Item2.Code != 0)
                    {
                        ClusterIndex = -1;
                        return CDResponse.Item2;
                    }
                    CalculatedDistance = CDResponse.Item1;
                    if (CalculatedDistance < MinDistance)
                    {
                        MinDistance = CalculatedDistance;
                        ClosestCentroid = j;
                    }
                }

                //decides based on the maximum distance in the cluster & the tolerance whether the sample really belongs to the cluster or not 
                if (MinDistance < Project.InClusterMaxDistance[ClosestCentroid] * (1.0 + Settings.tolerance / 100.0))
                {
                    ClusterIndex = ClosestCentroid;
                    return new AnomalyDetectionResponse(0, "This sample belongs to cluster: " + ClosestCentroid.ToString());
                }
                else
                {
                    ClusterIndex = -1;
                    return new AnomalyDetectionResponse(1, "This sample doesn't belong to any cluster");
                }
            }
            catch (Exception Ex)
            {
                ClusterIndex = -1;
                return new AnomalyDetectionResponse(400, "Function <CheckSample>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// GetResults is a function that returns the results of an existing clustering instance 
        /// </summary>
        /// <param name="LoadPath">path of the clustering instance</param>
        /// <param name="Result">the variable through which the clustering result are returned</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        public AnomalyDetectionResponse GetResults(string LoadPath, out ClusteringResults[] Result)
        {
            try
            {
                //gets the path of the results instead of the instance
                string ResultPath = Path.GetDirectoryName(LoadPath).ToString() + "\\Result\\" + Path.GetFileNameWithoutExtension(LoadPath).ToString() + ".json";
                Tuple<ClusteringResults[], AnomalyDetectionResponse> LJResponse;
                //load the results
                LJResponse = LoadJSON_ClusteringResults(ResultPath);
                if (LJResponse.Item2.Code != 0)
                {
                    Result = null;
                    return LJResponse.Item2;
                }
                Result = LJResponse.Item1;
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch(Exception Ex)
            {
                Result = null;
                return new AnomalyDetectionResponse(400, "Function <GetResults>: Unhandled exception:\t" + Ex.ToString());
            }
        }   

        /// <summary>
        /// GetPreviousSamples is a function that loads samples from a previous clustering instance
        /// </summary>
        /// <param name="LoadPath">path of the clustering instance</param>
        /// <param name="OldSamples">the variable through which the samples are returned</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        public AnomalyDetectionResponse GetPreviousSamples(string LoadPath, out double[][] OldSamples)
        {
            try
            {
                Tuple<AnomalyDetectionAPI, AnomalyDetectionResponse> LJResponse;
                //load the clustering instance
                LJResponse = LoadJSON_AnomalyDetectionAPI(LoadPath);
                if (LJResponse.Item2.Code != 0)
                {
                    OldSamples = null;
                    return LJResponse.Item2;
                }
                OldSamples = LJResponse.Item1.RawData;
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch (Exception Ex)
            {
                OldSamples = null;
                return new AnomalyDetectionResponse(400, "Function <GetPreviousSamples>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// RecommendedNumberOfClusters is a function that returns a recommended number of clusters for the given samples.
        /// </summary>
        /// <param name="RawData">the samples to be clustered</param>
        /// <param name="KmeansMaxIterations">maximum allowed number of Kmeans iteration for clustering</param>
        /// <param name="NumberOfAttributes">number of attributes for each sample</param>
        /// <param name="MaxNumberOfClusters">maximum desired number of clusters</param>
        /// <param name="MinNumberOfClusters">minimum desired number of clusters</param>
        /// <param name="Method">integer 0,1 or 2 representing the method to be used. 
        /// <ul style = "list-style-type:none" >
        /// <li> - Method 0: Radial method in which the farthest sample of each cluster must be closer to the cluster centoid than the nearest foreign sample of the other clusters </li>
        /// <li> - Method 1: Standard Deviation method in which the standard deviation in each cluster must be less than the desired standard deviation </li>
        /// <li> - Method 2: Both. uses radial and standard deviation methods at the same time </li>
        /// </ul>
        /// </param>
        /// <param name="StdDev">the desired standard deviation upper limit in each cluster</param>
        /// <param name="RecNumberOfClusters">the variable through which the recommended number of clusters is returned</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style = "list-style-type:none" >
        /// <li> - Code: 0, "OK" </li>
        /// <li> or </li>
        /// <li> - Code: 1, "Could not find a recommended number of clusters based on the desired constraints" </li>
        /// </ul>
        /// </returns>
        public AnomalyDetectionResponse RecommendedNumberOfClusters(double[][] RawData, int KmeansMaxIterations, int NumberOfAttributes, int MaxNumberOfClusters, int MinNumberOfClusters, int Method, double[] StdDev, out int RecNumberOfClusters)
        {
            try
            {
                //some checks
                if (MaxNumberOfClusters < 2)
                {
                    RecNumberOfClusters = 0;
                    return new AnomalyDetectionResponse(104, "Function <RecommendedNumberOfClusters>: Maximum number of clusters must be at least 2");
                }
                int MaxClusters = Math.Min(RawData.Length, MaxNumberOfClusters);

                if (MinNumberOfClusters < 2)
                {
                    MinNumberOfClusters = 2;
                }
                if (Method > 2 || Method < 0)
                {
                    RecNumberOfClusters = 0;
                    return new AnomalyDetectionResponse(122, "Function <RecommendedNumberOfClusters>: Method must be either 0,1 or 2");
                }
                if (Method != 0 && StdDev == null)
                {
                    RecNumberOfClusters = 0;
                    return new AnomalyDetectionResponse(123, "Function <RecommendedNumberOfClusters>: Parameter StdDev is needed");
                }
                if (KmeansMaxIterations < 1)
                {
                    RecNumberOfClusters = 0;
                    return new AnomalyDetectionResponse(108, "Function <RecommendedNumberOfClusters>: Unacceptable number of maximum iterations");
                }
                if (RawData == null)
                {
                    RecNumberOfClusters = 0;
                    return new AnomalyDetectionResponse(100, "Function <RecommendedNumberOfClusters>: RawData is null");
                }
                if (NumberOfAttributes < 1)
                {
                    RecNumberOfClusters = 0;
                    return new AnomalyDetectionResponse(107, "Function <RecommendedNumberOfClusters>: Unacceptable number of attributes. Must be at least 1");
                }
                //checks that all the samples have same number of attributes
                AnomalyDetectionResponse ADResponse = VerifyRawDataConsistency(RawData, NumberOfAttributes);
                if (ADResponse.Code != 0)
                {
                    RecNumberOfClusters = 0;
                    return ADResponse;
                }

                double[][] Centroids;
                int IterationReached = -1;
                Tuple<int[], AnomalyDetectionResponse> KMeansResponse;
                Tuple<ClusteringResults[], AnomalyDetectionResponse> CCRResponse;
                ClusteringResults[] Results;
                bool RadialCheck, StdDevCheck;
                Tuple<bool, AnomalyDetectionResponse> BoolChecks;

                for (int i = MinNumberOfClusters; i <= MaxClusters; i++)
                {
                    //cluster the data with number of clusters equals to i
                    KMeansResponse = KMeansClusteringAlg(RawData, i, NumberOfAttributes, KmeansMaxIterations, out Centroids, out IterationReached);
                    if (KMeansResponse.Item2.Code != 0)
                    {
                        RecNumberOfClusters = 0;
                        return KMeansResponse.Item2;
                    }
                    CCRResponse = ClusteringResults.CreateClusteringResult(RawData, KMeansResponse.Item1, Centroids, i);
                    if (CCRResponse.Item2.Code != 0)
                    {
                        RecNumberOfClusters = 0;
                        return CCRResponse.Item2;
                    }
                    Results = CCRResponse.Item1;

                    RadialCheck = true;
                    StdDevCheck = true;
                    if (Method != 1)
                    {
                        //radial method check
                        BoolChecks = RadialClustersCheck(Results);
                        if (BoolChecks.Item2.Code != 0)
                        {
                            RecNumberOfClusters = 0;
                            return BoolChecks.Item2;
                        }
                        RadialCheck = BoolChecks.Item1;
                    }
                    if (Method != 0)
                    {
                        //standard deviation check
                        BoolChecks =StdDeviationClustersCheck(Results, StdDev);
                        if (BoolChecks.Item2.Code != 0)
                        {
                            RecNumberOfClusters = 0;
                            return BoolChecks.Item2;
                        }
                        StdDevCheck = BoolChecks.Item1;
                    }
                    if (RadialCheck && StdDevCheck == true)
                    {
                        RecNumberOfClusters = i;
                        return new AnomalyDetectionResponse(0, "OK");
                    }
                }
                RecNumberOfClusters = 0;
                return new AnomalyDetectionResponse(1, "Could not find a recommended number of clusters based on the desired constraints");
            }
            catch (Exception Ex)
            {
                RecNumberOfClusters = 0;
                return new AnomalyDetectionResponse(400, "Function <RecommendedNumberOfClusters>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        //-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->


        //Supporting functions
        //-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->

        //Kmeans functions-->-->-->-->

        /// <summary>
        /// KMeansClusteringAlg is a function that clusters the given samples based on the K-means concept.
        /// </summary>
        /// <param name="rawData">the samples to be clustered</param>
        /// <param name="numClusters">desired number of clusters</param>
        /// <param name="numAttributes">number of attributes for each sample</param>
        /// <param name="maxCount">maximum allowed number of Kmeans iteration for clustering</param>
        /// <param name="centroids">the variable through which the resulting centroids are returned</param>
        /// <param name="IterationReached">the variable through which the iteration reached is returned</param>
        /// <returns>Tuple of two Items: <br />
        /// - Item 1: contains the assigned cluster number for each sample of the RawData <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return: 
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<int[], AnomalyDetectionResponse> KMeansClusteringAlg(double[][] rawData, int numClusters, int numAttributes, int maxCount, out double[][] centroids, out int IterationReached)
        {
            int[] clustering;
            try
            {
                bool changed = true;
                int cnt = 0;
                Tuple<int[], AnomalyDetectionResponse> ICResponse;
                Tuple<double[][], AnomalyDetectionResponse> AllocateResponse;
                Tuple<bool, AnomalyDetectionResponse> AssignResponse;
                AnomalyDetectionResponse ADResponse;

                int numTuples = rawData.Length;
                // 0 is a seed for random
                ICResponse = InitClustering(numTuples, numClusters, 0);  
                if (ICResponse.Item2.Code != 0)
                {
                    centroids = null;
                    IterationReached = -1;
                    clustering = null; 
                    return Tuple.Create(clustering,ICResponse.Item2);
                }
                clustering = ICResponse.Item1;
                // just makes things a bit cleaner
                AllocateResponse = Allocate(numClusters, numAttributes);       
                if (AllocateResponse.Item2.Code != 0)
                {
                    centroids = null;
                    IterationReached = -1;
                    clustering = null;
                    return Tuple.Create(clustering, AllocateResponse.Item2);
                }
                double[][] means = AllocateResponse.Item1;
                AllocateResponse = Allocate(numClusters, numAttributes);
                if (AllocateResponse.Item2.Code != 0)
                {
                    centroids = null;
                    IterationReached = -1;
                    clustering = null;
                    return Tuple.Create(clustering, AllocateResponse.Item2);
                }
                centroids = AllocateResponse.Item1;
                // could call this instead inside UpdateCentroids
                ADResponse = UpdateMeans(rawData, clustering, means);                       
                if (ADResponse.Code != 0)
                {
                    centroids = null;
                    IterationReached = -1;
                    clustering = null;
                    return Tuple.Create(clustering, ADResponse);
                }
                ADResponse = UpdateCentroids(rawData, clustering, means, centroids);
                if (ADResponse.Code != 0)
                {
                    centroids = null;
                    IterationReached = -1;
                    clustering = null;
                    return Tuple.Create(clustering, ADResponse);
                }

                while (changed == true && cnt < maxCount)
                {
                    ++cnt;
                    // use centroids to update cluster assignment
                    AssignResponse = Assign(rawData, clustering, centroids); 
                    if (AssignResponse.Item2.Code != 0)
                    {
                        centroids = null;
                        IterationReached = -1;
                        clustering = null;
                        return Tuple.Create(clustering, AssignResponse.Item2);
                    }
                    changed = AssignResponse.Item1;
                    // use new clustering to update cluster means
                    ADResponse = UpdateMeans(rawData, clustering, means); 
                    if (ADResponse.Code != 0)
                    {
                        centroids = null;
                        IterationReached = -1;
                        clustering = null;
                        return Tuple.Create(clustering, ADResponse);
                    }
                    // use new means to update centroids
                    ADResponse = UpdateCentroids(rawData, clustering, means, centroids);  
                    if (ADResponse.Code != 0)
                    {
                        centroids = null;
                        IterationReached = -1;
                        clustering = null;
                        return Tuple.Create(clustering, ADResponse);
                    }
                }
                
                IterationReached = cnt;
                return Tuple.Create(clustering, new AnomalyDetectionResponse(0,"OK"));
            }
            catch(Exception Ex)
            {
                centroids = null;
                IterationReached = -1;
                clustering = null;
                return Tuple.Create(clustering, new AnomalyDetectionResponse(400, "Fuction <KMeansClusteringAlg>: Unhandled exception:\t" + Ex.ToString()));
            }
        }

        /// <summary> 
        /// InitClustering is a function that assigns a sample to each cluster and then randomly assigns the remaining samples on all clusters.
        /// </summary>
        /// <param name="numTuples">number of samples</param>
        /// <param name="numClusters">number of clusters</param>
        /// <param name="randomSeed">random seed for randomly assigning the samples to the clusters</param>
        /// <returns>Tuple of two Items: <br />
        /// - Item 1: contains the assigned cluster number for each sample of the RawData <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<int[],AnomalyDetectionResponse> InitClustering(int numTuples, int numClusters, int randomSeed)
        {
            int[] clustering;
            AnomalyDetectionResponse ADResponse;
            try
            {
                if (numClusters < 2)
                {
                    clustering = null;
                    ADResponse = new AnomalyDetectionResponse(106, "Function <InitClustering>: Unacceptable number of clusters. Must be at least 2");
                    return Tuple.Create(clustering, ADResponse);
                }
                if (numTuples < numClusters)
                {
                    clustering = null;
                    ADResponse = new AnomalyDetectionResponse(105, "Function <InitClustering>: Unacceptable number of clusters. Clusters more than samples");
                    return Tuple.Create(clustering, ADResponse);
                }
                // assign each tuple to a random cluster, making sure that there's at least
                // one tuple assigned to every cluster
                Random random = new Random(randomSeed);
                clustering = new int[numTuples];

                // assign first numClusters tuples to clusters 0..k-1
                for (int i = 0; i < numClusters; ++i)
                    clustering[i] = i;
                // assign rest randomly
                for (int i = numClusters; i < clustering.Length; ++i)
                    clustering[i] = random.Next(0, numClusters);

                ADResponse = new AnomalyDetectionResponse(0, "OK");
                return Tuple.Create(clustering, ADResponse);
            }
            catch (Exception Ex)
            {
                clustering = null;
                ADResponse = new AnomalyDetectionResponse(400, "Function <InitClustering>: Unhandled exception:\t" + Ex.ToString());
                return Tuple.Create(clustering, ADResponse);
            }
        }

        /// <summary>
        /// Allocate is a function that creates a double[][] with the specified size (number of clusters x number of attributes).
        /// </summary>
        /// <param name="numClusters">number of clusters</param>
        /// <param name="numAttributes">number of attributes</param>
        /// <returns>Tuple of two Items: <br />
        /// - Item 1: the allocated double[][] <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<double[][], AnomalyDetectionResponse> Allocate(int numClusters, int numAttributes)
        {
            double[][] result;
            AnomalyDetectionResponse ADResponse;
            try
            {
                // helper allocater for means[][] and centroids[][]
                if (numClusters < 2)
                {
                    result = null;
                    ADResponse = new AnomalyDetectionResponse(106, "Function <Allocate>: Unacceptable number of clusters. Must be at least 2");
                    return Tuple.Create(result, ADResponse);
                }
                if (numAttributes < 1)
                {
                    result = null;
                    ADResponse = new AnomalyDetectionResponse(107, "Function <Allocate>: Unacceptable number of attributes. Must be at least 1");
                    return Tuple.Create(result, ADResponse);
                }

                result = new double[numClusters][];
                for (int k = 0; k < numClusters; ++k)
                    result[k] = new double[numAttributes];

                ADResponse = new AnomalyDetectionResponse(0, "OK");
                return Tuple.Create(result, ADResponse);
            }
            catch(Exception Ex)
            {
                result = null;
                ADResponse = new AnomalyDetectionResponse(400, "Function <Allocate>: Unhandled excepttion:\t" + Ex.ToString());
                return Tuple.Create(result, ADResponse);
            }
        }

        /// <summary>
        /// UpdateMeans is a function that calculates the new mean of each cluster.
        /// </summary>
        /// <param name="rawData">the samples to be clustered</param>
        /// <param name="clustering">contains the assigned cluster number for each sample of the RawData</param>
        /// <param name="means">mean of each cluster (Updated in the function)</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static AnomalyDetectionResponse UpdateMeans(double[][] rawData, int[] clustering, double[][] means)
        {
            try
            {
                if (rawData == null || rawData.Length < 1)
                {
                    return new AnomalyDetectionResponse(102, "Function <UpdateMeans>: RawData is empty");
                }
                if (means == null || means.Length < 1)
                {
                    return new AnomalyDetectionResponse(103, "Function <UpdateMeans>: Means is empty");
                }
                // assumes means[][] exists. consider making means[][] a ref parameter
                int numClusters = means.Length;

                // zero-out means[][]
                for (int k = 0; k < means.Length; ++k)
                {
                    for (int j = 0; j < means[0].Length; ++j)
                        means[k][j] = 0.0;
                }


                // make an array to hold cluster counts
                int[] clusterCounts = new int[numClusters];

                // walk through each tuple, accumulate sum for each attribute, update cluster count
                for (int i = 0; i < rawData.Length; ++i)
                {
                    int cluster = clustering[i];
                    ++clusterCounts[cluster];

                    for (int j = 0; j < rawData[i].Length; ++j)
                        means[cluster][j] += rawData[i][j];
                }

                // divide each attribute sum by cluster count to get average (mean)
                for (int k = 0; k < means.Length; ++k)
                {
                    if (clusterCounts[k] == 0)
                    {
                        continue;
                    }
                    for (int j = 0; j < means[k].Length; ++j)
                    {
                        means[k][j] /= clusterCounts[k];
                    }
                }

                return new AnomalyDetectionResponse(0, "OK");
            }
            catch (Exception Ex)
            {
                return new AnomalyDetectionResponse(400, "Function <UpdateMeans>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// UpdateCentroids is a function that assigns the nearest sample to each mean as the centroid of a cluster.
        /// </summary>
        /// <param name="rawData">the samples to be clustered</param>
        /// <param name="clustering">contains the assigned cluster number for each sample of the RawData</param>
        /// <param name="means">mean of each cluster</param>
        /// <param name="centroids">centroid of each cluster (Updated in the function)</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static AnomalyDetectionResponse UpdateCentroids(double[][] rawData, int[] clustering, double[][] means, double[][] centroids)
        {
            try
            {
                Tuple<double[], AnomalyDetectionResponse> CCResponse;
                // updates all centroids by calling helper that updates one centroid
                for (int k = 0; k < centroids.Length; ++k)
                {
                    CCResponse = ComputeCentroid(rawData, clustering, k, means);
                    if (CCResponse.Item2.Code != 0)
                    {
                        return CCResponse.Item2;
                    }
                    double[] centroid = CCResponse.Item1;
                    centroids[k] = centroid;
                }
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch(Exception EX)
            {
                return new AnomalyDetectionResponse(400, "Function<UpdateCentroids>: Unhandled exception:\t" + EX.ToString());
            }
        }

        /// <summary>
        /// ComputeCentroid is a function that assigns the nearest sample to the mean as the centroid of a cluster.
        /// </summary>
        /// <param name="rawData">the samples to be clustered</param>
        /// <param name="clustering">contains the assigned cluster number for each sample of the RawData</param>
        /// <param name="cluster">number of the cluster</param>
        /// <param name="means">mean of each cluster</param>
        /// <returns>Tuple of two Items: <br />
        /// - Item 1: the centroid <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns> <br />
        private static Tuple <double[],AnomalyDetectionResponse> ComputeCentroid(double[][] rawData, int[] clustering, int cluster, double[][] means)
        {
            double[] centroid;
            try
            {
                // the centroid is the actual tuple values that are closest to the cluster mean
                int numAttributes = means[0].Length;
                centroid = new double[numAttributes];
                double minDist = double.MaxValue;
                Tuple<double, AnomalyDetectionResponse> CDResponse;
                for (int i = 0; i < rawData.Length; ++i) // walk thru each data tuple
                {
                    int c = clustering[i];  // if current tuple isn't in the cluster we're computing for, continue on
                    if (c != cluster) continue;

                    CDResponse = CalculateDistance(rawData[i], means[cluster]);  // call helper
                    if (CDResponse.Item2.Code != 0)
                    {
                        centroid = null;
                        return Tuple.Create(centroid, CDResponse.Item2);
                    }
                    double currDist = CDResponse.Item1;
                    if (currDist < minDist)
                    {
                        minDist = currDist;
                        for (int j = 0; j < centroid.Length; ++j)
                            centroid[j] = rawData[i][j];
                    }
                }
                return Tuple.Create(centroid, new AnomalyDetectionResponse(0, "OK"));
            }
            catch(Exception Ex)
            {
                centroid = null;
                return Tuple.Create(centroid, new AnomalyDetectionResponse(400, "Function<ComputeCentroid>: Unhandled exception:\t" + Ex.ToString()));
            }

        }

        /// <summary>
        /// Assign is a function that assigns each sample to the nearest clusters' centroids. If the new assignment is the same as the older one, it returns true else it will return false.
        /// </summary>
        /// <param name="rawData">the samples to be clustered</param>
        /// <param name="clustering">contains the assigned cluster number for each sample of the RawData</param>
        /// <param name="centroids">centroid of each cluster</param>
        /// <returns>Tuple of two Items: <br />
        /// - Item 1: true if new assignment is the same as the old one, else false. <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<bool, AnomalyDetectionResponse> Assign(double[][] rawData, int[] clustering, double[][] centroids)
        {
            try
            {
                // assign each tuple to best cluster (closest to cluster centroid)
                // return true if any new cluster assignment is different from old/curr cluster
                // does not prevent a state where a cluster has no tuples assigned. see article for details
                int numClusters = centroids.Length;
                bool changed = false;
                int changedClusters = 0;
                Tuple<double, AnomalyDetectionResponse> CDResponse;
                Tuple<int, AnomalyDetectionResponse> MIResponse;

                double[] distances = new double[numClusters]; // distance from current tuple to each cluster mean
                for (int i = 0; i < rawData.Length; ++i)      // walk thru each tuple
                {
                    for (int k = 0; k < numClusters; ++k)       // compute distances to all centroids
                    {
                        CDResponse = CalculateDistance(rawData[i], centroids[k]);
                        if (CDResponse.Item2.Code != 0)
                        {
                            return Tuple.Create(false, CDResponse.Item2);
                        }
                        distances[k] = CDResponse.Item1;
                    }
                    // distances[k] = Distance(rawData[i], centroids[k]);

                    MIResponse = MinIndex(distances);  // find the index == custerID of closest 
                    if (MIResponse.Item2.Code != 0)
                    {
                        return Tuple.Create(false, MIResponse.Item2);
                    }
                    int newCluster = MIResponse.Item1;
                    if (newCluster != clustering[i]) // different cluster assignment?
                    {
                        changed = true;
                        clustering[i] = newCluster;
                        changedClusters++;
                    } // else no change              
                }

                //Console.WriteLine("Changed clusters {0}", changedClusters);
                return Tuple.Create(changed, new AnomalyDetectionResponse(0, "OK")); // was there any change in clustering?
            }
            catch(Exception Ex)
            {
                return Tuple.Create(false, new AnomalyDetectionResponse(400, "Function<Assign>: Unhandled exception:\t" + Ex.ToString()));
            }
        } // Assign

        /// <summary>
        /// MinIndex is a function that returns the index of the smallest distance between a set of distances.
        /// </summary>
        /// <param name="distances">distance between each sample and the centroid</param>
        /// <returns>
        /// - Item 1: index of the smallest distance <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<int, AnomalyDetectionResponse> MinIndex(double[] distances)
        {
            try
            {
                // index of smallest value in distances[]
                int indexOfMin = 0;
                double smallDist = distances[0];
                for (int k = 0; k < distances.Length; ++k)
                {
                    if (distances[k] < smallDist)
                    {
                        smallDist = distances[k];
                        indexOfMin = k;
                    }
                }
                return Tuple.Create(indexOfMin,new AnomalyDetectionResponse(0,"OK"));
            }
            catch (Exception Ex)
            {
                return Tuple.Create(0, new AnomalyDetectionResponse(400, "Function <MinIndex>: Unhandled exception:\t" + Ex.ToString()));
            }
        }

        //Kmeans functions-->-->-->-->

        //Other functions

        /// <summary>
        /// PreproccessingOfParameters is a function that does some checks on the passed parameters by the user. Some errors in the paths can be corrected.
        /// </summary>
        /// <param name="RawData">the samples to be clustered</param>
        /// <param name="KmeansMaxIterations">maximum allowed number of Kmeans iteration for clustering</param>
        /// <param name="NumberOfClusters">desired number of clusters</param>
        /// <param name="NumberOfAttributes">number of attributes for each sample</param>
        /// <param name="SavePath">path to save the clustering instance</param>
        /// <param name="LoadPath">path to load a clustering instance. can be "" or null in case of not loading</param>
        /// <param name="Replace">a bool where if it is true overwrite can happen</param>
        /// <param name="CheckedSavePath">the variable through which the error-less save path is returned</param>
        /// <param name="CheckedLoadPath">the variable through which the error-less load path is returned</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static AnomalyDetectionResponse PreproccessingOfParameters(double[][] RawData,/* int KmeansAlgorithmImplementation,*/ int KmeansMaxIterations, int NumberOfClusters, int NumberOfAttributes, string SavePath, string LoadPath, bool Replace, out string CheckedSavePath, out string CheckedLoadPath)
        {
            try
            {
                if (KmeansMaxIterations < 1)
                {
                    CheckedSavePath = "";
                    CheckedLoadPath = "";
                    return new AnomalyDetectionResponse(108, "Function <PreproccessingOfParameters>: Unacceptable number of maximum iterations");
                }
                if (NumberOfClusters < 2)
                {
                    CheckedSavePath = "";
                    CheckedLoadPath = "";
                    return new AnomalyDetectionResponse(106, "Function <PreproccessingOfParameters>: Unacceptable number of clusters. Must be at least 2");
                }
                if (RawData != null)
                {
                    if (NumberOfClusters > RawData.Length)
                    {
                        CheckedSavePath = "";
                        CheckedLoadPath = "";
                        return new AnomalyDetectionResponse(105, "Function <PreproccessingOfParameters>: Unacceptable number of clusters. Clusters more than samples");
                    }
                }
                else
                {
                    CheckedSavePath = "";
                    CheckedLoadPath = "";
                    return new AnomalyDetectionResponse(100, "Function <PreproccessingOfParameters>: RawData is null");
                }
                if (NumberOfAttributes < 1)
                {
                    CheckedSavePath = "";
                    CheckedLoadPath = "";
                    return new AnomalyDetectionResponse(107, "Function <PreproccessingOfParameters>: Unacceptable number of attributes. Must be at least 1");
                }
                if (SavePath.Equals(null) || SavePath.Equals(""))
                {
                    CheckedSavePath = "";
                    CheckedLoadPath = "";
                    return new AnomalyDetectionResponse(109, "Function <PreproccessingOfParameters>: Unacceptable savepath");
                }

                //Check Paths 
                string OutputPath;
                AnomalyDetectionResponse ADResponse = CheckPath(SavePath, out OutputPath);
                if (ADResponse.Code == 0)
                {               
                    if (File.Exists(OutputPath))
                    {
                        if ((!Replace))
                        {
                            CheckedSavePath = "";
                            CheckedLoadPath = "";
                            return new AnomalyDetectionResponse(201, "Function <PreproccessingOfParameters>: File already exists");
                        }
                        FileAttributes Attributes = File.GetAttributes(OutputPath);
                        if ((Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            CheckedSavePath = "";
                            CheckedLoadPath = "";
                            return new AnomalyDetectionResponse(204, "Function <PreproccessingOfParameters>: Unauthorized access to file. File is readonly.");
                        }
                        if ((Attributes & FileAttributes.System) == FileAttributes.System)
                        {
                            CheckedSavePath = "";
                            CheckedLoadPath = "";
                            return new AnomalyDetectionResponse(205, "Function <PreproccessingOfParameters>: Unauthorized access to file. File is a system file.");
                        }
                    }
                    CheckedSavePath = OutputPath;
                }
                else
                {
                    CheckedSavePath = "";
                    CheckedLoadPath = "";
                    return ADResponse;
                }



                if (!LoadPath.Equals(null) && !LoadPath.Equals(""))
                {
                    ADResponse = CheckPath(LoadPath, out OutputPath);
                    if (ADResponse.Code == 0)
                    {
                        if (!File.Exists(OutputPath))
                        {
                            CheckedSavePath = "";
                            CheckedLoadPath = "";
                            if (!Directory.Exists(Path.GetDirectoryName(OutputPath)))
                            {
                                return new AnomalyDetectionResponse(121, "Function <PreproccessingOfParameters>: Requested load path does not exist");
                            }
                            return new AnomalyDetectionResponse(200, "Function <PreproccessingOfParameters>: File not found");
                        }
                        CheckedLoadPath = OutputPath;
                    }
                    else
                    {
                        CheckedSavePath = "";
                        CheckedLoadPath = "";
                        return ADResponse;
                    }
                }
                else
                {
                    CheckedLoadPath = "";
                }

                ADResponse = VerifyRawDataConsistency(RawData, NumberOfAttributes);
                if (ADResponse.Code != 0)
                {
                    CheckedSavePath = "";
                    CheckedLoadPath = "";
                    return ADResponse;
                }

                return new AnomalyDetectionResponse(0, "OK");
            }
            catch(Exception Ex)
            {
                CheckedSavePath = "";
                CheckedLoadPath = "";
                return new AnomalyDetectionResponse(400, "Function <PreproccessingOfParameters>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// CheckPath is a function that checks the input path for errors. Some errors can be corrected.
        /// </summary>
        /// <param name="InputPath">path to be checked</param>
        /// <param name="OutputPath">the variable through which the error-less path is returned</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static AnomalyDetectionResponse CheckPath(string InputPath, out string OutputPath)
        {
            try
            {
                OutputPath = null;

                if (!Path.IsPathRooted(InputPath))
                {
                    return new AnomalyDetectionResponse(116, "Function<CheckPath>: Path provided : \"" + InputPath + "\" has no root");
                }

                //check directory issues
                char[] InvalidChars;
                string DirectoryPath = Path.GetDirectoryName(InputPath);

                if (DirectoryPath != null)
                {
                    InvalidChars = Path.GetInvalidPathChars();
                    for (int i = 0; i < InvalidChars.Length; i++)
                    {
                        if (DirectoryPath.Contains(InvalidChars[i]))
                        {
                            return new AnomalyDetectionResponse(117, "Function <CheckPath>: Path provided : \"" + InputPath + "\" contains invalid chars. First invalid char encountered is: \'" + InvalidChars[i] + "\'.");
                        }
                    }
                }

                InvalidChars = Path.GetInvalidFileNameChars();
                string SaveName = Path.GetFileNameWithoutExtension(InputPath);
                if (SaveName == "")
                {
                    return new AnomalyDetectionResponse(118, "Function <CheckPath>: Path provided : \"" + InputPath + "\" has no project name specified.");
                }
                for (int i = 0; i < InvalidChars.Length; i++)
                {
                    if (SaveName.Contains(InvalidChars[i]))
                    {
                        return new AnomalyDetectionResponse(119, "Function <CheckPath>: Path provided : \"" + InputPath + "\" has a project name containing invalid chars. First invalid char encountered is: \'" + InvalidChars[i] + "\'.");
                    }
                }

                if (Path.HasExtension(InputPath))
                {
                    if (!Path.GetExtension(InputPath).ToLower().Equals(".json"))
                    {
                        return new AnomalyDetectionResponse(120, "Function <CheckPath>: Path provided : \"" + InputPath + "\" has wrong extension.");
                    }
                }

                string RootPath = Path.GetPathRoot(InputPath);
                if (!RootPath.Contains("\\"))
                {
                    DirectoryPath = DirectoryPath.TrimStart(RootPath.ToCharArray());
                    DirectoryPath = RootPath + "\\" + DirectoryPath;
                }
                OutputPath = DirectoryPath + "\\" + SaveName + ".json";
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch(Exception Ex)
            {
                OutputPath = null;
                return new AnomalyDetectionResponse(400, "Function <CheckPath>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// VerifyRawDataConsistency is a function that checks that all the samples have same given number of attributes.
        /// </summary>
        /// <param name="RawData">the samples to be clustered</param>
        /// <param name="NumberOfAttributes">number of attributes for each sample</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static AnomalyDetectionResponse VerifyRawDataConsistency(double[][] RawData, int NumberOfAttributes)
        {
            try
            {
                if (RawData == null)
                {
                    return new AnomalyDetectionResponse(100, "Function <VerifyRawDataConsistency>: RawData is null");
                }
                if (RawData.Length < 1)
                {
                    return new AnomalyDetectionResponse(102, "Function <VerifyRawDataConsistency>: RawData is empty");
                }
                int DataLength = RawData.Length;
                for (int i = 0; i < DataLength; i++)
                {
                    if (RawData[i] == null || RawData[i].Length != NumberOfAttributes)
                    {
                        return new AnomalyDetectionResponse(111, "Function <VerifyRawDataConsistency>: Data sample and number of attributes are inconsistent. First encountered inconsistency in data sample: " + i + ".");
                    }
                }
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch (Exception Ex)
            {
                return new AnomalyDetectionResponse(400, "Function <VerifyRawDataConsistency>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// PrivateCheckSamples is a function that remove the outliers from the given samples and returns the accepted samples only.
        /// </summary>
        /// <param name="RawData">the samples to be clustered</param>
        /// <param name="Centroids">centroid of each cluster</param>
        /// <param name="InClusterMaxDistance">distance between the centroid and the farthest sample of each cluster</param>
        /// <returns>
        /// - Item 1: the accepted samples (outliers removed) <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<double[][], AnomalyDetectionResponse> PrivateCheckSamples(double[][] RawData, double[][] Centroids, double[] InClusterMaxDistance)
        {
            double[][] AcceptedSamples;
            AnomalyDetectionResponse ADResponse;
            try
            {
                int AcceptedSamplesCount = 0;
                int NumberOfSamples = RawData.Length;
                int NumberOfClusters = Centroids.Length;
                double MinDistance, CalculatedDistance;
                int ClosestCentroid;
                Tuple<double, AnomalyDetectionResponse> CDResponse;
                //allocate space for storing accepted samples
                double[][] Temp = new double[NumberOfSamples][];
                for (int i = 0; i < NumberOfSamples; i++)
                {
                    MinDistance = double.MaxValue;
                    ClosestCentroid = -1;
                    //check which centroid is closer to the sample
                    for (int j = 0; j < NumberOfClusters; j++)
                    {
                        CDResponse = CalculateDistance(RawData[i], Centroids[j]);
                        if (CDResponse.Item2.Code != 0)
                        {
                            AcceptedSamples = null;
                            return Tuple.Create(AcceptedSamples, CDResponse.Item2);
                        }
                        CalculatedDistance = CDResponse.Item1;
                        if (CalculatedDistance < MinDistance)
                        {
                            MinDistance = CalculatedDistance;
                            ClosestCentroid = j;
                        }
                    }
                    //accept sample if it is closer than  the farthest sample to the centroid else ignore it
                    if (MinDistance < InClusterMaxDistance[ClosestCentroid])
                    {
                        Temp[AcceptedSamplesCount] = RawData[i];
                        AcceptedSamplesCount++;
                    }
                }
                ADResponse = new AnomalyDetectionResponse(0, "OK");
                if (AcceptedSamplesCount < NumberOfSamples)
                {
                    //remove empty rows from Temp
                    AcceptedSamples = new double[AcceptedSamplesCount][];
                    for (int i = 0; i < AcceptedSamplesCount; i++)
                    {
                        AcceptedSamples[i] = Temp[i];
                    }
                    return Tuple.Create(AcceptedSamples, ADResponse);
                }
                return Tuple.Create(Temp, ADResponse);
            }
            catch (Exception Ex)
            {
                AcceptedSamples = null;
                ADResponse = new AnomalyDetectionResponse(400, "Function <PrivateCheckSamples>: Unhandled exception:\t" + Ex.ToString());
                return Tuple.Create(AcceptedSamples, ADResponse);
            }
        }

        /// <summary>
        /// CalculateDistance is a function that claculates the distance between two elements of same size.
        /// </summary>
        /// <param name="FirstElement">first element</param>
        /// <param name="SecondElement">second element</param>
        /// <returns>
        /// - Item 1: distance between the two elements <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        internal static Tuple<double, AnomalyDetectionResponse> CalculateDistance(double[] FirstElement, double[] SecondElement)
        {
            AnomalyDetectionResponse ADResponse;
            try
            {
                if (FirstElement == null || SecondElement == null)
                {
                    ADResponse = new AnomalyDetectionResponse(101, "Function <CalculateDistance>: At least one input is null");
                    return Tuple.Create(-1.0, ADResponse);
                }         
                if (FirstElement.Length != SecondElement.Length)
                {
                    ADResponse = new AnomalyDetectionResponse(115, "Function <CalculateDistance>: Inputs have different dimensions");
                    return Tuple.Create(-1.0, ADResponse);
                }
                double SquaredDistance = 0;
                for (int i = 0; i < FirstElement.Length; i++)
                {
                    SquaredDistance += Math.Pow(FirstElement[i] - SecondElement[i], 2);
                }
                ADResponse = new AnomalyDetectionResponse(0, "OK");
                return Tuple.Create(Math.Sqrt(SquaredDistance), ADResponse);
            }
            catch(Exception Ex)
            {
                ADResponse = new AnomalyDetectionResponse(400, "Function <CalculateDistance>: Unhandled exception:\t" + Ex.ToString());
                return Tuple.Create(-1.0, ADResponse);
            }
        }

        /// <summary>
        /// RadialClustersCheck is a function that returns true if the farthest sample of each cluster is closer to the cluster centoid than the nearest foreign sample of the other clusters.
        /// </summary>
        /// <param name="Results">results of a clustering instance</param>
        /// <returns>
        /// - Item 1: true if the farthest sample of each cluster is closer to the cluster centoid than the nearest foreign sample of the other clusters, else false <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<bool, AnomalyDetectionResponse> RadialClustersCheck(ClusteringResults[] Results)
        {
            try
            {
                for (int j = 0; j < Results.Length; j++)
                {
                    if (Results[j].DistanceToNearestForeignSample < Results[j].InClusterMaxDistance)
                    {
                        return Tuple.Create(false, new AnomalyDetectionResponse(0,"OK"));
                    }
                }
                return Tuple.Create(true, new AnomalyDetectionResponse(0, "OK"));
            }
            catch(Exception Ex)
            {
                return Tuple.Create(false, new AnomalyDetectionResponse(400, "Function <RadialClustersCheck>: Unhandled exception:\t" + Ex.ToString()));
            }
        }

        /// <summary>
        /// StdDeviationClustersCheck is a function that returns true if the standard deviation in each cluster is less than the desired standard deviation
        /// </summary>
        /// <param name="Results">results of a clustering instance</param>
        /// <param name="StdDeviation">the desired standard deviation upper limit in each cluster</param> 
        /// <returns>
        /// - Item 1: true if the standard deviation in each cluster is less than the desired standard deviation, else false <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<bool, AnomalyDetectionResponse> StdDeviationClustersCheck(ClusteringResults[] Results, double[] StdDeviation)
        {
            try
            {
                for (int j = 0; j < Results.Length; j++)
                {
                    for (int k=0; k < Results[j].StandardDeviation.Length; k++)
                    {
                        if (Results[j].StandardDeviation[k] > StdDeviation[k])
                        {
                            return Tuple.Create(false, new AnomalyDetectionResponse(0, "OK"));
                        }
                    }
                }
                return Tuple.Create(true, new AnomalyDetectionResponse(0, "OK"));
            }
            catch (Exception Ex)
            {
                return Tuple.Create(false, new AnomalyDetectionResponse(400, "Function <StdDeviationClustersCheck>: Unhandled exception:\t" + Ex.ToString()));
            }
        }

        /// <summary>
        /// SaveToJson is a function that serializes an instance of AnomalyDetectionAPI to a JSON file.
        /// </summary>
        /// <param name="SavePath">path to save the AnomalyDetectionAPI instance</param>
        /// <param name="Instance">AnomalyDetectionAPI object to be saved</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static AnomalyDetectionResponse SaveToJSON(string SavePath, AnomalyDetectionAPI Instance)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                FileStream fs = new FileStream(SavePath, FileMode.Create);
                DataContractJsonSerializer JSONSerializer = new DataContractJsonSerializer(typeof(AnomalyDetectionAPI));
                JSONSerializer.WriteObject(fs, Instance);
                fs.Dispose();
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch(Exception Ex)
            {
                return new AnomalyDetectionResponse(400, "Function <SaveToJSON (AnomalyDetecionAPI)>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// SaveToJson is a function that serializes the clustering results to a JSON file.
        /// </summary>
        /// <param name="SavePath">path to save the AnomalyDetectionAPI instance</param>
        /// <param name="Results">the  clustering results object to be saved</param>
        /// <returns>a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static AnomalyDetectionResponse SaveToJSON(string SavePath, ClusteringResults[] Results)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                FileStream fs = new FileStream(SavePath, FileMode.Create);
                DataContractJsonSerializer JSONSerializer = new DataContractJsonSerializer(typeof(ClusteringResults[]));
                JSONSerializer.WriteObject(fs, Results);
                fs.Dispose();
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch (Exception Ex)
            {
                return new AnomalyDetectionResponse(400, "Function <SaveToJSON (ClusteringResults[])>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// LoadJSON_AnomalyDetectionAPI is a function that deserializes and loads an AnomalyDetectionAPI object from a JSON file.
        /// </summary>
        /// <param name="LoadPath">path to load the AnomalyDetectionAPI instance from</param>
        /// <returns>
        /// - Item 1: the loaded AnomalyDetectionAPI object <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<AnomalyDetectionAPI, AnomalyDetectionResponse> LoadJSON_AnomalyDetectionAPI(string LoadPath)
        {
            try
            {
                FileStream fs = new FileStream(LoadPath, FileMode.Open);
                DataContractJsonSerializer JSONSerializer = new DataContractJsonSerializer(typeof(AnomalyDetectionAPI));
                AnomalyDetectionAPI Obj = (AnomalyDetectionAPI)JSONSerializer.ReadObject(fs);
                fs.Dispose();
                if (Obj.Centroids == null)
                {
                    Obj = null;
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(206, "Function<LoadJSON_AnomalyDetectionAPI>: Can't deserialize file"));
                }
                return Tuple.Create(Obj,new AnomalyDetectionResponse(0,"OK"));
            }
            catch (Exception Ex)
            {
                AnomalyDetectionAPI Obj = null;
                /*if(ex is System.IO.FileNotFoundException)
                {
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(200, "Function<LoadJSON_AnomalyDetectionAPI>: File not found"));
                }*/
                if (Ex.ToString().StartsWith("System.IO.FileNotFoundException"))
                {
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(200, "Function<LoadJSON_AnomalyDetectionAPI>: File not found"));
                }                
                if (Ex is FileLoadException)
                {
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(202, "Function<LoadJSON_AnomalyDetectionAPI>: File cannot be loaded"));
                }
                if (Ex is SerializationException)
                {
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(203, "Function<LoadJSON_AnomalyDetectionAPI>: File content is corrupted"));
                }
                return Tuple.Create(Obj, new AnomalyDetectionResponse(400, "Function<LoadJSON_AnomalyDetectionAPI>: Unhandled exception:\t" + Ex.ToString()));
            }
        }

        /// <summary>
        /// LoadJSON_ClusteringResults is a function that deserializes and loads a ClusteringResults[] object from a JSON file.
        /// </summary>
        /// <param name="LoadPath">path to load the ClusteringResults[] object from</param>
        /// <returns>
        /// - Item 1: the loaded ClusteringResults[] object <br />
        /// - Item 2: a code and a message that state whether the function succeeded or encountered an error. When the function succeeds, it will return:
        /// <ul style="list-style-type:none">
        /// <li> - Code: 0, "OK" </li>
        /// </ul>
        /// </returns>
        private static Tuple<ClusteringResults[], AnomalyDetectionResponse> LoadJSON_ClusteringResults(string LoadPath)
        {
            ClusteringResults[] Obj;
            try
            {
                FileStream fs = new FileStream(LoadPath, FileMode.Open);
                DataContractJsonSerializer JSONSerializer = new DataContractJsonSerializer(typeof(ClusteringResults[]));
                Obj = (ClusteringResults[])JSONSerializer.ReadObject(fs);                
                fs.Dispose();
                if (Obj.Length == 0)
                {
                    Obj = null;
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(206, "Function <LoadJSON_ClusteringResults>: Can't deserialize file"));
                }
                return Tuple.Create(Obj,new AnomalyDetectionResponse(0,"OK"));
            }
            catch (Exception Ex)
            {
                Obj = null;
                /*if(ex is System.IO.FileNotFoundException)
                {
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(200, "Function <LoadJSON_ClusteringResults>: File not found"));
                }*/
                if (Ex.ToString().StartsWith("System.IO.FileNotFoundException"))
                {
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(200, "Function <LoadJSON_ClusteringResults>: File not found"));
                }
                if (Ex is FileLoadException)
                {
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(202, "Function <LoadJSON_ClusteringResults>: File cannot be loaded"));
                }
                if (Ex is SerializationException)
                {
                    return Tuple.Create(Obj, new AnomalyDetectionResponse(203, "Function <LoadJSON_ClusteringResults>: File content is corrupted"));
                }
                return Tuple.Create(Obj, new AnomalyDetectionResponse(400, "Function <LoadJSON_ClusteringResults>: Unhandled exception:\t" + Ex.ToString()));
            }
        }

        public AnomalyDetectionResponse GetPreviousSamples(out double[][] OldSamples, string LoadPath)
        {
            throw new NotImplementedException();
        }

        //-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->-->
    }
}
