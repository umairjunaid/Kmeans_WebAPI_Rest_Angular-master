using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace AnomalyDetection.Interface
{
    /// <summary>
    /// ClusteringSettings is a class that contains the desired settings by the user for clustering. This class members are:
    /// - RawData: data to be clustered
    /// - KmeansMaxIterations: maximum allowed number of Kmeans iteration for clustering
    /// - NumberOfClusters: number of clusters
    /// - NumberOfAttributes: number of attributes for each sample
    /// - SavePath: path to save the clustering instance
    /// - LoadPath: path to load a clustering instance. can be "" or null in case of not loading
    /// - Replace: bool if true overwriting an existing clustering instance is allowed
    /// </summary>
    public class ClusteringSettings
    {
        public double[][] RawData { get; internal set; }
        public int KmeansMaxIterations { get; internal set; }
        public int NumberOfClusters { get; internal set; }
        public int NumberOfAttributes { get; internal set; }
        public string SavePath { get; internal set; }
        public string LoadPath { get; internal set; }
        public bool Replace { get; internal set; }

        /// <summary>
        /// Constructor to create the desired settings.
        /// </summary>
        /// <param name="RawData">data to be clustered</param>
        /// <param name="KmeansMaxIterations">maximum allowed number of Kmeans iteration for clustering</param>
        /// <param name="NumberOfClusters">number of clusters</param>
        /// <param name="NumberOfAttributes">number of attributes for each sample</param>
        /// <param name="SavePath">path to save the clustering instance</param>
        /// <param name="LoadPath">path to load a clustering instance. can be "" or null in case of not loading</param>
        /// <param name="Replace">bool if true overwriting an existing clustering instance is allowed</param>
        public ClusteringSettings(double[][] RawData, int KmeansMaxIterations, int NumberOfClusters, int NumberOfAttributes, string SavePath, string LoadPath = "", bool Replace = false)
        {
            this.RawData = RawData;
            this.KmeansMaxIterations = KmeansMaxIterations;
            this.NumberOfClusters = NumberOfClusters;
            this.NumberOfAttributes = NumberOfAttributes;
            this.LoadPath = LoadPath;
            this.SavePath = SavePath;
            this.Replace = Replace;
        }

    }
}
