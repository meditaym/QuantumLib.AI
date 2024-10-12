using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumLib.AI.COMMON.DATA
{
    public class QFoldVectorizedData
    {
        public List<List<decimal>> data { get; set; }
        public decimal OutputValue { get; set; }
        public bool IsUse { get; set; }
    }
}
