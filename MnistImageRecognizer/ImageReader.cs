using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics.Tensors;
using System.Text;


namespace MnistImageRecognizer
{
    class ImageReader
    {
        public static Tensor<float> GetTensorFromImageFile(string filepath)
        {
            Bitmap bitmap = GetBitmapFromFile(filepath);
            Tensor<float> tensor = ConvertImageToTensor(bitmap);
            return tensor;
        }
        
        public static Bitmap GetBitmapFromFile(string filepath)
        {
            return new Bitmap(filepath);
        }

        public static Bitmap ResizeBitmapToMnist1(Bitmap oldBitmap)
        {
            var newSize = new Size(28, 28);
            Bitmap newBitmap = new Bitmap(oldBitmap, newSize);
            return newBitmap;
        }
        public static Bitmap ResizeBitmapToMnist(Bitmap imgToResize)
        {
            Size size = new Size(28, 28);
            Bitmap b = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
            }
            return b;
        }
        public static Tensor<float> ConvertImageToTensor(Bitmap image)
        {
            image = ResizeBitmapToMnist(image);
            Tensor<float> imageTensor = new DenseTensor<float>(new[] {1, 1, 28, 28 });
            for (int y = 0; y < 28; y++)
            {
                for (int x = 0; x < 28; x++)
                {
                    Color color = image.GetPixel(x, y);
                    float pixelValue = (float)(0.3*color.R + 0.59*color.B + 0.11*color.G);
                    imageTensor[0, 0, y, x] = 255 - pixelValue;
                }
            }
            return imageTensor;
        }
    }
}
