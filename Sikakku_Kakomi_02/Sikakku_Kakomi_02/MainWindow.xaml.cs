using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Sikakku_Kakomi_02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
 //   public partial class MainWindow : Window
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
           
        private void OnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;

                // 画像を読み込む
                Mat image = Cv2.ImRead(imagePath);

                // エラー処理
                if (image.Empty())
                {
                    MessageBox.Show("画像の読み込みに失敗");
                    return;
                }

                // HSV変換
                Mat hsv = new Mat();
                Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);

                // 黄色の閾値 （デフォルト）
                Scalar lowerYellow = new Scalar(20, 100, 100);
                Scalar upperYellow = new Scalar(40, 255, 255);

                // マスク作成
                Mat mask = new Mat();
                Cv2.InRange(hsv, lowerYellow, upperYellow, mask);

                // カーネルのサイズ → OpenCvSharp.Size を明示
                OpenCvSharp.Size kernelSize = new OpenCvSharp.Size(3, 3);
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, kernelSize);
               

                // モルフォロジーオープン処理（ノイズ除去）
         //       Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
                Mat maskClean = new Mat();
                Cv2.MorphologyEx(mask, maskClean, MorphTypes.Open, kernel);

                // 輪郭検出
                OpenCvSharp.Point[][] contours;
                OpenCvSharp.HierarchyIndex[] hierarchy;
                Cv2.FindContours(maskClean, out contours, out hierarchy,
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                // 輪郭描画
                Mat output = image.Clone();
                for (int i = 0; i < contours.Length; i++)
                {
                    OpenCvSharp.Rect rect = Cv2.BoundingRect(contours[i]);
                    Cv2.Rectangle(output, rect, new Scalar(0, 255, 0), 2);

                    OpenCvSharp.Point textPos = new OpenCvSharp.Point(rect.X, rect.Y - 5);
                    Cv2.PutText(output, $"Obj {i + 1}", textPos, HersheyFonts.HersheySimplex,
                        0.6, new Scalar(36, 255, 12), 2);
                }

                // WPF用にBitmapSourceへ変換して表示
                ProcessedImage.Source = output.ToBitmapSource();

            }

        }

    }
}