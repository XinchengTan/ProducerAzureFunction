using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;


namespace ProducerAzure
{
    public static class ProducerAzure
    {
        [FunctionName("ProducerAzure")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ProducerAzure HTTP trigger function processing a request.");

            // Get the content of the request (i.e. Configuration)
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic httpData = JsonConvert.DeserializeObject(requestBody);
            JObject jConfigData = (JObject)httpData;
            
            string cfgName = (string)jConfigData.GetValue("config_name");
            if (cfgName == null) {
                return new BadRequestObjectResult("[ProducerAzure] Please specify a name in the config input!");
            }
            string configHostAddr = (string)jConfigData.GetValue("config_host_addr");
            if (configHostAddr == null) {
                return new BadRequestObjectResult("[ProducerAzure] Please specify consumer CONFIG endpoint in the config input!");
            }
            string dataHostAddr = (string)jConfigData.GetValue("data_host_addr");
            if (dataHostAddr == null)
            {
                return new BadRequestObjectResult("[ProducerAzure] Please specify consumer DATA endpoint in the config input!");
            }
            
            // TODO: Check null & other exceptions below!
            JObject producerCfg = (JObject)jConfigData.GetValue("producer_config");
            JObject consumerCfg = (JObject)jConfigData.GetValue("consumer_config");
            log.LogInformation($"Parsed configuration '{cfgName}'");

            // Send ConsumerConfig as JSON string
            string configUUID = Guid.NewGuid().ToString();
            // TODO: Try catch exception??
            //HttpWebResponse configResponse = Controller.SendConsumerConfig(configUUID, consumerCfg.ToString(), configHostAddr);

            //// Display the status from consumer
            //Console.WriteLine(configResponse.StatusDescription);

            // Get the stream containing content returned by the server.  
            // The using block ensures the stream is automatically closed.
            string responseFromServer = "Default Consumer Response";
            //using (Stream configDataStream = configResponse.GetResponseStream())
            //{
            //    // Open the stream using a StreamReader for easy access.  
            //    StreamReader reader = new StreamReader(configDataStream);
            //    // Read the content.  
            //    responseFromServer = reader.ReadToEnd();
            //}
            //configResponse.Close();

            // Producer generates data and send to consumer
            Controller.ProduceAndSend(configUUID, producerCfg, dataHostAddr);

            return (cfgName != null & dataHostAddr != null & configHostAddr != null)
                ? (ActionResult)new OkObjectResult(
                    $"[ProducerAzure] Received config: {jConfigData.ToString()}\n****************\n" +
                    $"Consumer response: {responseFromServer}")
                : new BadRequestObjectResult(
                    $"[ProducerAzure] Error! Consumer Response: {responseFromServer}"
                    );
        }
    }

    public class Controller
    {
        private static string LOCAL_HOST = "http://localhost:7071/api/ProducerAzure";

        public static void ProduceAndSend(string configUUID, JObject jProducerConfig, string host_addr)
        {
            FullConfig parsed_config = Parser.Translate(jProducerConfig);
            Producer producer = new Producer(parsed_config.records_count, parsed_config.fields);

            // Generate and Send Data Records
            host_addr = host_addr ?? LOCAL_HOST;
            for (int counter = parsed_config.records_count; counter > 0; counter--)
            {
                try
                {
                    producer.SendAllRecords(new ProducerToDefaultConsumerAddpt(), configUUID, host_addr);
                }
                catch (WebException webExcp)
                {
                    Console.WriteLine($"Got WebException while sending {counter}th record!");
                }
            };

        }


        public static HttpWebResponse SendConsumerConfig(string configUUID, string configData, string host_addr) {

            // Create a request using a URL that can receive a post.
            WebRequest configRequest = WebRequest.Create(host_addr);
            configRequest.Method = "POST";

            // Convert POST data to a byte array.
            byte[] byteConfigArray = Encoding.UTF8.GetBytes(configData);

            // Set the ContentType property of the WebRequest.
            configRequest.ContentType = "application/json";

            // Set the Config-GUID header
            configRequest.Headers["Config-GUID"] = configUUID;

            // Set the ContentLength property of the WebRequest.  
            configRequest.ContentLength = byteConfigArray.Length;

            // Get the request stream.  
            Stream configDataStream = configRequest.GetRequestStream();

            // Write the data to the request stream.  
            configDataStream.Write(byteConfigArray, 0, byteConfigArray.Length);

            // Close the Stream object.  
            configDataStream.Close();

            // Get the response.  
            HttpWebResponse consumerResponse = (HttpWebResponse)configRequest.GetResponse();

            return consumerResponse;
        }
    }

}
