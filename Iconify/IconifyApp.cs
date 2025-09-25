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

namespace Iconify;

internal sealed class IconifyApp : ApplicationContext
{
    private readonly ContextMenuManager _contextMenuManager;

    private Runner _runner;

    private Theme _theme;

    private Speed _speed;

    private readonly Timer _animationTimer = new();

    private bool _isDisposed = false;

    public IconifyApp()
    {
        UserSettings.Default.Reload();

        bool isFirstLaunch = UserSettings.Default.IsFirstLaunch;

        if (isFirstLaunch)
            StartupAppManager.SetStartup(true);

        _ = Runner.TryParse(UserSettings.Default.Runner, out _runner);
        _ = Theme.TryParse(UserSettings.Default.Theme, out _theme);
        _ = Speed.TryParse(UserSettings.Default.Speed, out _speed);

        SystemEvents.UserPreferenceChanged += UserPreferenceChanged;

        _contextMenuManager = new ContextMenuManager(
            GetRunner, SetRunner,
            GetSystemTheme, GetTheme, SetTheme,
            GetSpeed, SetSpeed,
            StartupAppManager.GetStartup, StartupAppManager.SetStartup,
            OpenRepository,
            Application.Exit
        );

        _animationTimer.Tick += AnimationTick;
        _animationTimer.Interval = _speed.GetDelay();
        _animationTimer.Start();

        if (isFirstLaunch)
        {
            UserSettings.Default.IsFirstLaunch = false;
            UserSettings.Default.Save();

            _contextMenuManager.ShowBalloonTip();
        }
    }

    private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is not UserPreferenceCategory.General)
            return; 

        var systemTheme = GetSystemTheme();
        _contextMenuManager.SetIcons(systemTheme, _theme, _runner);
    }

    private static Theme GetSystemTheme()
    {
        using var rKey = Registry.CurrentUser.OpenSubKey(AppStrings.RegistryNamePersonalization);
        object? value = rKey?.GetValue(AppStrings.RegistryKeyIsLightTheme);

        return value is 0 ? Theme.Dark : Theme.Light;
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

    private Theme GetTheme()
    {
        return _theme;
    }

    private void SetTheme(Theme value)
    {
        UserSettings.Default.Theme = value.GetString();
        UserSettings.Default.Save();

        _theme = value;
    }

    private Speed GetSpeed()
    {
        return _speed;
    }

    private void SetSpeed(Speed value)
    {
        UserSettings.Default.Speed = value.GetString();
        UserSettings.Default.Save();

        _speed = value;

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
