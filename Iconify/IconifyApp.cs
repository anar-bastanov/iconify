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
    private Runner _runner;

    private RunnerColor _runnerColor;

    private RunnerSpeed _runnerSpeed;

    private readonly UserSettings _userSettings;

    private readonly ContextMenuManager _contextMenuManager;

    private readonly SynchronizationContext _uiContext;

    private readonly Timer _animationTimer = new();

    private bool _isDisposed = false;

    private bool _isRestartRequested = false;

    public IconifyApp()
    {
        _userSettings = UserSettings.Default;
        _userSettings.Reload();

        _ = Runner.TryParse(_userSettings.Runner, out _runner);
        _ = RunnerColor.TryParse(_userSettings.RunnerColor, out _runnerColor);
        _ = RunnerSpeed.TryParse(_userSettings.RunnerSpeed, out _runnerSpeed);
        _ = RunnerColor.TryParse(_userSettings.SystemTheme, out var lastSystemTheme);

        bool isFirstLaunch = GetIsFirstLaunch();

        if (isFirstLaunch)
            StartupAppManager.SetStartup(true);

        var systemTheme = GetSystemTheme();

        if (systemTheme != lastSystemTheme)
            SetLastSystemTheme(systemTheme);

        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        SystemEvents.UserPreferenceChanged += UserPreferenceChanged;

        _contextMenuManager = new ContextMenuManager(
            GetRunner, SetRunner,
            GetRunnerColor, SetRunnerColor,
            GetRunnerSpeed, SetRunnerSpeed,
            systemTheme,
            StartupAppManager.GetStartup, StartupAppManager.SetStartup,
            OpenRepository,
            Application.Exit);

        _animationTimer.Tick += AnimationTick;
        _animationTimer.Interval = _runnerSpeed.GetDelay();
        _animationTimer.Start();

        if (isFirstLaunch)
            SetIsFirstLaunch(false);
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
            _animationTimer.Stop();
            _animationTimer.Tick -= AnimationTick;

            _contextMenuManager.Dispose();

            Application.Restart();
            Application.ExitThread(); // Just in case
        }, null);
    }

    private Runner GetRunner()
    {
        return _runner;
    }

    private void SetRunner(Runner value)
    {
        _userSettings.Runner = value.GetString();
        _userSettings.Save();

        _runner = value;
    }

    private RunnerColor GetRunnerColor()
    {
        return _runnerColor;
    }

    private void SetRunnerColor(RunnerColor value)
    {
        _userSettings.RunnerColor = value.GetString();
        _userSettings.Save();

        _runnerColor = value;
    }

    private RunnerSpeed GetRunnerSpeed()
    {
        return _runnerSpeed;
    }

    private void SetRunnerSpeed(RunnerSpeed value)
    {
        _userSettings.RunnerSpeed = value.GetString();
        _userSettings.Save();

        _runnerSpeed = value;

        _animationTimer.Stop();
        _animationTimer.Interval = value.GetDelay();
        _animationTimer.Start();
    }

    private bool GetIsFirstLaunch()
    {
        return _userSettings.IsFirstLaunch;
    }

    private void SetIsFirstLaunch(bool value)
    {
        _userSettings.IsFirstLaunch = value;
        _userSettings.Save();

        if (!value)
            _contextMenuManager.ShowBalloonTip();
    }

    private void SetLastSystemTheme(RunnerColor systemTheme)
    {
        _userSettings.SystemTheme = systemTheme.GetString();
        _userSettings.Save();

        if (_runnerColor.Value is RunnerColor.White or RunnerColor.Gray or RunnerColor.Black)
            SetRunnerColor(RunnerColor.System);
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

    private static RunnerColor GetSystemTheme()
    {
        using var rKey = Registry.CurrentUser.OpenSubKey(AppStrings.RegistryNamePersonalization);
        object? value = rKey?.GetValue(AppStrings.RegistryKeyIsLightTheme);

        // Black color for light theme, White color for dark theme
        return value is 0 ? RunnerColor.White : RunnerColor.Black;
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
