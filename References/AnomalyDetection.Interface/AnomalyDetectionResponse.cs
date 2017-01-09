using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnomalyDetection.Interface
{
    /// <summary>
    /// AnomalyDetectionResponse is a class that is used to determine whether the function succeeded or encountered an error. This class members are:
    /// - Code: integer that is related to the state of the function
    /// - Message: string that is related to the state of the function
    /// 
    /// Succes/Error Codes:
    /// - success: 0-99
    /// - user input errors: 100-199
    ///     - 100     "RawData is null"
    ///     - 101     "At least one input is null"
    ///     - 102     "RawData is empty"
    ///     - 103     "Means is empty"
    ///     - 104     "Maximum number of clusters must be at least 2"
    ///     - 105	  "Unacceptable number of clusters. Clusters more than samples"
    ///     - 106	  "Unacceptable number of clusters. Must be at least 2"
    ///     - 107	  "Unacceptable number of attributes. Must be at least 1"
    ///     - 108	  "Unacceptable number of maximum iterations"
    ///     - 109	  "Unacceptable savepath"
    ///     - 110	  "Unacceptable tolerance value"
    ///     - 111	  "Data sample and number of attributes are inconsistent. First encountered inconsistency in data sample: " + i + "."
    ///     - 112	  "Mismatch between old and new cluster numbers"
    ///     - 113	  "Mismatch between old and new number of atributes"
    ///     - 114	  "Mismatch in number of attributes"
    ///     - 115	  "Inputs have different dimensions"
    ///     - 116	  "Path provided : " + Path + " has no root"
    ///     - 117	  "Path provided : " + Path + " contains invalid chars. First invalid char encountered is: \'" + InvalidChars[i] + "\'."
    ///     - 118	  "Path provided : " + Path + " has no project name specified."
    ///     - 119	  "Path provided : " + Path + " has a project name containing invalid chars. First invalid char encountered is: \'" + InvalidChars[i] + "\'."
    ///     - 120	  "Path provided : " + Path + " has wrong extension."
    ///     - 121	  "Requested load path does not exist"
    ///     - 122	  "Method must be either 0,1 or 2"
    ///     - 123	  "Parameter StdDev is needed"
    /// - file related errors: 200-299
    ///     - 200	"File not found"
    ///     - 201	  "File already exists"
    ///     - 202	  "File cannot be loaded"
    ///     - 203	  "File content is corrupted"
    ///     - 204	  "Unauthorized access to file. File is readonly."
    ///     - 205	  "Unauthorized access to file. File is a system file."
    ///     - 206	  "Can't deserialize file"
    /// - calculation errors: 300-399
    ///     - 300	  "Division by zero"
    /// - unhandled errors: 400
    ///     - 400     "Function Function_Name: Unhnadled exception: " + Exception</summary>
    public class AnomalyDetectionResponse
    {
        public int Code { get; internal set; }
        public string Message { get; internal set; }

        /// <summary>
        /// Constructor to create the response
        /// </summary>
        /// <param name="Code"></param>
        /// <param name="Message"></param>
        public AnomalyDetectionResponse(int Code, string Message)
        {
            this.Code = Code;
            this.Message = Message;
        }
    }
}
