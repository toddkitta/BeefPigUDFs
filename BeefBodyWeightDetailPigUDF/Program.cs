using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeefBodyWeightDetailPigUDF
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ProcessDataFromHadoop();
            //ProcessLocalFiles();
        }

        private static void ProcessLocalFiles()
        {
            foreach (var file in Directory.GetFiles(@"C:\LOL Data\Weight", "*.csv"))
            {
                string fullLineFromFile;

                DateTime[] headerDates = null;
                StreamReader sr = new StreamReader(file);
                while ((fullLineFromFile = sr.ReadLine()) != null)
                {
                    string fullAzurePath = "wasb://hdidata@panctest.blob.core.windows.net/Weight/" + Path.GetFileName(file);
                    string fullLine = fullAzurePath + "\t" + fullLineFromFile;

                    string studyId = null;
                    var output = ProcessData(fullLine, headerDates, out headerDates, out studyId);

                    if (output != null)
                    {
                        foreach (string[] lineValues in output)
                        {
                            // TODO: incorporate study into payload
                            Console.WriteLine(studyId + "\t" + String.Join("\t", lineValues));
                        }
                    }
                }
            }
        }

        private static void ProcessDataFromHadoop()
        {
            string fullLine;
            DateTime[] headerDates = null;
            while ((fullLine = Console.ReadLine()) != null)
            {
                string studyId = null;
                var output = ProcessData(fullLine, headerDates, out headerDates, out studyId);

                if (output != null)
                {
                    foreach (string[] lineValues in output)
                    {
                        // TODO: incorporate study into payload
                        Console.WriteLine(studyId + "\t" + String.Join("\t", lineValues));
                    }
                }
            }
        }

        private static string[][] ProcessData(string fullLine, DateTime[] headerDatesIn, out DateTime[] headerDatesOut, out string studyId)
        {
            string[] fullLineParts = fullLine.Split('\t');
            studyId = GetStudyId(fullLineParts[0]);
            string line = fullLineParts[1];

            // see if this is the header row
            if (line.ToLower().Contains("ration"))
            {
                headerDatesOut = ParseHeaderDates(line);
                return null;
            }
            else
            {
                // keep the existing dates, they are not new
                headerDatesOut = headerDatesIn;
            }

            var payload = GetPayloadFromLine(line);
            if (headerDatesOut.Length != payload.Weights.Length)
            {
                // well, that's weird
                return null;
            }

            var output = BuildOutput(payload, headerDatesOut);
            return output;
        }

        private static string GetStudyId(string filePath)
        {
            return filePath.Split('/').Last().Split('.').First();
        }

        private static string[][] BuildOutput(InputPayload line, DateTime[] dates)
        {
            // only process files with 1 or more dates
            if (dates.Length == 0)
                return null;

            List<OutputPayload> outputs = BuildOutputs(line, dates);

            if (dates.Length > 1)
            {
                // check if the first two dates are contiguous
                if (AreDatesContiguous(dates[0], dates[1]))
                {
                    // remove the first output row
                    outputs.RemoveAt(0);

                    // set the weight value of the first output to the first two weights in the input
                    outputs[0].Weight = line.Weights.Take(2).Average();
                }

                // check if the last two dates are contiguous
                if (AreDatesContiguous(dates[dates.Length - 2], dates[dates.Length - 1]))
                {
                    // remove the 2nd to last output row
                    outputs.RemoveAt(outputs.Count - 2);

                    // set the weight value of the last output to the last two weights in the input
                    outputs[outputs.Count - 1].Weight = line.Weights.Skip(line.Weights.Length - 2).Average();
                }
            }

            string[][] results = new string[outputs.Count][];
            for (int i = 0; i < outputs.Count; i++)
            {
                if (results[i] == null)
                    results[i] = new string[11];

                // get previous values
                DateTime? prevDate = i != 0 ? outputs[i - 1].Date : (DateTime?)null;
                int? prevRunningDays = i != 0 ? outputs[i - 1].RunningDays : (int?)null;
                decimal? prevWeight = i != 0 ? outputs[i - 1].Weight : (decimal?)null;

                // set Period, Running Days, and ADG
                outputs[i].Period = i.ToString();
                outputs[i].DaysInPeriod = prevDate.HasValue ? (outputs[i].Date - prevDate).Value.Days : 1;
                outputs[i].RunningDays = prevRunningDays.HasValue ? prevRunningDays.Value + outputs[i].DaysInPeriod : 1;
                outputs[i].ADG = prevWeight.HasValue ? (outputs[i].Weight - prevWeight.Value) / outputs[i].DaysInPeriod : 0;

                // 0) Pen
                // 1) Treatment
                // 2) Replication
                // 3) Ration
                // 4) Tag
                // 5) Date
                // 6) WT
                // 7) Period
                // 8) DaysInPeriod
                // 9) RunningDays
                // 10) ADG
                results[i][0] = line.Pen;
                results[i][1] = line.TRT;
                results[i][2] = line.Rep;
                results[i][3] = line.Ration;
                results[i][4] = line.ID;
                results[i][5] = outputs[i].Date.ToShortDateString();
                results[i][6] = outputs[i].Weight.ToString();
                results[i][7] = outputs[i].Period;
                results[i][8] = outputs[i].DaysInPeriod.ToString();
                results[i][9] = outputs[i].RunningDays.ToString();
                results[i][10] = outputs[i].ADG.ToString();
            }

            return results;
        }

        private static List<OutputPayload> BuildOutputs(InputPayload line, DateTime[] dates)
        {
            List<OutputPayload> outputs = new List<OutputPayload>();
            for (int i = 0; i < line.Weights.Length; i++)
            {
                var o = new OutputPayload()
                {
                    Pen = line.Pen,
                    TRT = line.TRT,
                    Rep = line.Rep,
                    Ration = line.Ration,
                    ID = line.ID,
                    Date = dates[i],
                    Weight = line.Weights[i]
                };
                outputs.Add(o);
            }
            return outputs;
        }

        private static bool AreDatesContiguous(DateTime date1, DateTime date2)
        {
            int diff = Math.Abs((date2.Date - date1.Date).Days);
            return diff == 1 || diff == 0;
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
