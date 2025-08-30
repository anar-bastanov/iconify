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

using RunCat365Lite.Properties;
using System.ComponentModel;

namespace RunCat365Lite;

internal class ContextMenuManager : IDisposable
{
    private readonly CustomToolStripMenuItem SystemInfoMenu = new();

    private readonly NotifyIcon NotifyIcon = new();

    private readonly List<Icon> Icons = [];

    private readonly Lock IconLock = new();

    private int CurrentIconIndex = 0;

    internal ContextMenuManager(
        Func<Runner> getRunner,
        Action<Runner> setRunner,
        Func<Theme> getSystemTheme,
        Func<Theme> getManualTheme,
        Action<Theme> setManualTheme,
        Func<FPSMaxLimit> getFPSMaxLimit,
        Action<FPSMaxLimit> setFPSMaxLimit,
        Func<bool> getLaunchAtStartup,
        Func<bool, bool> toggleLaunchAtStartup,
        Action openRepository,
        Action onExit
    )
    {
        SystemInfoMenu.Text = "-\n-\n-\n-\n-";
        SystemInfoMenu.Enabled = false;

        var runnersMenu = new CustomToolStripMenuItem("Runners");
        runnersMenu.SetupSubMenusFromEnum<Runner>(
            r => r.GetString(),
            (parent, sender, e) =>
            {
                HandleMenuItemSelection(
                    parent,
                    sender,
                    (string? s, out Runner r) => Enum.TryParse(s, out r),
                    r => setRunner(r)
                );
                SetIcons(getSystemTheme(), getManualTheme(), getRunner());
            },
            r => getRunner() == r,
            r => GetRunnerThumbnailBitmap(getSystemTheme(), r)
        );

        var themeMenu = new CustomToolStripMenuItem("Theme");
        themeMenu.SetupSubMenusFromEnum<Theme>(
            t => t.GetString(),
            (parent, sender, e) =>
            {
                HandleMenuItemSelection(
                    parent,
                    sender,
                    (string? s, out Theme t) => Enum.TryParse(s, out t),
                    t => setManualTheme(t)
                );
                SetIcons(getSystemTheme(), getManualTheme(), getRunner());
            },
            t => getManualTheme() == t,
            _ => null
        );

        var fpsMaxLimitMenu = new CustomToolStripMenuItem("FPS Max Limit");
        fpsMaxLimitMenu.SetupSubMenusFromEnum<FPSMaxLimit>(
            f => f.GetString(),
            (parent, sender, e) =>
            {
                HandleMenuItemSelection(
                    parent,
                    sender,
                    (string? s, out FPSMaxLimit f) => FPSMaxLimitExtension.TryParse(s, out f),
                    f => setFPSMaxLimit(f)
                );
            },
            f => getFPSMaxLimit() == f,
            _ => null
        );

        var launchAtStartupMenu = new CustomToolStripMenuItem("Launch at startup")
        {
            Checked = getLaunchAtStartup()
        };
        launchAtStartupMenu.Click += (sender, e) => HandleStartupMenuClick(sender, toggleLaunchAtStartup);

        var settingsMenu = new CustomToolStripMenuItem("Settings");
        settingsMenu.DropDownItems.AddRange(
            themeMenu,
            fpsMaxLimitMenu,
            launchAtStartupMenu
        );

        var appVersionMenu = new CustomToolStripMenuItem(
            $"{Application.ProductName} v{Application.ProductVersion}"
        )
        {
            Enabled = false
        };

        var repositoryMenu = new CustomToolStripMenuItem("Open Repository");
        repositoryMenu.Click += (sender, e) => openRepository();

        var informationMenu = new CustomToolStripMenuItem("Information");
        informationMenu.DropDownItems.AddRange(
            appVersionMenu,
            repositoryMenu
        );

        var exitMenu = new CustomToolStripMenuItem("Exit");
        exitMenu.Click += (sender, e) => onExit();

        var contextMenuStrip = new ContextMenuStrip(new Container());
        contextMenuStrip.Items.AddRange(
            SystemInfoMenu,
            new ToolStripSeparator(),
            runnersMenu,
            new ToolStripSeparator(),
            settingsMenu,
            informationMenu,
            new ToolStripSeparator(),
            exitMenu
        );
        contextMenuStrip.Renderer = new ContextMenuRenderer();

        SetIcons(getSystemTheme(), getManualTheme(), getRunner());

        NotifyIcon.Text = "-";
        NotifyIcon.Icon = Icons[0];
        NotifyIcon.Visible = true;
        NotifyIcon.ContextMenuStrip = contextMenuStrip;
    }

    private static void HandleMenuItemSelection<T>(
        ToolStripMenuItem parentMenu,
        object? sender,
        CustomTryParseDelegate<T> tryParseMethod,
        Action<T> assignValueAction
    )
    {
        if (sender is null) return;
        var item = (ToolStripMenuItem)sender;
        foreach (ToolStripMenuItem childItem in parentMenu.DropDownItems)
        {
            childItem.Checked = false;
        }
        item.Checked = true;
        if (tryParseMethod(item.Text, out T parsedValue))
        {
            assignValueAction(parsedValue);
        }
    }

    private static Bitmap? GetRunnerThumbnailBitmap(Theme systemTheme, Runner runner)
    {
        var iconName = $"{systemTheme.GetString()}_{runner.GetString()}_0".ToLower();
        var obj = Resources.ResourceManager.GetObject(iconName);
        return obj is Icon icon ? icon.ToBitmap() : null;
    }

    internal void SetIcons(Theme systemTheme, Theme manualTheme, Runner runner)
    {
        var prefix = (manualTheme == Theme.System ? systemTheme : manualTheme).GetString();
        var runnerName = runner.GetString();
        var rm = Resources.ResourceManager;
        var capacity = runner.GetFrameNumber();
        var list = new List<Icon>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            var iconName = $"{prefix}_{runnerName}_{i}".ToLower();
            var icon = rm.GetObject(iconName);
            if (icon is null) continue;
            list.Add((Icon)icon);
        }

        lock (IconLock)
        {
            Icons.ForEach(icon => icon.Dispose());
            Icons.Clear();
            Icons.AddRange(list);
            CurrentIconIndex = 0;
        }
    }

    private static void HandleStartupMenuClick(object? sender, Func<bool, bool> toggleLaunchAtStartup)
    {
        if (sender is null) return;
        var item = (ToolStripMenuItem)sender;
        try
        {
            if (toggleLaunchAtStartup(item.Checked))
            {
                item.Checked = !item.Checked;
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    internal void ShowBalloonTip()
    {
        var message = "App has launched. " +
            "If the icon is not on the taskbar, it has been omitted, " +
            "so please move it manually and pin it.";
        NotifyIcon.ShowBalloonTip(5000, "RunCat 365 Lite", message, ToolTipIcon.Info);
    }

    internal void AdvanceFrame()
    {
        lock (IconLock)
        {
            if (Icons.Count == 0) return;
            if (Icons.Count <= CurrentIconIndex) CurrentIconIndex = 0;
            NotifyIcon.Icon = Icons[CurrentIconIndex];
            CurrentIconIndex = (CurrentIconIndex + 1) % Icons.Count;
        }
    }

    internal void SetSystemInfoMenuText(string text)
    {
        SystemInfoMenu.Text = text;
    }

    internal void SetNotifyIconText(string text)
    {
        NotifyIcon.Text = text;
    }

    internal void HideNotifyIcon()
    {
        NotifyIcon.Visible = false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (IconLock)
            {
                Icons.ForEach(icon => icon.Dispose());
                Icons.Clear();
            }

            if (NotifyIcon is not null)
            {
                NotifyIcon.ContextMenuStrip?.Dispose();
                NotifyIcon.Dispose();
            }
        }
    }

    private delegate bool CustomTryParseDelegate<T>(string? value, out T result);
}
