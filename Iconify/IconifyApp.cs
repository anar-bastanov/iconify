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
using Iconify.Properties;
using System.Diagnostics;

using Timer = System.Windows.Forms.Timer;
using UPCategory = Microsoft.Win32.UserPreferenceCategory;

namespace Iconify;

internal sealed class IconifyApp : ApplicationContext
{
    private readonly SynchronizationContext _uiContext;

    private readonly ContextMenuManager _contextMenuManager;

    private Runner _runner;

    private RunnerColor _runnerColor;

    private RunnerSpeed _runnerSpeed;

    private readonly Timer _animationTimer = new();

    private bool _isDisposed = false;

    private bool _isRestartRequested = false;

    public IconifyApp()
    {
        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();

        UserSettings.Default.Reload();

        bool isFirstLaunch = UserSettings.Default.IsFirstLaunch;

        if (isFirstLaunch)
            StartupAppManager.SetStartup(true);

        _ = Runner.TryParse(UserSettings.Default.Runner, out _runner);
        _ = RunnerColor.TryParse(UserSettings.Default.RunnerColor, out _runnerColor);
        _ = RunnerSpeed.TryParse(UserSettings.Default.RunnerSpeed, out _runnerSpeed);

        SystemEvents.UserPreferenceChanged += UserPreferenceChanged;

        _contextMenuManager = new ContextMenuManager(
            GetRunner, SetRunner,
            GetRunnerColor, SetRunnerColor,
            GetRunnerSpeed, SetRunnerSpeed,
            GetSystemTheme,
            StartupAppManager.GetStartup, StartupAppManager.SetStartup,
            OpenRepository,
            Application.Exit);

        _animationTimer.Tick += AnimationTick;
        _animationTimer.Interval = _runnerSpeed.GetDelay();
        _animationTimer.Start();

        if (isFirstLaunch)
        {
            UserSettings.Default.IsFirstLaunch = false;
            UserSettings.Default.Save();

            _contextMenuManager.ShowBalloonTip();
        }
    }

    private void UserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is not (UPCategory.General or UPCategory.Color or UPCategory.VisualStyle))
            return;

        if (Interlocked.Exchange(ref _isRestartRequested, true))
            return;

        SystemEvents.UserPreferenceChanged -= UserPreferenceChanged;

        _uiContext.Post(_ =>
        {
            if (_runnerColor.Value is RunnerColor.White or RunnerColor.Gray or RunnerColor.Black)
                SetRunnerColor(RunnerColor.System);

            _animationTimer.Stop();
            _animationTimer.Tick -= AnimationTick;

            _contextMenuManager.Dispose();

            Application.Restart();
            Application.ExitThread(); // Just in case
        }, null);
    }

    private static RunnerColor GetSystemTheme()
    {
        using var rKey = Registry.CurrentUser.OpenSubKey(AppStrings.RegistryNamePersonalization);
        object? value = rKey?.GetValue(AppStrings.RegistryKeyIsLightTheme);

        // Black color for light theme, White color for dark theme
        return value is 0 ? RunnerColor.White : RunnerColor.Black;
    }

    private Runner GetRunner()
    {
        return _runner;
    }

    private void SetRunner(Runner value)
    {
        UserSettings.Default.Runner = value.GetString();
        UserSettings.Default.Save();

        _runner = value;
    }

    private RunnerColor GetRunnerColor()
    {
        return _runnerColor;
    }

    private void SetRunnerColor(RunnerColor value)
    {
        UserSettings.Default.RunnerColor = value.GetString();
        UserSettings.Default.Save();

        _runnerColor = value;
    }

    private RunnerSpeed GetRunnerSpeed()
    {
        return _runnerSpeed;
    }

    private void SetRunnerSpeed(RunnerSpeed value)
    {
        UserSettings.Default.RunnerSpeed = value.GetString();
        UserSettings.Default.Save();

        _runnerSpeed = value;

        _animationTimer.Stop();
        _animationTimer.Interval = value.GetDelay();
        _animationTimer.Start();
    }

    private void AnimationTick(object? sender, EventArgs e)
    {
        _contextMenuManager.AdvanceFrame();
    }

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed && disposing)
        {
            SystemEvents.UserPreferenceChanged -= UserPreferenceChanged;

            _animationTimer.Tick -= AnimationTick;
            _animationTimer.Stop();
            _animationTimer.Dispose();

            _contextMenuManager.Dispose();
        }

        _isDisposed = true;
        base.Dispose(disposing);
    }

    private static void OpenRepository()
    {
        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = AppStrings.RepositoryLink,
                UseShellExecute = true
            });
        }
#if DEBUG
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open repository: {ex}");
        }
#else
        catch
        {
        }
#endif
    }
}
