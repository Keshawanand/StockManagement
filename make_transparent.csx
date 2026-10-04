#!/usr/bin/env dotnet-script
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

var src = @"D:\Stock Management\StockManagement\StockManagementApp\Resources\logo.png";

var bmp = new Bitmap(src);
var output = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
using (var g = Graphics.FromImage(output))
    g.DrawImage(bmp, 0, 0);
bmp.Dispose();

var rect    = new Rectangle(0, 0, output.Width, output.Height);
var bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
var bytes   = new byte[Math.Abs(bmpData.Stride) * bmpData.Height];
Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length);

const int tol = 35;
for (int i = 0; i < bytes.Length; i += 4)
{
    byte b = bytes[i], gr = bytes[i + 1], r = bytes[i + 2], a = bytes[i + 3];
    if (a > 0 && r >= 255 - tol && gr >= 255 - tol && b >= 255 - tol)
        bytes[i] = bytes[i+1] = bytes[i+2] = bytes[i+3] = 0;
}

Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length);
output.UnlockBits(bmpData);
output.Save(src, ImageFormat.Png);
output.Dispose();
Console.WriteLine("Done — transparent background saved.");
