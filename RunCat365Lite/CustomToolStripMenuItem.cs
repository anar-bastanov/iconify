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

using System.Text.RegularExpressions;

namespace RunCat365Lite;

internal partial class CustomToolStripMenuItem : ToolStripMenuItem
{
    private const TextFormatFlags MultiLineTextFlags =
        TextFormatFlags.LeftAndRightPadding |
        TextFormatFlags.VerticalCenter |
        TextFormatFlags.WordBreak |
        TextFormatFlags.TextBoxControl;

    private const TextFormatFlags SingleLineTextFlags =
        TextFormatFlags.LeftAndRightPadding |
        TextFormatFlags.VerticalCenter |
        TextFormatFlags.EndEllipsis;

    public CustomToolStripMenuItem() : base()
    {
    }

    public CustomToolStripMenuItem(string? text) : base(text)
    {
    }

    public CustomToolStripMenuItem(string? text, Image? image, bool isChecked, EventHandler? onClick) : base(text, image, onClick)
    {
        Checked = isChecked;
    }

    public override Size GetPreferredSize(Size constrainingSize)
    {
        Size baseSize = base.GetPreferredSize(constrainingSize);

        if (Text is null or "")
            return baseSize with { Height = 22 };

        int textRenderWidth = Math.Max(constrainingSize.Width - 20, 1);

        Size measuredSize = TextRenderer.MeasureText(
            Text,
            Font,
            new Size(textRenderWidth, int.MaxValue),
            Flags()
        );

        int calculatedHeight = measuredSize.Height + 4;
        int height = IsSingleLine() ? calculatedHeight : Math.Max(baseSize.Height, calculatedHeight);

        return baseSize with { Height = height };
    }

    public bool IsSingleLine()
    {
        return Text is null || !Text.Contains('\n');
    }

    public TextFormatFlags Flags()
    {
        return IsSingleLine() ? SingleLineTextFlags : MultiLineTextFlags;
    }

    public void SetupSubMenusFromEnum<T>(
        Action<CustomToolStripMenuItem, object?> onClick,
        T selection,
        Func<T, Bitmap?> getIconBitmap
    ) where T : struct, IClosedEnum<T>
    {
        var values = T.GetValues();
        var items = new CustomToolStripMenuItem[values.Length];

        for (int i = 0; i < values.Length; ++i)
        {
            var value = values[i];

            items[i] = new CustomToolStripMenuItem(
                ToTitleCase(value.GetString()),
                getIconBitmap(value),
                selection == value,
                (sender, e) => onClick(this, sender)
            );
        }

        DropDownItems.AddRange(items);
    }

    static string ToTitleCase(string name) => TitleCaseRegex().Replace(name, " $1");

    [GeneratedRegex(@"(?<=[a-z])([A-Z])")]
    private static partial Regex TitleCaseRegex();
}
