using System.Numerics;
using Raylib_cs;
using VrtAimTrainer.Core.Features.Analytics;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed partial class TrainerApplication
{
    private void DrawArena(ShellLayout layout, ArenaRenderTransform arenaTransform)
    {
        var state = _frameState;
        DrawPanelShape(layout.PlayAreaBounds, Card);
        Raylib.DrawRectangleLinesEx(layout.PlayAreaBounds, 1.5f, CardBorder);

        var outerBounds = arenaTransform.ToScreenRectangle(state.Layout.OuterBounds);
        var innerBounds = arenaTransform.ToScreenRectangle(state.Layout.InnerBounds);
        var spawnBounds = arenaTransform.ToScreenRectangle(state.Layout.SpawnBounds);

        DrawPanelShape(outerBounds, ArenaOuter);
        DrawPanelShape(innerBounds, ArenaInner);
        Raylib.DrawRectangleLinesEx(spawnBounds, 1.5f, SpawnBounds);

        foreach (var target in state.Targets)
        {
            DrawTarget(arenaTransform, target);
        }

        DrawCursor(arenaTransform, state.CursorPosition, state.Settings.CursorDiameter / 2f);
        DrawArenaOverlay(layout, state);
        if (state.IsStartCountdownActive)
        {
            DrawStartCountdown(layout.PlayAreaBounds, state.StartCountdownValue);
        }
    }

    private void DrawArenaOverlay(ShellLayout layout, AimSessionStateSnapshot state)
    {
        if (state.Settings.UsesFixedDuration)
        {
            DrawFixedDurationArenaOverlay(layout, state);
            return;
        }

        var overlayBounds = new Rectangle(
            layout.PlayAreaBounds.X + 18f,
            layout.PlayAreaBounds.Y + 18f,
            300f,
            214f);

        DrawPanelShape(overlayBounds, Card);
        Raylib.DrawRectangleLinesEx(overlayBounds, 1.5f, CardBorder);

        DrawLabel(_activeGamemode.DisplayName, overlayBounds.X + 14f, overlayBounds.Y + 12f, 24, Text);
        DrawLabel(state.Phase.ToString(), overlayBounds.X + 14f, overlayBounds.Y + 42f, 18, MutedText);
        DrawLabel($"Score  {state.Score:0.00}", overlayBounds.X + 14f, overlayBounds.Y + 72f, 24, Text);
        DrawLabel($"Hits  {state.HitCount}", overlayBounds.X + 14f, overlayBounds.Y + 102f, 18, Text);
        DrawLabel($"Avg react  {state.AverageReactionMs:0.00} ms", overlayBounds.X + 14f, overlayBounds.Y + 126f, 18, MutedText);
        DrawLabel($"Live targets  {state.Targets.Count}  HP  {state.Health:0.0}", overlayBounds.X + 14f, overlayBounds.Y + 150f, 16, MutedText);
        DrawLabel(GetRunStatusText(state), overlayBounds.X + 14f, overlayBounds.Y + 172f, 16, state.Settings.UsesFixedDuration ? Positive : MutedText);

        DrawRunProgressBar(new Rectangle(overlayBounds.X + 14f, overlayBounds.Bottom() - 18f, overlayBounds.Width - 28f, 10f), state);

        if (state.Phase == SessionPhase.PreGame)
        {
            DrawCenteredBanner(layout.PlayAreaBounds, "Click inside the sheet to start");
        }
        else if (state.Phase == SessionPhase.InGame)
        {
            DrawCenteredBanner(layout.PlayAreaBounds, GetArenaBannerText(state));
        }
    }

    private void DrawFixedDurationArenaOverlay(ShellLayout layout, AimSessionStateSnapshot state)
    {
        var overlayBounds = new Rectangle(
            layout.PlayAreaBounds.X + 18f,
            layout.PlayAreaBounds.Y + 18f,
            320f,
            152f);

        DrawPanelShape(overlayBounds, Card);
        Raylib.DrawRectangleLinesEx(overlayBounds, 1.5f, CardBorder);

        DrawLabel(_activeGamemode.DisplayName, overlayBounds.X + 14f, overlayBounds.Y + 12f, 24, Text);
        if (state.Phase == SessionPhase.PreGame)
        {
            DrawLabel("Ready", overlayBounds.X + 14f, overlayBounds.Y + 50f, 22, Text);
            DrawLabel("Click inside the sheet to start", overlayBounds.X + 14f, overlayBounds.Y + 82f, 16, MutedText);
            return;
        }

        DrawLabel($"Score  {state.Score:0.00}", overlayBounds.X + 14f, overlayBounds.Y + 50f, 22, Text);
        DrawLabel($"Hits  {state.HitCount}", overlayBounds.X + 14f, overlayBounds.Y + 78f, 18, Text);
        DrawLabel("Time Remaining", overlayBounds.X + 14f, overlayBounds.Y + 104f, 16, MutedText);
        DrawDurationBar(new Rectangle(overlayBounds.X + 14f, overlayBounds.Bottom() - 18f, overlayBounds.Width - 28f, 10f), state);
    }

    private void DrawTarget(ArenaRenderTransform arenaTransform, TargetSnapshot target)
    {
        var position = arenaTransform.ToScreenPoint(target.Position);
        var radius = arenaTransform.ToScreenLength(target.Radius);
        Raylib.DrawCircleV(position, radius, Danger);
        Raylib.DrawCircleV(position, radius * 0.42f, CardAlt);
    }

    private static void DrawCursor(ArenaRenderTransform arenaTransform, AimPoint cursorPosition, float radius)
    {
        var position = arenaTransform.ToScreenPoint(cursorPosition);
        var scaledRadius = arenaTransform.ToScreenLength(radius);
        Raylib.DrawCircleV(position, scaledRadius, CursorFill);
        Raylib.DrawCircleV(position, Math.Max(3f, scaledRadius * 0.45f), CursorHole);
    }

    private void DrawCenteredBanner(Rectangle areaBounds, string text)
    {
        var bannerWidth = Math.Min(500f, areaBounds.Width - 48f);
        var bounds = new Rectangle(
            areaBounds.X + ((areaBounds.Width - bannerWidth) / 2f),
            areaBounds.Y + 18f,
            bannerWidth,
            46f);

        DrawPanelShape(bounds, Card);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);
        DrawLabel(FitTextToWidth(text, 20, bounds.Width - 24f), bounds.X + 12f, bounds.Y + 12f, 20, Text);
    }

    private void DrawStartCountdown(Rectangle areaBounds, int? countdownValue)
    {
        if (countdownValue is null)
        {
            return;
        }

        var overlayWidth = 180f;
        var overlayHeight = 180f;
        var bounds = new Rectangle(
            areaBounds.X + ((areaBounds.Width - overlayWidth) / 2f),
            areaBounds.Y + ((areaBounds.Height - overlayHeight) / 2f),
            overlayWidth,
            overlayHeight);

        DrawPanelShape(bounds, Card);
        Raylib.DrawRectangleLinesEx(bounds, 2f, Accent);
        DrawLabel(countdownValue.Value.ToString(), bounds.X + 66f, bounds.Y + 48f, 72, Text);
    }

    private void DrawHealthBar(Rectangle bounds, AimSessionStateSnapshot state)
    {
        var normalizedHealth = Math.Clamp(
            state.Health / state.Settings.StartingHealth,
            0d,
            1.5d);

        Raylib.DrawRectangleRec(bounds, CardAlt);
        Raylib.DrawRectangleRec(
            new Rectangle(bounds.X, bounds.Y, bounds.Width * (float)normalizedHealth, bounds.Height),
            state.Health > state.Settings.StartingHealth * 0.3d ? Positive : Warning);
        Raylib.DrawRectangleLinesEx(bounds, 1f, CardBorder);
    }

    private void DrawDurationBar(Rectangle bounds, AimSessionStateSnapshot state)
    {
        var totalDuration = Math.Max(1d, state.Settings.FixedDurationSeconds ?? 1d);
        var remainingRatio = Math.Clamp(
            (totalDuration - state.ElapsedRunSeconds) / totalDuration,
            0d,
            1d);

        Raylib.DrawRectangleRec(bounds, CardAlt);
        Raylib.DrawRectangleRec(
            new Rectangle(bounds.X, bounds.Y, bounds.Width * (float)remainingRatio, bounds.Height),
            Positive);
        Raylib.DrawRectangleLinesEx(bounds, 1f, CardBorder);
    }

    private void DrawRunProgressBar(Rectangle bounds, AimSessionStateSnapshot state)
    {
        if (state.Settings.UsesFixedDuration)
        {
            DrawDurationBar(bounds, state);
            return;
        }

        DrawHealthBar(bounds, state);
    }

    private void DrawResultsOverlay(ShellLayout layout, SessionResult result)
    {
        var overlayWidth = Math.Min(840f, layout.PlayAreaBounds.Width - 36f);
        var overlayHeight = Math.Min(480f, layout.PlayAreaBounds.Height - 36f);
        var overlay = new Rectangle(
            layout.PlayAreaBounds.X + ((layout.PlayAreaBounds.Width - overlayWidth) / 2f),
            layout.PlayAreaBounds.Y + ((layout.PlayAreaBounds.Height - overlayHeight) / 2f),
            overlayWidth,
            overlayHeight);

        DrawPanelShape(overlay, Card);
        Raylib.DrawRectangleLinesEx(overlay, 2f, CardBorder);
        DrawLabel("Session Review", overlay.X + 20f, overlay.Y + 16f, 28, Text);

        var graphBounds = new Rectangle(overlay.X + 20f, overlay.Y + 60f, Math.Min(390f, overlay.Width * 0.46f), overlay.Height - 102f);
        DrawReactionGraph(graphBounds, result.Samples);

        var summaryX = graphBounds.Right() + 24f;
        var summaryWidth = overlay.Right() - summaryX - 20f;
        var telemetry = result.Telemetry;
        DrawLabel($"Time: {result.Summary.DurationSeconds:0.00}s", summaryX, overlay.Y + 60f, 22, Text);
        DrawLabel($"Score: {result.Summary.Score:0.00}", summaryX, overlay.Y + 90f, 22, Text);
        DrawLabel($"Avg react: {result.Summary.AverageReactionMs:0.00} ms", summaryX, overlay.Y + 120f, 20, Text);
        DrawLabel($"Avg hits/s: {result.Summary.AverageHitsPerSecond:0.00}", summaryX, overlay.Y + 146f, 20, Text);
        DrawLabel($"Avg eat: {result.Summary.AverageEatPerSecond:0.00}", summaryX, overlay.Y + 172f, 20, Text);
        DrawLabel($"Hits: {result.Summary.Hits}", summaryX, overlay.Y + 198f, 20, Text);
        if (telemetry is not null)
        {
            DrawLabel($"Median: {telemetry.MedianReactionMs:0.0} ms", summaryX, overlay.Y + 224f, 18, Text);
            DrawLabel($"P95: {telemetry.P95ReactionMs:0.0} ms", summaryX, overlay.Y + 248f, 18, Text);
            DrawLabel($"Path: {telemetry.AveragePathEfficiency * 100d:0}% direct", summaryX, overlay.Y + 272f, 18, Text);
            DrawLabel($"Travel: {telemetry.TotalCursorTravelDistance:0}px", summaryX, overlay.Y + 296f, 18, Text);
        }

        if (result.Summary.MaxEat.HasValue)
        {
            DrawLabel($"Max eat: {result.Summary.MaxEat.Value:0.00}", summaryX, overlay.Y + 326f, 20, Text);
            DrawLabel($"Cheese avg: {result.Summary.MaxEatReactionMs:0.00} ms", summaryX, overlay.Y + 352f, 18, MutedText);
            DrawLabel($"True avg: {result.Summary.MaxEatTrueReactionMs:0.00} ms", summaryX, overlay.Y + 376f, 18, MutedText);
        }
        else
        {
            DrawWrappedLabel(
                "Max eat unlocks once the run covers a full rolling 5-second segment.",
                summaryX,
                overlay.Y + 326f,
                summaryWidth,
                18,
                MutedText,
                3);
        }

        DrawWrappedLabel(
            result.Summary.SettingsChangedDuringRun
                ? "Settings changed during this run, so compare the graph with caution."
                : "Settings stayed stable during this run.",
            summaryX,
            overlay.Y + 404f,
            summaryWidth,
            18,
            result.Summary.SettingsChangedDuringRun ? Warning : Positive,
            3);

        DrawWrappedLabel(
            "Press R to stage a fresh run or switch to Analytics for the recent trend line.",
            overlay.X + 20f,
            overlay.Bottom() - 52f,
            overlay.Width - 40f,
            18,
            MutedText,
            2);
    }

    private void DrawExitPrompt(Rectangle playAreaBounds)
    {
        var scrim = new Color(2, 7, 10, 210);
        Raylib.DrawRectangleRec(playAreaBounds, scrim);

        var promptBounds = new Rectangle(
            playAreaBounds.X + ((playAreaBounds.Width - 420f) / 2f),
            playAreaBounds.Y + ((playAreaBounds.Height - 210f) / 2f),
            420f,
            210f);

        DrawPanelShape(promptBounds, Card);
        Raylib.DrawRectangleLinesEx(promptBounds, 2f, Warning);
        DrawLabel("Quit VrtAimTrainer?", promptBounds.X + 18f, promptBounds.Y + 18f, 28, Text);
        DrawWrappedLabel(
            "Escape now opens this warning first. Press Enter to quit, or choose Stay to keep your session open.",
            promptBounds.X + 18f,
            promptBounds.Y + 60f,
            promptBounds.Width - 36f,
            18,
            MutedText,
            3);

        var buttonY = promptBounds.Bottom() - 58f;
        var stayBounds = new Rectangle(promptBounds.X + 18f, buttonY, 178f, 40f);
        var quitBounds = new Rectangle(promptBounds.Right() - 196f, buttonY, 178f, 40f);

        if (_ui.Button("exit-stay", stayBounds, "Stay", CardAlt, Accent, Text))
        {
            DismissExitPrompt();
        }

        if (_ui.Button("exit-quit", quitBounds, "Quit", CardAlt, Danger, Text))
        {
            ConfirmExit();
        }
    }

    private void DrawReactionGraph(Rectangle bounds, IReadOnlyList<HitSample> samples)
    {
        DrawPanelShape(bounds, CardAlt);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);
        DrawLabel("Reaction Graph", bounds.X + 14f, bounds.Y + 12f, 22, Text);

        if (samples.Count < 2)
        {
            DrawWrappedLabel(
                "At least two hits are needed to draw the line.",
                bounds.X + 14f,
                bounds.Y + 48f,
                bounds.Width - 28f,
                18,
                MutedText,
                3);
            return;
        }

        var plotBounds = new Rectangle(bounds.X + 14f, bounds.Y + 44f, bounds.Width - 28f, bounds.Height - 62f);
        DrawGraphGrid(plotBounds);

        var maxX = Math.Max(1d, samples[^1].ElapsedSeconds);
        var maxY = Math.Max(1d, samples.Max(sample => sample.ReactionMilliseconds));

        for (var index = 1; index < samples.Count; index++)
        {
            var previous = PlotPoint(plotBounds, samples[index - 1].ElapsedSeconds / maxX, samples[index - 1].ReactionMilliseconds / maxY);
            var current = PlotPoint(plotBounds, samples[index].ElapsedSeconds / maxX, samples[index].ReactionMilliseconds / maxY);

            Raylib.DrawLineEx(previous, current, 2.5f, Accent);
            Raylib.DrawCircleV(current, 3f, Danger);
        }

        DrawLabel($"{maxX:0.0}s", plotBounds.Right() - 48f, plotBounds.Bottom() + 4f, 16, MutedText);
        DrawLabel($"{maxY:0}ms", plotBounds.X, plotBounds.Y - 18f, 16, MutedText);
    }

    private void DrawHistoryTrendGraph(Rectangle bounds, IReadOnlyList<SessionResult> sessions)
    {
        DrawSessionMetricTrendGraph(
            bounds,
            "Average Reaction Trend",
            sessions,
            session => session.Summary.AverageReactionMs,
            value => $"{value:0.0} ms latest",
            "Finish two runs to build a local trend line.",
            Positive,
            CursorFill);
    }

    private void DrawPathEfficiencyTrendGraph(Rectangle bounds, IReadOnlyList<SessionResult> sessions)
    {
        DrawSessionMetricTrendGraph(
            bounds,
            "Path Efficiency Trend",
            sessions,
            session => session.Telemetry is null || session.Telemetry.AveragePathEfficiency <= 0d
                ? null
                : session.Telemetry.AveragePathEfficiency * 100d,
            value => $"{value:0}% direct latest",
            "Finish two telemetry runs to chart movement quality.",
            Accent,
            CursorFill);
    }

    private void DrawOvertravelTrendGraph(Rectangle bounds, IReadOnlyList<SessionResult> sessions)
    {
        DrawSessionMetricTrendGraph(
            bounds,
            "Overtravel Trend",
            sessions,
            session => session.Telemetry?.AverageOvertravelDistance,
            value => $"{value:0}px latest",
            "Finish two telemetry runs to chart cursor waste.",
            Warning,
            CursorFill);
    }

    private void DrawSessionMetricTrendGraph(
        Rectangle bounds,
        string title,
        IReadOnlyList<SessionResult> sessions,
        Func<SessionResult, double?> valueSelector,
        Func<double, string> latestLabelSelector,
        string emptyText,
        Color lineColor,
        Color pointColor)
    {
        DrawPanelShape(bounds, CardAlt);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);
        DrawLabel(title, bounds.X + 12f, bounds.Y + 10f, 20, Text);

        var points = sessions
            .Take(8)
            .Reverse()
            .Select(session => new { Value = valueSelector(session), Session = session })
            .Where(entry => entry.Value.HasValue)
            .Select(entry => new { entry.Session, Value = entry.Value!.Value })
            .ToArray();

        if (points.Length < 2)
        {
            DrawWrappedLabel(
                emptyText,
                bounds.X + 12f,
                bounds.Y + 42f,
                bounds.Width - 24f,
                18,
                MutedText,
                3);
            return;
        }

        var plotBounds = new Rectangle(bounds.X + 12f, bounds.Y + 40f, bounds.Width - 24f, bounds.Height - 54f);
        DrawGraphGrid(plotBounds);

        var maxY = Math.Max(1d, points.Max(point => point.Value));
        for (var index = 1; index < points.Length; index++)
        {
            var previous = PlotPoint(plotBounds, (index - 1d) / (points.Length - 1d), points[index - 1].Value / maxY);
            var current = PlotPoint(plotBounds, index / (double)(points.Length - 1), points[index].Value / maxY);
            Raylib.DrawLineEx(previous, current, 2.5f, lineColor);
            Raylib.DrawCircleV(current, 3f, pointColor);
        }

        DrawLabel(latestLabelSelector(points[^1].Value), plotBounds.X, plotBounds.Bottom() + 4f, 16, MutedText);
    }

    private void DrawAnalyticsOverviewCard(Rectangle bounds, IReadOnlyList<SessionResult> sessions)
    {
        DrawPanelShape(bounds, CardAlt);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);
        DrawLabel("Local Overview", bounds.X + 14f, bounds.Y + 12f, 22, Text);

        if (sessions.Count == 0)
        {
            DrawLabel("No saved sessions yet.", bounds.X + 14f, bounds.Y + 48f, 18, MutedText);
            return;
        }

        var latestSession = sessions[0];
        var bestAverageReaction = sessions.Min(session => session.Summary.AverageReactionMs);
        var bestPathEfficiency = sessions
            .Where(session => session.Telemetry is not null)
            .Select(session => session.Telemetry!.AveragePathEfficiency)
            .DefaultIfEmpty()
            .Max();
        var totalHits = sessions.Sum(session => session.Summary.Hits);
        var recentAverageReaction = sessions.Take(5).Average(session => session.Summary.AverageReactionMs);
        var latestOvertravel = latestSession.Telemetry?.AverageOvertravelDistance ?? 0d;

        DrawAnalyticsOverviewStat(bounds, 14f, "Sessions", sessions.Count.ToString());
        DrawAnalyticsOverviewStat(bounds, 132f, "Hits", totalHits.ToString());
        DrawAnalyticsOverviewStat(bounds, 246f, "Best avg", $"{bestAverageReaction:0.0} ms");
        DrawAnalyticsOverviewStat(bounds, 14f, "Recent avg", $"{recentAverageReaction:0.0} ms", 60f);
        DrawAnalyticsOverviewStat(bounds, 132f, "Best path", $"{bestPathEfficiency * 100d:0}%", 60f);
        DrawAnalyticsOverviewStat(bounds, 246f, "Latest waste", $"{latestOvertravel:0}px", 60f);
        DrawLabel(
            FitTextToWidth($"Latest  {latestSession.GamemodeDisplayName}  |  {latestSession.CompletedAtUtc.LocalDateTime:MM-dd HH:mm}", 15, bounds.Width - 28f),
            bounds.X + 14f,
            bounds.Bottom() - 22f,
            15,
            MutedText);
    }

    private void DrawAnalyticsOverviewStat(Rectangle bounds, float offsetX, string label, string value, float offsetY = 34f)
    {
        DrawLabel(label, bounds.X + offsetX, bounds.Y + offsetY, 14, MutedText);
        DrawLabel(value, bounds.X + offsetX, bounds.Y + offsetY + 18f, 20, Text);
    }

    private void DrawAnalyticsRecentSessionCard(Rectangle bounds, SessionResult result)
    {
        DrawPanelShape(bounds, CardAlt);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);

        var telemetry = result.Telemetry;
        var pathText = telemetry is null ? "--" : $"{telemetry.AveragePathEfficiency * 100d:0}%";
        var overtravelText = telemetry is null ? "--" : $"{telemetry.AverageOvertravelDistance:0}px";
        var p95Text = telemetry is null ? "--" : $"{telemetry.P95ReactionMs:0.0} ms";

        DrawLabel(
            FitTextToWidth($"{result.CompletedAtUtc.LocalDateTime:MM-dd HH:mm}  {result.GamemodeDisplayName}", 16, bounds.Width - 28f),
            bounds.X + 12f,
            bounds.Y + 10f,
            16,
            Text);
        DrawLabel(
            FitTextToWidth($"Avg {result.Summary.AverageReactionMs:0.0} ms  |  P95 {p95Text}  |  Hits {result.Summary.Hits}", 14, bounds.Width - 24f),
            bounds.X + 12f,
            bounds.Y + 34f,
            14,
            MutedText);
        DrawLabel(
            FitTextToWidth($"Path {pathText}  |  Waste {overtravelText}  |  Score {result.Summary.Score:0}", 14, bounds.Width - 24f),
            bounds.X + 12f,
            bounds.Y + 54f,
            14,
            telemetry is null ? MutedText : Positive);
    }

    private static void DrawGraphGrid(Rectangle plotBounds)
    {
        const int divisions = 4;
        for (var index = 0; index <= divisions; index++)
        {
            var x = plotBounds.X + (plotBounds.Width * index / divisions);
            var y = plotBounds.Y + (plotBounds.Height * index / divisions);

            Raylib.DrawLineEx(new Vector2(x, plotBounds.Y), new Vector2(x, plotBounds.Bottom()), 1f, CardBorder);
            Raylib.DrawLineEx(new Vector2(plotBounds.X, y), new Vector2(plotBounds.Right(), y), 1f, CardBorder);
        }
    }

    private static Vector2 PlotPoint(Rectangle plotBounds, double normalizedX, double normalizedY)
    {
        return new Vector2(
            plotBounds.X + (float)(plotBounds.Width * normalizedX),
            plotBounds.Bottom() - (float)(plotBounds.Height * normalizedY));
    }

    private static void DrawSectionHeader(Rectangle bounds, string title)
    {
        DrawLabel(title, bounds.X, bounds.Y, 24, Text);
    }

    private static void DrawPanelShape(Rectangle bounds, Color fillColor)
    {
        Raylib.DrawRectangleRec(bounds, fillColor);
    }

    private static void DrawLabel(string text, float x, float y, int fontSize, Color color)
    {
        Raylib.DrawText(text, (int)x, (int)y, fontSize, color);
    }

    private static void DrawWrappedLabel(string text, float x, float y, float maxWidth, int fontSize, Color color, int maxLines)
    {
        var currentY = y;
        foreach (var line in WrapText(text, fontSize, maxWidth, maxLines))
        {
            Raylib.DrawText(line, (int)x, (int)currentY, fontSize, color);
            currentY += fontSize + 4f;
        }
    }

    private static float MeasureWrappedTextHeight(string text, int fontSize, float maxWidth, int maxLines)
    {
        var lines = WrapText(text, fontSize, maxWidth, maxLines);
        if (lines.Count == 0)
        {
            return 0f;
        }

        return (lines.Count * fontSize) + ((lines.Count - 1) * 4f);
    }

    private static IReadOnlyList<string> WrapText(string text, int fontSize, float maxWidth, int maxLines)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [string.Empty];
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(currentLine)
                ? word
                : $"{currentLine} {word}";

            if (Raylib.MeasureText(candidate, fontSize) <= maxWidth)
            {
                currentLine = candidate;
                continue;
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                if (lines.Count == maxLines)
                {
                    return TrimWrappedLines(lines, fontSize, maxWidth);
                }
            }

            currentLine = FitTextToWidth(word, fontSize, maxWidth);
        }

        if (!string.IsNullOrEmpty(currentLine) && lines.Count < maxLines)
        {
            lines.Add(currentLine);
        }

        return TrimWrappedLines(lines, fontSize, maxWidth);
    }

    private static IReadOnlyList<string> TrimWrappedLines(List<string> lines, int fontSize, float maxWidth)
    {
        if (lines.Count == 0)
        {
            return [string.Empty];
        }

        lines[^1] = FitTextToWidth(lines[^1], fontSize, maxWidth);
        return lines;
    }

    private static string FitTextToWidth(string text, int fontSize, float maxWidth)
    {
        if (Raylib.MeasureText(text, fontSize) <= maxWidth)
        {
            return text;
        }

        const string ellipsis = "...";
        var trimmed = text;
        while (trimmed.Length > 1 && Raylib.MeasureText(trimmed + ellipsis, fontSize) > maxWidth)
        {
            trimmed = trimmed[..^1];
        }

        return trimmed + ellipsis;
    }

    private string GetRunStatusText(AimSessionStateSnapshot state)
    {
        if (state.Settings.UsesFixedDuration)
        {
            return string.Empty;
        }

        return $"Minimum spread  {state.Settings.MinimumSpawnDistance:0}px";
    }

    private string GetArenaBannerText(AimSessionStateSnapshot state)
    {
        if (state.Settings.UsesGrid)
        {
            return $"{state.Settings.GridRows}x{state.Settings.GridColumns} grid  |  {state.Settings.EffectiveConcurrentTargetCount} live target(s)";
        }

        return $"Minimum spawn distance: {state.Settings.MinimumSpawnDistance:0}px";
    }
}
