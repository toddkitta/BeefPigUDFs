using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeefBodyWeightDetailPigUDF
{
    public class OutputPayload
    {
        public string StudyID { get; set; }
        public string Pen { get; set; }
        public string TRT { get; set; }
        public string Rep { get; set; }
        public string Ration { get; set; }
        public string ID { get; set; }
        public DateTime Date { get; set; }
        public decimal Weight { get; set; }
        public string Period { get; set; }
        public int DaysInPeriod { get; set; }
        public int RunningDays { get; set; }
        public decimal ADG { get; set; }
    }
}
