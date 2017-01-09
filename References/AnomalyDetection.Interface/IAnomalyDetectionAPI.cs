using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnomalyDetection.Interface
{  /// <summary>
   /// Interface for AnomalyDetectionAPI
   /// </summary>
    public interface IAnomalyDetectionApi

    {
        /// <summary>
        /// ImportNewDataForClustering is a function that start a new clustering instance or add to an existing one. It saves the result.
        /// </summary>
        /// <param name="Settings">contains the desired settings for the clustering process</param>
        /// <returns>AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        /// when the function succeeds, it will return:
        /// - Code: 0, "Clustering Complete. K-means stopped at the maximum allowed iteration: " + Maximum_Allowed_Iteration
        /// or
        /// - Code: 0, "Clustering Complete. K-means converged at iteration: " + Iteration_Reached</returns>
        AnomalyDetectionResponse ImportNewDataForClustering(ClusteringSettings Settings);

        /// <summary>
        /// CheckSample is a function that detect to which cluster the given sample belongs to.
        /// </summary>
        /// <param name="Settings">contains the desired settings for detecting to which, if any, cluster the sample belongs</param>
        /// <param name="ClusterIndex">the cluster number towhich the sample belongs (-1 if the sample doesn't belong to any cluster or if an error was encountered).</param>
        /// <returns>AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        /// when the function succeeds, it will return:
        /// - Code: 0, "This sample belongs to cluster: " + Cluster_Number
        /// or
        /// - Code: 1, "This sample doesn't belong to any cluster"</returns></returns>
        AnomalyDetectionResponse CheckSample(CheckingSampleSettings Settings, out int ClusterIndex);

        /// <summary>
        /// GetResults is a function that returns the results of an existing clustering instance 
        /// </summary>
        /// <param name="LoadPath">path of the clustering instance</param>
        /// <param name="Result">the variable through which the clustering result are returned</param>
        /// <returns>AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        /// when the function succeeds, it will return:
        /// - Code: 0, "OK"</returns>
        AnomalyDetectionResponse GetResults(string LoadPath, out ClusteringResults[] Result);

        /// <summary>
        /// GetPreviousSamples is a function that loads samples from a previous clustering instance
        /// </summary>
        /// <param name="OldSamples">the variable through which the samples are returned</param>
        /// <param name="LoadPath">path of the clustering instance</param>
        /// <returns>AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        /// when the function succeeds, it will return:
        /// - Code: 0, "OK"</returns>
        AnomalyDetectionResponse GetPreviousSamples(out double[][] OldSamples, string LoadPath);

        /// <summary>
        /// RecommendedNumberOfClusters is a function that returns a recommended number of clusters for the given samples.
        /// </summary>
        /// <param name="RawData">the samples to be clustered</param>
        /// <param name="KmeansMaxIterations">maximum allowed number of Kmeans iteration for clustering</param>
        /// <param name="NumberOfAttributes">number of attributes for each sample</param>
        /// <param name="MaxNumberOfClusters">maximum desired number of clusters</param>
        /// <param name="MinNumberOfClusters">minimum desired number of clusters</param>
        /// <param name="Method">integer 0,1 or 2 representing the method to be used. 
        /// - Method 0: Radial method in which the farthest sample of each cluster must be closer to the cluster centoid than the nearest foreign sample of the other clusters
        /// - Method 1: Standard Deviation method in which the standard deviation in each cluster must be less than the desired standard deviation
        /// - Method 2: Both. uses radial & standard deviation methods at the same time</param>
        /// <param name="StdDev">the desired standard deviation upper limit in each cluster</param>
        /// <param name="RecNumberOfClusters">the variable through which the recommended number of clusters is returned</param>
        /// <returns>AnomalyDetectionResponse: a code and a message that state whether the function succeeded or encountered an error
        /// when the function succeeds, it will return:
        /// - Code: 0, "OK"
        /// or
        /// - Code: 1, "Could not find a recommended number of clusters based on the desired constraints"</returns>
        AnomalyDetectionResponse RecommendedNumberOfClusters(double[][] RawData, int KmeansMaxIterations, int NumberOfAttributes, int MaxNumberOfClusters, int MinNumberOfClusters, int Method, double[] StdDev, out int RecNumberOfClusters);
    }
}