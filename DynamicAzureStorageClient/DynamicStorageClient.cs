using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsAzure;

namespace DynamicAzureStorageClient
{
    public class DynamicStorageClient
    {
        private CloudStorageAccount _cloudStorageAccount;

        public DynamicStorageClient(CloudStorageAccount cloudStorageAccount)
        {
            _cloudStorageAccount = cloudStorageAccount;


            //HttpClient client = new HttpClient();
        }
        public string GetEntities(string tableName){
            string accountName = _cloudStorageAccount.Credentials.AccountName;
            string key = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

            //var signature = string.Format("GET\n\n\n{0}\n/{1}/{2}", request.Headers["x-ms-date"], accountName, resource);
            var uri = new Uri(string.Format("{0}/{1}", _cloudStorageAccount.TableEndpoint, tableName));
            var request = CreateRequest(uri, "GET");
            _cloudStorageAccount.Credentials.SignRequestLite(request);
            
            var response = request.GetResponse();
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string readToEnd = reader.ReadToEnd();
                Console.WriteLine(readToEnd);
                return readToEnd;
            }
        }

        private static HttpWebRequest CreateRequest(Uri uri, string method)
        {
            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(uri);
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/atom+xml";
            request.ReadWriteTimeout = 3000;
            return request;
        }
    }
}
