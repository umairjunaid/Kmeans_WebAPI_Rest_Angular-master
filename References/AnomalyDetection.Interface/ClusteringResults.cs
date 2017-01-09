using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AnomalyDetection.Interface
{
    /// <summary>
    /// ClusteringResults is a class that contains the results per cluster of a clustering instance with additional statistics. This class members are:
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
    [DataContract]
    public class ClusteringResults
    {
        [DataMember]
        public int[] ClusterDataOriginalIndex { get; internal set; }
        [DataMember]
        public double[][] ClusterData { get; internal set; }
        [DataMember]
        public double[] ClusterDataDistanceToCentroid { get; internal set; }
        [DataMember]
        public double[] Centroid { get; internal set; }
        [DataMember]
        public double[] Mean { get; internal set; }
        [DataMember]
        public double[] StandardDeviation { get; internal set; }
        [DataMember]
        public int InClusterFarthestSampleIndex { get; internal set; }
        [DataMember]
        public double[] InClusterFarthestSample { get; internal set; }
        [DataMember]
        public double InClusterMaxDistance { get; internal set; }
        [DataMember]
        public int NearestCluster { get; internal set; }
        [DataMember]
        public double DistanceToNearestClusterCentroid { get; internal set; }
        [DataMember]
        public double[] NearestForeignSampleInNearestCluster { get; internal set; }
        [DataMember]
        public double DistanceToNearestForeignSampleInNearestCluster { get; internal set; }
        [DataMember]
        public double[] NearestForeignSample { get; internal set; }
        [DataMember]
        public double DistanceToNearestForeignSample { get; internal set; }
        [DataMember]
        public int ClusterOfNearestForeignSample { get; internal set; }


        /// <summary>
        /// Dummy constructor
        /// </summary>
        private ClusteringResults()
        {
        }

        /// <summary>
        /// CreateClusteringResult is a function that generates the clustering results using several subfunctions.
        /// </summary>
        /// <param name="RawData">data to be clustered</param>
        /// <param name="DataToClusterMapping">contains the assigned cluster number for each sample of the RawData</param>
        /// <param name="Centroids">the centroids of the clusters</param>
        /// <param name="NumberOfClusters">the number of clusters</param>
        /// <returns>Tuple of two Items:
        /// - Item 1: contains the results for each cluster
        /// - Item 2: AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        ///     when the function succeeds, it will return:
        ///     - Code: 0, "OK"</returns>
        public static Tuple<ClusteringResults[], AnomalyDetectionResponse> CreateClusteringResult(double[][] RawData, int[] DataToClusterMapping, double[][] Centroids, int NumberOfClusters)
        {
            ClusteringResults[] Results;
            AnomalyDetectionResponse ADResponse;
            try
            {
                //get how many samples are there in each cluster
                Tuple<int[], AnomalyDetectionResponse> SICNResponse = SamplesInClusterNumber(RawData, DataToClusterMapping, NumberOfClusters);
                if (SICNResponse.Item2.Code != 0)
                {
                    Results = null;
                    return Tuple.Create(Results, SICNResponse.Item2);
                }
                int[] ClusterSamplesCounter = SICNResponse.Item1;
                Results = new ClusteringResults[NumberOfClusters];
                for (int i = 0; i < NumberOfClusters; i++)
                {
                    Results[i] = new ClusteringResults();
                    Results[i].Centroid = Centroids[i];
                    Results[i].ClusterData = new double[ClusterSamplesCounter[i]][];
                    Results[i].ClusterDataOriginalIndex = new int[ClusterSamplesCounter[i]];
                    //group the samples of the cluster
                    ADResponse = Results[i].AssignSamplesToClusters(RawData, DataToClusterMapping, i);
                    if (ADResponse.Code != 0)
                    {
                        Results = null;
                        return Tuple.Create(Results, ADResponse);
                    }
                    ADResponse = Results[i].CalculateStatistics();
                    if (ADResponse.Code != 0)
                    {
                        Results = null;
                        return Tuple.Create(Results, ADResponse);
                    }
                }

                //use functions to calculate the properties and statistics of each clusters.
                Tuple<int[], double[], AnomalyDetectionResponse> CNCResponse = CalculateNearestCluster(Centroids, ClusterSamplesCounter);
                if (CNCResponse.Item3.Code != 0)
                {
                    Results = null;
                    return Tuple.Create(Results, CNCResponse.Item3);
                }
                for (int i = 0; i < NumberOfClusters; i++)
                {
                    Results[i].NearestCluster = CNCResponse.Item1[i];
                    Results[i].DistanceToNearestClusterCentroid = CNCResponse.Item2[i];
                }

                double[][] NearestForeignSampleInNearestClusterArray;
                double[] DistanceToNearestForeignSampleInNearestClusterArray;
                double[][] NearestForeignSampleArray;
                double[] DistanceToNearestForeignSampleArray;
                int[] ClusterOfNearestForeignSampleArray;
                ADResponse = CalculateMoreStatistics(RawData, DataToClusterMapping, Centroids, CNCResponse.Item1, out NearestForeignSampleInNearestClusterArray, out DistanceToNearestForeignSampleInNearestClusterArray, out NearestForeignSampleArray, out DistanceToNearestForeignSampleArray, out ClusterOfNearestForeignSampleArray);
                if (ADResponse.Code != 0)
                {
                    Results = null;
                    return Tuple.Create(Results, ADResponse);
                }
                for (int i = 0; i < NumberOfClusters; i++)
                {
                    Results[i].NearestForeignSampleInNearestCluster = NearestForeignSampleInNearestClusterArray[i];
                    Results[i].DistanceToNearestForeignSampleInNearestCluster = DistanceToNearestForeignSampleInNearestClusterArray[i];
                    Results[i].NearestForeignSample = NearestForeignSampleArray[i];
                    Results[i].DistanceToNearestForeignSample = DistanceToNearestForeignSampleArray[i];
                    Results[i].ClusterOfNearestForeignSample = ClusterOfNearestForeignSampleArray[i];
                }

                ADResponse = new AnomalyDetectionResponse(0, "OK");
                return Tuple.Create(Results, ADResponse);
            }
            catch (Exception Ex)
            {
                Results = null;
                ADResponse = new AnomalyDetectionResponse(400, "Function <CreateClusteringResult>: Unhandled exception:\t" + Ex.ToString());
                return Tuple.Create(Results, ADResponse);
            }
        }

        /// <summary>
        /// SamplesInCLusterNumber is a function that counts the number of samples of each cluster.
        /// </summary>
        /// <param name="RawData">data to be clustered</param>
        /// <param name="DataToClusterMapping">contains the assigned cluster number for each sample of the RawData</param>
        /// <param name="NumberOfClusters">the number of clusters</param>
        /// <returns>Tuple of two Items:
        /// - Item 1: contains the number of samples for each cluster
        /// - Item 2: AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        ///     when the function succeeds, it will return:
        ///     - Code: 0, "OK"</returns>
        private static Tuple<int[], AnomalyDetectionResponse> SamplesInClusterNumber(double[][] RawData, int[] DataToClusterMapping, int NumberOfClusters)
        {
            AnomalyDetectionResponse ADResponse;
            int[] ClusterSamplesCounter;
            try
            {
                ClusterSamplesCounter = new int[NumberOfClusters];
                for (int i = 0; i < DataToClusterMapping.Length; i++)
                {
                    ClusterSamplesCounter[DataToClusterMapping[i]]++;

                }
                ADResponse = new AnomalyDetectionResponse(0, "OK");
                return Tuple.Create(ClusterSamplesCounter, ADResponse);
            }
            catch (Exception Ex)
            {
                ClusterSamplesCounter = null;
                ADResponse = new AnomalyDetectionResponse(400, "Function <SamplesInClusterNumber>: Unhandled exception:\t" + Ex.ToString());
                return Tuple.Create(ClusterSamplesCounter, ADResponse);
            }
        }

        /// <summary>
        /// AssignSamplesToClusters is a function that groups the samples of a cluster.
        /// </summary>
        /// <param name="RawData">data to be clustered</param>
        /// <param name="DataToClusterMapping">contains the assigned cluster number for each sample of the RawData</param>
        /// <param name="ClusterIndex">number of the cluster</param>
        /// <returns>AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        /// when the function succeeds, it will return:
        /// - Code: 0, "OK"</returns>
        private AnomalyDetectionResponse AssignSamplesToClusters(double[][] RawData, int[] DataToClusterMapping, int ClusterIndex)
        {
            try
            {
                for (int i = 0, j = 0; i < DataToClusterMapping.Length; i++)
                {
                    if (DataToClusterMapping[i] == ClusterIndex)
                    {
                        this.ClusterData[j] = RawData[i];
                        this.ClusterDataOriginalIndex[j] = i;
                        j++;
                    }
                }
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch (Exception Ex)
            {
                return new AnomalyDetectionResponse(400, "Function <AssignSamplesToClusters>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// CalculateStatistics is a function that claculates statistics and properties of a cluster. These statistics are independent on other clusters.
        /// </summary>
        /// <returns>AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        /// when the function succeeds, it will return:
        /// - Code: 0, "OK"</returns>
        private AnomalyDetectionResponse CalculateStatistics()
        {
            try
            {
                int NumberOfSamples = this.ClusterData.Length;
                int NumberOfAttributes = this.Centroid.Length;
                this.ClusterDataDistanceToCentroid = new double[NumberOfSamples];
                this.Mean = new double[NumberOfAttributes];
                this.StandardDeviation = new double[NumberOfAttributes];
                this.InClusterMaxDistance = -1;
                //in case of empty cluster
                if (NumberOfSamples == 0)
                {
                    this.InClusterFarthestSampleIndex = 0;
                    this.InClusterMaxDistance = 0;
                    this.InClusterFarthestSample = new double[2];
                    for (int j = 0; j < NumberOfAttributes; j++)
                    {
                        this.Mean[j] = 0;
                        this.Centroid[j] = 0;
                        this.InClusterFarthestSample[j] = 0;
                    }
                    this.NearestCluster = -1;
                    return new AnomalyDetectionResponse(0, "OK");
                }
                Tuple<double, AnomalyDetectionResponse> CDResponse;
                for (int i = 0; i < NumberOfSamples; i++)
                {
                    CDResponse = CalculateDistance(this.ClusterData[i], this.Centroid);
                    if (CDResponse.Item2.Code != 0)
                    {
                        return CDResponse.Item2;
                    }
                    //calculate distance for each sample
                    this.ClusterDataDistanceToCentroid[i] = CDResponse.Item1;
                    if (this.ClusterDataDistanceToCentroid[i] > this.InClusterMaxDistance)
                    {
                        //farthest sample
                        this.InClusterFarthestSampleIndex = i;
                        this.InClusterFarthestSample = this.ClusterData[i];
                        this.InClusterMaxDistance = this.ClusterDataDistanceToCentroid[i];
                    }
                    for (int j = 0; j < NumberOfAttributes; j++)
                    {
                        this.Mean[j] += ClusterData[i][j] / NumberOfSamples;
                    }
                }
                double[] ClusterVariance = new double[NumberOfAttributes];
                for (int i = 0; i < NumberOfSamples; i++)
                {
                    for (int j = 0; j < NumberOfAttributes; j++)
                    {
                        ClusterVariance[j] += Math.Pow((ClusterData[i][j] - this.Mean[j]), 2) / NumberOfSamples;
                    }
                }
                for (int i = 0; i < NumberOfAttributes; i++)
                {
                    this.StandardDeviation[i] = Math.Sqrt(ClusterVariance[i]);
                }

                return new AnomalyDetectionResponse(0, "OK");
            }
            catch (Exception Ex)
            {
                return new AnomalyDetectionResponse(400, "Function <CalculateStatistics>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// CalculateNearestCluster is a function that determines the nearest cluster and calculates the distance between those two clusters.
        /// </summary>
        /// <param name="Centroids">the centroids of the clusters</param>
        /// <param name="SamplesInClusters">number of samples in each cluster</param>
        /// <returns>Tuple of three Items:
        /// - Item 1: contains the number of nearest cluster
        /// - Item 2: contains the distance to the nearest cluster
        /// - Item 3: AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        ///     when the function succeeds, it will return:
        ///     - Code: 0, "OK"</returns>
        private static Tuple<int[], double[], AnomalyDetectionResponse> CalculateNearestCluster(double[][] Centroids, int[] SamplesInClusters)
        {
            AnomalyDetectionResponse ADResponse;
            int[] NearestClustersArray = new int[Centroids.Length];
            double[] DistanceToNearestClusterArray = new double[Centroids.Length];
            try
            {
                Tuple<double, AnomalyDetectionResponse> CDResponse;
                for (int i = 0; i < Centroids.Length; i++)
                {
                    //in case of empty cluster
                    if (SamplesInClusters[i] == 0)
                    {
                        NearestClustersArray[i] = -1;
                        DistanceToNearestClusterArray[i] = -1;
                        continue;
                    }
                    DistanceToNearestClusterArray[i] = double.MaxValue;
                    for (int j = 0; j < Centroids.Length; j++)
                    {
                        if (i == j || SamplesInClusters[j] == 0)
                        {
                            continue;
                        }
                        CDResponse = CalculateDistance(Centroids[i], Centroids[j]);
                        if (CDResponse.Item2.Code != 0)
                        {
                            NearestClustersArray = null;
                            DistanceToNearestClusterArray = null;
                            return Tuple.Create(NearestClustersArray, DistanceToNearestClusterArray, CDResponse.Item2);
                        }
                        if (CDResponse.Item1 < DistanceToNearestClusterArray[i])
                        {
                            DistanceToNearestClusterArray[i] = CDResponse.Item1;
                            NearestClustersArray[i] = j;
                        }
                    }
                }
                ADResponse = new AnomalyDetectionResponse(0, "OK");
                return Tuple.Create(NearestClustersArray, DistanceToNearestClusterArray, ADResponse);
            }
            catch (Exception Ex)
            {
                NearestClustersArray = null;
                DistanceToNearestClusterArray = null;
                ADResponse = new AnomalyDetectionResponse(400, "Function <CalculateNearestCluster>: Unhandled exception:\t" + Ex.ToString());
                return Tuple.Create(NearestClustersArray, DistanceToNearestClusterArray, ADResponse);
            }

        }

        /// <summary>
        /// CalculateNoreStatistics is a function that claculates statistics of a cluster. These statistics are dependent on other clusters.
        /// </summary>
        /// <param name="RawData">data to be clustered</param>
        /// <param name="DataToClusterMapping">contains the assigned cluster number for each sample of the RawData</param>
        /// <param name="Centroids">the centroids of the clusters</param>
        /// <param name="NearestCluster">nearest cluster number</param>
        /// <param name="NearestForeignSampleInNearestCluster">nearest sample belonging to the nearest cluster to this cluster's centroid</param>
        /// <param name="DistanceToNearestForeignSampleInNearestCluster">distance between the nearest sample of the nearest cluster and this cluster's centroid</param>
        /// <param name="NearestForeignSample">nearest sample not belonging to this cluster and this cluster's centroid</param>
        /// <param name="DistanceToNearestForeignSample">distance between the nearest foreign sample and this cluster's centroid</param>
        /// <param name="ClusterOfNearestForeignSample">the cluster to which the nearest foreign sample belongs</param>
        /// <returns>AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        /// when the function succeeds, it will return:
        /// - Code: 0, "OK"</returns>
        private static AnomalyDetectionResponse CalculateMoreStatistics(double[][] RawData, int[] DataToClusterMapping, double[][] Centroids, int[] NearestCluster, out double[][] NearestForeignSampleInNearestCluster, out double[] DistanceToNearestForeignSampleInNearestCluster, out double[][] NearestForeignSample, out double[] DistanceToNearestForeignSample, out int[] ClusterOfNearestForeignSample)
        {
            try
            {
                NearestForeignSampleInNearestCluster = new double[Centroids.Length][];
                DistanceToNearestForeignSampleInNearestCluster = new double[Centroids.Length];
                NearestForeignSample = new double[Centroids.Length][];
                DistanceToNearestForeignSample = new double[Centroids.Length];
                ClusterOfNearestForeignSample = new int[Centroids.Length];

                for (int i = 0; i < Centroids.Length; i++)
                {
                    //in case of empty cluster
                    if (NearestCluster[i] == -1)
                    {
                        NearestForeignSampleInNearestCluster[i] = null;
                        NearestForeignSample[i] = null;
                        DistanceToNearestForeignSampleInNearestCluster[i] = -1;
                        DistanceToNearestForeignSample[i] = -1;
                        ClusterOfNearestForeignSample[i] = -1;
                    }
                    else
                    {
                        DistanceToNearestForeignSampleInNearestCluster[i] = double.MaxValue;
                        DistanceToNearestForeignSample[i] = double.MaxValue;
                    }
                }

                Tuple<double, AnomalyDetectionResponse> CDResponse;
                for (int i = 0; i < RawData.Length; i++)
                {
                    for (int j = 0; j < Centroids.Length; j++)
                    {
                        //skip if sample belong to the cluster itself or the cluster is empty
                        if (DataToClusterMapping[i] == j || NearestCluster[j] == -1)
                        {
                            continue;
                        }

                        CDResponse = CalculateDistance(RawData[i], Centroids[j]);
                        if (CDResponse.Item2.Code != 0)
                        {
                            NearestForeignSampleInNearestCluster = null;
                            DistanceToNearestForeignSampleInNearestCluster = null;
                            NearestForeignSample = null;
                            DistanceToNearestForeignSample = null;
                            ClusterOfNearestForeignSample = null;
                            return CDResponse.Item2;
                        }

                        if (CDResponse.Item1 < DistanceToNearestForeignSample[j])
                        {
                            DistanceToNearestForeignSample[j] = CDResponse.Item1;
                            NearestForeignSample[j] = RawData[i];
                            ClusterOfNearestForeignSample[j] = DataToClusterMapping[i];
                        }

                        if (DataToClusterMapping[i] == NearestCluster[j])
                        {
                            if (CDResponse.Item1 < DistanceToNearestForeignSampleInNearestCluster[j])
                            {
                                DistanceToNearestForeignSampleInNearestCluster[j] = CDResponse.Item1;
                                NearestForeignSampleInNearestCluster[j] = RawData[i];
                            }
                        }
                    }
                }
                return new AnomalyDetectionResponse(0, "OK");
            }
            catch (Exception Ex)
            {
                NearestForeignSampleInNearestCluster = null;
                DistanceToNearestForeignSampleInNearestCluster = null;
                NearestForeignSample = null;
                DistanceToNearestForeignSample = null;
                ClusterOfNearestForeignSample = null;
                return new AnomalyDetectionResponse(400, "Function <CalculateMoreStatistics>: Unhandled exception:\t" + Ex.ToString());
            }
        }

        /// <summary>
        /// CalculateDistance is a function that claculates the distance between two elements of same size.
        /// </summary>
        /// <param name="FirstElement">first element</param>
        /// <param name="SecondElement">second element</param>
        /// <returns>
        /// - Item 1: distance between the two elements
        /// - Item 2: AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        ///     when the function succeeds, it will return:
        ///     - Code: 0, "OK"</returns>
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
            catch (Exception Ex)
            {
                ADResponse = new AnomalyDetectionResponse(400, "Function <CalculateDistance>: Unhandled exception:\t" + Ex.ToString());
                return Tuple.Create(-1.0, ADResponse);
            }
        }

    }
}
