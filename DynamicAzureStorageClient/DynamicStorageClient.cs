using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.WindowsAzure;

namespace DynamicAzureStorageClient
{
    public class DynamicStorageClient
    {
        private const string MethodGet = "GET";
        private CloudStorageAccount _cloudStorageAccount;
        private readonly IParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicStorageClient"/> class.
        /// </summary>
        /// <param name="cloudStorageAccount">The cloud storage account.</param>
        public DynamicStorageClient(CloudStorageAccount cloudStorageAccount)
        {
            _cloudStorageAccount = cloudStorageAccount;
            _parser = new DynamicEntityParser();
        }

        public string GetStreams(string tableName)
        {

            var uri = new Uri(string.Format("{0}/{1}", _cloudStorageAccount.TableEndpoint, tableName));
            var request = CreateRequest(uri, MethodGet);
            _cloudStorageAccount.Credentials.SignRequestLite(request);
            return ReturnResponse(request);
        }
        public IEnumerable<ExpandoEntity> GetEntities(string tableName)
        {
            var uri = new Uri(string.Format("{0}/{1}", _cloudStorageAccount.TableEndpoint, tableName));
            var request = CreateRequest(uri, MethodGet);
            request.Proxy = null; // shaves off 1ms/run on average.
            _cloudStorageAccount.Credentials.SignRequestLite(request);
            return ReturnEntities(request);
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

        private IEnumerable<ExpandoEntity> ReturnEntities(HttpWebRequest request)
        {
            using (var stream = request.GetResponse().GetResponseStream())
                return _parser.ParseEntries(stream);
        }

        /// <summary>
        /// Gets the streams.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns></returns>
        public string GetStreams(string tableName, string partitionKey)
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
