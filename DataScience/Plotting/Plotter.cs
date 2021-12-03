using BAVCL.Geometric;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;


namespace BAVCL.Plotting
{
    public class Plotter
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public void Noise(int width= 800, int height=600)
        {
            Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            Rectangle BoundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(BoundsRect, ImageLockMode.WriteOnly,b.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            Random rnd = new Random();

            byte[] arr = new byte[width * height * 3];

            rnd.NextBytes(arr);


            // fill in rgbValues
            Marshal.Copy(arr, 0, ptr, arr.Length);
            b.UnlockBits(bmpData); 
            b.Save(@"C:\Users\marce\source\repos\DataScience\DataScience\Plotting\Saved Plots\noise.raw", ImageFormat.Bmp);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public void Line(int width = 800, int height = 600)
        {
            Bitmap b = new Bitmap(width, height, PixelFormat.Format32bppRgb);

            Rectangle BoundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(BoundsRect, ImageLockMode.WriteOnly, b.PixelFormat);
            IntPtr ptr = bmpData.Scan0;


            float[] p1 = new float[2] { 10, 10 };
            float[] p2 = new float[2] { 500, 500 };

            float m = (p2[1] - p1[1]) / (p2[0] - p1[0]);

            GPU gpu = new GPU();
            float[] Data = new float[width * height * 4];
            Array.Fill<float>(Data, 255);
            Vector3 vector = new Vector3(gpu, Data);
            Vector3 vectorMask = Vector3.Fill(gpu, 1, (width * height) << 2);

            Vector range = Vector.Arange(gpu, p1[0], p2[0], 1);
            range.OP_IP(m, Operations.multiply).OP_IP(p1[1], Operations.add);

            range.SyncCPU();
            HashSet<float> idxs = range.Value.ToHashSet();
            byte min = (byte)range.Value.Min();
            byte add = (byte)(min < 0 ? -min : 0);

            byte[] arr = new byte[width * height * 4];
            for (int i = 0, j=0; i < arr.Length; i+=4,j++)
            {
                arr[i] = (byte)(((byte)Data[i]) * Convert.ToByte(!idxs.Contains(j+add)));
                arr[i+1] = (byte)(((byte)Data[i+1]) * Convert.ToByte(!idxs.Contains(j + add)));
                arr[i+2] = (byte)(((byte)Data[i+2]) * Convert.ToByte(!idxs.Contains(j + add)));
                arr[i+3] = (byte)(((byte)Data[i+3]) * Convert.ToByte(!idxs.Contains(j + add)));
            }
            

            // fill in rgbValues
            Marshal.Copy(arr, 0, ptr, arr.Length);
            b.UnlockBits(bmpData);
            b.Save(@"C:\Users\marce\source\repos\DataScience\DataScience\Plotting\Saved Plots\test.raw", ImageFormat.Bmp);
        }




    }



}
