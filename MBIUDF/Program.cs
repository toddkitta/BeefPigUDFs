using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBIUDF
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessDataFromHadoop();
            //ProcessLocalFiles();
        }

        private static void ProcessDataFromHadoop()
        {
            string fullLine;

            string[] animals = null;
            string[] trts = null;
            string[] reps = null;

            while ((fullLine = Console.ReadLine()) != null)
            {
                var output = ProcessLine(fullLine, animals, trts, reps, out animals, out trts, out reps);

                if (output != null)
                {
                    foreach (string[] lineValues in output)
                    {
                        Console.WriteLine(String.Join("\t", lineValues));
                    }
                }
            }
        }

        private static void ProcessLocalFiles()
        {
            foreach (var file in Directory.GetFiles(@"D:\OneDrive - Microsoft\Customers\Land O Lakes\Reorganzied Data\MBI", "*.csv"))
            {
                string fullLineFromFile;

                string[] animals = null;
                string[] trts = null;
                string[] reps = null;

                StreamReader sr = new StreamReader(file);
                while ((fullLineFromFile = sr.ReadLine()) != null)
                {
                    string fullAzurePath = "wasb://hdidata@panctest.blob.core.windows.net/Weight/" + Path.GetFileName(file);
                    string fullLine = fullAzurePath + "\t" + fullLineFromFile;

                    var output = ProcessLine(fullLine, animals, trts, reps, out animals, out trts, out reps);

                    if (output != null)
                    {
                        foreach (string[] lineValues in output)
                        {
                            Console.WriteLine(String.Join("\t", lineValues));
                        }
                    }
                }
            }
        }

        private static string[][] ProcessLine(string fullLine, string[] animalsIn, string[] trtsIn, string[] repsIn, out string[] animalsOut, out string[] trtsOut, out string[] repsOut)
        {
            string[] fullLineParts = fullLine.Split('\t');
            string studyId = GetStudyId(fullLineParts[0]);
            string line = fullLineParts[1];

            if (line.ToLower().Contains("animal"))
            {
                animalsOut = ReadHeaderValues(line);
                trtsOut = trtsIn;
                repsOut = repsIn;
                return null;
            }
            else if (line.ToLower().Contains("trt"))
            {
                trtsOut = ReadHeaderValues(line);
                animalsOut = animalsIn;
                repsOut = repsIn;
                return null;
            }
            else if (line.ToLower().Contains("rep"))
            {
                repsOut = ReadHeaderValues(line);
                animalsOut = animalsIn;
                trtsOut = trtsIn;
                return null;
            }
            else
            {
                // we're not on a header row
                animalsOut = animalsIn;
                trtsOut = trtsIn;
                repsOut = repsIn;

                string[] rowValues = line.Split(',');

                if(String.IsNullOrWhiteSpace(rowValues[0]))
                    return null;

                string[][] output = new string[animalsIn.Length][];
                DateTime date = DateTime.Parse(rowValues[0]);
                string[] indicators = rowValues.Skip(1).ToArray();

                for (int i = 0; i < animalsIn.Length; i++)
                {
                    // 0) StudyID
                    // 1) Tag
                    // 2) TRT
                    // 3) Rep
                    // 4) Date
                    // 5) HasMBI
                    output[i] = new string[6];

                    output[i][0] = studyId;
                    output[i][1] = animalsIn[i];
                    output[i][2] = trtsIn[i];
                    output[i][3] = repsIn[i];
                    output[i][4] = date.ToShortDateString();
                    output[i][5] = (indicators[i] == "1").ToString().ToLower();
                }

                return output;
            }
        }

        private static string[] ReadHeaderValues(string fullLine)
        {
            string[] values = fullLine.Split(',');
            return values.Skip(1).ToArray();
        }

        private static string GetStudyId(string filePath)
        {
            return filePath.Split('/').Last().Split('.').First().Split('_').First();
        }
    }
}
