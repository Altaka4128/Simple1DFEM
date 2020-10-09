using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple1DFEM
{
    public struct Material
    {
        public double Area;
        public double Young;
    }

    public struct Node
    {
        public int No;
        public double Point;
        public bool Constraint;
        public double Displacement;
        public double Force;
    }

    public struct Element
    {
        public int NodeNo1;
        public int NodeNo2;
        public int MaterialNo;
    }

    public class _1DFEMData
    {
        public List<Material> materials = new List<Material>();
        public List<Node> nodes = new List<Node>();
        public List<Element> elems = new List<Element>();

        public void ReadCSVFile(String filename)
        {
            /*ファイルフォーマット*/
            //材料
            //Material,Young, Area
            //1,200000,400
            //2,150000,300
            //︙
            //節点
            //Node, Coordinate, Constraint, Displacement, Force
            //1,0, True,0,0
            //2,150, False,0,0
            //︙
            //要素
            //Element, Node1, Node2, Material,
            //1,1,2,1
            //2,2,3,2
            //︙

            var fs = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            List<String> lines = new List<string>();
            while (!sr.EndOfStream)
            {
                lines.Add(sr.ReadLine());
            }

            // 材料情報の読み込み
            // 材料情報の文字部を読み飛ばす
            int lineNo = 0;
            while (lines.Count != lineNo)
            {
                String[] values = lines[lineNo].Split(',');
                bool headerFlg = true;
                for (int i = 0; i < values.Length; i++)
                {
                    double j = 0;
                    bool result = double.TryParse(values[i], out j);

                    // 文字が含まれている場合、数値まで読み飛ばす
                    if (result == false)
                    {
                        lineNo++;
                        break;
                    }
                    else
                    {
                        headerFlg = false;
                    }
                }
                if (headerFlg == false)
                {
                    lineNo--;
                    break;
                }
            }
            // 材料情報の読み込み
            while (lines.Count != lineNo)
            {
                String[] values = lines[lineNo].Split(',');
                bool headerFlg = false;
                // 最初に文字が含まれている場合、終了する
                double j = 0;
                bool result = double.TryParse(values[0], out j);
                if (result == false && values[0] != "")
                {
                    headerFlg = true;
                    break;
                }
                if (headerFlg == true)
                {
                    break;
                }
                // 材料情報を読み込む
                Material material = new Material();
                material.Young = double.Parse(values[1]);
                material.Area = double.Parse(values[2]);
                materials.Add(material);
                lineNo++;
            }

            // 節点情報の読み込み
            // 節点情報の文字部を読み飛ばす
            while (lines.Count != lineNo)
            {
                String[] values = lines[lineNo].Split(',');
                bool headerFlg = true;
                for (int i = 0; i < values.Length; i++)
                {
                    double j = 0;
                    bool result = double.TryParse(values[i], out j);

                    // 文字が含まれている場合、数値まで読み飛ばす
                    if (result == false)
                    {
                        lineNo++;
                        break;
                    }
                    else
                    {
                        headerFlg = false;
                    }
                }
                if (headerFlg == false)
                {
                    lineNo--;
                    break;
                }
            }
            // 節点情報の読み込み
            while (lines.Count != lineNo)
            {
                String[] values = lines[lineNo].Split(',');
                bool headerFlg = false;
                // 最初に文字が含まれている場合、終了する
                double j = 0;
                bool result = double.TryParse(values[0], out j);
                if (result == false && values[0] != "")
                {
                    headerFlg = true;
                    break;
                }
                if (headerFlg == true)
                {
                    break;
                }
                // 節点情報を読み込む
                Node node = new Node();
                node.No = int.Parse(values[0]);
                node.Point = double.Parse(values[1]);
                node.Constraint = Convert.ToBoolean(values[2]);
                node.Displacement = double.Parse(values[3]);
                node.Force = double.Parse(values[4]);
                nodes.Add(node);
                lineNo++;
            }

            // 要素情報の読み込み
            // 要素情報の文字部を読み飛ばす
            while (lines.Count != lineNo)
            {
                String[] values = lines[lineNo].Split(',');
                bool headerFlg = true;
                for (int i = 0; i < values.Length; i++)
                {
                    int j = 0;
                    bool result = int.TryParse(values[i], out j);

                    // 文字が含まれている場合、数値まで読み飛ばす
                    if (result == false)
                    {
                        lineNo++;
                        break;
                    }
                    else
                    {
                        headerFlg = false;
                    }
                }
                if (headerFlg == false)
                {
                    lineNo--;
                    break;
                }
            }
            // 要素情報の読み込み
            while (lines.Count != lineNo)
            {
                String[] values = lines[lineNo].Split(',');
                bool headerFlg = false;
                // 最初に文字が含まれている場合、終了する
                double j = 0;
                bool result = double.TryParse(values[0], out j);
                if (result == false && values[0] != "")
                {
                    headerFlg = true;
                    break;
                }
                if (headerFlg == true)
                {
                    break;
                }
                // 要素情報を読み込む
                Element elem = new Element();
                elem.NodeNo1 = int.Parse(values[1]);
                elem.NodeNo2 = int.Parse(values[2]);
                elem.MaterialNo = int.Parse(values[3]);
                elems.Add(elem);
                lineNo++;
            }
        }
    }
}
