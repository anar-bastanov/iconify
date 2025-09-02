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

internal readonly record struct Runner(uint value) : IClosedEnum<Runner>
{
    public const uint
        Cat = 0,
        Parrot = 1,
        Horse = 2;

    private static ReadOnlySpan<uint> EnumerationValues => [
        Cat,
        Parrot,
        Horse,
    ];

    public uint Value => value;

    public int GetFrameNumber()
    {
        return value switch
        {
            Cat => 5,
            Parrot => 10,
            Horse => 14,
            _ => 0,
        };
    }

    public string GetString()
    {
        return value switch
        {
            Cat => nameof(Cat),
            Parrot => nameof(Parrot),
            Horse => nameof(Horse),
            _ => "",
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out Runner result)
    {
        Runner? nullableResult = value switch
        {
            nameof(Cat) => Cat,
            nameof(Parrot) => Parrot,
            nameof(Horse) => Horse,
            _ => null,
        };

        result = nullableResult.GetValueOrDefault();
        return nullableResult.HasValue;
    }

    public static ReadOnlySpan<Runner> GetValues() => MemoryMarshal.Cast<uint, Runner>(EnumerationValues);

    public static implicit operator uint(Runner arg) => arg.Value;

    public static implicit operator Runner(uint value) => new(value);
}
