﻿namespace ParkingDataSetup
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Web;

    using Newtonsoft.Json.Linq;
    using ParkingDataSetup.Model;

    public class DWGZipUploader
    {
        private const string BaseUrl = "https://us.atlas.microsoft.com";

        private const string ApiVersion = "1.0";

        private const string SubscriptionKey = "Qx05yzZN1EVBesESLh1CCFiiUSRbccCB-d5LP-sqkxE";

        private const string BlueprintFilFormat = "zip";

        public Parking Parking { get; set; }

        public bool IsError { get; set; }

        public DWGZipUploader()
        {
            Parking = new Parking();
            IsError = false;
        }

        public void Run()
        {
            try
            {
                ValidateBlueprintFile();

                var httpResponseMessage = UploadDwg(Parking.Blueprint);

                var resourceLocation = GetUploadStatus(httpResponseMessage.Headers.Location.AbsoluteUri);

                httpResponseMessage = ConvertDwg(GetUdid(resourceLocation));

                resourceLocation = GetConversionStatus(httpResponseMessage.Headers.Location.AbsoluteUri);

                httpResponseMessage = GenerateDataset(GetConversionId(resourceLocation));

                resourceLocation = GetDatasetStatus(httpResponseMessage.Headers.Location.AbsoluteUri);

                Parking.DatasetID = GetDatasetId(resourceLocation);

                httpResponseMessage = GenerateTileset(Parking.DatasetID);

                resourceLocation = GetTilesetStatus(httpResponseMessage.Headers.Location.AbsoluteUri);

                Parking.TilesetID = GetTilesetId(resourceLocation);

                IsError = false;

                Console.WriteLine("DWGZipUploader successfull for : " + Parking.Id);
            }
            catch (Exception ex)
            {
                IsError = true;
                var errorMessage = string.Format("Record not processed : {0}; {1}", Parking.Id, ex);
                Console.WriteLine(errorMessage);
            }
        }

        private void ValidateBlueprintFile()
        {
            var fileType = Path.GetExtension(Parking.Blueprint);
            if (fileType.ToLower() == BlueprintFilFormat)
            {
                return;
            }

            if (File.Exists(Parking.Blueprint))
            {
                return;
            }

            throw new FileNotFoundException("Incorrect file format");
        }

        private HttpResponseMessage UploadDwg(string file)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request parameters
            queryString["api-version"] = ApiVersion;
            queryString["dataFormat"] = BlueprintFilFormat;
            queryString["subscription-key"] = SubscriptionKey;
            var uri = BaseUrl + "/mapData/upload?" + queryString;

            HttpResponseMessage response;

            // Request body
            byte[] bytes = File.ReadAllBytes(file);

            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response = client.PostAsync(uri, content)?.Result;
            if (response?.StatusCode != HttpStatusCode.Accepted)
            {
                throw new HttpRequestException("Failed in UploadDwg for id: " + Parking.Id);
            }

            Console.WriteLine("DWG Upload accepted for id: " + Parking.Id);

            return response;
        }

        private string GetUploadStatus(string url)
        {
            var apiUrl = url.Replace("atlas.", "us.atlas.") + "&subscription-key=" + SubscriptionKey;
            var client = new HttpClient();
            var response = client.GetAsync(apiUrl)?.Result;
            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync()?.Result;
            dynamic data = JObject.Parse(responseBody);
            if (data.status == "Succeeded")
            {
                Console.WriteLine("DWG upload successful for id: " + Parking.Id);
                return data.resourceLocation;
            }

            if (data.status == "Failed")
            {
                Console.WriteLine("DWG upload failed for id: " + Parking.Id);
                throw new Exception(data.error);
            }

            Thread.Sleep(1000);
            return GetUploadStatus(url);
        }

        private string GetUdid(string url)
        {
            var apiUrl = url.Replace("atlas.", "us.atlas.") + "&subscription-key=" + SubscriptionKey;
            var client = new HttpClient();
            var response = client.GetAsync(apiUrl)?.Result;
            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync()?.Result;
            dynamic data = JObject.Parse(responseBody);
            return data.udid;
        }

        private HttpResponseMessage ConvertDwg(string udid)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request parameters
            queryString["subscription-key"] = SubscriptionKey;
            queryString["api-version"] = ApiVersion;
            queryString["udid"] = udid;
            queryString["inputType"] = "DWG";

            var uri = BaseUrl + "/conversion/convert?" + queryString;

            HttpResponseMessage response;
            response = client.PostAsync(uri, null)?.Result;
            if (response?.StatusCode != HttpStatusCode.Accepted)
            {
                throw new HttpRequestException("Failed in ConvertDwg for id: " + Parking.Id);
            }

            Console.WriteLine("DWG Conversion started for id: " + Parking.Id);

            return response;
        }

        private string GetConversionStatus(string url)
        {
            var apiUrl = url.Replace("atlas.", "us.atlas.") + "&subscription-key=" + SubscriptionKey;
            var client = new HttpClient();
            var response = client.GetAsync(apiUrl)?.Result;
            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync()?.Result;
            dynamic data = JObject.Parse(responseBody);
            if (data.status == "Succeeded")
            {
                if (responseBody.Contains("warning"))
                {
                    Console.WriteLine("DWG conversion successful with warnings!");
                }
                else
                {
                    Console.WriteLine("DWG conversion successful for id: " + Parking.Id);
                }

                return data.resourceLocation;
            }

            if (data.status == "Failed")
            {
                Console.WriteLine("DWG conversion process failed for id: " + Parking.Id);
                throw new Exception(data.error);
            }

            Thread.Sleep(1000);
            return GetConversionStatus(url);
        }

        private string GetConversionId(string url)
        {
            return url.Replace("https://atlas.microsoft.com/conversion/", "").Replace("?api-version=1.0", "");
        }

        private HttpResponseMessage GenerateDataset(string conversionId)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request parameters
            queryString["subscription-key"] = SubscriptionKey;
            queryString["api-version"] = ApiVersion;
            queryString["conversionId"] = conversionId;
            queryString["type"] = "facility";

            var uri = BaseUrl + "/dataset/create?" + queryString;

            HttpResponseMessage response;
            response = client.PostAsync(uri, null)?.Result;
            if (response?.StatusCode != HttpStatusCode.Accepted)
            {
                throw new HttpRequestException("Request to generate dataset failed for id: " + Parking.Id);
            }

            Console.WriteLine("Dataset generation started for id: " + Parking.Id);

            return response;
        }

        private string GetDatasetStatus(string url)
        {
            var apiUrl = url.Replace("atlas.", "us.atlas.") + "&subscription-key=" + SubscriptionKey;
            var client = new HttpClient();
            var response = client.GetAsync(apiUrl)?.Result;
            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync()?.Result;
            dynamic data = JObject.Parse(responseBody);
            if (data.status == "Succeeded")
            {
                Console.WriteLine("Dataset generated successfully for id: " + Parking.Id);
                return data.resourceLocation;
            }

            if (data.status == "Failed")
            {
                Console.WriteLine("Dataset generation failed for id: " + Parking.Id);
                throw new Exception(data.error);
            }

            Thread.Sleep(1000);
            return GetDatasetStatus(url);
        }

        private Guid GetDatasetId(string url)
        {
            return url.Replace("https://atlas.microsoft.com/dataset/", "").Replace("?api-version=1.0", "").ToGuid();
        }

        private HttpResponseMessage GenerateTileset(Guid datasetId)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request parameters
            queryString["subscription-key"] = SubscriptionKey;
            queryString["api-version"] = ApiVersion;
            queryString["datasetId"] = datasetId.ToString();

            var uri = BaseUrl + "/tileset/create/vector?" + queryString;

            HttpResponseMessage response;
            response = client.PostAsync(uri, null)?.Result;
            if (response?.StatusCode != HttpStatusCode.Accepted)
            {
                throw new HttpRequestException("Request to generate tileset failed for id: " + Parking.Id);
            }

            Console.WriteLine("Tileset generation started for id: " + Parking.Id);

            return response;
        }

        private string GetTilesetStatus(string url)
        {
            var apiUrl = url.Replace("atlas.", "us.atlas.") + "&subscription-key=" + SubscriptionKey;
            var client = new HttpClient();
            var response = client.GetAsync(apiUrl)?.Result;
            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync()?.Result;
            dynamic data = JObject.Parse(responseBody);
            if (data.status == "Succeeded")
            {
                Console.WriteLine("Tileset generated successfully for id: " + Parking.Id);
                return data.resourceLocation;
            }

            if (data.status == "Failed")
            {
                Console.WriteLine("Tileset generation failed for id: " + Parking.Id);
                throw new Exception(data.error);
            }

            Thread.Sleep(100000);
            return GetTilesetStatus(url);
        }

        private Guid GetTilesetId(string url)
        {
            return url.Replace("https://atlas.microsoft.com/tileset/", "").Replace("?api-version=1.0", "").ToGuid();
        }
    }
}
