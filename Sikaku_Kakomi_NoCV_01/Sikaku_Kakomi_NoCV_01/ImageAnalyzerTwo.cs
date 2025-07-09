using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sikaku_Kakomi_NoCV_01
{
    class ImageAnalyzerTwo
    {

        public static void Analyze(Window parent, Image displayTarget)
        {
            // 画像選択ダイアログを表示
            var dlg = new OpenFileDialog
            {
                Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp"
            }; 

            if (dlg.ShowDialog() == true)
            {   
                // 画像読み込み
                BitmapImage original = new BitmapImage(new Uri(dlg.FileName));
                WriteableBitmap bitmap = new WriteableBitmap(original);

                // 画像の幅と高さを取得
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;

                // RGBA 形式なので 4 バイト
                int stride = width * 4; // 1 行あたりのバイト数

                // ピクセルデータを格納する配列
                byte[] pixels = new byte[height * stride];
                bitmap.CopyPixels(pixels, stride, 0); // ピクセルデータを取得

                // マスク画像用の配列
                byte[] output = new byte[height * stride];

                
                for(int y = 0; y < height; y++)
                {
                    for(int x= 0; x < width; x++)
                    {
                        int idx = y * stride + x * 4; // RGBA のインデックス計算

                        byte b = pixels[idx];     // 青
                        byte g = pixels[idx + 1]; // 緑
                        byte r = pixels[idx + 2]; // 赤

                        // ****************************************
                        // ********* RGB から HSV に変換  *********
                        (double h, double s, double v) = ImageProcessingHelper.RgbToHsv(r, g, b);

                        // HSVでマスク対象を白に
                        bool isTarget = h >= 20 && h <= 60 && s > 0.4 && v > 0.4;

                        // エッジ強調：上下左右にマスクとの差分があればエッジとみなす
                        bool isEdge = false;


                        if(isTarget)
                        {
                            // マスク対象なら白に設定
                            for (int dy = -1; dy <= 1 && !isEdge; dy++) 
                            {
                                for (int dx = -1; dx <= 1 && !isEdge; dx++)
                                {
                                    if (dx == 0 && dy == 0) continue; // 自分自身は除外

                                    int nx = x + dx;
                                    int ny = y + dy;

                                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                    {
                                        int ni = (y + dy) * stride + (x + dx) * 4; // 隣接ピクセルのインデックス計算
                                        byte nb = pixels[ni]; // 隣接ピクセルの 青
                                        byte ng = pixels[ni + 1]; // 隣接ピクセルの 緑
                                        byte nr = pixels[ni + 2]; // 隣接ピクセルの 赤
                                        (double nh, double ns, double nv) = ImageProcessingHelper.RgbToHsv(nr, ng, nb); // 隣接ピクセルの HSV 変換

                                        if (!(nh >= 20 && nh <= 60 && ns > 0.4 && nv > 0.4)) // 隣接ピクセルがマスク対象でない場合

                                        {
                                            isEdge = true; // エッジとみなす
                                        }
                                    }
                                  
                                }
                            }
                        }



                        if (isEdge)
                        {
                            // エッジ → 黄色
                            output[idx] = 0; // B
                            output[idx + 1] = 255; // G
                            output[idx + 2] = 255; // R
                            output[idx + 3] = 255; // A
                        }
                        else if (isTarget)
                        {
                            // 内側 → 黄色
                            output[idx] = 0;
                           // output[idx] = 255; // => 白
                            output[idx + 1] = 255;
                            output[idx + 2] = 255;
                            output[idx + 3] = 255;
                        }
                        else
                        {
                            // 背景 → 黒
                            output[idx] = 0;
                            output[idx + 1] = 0;
                            output[idx + 2] = 0;
                            output[idx + 3] = 255;
                        }

                    }
                }

                WriteableBitmap result = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null); // 新しい画像を作成
                result.WritePixels(new Int32Rect(0, 0, width, height), output, stride, 0); // ピクセルデータを書き込む
                displayTarget.Source = result; // 画像を表示する Image コントロールに設定


            }

        }

    }
}
