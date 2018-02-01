﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Docker.Registry.DotNet.Models;

namespace Docker.Registry.DotNet.Endpoints
{
    internal class BlobUploadOperations : IBlobUploadOperations
    {
        private readonly NetworkClient _client;

        internal BlobUploadOperations(NetworkClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Perform a monolithic upload.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contentLength"></param>
        /// <param name="stream"></param>
        /// <param name="digest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UploadBlobAsync(string name, int contentLength, Stream stream, string digest,
            CancellationToken cancellationToken = new CancellationToken())
        {
            string path = $"v2/{name}/blobs/uploads/";

            var response = await _client.MakeRequestAsync(cancellationToken, HttpMethod.Post, path);

            string uuid = response.Headers.GetString("Docker-Upload-UUID");

            Console.WriteLine($"Uploading with uuid: {uuid}");

            var location = response.Headers.GetString("Location");

            Console.WriteLine($"Using location: {location}");

            //await GetBlobUploadStatus(name, uuid, cancellationToken);

            

            try
            {
                using (var client = new HttpClient())
                {
                    var progressResponse = await client.GetAsync(location, cancellationToken);

                    //Send the contents of the whole file
                    var content = new StreamContent(stream);

                    content.Headers.ContentLength = stream.Length;
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Headers.ContentRange = new ContentRangeHeaderValue(0, stream.Length);

                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), location + $"&digest={digest}")
                    {
                        Content = content
                    };

                    var response2 = await client.SendAsync(request, cancellationToken);

                    if (response2.StatusCode < HttpStatusCode.OK || response2.StatusCode >= HttpStatusCode.BadRequest)
                    {
                        throw new RegistryApiException(new RegistryApiResponse<string>(response2.StatusCode, null, response.Headers));
                    }

                   progressResponse = await client.GetAsync(location, cancellationToken);
                }

                ////{

                ////    var queryString = new QueryString();

                ////    queryString.Add("digest", digest);

                ////    var content = new StreamContent(stream);

                ////    content.Headers.ContentLength = 0;
                ////    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                ////    //content.Headers.ContentRange = new ContentRangeHeaderValue(0, stream.Length);

                ////    await _client.MakeRequestAsync(cancellationToken, HttpMethod.Put, $"v2/{name}/blobs/uploads/{uuid}",
                ////        queryString);
                ////}

                //using (var client = new HttpClient())
                //{
                //    var content = new StringContent("");

                //    content.Headers.ContentLength = 0;
                //    //content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                //    var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"http://10.0.4.44:5000/v2/{name}/blobs/uploads/{uuid}&digest={digest}"))
                //    {
                //        Content = content
                //    };

                //    var response2 = await client.SendAsync(request, cancellationToken);


                //    //content.Headers.ContentRange = new ContentRangeHeaderValue(0, stream.Length);

                //    if (response2.StatusCode < HttpStatusCode.OK || response2.StatusCode >= HttpStatusCode.BadRequest)
                //    {
                //        throw new RegistryApiException(new RegistryApiResponse<string>(response2.StatusCode, null, response.Headers));
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                Console.WriteLine("Attempting to cancel the upload...");
                await _client.MakeRequestAsync(cancellationToken, HttpMethod.Delete, $"v2/{name}/blobs/uploads/{uuid}");

                throw;
            }

         

            //string path2 = $"v2/{name}/blobs/uploads/{uuid}";

            //var response2 = await _client.MakeRequestAsync(cancellationToken, HttpMethod.Put, path2, queryString);


            //await _client.MakeRequestAsync(cancellationToken, HttpMethod.Put, location, queryString);
        }

        public Task<ResumableUploadResponse> InitiateBlobUploadAsync(string name, Stream stream = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<MountResponse> MountBlobAsync(string name, MountParameters parameters,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public async Task<BlobUploadStatus> GetBlobUploadStatus(string name, string uuid, CancellationToken cancellationToken = new CancellationToken())
        {
            
            throw new NotImplementedException();
        }

        public Task<ResumableUploadResponse> UploadBlobChunkAsync(string name, string uuid, Stream chunk,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task<ResumableUploadResponse> CompleteBlobUploadAsync(string name, string uuid, Stream chunk = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task CancelBlobUploadAsync(string name, string uuid, CancellationToken cancellationToken = new CancellationToken())
        {
            string path = $"v2/{name}/blobs/uploads/{uuid}";

            return _client.MakeRequestAsync(cancellationToken, HttpMethod.Delete, path);
        }
    }
}