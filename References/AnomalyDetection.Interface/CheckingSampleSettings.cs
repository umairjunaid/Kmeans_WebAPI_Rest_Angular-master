using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnomalyDetection.Interface
{
    /// <summary>
    /// CheckingSampleSettings is a class that contains the desired settings by the user for checking to which cluster a sample belongs. This class members are:
    /// - ProjectPath: path to the clustering intance that contains the clusters data
    /// - Sample: the sample to be checked
    /// - tolerance: a value in % representing the tolerance to possible outliers
    /// </summary>
    public class CheckingSampleSettings
    {
        public string ProjectPath { get; internal set; }
        public double[] Sample { get; internal set; }
        public double tolerance { get; internal set; }

        /// <summary>
        /// Constructor to create the desired settings
        /// </summary>
        /// <param name="ProjectPath">path to the clustering intance that contains the clusters data</param>
        /// <param name="Sample">the sample to be checked</param>
        /// <param name="tolerance">a value in % representing the tolerance to possible outliers</param>
        public CheckingSampleSettings(string ProjectPath, double[] Sample, double tolerance = 0)
        {
            this.ProjectPath = ProjectPath;
            this.Sample = Sample;
            this.tolerance = tolerance;
        }
    }
}
