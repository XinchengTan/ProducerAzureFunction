using NumSharp;
using Newtonsoft.Json.Linq;
using System.Text;
using System;

namespace ProducerAzure

{
    public interface IFieldDataGenerator
    {
        JValue GenerateFieldData();
        string GetFieldName();

    }


    public interface IDataGenerator<T> : IFieldDataGenerator
    {

        T Generate();

        new JValue GenerateFieldData();
    }


    public class DoubleDataGenerator : IDataGenerator<double>
    {
        private static readonly double DEFAULT_MEAN = 0.0;
        private static readonly double DEFAULT_STANDARD_DEVIATION = 20.0;

        public DoubleDataGenerator(string name, double? mean, double? standardDeviation) : this(name, mean.GetValueOrDefault(DEFAULT_MEAN), standardDeviation.GetValueOrDefault(DEFAULT_STANDARD_DEVIATION)) { }

        public DoubleDataGenerator(string name, double mean, double standardDeviation)
        {
            this.Name = name;
            this.Mean = mean;
            this.StandardDeviation = standardDeviation;
        }

        public double Mean { get; private set; }

        public double StandardDeviation { get; private set; }

        public string Name { get; private set; }

        public double Generate()
        {
            var data = np.random.normal(this.Mean, this.StandardDeviation, 1);
            return data[0];
        }

        public JValue GenerateFieldData()
        {
            double data = this.Generate();
            return new JValue(data);
        }

        public string GetFieldName()
        {
            return Name;
        }
    }


    public class IntegerDataGenerator : IDataGenerator<int>
    {
        private static readonly double DEFAULT_MEAN = 10.0;
        private static readonly double DEFAULT_STANDARD_DEVIATION = 10.0;

        public IntegerDataGenerator(string name, double? mean, double? standardDeviation) : this(name, mean.GetValueOrDefault(DEFAULT_MEAN), standardDeviation.GetValueOrDefault(DEFAULT_STANDARD_DEVIATION)) { }

        public IntegerDataGenerator(string name, double mean, double standardDeviation)
        {
            this.Name = name; 
            this.Mean = mean;
            this.StandardDeviation = standardDeviation;
        }

        public string Name { get; private set; }

        public double Mean { get; private set; }

        public double StandardDeviation { get; private set; }

        public int Generate()
        {
            var data = np.random.normal(this.Mean, this.StandardDeviation, 1);
            return (int)data[0];
        }

        public JValue GenerateFieldData()
        {
            int data = this.Generate();
            return new JValue(data);
        }

        public string GetFieldName()
        {
            return Name;
        }
    }

    public class StringDataGenerator : IDataGenerator<string>
    {
        private static readonly int DEFAULT_MAXLEN = 10;

        public StringDataGenerator(string name, int? maxlen) : this(name, maxlen.GetValueOrDefault(DEFAULT_MAXLEN)) { }

        public StringDataGenerator(string name, int maxlen)
        {
            this.Name = name;
            this.MaxLen = maxlen;
        }
        public string Name { get; private set; }
        public double MaxLen { get; private set; }

        public string Generate()
        {

            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int j = 0; j < this.MaxLen; j++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public JValue GenerateFieldData()
        {
            string data = this.Generate();
            return new JValue(data);
        }

        public string GetFieldName()
        {
            return Name;
        }
    }


}

