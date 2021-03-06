﻿using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple1DFEM
{
    public class BeamElement
    {
        public Node[] Nodes
        {
            get;
            private set;
        }
        private double Area;
        private double Young;
        private DenseMatrix KeMatrix;
        public DenseVector StrainVector
        {
            get;
            private set;
        }
        public DenseVector StressVector
        {
            get;
            private set;
        }

        public BeamElement()
        {
        }

        public BeamElement(
            Node[] nodes,
            double area,
            double young)
        {

            if (nodes.Length != 2)
            {
                return;
            }

            Nodes = nodes;
            Area = area;
            Young = young;

        }

        // Bマトリックスを計算する
        private DenseMatrix makeBMatirx()
        {
            // 例外処理
            if (Area <= 0)
            {
                return null;
            }

            double length = Nodes[1].Point - Nodes[0].Point;
            double[,] bmatrixArray = new double[1, 2];
            bmatrixArray[0, 0] = -1.0 / length;
            bmatrixArray[0, 1] = 1.0 / length;

            return DenseMatrix.OfArray(bmatrixArray);
        }

        // Keマトリックスを計算する
        public DenseMatrix makeKeMatrix()
        {
            // Bマトリックスを計算する
            DenseMatrix BMatrix = makeBMatirx();
            Console.WriteLine("Bマトリックス");
            Console.WriteLine(BMatrix);

            // 例外処理
            if (BMatrix == null || Young <= 0)
            {
                return null;
            }

            double Volume = Area * (Nodes[1].Point - Nodes[0].Point);
            var keMatrix = Young * Volume * BMatrix.Transpose() * BMatrix;
            DenseMatrix KeMatrix = DenseMatrix.OfColumnArrays(keMatrix.ToColumnArrays());
            Console.WriteLine("Keマトリックス");
            Console.WriteLine(KeMatrix);

            return KeMatrix;
        }

        // ひずみベクトルを計算する
        public void makeStrainVector(DenseVector dispvector)
        {
            DenseMatrix bMatrix = makeBMatirx();
            StrainVector = (DenseVector)bMatrix.Multiply(dispvector);
            Console.WriteLine("ひずみベクトル");
            Console.WriteLine(StrainVector);
        }

        // 応力ベクトルを計算する
        public void makeStressVector()
        {
            if (StrainVector == null || Young <= 0)
            {
                return;
            }

            StressVector = Young * StrainVector;
            Console.WriteLine("応力ベクトル");
            Console.WriteLine(StressVector);
        }

        public BeamElement ShallowCopy()
        {
            return (BeamElement)MemberwiseClone();
        }
    }
}
