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
        // Terminate RunCat365Lite if there iss any existing instance.
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
    private const int FetchTimerDefaultInterval = 1000;

    private const int FetchCounterSize = 5;

    private readonly CPURepository CpuRepository;

    private readonly MemoryRepository MemoryRepository;

    private readonly StorageRepository StorageRepository;

    private readonly LaunchAtStartupManager LaunchAtStartupManager;

    private readonly ContextMenuManager ContextMenuManager;

    private readonly FormsTimer FetchTimer;

    private readonly FormsTimer AnimateTimer;

    private Runner runner = Runner.Cat;

    private Theme manualTheme = Theme.System;

    private FPSMaxLimit fpsMaxLimit = FPSMaxLimit.FPS40;

    private int FetchCounter = 5;

    public RunCat365LiteApplicationContext()
    {
        UserSettings.Default.Reload();
        _ = Enum.TryParse(UserSettings.Default.Runner, out runner);
        _ = Enum.TryParse(UserSettings.Default.Theme, out manualTheme);
        _ = Enum.TryParse(UserSettings.Default.FPSMaxLimit, out fpsMaxLimit);

        SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(UserPreferenceChanged);

        CpuRepository = new CPURepository();
        MemoryRepository = new MemoryRepository();
        StorageRepository = new StorageRepository();
        LaunchAtStartupManager = new LaunchAtStartupManager();

        ContextMenuManager = new ContextMenuManager(
            () => runner,
            ChangeRunner,
            GetSystemTheme,
            () => manualTheme,
            ChangeManualTheme,
            () => fpsMaxLimit,
            ChangeFPSMaxLimit,
            LaunchAtStartupManager.GetStartup,
            LaunchAtStartupManager.SetStartup,
            OpenRepository,
            Application.Exit
        );

        AnimateTimer = new FormsTimer
        {
            Interval = CalculateInterval()
        };
        AnimateTimer.Tick += new EventHandler(AnimationTick);
        AnimateTimer.Start();

        FetchTimer = new FormsTimer
        {
            Interval = FetchTimerDefaultInterval
        };
        FetchTimer.Tick += new EventHandler(FetchTick);
        FetchTimer.Start();

        ShowBalloonTip();
    }

    private static Theme GetSystemTheme()
    {
        var keyName = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var rKey = Registry.CurrentUser.OpenSubKey(keyName);
        if (rKey is null) return Theme.Light;
        var value = rKey.GetValue("SystemUsesLightTheme");
        if (value is null) return Theme.Light;
        return (int)value == 0 ? Theme.Dark : Theme.Light;
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
            ContextMenuManager.SetIcons(systemTheme, manualTheme, runner);
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
        runner = r;
        UserSettings.Default.Runner = runner.ToString();
        UserSettings.Default.Save();
    }

    private void ChangeManualTheme(Theme t)
    {
        manualTheme = t;
        UserSettings.Default.Theme = manualTheme.ToString();
        UserSettings.Default.Save();
    }

    private void ChangeFPSMaxLimit(FPSMaxLimit f)
    {
        fpsMaxLimit = f;
        UserSettings.Default.FPSMaxLimit = fpsMaxLimit.ToString();
        UserSettings.Default.Save();

        AnimateTimer.Stop();
        AnimateTimer.Interval = CalculateInterval();
        AnimateTimer.Start();
    }

    private void AnimationTick(object? sender, EventArgs e)
    {
        ContextMenuManager.AdvanceFrame();
    }

    private void FetchSystemInfo(
        CPUInfo cpuInfo,
        MemoryInfo memoryInfo,
        List<StorageInfo> storageValue
    )
    {
        ContextMenuManager.SetNotifyIconText(cpuInfo.GetDescription());
    }

    private int CalculateInterval()
    {
        return (int)(50.0f / fpsMaxLimit.GetRate());
    }

    private void FetchTick(object? state, EventArgs e)
    {
        CpuRepository.Update();
        FetchCounter += 1;
        if (FetchCounter < FetchCounterSize) return;
        FetchCounter = 0;

        var cpuInfo = CpuRepository.Get();
        var memoryInfo = MemoryRepository.Get();
        var storageInfo = StorageRepository.Get();
        FetchSystemInfo(cpuInfo, memoryInfo, storageInfo);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SystemEvents.UserPreferenceChanged -= UserPreferenceChanged;

            AnimateTimer?.Stop();
            AnimateTimer?.Dispose();
            FetchTimer?.Stop();
            FetchTimer?.Dispose();

            CpuRepository?.Close();

            ContextMenuManager?.HideNotifyIcon();
            ContextMenuManager?.Dispose();
        }

        base.Dispose(disposing);
    }
}
