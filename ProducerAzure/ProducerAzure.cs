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
using TestInstrumentation;
using System.Text;


namespace ProducerAzure
{
    public static class ProducerAzureController
    {
        static CustomLogger customLog = new CustomLogger("Producer_TeamA");

        // TODO: 3. Add "How to run" instruction on documentation!
        // TODO: 4. Analysis?

        [FunctionName("ProducerAzure")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            customLog.RawLog(LogLevel.INFO, "ProducerAzure HTTP trigger function start to process a request...");

            // Get Configuration json
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic httpData = JsonConvert.DeserializeObject(requestBody);
            JObject jConfigData = (JObject)httpData;


            // Parse Configuration json
            string? cfgName = (string)jConfigData.GetValue("config_name");
            if (cfgName == null) {
                customLog.RawLog(LogLevel.WARN, "[ProducerAzure] Got empty Config Name, using default name");
                cfgName = "Default Config";
            }
            string configHostAddr = (string)jConfigData.GetValue("config_url");
            if (configHostAddr == null)
            {
                return new BadRequestObjectResult(
                    "[ProducerAzure] Please specify consumer CONFIG endpoint in the config input!");
            }
            string dataHostAddr = (string)jConfigData.GetValue("data_url");
            if (dataHostAddr == null)
            {
                return new BadRequestObjectResult(
                    "[ProducerAzure] Please specify consumer DATA endpoint in the config input!");
            }
            // NOTE: consumer behavior differs only in whether it sends back analysis
            string team = (string)jConfigData.GetValue("consumer_team");
            if (team == null)
            {
                return new BadRequestObjectResult(
                    "[ProducerAzure] Please specify WHICH consumer you are procuding to!");
            }
            string analysisHostAddr = (string)jConfigData.GetValue("analysis_url");
            if (analysisHostAddr == null & team == "A")
            {
                return new BadRequestObjectResult(
                    "[ProducerAzure] Please specify consumer ANALYSIS endpoint in the config input!");
            }

            JObject consumerCfg = (JObject)jConfigData.GetValue("consumer_config");
            if (consumerCfg == null)
            {
                return new BadRequestObjectResult(
                    "[ProducerAzure] Consumer config cannot be null!");
            }
            JObject producerCfg = (JObject)jConfigData.GetValue("producer_config");
            FullConfig? parsedProducerCfg = Util.ParseConfig(producerCfg);
            if (parsedProducerCfg == null)
            {
                return new BadRequestObjectResult(
                    "[ProducerAzure] Incorrect Producer config format!");
            }
            customLog.RawLog(LogLevel.INFO, $"[ProducerAzure] Successfully parsed configuration: {cfgName}");


            // Send ConsumerConfig as JSON string
            string configUUID = Guid.NewGuid().ToString();
            
            string configResponse = MessageSender.SendConsumerConfig(configUUID, consumerCfg.ToString(), configHostAddr, customLog);
            customLog.RawLog(LogLevel.INFO, $"[ProducerAzure] Config Response: {configResponse}");


            // Producer generates data and send to consumer
            customLog.RawLog(LogLevel.INFO, "[ProducerAzure] Start to generate data and send to consumer...");
            MessageSender.ProduceAndSend(configUUID, parsedProducerCfg.Value, dataHostAddr, customLog);


            // Producer sends end signal
            string? analysisResponse = null;
            if (team == "A") {
                analysisResponse = MessageSender.SendEndSignal(configUUID, analysisHostAddr, customLog);
                customLog.RawLog(LogLevel.INFO, $"[ProducerAzure] End Signal Response: {analysisResponse}");
            }

            return (cfgName != null & dataHostAddr != null & configHostAddr != null)
                ? (ActionResult)new OkObjectResult(
                    Util.FormatProducerResult(configUUID, cfgName, jConfigData.ToString(), analysisResponse))
                : new BadRequestObjectResult(
                    $"[ProducerAzure] Got exception during data transfer!");
        }
    }





}
