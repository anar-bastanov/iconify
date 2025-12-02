// Copyright 2025 Anar Bastanov
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Iconify;

internal static partial class IconColorizer
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon(IntPtr handle);

    public static Icon CreateTintedIcon(Icon baseIcon, Color accent)
    {
        using var srcBitmap = baseIcon.ToBitmap();
        using var tintedBitmap = TintBitmap(srcBitmap, accent);

        IntPtr hIcon = tintedBitmap.GetHicon();

        try
        {
            using var tempIcon = Icon.FromHandle(hIcon);
            return (Icon)tempIcon.Clone();
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static Bitmap TintBitmap(Bitmap source, Color accent)
    {
        var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

        for (int y = 0; y < source.Height; ++y)
        {
            for (int x = 0; x < source.Width; ++x)
            {
                var c = source.GetPixel(x, y);

                if (c.A is 0)
                {
                    result.SetPixel(x, y, Color.Transparent);
                    continue;
                }

                float intensity = (0.299f * c.R + 0.587f * c.G + 0.114f * c.B) / 255.0f;

                int r = (int)(accent.R * intensity);
                int g = (int)(accent.G * intensity);
                int b = (int)(accent.B * intensity);

                result.SetPixel(x, y, Color.FromArgb(c.A, r, g, b));
            }
        }

        return result;
    }
}
