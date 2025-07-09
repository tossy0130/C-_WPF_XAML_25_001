using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Sikaku_Kakomi_NoCV_01;

namespace Sikaku_Kakomi_NoCV_01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // =================================
        // ================== ボタン処理 処理 01
        // =================================
        private void OnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp"
            };

            // エラー処理 OK の場合
            if (dlg.ShowDialog() == true)
            {

                BitmapImage original = new BitmapImage(new Uri(dlg.FileName));
                WriteableBitmap bitmap = new WriteableBitmap(original);


                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                bitmap.CopyPixels(pixels, stride, 0);

                // マスク画像生成
                byte[] output = new byte[height * stride];


                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * stride + x * 4;

                        byte b = pixels[index];
                        byte g = pixels[index + 1];
                        byte r = pixels[index + 2];

                        // RGB → HSV 変換
                        (double h, double s, double v) = ImageProcessingHelper.RgbToHsv(r, g, b);

                        // 黄色閾値（h:20～40, s,v高め）
                        if (h >= 20 && h <= 40 && s > 0.4 && v > 0.4)
                        {
                            // 白で塗る
                            output[index] = 255;
                            output[index + 1] = 255;
                            output[index + 2] = 255;
                            output[index + 3] = 255;
                        }
                        else
                        {
                            // 黒
                            output[index] = 0;
                            output[index + 1] = 0;
                            output[index + 2] = 0;
                            output[index + 3] = 255;
                        }
                    }
                }

                // 新しい画像として表示
                WriteableBitmap result = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                result.WritePixels(new Int32Rect(0, 0, width, height), output, stride, 0);
                ProcessedImage.Source = result;

            } // === END if 

        }

      

        // =================================
        // ================== ボタン処理 処理 02
        // =================================
        private void OnSelectImage_Click_02(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("処理「2」実行開始");
            ImageAnalyzer.Analyze(this, ProcessedImage);

        } // ======================== END Function OnSelectImage_Click_02


        // =================================
        // ================== ボタン処理 処理 03
        // =================================
        private void OnSelectImage_Click_03(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("処理「3」実行開始");
            ImageAnalyzerTwo.Analyze(this, ProcessedImage);

        } // ======================== END Function OnSelectImage_Click_03



    }

}