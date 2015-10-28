using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeefBodyWeightSummaryPigUDF
{
    public struct InputPayload
    {
        public string StudyID { get; set; }
        public DateTime StudyStartDate { get; set; }
        public string Pen { get; set; }
        public string TRT { get; set; }
        public string Rep { get; set; }
        public string Ration { get; set; }
        public string ID { get; set; }
        public decimal Weight { get; set; }
        public decimal[] Weights{ get; set; }
    }
}
