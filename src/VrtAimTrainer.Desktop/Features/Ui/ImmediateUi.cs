using System.Numerics;
using Raylib_cs;

namespace VrtAimTrainer.Desktop.Features.Ui;

public sealed class ImmediateUi
{
    private string? _activeControlId;
    private string? _committedControlId;
    private bool _inputEnabled = true;
    private Vector2 _mousePosition;
    private string? _openDropdownId;
    private bool _primaryButtonDown;
    private bool _primaryButtonPressed;
    private bool _primaryButtonReleased;

    public void BeginFrame(Vector2 mousePosition, bool primaryButtonPressed, bool primaryButtonDown, bool primaryButtonReleased)
    {
        _committedControlId = null;
        _inputEnabled = true;
        _mousePosition = mousePosition;
        _primaryButtonPressed = primaryButtonPressed;
        _primaryButtonDown = primaryButtonDown;
        _primaryButtonReleased = primaryButtonReleased;
    }

    public void SetInputEnabled(bool inputEnabled)
    {
        _inputEnabled = inputEnabled;
    }

    public bool WasCommitted(string controlId)
    {
        return string.Equals(_committedControlId, controlId, StringComparison.Ordinal);
    }

    public bool Button(string controlId, Rectangle bounds, string label, Color fillColor, Color hoverColor, Color textColor)
    {
        var hovered = _inputEnabled && Raylib.CheckCollisionPointRec(_mousePosition, bounds);
        if (hovered && _primaryButtonPressed)
        {
            _activeControlId = controlId;
        }

        var clicked = hovered && _primaryButtonReleased && _activeControlId == controlId;
        if (_primaryButtonReleased && _activeControlId == controlId)
        {
            _activeControlId = null;
        }

        if (clicked)
        {
            _committedControlId = controlId;
        }

        var background = hovered ? hoverColor : fillColor;
        Raylib.DrawRectangleRec(bounds, background);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, textColor);

        var fontSize = 20;
        var textWidth = Raylib.MeasureText(label, fontSize);
        var textX = bounds.X + ((bounds.Width - textWidth) / 2f);
        var textY = bounds.Y + ((bounds.Height - fontSize) / 2f);
        Raylib.DrawText(label, (int)textX, (int)textY, fontSize, textColor);

        return clicked;
    }

    public bool Toggle(
        string controlId,
        Rectangle bounds,
        string label,
        bool currentValue,
        Color fillColor,
        Color accentColor,
        Color textColor)
    {
        var toggled = Button(
            controlId,
            bounds,
            currentValue ? $"{label}: ON" : $"{label}: OFF",
            currentValue ? accentColor : fillColor,
            accentColor,
            textColor);

        return toggled ? !currentValue : currentValue;
    }

    public float Slider(
        string controlId,
        Rectangle bounds,
        string label,
        float currentValue,
        float minimum,
        float maximum,
        string format,
        Color fillColor,
        Color accentColor,
        Color textColor)
    {
        var clampedValue = Math.Clamp(currentValue, minimum, maximum);
        var trackBounds = new Rectangle(bounds.X, bounds.Y + 28f, bounds.Width, 12f);
        var hovered = _inputEnabled && Raylib.CheckCollisionPointRec(_mousePosition, trackBounds);

        if ((hovered || _activeControlId == controlId) && _primaryButtonPressed)
        {
            _activeControlId = controlId;
        }

        if (_activeControlId == controlId)
        {
            if (_primaryButtonDown)
            {
                var t = trackBounds.Width <= 0f
                    ? 0f
                    : Math.Clamp((_mousePosition.X - trackBounds.X) / trackBounds.Width, 0f, 1f);
                clampedValue = minimum + ((maximum - minimum) * t);
            }

            if (_primaryButtonReleased)
            {
                _activeControlId = null;
                _committedControlId = controlId;
            }
        }

        Raylib.DrawText(label, (int)bounds.X, (int)bounds.Y, 18, textColor);
        Raylib.DrawText(
            clampedValue.ToString(format),
            (int)(bounds.X + bounds.Width - 84f),
            (int)bounds.Y,
            18,
            textColor);

        Raylib.DrawRectangleRec(trackBounds, fillColor);
        Raylib.DrawRectangleLinesEx(trackBounds, 1f, textColor);

        var normalized = (clampedValue - minimum) / (maximum - minimum);
        var handleX = trackBounds.X + (trackBounds.Width * normalized);
        var handleBounds = new Rectangle(handleX - 8f, trackBounds.Y - 5f, 16f, 22f);
        Raylib.DrawRectangleRec(handleBounds, accentColor);
        Raylib.DrawRectangleLinesEx(handleBounds, 1f, textColor);

        return clampedValue;
    }

    public int Dropdown(
        string controlId,
        Rectangle bounds,
        string label,
        IReadOnlyList<string> options,
        int selectedIndex,
        Color fillColor,
        Color accentColor,
        Color textColor)
    {
        if (options.Count == 0)
        {
            return -1;
        }

        var clampedIndex = Math.Clamp(selectedIndex, 0, options.Count - 1);
        var buttonBounds = new Rectangle(bounds.X, bounds.Y + 22f, bounds.Width, 36f);
        var hovered = _inputEnabled && Raylib.CheckCollisionPointRec(_mousePosition, buttonBounds);
        if (hovered && _primaryButtonPressed)
        {
            _activeControlId = controlId;
        }

        var toggled = hovered && _primaryButtonReleased && _activeControlId == controlId;
        if (_primaryButtonReleased && _activeControlId == controlId)
        {
            _activeControlId = null;
        }

        if (toggled)
        {
            _openDropdownId = string.Equals(_openDropdownId, controlId, StringComparison.Ordinal)
                ? null
                : controlId;
        }

        DrawDropdownButton(buttonBounds, $"{label}: {options[clampedIndex]}", hovered, fillColor, accentColor, textColor);

        if (!string.Equals(_openDropdownId, controlId, StringComparison.Ordinal))
        {
            Raylib.DrawText(label, (int)bounds.X, (int)bounds.Y, 18, textColor);
            return clampedIndex;
        }

        if (!_inputEnabled)
        {
            DrawDropdownButton(buttonBounds, $"{label}: {options[clampedIndex]}", false, fillColor, accentColor, textColor);
            Raylib.DrawText(label, (int)bounds.X, (int)bounds.Y, 18, textColor);
            return clampedIndex;
        }

        var optionHeight = 34f;
        var optionsBounds = new Rectangle(buttonBounds.X, buttonBounds.Y + buttonBounds.Height + 6f, buttonBounds.Width, optionHeight * options.Count);
        Raylib.DrawRectangleRec(optionsBounds, fillColor);
        Raylib.DrawRectangleLinesEx(optionsBounds, 1f, accentColor);

        var clickedInsideOptions = false;
        for (var index = 0; index < options.Count; index++)
        {
            var optionId = $"{controlId}-option-{index}";
            var optionBounds = new Rectangle(optionsBounds.X, optionsBounds.Y + (optionHeight * index), optionsBounds.Width, optionHeight);
            var optionHovered = _inputEnabled && Raylib.CheckCollisionPointRec(_mousePosition, optionBounds);
            if (optionHovered && _primaryButtonPressed)
            {
                _activeControlId = optionId;
            }

            var optionClicked = optionHovered && _primaryButtonReleased && _activeControlId == optionId;
            if (_primaryButtonReleased && _activeControlId == optionId)
            {
                _activeControlId = null;
            }

            var optionFill = index == clampedIndex
                ? accentColor
                : optionHovered
                    ? accentColor
                    : fillColor;
            var optionText = index == clampedIndex || optionHovered
                ? new Color(4, 11, 15, 255)
                : textColor;
            Raylib.DrawRectangleRec(optionBounds, optionFill);
            Raylib.DrawRectangleLinesEx(optionBounds, 1f, accentColor);
            Raylib.DrawText(options[index], (int)(optionBounds.X + 10f), (int)(optionBounds.Y + 8f), 18, optionText);

            if (!optionClicked)
            {
                continue;
            }

            _openDropdownId = null;
            _committedControlId = controlId;
            clickedInsideOptions = true;
            clampedIndex = index;
        }

        if (_primaryButtonReleased && !hovered && !clickedInsideOptions && !Raylib.CheckCollisionPointRec(_mousePosition, optionsBounds))
        {
            _openDropdownId = null;
        }

        Raylib.DrawText(label, (int)bounds.X, (int)bounds.Y, 18, textColor);
        return clampedIndex;
    }

    private static void DrawDropdownButton(Rectangle bounds, string text, bool hovered, Color fillColor, Color accentColor, Color textColor)
    {
        var background = hovered ? accentColor : fillColor;
        var foreground = hovered ? new Color(4, 11, 15, 255) : textColor;
        Raylib.DrawRectangleRec(bounds, background);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, accentColor);
        Raylib.DrawText(text, (int)(bounds.X + 10f), (int)(bounds.Y + 8f), 18, foreground);
        Raylib.DrawText("v", (int)(bounds.X + bounds.Width - 22f), (int)(bounds.Y + 8f), 18, foreground);
    }
}
