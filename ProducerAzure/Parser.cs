using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProducerAzure
{
    public class Parser
    {
        private static readonly string LOCAL_FILEPATH = "/Users/caratan/Desktop/Spring 2020/producerConfig.json";

        public static FullConfig ParseConfig(string? filePath)
        {
            Console.WriteLine($"Echoing input file path: {filePath}");

            if (String.IsNullOrEmpty(filePath))
            {
                // TODO: Throw exception
                Console.WriteLine("Got empty file path! Start trying default path...");
                filePath = LOCAL_FILEPATH;
            }

            using StreamReader stream = File.OpenText(filePath);
            using JsonTextReader reader = new JsonTextReader(stream);
            JObject jConfig = (JObject)JToken.ReadFrom(reader);
            //ConfigToFieldsTranslator parser = new ConfigToFieldsTranslator();
            FullConfig cfg = Translate(jConfig);

            return cfg;
        }

        // Takes in the whole config JSON file as a JObject and returns a list of Fields
        public static FullConfig Translate(JObject config)
        {
            ConfigToFieldsTranslator translator = new ConfigToFieldsTranslator();
            List<FieldAttributes> fields = new List<FieldAttributes>();
            foreach (JObject fieldConfig in (JArray)config["dimension_attributes"])
            {
                string typeID = (string)fieldConfig["type"];
                fields.Add(translator.CaseAt(typeID, fieldConfig));
            }
            FullConfig fullConfig = new FullConfig(
                (int)config["threads_count"],
                (int)config["records_count"],
                (double)config["error_rate"],
                fields
                );

            return fullConfig;
        }
    }
}
