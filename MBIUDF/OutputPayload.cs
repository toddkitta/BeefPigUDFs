using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBIUDF
{
    public class OutputPayload
    {
        public string Tag { get; set; }
        public string TRT { get; set; }
        public string Rep { get; set; }
        public DateTime Date { get; set; }
        public bool HasMBI { get; set; }
    }
}
