using System.Numerics;
using Raylib_cs;
using VrtAimTrainer.Core.Features.Gameplay;
using VrtAimTrainer.Desktop.Features.Gamemodes;
using VrtAimTrainer.Desktop.Features.Workflow;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed partial class TrainerApplication
{
    private const float ControlHubGap = 12f;

    private void DrawPlayPage(Rectangle bounds)
    {
        const string introText = "Live-run actions only. Modes, presets, run settings, mouse tuning, and sheet settings now apply instantly.";

        DrawSectionHeader(bounds, "Play");
        var cursor = CreatePageCursor(bounds, introText, 17, 3);

        var workflowBounds = cursor.Take(132f);
        DrawPanelShape(workflowBounds, CardAlt);
        Raylib.DrawRectangleLinesEx(workflowBounds, 1.5f, CardBorder);
        DrawLabel("Workflow", workflowBounds.X + 14f, workflowBounds.Y + 14f, 22, Text);
        DrawLabel(
            FitTextToWidth($"Live mode  {_activeGamemode.DisplayName}", 18, workflowBounds.Width - 28f),
            workflowBounds.X + 14f,
            workflowBounds.Y + 42f,
            18,
            Text);
        DrawLabel(
            FitTextToWidth($"Preset  {GetDraftPresetLabel()}", 16, workflowBounds.Width - 28f),
            workflowBounds.X + 14f,
            workflowBounds.Y + 70f,
            16,
            MutedText);
        DrawWrappedLabel(
            "Changes from Modes, Presets, Run, Mouse, and Sheet rebuild the live controller immediately. Use the controls below only to reset or finish the current run.",
            workflowBounds.X + 14f,
            workflowBounds.Y + 92f,
            workflowBounds.Width - 28f,
            15,
            Positive,
            2);

        cursor.Space(ControlHubGap);
        var (stageBounds, finishBounds) = cursor.TakePair(42f, 10f);
        if (_ui.Button(
                "stage-run",
                stageBounds,
                "Stage Fresh Run",
                CardAlt,
                Accent,
                Text))
        {
            _runtimeHost.Reset();
        }

        if (_ui.Button(
                "finish-run",
                finishBounds,
                "Finish Run",
                CardAlt,
                Warning,
                Text))
        {
            _runtimeHost.Finish();
        }

        cursor.Space(ControlHubGap);
        var bestScore = _recentSessions.Count == 0 ? 0d : _recentSessions.Max(session => session.Summary.Score);
        var bestReaction = _recentSessions.Count == 0 ? 0d : _recentSessions.Min(session => session.Summary.AverageReactionMs);
        var benchmarkBounds = cursor.Take(154f);
        DrawPanelShape(benchmarkBounds, CardAlt);
        Raylib.DrawRectangleLinesEx(benchmarkBounds, 1.5f, CardBorder);
        DrawLabel("Local Benchmarks", benchmarkBounds.X + 14f, benchmarkBounds.Y + 14f, 22, Text);
        DrawLabel($"Best score  {bestScore:0.00}", benchmarkBounds.X + 14f, benchmarkBounds.Y + 46f, 18, Text);
        DrawLabel($"Best avg reaction  {bestReaction:0.00} ms", benchmarkBounds.X + 14f, benchmarkBounds.Y + 70f, 18, Text);
        DrawLabel($"Stored sessions  {_recentSessions.Count}", benchmarkBounds.X + 14f, benchmarkBounds.Y + 94f, 18, MutedText);
        DrawWrappedLabel(
            "Page navigation lives in the top bar. The control hub stays focused on the current page only.",
            benchmarkBounds.X + 14f,
            benchmarkBounds.Y + 116f,
            benchmarkBounds.Width - 28f,
            15,
            MutedText,
            2);
    }

    private void DrawModesPage(Rectangle bounds)
    {
        DrawSectionHeader(bounds, "Modes");
        var cursor = CreatePageCursor(bounds);

        DrawModesCatalogStatus(cursor.Take(_gamemodeCatalog.LoadFailures.Count > 0 ? 112f : 86f));
        cursor.Space(ControlHubGap);

        foreach (var gamemode in _gamemodeCatalog.Gamemodes)
        {
            var cardBounds = cursor.Take(116f);
            var isLive = string.Equals(gamemode.Id, _activeGamemode.Id, StringComparison.OrdinalIgnoreCase);
            var fillColor = isLive ? Accent : CardAlt;
            var borderColor = isLive ? Accent : CardBorder;
            DrawPanelShape(cardBounds, fillColor);
            Raylib.DrawRectangleLinesEx(cardBounds, 1.5f, borderColor);

            DrawLabel(
                FitTextToWidth(gamemode.DisplayName, 22, cardBounds.Width - 150f),
                cardBounds.X + 14f,
                cardBounds.Y + 12f,
                22,
                isLive ? Background : Text);
            DrawLabel(
                FitTextToWidth($"{gamemode.PluginMetadata.DisplayName}  v{gamemode.PluginMetadata.Version}", 16, cardBounds.Width - 150f),
                cardBounds.X + 14f,
                cardBounds.Y + 40f,
                16,
                isLive ? Background : MutedText);
            DrawWrappedLabel(
                gamemode.Description,
                cardBounds.X + 14f,
                cardBounds.Y + 64f,
                cardBounds.Width - 130f,
                16,
                isLive ? Background : Text,
                2);
            DrawLabel(
                isLive ? "Live" : string.Empty,
                cardBounds.Right() - 106f,
                cardBounds.Y + 14f,
                16,
                isLive ? Background : MutedText);

            if (_ui.Button(
                    $"mode-{gamemode.Id}",
                    new Rectangle(cardBounds.Right() - 104f, cardBounds.Y + 34f, 90f, 38f),
                    isLive ? "Live" : "Use",
                    isLive ? Background : Card,
                    Accent,
                    isLive ? Accent : Text) && !isLive)
            {
                SelectGamemode(gamemode);
            }

            cursor.Space(ControlHubGap);
        }
    }

    private void DrawPresetsPage(Rectangle bounds)
    {
        DrawSectionHeader(bounds, "Presets");
        var cursor = CreatePageCursor(bounds);
        var presets = _presetCatalog.GetPresetsForGamemode(_draftGamemode.Id);

        DrawPresetsCatalogStatus(cursor.Take(86f), presets);
        cursor.Space(ControlHubGap);

        foreach (var preset in presets)
        {
            var cardBounds = cursor.Take(88f);
            var isSelected = string.Equals(_selectedPreset?.Id, preset.Id, StringComparison.OrdinalIgnoreCase);
            DrawPanelShape(cardBounds, isSelected ? Accent : CardAlt);
            Raylib.DrawRectangleLinesEx(cardBounds, 1.5f, isSelected ? Accent : CardBorder);
            DrawLabel(
                FitTextToWidth(preset.Name, 21, cardBounds.Width - 132f),
                cardBounds.X + 14f,
                cardBounds.Y + 12f,
                21,
                isSelected ? Background : Text);
            DrawLabel(
                FitTextToWidth(GetPresetSummaryLine(preset), 14, cardBounds.Width - 128f),
                cardBounds.X + 14f,
                cardBounds.Y + 38f,
                14,
                isSelected ? Background : MutedText);
            DrawWrappedLabel(
                preset.Description,
                cardBounds.X + 14f,
                cardBounds.Y + 56f,
                cardBounds.Width - 128f,
                14,
                isSelected ? Background : Text,
                2);

            if (_ui.Button(
                    $"preset-{preset.Id}",
                    new Rectangle(cardBounds.Right() - 104f, cardBounds.Y + 30f, 90f, 38f),
                    isSelected ? "Live" : "Use",
                    isSelected ? Background : Card,
                    Accent,
                    isSelected ? Accent : Text) && !isSelected)
            {
                ApplyPreset(preset);
            }

            cursor.Space(ControlHubGap);
        }
    }

    private void SelectGamemode(LoadedAimGamemode gamemode)
    {
        _workflowState.SelectGamemode(gamemode);
        ApplyDraftIfChanged();
        _activePage = ShellPage.Presets;
    }

    private void DrawRunSetupPage(Rectangle bounds)
    {
        const string introText = "Edit live run values here. Slider changes apply when you release the handle.";

        DrawSectionHeader(bounds, "Run Setup");
        var cursor = CreatePageCursor(bounds, introText, 16, 3);
        var currentSettings = _draftSettings;

        var cursorDiameter = _ui.Slider(
            "cursor-size",
            cursor.Take(46f),
            "Cursor diameter",
            _draftSettings.CursorDiameter,
            5f,
            100f,
            "0.0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var scorePerHit = _ui.Slider(
            "score-per-hit",
            cursor.Take(46f),
            "Score per hit",
            (float)_draftSettings.ScorePerHit,
            5f,
            150f,
            "0.0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var pressure = _ui.Slider(
            "pressure",
            cursor.Take(46f),
            "Pressure",
            (float)_draftSettings.Pressure,
            5f,
            150f,
            "0.0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var startingHealth = _ui.Slider(
            "starting-health",
            cursor.Take(46f),
            "Starting HP",
            (float)_draftSettings.StartingHealth,
            20f,
            250f,
            "0.0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var roundTimeSeconds = _ui.Slider(
            "round-time",
            cursor.Take(46f),
            "Round time (s)",
            (float)(_draftSettings.FixedDurationSeconds ?? 45d),
            10f,
            180f,
            "0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var cheeseThreshold = _ui.Slider(
            "cheese-threshold",
            cursor.Take(46f),
            "Cheese threshold (ms)",
            (float)_draftSettings.CheeseThresholdMs,
            2f,
            250f,
            "0.0",
            CardAlt,
            Accent,
            Text);

        _draftSettings = currentSettings with
        {
            CursorDiameter = cursorDiameter,
            ScorePerHit = scorePerHit,
            Pressure = pressure,
            StartingHealth = startingHealth,
            FixedDurationSeconds = roundTimeSeconds,
            CheeseThresholdMs = cheeseThreshold,
        };

        var hasCommittedChange =
            WasCommittedSettingChange("cursor-size", currentSettings.CursorDiameter, cursorDiameter) ||
            WasCommittedSettingChange("score-per-hit", currentSettings.ScorePerHit, scorePerHit) ||
            WasCommittedSettingChange("pressure", currentSettings.Pressure, pressure) ||
            WasCommittedSettingChange("starting-health", currentSettings.StartingHealth, startingHealth) ||
            WasCommittedSettingChange("round-time", currentSettings.FixedDurationSeconds ?? 45d, roundTimeSeconds) ||
            WasCommittedSettingChange("cheese-threshold", currentSettings.CheeseThresholdMs, cheeseThreshold);
        if (hasCommittedChange)
        {
            ApplyDraftIfChanged();
        }

        cursor.Space(ControlHubGap);
        var (resetBounds, _) = cursor.TakePair(40f, 10f);
        if (_ui.Button(
                "run-reset-preset",
                resetBounds,
                "Reset To Preset",
                CardAlt,
                Accent,
                Text))
        {
            ResetDraftToSelectedPreset();
        }

        cursor.Space(ControlHubGap);
        DrawWrappedLabel(
            GetRunSetupSummary(),
            cursor.X,
            cursor.CurrentY,
            cursor.Width,
            16,
            MutedText,
            2);
    }

    private void DrawSheetPage(Rectangle bounds)
    {
        const string introText = "Edit live sheet geometry here. Slider changes apply when you release the handle.";

        DrawSectionHeader(bounds, "Sheet");
        var cursor = CreatePageCursor(bounds, introText, 16, 3);
        var currentSettings = _draftSettings;

        var sheetWidth = _ui.Slider(
            "sheet-width",
            cursor.Take(46f),
            "Sheet width",
            _draftSettings.EffectiveSheetWidth,
            700f,
            1800f,
            "0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var sheetHeight = _ui.Slider(
            "sheet-height",
            cursor.Take(46f),
            "Sheet height",
            _draftSettings.EffectiveSheetHeight,
            700f,
            1800f,
            "0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var targetDiameter = _ui.Slider(
            "target-size",
            cursor.Take(46f),
            "Ball size",
            _draftSettings.TargetDiameter,
            10f,
            140f,
            "0.0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var minimumSpawnDistance = _ui.Slider(
            "spawn-safety",
            cursor.Take(46f),
            "Ball spread",
            _draftSettings.MinimumSpawnDistance,
            120f,
            700f,
            "0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var spawnInsetRatio = _ui.Slider(
            "spawn-inset",
            cursor.Take(46f),
            "Edge padding",
            _draftSettings.SpawnInsetRatio,
            0.05f,
            0.30f,
            "0.00",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var spawnTopPadding = _ui.Slider(
            "spawn-top",
            cursor.Take(46f),
            "Top safe padding",
            _draftSettings.SpawnTopPadding,
            0f,
            160f,
            "0",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var spawnBottomPadding = _ui.Slider(
            "spawn-bottom",
            cursor.Take(46f),
            "Bottom safe padding",
            _draftSettings.SpawnBottomPadding,
            0f,
            180f,
            "0",
            CardAlt,
            Accent,
            Text);

        var updatedDraftSettings = currentSettings with
        {
            ArenaSize = MathF.Max(sheetWidth, sheetHeight),
            SheetWidth = sheetWidth,
            SheetHeight = sheetHeight,
            TargetDiameter = targetDiameter,
            MinimumSpawnDistance = minimumSpawnDistance,
            SpawnInsetRatio = spawnInsetRatio,
            SpawnTopPadding = spawnTopPadding,
            SpawnBottomPadding = spawnBottomPadding,
        };

        cursor.Space(ControlHubGap);
        var (squareBounds, wideBounds) = cursor.TakePair(40f, 10f);
        if (_ui.Button(
                "sheet-square",
                squareBounds,
                "Square",
                CardAlt,
                Accent,
                Text))
        {
            updatedDraftSettings = updatedDraftSettings with
            {
                ArenaSize = 1200f,
                SheetWidth = 1200f,
                SheetHeight = 1200f,
                TargetDiameter = 60f,
                MinimumSpawnDistance = 300f,
                SpawnInsetRatio = 1f / 6f,
            };
        }

        if (_ui.Button(
                "sheet-wide",
                wideBounds,
                "Wide",
                CardAlt,
                Positive,
                Text))
        {
            updatedDraftSettings = updatedDraftSettings with
            {
                ArenaSize = 1450f,
                SheetWidth = 1450f,
                SheetHeight = 980f,
                TargetDiameter = 58f,
                MinimumSpawnDistance = 360f,
                SpawnInsetRatio = 0.13f,
            };
        }

        _draftSettings = updatedDraftSettings;

        var hasCommittedChange =
            WasCommittedSettingChange("sheet-width", currentSettings.EffectiveSheetWidth, sheetWidth) ||
            WasCommittedSettingChange("sheet-height", currentSettings.EffectiveSheetHeight, sheetHeight) ||
            WasCommittedSettingChange("target-size", currentSettings.TargetDiameter, targetDiameter) ||
            WasCommittedSettingChange("spawn-safety", currentSettings.MinimumSpawnDistance, minimumSpawnDistance) ||
            WasCommittedSettingChange("spawn-inset", currentSettings.SpawnInsetRatio, spawnInsetRatio) ||
            WasCommittedSettingChange("spawn-top", currentSettings.SpawnTopPadding, spawnTopPadding) ||
            WasCommittedSettingChange("spawn-bottom", currentSettings.SpawnBottomPadding, spawnBottomPadding);
        if (hasCommittedChange)
        {
            ApplyDraftIfChanged();
        }

        cursor.Space(ControlHubGap);
        var (resetBounds, _) = cursor.TakePair(40f, 10f);
        if (_ui.Button(
                "sheet-reset-preset",
                resetBounds,
                "Reset To Preset",
                CardAlt,
                Accent,
                Text))
        {
            ResetDraftToSelectedPreset();
        }

        cursor.Space(ControlHubGap);
        DrawWrappedLabel(
            "Wider sheets benefit most from 1440p or borderless, but mouse feel stays the same because the playfield mapping is fixed.",
            cursor.X,
            cursor.CurrentY,
            cursor.Width,
            16,
            MutedText,
            3);
    }

    private void DrawMouseSettingsPage(Rectangle bounds)
    {
        const string introText = "Gameplay mouse tuning is app-level. Sensitivity changes apply live during active runs and save automatically with the rest of the shell state.";

        DrawSectionHeader(bounds, "Mouse");
        var cursor = CreatePageCursor(bounds, introText, 16, 3);
        var currentSettings = _mouseSettings;

        var sensitivity = _ui.Slider(
            "mouse-sensitivity",
            cursor.Take(46f),
            "Sensitivity",
            currentSettings.Sensitivity,
            0.20f,
            3f,
            "0.00",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var horizontalScale = _ui.Slider(
            "mouse-horizontal-scale",
            cursor.Take(46f),
            "Horizontal scale",
            currentSettings.HorizontalScale,
            0.50f,
            2f,
            "0.00",
            CardAlt,
            Accent,
            Text);

        cursor.Space(8f);
        var verticalScale = _ui.Slider(
            "mouse-vertical-scale",
            cursor.Take(46f),
            "Vertical scale",
            currentSettings.VerticalScale,
            0.50f,
            2f,
            "0.00",
            CardAlt,
            Accent,
            Text);

        cursor.Space(ControlHubGap);
        var invertVertical = _ui.Toggle(
            "mouse-invert-vertical",
            cursor.Take(42f),
            "Invert vertical",
            currentSettings.InvertVertical,
            CardAlt,
            Accent,
            Text);

        _mouseSettings = new MouseSettings(
            sensitivity,
            horizontalScale,
            verticalScale,
            invertVertical).Normalize();

        cursor.Space(ControlHubGap);
        DrawWrappedLabel(
            GetMouseSettingsSummary(),
            cursor.X,
            cursor.CurrentY,
            cursor.Width,
            16,
            MutedText,
            3);
    }

    private void DrawDisplayPage(Rectangle bounds)
    {
        const string introText = "Workspace size changes the shell only. The playfield mapping stays fixed and the display profile is saved automatically.";

        DrawSectionHeader(bounds, "Display");
        var cursor = CreatePageCursor(bounds, introText, 17, 3);

        foreach (var profile in DisplayProfiles)
        {
            var isActive = profile.Id == _activeDisplayProfileId;
            if (_ui.Button(
                    $"display-{profile.Id}",
                    cursor.Take(44f),
                    profile.Label,
                    isActive ? Accent : CardAlt,
                    Accent,
                    isActive ? Background : Text))
            {
                ApplyDisplayProfile(profile);
            }

            cursor.Space(4f);
            var descriptionHeight = MeasureWrappedTextHeight(profile.Description, 16, cursor.Width - 12f, 3);
            DrawWrappedLabel(profile.Description, cursor.X + 6f, cursor.CurrentY, cursor.Width - 12f, 16, MutedText, 3);
            cursor.Space(descriptionHeight + 18f);
        }

        var currentBounds = cursor.Take(110f);
        DrawPanelShape(currentBounds, CardAlt);
        Raylib.DrawRectangleLinesEx(currentBounds, 1.5f, CardBorder);
        DrawLabel("Current workspace", currentBounds.X + 14f, currentBounds.Y + 14f, 22, Text);
        DrawLabel($"{Raylib.GetScreenWidth()} x {Raylib.GetScreenHeight()}", currentBounds.X + 14f, currentBounds.Y + 44f, 18, Text);
        DrawLabel(
            _frameState.Phase == SessionPhase.PostGame
                ? "Cursor visible for review."
                : "Cursor hides only when you are over the playfield.",
            currentBounds.X + 14f,
            currentBounds.Y + 70f,
            16,
            MutedText);
    }

    private void DrawAnalyticsPage(Rectangle bounds)
    {
        DrawSectionHeader(bounds, "Analytics");
        var cursor = CreatePageCursor(bounds);
        var analyticsFilterOptions =
            new[] { (AllGamemodesAnalyticsFilterId, "All modes") }
                .Concat(_gamemodeCatalog.Gamemodes.Select(gamemode => (gamemode.Id, gamemode.DisplayName)))
                .ToArray();
        var selectedFilterId = ResolveAnalyticsGamemodeFilterId(_analyticsGamemodeFilterId);
        var selectedFilterIndex = Array.FindIndex(
            analyticsFilterOptions,
            option => string.Equals(option.Item1, selectedFilterId, StringComparison.OrdinalIgnoreCase));
        var nextFilterIndex = _ui.Dropdown(
            "analytics-mode-filter",
            cursor.Take(62f),
            "Mode filter",
            analyticsFilterOptions.Select(option => option.Item2).ToArray(),
            selectedFilterIndex < 0 ? 0 : selectedFilterIndex,
            CardAlt,
            Accent,
            Text);
        _analyticsGamemodeFilterId = analyticsFilterOptions[nextFilterIndex].Item1;
        cursor.Space(ControlHubGap);

        var analyticsSessions = GetFilteredAnalyticsSessions();

        DrawAnalyticsOverviewCard(cursor.Take(104f), analyticsSessions);
        cursor.Space(ControlHubGap);

        DrawHistoryTrendGraph(cursor.Take(134f), analyticsSessions);
        cursor.Space(ControlHubGap);
        DrawPathEfficiencyTrendGraph(cursor.Take(134f), analyticsSessions);
        cursor.Space(ControlHubGap);
        DrawOvertravelTrendGraph(cursor.Take(134f), analyticsSessions);

        cursor.Space(14f);
        DrawLabel("Recent sessions", cursor.X, cursor.CurrentY, 22, Text);
        cursor.Space(34f);

        if (analyticsSessions.Count == 0)
        {
            DrawLabel("No local sessions saved for this filter yet.", cursor.X, cursor.CurrentY, 18, MutedText);
            cursor.Space(24f);
        }
        else
        {
            foreach (var result in analyticsSessions.Take(3))
            {
                DrawAnalyticsRecentSessionCard(cursor.Take(74f), result);
                cursor.Space(8f);
            }
        }

        var storageText = FitTextToWidth(_historyStore.StorageDirectory, 15, cursor.Width);
        DrawWrappedLabel(storageText, cursor.X, bounds.Bottom() - 38f, cursor.Width, 15, MutedText, 2);
    }

    private VerticalFlowCursor CreatePageCursor(Rectangle bounds, string introText, int fontSize, int maxLines)
    {
        var introY = bounds.Y + 34f;
        DrawWrappedLabel(introText, bounds.X, introY, bounds.Width, fontSize, MutedText, maxLines);
        var introHeight = MeasureWrappedTextHeight(introText, fontSize, bounds.Width, maxLines);
        return new VerticalFlowCursor(new Rectangle(bounds.X, introY + introHeight + 22f, bounds.Width, bounds.Height));
    }

    private static VerticalFlowCursor CreatePageCursor(Rectangle bounds)
    {
        return new VerticalFlowCursor(new Rectangle(bounds.X, bounds.Y + 42f, bounds.Width, bounds.Height));
    }

    private void DrawModesCatalogStatus(Rectangle bounds)
    {
        var issuesColor = _gamemodeCatalog.LoadFailures.Count == 0 ? Positive : Warning;
        var activeModeText = FitTextToWidth(_activeGamemode.DisplayName, 16, bounds.Width - 244f);

        Raylib.DrawRectangleRec(bounds, Background);
        Raylib.DrawRectangleLinesEx(bounds, 1f, CardBorder);
        Raylib.DrawRectangleRec(new Rectangle(bounds.X, bounds.Y, bounds.Width, 4f), CardBorder);
        DrawLabel("Catalog Status", bounds.X + 14f, bounds.Y + 10f, 17, MutedText);

        var firstRowLabelY = bounds.Y + 32f;
        var firstRowValueY = firstRowLabelY + 18f;
        DrawLabel("Loaded", bounds.X + 14f, firstRowLabelY, 14, MutedText);
        DrawLabel(_gamemodeCatalog.Gamemodes.Count.ToString(), bounds.X + 14f, firstRowValueY, 22, Text);
        DrawLabel("Issues", bounds.X + 132f, firstRowLabelY, 14, MutedText);
        DrawLabel(_gamemodeCatalog.LoadFailures.Count.ToString(), bounds.X + 132f, firstRowValueY, 22, issuesColor);
        DrawLabel("Live", bounds.X + 230f, firstRowLabelY, 14, MutedText);
        DrawLabel(
            activeModeText,
            bounds.X + 230f,
            firstRowValueY + 2f,
            16,
            Positive);

        if (_gamemodeCatalog.LoadFailures.Count > 0)
        {
            Raylib.DrawLineEx(
                new Vector2(bounds.X + 14f, bounds.Y + 72f),
                new Vector2(bounds.Right() - 14f, bounds.Y + 72f),
                1f,
                CardBorder);
            DrawLabel(
                FitTextToWidth($"Latest issue  {_gamemodeCatalog.LoadFailures[0].Message}", 14, bounds.Width - 28f),
                bounds.X + 14f,
                bounds.Y + 82f,
                14,
                Warning);
        }
    }

    private void DrawPresetsCatalogStatus(Rectangle bounds, IReadOnlyList<TrainingPreset> presets)
    {
        var builtInCount = presets.Count(preset => preset.IsBuiltIn);
        var activePresetText = IsDraftModifiedFromPreset()
            ? "Custom live edits"
            : GetDraftPresetLabel();

        Raylib.DrawRectangleRec(bounds, Background);
        Raylib.DrawRectangleLinesEx(bounds, 1f, CardBorder);
        Raylib.DrawRectangleRec(new Rectangle(bounds.X, bounds.Y, bounds.Width, 4f), CardBorder);
        DrawLabel("Preset Status", bounds.X + 14f, bounds.Y + 10f, 17, MutedText);

        var firstRowLabelY = bounds.Y + 32f;
        var firstRowValueY = firstRowLabelY + 18f;
        DrawLabel("Loaded", bounds.X + 14f, firstRowLabelY, 14, MutedText);
        DrawLabel(presets.Count.ToString(), bounds.X + 14f, firstRowValueY, 22, Text);
        DrawLabel("Built-in", bounds.X + 124f, firstRowLabelY, 14, MutedText);
        DrawLabel(builtInCount.ToString(), bounds.X + 124f, firstRowValueY, 22, Text);
        DrawLabel("Active", bounds.X + 236f, firstRowLabelY, 14, MutedText);
        DrawLabel(
            FitTextToWidth(activePresetText, 16, bounds.Width - 250f),
            bounds.X + 236f,
            firstRowValueY + 2f,
            16,
            IsDraftModifiedFromPreset() ? Warning : Positive);
    }

    private string GetPresetSummaryLine(TrainingPreset preset)
    {
        var sourceLabel = preset.IsBuiltIn ? "Built-in" : "Plugin";
        if (preset.Settings.UsesGrid)
        {
            return $"{sourceLabel}  |  {preset.Settings.GridRows}x{preset.Settings.GridColumns}  |  {preset.Settings.EffectiveConcurrentTargetCount} live  |  {preset.Settings.FixedDurationSeconds ?? 0d:0}s";
        }

        return $"{sourceLabel}  |  {preset.Settings.EffectiveSheetWidth:0}x{preset.Settings.EffectiveSheetHeight:0}  |  spread {preset.Settings.MinimumSpawnDistance:0}px";
    }

    private string GetRunSetupSummary()
    {
        var format = _draftSettings.UsesGrid
            ? $"{_draftSettings.GridRows}x{_draftSettings.GridColumns}, {_draftSettings.EffectiveConcurrentTargetCount} live target(s)"
            : $"reactive sheet, {_draftSettings.MinimumSpawnDistance:0}px minimum spread";

        return $"Preset source: {GetDraftPresetLabel()}. Mode format: {format}. Fixed round: {(_draftSettings.FixedDurationSeconds ?? 45d):0}s.";
    }

    private string GetMouseSettingsSummary()
    {
        var normalizedSettings = _mouseSettings.Normalize();
        var verticalDirection = normalizedSettings.InvertVertical ? "inverted" : "normal";
        return $"Live aim scale: X {normalizedSettings.EffectiveHorizontalSensitivity:0.00}x, Y {Math.Abs(normalizedSettings.EffectiveVerticalSensitivity):0.00}x ({verticalDirection} vertical). During a run the game recenters raw mouse motion so you can tune feel without changing display scale.";
    }

    private bool WasCommittedSettingChange(string controlId, float previousValue, float nextValue)
    {
        return _ui.WasCommitted(controlId) && Math.Abs(previousValue - nextValue) > 0.001f;
    }

    private bool WasCommittedSettingChange(string controlId, double previousValue, double nextValue)
    {
        return _ui.WasCommitted(controlId) && Math.Abs(previousValue - nextValue) > 0.001d;
    }
}
