﻿using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.UI;

/// <summary>
/// Represents a theme for display controls in the user interface.
/// </summary>
public class DisplayTheme
{
    /// <summary>
    /// Gets or sets the background color for display controls using this theme.
    /// </summary>
    public Color? BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground color for display controls using this theme.
    /// </summary>
    public Color? ForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the color for display controls when pressed (e.g., buttons).
    /// </summary>
    public Color? PressedColor { get; set; }

    /// <summary>
    /// Gets or sets the color for display controls when highlighted.
    /// </summary>
    public Color? HighlightColor { get; set; }

    /// <summary>
    /// Gets or sets the shadow color for display controls.
    /// </summary>
    public Color? ShadowColor { get; set; }

    /// <summary>
    /// Gets or sets the text color for display controls that display text.
    /// </summary>
    public Color? TextColor { get; set; }

    /// <summary>
    /// Gets or sets the font for display controls that display text.
    /// </summary>
    public IFont? Font { get; set; }
}
