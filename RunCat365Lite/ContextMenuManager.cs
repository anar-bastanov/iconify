﻿// Copyright 2025 Anar Bastanov
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

using RunCat365Lite.Properties;
using System.ComponentModel;

namespace RunCat365Lite;

internal sealed class ContextMenuManager : IDisposable
{
    private delegate bool CustomTryParseDelegate<T>(string? value, out T result);

    private List<Icon> _icons = [];

    private readonly NotifyIcon _notifyIcon = new();

    private readonly Lock _iconLock = new();

    private int _currentIconIndex = 0;

    public ContextMenuManager(
        Func<Runner> getRunner, Action<Runner> setRunner,
        Func<Theme> getSystemTheme, Func<Theme> getTheme, Action<Theme> setTheme,
        Func<Speed> getSpeed, Action<Speed> setSpeed,
        Func<bool> getStartup, Func<bool, bool> setStartup,
        Action openRepository,
        Action onExit
    )
    {
        var runnersMenu = new CustomToolStripMenuItem("Runners");
        var upcomingItemsMenu = new CustomToolStripMenuItem($"More soon");
        var settingsMenu = new CustomToolStripMenuItem("Settings");
        var themeMenu = new CustomToolStripMenuItem("Theme");
        var speedMenu = new CustomToolStripMenuItem("Animation speed");
        var startupMenu = new CustomToolStripMenuItem("Launch at startup");
        var informationMenu = new CustomToolStripMenuItem("Information");
        var appVersionMenu = new CustomToolStripMenuItem($"{Application.ProductName} v{Application.ProductVersion}");
        var repositoryMenu = new CustomToolStripMenuItem("Open repository");
        var exitMenu = new CustomToolStripMenuItem("Exit");
        var contextMenuStrip = new ContextMenuStrip(new Container());

        settingsMenu.DropDownItems.AddRange(
            themeMenu,
            speedMenu,
            startupMenu
        );

        informationMenu.DropDownItems.AddRange(
            appVersionMenu,
            repositoryMenu
        );

        contextMenuStrip.Items.AddRange(
            new ToolStripSeparator(),
            runnersMenu,
            new ToolStripSeparator(),
            settingsMenu,
            informationMenu,
            new ToolStripSeparator(),
            exitMenu
        );

        runnersMenu.SetupSubMenusFromEnum(
            (parent, sender) =>
            {
                HandleMenuItemSelection(parent, sender, setRunner);

                SetIcons(getSystemTheme(), getTheme(), getRunner());
            },
            getRunner(),
            r => GetRunnerThumbnailBitmap(getSystemTheme(), r)
        );

        themeMenu.SetupSubMenusFromEnum(
            (parent, sender) =>
            {
                HandleMenuItemSelection(parent, sender, setTheme);

                SetIcons(getSystemTheme(), getTheme(), getRunner());
            },
            getTheme(),
            _ => null
        );

        speedMenu.SetupSubMenusFromEnum(
            (parent, sender) =>
            {
                HandleMenuItemSelection(parent, sender, setSpeed);
            },
            getSpeed(),
            _ => null
        );

        upcomingItemsMenu.Enabled = false;
        runnersMenu.DropDownItems.Add(upcomingItemsMenu);

        startupMenu.Checked = getStartup();
        startupMenu.Click += (sender, _) => HandleStartupMenuClick(sender, setStartup);

        appVersionMenu.Enabled = false;

        repositoryMenu.Click += (_, _) => openRepository();

        exitMenu.Click += (_, _) => onExit();

        contextMenuStrip.Renderer = new ContextMenuRenderer();

        SetIcons(getSystemTheme(), getTheme(), getRunner());

        _notifyIcon.Text = AppStrings.ApplicationName;
        _notifyIcon.Icon = _icons![0];
        _notifyIcon.Visible = true;
        _notifyIcon.ContextMenuStrip = contextMenuStrip;
    }

    private static void HandleMenuItemSelection<T>(
        ToolStripMenuItem parentMenu,
        object? sender,
        Action<T> assignValue
    ) where T : struct, IClosedEnum<T>
    {
        if (sender is not ToolStripMenuItem item)
            return;

        foreach (ToolStripMenuItem childItem in parentMenu.DropDownItems)
            childItem.Checked = false;

        item.Checked = true;

        if (T.TryParse(item.Text, out T parsedValue))
            assignValue(parsedValue);
    }

    private static Bitmap? GetRunnerThumbnailBitmap(Theme systemTheme, Runner runner)
    {
        string iconName = $"{systemTheme.GetString()}_{runner.GetString()}_0".ToLower();
        var icon = Resources.ResourceManager.GetObject(iconName) as Icon;

        return icon?.ToBitmap();
    }

    public void SetIcons(Theme systemTheme, Theme theme, Runner runner)
    {
        var rm = Resources.ResourceManager;

        string prefix = (theme == Theme.System ? systemTheme : theme).GetString();
        string runnerName = runner.GetString();
        int capacity = runner.GetFrameNumber();
        var list = new List<Icon>(capacity);

        for (int i = 0; i < capacity; i++)
        {
            string iconName = $"{prefix}_{runnerName}_{i}".ToLower();

            if (rm.GetObject(iconName) is Icon icon)
                list.Add(icon);
        }

        lock (_iconLock)
        {
            foreach (var icon in _icons)
                icon.Dispose();

            _currentIconIndex = 0;
            _icons = list;
        }
    }

    private static void HandleStartupMenuClick(object? sender, Func<bool, bool> setStartup)
    {
        if (sender is not ToolStripMenuItem item)
            return;

        try
        {
            bool success = setStartup(!item.Checked);
            item.Checked ^= success;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public void ShowBalloonTip()
    {
        const string message =
            "App has launched. If the icon is not on the taskbar, " +
            "it has been omitted. Please move it manually and pin it.";

        _notifyIcon.ShowBalloonTip(30000, AppStrings.ApplicationName, message, ToolTipIcon.Info);
    }

    public void AdvanceFrame()
    {
        lock (_iconLock)
        {
            if (_icons.Count is 0)
                return;

            _currentIconIndex = (_currentIconIndex + 1) % _icons.Count;
            _notifyIcon.Icon = _icons[_currentIconIndex];
        }
    }

    public void Dispose()
    {
        lock (_iconLock)
        {
            foreach (var icon in _icons)
                icon.Dispose();

            _icons.Clear();
        }

        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.ContextMenuStrip?.Dispose();
            _notifyIcon.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
