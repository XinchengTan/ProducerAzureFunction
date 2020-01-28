using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace ProducerAzure
{
    class Util {
        //private static readonly string LOCAL_FILEPATH = "/Users/shenhongyu/Desktop/producerConfig.json";
        private static string LOCAL_FILEPATH = "/Users/caratan/Desktop/COMP410/producerConfig.json";

        private static readonly ConfigToFieldsTranslator configToFieldsTranslator = new ConfigToFieldsTranslator();
        private static readonly FieldDataGeneratorFactory generatorFactory = new FieldDataGeneratorFactory();


        private static JObject LoadConfig(string? filePath)
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
            return jConfig;
        }

        public static FullConfig? ParseConfig(JObject jConfig)
        {
            List<FieldAttributes> fields = new List<FieldAttributes>();
            foreach (JObject fieldConfig in (JArray)jConfig["dimension_attributes"])
            {
                string typeID = (string)fieldConfig["type"];
                fields.Add(configToFieldsTranslator.CaseAt(typeID, fieldConfig));
            }

            // TESTING:
            try
            {
                int threads_count = (int)jConfig["threads_count"];
                int records_count = (int)jConfig["records_count"] < 2147483647 ? (int)jConfig["records_count"] : 2147483647;
                double error_rate = (double)jConfig["error_rate"];

                FullConfig fullConfig = new FullConfig(
                    threads_count,
                    records_count,
                    error_rate,
                    fields
                );
                return fullConfig;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Type casting error in thread or record or error_rate.");
                return null;
            }

        }


        public static IFieldDataGenerator MakeFieldDataGenerator(FieldAttributes f)
        {
            return generatorFactory.CaseAt(f.typeID, f);
        }


        public static T? GetValueOrNull<T>(Object obj) where T : struct
        {
            try
            {
                return (T) obj;
            }
            catch
            {
                //Console.WriteLine("Type casting error in Config file.");
                return null;
            }
        }
    }
}
