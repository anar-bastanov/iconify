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

internal readonly record struct Runner(uint value) : IClosedEnum<Runner>
{
    public const uint
        Cat     = 0,
        Bird    = 1,
        Cloud   = 2,
        Flame   = 3,
        Eye     = 4,
        YinYang = 5;

    private static ReadOnlySpan<uint> EnumerationValues => [
        Cat,
        Bird,
        Cloud,
        Flame,
        Eye,
        YinYang
    ];

    public uint Value => value;

    public int GetFrameNumber()
    {
        // Hardcoded values for the total number of sprites per animation in project resources
        return value switch
        {
            Cat     => 5,
            Bird    => 6,
            Cloud   => 24,
            Flame   => 16,
            Eye     => 35,
            YinYang => 8,
            _       => 0
        };
    }

    public string GetString()
    {
        return value switch
        {
            Cat     => nameof(Cat),
            Bird    => nameof(Bird),
            Cloud   => nameof(Cloud),
            Flame   => nameof(Flame),
            Eye     => nameof(Eye),
            YinYang => nameof(YinYang),
            _       => ""
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out Runner result)
    {
        Runner? nullableResult = value switch
        {
            nameof(Cat)     => Cat,
            nameof(Bird)    => Bird,
            nameof(Cloud)   => Cloud,
            nameof(Flame)   => Flame,
            nameof(Eye)     => Eye,
            nameof(YinYang) => YinYang,
            _               => null
        };

        result = nullableResult.GetValueOrDefault();
        return nullableResult.HasValue;
    }

    public static ReadOnlySpan<Runner> GetValues()
    {
        return MemoryMarshal.Cast<uint, Runner>(EnumerationValues);
    }

    public static implicit operator uint(Runner runner) => runner.Value;

    public static implicit operator Runner(uint value) => new(value);
}
