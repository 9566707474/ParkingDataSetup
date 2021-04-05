namespace ParkingDataSetup
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class DWGZipUploader
    {
        #region snippet_HttpClient
        static HttpClient client = new HttpClient();
        #endregion

        ////#region snippet_CreateProductAsync
        ////static async Task<Uri> CreateProductAsync(Product product)
        ////{
        ////    HttpResponseMessage response = await client.PostAsJsonAsync(
        ////        "api/products", product);
        ////    response.EnsureSuccessStatusCode();

        ////    // return URI of the created resource.
        ////    return response.Headers.Location;
        ////}
        ////#endregion

        ////#region snippet_GetProductAsync
        ////static async Task<Product> GetProductAsync(string path)
        ////{
        ////    Product product = null;
        ////    HttpResponseMessage response = await client.GetAsync(path);
        ////    if (response.IsSuccessStatusCode)
        ////    {
        ////        product = await response.Content.ReadAsAsync<Product>();
        ////    }
        ////    return product;
        ////}
        ////#endregion

        ////static void Main()
        ////{
        ////    RunAsync().GetAwaiter().GetResult();
        ////}

        ////#region snippet_run
        ////#region snippet5
        ////static async Task RunAsync()
        ////{
        ////    client.BaseAddress = new Uri("https://us.atlas.microsoft.com/");
        ////    client.DefaultRequestHeaders.Accept.Clear();
        ////    client.DefaultRequestHeaders.Accept.Add(
        ////        new MediaTypeWithQualityHeaderValue("application/json"));
        ////    #endregion

        ////    try
        ////    {

        ////    }
        ////    catch (Exception e)
        ////    {
        ////        Console.WriteLine(e.Message);
        ////    }

        ////    Console.ReadLine();
        ////}
        ////#endregion

        public async Task<string> UploadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File [{filePath}] not found.");
            }

            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            ///fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            form.Add(fileContent, "file", Path.GetFileName(filePath));
            ////form.Add(new StringContent("789"), "userId");
            ////form.Add(new StringContent("some comments"), "comment");
            ////form.Add(new StringContent("true"), "isPrimary");

            var response = await client.PostAsync($"api/files", form);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
