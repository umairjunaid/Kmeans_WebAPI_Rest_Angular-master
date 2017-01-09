using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
//Susing System.Web.Http;
using System.Web.Mvc;
using System.IO;
using System.Web.Http.Cors;

namespace Kmeans_Web_API.Controllers
{
    //[EnableCors(origins: "http://localhost:3442", headers: "*", methods: "*")]
    public class FileUploadController : Controller
    {
        public ActionResult UploadMultipleFile()
        {
            return View();
        }

        [HttpPost]
        public virtual string UploadFile(object obj)
        {
            Server.MapPath("File1.t‌​xt");
            var length = Request.ContentLength;//Post Data
            var bytes = new byte[length];
            Request.InputStream.Read(bytes, 0, length);
           string strpath = Server.MapPath("File1.t‌​xt");
            if (System.IO.File.Exists(Server.MapPath("cluster.json")));// in case some files exist already delete them and upload the current one
            {
                System.IO.File.Delete(Server.MapPath("cluster.json"));
                System.IO.File.Delete(Server.MapPath("cluster.json"));
            }
            var fileName = Request.Headers["X-File-Name"];
            var fileSize = Request.Headers["X-File-Size"];
            var fileType = Request.Headers["X-File-Type"];

            fileName = "data.csv";
            var saveToFileLoc = Server.MapPath("data.csv") ; //Upload the file to folder
            // save the file.
            var fileStream = new FileStream(saveToFileLoc, FileMode.Create, FileAccess.ReadWrite);
            fileStream.Write(bytes, 0, length);
            fileStream.Close();

            return string.Format("{0} bytes uploaded", bytes.Length);
        }
    }
}
