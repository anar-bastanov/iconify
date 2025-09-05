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

namespace RunCat365Lite;

internal readonly record struct Theme(uint value) : IClosedEnum<Theme>
{
    public const uint
        System = 0,
        Light  = 1,
        Dark   = 2;

    private static ReadOnlySpan<uint> EnumerationValues => [
        System,
        Light,
        Dark
    ];

    public uint Value => value;

    public string GetString()
    {
        return value switch
        {
            System => nameof(System),
            Light  => nameof(Light),
            Dark   => nameof(Dark),
            _      => ""
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out Theme result)
    {
        Theme? nullableResult = value switch
        {
            nameof(System) => System,
            nameof(Light)  => Light,
            nameof(Dark)   => Dark,
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
