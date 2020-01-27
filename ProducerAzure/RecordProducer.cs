using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ProducerAzure
{
    public interface IRecordGenerator
    {
        JObject GenerateRecord();
    }

    public class RecordGeneratorWithError : IRecordGenerator
    {
        private readonly IRecordGenerator gen;

        public RecordGeneratorWithError(List<FieldAttributes> fields)
        {
            this.gen = new AdditionalFieldErrorGenerator
                (new MissingFieldErrorGenerator(
                    new WrongTypeErrorGenerator(
                        new RecordGenerator(fields))));
        }

        public JObject GenerateRecord()
        {
            return gen.GenerateRecord();
        }
    }


    public class RecordGenerator : IRecordGenerator
    {
        private readonly List<IFieldDataGenerator> fieldDataGens;

        public RecordGenerator(List<FieldAttributes> fields)
        {
            FieldDataGeneratorFactory factory = new FieldDataGeneratorFactory();
            List<IFieldDataGenerator> gens = new List<IFieldDataGenerator>();
            foreach (FieldAttributes field in fields)
            {
                gens.Add(factory.CaseAt(field.typeID, field));
            }
            //fields.ForEach(field => factory.CaseAt(field.typeID, field));
            this.fieldDataGens = gens;
        }


        public JObject GenerateRecord()
        {
            JObject record = new JObject();
            foreach (IFieldDataGenerator gen in fieldDataGens)
            {
                string field_name = gen.GetFieldName();
                record[field_name] = gen.GenerateFieldData();
                //Console.WriteLine($"Field_name: {field_name}, value: {record[field_name]}");
            }
            //fieldDataGens.ForEach(generator => record[generator.GetFieldName()] = generator.GenerateFieldData());

            return record;
        }

    }

    public abstract class AErrorGenerator : IRecordGenerator
    {
        private static readonly double DEFULT_ERROR_RATE = 0.1;

        public IRecordGenerator Generator;
        public double ErrorRate { get; private set; }

        private readonly Random random = new Random();

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

    public class ErrorGenerator : AErrorGenerator
    {
        public ErrorGenerator(IRecordGenerator gen) :
            base(new AdditionalFieldErrorGenerator(new MissingFieldErrorGenerator(new WrongTypeErrorGenerator(gen))))
        { }

        protected override JObject ApplyError(JObject record)
        {
            return record;
        }
    }

    public class WrongTypeErrorGenerator : AErrorGenerator
    {
        public WrongTypeErrorGenerator(IRecordGenerator gen) : base(gen) { }

        protected override JObject ApplyError(JObject record)
        {
            // TODO: Debug me! property is null
            //JProperty property = record.Properties().GetEnumerator().Current;
            //string name = property.Name;
            //JTokenType valueType = property.Value.Type;
            //if (valueType == JTokenType.String)
            //{
            //    record.Add(name, new JValue(0));
            //} else
            //{
            //    record.Add(name, new JValue("wrong type data"));
            //}
            return record;

        }
    }

    public class AdditionalFieldErrorGenerator : AErrorGenerator
    {
        public AdditionalFieldErrorGenerator(IRecordGenerator gen) : base(gen) { }

        protected override JObject ApplyError(JObject record)
        {
            record.Add("error", "This is an additional field");
            return record;
        }
    }

    public class MissingFieldErrorGenerator : AErrorGenerator
    {

        public MissingFieldErrorGenerator(IRecordGenerator gen) : base(gen) { }

        protected override JObject ApplyError(JObject record)
        {
            JProperty property = record.Properties().GetEnumerator().Current;
            record.Remove(property.Name);
            return record;
        }
    }
}
