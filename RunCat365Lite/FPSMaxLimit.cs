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

internal readonly record struct FpsMaxLimit(uint value) : IClosedEnum<FpsMaxLimit>
{
    public const uint
        Fps10 = 0,
        Fps20 = 1,
        Fps30 = 2,
        Fps40 = 3;

    private static ReadOnlySpan<uint> EnumerationValues => [
        Fps10,
        Fps20,
        Fps30,
        Fps40
    ];

    public uint Value => value;

    public int GetIntervalMs()
    {
        const float rate = 50.0f;

        return value switch
        {
            Fps10 => (int)(rate / 0.25f),
            Fps20 => (int)(rate / 0.5f),
            Fps30 => (int)(rate / 0.75f),
            Fps40 => (int)(rate / 1.0f),
            _ =>     (int)(rate / 1.0f),
        };
    }

    public string GetString()
    {
        return value switch
        {
            Fps10 => nameof(Fps10),
            Fps20 => nameof(Fps20),
            Fps30 => nameof(Fps30),
            Fps40 => nameof(Fps40),
            _ => "",
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out FpsMaxLimit result)
    {
        FpsMaxLimit? nullableResult = value switch
        {
            nameof(Fps10) => Fps10,
            nameof(Fps20) => Fps20,
            nameof(Fps30) => Fps30,
            nameof(Fps40) => Fps40,
            _ => null,
        };

        result = nullableResult.GetValueOrDefault();
        return nullableResult.HasValue;
    }

    public static ReadOnlySpan<FpsMaxLimit> GetValues() => MemoryMarshal.Cast<uint, FpsMaxLimit>(EnumerationValues);

    public static implicit operator uint(FpsMaxLimit arg) => arg.Value;

    public static implicit operator FpsMaxLimit(uint value) => new(value);
}
