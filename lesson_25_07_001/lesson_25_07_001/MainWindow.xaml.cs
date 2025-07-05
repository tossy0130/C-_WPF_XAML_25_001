using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace lesson_25_07_001
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        private BitmapImage bitmap;

        private Point startPoint;
        private Rectangle selectionRect;
        private bool isDragging;


        public MainWindow()
        {
            InitializeComponent();
        }

        // === 画像読み込み
        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "画像ファイル（*.png; *.jpg）| *.png; *.jpg";

            if(dialog.ShowDialog() == true)
            {
                bitmap = new BitmapImage(new Uri(dialog.FileName));
                LoadedImage.Source = bitmap;

                // サイズをキャンバスに反映
                LoadedImage.Width = bitmap.PixelWidth;
                LoadedImage.Height = bitmap.PixelHeight;

                ImageCanvas.Width = bitmap.PixelWidth;
                ImageCanvas.Height = bitmap.PixelHeight;

                // 既存の矩形をクリア
                ImageCanvas.Children.Clear();
                ImageCanvas.Children.Add(LoadedImage);
            }

        }

        // ====================================
        // ====================== 描画  01 
        // ====================================
        private void DrawRectangle_Click(object sender, RoutedEventArgs e)
        {   
            // エラー処理 , 画像が読み込まれない場合は、何もしない
            if (bitmap == null) return;

            // 座標取得
            if (int.TryParse(XInput.Text, out int x) && 
                int.TryParse(YInput.Text, out int y) &&
                int.TryParse(DivisionInput.Text, out int divisions) &&
                divisions > 0)
            {
                // ============================================
                // =============================== 四角のサイズ 
                // ============================================

                // === 通常 

                double parentWidth = 200;
                double parentHeight = 200;


                Rectangle rect = new Rectangle
                {
                    Width = parentWidth,
                    Height = parentHeight,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };
                

                // === 画像に合わせて、可変
                /*
                double rectWidth = bitmap.PixelWidth * 0.5;  // 画像の横幅の50%
                double rectHeight = bitmap.PixelHeight * 0.5; // 画像の高さの50%

                Rectangle rect = new Rectangle
                {
                    Width = rectWidth,
                    Height = rectHeight,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };
                */


                // 短径をキャンバスに追加
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);

                ImageCanvas.Children.Add(rect);

                // 分割矩形サイズ
                double cellWidth = parentWidth / divisions;
                double cellHeight = parentHeight / divisions;

                // ================= 分割描画　内
                for (int i = 0; i < divisions; i++)
                {

                    for(int j = 0; j < divisions; j++)
                    {

                        Rectangle cell = new Rectangle
                        {
                            Width = cellWidth,
                            Height = cellHeight,
                            Stroke = Brushes.Blue,
                            StrokeThickness = 1
                        };

                        // === 描画処理
                        double cellX = x + i * cellWidth;
                        double cellY = y + j * cellHeight;

                        Canvas.SetLeft(cell, cellX);
                        Canvas.SetTop(cell, cellY);
                        ImageCanvas.Children.Add(cell);

                    }

                }
            }
            else
            {
                MessageBox.Show("X と Y に整数を入力してください", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        // ====================================
        // ====================== // マウスムーブ：矩形サイズを動的変更
        // ====================================
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(!isDragging || bitmap == null || selectionRect == null) return;

            Point pos = e.GetPosition(ImageCanvas);
            double x = Math.Min(pos.X, startPoint.X);
            double y = Math.Min(pos.Y, startPoint.Y);
            double w = Math.Abs(pos.X - startPoint.X);
            double h = Math.Abs(pos.Y - startPoint.Y);

            selectionRect.Width = w;
            selectionRect.Height = h;

            Canvas.SetLeft(selectionRect, x);
            Canvas.SetTop(selectionRect, y);

        }


        // ====================================
        // ====================== // マウスアップ：描画確定 & 分割描画
        // ====================================
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (bitmap == null) return;

            startPoint = e.GetPosition(ImageCanvas);
            isDragging = true;

            selectionRect = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            Canvas.SetLeft(selectionRect, startPoint.X);
            Canvas.SetTop(selectionRect, startPoint.Y);
            ImageCanvas.Children.Add(selectionRect);
        }


        // マウスアップ：描画確定 & 分割描画
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isDragging || selectionRect == null) return;
            isDragging = false;

            if (!int.TryParse(DivisionInput.Text, out int divisions) || divisions <= 0)
            {
                MessageBox.Show("分割数を正しく入力してください（例: 16, 32, 64）");
                return;
            }

            double left = Canvas.GetLeft(selectionRect);
            double top = Canvas.GetTop(selectionRect);
            double width = selectionRect.Width;
            double height = selectionRect.Height;

            // セルサイズを丸めて矩形サイズ再調整
            double cellWidth = Math.Floor(width / divisions);
            double cellHeight = Math.Floor(height / divisions);
            double adjustedWidth = cellWidth * divisions;
            double adjustedHeight = cellHeight * divisions;

            // 親矩形の再設定（ピタリサイズ）
            selectionRect.Width = adjustedWidth;
            selectionRect.Height = adjustedHeight;

            Canvas.SetLeft(selectionRect, left);
            Canvas.SetTop(selectionRect, top);

            // 分割矩形を描画
            for (int i = 0; i < divisions; i++)
            {
                for (int j = 0; j < divisions; j++)
                {
                    Rectangle cell = new Rectangle
                    {
                        Width = cellWidth,
                        Height = cellHeight,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 0.5
                    };

                    double cellX = left + i * cellWidth;
                    double cellY = top + j * cellHeight;

                    Canvas.SetLeft(cell, cellX);
                    Canvas.SetTop(cell, cellY);
                    ImageCanvas.Children.Add(cell);
                }
            }

            selectionRect = null; // 次の描画のためにリセット
        }

    }
}
