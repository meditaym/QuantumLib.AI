using QuantumLib.AI.COMMON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumLib.AI.DIFFRACTION.DATA
{
    public class QDiffractionStruct
    {
        public List<QDiffractionPoint> ListOfDiffPoints { get; set; } = new List<QDiffractionPoint>();
        public int InputIndex { get; set; }
        public int OutputIndex { get; set; }
        public override string ToString()
        {
            string value = "OutInd:" + OutputIndex  + "\tInInd:" + InputIndex;
            ListOfDiffPoints.ForEach(x => value += "\r\n\t" + QCommons.InsertTabToText(x.ToString()));
            return value;

        }
    }
}
