using QuantumLib.AI.DIFFRACTION.DATA;
using QuantumLib.AI.COMMON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumLib.AI.DIFFRACTION
{
    public class QDiffractionAnalysis
    {
        public List<int> inputIndexes { get; set; }
        public List<int> outputIndexes { get; set; }
        public List<List<decimal>> datas { get; set; }
        public QDiffractionAnalysis(string path, List<int> inputIndexes, List<int> outputIndexes)
        {
            datas = QData.ReadFileDecimalList(path);
            this.inputIndexes = inputIndexes;
            this.outputIndexes = outputIndexes;
            
        }
        public QDiffractionAnalysis(List<List<decimal>> datas, List<int> inputIndexes, List<int> outputIndexes)
        {
            this.datas = datas;
            this.inputIndexes = inputIndexes;
            this.outputIndexes = outputIndexes;
            
        }

        static List<List<decimal>> SubData(List<List<decimal>> data, int begin, int end)
        {
            // Deep copy list
            List<List<decimal>> subData = new List<List<decimal>>();

            for (int i = begin; i <= end; i++)
            {
                List<decimal> subRow = new List<decimal>(data[i]);
                subData.Add(subRow);
            }
            return subData;
        }
        public List<QDiffractionStruct> DiffractionAnalys(bool retryInputs=false)
        {
            List<QDiffractionStruct> listOfData = new List<QDiffractionStruct>();
            foreach (var output in outputIndexes)
            {
                var data = DiffractionAnalys(datas, inputIndexes, output, retryInputs);
                if (data != null)

                    listOfData.Add(data);
                else
                    throw new Exception("Cannot be calculate");
            }
            return listOfData;
        }
        public QDiffractionStruct DiffractionAnalys(List<List<decimal>> data, List<int> inputs, int output,bool retryInputs=false,int beforeInput=-1)
        {
            List<QDiffractionStruct> listOfDiffractions = new List<QDiffractionStruct>();
            //Console.WriteLine("IN:"+inputs.Count+"\tDataC:"+data.Count);
            int manuelIndex = 0;
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i] == beforeInput)
                    continue;
                QDiffractionStruct incoming = CalculateDiffPoints(data, inputs[i], output);

                if (incoming != null)
                {
                    listOfDiffractions.Add(incoming);
                }
            }
            listOfDiffractions = listOfDiffractions.OrderByDescending(x => x.ListOfDiffPoints.Where(y => y.IsRegular).Sum(y => y.EndIndex - y.BeginIndex)).ToList();

            //printf("Ordered diff lists\n");
            bool isOk = true;
            QDiffractionStruct currentDiffractionStruct = null;
            QDiffractionPoint currentDiffractionPoint = null;

            for (int diffIndex = 0; diffIndex < listOfDiffractions.Count; diffIndex++)
            {
                isOk = true;
                currentDiffractionStruct = listOfDiffractions[diffIndex];
                data = data.OrderBy(x => x[currentDiffractionStruct.InputIndex]).ThenBy(x => x[output]).ToList();
                

                
                int[] remainingInputs = null;
                if (retryInputs)
                {
                    remainingInputs = inputs.ToArray();
                }
                else
                {
                    remainingInputs = new int[inputs.Count - 1];
                    manuelIndex = 0;
                    for (int i = 0; i < inputs.Count; i++)
                    {
                        if (inputs[i] == currentDiffractionStruct.InputIndex)
                            continue;
                        remainingInputs[manuelIndex] = inputs[i];
                        manuelIndex++;
                    }
                }
                for (int item = 0; item < currentDiffractionStruct.ListOfDiffPoints.Count; item++)
                {
                    currentDiffractionPoint = currentDiffractionStruct.ListOfDiffPoints[item];
                    if (!currentDiffractionPoint.IsRegular)
                    {
                        //Irregular
                        if (inputs.Count == 1)
                        {
                            isOk = false;
                            break;
                        }
                        var reAnalysingData = SubData(data, currentDiffractionPoint.BeginIndex, currentDiffractionPoint.EndIndex);
                        QDiffractionStruct incomingAnalysis = DiffractionAnalys(reAnalysingData, remainingInputs.ToList(), output,retryInputs,currentDiffractionStruct.InputIndex);

                        if (incomingAnalysis != null)
                            currentDiffractionPoint.RegularizationDiffractionStruct = incomingAnalysis;
                        else
                        {
                            isOk = false;
                            break;
                        }
                    }
                }
                if (isOk)
                    return currentDiffractionStruct;
            }
            return null;

        }

        private static QPoint CalculateWeights(decimal pointOfZero, decimal pointOfOne, decimal sharpnessRatio = 0.001m)
        {
            decimal midPoint = (pointOfZero + pointOfOne) / 2;
            decimal A = (-2) * (decimal)Math.Log((double)sharpnessRatio) / (pointOfOne - pointOfZero);
            decimal B = (-1) * (A) * midPoint;
            return new QPoint() { _1 = A, _2 = B };
        }

        private QDiffractionStruct CalculateDiffPoints(List<List<decimal>> data, int input, int output, decimal sharpnessRation = 0.001m)
        {
            var orderedData = data.OrderBy(x => x[input]).ThenBy(x => x[output]).ToList();
            QDiffractionStruct diffractions = new QDiffractionStruct() { InputIndex = input, OutputIndex=output };
            QDiffractionPoint currentDiffPoint = new QDiffractionPoint();
            bool IsDiffracted = false;
            int begin = 0;
            bool IsEmergencyExit = false;
            bool IsContainZero = false;
            bool IsContainOne = false;

            while (begin < orderedData.Count - 1)
            {
                IsContainZero = false;
                IsContainOne = false;
                int end = begin;
                while (end < orderedData.Count - 1)
                {
                    if (orderedData[end][input] == orderedData[end + 1][input])
                    {
                        if (orderedData[end][output] == 0 || orderedData[end + 1][output] == 0)
                            IsContainZero = true;
                        if (orderedData[end][output] == 1 || orderedData[end + 1][output] == 1)
                            IsContainOne = true;
                    }
                    else
                    {
                        if (orderedData[end][output] == 0)
                            IsContainZero = true;
                        if (orderedData[end][output] == 1)
                            IsContainOne = true;
                        break;
                    }
                    end++;
                }
                if (IsDiffracted)
                {
                    
                    if ((currentDiffPoint.IsRegular && IsContainOne && !IsContainZero) || (!currentDiffPoint.IsRegular && IsContainOne && IsContainZero))
                    {
                       
                        currentDiffPoint.EndIndex = end;
                        if (end == orderedData.Count - 1)
                        {
                            currentDiffPoint.Point2 = null;
                            diffractions.ListOfDiffPoints.Add(currentDiffPoint);
                            IsDiffracted = false;
                            IsEmergencyExit = true;
                            break;
                        }
                        currentDiffPoint.Point2 = CalculateWeights(orderedData[end + 1][input], orderedData[end][input], sharpnessRation);
                    }
                    else if (IsContainOne)
                    {
                        diffractions.ListOfDiffPoints.Add(currentDiffPoint);
                        currentDiffPoint = new QDiffractionPoint();
                        currentDiffPoint.BeginIndex = begin;
                        if (IsContainZero)
                            currentDiffPoint.IsRegular = false;
                        else
                            currentDiffPoint.IsRegular = true;
                        if (begin != 0)
                            currentDiffPoint.Point1 = CalculateWeights(orderedData[begin - 1][input], orderedData[begin][input], sharpnessRation);
                        currentDiffPoint.EndIndex = end;
                        if (end == orderedData.Count - 1)
                        {
                            currentDiffPoint.Point2 = null;
                            diffractions.ListOfDiffPoints.Add(currentDiffPoint);
                            IsDiffracted = false;
                            IsEmergencyExit = true;
                            break;
                        }
                        currentDiffPoint.Point2 = CalculateWeights(orderedData[end + 1][input], orderedData[end][input], sharpnessRation);
                    }
                    else
                    {
                        diffractions.ListOfDiffPoints.Add(currentDiffPoint);
                        IsDiffracted = false;
                    }

                }

                else if (IsContainOne)
                {
                    
                    IsDiffracted = true;
                    currentDiffPoint = new QDiffractionPoint();
                    currentDiffPoint.BeginIndex = begin;
                    if (IsContainZero)
                        currentDiffPoint.IsRegular = false;
                    else
                        currentDiffPoint.IsRegular = true;
                    if (begin != 0)
                        currentDiffPoint.Point1 = CalculateWeights(orderedData[begin - 1][input], orderedData[begin][input], sharpnessRation);
                    currentDiffPoint.EndIndex = end;
                    if (end == orderedData.Count - 1)
                    {
                        currentDiffPoint.Point2 = null;
                        diffractions.ListOfDiffPoints.Add(currentDiffPoint);
                        IsDiffracted = false;
                        IsEmergencyExit = true;
                        break;
                    }
                    currentDiffPoint.Point2 = CalculateWeights(orderedData[end + 1][input], orderedData[end][input], sharpnessRation);
                }
                begin = end;
                begin += 1;
            }
            
            IsContainZero = false;
            IsContainOne = false;

            
            if (IsEmergencyExit)
            {
                
                if (diffractions.ListOfDiffPoints.Count == 0)
                    return null;
                else
                    return diffractions;
            }
            
            if (orderedData[orderedData.Count - 1][output] == 0)
                IsContainZero = true;
            if (orderedData[orderedData.Count - 1][output] == 1)
                IsContainOne = true;
            
            if (IsDiffracted && (!currentDiffPoint.IsRegular || (currentDiffPoint.IsRegular && IsContainZero)))
            {
                
                diffractions.ListOfDiffPoints.Add(currentDiffPoint);
                IsDiffracted = false;
            }
            if (!IsDiffracted && IsContainOne)
            {
                
                currentDiffPoint = new QDiffractionPoint();
                begin = orderedData.Count - 1;
                currentDiffPoint.BeginIndex = begin;
                currentDiffPoint.IsRegular = true;
                currentDiffPoint.Point1 = CalculateWeights(orderedData[begin - 1][input], orderedData[begin][input], sharpnessRation);
                currentDiffPoint.EndIndex = begin;
                diffractions.ListOfDiffPoints.Add(currentDiffPoint);
                IsDiffracted = false;
            }
            if (IsDiffracted)
            {
                
                currentDiffPoint.Point2 = null;
                currentDiffPoint.EndIndex = orderedData.Count - 1;
                if (currentDiffPoint.BeginIndex != 0)
                    
                    diffractions.ListOfDiffPoints.Add(currentDiffPoint);
                IsDiffracted = false;
                
            }
            if (diffractions.ListOfDiffPoints.Count == 0)
            {
                
                return null;
            }
           
            return diffractions;
        }
    }
}
