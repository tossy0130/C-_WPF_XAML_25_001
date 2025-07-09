using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sikaku_Kakomi_NoCV_01
{
    public static class ImageProcessingHelper
    {

        // ================================
        // ================ RGB → HSV変換
        // ================================
        public static (double H, double S, double V) RgbToHsv(byte r, byte g, byte b)
        {   
            // RGBを　 0 ～ 1 に正規化
            double rf = r / 255.0;
            double gf = g / 255.0;
            double bf = b / 255.0;

            // 最大・最小値を探す
            double max = Math.Max(rf, Math.Max(gf, bf));
            double min = Math.Min(rf, Math.Min(gf, bf));

            double delta = max - min; // 差（delta）が色の鮮やかさに関係

            // === 色相(Hue)の計算
            /*
             * ・どの色が一番強いかで「赤/緑/青」系統を決めて、「その中のどのあたりか（角度）」を出す
               ・0～360度（赤～緑～青～赤…）の円環上の色相値に
               ・マイナス値は+360して0～360に収める
             * 
             */
            double h = 0;

            if (delta != 0)
            {
                if (max == rf)
                    h = 60 * (((gf - bf) / delta) % 6);
                else if (max == gf)
                    h = 60 * (((bf - rf) / delta) + 2);
                else if (max == bf)
                    h = 60 * (((rf - gf) / delta) + 4);
            }

            if (h < 0) h += 360;

            // === 彩度(Saturation)の計算
            double s = max == 0 ? 0 : delta / max;

            // === 明度(Value)の計算
            double v = max;

            // (色相, 彩度, 明度)
            return (h, s, v);
        }


        // =================================
        // ================== 矩形取得 処理
        // =================================

        // *** 輪郭抽出 
        /*
            ・Queue を使った BFS探索 によって、mask が true のピクセルを走査
            ・各領域の minX, minY, maxX, maxY を記録し → Rect（矩形）に変換
        */

        public static Rect FloodFill(bool[,] mask, bool[,] visited, int startX, int startY)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((startX, startY));

            visited[startX, startY] = true;

            int minX = startX, maxX = startX;
            int minY = startY, maxY = startY;

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i], ny = y + dy[i];
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                    {
                        if (mask[nx, ny] && !visited[nx, ny])
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue((nx, ny));
                            minX = Math.Min(minX, nx);
                            maxX = Math.Max(maxX, nx);
                            minY = Math.Min(minY, ny);
                            maxY = Math.Max(maxY, ny);
                        }
                    }
                }
            }

            return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);

        }

        // =================================
        // ================== 輪郭ピクセルリスト付き FloodFill
        // =================================
        public static List<(int x, int y)> FloodFillAndGetPixels(bool[,] mask, bool[,] visited, int startX, int startY)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((startX, startY));
            visited[startX, startY] = true;

            List<(int, int)> pixels = new List<(int, int)>();
            pixels.Add((startX, startY));

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i], ny = y + dy[i];
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                    {
                        if (mask[nx, ny] && !visited[nx, ny])
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue((nx, ny));
                            pixels.Add((nx, ny));
                        }
                    }
                }
            }
            return pixels;
        }




        /*
        * マスク画像のノイズ除去（モルフォロジー処理）01
        */
        public static bool[,] Erode(bool[,] mask, int radius = 1)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            bool[,] result = new bool[width, height];

            for (int y = radius; y < height - radius; y++)
            {
                for (int x = radius; x < width - radius; x++)
                {
                    bool ok = true;
                    for (int dy = -radius; dy <= radius; dy++)
                        for (int dx = -radius; dx <= radius; dx++)
                            if (!mask[x + dx, y + dy])
                                ok = false;
                    result[x, y] = ok;
                }
            }
            return result;
        }

        /*
        * マスク画像のノイズ除去（モルフォロジー処理）02
        */
        public static bool[,] Dilate(bool[,] mask, int radius = 1)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            bool[,] result = new bool[width, height];

            for (int y = radius; y < height - radius; y++)
            {
                for (int x = radius; x < width - radius; x++)
                {
                    bool found = false;
                    for (int dy = -radius; dy <= radius; dy++)
                        for (int dx = -radius; dx <= radius; dx++)
                            if (mask[x + dx, y + dy])
                                found = true;
                    result[x, y] = found;
                }
            }
            return result;
        }


    }
}
