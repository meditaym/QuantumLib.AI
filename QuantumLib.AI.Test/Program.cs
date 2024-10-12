using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System;
using QuantumLib.AI.DIFFRACTION;
using QuantumLib.AI.COMMON;

namespace QuantumLib.AI.Test
{
    internal class Program
    {

        static void Main(string[] args)
        {
            QDiffractionAnalysis analiz = new QDiffractionAnalysis(@"filePath", new List<int> { 0, 1, 2, 3 }, new List<int> { 4, 5, 6 });
            var incoming = analiz.DiffractionAnalys();
            incoming.PrintDiffractionsToScreen();
        }
    }
}
