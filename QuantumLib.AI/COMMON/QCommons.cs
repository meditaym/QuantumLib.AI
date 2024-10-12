using QuantumLib.AI.DIFFRACTION.DATA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumLib.AI.COMMON
{
    public static class QCommons
    {
        public static string InsertTabToText(string value)
        {
            return value.Replace("\r\n", "\r\n\t");
        }

        public static void PrintDiffractionsToScreen(this List<QDiffractionStruct> datas)
        {
            datas.ForEach(x => Console.WriteLine("**************\r\n" + x + "\r\n**************\r\n"));
        }
    }
}
