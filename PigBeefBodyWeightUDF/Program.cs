using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace PigBeefBodyWeightUDF
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string line;
            DateTime[] headerDates = null;
            while ((line = Console.ReadLine()) != null)
            {
                // see if this is the header row
                if (line.Contains("\"Weight\""))
                {
                    headerDates = ParseHeaderDates(line);
                    continue;
                }

                var payload = GetPayloadFromLine(line);
                if (headerDates.Length != payload.Weights.Length)
                {
                    // well, that's weird
                    continue;
                }

                var output = BuildOutput(payload, headerDates);

                //Console.WriteLine(String.Join("\t", field));
            }
        }

        private static string[] BuildOutput(BodyWeightPayload line, DateTime[] dates)
        {
            decimal? iwt = null;
            decimal? fwt = null;

            // check if the first two dates are contiguous
            if (dates[1].Subtract(dates[0]).Days == 1)
            {
                iwt = line.Weights.Take(2).Average();
            }
            else
            {
                iwt = line.Weights[0];
            }

            // check if the last two dates are contiguous
            if (dates.Last().Subtract(dates[dates.Length - 2]).Days == 1)
            {
                fwt = line.Weights.Skip(line.Weights.Length - 2).Average();
            }
            else
            {
                fwt = line.Weights.Last();
            }

            return null;
        }

        private static DateTime[] ParseHeaderDates(string line)
        {
            var fields = GetFields(line);
            return fields.Skip(8).Select(s => DateTime.Parse(s)).ToArray();
        }

        private static BodyWeightPayload GetPayloadFromLine(string line)
        {
            var fields = GetFields(line);

            var payload = new BodyWeightPayload()
            {
                StudyStartDate = DateTime.Parse(fields[0]),
                StudyID = fields[1],
                Pen = fields[2],
                TRT = fields[3],
                Rep = fields[4],
                Ration = fields[5],
                ID = fields[6],
                Weight = Decimal.Parse(fields[7]),
                Weights = fields.Skip(8).Select(s => Decimal.Parse(s)).ToArray()
            };

            return payload;
        }

        private static string[] GetFields(string line)
        {
            var parser = new TextFieldParser(new StringReader(line))
            {
                HasFieldsEnclosedInQuotes = true
            };

            parser.SetDelimiters(",");

            try
            {
                string[] fields = null;
                if (!parser.EndOfData)
                    fields = parser.ReadFields();

                return fields;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                parser.Close();
            }
        }
    }
}
