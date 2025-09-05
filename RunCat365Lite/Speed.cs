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

internal readonly record struct Speed(uint value) : IClosedEnum<Speed>
{
    public const uint
        X050 = 0,
        X075 = 1,
        X100 = 2,
        X150 = 3,
        X200 = 4;

    private static ReadOnlySpan<uint> EnumerationValues => [
        X050,
        X075,
        X100,
        X150,
        X200
    ];

    public uint Value => value;

    public int GetDelay()
    {
        // Each animation is assumed to 8 frames per second
        const float interval = 1000.0f / 8;

        return value switch
        {
            X050 => (int)(interval / 0.50f),
            X075 => (int)(interval / 0.75f),
            X100 => (int)(interval / 1.00f),
            X150 => (int)(interval / 1.50f),
            X200 => (int)(interval / 2.00f),
            _    => (int)(interval / 1.00f)
        };
    }

    public string GetString()
    {
        return value switch
        {
            X050 =>  "50%",
            X075 =>  "75%",
            X100 => "100%",
            X150 => "150%",
            X200 => "200%",
            _    => ""
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out Speed result)
    {
        Speed? nullableResult = value switch
        {
             "50%" => X050,
             "75%" => X075,
            "100%" => X100,
            "150%" => X150,
            "200%" => X200,
            _      => null
        };

        result = nullableResult.GetValueOrDefault();
        return nullableResult.HasValue;
    }

    public static ReadOnlySpan<Speed> GetValues()
    {
        return MemoryMarshal.Cast<uint, Speed>(EnumerationValues);
    }

    public static implicit operator uint(Speed speed) => speed.Value;

    public static implicit operator Speed(uint value) => new(value);
}
