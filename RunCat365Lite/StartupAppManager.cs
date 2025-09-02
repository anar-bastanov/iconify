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

internal static class StartupAppManager
{
    public static bool GetStartup()
    {
        using var rKey = Registry.CurrentUser.OpenSubKey(AppStrings.RegistryNameStartupApps);

        return rKey?.GetValue(AppStrings.ApplicationName) is not null;
    }

    public static bool SetStartup(bool enable)
    {
        using var rKey = Registry.CurrentUser.OpenSubKey(AppStrings.RegistryNameStartupApps, writable: true);

        if (rKey is null)
            return false;

        if (enable)
        {
            if (Environment.ProcessPath is not string exePath)
                return false;

            rKey.SetValue(AppStrings.ApplicationName, exePath);
        }
        else
        {
            rKey.DeleteValue(AppStrings.ApplicationName, throwOnMissingValue: false);
        }

        return true;
    }
}
