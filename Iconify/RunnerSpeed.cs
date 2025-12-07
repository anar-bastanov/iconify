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

internal readonly record struct RunnerSpeed(uint value) : IClosedEnum<RunnerSpeed>
{
    public const uint
        X100 = 0,
        X125 = 1,
        X150 = 2,
        X175 = 3,
        X200 = 4;

    private static ReadOnlySpan<uint> EnumerationValues => [
        X100,
        X125,
        X150,
        X175,
        X200
    ];

    public uint Value => value;

    public int GetDelay()
    {
        // Each animation is assumed to be 8 frames per second
        const float interval = 1000.0f / 8;

        return value switch
        {
            X100 => (int)(interval / 1.00f),
            X125 => (int)(interval / 1.25f),
            X150 => (int)(interval / 1.50f),
            X175 => (int)(interval / 1.75f),
            X200 => (int)(interval / 2.00f),
            _    => (int)(interval / 1.00f)
        };
    }

    public string GetString()
    {
        return value switch
        {
            X100 => "100%",
            X125 => "125%",
            X150 => "150%",
            X175 => "175%",
            X200 => "200%",
            _    => ""
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out RunnerSpeed result)
    {
        RunnerSpeed? nullableResult = value switch
        {
            "100%" => X100,
            "125%" => X125,
            "150%" => X150,
            "175%" => X175,
            "200%" => X200,
            _      => null
        };

        result = nullableResult.GetValueOrDefault();
        return nullableResult.HasValue;
    }

    public static ReadOnlySpan<RunnerSpeed> GetValues()
    {
        return MemoryMarshal.Cast<uint, RunnerSpeed>(EnumerationValues);
    }

    public static implicit operator uint(RunnerSpeed runnerSpeed) => runnerSpeed.Value;

    public static implicit operator RunnerSpeed(uint value) => new(value);
}
