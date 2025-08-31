// Copyright 2025 Anar Bastanov
// Copyright 2020 Takuto Nakamura
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
using RunCat365Lite.Properties;
using System.Diagnostics;
using FormsTimer = System.Windows.Forms.Timer;

namespace RunCat365Lite;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Terminate RunCat 365 Lite if there is any existing instance.
        using var processMutex = new Mutex(true, "_RUNCATLITE_MUTEX", out var result);

        if (!result)
            return;

        try
        {
            ApplicationConfiguration.Initialize();
            Application.SetColorMode(SystemColorMode.System);
            Application.Run(new RunCat365LiteApplicationContext());
        }
        finally
        {
            processMutex.ReleaseMutex();
        }
    }
}

internal class RunCat365LiteApplicationContext : ApplicationContext
{
    private readonly LaunchAtStartupManager LaunchAtStartupManager;

    private readonly ContextMenuManager ContextMenuManager;

    private readonly FormsTimer AnimateTimer;

    private Runner Runner = Runner.Cat;

    private Theme ManualTheme = Theme.System;

    private FPSMaxLimit FpsMaxLimit = FPSMaxLimit.FPS40;

    public RunCat365LiteApplicationContext()
    {
        UserSettings.Default.Reload();
        _ = Enum.TryParse(UserSettings.Default.Runner, out Runner);
        _ = Enum.TryParse(UserSettings.Default.Theme, out ManualTheme);
        _ = Enum.TryParse(UserSettings.Default.FPSMaxLimit, out FpsMaxLimit);

        SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(UserPreferenceChanged);

        LaunchAtStartupManager = new LaunchAtStartupManager();

        ContextMenuManager = new ContextMenuManager(
            () => Runner,
            ChangeRunner,
            GetSystemTheme,
            () => ManualTheme,
            ChangeManualTheme,
            () => FpsMaxLimit,
            ChangeFPSMaxLimit,
            LaunchAtStartupManager.GetStartup,
            LaunchAtStartupManager.SetStartup,
            OpenRepository,
            Application.Exit
        );

        ContextMenuManager.SetNotifyIconText("RunCat 365 Lite");

        AnimateTimer = new FormsTimer
        {
            Interval = CalculateInterval()
        };
        AnimateTimer.Tick += new EventHandler(AnimationTick);
        AnimateTimer.Start();

        ShowBalloonTip();
    }

    private static Theme GetSystemTheme()
    {
        var keyName = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var rKey = Registry.CurrentUser.OpenSubKey(keyName);
        if (rKey is null) return Theme.Light;
        var value = rKey.GetValue("SystemUsesLightTheme");
        if (value is null) return Theme.Light;
        return value is 0 ? Theme.Dark : Theme.Light;
    }

    private void ShowBalloonTip()
    {
        if (UserSettings.Default.FirstLaunch)
        {
            ContextMenuManager.ShowBalloonTip();
            UserSettings.Default.FirstLaunch = false;
            UserSettings.Default.Save();
        }
    }

    private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General)
        {
            var systemTheme = GetSystemTheme();
            ContextMenuManager.SetIcons(systemTheme, ManualTheme, Runner);
        }
    }

    private static void OpenRepository()
    {
        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://github.com/anar-bastanov/run-cat-365-lite",
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    private void ChangeRunner(Runner r)
    {
        Runner = r;
        UserSettings.Default.Runner = Runner.ToString();
        UserSettings.Default.Save();
    }

    private void ChangeManualTheme(Theme t)
    {
        ManualTheme = t;
        UserSettings.Default.Theme = ManualTheme.ToString();
        UserSettings.Default.Save();
    }

    private void ChangeFPSMaxLimit(FPSMaxLimit f)
    {
        FpsMaxLimit = f;
        UserSettings.Default.FPSMaxLimit = FpsMaxLimit.ToString();
        UserSettings.Default.Save();

        AnimateTimer.Stop();
        AnimateTimer.Interval = CalculateInterval();
        AnimateTimer.Start();
    }

    private void AnimationTick(object? sender, EventArgs e)
    {
        ContextMenuManager.AdvanceFrame();
    }

    private int CalculateInterval()
    {
        return (int)(50.0f / FpsMaxLimit.GetRate());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SystemEvents.UserPreferenceChanged -= UserPreferenceChanged;

            AnimateTimer?.Stop();
            AnimateTimer?.Dispose();

            ContextMenuManager?.HideNotifyIcon();
            ContextMenuManager?.Dispose();
        }

        base.Dispose(disposing);
    }
}
