namespace ParkingDataSetup
{
    using Newtonsoft.Json.Linq;
    using ParkingDataSetup.Model;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Web;

    public class DWGZipUploader
    {
        private const string BaseUrl = "https://us.atlas.microsoft.com";

        private const string ApiVersion = "1.0";

        private const string SubscriptionKey = "Qx05yzZN1EVBesESLh1CCFiiUSRbccCB-d5LP-sqkxE";

        private const string BlueprintFilFormat = "zip";

        public Parking Parking { get; set; }

        public DWGZipUploader()
        {
            Parking = new Parking();
        }

        public void Run()
        {
            try
            {
                ValidateBlueprintFile();

                var httpResponseMessage = UploadDwg(Parking.Blueprint);

                GetUploadStatus(httpResponseMessage.Headers.Location.AbsoluteUri);

                Console.WriteLine("DWGZipUploader successfull for : " + Parking.Id);
            }
            catch (Exception ex)
            {
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

        private HttpResponseMessage GetUploadStatus(string url)
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
                return response;
            }

            if (data.status == "Failed")
            {
                Console.WriteLine("DWG upload failed for id: " + Parking.Id);
                throw new Exception(data.error);
            }

            Thread.Sleep(1000);
            return GetUploadStatus(url);
        }

    }
}
