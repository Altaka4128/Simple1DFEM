﻿using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple1DFEM
{
    
    public struct InputData
    {

        public List<double> Node;
        public List<BeamElement> Elem;
    }

    public class Beam1DFEM
    {
        private int NodeNum = 0;   // 節点数
        public List<BeamElement> BeamElems   // 要素の集合
        {
            get;
            private set;
        }
        public DenseVector DispVector   // 変位の境界条件
        {
            get;
            private set;
        }

        public DenseVector ForceVector   // 荷重の境界条件
        {
            get;
            private set;
        }

        public List<bool> Rest   // 拘束の境界条件
        {
            get;
            private set;
        }
        bool AnalysisFlg = false;

        public Beam1DFEM(int nodenum, List<BeamElement> beamelems)
        {
            NodeNum = nodenum;
            BeamElems = beamelems;
        }

        public Beam1DFEM(_1DFEMData data)
        {
            // 要素の形式を変換して格納する
            List<BeamElement> elems = new List<BeamElement>();
            {
                for (int i = 0; i < data.elems.Count; i++)
                {
                    Node[] nodes = new Node[2];
                    nodes[0].No = data.elems[i].NodeNo1;
                    nodes[0].Point = data.nodes[nodes[0].No - 1].Point;
                    nodes[1].No = data.elems[i].NodeNo2;
                    nodes[1].Point = data.nodes[nodes[1].No - 1].Point;

                    int materialNo = data.elems[i].MaterialNo - 1;
                    double area = data.materials[materialNo].Area;
                    double young = data.materials[materialNo].Young;

                    elems.Add(new BeamElement(nodes, area, young));
                }
            }
            NodeNum = data.nodes.Count;
            BeamElems = elems;

            // 拘束条件の形式を変換して格納する
            // 変位
            List<double> disp = new List<double>();
            List<double> force = new List<double>();
            List<bool> constraint = new List<bool>();
            for (int i = 0; i < (data.nodes.Count * 1); i++)
            {
                disp.Add(data.nodes[i].Displacement);
                force.Add(data.nodes[i].Force);
                constraint.Add(data.nodes[i].Constraint);
            }
            DenseVector dispVector = DenseVector.OfArray(disp.ToArray());
            DenseVector forceVector = DenseVector.OfArray(force.ToArray());
            setBoundaryCondition(dispVector, forceVector, constraint);
        }

        // Kマトリックスを作成する
        private DenseMatrix makeKMatrix()
        {
            // 例外処理
            if (Rest == null)
            {
                return null;
            }
            if (NodeNum <= 0 || BeamElems == null || Rest.Count != NodeNum)
            {
                return null;
            }

            DenseMatrix kMatrix = DenseMatrix.Create(NodeNum, NodeNum, 0.0);

            // 各要素のKeマトリックスを計算し、Kマトリックスに統合する
            for (int i = 0; i < BeamElems.Count; i++)
            {
                Console.WriteLine("要素" + (i + 1).ToString());
                DenseMatrix keMatrix = BeamElems[i].makeKeMatrix();

                for (int r = 0; r < 2; r++)
                {
                    int rt = BeamElems[i].Nodes[r].No - 1;
                    for (int c = 0; c < 2; c++)
                    {
                        int ct = BeamElems[i].Nodes[c].No - 1;
                        kMatrix[rt, ct] += keMatrix[r, c];
                    }
                }
            }

            Console.WriteLine("Kマトリックス");
            Console.WriteLine(kMatrix);

            // 境界条件を考慮して修正する
            ForceVector = ForceVector - kMatrix * DispVector;
            for (int i = 0; i < Rest.Count; i++)
            {
                if (Rest[i] == true)
                {
                    for (int j = 0; j < kMatrix.ColumnCount; j++)
                    {
                        kMatrix[i, j] = 0.0;
                    }
                    for (int k = 0; k < kMatrix.RowCount; k++)
                    {
                        kMatrix[k, i] = 0.0;
                    }
                    kMatrix[i, i] = 1.0;

                    ForceVector[i] = DispVector[i];
                }
            }

            Console.WriteLine("Kマトリックス(境界条件考慮)");
            Console.WriteLine(kMatrix);
            Console.WriteLine("荷重ベクトル(境界条件考慮)");
            Console.WriteLine(ForceVector);

            return kMatrix;
        }

        // 境界条件を設定する
        public void setBoundaryCondition(DenseVector dispvector, DenseVector forcevector, List<bool> rest)
        {
            DispVector = dispvector;
            ForceVector = forcevector;
            Rest = rest;
        }

        public void Analysis()
        {
            DenseMatrix kMatrix = makeKMatrix();

            if(kMatrix == null)
            {
                return;
            }

            // 変位を計算する
            DispVector = (DenseVector)(kMatrix.Inverse().Multiply(ForceVector));
            Console.WriteLine("変位ベクトル");
            Console.WriteLine(DispVector);

            // 各要素の応力を計算する
            DenseVector dispElemVector = DenseVector.Create(2, 0.0);
            for (int i = 0; i < BeamElems.Count; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    dispElemVector[j] = DispVector[BeamElems[i].Nodes[j].No - 1];
                }

                Console.WriteLine("要素" + (i + 1).ToString());
                BeamElems[i].makeStrainVector(dispElemVector);
                BeamElems[i].makeStressVector();
            }

            AnalysisFlg = true;
        }

        // 結果を出力する
        public void outputReport()
        {
            if (AnalysisFlg != true)
            {
                return;
            }

            StreamWriter sw = new StreamWriter("output.csv");

            // 節点変位を書き込む
            List<Node> nodes = new List<Node>();
            for(int i = 0; i < BeamElems.Count; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    nodes.Add(BeamElems[i].Nodes[j]);
                }
            }
            nodes = nodes.Distinct().ToList();
            nodes.Sort((a,b) => a.No - b.No);
            
            sw.WriteLine("Node, Coordinate, Ux");
            for (int i = 0; i < NodeNum; i++)
            {
                sw.WriteLine((i + 1).ToString() + ", " + nodes[i].Point + ", " + DispVector[i]);
            }

            // 要素情報を書き込む
            sw.WriteLine("Element, X-Stress");
            for(int i = 0; i < BeamElems.Count; i++)
            {
                sw.WriteLine((i + 1).ToString() + "," + BeamElems[i].StressVector[0]);
            }
            sw.Close();
        }
    }
}
