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
    public static partial bool DestroyIcon(nint handle);

    public static Icon CreateTintedIcon(Icon baseIcon, Color accent)
    {
        using var srcBitmap = baseIcon.ToBitmap();
        using var tintedBitmap = TintBitmap(srcBitmap, accent);

        nint hIcon = tintedBitmap.GetHicon();

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
                var src = source.GetPixel(x, y);
                var dst = src.A is 0 ? Color.Transparent :
                    Color.FromArgb(src.A, accent.R, accent.G, accent.B);

                result.SetPixel(x, y, dst);
            }
        }

        return result;
    }
}
