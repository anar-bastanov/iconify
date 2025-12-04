# Iconify

**Animated tray icons on your Windows taskbar.**

Forked and stripped down from [RunCat 365](https://github.com/Kyome22/RunCat365) by [Kyome22](https://github.com/Kyome22), go give it a star!

## Features

- Animations in the tray
- Very lightweight
- Not much else

## Preview

![Preview](/res/iconify-thumbnail-480p.png)

## Requirements

- Windows 10 version 19041 (20H1) or later, or Windows 11
- 64-bit (x64) operating system

## Installation

Download and extract one of the archives below, then run `Iconify.exe`:

| [Portable build](https://github.com/anar-bastanov/iconify/releases/latest/download/Iconify-portable-win-x64.zip) | [Framework-dependent build](https://github.com/anar-bastanov/iconify/releases/latest/download/Iconify-runtime-win-x64.zip) |
|:-|:-|
| Bigger download | Smaller download |
| No .NET runtime required | Requires [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Executable can be moved anywhere | Executable must stay with other files |

> [!TIP]
> Not sure which one to choose? Get the **portable build**, it works out of the box.

> [!NOTE]
> To check if .NET is installed, run:
> ```sh
> dotnet --list-runtimes
> ```
> Look for `Microsoft.WindowsDesktop.App 10.x`.

> [!TIP]
> For a cleaner setup, put the extracted folder in `%ProgramFiles%` (system-wide, admin required)  or `%LOCALAPPDATA%` (per-user).

## License

Copyright &copy; 2025 Anar Bastanov  
Licensed under the [Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0).  
Includes code from RunCat365 by Kyome22.
