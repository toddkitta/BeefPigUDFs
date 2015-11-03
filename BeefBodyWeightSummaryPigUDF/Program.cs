using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Linq;

namespace BeefBodyWeightSummaryPigUDF
{
    public class Program
    {
        //public static void Main(string[] args)
        //{
        //    foreach (var file in Directory.GetFiles(@"C:\LOL Data\Weight", "*.csv"))
        //    {
        //        string fullLineFromFile;
        //        DateTime[] headerDates = null;

        //        StreamReader sr = new StreamReader(file);
        //        while ((fullLineFromFile = sr.ReadLine()) != null)
        //        //while ((fullLine = Console.ReadLine()) != null)
        //        {
        //            string fullAzurePath = "wasb://hdidata@panctest.blob.core.windows.net/Weight/" + Path.GetFileName(file);
        //            string fullLine = fullAzurePath + "\t" + fullLineFromFile;

        //            string[] fullLineParts = fullLine.Split('\t');
        //            string studyId = GetStudyId(fullLineParts[0]);
        //            string line = fullLineParts[1];

        //            // see if this is the header row
        //            if (line.ToLower().Contains("ration"))
        //            {
        //                headerDates = ParseHeaderDates(line);
        //                continue;
        //            }

        //            var payload = GetPayloadFromLine(line);
        //            if (headerDates.Length != payload.Weights.Length)
        //            {
        //                // well, that's weird
        //                continue;
        //            }

        //            var output = BuildOutput(payload, headerDates);

        //            // TODO: incorporate study into payload
        //            Console.WriteLine(studyId + "\t" + String.Join("\t", output));
        //        }
        //    }
        //}

        public static void Main(string[] args)
        {
            string fullLine;
            DateTime[] headerDates = null;
            while ((fullLine = Console.ReadLine()) != null)
            {
                string[] fullLineParts = fullLine.Split('\t');
                string studyId = GetStudyId(fullLineParts[0]);
                string line = fullLineParts[1];

                // see if this is the header row
                if (line.ToLower().Contains("ration"))
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

                // TODO: incorporate study into payload
                Console.WriteLine(studyId + "\t" + String.Join("\t", output));
            }
        }

        private static string GetStudyId(string filePath)
        {
            return filePath.Split('/').Last().Split('.').First();
        }

        private static string[] BuildOutput(InputPayload line, DateTime[] dates)
        {
            // 1) "Pen"
            // 2) "TRT"
            // 3) "Rep"
            // 4) "Ration"
            // 5) "ID"
            // 6) "IWT"
            // 7) "FWT"
            // 8) "ADG"
            string[] results = new string[8];
            results[0] = line.Pen;
            results[1] = line.TRT;
            results[2] = line.Rep;
            results[3] = line.Ration;
            results[4] = line.ID;

            decimal? iwt = null;
            decimal? fwt = null;
            decimal? adg = null;

            if (dates.Length > 1)
            {
                // calculate IWT
                // check if the first two dates are contiguous
                if (AreDatesContiguous(dates[0], dates[1]))
                {
                    iwt = line.Weights.Take(2).Average();
                }
                else
                {
                    iwt = line.Weights[0];
                }

                // calculate FWT
                // check if the last two dates are contiguous
                if (AreDatesContiguous(dates[dates.Length - 2], dates[dates.Length - 1]))
                {
                    fwt = line.Weights.Skip(line.Weights.Length - 2).Average();
                }
                else
                {
                    fwt = line.Weights.Last();
                }

                // calculate ADG
                int daysInStudy = (dates[dates.Length - 1].Date - dates[0].Date).Days;
                decimal gain = line.Weights[line.Weights.Length - 1] - line.Weights[0];
                if (daysInStudy > 0)
                    adg = gain / daysInStudy;
            }

            results[5] = iwt.HasValue ? iwt.Value.ToString() : "0";
            results[6] = fwt.HasValue ? fwt.Value.ToString() : "0";
            results[7] = adg.HasValue ? adg.Value.ToString() : "0";

            return results;
        }

        private static bool AreDatesContiguous(DateTime date1, DateTime date2)
        {
            return Math.Abs((date2.Date - date1.Date).Days) == 1;
        }

        private static DateTime[] ParseHeaderDates(string line)
        {
            var fields = GetFields(line);
            return fields.Skip(5).Select(s => DateTime.Parse(s)).ToArray();
        }

        private static InputPayload GetPayloadFromLine(string line)
        {
            var fields = GetFields(line);

            var payload = new InputPayload()
            {
                Pen = fields[0],
                TRT = fields[1],
                Rep = fields[2],
                Ration = fields[3],
                ID = fields[4],
                Weights = fields.Skip(5).Select(s => !String.IsNullOrWhiteSpace(s) ? Decimal.Parse(s) : 0).ToArray()
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
