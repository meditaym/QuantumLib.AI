using QuantumLib.AI.COMMON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumLib.AI.DIFFRACTION.DATA
{
    public class QDiffractionPoint
    {
        public QPoint Point1 { get; set; }
        public QPoint Point2 { get; set; }
        public bool IsRegular { get; set; }
        public int BeginIndex { get; set; }
        public int EndIndex { get; set; }
        public QDiffractionStruct RegularizationDiffractionStruct { get; set; }

        public override string ToString()
        {
            string value = "Beg.Ind.:"+BeginIndex+"\tEndInd.:"+EndIndex+"\t"+"REG.:"+IsRegular+"\t"+(Point1!=null? "P1_1:"+Point1._1+" P1_2:"+Point1._2:"")+"\t"+ (Point2 != null ? "P2_1:" + Point2._1 + " P2_2:" + Point2._2 : "");
            value += (RegularizationDiffractionStruct != null ? "\r\n\t" + QCommons.InsertTabToText(RegularizationDiffractionStruct.ToString()) : "");
            return value;
        }

    }
}
