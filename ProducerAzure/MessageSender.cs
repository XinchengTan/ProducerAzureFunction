using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using TestInstrumentation;
using System.Text;

namespace ProducerAzure
{
    public class MessageSender
    {
        private static string LOCAL_HOST = "http://localhost:7071/api/ProducerAzure";

        public MessageSender() {}

        public static void ProduceAndSend(string configUUID, FullConfig producerConfig, string host_addr, CustomLogger log)
        {
            Producer producer = new Producer(producerConfig.records_count, producerConfig.fields);

            // Generate and Send Data Records
            host_addr = host_addr ?? LOCAL_HOST;
            ProducerToDefaultConsumerAddpt adapter = new ProducerToDefaultConsumerAddpt();
            for (int counter = producerConfig.records_count; counter > 0; counter--)
            {
                try
                {
                    producer.SendRecord(adapter, configUUID, host_addr, log);
                }
                catch (WebException webExcp)
                {
                    log.RawLog(LogLevel.ERROR, $"[ERROR] Got WebException {webExcp} while sending {counter}th record!");
                }
            };
        }

        public static string SendEndSignal(string configUUID, string host_addr, CustomLogger clog)
        {

            // Create a request using a URL that can receive a post.
            WebRequest analysisRequest = WebRequest.Create(host_addr);
            analysisRequest.Method = "GET";

            // Set the Config-GUID header
            analysisRequest.Headers["Config-GUID"] = configUUID;

            string analysisResponse = "[ProducerAzure] Default analysis response";
            try
            {
                HttpWebResponse endResponse = (HttpWebResponse)analysisRequest.GetResponse();
                if (endResponse.StatusCode != HttpStatusCode.OK)
                {
                    clog.RawLog(LogLevel.ERROR, "[ProducerAzure] Bad consumer analysis status");
                }
                using (Stream responseStream = endResponse.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access and read the content
                    analysisResponse = new StreamReader(responseStream).ReadToEnd();
                }
                //analysisResponse = endResponse.StatusDescription;
                endResponse.Close();
            }
            catch (WebException)
            {
                clog.RawLog(LogLevel.ERROR, "Got Web exception!");
            }
            return analysisResponse;
        }


        public static string SendConsumerConfig(string configUUID, string configData, string host_addr, CustomLogger clog)
        {

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
            string consumerResponse = "[SendConsumerConfig] Default Consumer Response";
            try
            {
                HttpWebResponse response = (HttpWebResponse)configRequest.GetResponse();

                using (Stream cfgDataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access & Read content.
                    consumerResponse = new StreamReader(cfgDataStream).ReadToEnd();
                    clog.RawLog(LogLevel.INFO, consumerResponse);
                }
                response.Close();
            }
            catch (WebException)
            {
                clog.RawLog(LogLevel.ERROR, "[SendConsumerConfig] Got WebException when sending consumer config!");
            }

            return consumerResponse;
        }
    }
}
