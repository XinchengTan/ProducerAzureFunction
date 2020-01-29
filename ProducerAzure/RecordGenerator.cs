using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ProducerAzure
{
    public interface IRecordGenerator
    {
        JObject GenerateRecord();
    }



    public class RecordGenerator : IRecordGenerator
    {
        private readonly List<IFieldDataGenerator> fieldDataGens;

        public RecordGenerator(List<FieldAttributes> fields)
        {
            List<IFieldDataGenerator> gens = new List<IFieldDataGenerator>();
            foreach (FieldAttributes field in fields)
            {
                gens.Add(Util.MakeFieldDataGenerator(field));
            }
            this.fieldDataGens = gens;
        }


        public JObject GenerateRecord()
        {
            JObject record = new JObject();
            foreach (IFieldDataGenerator gen in fieldDataGens)
            {
                string field_name = gen.GetFieldName();
                record.Add(field_name, gen.GenerateFieldData());
                //Console.WriteLine($"Field_name: {field_name}, value: {record[field_name]}");
            }

            return record;
        }

    }

    public abstract class AErrorGenerator : IRecordGenerator
    {
        private static readonly double DEFULT_ERROR_RATE = 0;

        public IRecordGenerator Generator;
        public double ErrorRate { get; private set; }

        protected readonly Random random = new Random();

        public AErrorGenerator(IRecordGenerator generator, double errorRate)
        {
            this.Generator = generator;
            this.ErrorRate = errorRate;
        }

        public AErrorGenerator(IRecordGenerator generator) : this(generator, DEFULT_ERROR_RATE) { }

        public JObject GenerateRecord()
        {
            JObject rec = Generator.GenerateRecord();
            //Console.WriteLine($"Record: {rec.ToString()}");
            if (random.NextDouble() < this.ErrorRate)
            {
                return ApplyError(rec);
            }
            else
            {
                return rec;
            }
        }

        protected abstract JObject ApplyError(JObject record);
    }


    public class WrongTypeErrorGenerator : AErrorGenerator
    {
        public WrongTypeErrorGenerator(IRecordGenerator gen) : base(gen) { }

        public WrongTypeErrorGenerator(IRecordGenerator gen, double rate) : base(gen, rate) { }

        protected override JObject ApplyError(JObject record)
        {
            var e = record.Properties().GetEnumerator();
            if (e.MoveNext())
            {
                JProperty property = e.Current;
                if (this.random.NextDouble() < 0.5)
                {
                    record[property.Name] = null;
                }
                else
                {
                    JValue newValue = property.Value.Type == JTokenType.String ? new JValue(0) : new JValue("wrong type data");
                    record[property.Name] = newValue;
                }
            }

            return record;

        }
    }

    public class AdditionalFieldErrorGenerator : AErrorGenerator
    {
        public AdditionalFieldErrorGenerator(IRecordGenerator gen) : base(gen) { }

        public AdditionalFieldErrorGenerator(IRecordGenerator gen, double rate) : base(gen, rate) { }

        protected override JObject ApplyError(JObject record)
        {
            record.Add("error", "This is an additional field");
            return record;
        }
    }

    //public class NullErrorGenerator : AErrorGenerator
    //{
    //    public NullErrorGenerator(IRecordGenerator gen) : base(gen) { }

    //    protected override JObject ApplyError(JObject record)
    //    {
    //        var e = record.Properties().GetEnumerator();
    //        if (e.MoveNext())
    //        {
    //            JProperty property = e.Current;
    //            record.Remove(property.Name);
    //            record[property.Name] = null;
    //        }

    //        return record;
    //    }
    //}

    public class MissingFieldErrorGenerator : AErrorGenerator
    {

        public MissingFieldErrorGenerator(IRecordGenerator gen) : base(gen) { }

        public MissingFieldErrorGenerator(IRecordGenerator gen, double rate) : base(gen, rate) { }

        protected override JObject ApplyError(JObject record)
        {
            var e = record.Properties().GetEnumerator();
            if (e.MoveNext())
            {
                JProperty property = e.Current;
                record.Remove(property.Name);
            }

            return record;
        }
    }
}
