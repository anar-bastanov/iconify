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
        Magenta = 19;

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
        Magenta
    ];

    public uint Value => value;

    public bool IsAccentTheme()
    {
        return Value is
            Gray or Red or Orange or Yellow or
            Lime or Green or Teal or Cyan or
            Blue or Purple or Magenta;
    }

    public Color? GetAccentColor()
    {
        return Value switch
        {
            Gray    => Color.FromArgb(255, 107, 114, 128),
            Red     => Color.FromArgb(255, 239, 68,  68),
            Orange  => Color.FromArgb(255, 249, 115, 22),
            Yellow  => Color.FromArgb(255, 234, 179, 8),
            Lime    => Color.FromArgb(255, 132, 204, 22),
            Green   => Color.FromArgb(255, 34,  197, 94),
            Teal    => Color.FromArgb(255, 20,  184, 166),
            Cyan    => Color.FromArgb(255, 0,   199, 252),
            Blue    => Color.FromArgb(255, 59,  130, 246),
            Purple  => Color.FromArgb(255, 139, 92,  246),
            Magenta => Color.FromArgb(255, 217, 70,  239),
            _ => null
        };
    }

    public Theme ResolveBaseTheme(Theme systemTheme)
    {
        if (Value is System)
            return systemTheme;

        return IsAccentTheme() ? White : Value;
    }

    public string GetString()
    {
        return value switch
        {
            System  => nameof(System),
            White   => nameof(White),
            Gray    => nameof(Gray),
            Black   => nameof(Black),

            Red     => nameof(Red),
            Orange  => nameof(Orange),
            Yellow  => nameof(Yellow),
            Lime    => nameof(Lime),
            Green   => nameof(Green),
            Teal    => nameof(Teal),
            Cyan    => nameof(Cyan),
            Blue    => nameof(Blue),
            Purple  => nameof(Purple),
            Magenta => nameof(Magenta),

            _       => ""
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out Theme result)
    {
        Theme? nullableResult = value switch
        {
            nameof(System)  => System,
            nameof(White)   => White,
            nameof(Gray)    => Gray,
            nameof(Black)   => Black,

            nameof(Red)     => Red,
            nameof(Orange)  => Orange,
            nameof(Yellow)  => Yellow,
            nameof(Lime)    => Lime,
            nameof(Green)   => Green,
            nameof(Teal)    => Teal,
            nameof(Cyan)    => Cyan,
            nameof(Blue)    => Blue,
            nameof(Purple)  => Purple,
            nameof(Magenta) => Magenta,

            _               => null
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
