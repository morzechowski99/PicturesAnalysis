using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PicturesAnalysis.Utils
{
    public static class BitmapAnalyser
    {
        private static Bitmap GetBitmapFromBitmapImage(BitmapImage image)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(image));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static Bitmap Calculate(BitmapImage image, out double percents)
        {
            var bitmap = GetBitmapFromBitmapImage(image);
            var copy = new Bitmap(bitmap);
            bitmap = AverageFilter(bitmap); // filtr usredniajacy
            for (int i = 10; i < bitmap.Width - 10; i += 10) //sprawdzenie maską
                for (int j = 10; j < bitmap.Height - 10; j += 10)
                {
                    var greenWin = 0;
                    for (int x = i - 10; x <= i; x++)
                        for (int y = j - 10; y <= j; y++)
                        {
                            var color = bitmap.GetPixel(x, y);
                            if (color.G > color.R && color.G > color.B)
                            {
                                greenWin++;
                            }
                        }
                    if (greenWin < 50)
                    {
                        for (int x = i - 10; x <= i; x++)
                            for (int y = j - 10; y <= j; y++)
                            {
                                var color = bitmap.GetPixel(x, y);
                                bitmap.SetPixel(x, y, Color.FromArgb(color.R, 0, color.B));
                            }
                    }
                }

            var pixels = bitmap.Width * bitmap.Height;
            var counter = 0;
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    if (2*color.G -color.R - color.B > 15 && color.G > 10)
                    {
                        counter++;
                        copy.SetPixel(i, j, Color.FromArgb(Math.Min(color.R + 150,255), color.G, color.B));
                    }
                }
            percents = counter / (double)pixels;
            return copy;
        }



        private static Color[,] getColors(Bitmap bitmap)
        {
            Color[,] colors = new Color[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    colors[i, j] = bitmap.GetPixel(i, j);
                }
            }

            return colors;
        }
        public static Bitmap AverageFilter(Bitmap bitmap)
        {
            double[,] mask = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    mask[i, j] = 1.0 / 9.0;
                }
            }

            var colors = getColors(bitmap);
            Bitmap newBitmap = new Bitmap(bitmap);

            for (int i = 3; i < bitmap.Width - 3; i++)
            {
                for (int j = 3; j < bitmap.Height - 3; j++)
                {
                    newBitmap.SetPixel(i, j, GetColorValueLinear(i, j, mask, colors));
                }
            }
            return newBitmap;
        }
        private static Color GetColorValueLinear(int x, int y, double[,] mask, Color[,] colors)
        {
            var R = 0;
            var G = 0;
            var B = 0;
            var maskI = 0;
            var maskJ = 0;
            for (int i = x - 1; i <= x + 1; i++)
            {
                maskJ = 0;
                for (int j = y - 1; j <= y + 1; j++)
                {
                    R += (int)(colors[i, j].R * mask[maskI, maskJ]);
                    G += (int)(colors[i, j].G * mask[maskI, maskJ]);
                    B += (int)(colors[i, j].B * mask[maskI, maskJ]);
                    maskJ++;
                }
                maskI++;
            }
            R = R < 0 ? 0 : R > 255 ? 255 : R;
            G = G < 0 ? 0 : G > 255 ? 255 : G;
            B = B < 0 ? 0 : B > 255 ? 255 : B;
            return Color.FromArgb(R, G, B);
        }


    }
}
