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
        private const string MethodGet = "GET";
        private CloudStorageAccount _cloudStorageAccount;

        public DynamicStorageClient(CloudStorageAccount cloudStorageAccount)
        {
            _cloudStorageAccount = cloudStorageAccount;


            //HttpClient client = new HttpClient();
        }
        public string GetEntities(string tableName)
        {

            var uri = new Uri(string.Format("{0}/{1}", _cloudStorageAccount.TableEndpoint, tableName));
            var request = CreateRequest(uri, MethodGet);
            _cloudStorageAccount.Credentials.SignRequestLite(request);

            return ReturnResponse(request);
        }

        private static string ReturnResponse(HttpWebRequest request)
        {
            try
            {
                var response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string readToEnd = reader.ReadToEnd();
                    Console.WriteLine(readToEnd);
                    return readToEnd;
                }
            }
            catch
            {
                Console.WriteLine("Uri:" + request.RequestUri);
                foreach (string header in request.Headers.Keys)
                {
                    Console.WriteLine("{0}: {1}", header, request.Headers[header]);
                }
                throw;
            }
        }

        public string GetEntities(string tableName, string partitionKey)
        {
            var uri = new Uri(string.Format("{0}/{1}()?$filter=PartitionKey%20eq%20'{2}'", _cloudStorageAccount.TableEndpoint, tableName, partitionKey));
            var request = CreateRequest(uri, MethodGet);
            _cloudStorageAccount.Credentials.SignRequestLite(request);
            return ReturnResponse(request);
        }

        private static HttpWebRequest CreateRequest(Uri uri, string method)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/atom+xml";
            request.ReadWriteTimeout = 3000;
            return request;
        }
    }
}
