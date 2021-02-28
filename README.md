# LicensePlateSimulator

This is the licenseplate simulator project. The code allows you to upload the license plates (also available in this project) to a service bus.

![image](https://user-images.githubusercontent.com/104528/109414681-76415780-79b4-11eb-8d49-36d5423f6f34.png)


The demo setup a shown in the picture above is as follows:
 - Create a strorage account with a container called images. Note that you need to create a v2 storage account that supports Event Grid.
 - Retrieve the connection string for the blob container.
 - Create a Computer Vision API instance in Azure.
 - Subsequently, create a Function App and function in the Azure Portal.
 - Lastly, create a service bus namespace and queue.

The function code is shown below (you can author the function in the portal or use VS Code/VS 2019):

#r "Newtonsoft.Json"
#r "System.Web"

using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

public static void Run(JObject eventGridEvent, ICollector<string> outputSbMsg, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    //intiliaze
    string imageInfo = string.Empty;
    
    //get content
    string jsonContent = eventGridEvent.ToString();

    log.Info($"Event : {jsonContent}");

    var imageUrl = eventGridEvent["data"]["url"].Value<string>();

    log.Info($"URL : {imageUrl}");

    //read image
    var webClient = new WebClient();
    byte[] image = webClient.DownloadData(imageUrl);

    //analyze image
    imageInfo = AnalyzeImage(image);

    //write to the console window
    log.Info(imageInfo);

    //output to service bus queue
    outputSbMsg.Add(imageInfo);
}

private static string AnalyzeImage(byte[] fileLocation) 
{
    var client = new HttpClient();
    var queryString = HttpUtility.ParseQueryString(string.Empty);
 
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ",<Subscription key of Face API>");
 
    queryString["language"] = "en";
    queryString["detectOrientation"] = "true";
    var uri = "https://westeurope.api.cognitive.microsoft.com/vision/v1.0/ocr?" + queryString;
    HttpResponseMessage response;
 
    using (var content = new ByteArrayContent(fileLocation)) {
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        response = client.PostAsync(uri, content).Result;
 
        string imageInfo = response.Content.ReadAsStringAsync().Result;
 
        return imageInfo;
    }
}

Note that you can also use another version of the vision API: 
 - V2.1: https://westus.dev.cognitive.microsoft.com/docs/services/5cd27ec07268f6c679a3e641/operations/56f91f2e778daf14a499f21b
 - v3.1: https://westcentralus.dev.cognitive.microsoft.com/docs/services/computer-vision-v3-1-ga/operations/5d986960601faab4bf452005



