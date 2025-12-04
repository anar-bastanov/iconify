// Copyright 2025 Anar Bastanov
// Copyright 2025 Takuto Nakamura
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Iconify;

internal readonly record struct Theme(uint value) : IClosedEnum<Theme>
{
    public const uint
        System  = 0,
        White   = 1,
        Gray    = 2,
        Black   = 3,
        Red     = 10,
        Orange  = 11,
        Yellow  = 12,
        Lime    = 13,
        Green   = 14,
        Teal    = 15,
        Cyan    = 16,
        Blue    = 17,
        Purple  = 18,
        Pink    = 19;

    private static ReadOnlySpan<uint> EnumerationValues => [
        System,
        White,
        Gray,
        Black,
        Red,
        Orange,
        Yellow,
        Lime,
        Green,
        Teal,
        Cyan,
        Blue,
        Purple,
        Pink
    ];

    public uint Value => value;

    public Color? GetAccentColor()
    {
        return Value switch
        {
            Gray   => Color.FromArgb(255, 148, 163, 184),
            Red    => Color.FromArgb(255, 248,  68,  68),
            Orange => Color.FromArgb(255, 252, 129,  47),
            Yellow => Color.FromArgb(255, 250, 204,  21),
            Lime   => Color.FromArgb(255, 190, 242, 100),
            Green  => Color.FromArgb(255,  46, 204,  64),
            Teal   => Color.FromArgb(255,  33, 163, 151),
            Cyan   => Color.FromArgb(255,   0, 225, 245),
            Blue   => Color.FromArgb(255,  59, 130, 246),
            Purple => Color.FromArgb(255, 147, 112, 250),
            Pink   => Color.FromArgb(255, 236,  72, 153),
            _      => null
        };
    }

    public Theme ResolveBaseTheme(Theme systemTheme)
    {
        return Value is Black ? Black : White;
    }

    public string GetString()
    {
        return value switch
        {
            System => nameof(System),
            White  => nameof(White),
            Gray   => nameof(Gray),
            Black  => nameof(Black),
            Red    => nameof(Red),
            Orange => nameof(Orange),
            Yellow => nameof(Yellow),
            Lime   => nameof(Lime),
            Green  => nameof(Green),
            Teal   => nameof(Teal),
            Cyan   => nameof(Cyan),
            Blue   => nameof(Blue),
            Purple => nameof(Purple),
            Pink   => nameof(Pink),
            _      => ""
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out Theme result)
    {
        Theme? nullableResult = value switch
        {
            nameof(System) => System,
            nameof(White)  => White,
            nameof(Gray)   => Gray,
            nameof(Black)  => Black,
            nameof(Red)    => Red,
            nameof(Orange) => Orange,
            nameof(Yellow) => Yellow,
            nameof(Lime)   => Lime,
            nameof(Green)  => Green,
            nameof(Teal)   => Teal,
            nameof(Cyan)   => Cyan,
            nameof(Blue)   => Blue,
            nameof(Purple) => Purple,
            nameof(Pink)   => Pink,
            _              => null
        };

        result = nullableResult.GetValueOrDefault();
        return nullableResult.HasValue;
    }

    public static ReadOnlySpan<Theme> GetValues()
    {
        return MemoryMarshal.Cast<uint, Theme>(EnumerationValues);
    }

    public static implicit operator uint(Theme theme) => theme.Value;

    public static implicit operator Theme(uint value) => new(value);
}
