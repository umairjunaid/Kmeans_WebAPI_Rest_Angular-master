using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using AnomalyDetection.Interface;

namespace MockApi
{
    public class MockApi: IAnomalyDetectionApi
    {
        
        public MockApi(double[][] RawData, int NumberOfClusters)
        { 
        }

        
        [DataMember]
        private double[][] RawData { get; set; }

        [DataMember]
        private int NumberOfClusters { get; set; }

        [DataMember]
        private double[][] Centroids { get; set; }
        
        [DataMember]
        private double[] InClusterMaxDistance { get; set; }
        
        [DataMember]
        private int[] DataToClusterMapping { get; set; }

        //Main functions
        
        public AnomalyDetectionResponse ImportNewDataForClustering(ClusteringSettings Settings)
        {
            Random rnd = new Random();
            int n = rnd.Next(1);
            if (n == 0)
            {
                return new AnomalyDetectionResponse(0, "Clustering Complete. K-means converged at iteration: 5");
            }
            else
            {
                return new AnomalyDetectionResponse(0, "Clustering Complete. K-means stopped at the maximum allowed iteration: 300");
            }
        }

        
        public AnomalyDetectionResponse CheckSample(CheckingSampleSettings Settings, out int ClusterIndex)
        {
            Random rnd = new Random();
            int n = rnd.Next(1);
            int c = rnd.Next(2);
            if (n == 0)
            {
                ClusterIndex = c;
                return new AnomalyDetectionResponse(0, "This sample belongs to cluster: " + c);
            }
            else
            {
                ClusterIndex = -1;
                return new AnomalyDetectionResponse(1, "This sample doesn't belong to any cluster");
            }
        }

        //send either 2 or 3 in the load path representing the number of attributes you need in the mock
        public AnomalyDetectionResponse GetResults(string LoadPath, out ClusteringResults[] Result)
        {
            if (LoadPath.Equals("2"))
            {
                FileStream fs = new FileStream(@"C:\MOCK\Result\Mock2D.json", FileMode.Open);
                DataContractJsonSerializer JSONSerializer = new DataContractJsonSerializer(typeof(ClusteringResults[]));
                Result = (ClusteringResults[])JSONSerializer.ReadObject(fs);
                fs.Dispose();
            }
            else
            {
                FileStream fs = new FileStream(@"C:\MOCK\Result\Mock3D.json", FileMode.Open);
                DataContractJsonSerializer JSONSerializer = new DataContractJsonSerializer(typeof(ClusteringResults[]));
                Result = (ClusteringResults[])JSONSerializer.ReadObject(fs);
                fs.Dispose();
            }

            return new AnomalyDetectionResponse(0, "OK");

        }

        //send either 2 or 3 in the load path representing the number of attributes you need in the mock
        public AnomalyDetectionResponse GetPreviousSamples(string LoadPath, out double[][] OldSamples)
        {
            if (LoadPath.Equals("2"))
            {
                OldSamples = new double[12][] { new double[] {1,3},
                                                new double[] {0,2},
                                                new double[] {9,7},
                                                new double[] {25,20},
                                                new double[] {0.5,1.5},
                                                new double[] {27,22},
                                                new double[] {25,28},
                                                new double[] {8,9},
                                                new double[] {26,23.5},
                                                new double[] {24,22},
                                                new double[] {10,8},
                                                new double[] {10.5,11}};
            }
            else
            {
                OldSamples = new double[12][] { new double[] {1,3,2},
                                                new double[] {0,2,2.5},
                                                new double[] {9,7,10},
                                                new double[] {25,20,23},
                                                new double[] {0.5,1.5,1},
                                                new double[] {27,22,24},
                                                new double[] {25,28,26},
                                                new double[] {8,9,9},
                                                new double[] {26,23.5,27},
                                                new double[] {24,22,25},
                                                new double[] {10,8,8},
                                                new double[] {10.5,11,8}};
            }

            return new AnomalyDetectionResponse(0, "OK");
        }
       
        public AnomalyDetectionResponse RecommendedNumberOfClusters(double[][] RawData, int KmeansMaxIterations, int NumberOfAttributes, int MaxNumberOfClusters, int MinNumberOfClusters, int Method, double[] StdDev, out int RecNumberOfClusters)
        {
            Random rnd = new Random();
            int n = rnd.Next(1);
            if (n == 0)
            {
                RecNumberOfClusters = 3;
                return new AnomalyDetectionResponse(0, "OK");
            }
            else
            {
                RecNumberOfClusters = 0;
                return new AnomalyDetectionResponse(1, "Could not find a recommended number of clusters based on the desired constraints");
            }        
        }


        ///// read csv
        public static double[][] CSVtoDoubleJaggedArray(string FilePath)
        {
            if (FilePath.EndsWith(".csv"))
            {
                if (File.Exists(FilePath))
                {
                    string CsvFile = "";
                    double[][] CsvData;
                    CsvFile = File.ReadAllText(FilePath);
                    if (CsvFile.EndsWith("\r\n"))
                    {
                        CsvFile = CsvFile.Remove(CsvFile.Length - 2, 2);
                    }
                    string[] RowDelimiter = { "\r\n" };
                    string[] CellDelimiter = { "," };

                    int CsvFileRowsNumber, CsvFileCellsNumber;
                    string[] Rows, Cells;
                    
                    Rows = CsvFile.Split(RowDelimiter, StringSplitOptions.None);
                    CsvFileRowsNumber = Rows.Length;

                    CsvFileCellsNumber = Rows[0].Split(CellDelimiter, StringSplitOptions.None).Length;
                    CsvData = new double[CsvFileRowsNumber][];
                    for (int i=0; i < CsvFileRowsNumber; i++)
                    {
                        CsvData[i] = new double[CsvFileCellsNumber];
                    }
                    
                    for (int i = 0; i < CsvFileRowsNumber; i++)
                    {
                        Cells = Rows[i].Split(CellDelimiter, StringSplitOptions.None);

                        for (int j = 0; j < CsvFileCellsNumber; j++)
                        {
                            try
                            {
                                CsvData[i][j] = Convert.ToDouble(Cells[j]);
                            }
                            catch (FormatException)
                            {
                                return null;
                            }
                            catch (OverflowException)
                            {
                                return null;
                            }

                        }
                    }

                    return CsvData;
                }
            }
            return null;
        }

        public AnomalyDetectionResponse GetPreviousSamples(out double[][] OldSamples, string LoadPath)
        {
            throw new NotImplementedException();
        }
    }
}
