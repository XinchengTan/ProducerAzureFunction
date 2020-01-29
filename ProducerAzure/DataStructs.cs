using System;
using System.Collections.Generic;

namespace ProducerAzure
{
    // Same as DimentionAttribute
    public struct FieldAttributes
    {
        public readonly string name;
        public readonly string typeID;
        public readonly FieldParam param;

        public FieldAttributes(string name, string typeID, FieldParam param)
        {
            this.name = name;
            this.typeID = typeID;
            this.param = param;
        } 
    }

    public struct FieldParam
    {
        public double? mean;
        public double? standard_deviation;
        public int? max_len;

        // TODO: add all possible args
    }


    public struct FullConfig
    {
        public int threads_count { get; }
        public int records_count { get; }
        public double error_rate { get; }
        public List<FieldAttributes> fields;

        public FullConfig(int threads, int records, double err_rate, List<FieldAttributes> fields)
        {
            this.threads_count = threads;
            this.records_count = records;
            this.error_rate = err_rate;
            this.fields = fields;
        }
    } 

}
