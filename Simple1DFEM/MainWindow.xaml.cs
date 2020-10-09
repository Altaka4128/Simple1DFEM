using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simple1DFEM
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private Beam1DFEM fem;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += loadedEvent;
        }
        private void loadedEvent(object sender, RoutedEventArgs e)
        {
            DrawXArrow();
        }

        private void FileOpenClicked(object sender, RoutedEventArgs e)
        {
            // 初期化
            ClearClicked(null, null);

            // 入力用のCSVファイルを読み込む
            var diag = new OpenFileDialog();
            diag.Filter = "CSVファイル (*.csv)|*.csv";
            if(diag.ShowDialog() == true)
            {
                _1DFEMData femData = new _1DFEMData();
                femData.ReadCSVFile(diag.FileName);

                fem = new Beam1DFEM(femData);

                // モデルを描画する
                List<BeamElement> elems = fem.BeamElems;
                for (int i = 0; i < elems.Count; i++)
                {
                    BeamElement elem = elems[i].ShallowCopy();   // 要素をコピーする

                    // 描画する
                    DrawElement(elems[i], Brushes.LightGreen, Brushes.Blue, 1.0, 1.0);
                }
            }

        }

        private void Example1Clicked(object sender, RoutedEventArgs e)
        {
            // 初期化
            ClearClicked(null, null);

            double area = 50.0;
            double young = 200000;

            // 要素を作成する
            List<BeamElement> elems = new List<BeamElement>();
            {
                // 要素1
                {
                    Node[] nodes = new Node[2];
                    nodes[0].No = 1;
                    nodes[0].Point = 0.0;
                    nodes[1].No = 2;
                    nodes[1].Point = 4.0;
                    elems.Add(new BeamElement(nodes, area, young));
                }
                // 要素2
                {
                    Node[] nodes = new Node[2];
                    nodes[0].No = 2;
                    nodes[0].Point = 4.0;
                    nodes[1].No = 3;
                    nodes[1].Point = 8.0;
                    elems.Add(new BeamElement(nodes, area, young));
                }
                // 要素3
                {
                    Node[] nodes = new Node[2];
                    nodes[0].No = 3;
                    nodes[0].Point = 8.0;
                    nodes[1].No = 4;
                    nodes[1].Point = 10.0;
                    elems.Add(new BeamElement(nodes, area, young));
                }
            }

            // 境界条件を設定する
            // 変位
            List<double> disp = new List<double>();
            for (int i = 0; i < 4; i++)
            {
                disp.Add(0);
            }
            DenseVector DispVector = DenseVector.OfArray(disp.ToArray());
            // 荷重
            List<double> force = new List<double>();
            for (int i = 0; i < 4; i++)
            {
                force.Add(0);
            }
            force[3] = 1000000.0;
            DenseVector ForceVector = DenseVector.OfArray(force.ToArray());
            // 拘束
            List<bool> Rest = new List<bool>();
            {
                for (int i = 0; i < 4; i++)
                {
                    Rest.Add(false);
                }
                Rest[0] = true;
            }

            fem = new Beam1DFEM(4, elems);
            fem.setBoundaryCondition(DispVector, ForceVector, Rest);

            // モデルを描画する
            for (int i = 0; i < elems.Count; i++)
            {
                DrawElement(elems[i], Brushes.LightGreen, Brushes.Blue, 1.0, 5.0);
            }
        }

        private void Example2Clicked(object sender, RoutedEventArgs e)
        {
            // 初期化
            ClearClicked(null, null);

            double area = 100.0;
            double young = 200000;

            // 要素を作成する
            List<BeamElement> elems = new List<BeamElement>();
            {
                for (int i = 0; i < 10; i++)
                {
                    Node[] nodes = new Node[2];
                    nodes[0].No = i + 1;
                    nodes[0].Point = 10.0 * i;
                    nodes[1].No = i + 2;
                    nodes[1].Point = 10.0 * (i + 1);
                    elems.Add(new BeamElement(nodes, area, young));
                }
            }

            // 境界条件を設定する
            // 変位
            List<double> disp = new List<double>();
            for (int i = 0; i < 11; i++)
            {
                disp.Add(0);
            }
            DenseVector DispVector = DenseVector.OfArray(disp.ToArray());
            // 荷重
            List<double> force = new List<double>();
            for (int i = 0; i < 11; i++)
            {
                force.Add(0);
            }
            force[10] = 2000.0;
            DenseVector ForceVector = DenseVector.OfArray(force.ToArray());
            // 拘束
            List<bool> Rest = new List<bool>();
            {
                for (int i = 0; i < 11; i++)
                {
                    Rest.Add(false);
                }
                Rest[0] = true;
            }

            fem = new Beam1DFEM(11, elems);
            fem.setBoundaryCondition(DispVector, ForceVector, Rest);

            // モデルを描画する
            for (int i = 0; i < elems.Count; i++)
            {
                DrawElement(elems[i], Brushes.LightGreen, Brushes.Blue, 1.0, 5.0);
            }
        }

        private void AnalysisClicked(object sender, RoutedEventArgs e)
        {
            // 例外処理
            if (fem == null)
            {
                return;
            }

            // FEM解析を実行する
            fem.Analysis();

            // 結果を出力する
            fem.outputReport();

            // モデルを描画する
            List<BeamElement> elems = fem.BeamElems;
            DenseVector dispElemVector = DenseVector.Create(2, 0.0);
            DenseVector dispVector = fem.DispVector;
            for (int i = 0; i < elems.Count; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    dispElemVector[j] = dispVector[elems[i].Nodes[j].No - 1];
                }

                // 変形後の要素を計算する
                BeamElement elem = elems[i].ShallowCopy();   // 変形前の要素をコピーする
                for (int j = 0; j < 2; j++)
                {
                    elem.Nodes[j].Point += dispElemVector[j];
                }

                // 変形後の要素を描画する
                DrawElement(elem, Brushes.LightCoral, Brushes.Red, 0.5, 1.0);
            }
        }

        private void SampleAnalysisClicked(object sender, RoutedEventArgs e)
        {
            // 例外処理
            if (fem == null)
            {
                return;
            }

            // FEM解析を実行する
            fem.Analysis();

            // 結果を出力する
            fem.outputReport();

            // モデルを描画する
            List<BeamElement> elems = fem.BeamElems;
            DenseVector dispElemVector = DenseVector.Create(2, 0.0);
            DenseVector dispVector = fem.DispVector;
            for (int i = 0; i < elems.Count; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    dispElemVector[j] = dispVector[elems[i].Nodes[j].No - 1];
                }

                // 変形後の要素を計算する
                BeamElement elem = elems[i].ShallowCopy();   // 変形前の要素をコピーする
                for (int j = 0; j < 2; j++)
                {
                    elem.Nodes[j].Point += dispElemVector[j];
                }

                // 変形後の要素を描画する
                DrawElement(elem, Brushes.LightCoral, Brushes.Red, 0.5, 5.0);
            }
        }
        private void ClearClicked(object sender, RoutedEventArgs e)
        {
            fem = null;
            this.Canvas.Children.Clear();
            DrawXArrow();
        }

        private void DrawXArrow()
        {
            // X軸を描画する
            Line xArrow = new Line();
            xArrow.Stroke = Brushes.Red;
            xArrow.StrokeThickness = 4;
            xArrow.StrokeEndLineCap = PenLineCap.Triangle;
            xArrow.X1 = 0;
            xArrow.Y1 = 0;
            xArrow.X2 = 50;
            xArrow.Y2 = 0;
            this.Canvas.Children.Add(xArrow);
        }

        private void DrawElement(BeamElement elem, Brush elemcolor, Brush nodecolor, Double opacity, double scale)
        {
            // 要素を描画する
            Line line = new Line();
            line.Stroke = elemcolor;
            line.StrokeThickness = 3;
            line.Opacity = opacity;
            line.X1 = elem.Nodes[0].Point * scale + 200;
            line.X2 = elem.Nodes[1].Point * scale + 200;
            line.Y1 = 300.0;
            line.Y2 = 300.0;
            this.Canvas.Children.Add(line);

            // 節点を描画する
            for (int i = 0; i < 2; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Fill = nodecolor;
                ellipse.Width = 10;
                ellipse.Height = 10;
                Canvas.SetLeft(ellipse, elem.Nodes[i].Point * scale - ellipse.Width * 0.5 + 200);
                Canvas.SetTop(ellipse, 300 - ellipse.Height * 0.5);

                this.Canvas.Children.Add(ellipse);
            }
        }
    }
}
