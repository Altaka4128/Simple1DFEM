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
