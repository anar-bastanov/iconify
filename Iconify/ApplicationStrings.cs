// Copyright 2025 Anar Bastanov
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

global using AppStrings = Iconify.ApplicationStrings;

namespace Iconify;

public static class ApplicationStrings
{
    public static readonly string ApplicationName = Application.ProductName!;

    public const string ApplicationGuid = "414E4152-8DF7-4EC1-9432-7E5EF7CB03C4";

    public static readonly string GlobalMutexName = @$"Global\{ApplicationName}_{ApplicationGuid}";

    public const string RepositoryLink = "https://github.com/anar-bastanov/iconify";

    public const string RegistryNamePersonalization = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public const string RegistryNameStartupApps = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public const string RegistryKeyIsLightTheme = "SystemUsesLightTheme";
}
