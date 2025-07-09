using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace Sikaku_Kakomi_NoCV_01
{
    public class ImageAnalyzer
    {
        public static void Analyze(Window parent, Image displayTarget, bool areaFilter)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dlg.ShowDialog() != true) return;

            BitmapImage original = new BitmapImage(new Uri(dlg.FileName));
            WriteableBitmap bitmap = new WriteableBitmap(original);

            int width = bitmap.PixelWidth, height = bitmap.PixelHeight, stride = width * 4;
            byte[] pixels = new byte[height * stride];
            bitmap.CopyPixels(pixels, stride, 0);

            bool[,] mask = new bool[width, height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 4;
                    byte b = pixels[index], g = pixels[index + 1], r = pixels[index + 2];

                    var (h, s, v) = ImageProcessingHelper.RgbToHsv(r, g, b);
                    if (h >= 20 && h <= 55 && s >= 0.3 && v >= 0.4)
                        mask[x, y] = true;
                }

            mask = ImageProcessingHelper.Erode(mask, 1);
            mask = ImageProcessingHelper.Dilate(mask, 1);

            DrawingVisual visual = new DrawingVisual();
            int labelCount = 0;

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(original, new Rect(0, 0, width, height));
                bool[,] visited = new bool[width, height];

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        if (mask[x, y] && !visited[x, y])
                        {

                            // 追加 *** 変更 => 境界追跡（輪郭トレース）」でピクセルリストを取得
                            var regionPixels = ImageProcessingHelper.FloodFillAndGetPixels(mask, visited, x, y);

                            if (regionPixels == null || regionPixels.Count == 0) continue; // ★リストが空ならスキップ

                            int minX = regionPixels.Min(p => p.x);
                            int minY = regionPixels.Min(p => p.y);
                            int maxX = regionPixels.Max(p => p.x);
                            int maxY = regionPixels.Max(p => p.y);

                            Rect rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);

                            //　従来 0708 _ 変更　// 矩形 
                            //   Rect rect = ImageProcessingHelper.FloodFill(mask, visited, x, y);


                            double area = rect.Width * rect.Height;
                            areaFilter = false; // true = 小さい物体を除外

                            if (areaFilter == false)
                            {
                                if (rect.Width > 5 && rect.Height > 5)
                                {
                                    // DPI情報取得
                                    double dpi = VisualTreeHelper.GetDpi(parent).PixelsPerDip;

                                    FormattedText text = new FormattedText(
                                        $"Obj {++labelCount}",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        FlowDirection.LeftToRight,
                                        new Typeface("Arial"),
                                        12,
                                        Brushes.Yellow,
                                        dpi
                                    );

                                    // 明示的な Point を使用
                                    dc.DrawRectangle(null, new Pen(Brushes.Lime, 2), rect);
                                    dc.DrawText(text, new System.Windows.Point(rect.X, rect.Y - 15));
                                }
                            }
                            else
                            {

                                if (area > 40) // 面積が40ピクセル以上
                                {
                                    // 描画・カウント処理
                                    // DPI情報取得
                                    double dpi = VisualTreeHelper.GetDpi(parent).PixelsPerDip;

                                    FormattedText text = new FormattedText(
                                        $"NM_Obj {++labelCount}",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        FlowDirection.LeftToRight,
                                        new Typeface("Arial"),
                                        12,
                                        Brushes.Yellow,
                                        dpi
                                    );

                                    // 明示的な Point を使用
                                    dc.DrawRectangle(null, new Pen(Brushes.Lime, 2), rect);
                                    dc.DrawText(text, new System.Windows.Point(rect.X, rect.Y - 15));
                                }
                                else
                                {
                                    // 小さいオブジェクトは無視
                                    continue;

                                }
                            }
                        }
            }

            RenderTargetBitmap result = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            result.Render(visual);
            displayTarget.Source = result;

            MessageBox.Show($"検出オブジェクト数: {labelCount}", "処理結果");
        }
    }
}
