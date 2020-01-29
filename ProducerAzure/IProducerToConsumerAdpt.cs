using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ProducerAzure
{
    public interface IProducerToConsumerAdpt
    {
        void Send(JToken record, string configUUID, string host_addr);

    }

    public class ProducerToDefaultConsumerAddpt : IProducerToConsumerAdpt
    {
        public int receivedResNum = 0;

        public void Send(JToken record, string configUUID, string host_addr)
        {
            {
                try
                {
                    // Create a request using a URL that can receive a post.
                    WebRequest request = WebRequest.Create(host_addr);   
                    //WebRequest request = WebRequest.Create("http://10.209.177.26:7074/api/DataStorage");

                    // Set the Method property of the request to POST.  
                    request.Method = "POST";

                    // Create POST data and convert it to a byte array.
                    string postData = record.ToString();

                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    // Set the ContentType property of the WebRequest.  
                    request.ContentType = "application/json";

                    // Set the Config-GUID
                    request.Headers["Config-GUID"] = configUUID;

                    // Set the ContentLength property of the WebRequest.  
                    request.ContentLength = byteArray.Length;

                    // Get the request stream.  
                    Stream dataStream = request.GetRequestStream();
                    // Write the data to the request stream.  
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    // Close the Stream object.  
                    dataStream.Close();

                    // Get the response.  
                    WebResponse response = request.GetResponse();
                    // increment number of responses received
                    receivedResNum += 1;

                    // Display the status.  
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                    // Get the stream containing content returned by the server.  
                    // The using block ensures the stream is automatically closed.
                    using (dataStream = response.GetResponseStream())
                    {
                        // Open the stream using a StreamReader for easy access.  
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.  
                        string responseFromServer = reader.ReadToEnd();
                        // Display the content.  
                        Console.WriteLine(responseFromServer);
                    }

                    // Close the response.  
                    response.Close();

                }
                catch (WebException webExcp)
                {
                    // clear number of responses received
                    receivedResNum = 0;

                    // If you reach this point, an exception has been caught.  
                    Console.WriteLine("A WebException has been caught.");
                    // Write out the WebException message.  
                    Console.WriteLine(webExcp.ToString());
                    // Get the WebException status code.  
                    WebExceptionStatus status = webExcp.Status;
                    // If status is WebExceptionStatus.ProtocolError,   
                    // there has been a protocol error and a WebResponse   
                    // should exist. Display the protocol error.  
                    if (status == WebExceptionStatus.ProtocolError)
                    {
                        Console.Write("The server returned protocol error ");
                        // Get HttpWebResponse so that you can check the HTTP status code.  
                        HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
                        Console.WriteLine((int)httpResponse.StatusCode + " - "
                           + httpResponse.StatusCode);
                    }
                }
                catch (Exception e)
                {
                    // Code to catch other exceptions goes here.  
                }


            }
        }
    }
}
