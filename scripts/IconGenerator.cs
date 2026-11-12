// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class IconGenerator
{
    private class IconEntry
    {
        public int Size { get; set; }
        public byte[] Bytes { get; set; }
    }

    public static void Generate(string outputPath)
    {
        int[] sizes = new int[] { 256, 64, 48, 32, 16 };
        List<IconEntry> images = new List<IconEntry>();

        foreach (int size in sizes)
        {
            using (Bitmap bmp = DrawStardustIcon(size))
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                IconEntry entry = new IconEntry();
                entry.Size = size;
                entry.Bytes = ms.ToArray();
                images.Add(entry);
            }
        }

        string dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (FileStream fs = File.Create(outputPath))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            // ICONDIR
            bw.Write((ushort)0); // reserved
            bw.Write((ushort)1); // type 1 = icon
            bw.Write((ushort)images.Count); // image count

            // Entries
            int offset = 6 + (16 * images.Count);
            foreach (IconEntry img in images)
            {
                byte w = img.Size >= 256 ? (byte)0 : (byte)img.Size;
                byte h = img.Size >= 256 ? (byte)0 : (byte)img.Size;

                bw.Write(w);
                bw.Write(h);
                bw.Write((byte)0); // color count
                bw.Write((byte)0); // reserved
                bw.Write((ushort)1); // color planes
                bw.Write((ushort)32); // bits per pixel
                bw.Write((uint)img.Bytes.Length);
                bw.Write((uint)offset);

                offset += img.Bytes.Length;
            }

            // Image bytes (PNG encoded)
            foreach (IconEntry img in images)
            {
                bw.Write(img.Bytes);
            }

            bw.Flush();
        }

        Console.WriteLine(string.Format("Successfully generated icon at {0} with {1} sizes.", outputPath, images.Count));
    }

    private static Bitmap DrawStardustIcon(int size)
    {
        Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            float s = size / 256.0f;
            float center = size / 2.0f;

            // 1. Dark cosmic sphere background (#0B0F17 to deep nebula purple #18122B)
            float outerMargin = 12.0f * s;
            float outerSize = size - (2.0f * outerMargin);
            RectangleF outerRect = new RectangleF(outerMargin, outerMargin, outerSize, outerSize);

            using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                outerRect,
                Color.FromArgb(245, 11, 15, 23),
                Color.FromArgb(245, 24, 18, 43),
                LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(bgBrush, outerRect);
            }

            // 2. Neon cyan outer ring
            float ringThickness = Math.Max(1.5f, 7.0f * s);
            using (Pen ringPen = new Pen(Color.FromArgb(230, 0, 240, 255), ringThickness))
            {
                g.DrawEllipse(ringPen, outerRect);
            }

            // 3. Stardust Amber dashed inner orbit ring
            float innerMargin = 26.0f * s;
            float innerSize = size - (2.0f * innerMargin);
            RectangleF innerRect = new RectangleF(innerMargin, innerMargin, innerSize, innerSize);
            float amberThickness = Math.Max(1.0f, 2.5f * s);
            using (Pen amberPen = new Pen(Color.FromArgb(180, 245, 158, 11), amberThickness))
            {
                amberPen.DashStyle = DashStyle.Dash;
                g.DrawEllipse(amberPen, innerRect);
            }

            // 4. Central 4-point Stardust Star & Inversion Crystal
            float starRadiusOuter = 80.0f * s;
            float starRadiusInner = 24.0f * s;

            PointF[] starPoints = new PointF[]
            {
                new PointF(center, center - starRadiusOuter),
                new PointF(center + starRadiusInner, center - starRadiusInner),
                new PointF(center + starRadiusOuter, center),
                new PointF(center + starRadiusInner, center + starRadiusInner),
                new PointF(center, center + starRadiusOuter),
                new PointF(center - starRadiusInner, center + starRadiusInner),
                new PointF(center - starRadiusOuter, center),
                new PointF(center - starRadiusInner, center - starRadiusInner)
            };

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(starPoints);

                using (LinearGradientBrush starBrush = new LinearGradientBrush(
                    outerRect,
                    Color.FromArgb(255, 0, 240, 255),    // Neon cyan
                    Color.FromArgb(255, 168, 85, 247),   // Cosmic purple
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(starBrush, path);
                }

                using (Pen starPen = new Pen(Color.FromArgb(255, 255, 255, 255), Math.Max(1.0f, 2.0f * s)))
                {
                    g.DrawPath(starPen, path);
                }
            }

            // 5. White glowing core
            float coreRadius = 14.0f * s;
            RectangleF coreRect = new RectangleF(center - coreRadius, center - coreRadius, coreRadius * 2, coreRadius * 2);
            using (SolidBrush coreBrush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
            {
                g.FillEllipse(coreBrush, coreRect);
            }

            // 6. Gold stardust diamond in the center
            float diagRadius = 7.0f * s;
            PointF[] diagPoints = new PointF[]
            {
                new PointF(center, center - diagRadius),
                new PointF(center + diagRadius, center),
                new PointF(center, center + diagRadius),
                new PointF(center - diagRadius, center)
            };
            using (SolidBrush goldBrush = new SolidBrush(Color.FromArgb(255, 250, 204, 21)))
            {
                g.FillPolygon(goldBrush, diagPoints);
            }
        }

        return bmp;
    }
}
