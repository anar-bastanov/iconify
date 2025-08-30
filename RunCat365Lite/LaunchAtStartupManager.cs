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

using Microsoft.Win32;

namespace RunCat365Lite;

internal sealed class LaunchAtStartupManager
{
    private const string KeyName = @"Software\Microsoft\Windows\CurrentVersion\Run";

    private readonly string AppName = Application.ProductName!;

    public bool GetStartup()
    {
        using var rKey = Registry.CurrentUser.OpenSubKey(KeyName);

        return rKey is not null && rKey.GetValue(AppName) is not null;
    }

    public bool SetStartup(bool enabled)
    {
        using var rKey = Registry.CurrentUser.OpenSubKey(KeyName, writable: true);

        if (rKey is null)
            return false;

        if (enabled)
        {
            var exePath = Environment.ProcessPath;

            if (exePath is null)
                return false;

            rKey.SetValue(AppName, exePath);
        }
        else
        {
            rKey.DeleteValue(AppName, throwOnMissingValue: false);
        }

        return true;
    }
}
