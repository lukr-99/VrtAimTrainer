using Raylib_cs;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed partial class TrainerApplication
{
    private void DrawTopTabs(Rectangle bounds)
    {
        DrawPanelShape(bounds, Card);
        Raylib.DrawRectangleLinesEx(bounds, 1f, CardBorder);

        var buttons =
            new[]
            {
                (ShellPage.Play, "Play"),
                (ShellPage.Modes, "Modes"),
                (ShellPage.Presets, "Presets"),
                (ShellPage.RunSetup, "Run"),
                (ShellPage.MouseSettings, "Mouse"),
                (ShellPage.Sheet, "Sheet"),
                (ShellPage.Display, "Display"),
                (ShellPage.Analytics, "Analytics"),
                (ShellPage.About, "About"),
            };

        var gap = 8f;
        var navLeft = bounds.X + 12f;
        var navRight = bounds.Right() - 12f;
        var availableWidth = navRight - navLeft;
        var buttonWidth = Math.Clamp(
            (availableWidth - (gap * (buttons.Length - 1))) / buttons.Length,
            70f,
            104f);
        var totalButtonsWidth = (buttonWidth * buttons.Length) + (gap * (buttons.Length - 1));
        var buttonX = navLeft + Math.Max(0f, (availableWidth - totalButtonsWidth) / 2f);
        foreach (var (page, label) in buttons)
        {
            var isActive = _activePage == page;
            if (_ui.Button(
                    $"nav-{page}",
                    new Rectangle(buttonX, bounds.Y + 11f, buttonWidth, 38f),
                    label,
                    isActive ? Accent : CardAlt,
                    Accent,
                    isActive ? Background : Text))
            {
                _activePage = page;
            }

            buttonX += buttonWidth + gap;
        }
    }

    private void DrawNavigationPanel(Rectangle bounds, Rectangle contentBounds)
    {
        DrawPanelShape(bounds, Card);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);
        DrawActivePage(contentBounds);
    }

    private void DrawStatusRail(Rectangle bounds)
    {
        DrawPanelShape(bounds, Card);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);

        DrawLabel("Control Hub", bounds.X + 16f, bounds.Y + 16f, 28, Text);
        DrawWrappedLabel(
            "AimLabs-style shell for run staging, display presets, and local analytics without cluttering the playfield.",
            bounds.X + 16f,
            bounds.Y + 48f,
            bounds.Width - 32f,
            16,
            MutedText,
            3);

        DrawRunSummaryCard(new Rectangle(bounds.X + 16f, bounds.Y + 104f, bounds.Width - 32f, 154f));
    }

    private void DrawActivePage(Rectangle bounds)
    {
        switch (_activePage)
        {
            case ShellPage.Play:
                DrawPlayPage(bounds);
                break;
            case ShellPage.Modes:
                DrawModesPage(bounds);
                break;
            case ShellPage.Presets:
                DrawPresetsPage(bounds);
                break;
            case ShellPage.RunSetup:
                DrawRunSetupPage(bounds);
                break;
            case ShellPage.MouseSettings:
                DrawMouseSettingsPage(bounds);
                break;
            case ShellPage.Sheet:
                DrawSheetPage(bounds);
                break;
            case ShellPage.Display:
                DrawDisplayPage(bounds);
                break;
            case ShellPage.Analytics:
                DrawAnalyticsPage(bounds);
                break;
            case ShellPage.About:
                DrawAboutPage(bounds);
                break;
        }
    }

    private void DrawRunSummaryCard(Rectangle bounds)
    {
        var state = _frameState;
        DrawPanelShape(bounds, CardAlt);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);

        DrawLabel("Live Run", bounds.X + 14f, bounds.Y + 12f, 22, Text);
        DrawLabel(FitTextToWidth(_activeGamemode.DisplayName, 18, bounds.Width - 28f), bounds.X + 14f, bounds.Y + 40f, 18, Text);
        DrawLabel($"Stage {FitTextToWidth(GetDraftPresetLabel(), 16, bounds.Width - 28f)}", bounds.X + 14f, bounds.Y + 66f, 16, MutedText);
        DrawLabel(GetRunSummaryLine(state), bounds.X + 14f, bounds.Y + 90f, 18, Text);
        DrawLabel(
            HasDraftChanges()
                ? "Draft changes are waiting on Play."
                : $"Sheet {state.Settings.EffectiveSheetWidth:0} x {state.Settings.EffectiveSheetHeight:0}",
            bounds.X + 14f,
            bounds.Y + 116f,
            16,
            HasDraftChanges() ? Warning : MutedText);

        if (!state.Settings.UsesFixedDuration)
        {
            DrawHealthBar(new Rectangle(bounds.X + 14f, bounds.Bottom() - 18f, bounds.Width - 28f, 10f), state);
        }
    }

    private static string GetRunSummaryLine(AimSessionStateSnapshot state)
    {
        if (state.Settings.UsesFixedDuration)
        {
            return $"Fixed round  {state.Settings.FixedDurationSeconds ?? 0d:0}s   Hits {state.HitCount}";
        }

        return $"HP {state.Health:0.00}   Last {state.LastReactionMs ?? 0d:0.00} ms";
    }
}
