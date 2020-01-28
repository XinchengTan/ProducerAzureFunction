using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace ProducerAzure
{
    public class Producer
    {

        public int Amount { get; private set; }

        private readonly IRecordGenerator recordGenerator;

        public Producer(int amount, List<FieldAttributes> fields)
        {
            this.Amount = amount;
            this.recordGenerator = new RecordGeneratorWithError(fields);

        }

        // Sends one data record
        public void SendRecord(IProducerToConsumerAdpt adpt, string uuid, string receiver_addr, ILogger log)
        {
            JObject record = recordGenerator.GenerateRecord();
            log.LogInformation($"[Producer] Generated Data Record: {record}");
            //adpt.Send(record, uuid, receiver_addr); //TODO: Uncomment me to test connection with consumer!
            this.Amount --;
        }

        // Sends all data records required
        public void SendAllRecords(IProducerToConsumerAdpt adpt, string uuid, string receiver_addr, ILogger log)
        {
            while(this.Amount != 0)
            {
                this.SendRecord(adpt, uuid, receiver_addr, log);
            }
        }
    }
}
