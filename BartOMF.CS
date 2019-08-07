using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BartApp
{
    public class BartOMF
    {
        private static readonly HttpClient client = new HttpClient();

        // The version of the OMFmessages
        static string omfVersion = "1.1";

        // Holders for parameters set by configuration
        static string producerToken;
        static string omfendpoint;
        static string checkBase;
        static string resource;
        static string pidataserver = "";
        static string username = "";
        static string password = "";

        //Holders for the data message values
        static Random rnd = new Random();
        static bool dynamicBoolHolder = false;
        static int dynamicIntHolder = 0;

        static string etdContainerName = "etdContainer";
        static string etdDataType = "etdDataType";

        public bool initializeConfigure(IConfiguration configuration)
        {
            if (configuration == null)
            {
                return false;
            }

            // Step 1
            resource = configuration["Resource"];
            producerToken = configuration["ProducerToken"];
            pidataserver = configuration["DataServerName"];
            username = configuration["UserName"];
            password = configuration["Password"];
            checkBase = resource;
            omfendpoint = checkBase + $"/omf";

            return true;
        }

        public bool createTypesAndContainers()
        {
            try
            {
                // create dynamic types
                sendEtdDynamicType();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            try
            {
                // create containers.
                sendContainers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public bool createDataValues(List<OMFBartETDSchedule> schedule)
        {
            //hold on to these in case there is a failure in deleting
            var success = true;

            try
            {
                OMFBartDataValues val = new OMFBartDataValues();
                val.containerid = etdContainerName;
                val.values = schedule;

                List<OMFBartDataValues> values = new List<OMFBartDataValues>();
                values.Add(val);
                sendValue("data", JsonConvert.SerializeObject(values));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Checks the last value of Container1 to see if it matches the incoming value
        /// </summary>
        /// <param name="value">last sent value to Container1</param>
        private static void CheckValues(string value)
        {
            Console.WriteLine("Checks");
            Console.WriteLine("Letting OMF get to data store");
            Thread.Sleep(10000);

            string json1 = checkValue(checkBase + $"/dataservers?name=" + pidataserver);
            JObject result = JsonConvert.DeserializeObject<JObject>(json1);
            string pointsURL = result.Value<JObject>("Links").Value<String>("Points");

            string json2 = checkValue(pointsURL + "?nameFilter=container1*");
            JObject result2 = JsonConvert.DeserializeObject<JObject>(json2);
            string EndValueUrl = result2.Value<JArray>("Items")[0].Value<JObject>("Links").Value<String>("EndValue");

            string json3 = checkValue(EndValueUrl);

            var valueJ = JsonConvert.DeserializeObject<List<JObject>>(value);
            var jsonJ = JsonConvert.DeserializeObject<JObject>(json3);

            if (valueJ[0]["values"][0]["IntegerProperty"].ToString() != jsonJ["Value"].ToString())
                throw new Exception("Returned value is not expected.");
        }

        /// <summary>
        /// Sends the values to the preconfigured endpoint
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="dataJson"></param>
        /// <param name="action"></param>
        private static void sendValue(string messageType, string dataJson, string action = "create")
        {
            WebRequest request = WebRequest.Create(new Uri(omfendpoint));
            request.Method = "post";
            //request.Headers.Add("producertoken", producerToken);
            request.Headers.Add("messagetype", messageType);
            request.Headers.Add("action", action);
            request.Headers.Add("messageformat", "json");
            request.Headers.Add("omfversion", omfVersion);
            request.Headers.Add("x-requested-with", "XMLHTTPRequest");
            //request.Credentials = new NetworkCredential(username, password);

            byte[] byteArray;

            request.ContentType = "application/x-www-form-urlencoded";
            byteArray = Encoding.UTF8.GetBytes(dataJson);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.  
            dataStream.Close();

            Send(request);
            Console.WriteLine("Success!");
        }


        private static string checkValue(string URL)
        {
            WebRequest request = WebRequest.Create(new Uri(URL));
            request.Method = "get";
            request.Headers.Add("x-requested-with", "XMLHTTPRequest");
            request.Credentials = new NetworkCredential(username, password);
            return Send(request);
        }

        /// <summary>
        /// Actual async call to send message to omf endpoint
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string Send(WebRequest request)
        {
            try
            {
                using (var resp = request.GetResponse())
                {
                    using (HttpWebResponse response = (HttpWebResponse)resp)
                    {
                        var stream = resp.GetResponseStream();
                        var code = (int)response.StatusCode;

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            // Read the content.  
                            string responseString = reader.ReadToEnd();
                            // Display the content.  

                            return responseString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }


        /// <summary>
        /// wrapper around definition of first dynamic type
        /// </summary>
        /// <param name="action"></param>
        public static void sendEtdDynamicType(string action = "create")
        {
            var val = String.Format(@"[{{
                ""id"": ""{0}"",
                ""classification"": ""dynamic"",
                ""type"": ""object"",
                ""properties"": {{
                    ""datetime"": {{
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""isindex"": true,
                        ""name"": ""name"",
                        ""description"": ""capture time""
                    }},
                    ""origName"": {{
                        ""type"": ""string"",
                        ""name"": ""origname"",
                        ""description"": ""original station name""
                    }},
                    ""origAbbr"": {{
                        ""type"": ""string"",
                        ""name"": ""origabbr"",
                        ""description"": ""original station name abbreviation""
                    }},
                    ""destName"": {{
                        ""type"": ""string"",
                        ""name"": ""destname"",
                        ""description"": ""destination station name""
                    }},
                    ""destAbbr"": {{
                        ""type"": ""string"",
                        ""name"": ""destabbr"",
                        ""description"": ""destination station name abbreviation""
                    }},
                    ""minutes"": {{
                        ""type"": ""number"",
                        ""name"": ""nextarrivalminutes"",
                        ""description"": ""next arrive minutes""
                    }},
                    ""platform"": {{
                        ""type"": ""integer"",
                        ""name"": ""platform"",
                        ""description"": ""platform number""
                    }},
                    ""direction"": {{
                        ""type"": ""string"",
                        ""name"": ""direction"",
                        ""description"": ""drive direction""
                    }},
                    ""length"": {{
                        ""type"": ""number"",
                        ""name"": ""length"",
                        ""description"": ""length""
                    }},
                    ""delay"": {{
                        ""type"": ""number"",
                        ""name"": ""delaytime"",
                        ""description"": ""delay time""
                    }}
                }}
            }}]", etdDataType);

            sendValue("type", val, action);
        }

        /// <summary>
        /// wrapper around definition of containers
        /// </summary>
        /// <param name="action"></param>
        public static void sendContainers(string action = "create")
        {
            var val = String.Format(@"[{{
                ""id"": ""{0}"",
                ""typeid"": ""{1}""
            }}]", etdContainerName, etdDataType);

            sendValue("container", val, action);
        }
    }
}
