using QuantumLib.AI.COMMON.DATA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumLib.AI.COMMON
{

    public static class QData
    {
        public static decimal[,] ReadFileDecimalArray(string path)
        {
            string[] lines = File.ReadAllLines(path);
            decimal[,] datas = new decimal[lines.Length, lines[0].Split(new[] { ",", "\t" }, StringSplitOptions.RemoveEmptyEntries).Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parsed = lines[i].Split(new[] { ",", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < parsed.Length; j++)
                {
                    datas[i, j] = decimal.Parse(parsed[j]);
                }
            }
            return datas;
        }
        public static List<List<decimal>> ReadFileDecimalList(string path)
        {
            string[] lines = File.ReadAllLines(path);
            List<List<decimal>> datas = new List<List<decimal>>();

            for (int i = 0; i < lines.Length; i++)
            {
                datas.Add(new List<decimal>(new decimal[lines[0].Split(new[] { ",", "\t" }, StringSplitOptions.RemoveEmptyEntries).Length].ToList()));
                string[] parsed = lines[i].Split(new[] { ",", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < parsed.Length; j++)
                {
                    datas[i][j] = Convert.ToDecimal(parsed[j].Replace(".", ","));
                }
            }
            return datas;
        }

        public static void PrintData(this List<List<decimal>> data)
        {
            foreach (var row in data)
            {
                foreach (var coulomb in row)
                {
                    Console.Write(coulomb + "\t");
                }
                Console.WriteLine();
            }
        }

        public static List<List<List<decimal>>> KFold(List<List<decimal>> datas, List<int> outputIndexes, int foldValue = 5)
        {
            List<QFoldData> dumpDatas = datas.Select(x => new QFoldData() { data = x, IsUse = false }).ToList();


            Random rnd = new Random();
            int luckyRow = 0;
            List<int>[] listOfOneOutputsDatas = new List<int>[outputIndexes.Count];
            for (int i = 0; i < outputIndexes.Count; i++)
            {
                listOfOneOutputsDatas[i] = new List<int>();
            }
            List<int> listOfNotUse = new List<int>();
            //List<int>[] listOfZeroOutputsDatas = new List<int>[outputIndexes.Count];

            for (int rowIndex = 0; rowIndex < dumpDatas.Count; rowIndex++)
            {
                for (int outputIndex = 0; outputIndex < outputIndexes.Count; outputIndex++)
                {
                    if (!dumpDatas[rowIndex].IsUse && dumpDatas[rowIndex].data[outputIndexes[outputIndex]] == 1)
                    {
                        listOfOneOutputsDatas[outputIndex].Add(rowIndex);
                        dumpDatas[rowIndex].IsUse = true;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < dumpDatas.Count; rowIndex++)
            {
                if (!dumpDatas[rowIndex].IsUse)
                {
                    listOfNotUse.Add(rowIndex);
                }
            }



            List<List<decimal>>[] Clusters = new List<List<decimal>>[foldValue];
            for (int i = 0; i < foldValue; i++)
            {
                Clusters[i] = new List<List<decimal>>();
            }
            for (int outputIndex = 0; outputIndex < outputIndexes.Count; outputIndex++)
            {
                int perSample_1 = (int)Math.Floor(listOfOneOutputsDatas[outputIndex].Count / (double)foldValue);

                for (int i = 0; i < foldValue; i++)
                {
                    for (int j = 0; j < perSample_1; j++)
                    {
                        luckyRow = rnd.Next(0, listOfOneOutputsDatas[outputIndex].Count);
                        Clusters[i].Add(dumpDatas[listOfOneOutputsDatas[outputIndex][luckyRow]].data);
                        listOfOneOutputsDatas[outputIndex].RemoveAt(luckyRow);
                    }
                }
                for (int i = 0; i < listOfOneOutputsDatas[outputIndex].Count; i++)
                {
                    Clusters[i].Add(dumpDatas[listOfOneOutputsDatas[outputIndex][i]].data);
                }
            }

            for (int outputIndex = 0; outputIndex < outputIndexes.Count; outputIndex++)
            {
                int perSample_1 = (int)Math.Floor(listOfNotUse.Count / (double)foldValue);

                for (int i = 0; i < foldValue; i++)
                {
                    for (int j = 0; j < perSample_1; j++)
                    {
                        luckyRow = rnd.Next(0, listOfNotUse.Count);
                        Clusters[i].Add(dumpDatas[listOfNotUse[luckyRow]].data);
                        listOfNotUse.RemoveAt(luckyRow);
                    }
                }
                for (int i = 0; i < listOfNotUse.Count; i++)
                {
                    Clusters[i].Add(dumpDatas[listOfNotUse[i]].data);
                }
            }


            return Clusters.ToList();
        }

        public static List<List<List<decimal>>> KFoldVectorizedData(List<List<decimal>> datas, int outputIndex, int vektorIndex, int foldValue = 5)
        {
            List<QFoldVectorizedData> dumpDatas = datas.GroupBy(x => x[vektorIndex]).Select(x => new QFoldVectorizedData() { data = x.Select(y => y).ToList(), OutputValue = x.First()[outputIndex] }).ToList();  //datas.Select(x => new QFoldData() { data = x, IsUse = false }).ToList();


            Random rnd = new Random();
            int luckyRow = 0;
            List<int> listOfOneOutputsDatas = new List<int>();
            List<int> listOfNotUse = new List<int>();
            //List<int>[] listOfZeroOutputsDatas = new List<int>[outputIndexes.Count];

            for (int rowIndex = 0; rowIndex < dumpDatas.Count; rowIndex++)
            {

                if (!dumpDatas[rowIndex].IsUse && dumpDatas[rowIndex].OutputValue == 1)
                {
                    listOfOneOutputsDatas.Add(rowIndex);
                    dumpDatas[rowIndex].IsUse = true;
                }

            }

            for (int rowIndex = 0; rowIndex < dumpDatas.Count; rowIndex++)
            {
                if (!dumpDatas[rowIndex].IsUse)
                {
                    listOfNotUse.Add(rowIndex);
                }
            }


            

            List<List<decimal>>[] Clusters = new List<List<decimal>>[foldValue];
            for (int i = 0; i < foldValue; i++)
            {
                Clusters[i] = new List<List<decimal>>();
            }

            int perSample_1 = (int)Math.Floor(listOfOneOutputsDatas.Count / (double)foldValue);

            for (int i = 0; i < foldValue; i++)
            {
                for (int j = 0; j < perSample_1; j++)
                {
                    luckyRow = rnd.Next(0, listOfOneOutputsDatas.Count);
                    Clusters[i].AddRange(dumpDatas[listOfOneOutputsDatas[luckyRow]].data);
                    listOfOneOutputsDatas.RemoveAt(luckyRow);
                }
            }
            
            for (int i = 0; i < listOfOneOutputsDatas.Count; i++)
            {
                Clusters[i].AddRange(dumpDatas[listOfOneOutputsDatas[i]].data);
            }


            

            int perSample_0 = (int)Math.Floor(listOfNotUse.Count / (double)foldValue);

            for (int i = 0; i < foldValue; i++)
            {
                for (int j = 0; j < perSample_0; j++)
                {
                    luckyRow = rnd.Next(0, listOfNotUse.Count);
                    Clusters[i].AddRange(dumpDatas[listOfNotUse[luckyRow]].data);
                    listOfNotUse.RemoveAt(luckyRow);
                }
            }
            
            for (int i = 0; i < listOfNotUse.Count; i++)
            {
                Clusters[i].AddRange(dumpDatas[listOfNotUse[i]].data);
            }

            foreach (var item in Clusters)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].RemoveAt(vektorIndex);
                }
            }


            return Clusters.ToList();
        }
    }
}
