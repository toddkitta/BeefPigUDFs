using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Linq;

namespace BeefBodyWeightDetailPigUDF
{
    class Program
    {
        static void Main(string[] args)
        {
            string line;
            DateTime[] headerDates = null;
            while ((line = Console.ReadLine()) != null)
            {
                // see if this is the header row
                if (line.Contains("\"Ration\""))
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

                Console.WriteLine(String.Join(",", output));
            }
        }

        private static string[,] BuildOutput(InputPayload line, DateTime[] dates)
        {
            // 1) "Pen"
            // 2) "TRT"
            // 3) "Rep"
            // 4) "Ration"
            // 5) "ID"
            // 6) "Date"
            // 7) "Period"
            // 8) "ADG"
            string[,] results = new string[dates.Length, 8];

            // create a line for each date
            for(int i = 0; i < dates.Length; i++)
            {
                decimal? adg = null;

            }

            return results;
        }

        private static bool AreDatesContiguous(DateTime date1, DateTime date2)
        {
            return (date2.Date - date1.Date).Days == 1;
        }

        private static DateTime[] ParseHeaderDates(string line)
        {
            var fields = GetFields(line);
            return fields.Skip(8).Select(s => DateTime.Parse(s)).ToArray();
        }

        private static InputPayload GetPayloadFromLine(string line)
        {
            var fields = GetFields(line);

            var payload = new InputPayload()
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
