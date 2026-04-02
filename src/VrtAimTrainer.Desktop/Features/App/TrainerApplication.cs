using System.Numerics;
using Raylib_cs;
using VrtAimTrainer.Core.Features.Analytics;
using VrtAimTrainer.Core.Features.Gameplay;
using VrtAimTrainer.Core.Features.History;
using VrtAimTrainer.Desktop.Features.Gamemodes;
using VrtAimTrainer.Desktop.Features.Ui;
using VrtAimTrainer.Desktop.Features.Workflow;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed partial class TrainerApplication
{
    private const string AllGamemodesAnalyticsFilterId = "__all__";
    private const int DefaultWindowHeight = 1080;
    private const int DefaultWindowWidth = 1920;
    private static readonly Color Accent = new(82, 205, 225, 255);
    private static readonly Color ArenaInner = new(14, 45, 55, 255);
    private static readonly Color ArenaOuter = new(9, 28, 35, 255);
    private static readonly Color Background = new(4, 11, 15, 255);
    private static readonly Color Card = new(10, 24, 30, 240);
    private static readonly Color CardAlt = new(15, 33, 40, 255);
    private static readonly Color CardBorder = new(57, 103, 112, 255);
    private static readonly Color CursorFill = new(244, 231, 183, 255);
    private static readonly Color CursorHole = new(4, 11, 15, 255);
    private static readonly Color Danger = new(243, 101, 74, 255);
    private static readonly Color MutedText = new(139, 172, 181, 255);
    private static readonly Color Positive = new(103, 214, 134, 255);
    private static readonly Color SpawnBounds = new(7, 20, 25, 255);
    private static readonly Color Text = new(234, 241, 243, 255);
    private static readonly Color Warning = new(248, 199, 114, 255);
    private static readonly DisplayProfile[] DisplayProfiles =
    [
        new DisplayProfile("1080p", "Windowed 1080p", "Default balanced workspace for gameplay and menus.", 1920, 1080, false),
        new DisplayProfile("1440p", "Windowed 1440p", "Extra room for the larger sheet and analytics cards.", 2560, 1440, false),
        new DisplayProfile("fullscreen", "Borderless", "Fills the active monitor without changing the desktop display mode.", null, null, true),
    ];

    private readonly AimGamemodeCatalog _gamemodeCatalog;
    private readonly ISessionHistoryStore _historyStore;
    private readonly int _logicalArenaViewportHeight;
    private readonly int _logicalArenaViewportWidth;
    private readonly TrainingPresetCatalog _presetCatalog;
    private readonly List<SessionResult> _recentSessions;
    private readonly TrainerAppStateStore _appStateStore;
    private readonly AimSessionRuntimeHost _runtimeHost;
    private readonly TrainerWindowState _windowState = new();
    private readonly TrainerWorkflowState _workflowState;
    private readonly ImmediateUi _ui = new();
    private string _analyticsGamemodeFilterId = string.Empty;
    private AimSessionStateSnapshot _frameState;
    private Guid? _lastPersistedSessionId;
    private TrainerAppState? _lastPersistedAppState;
    private ShellPage _activePage = ShellPage.Play;
    private MouseSettings _mouseSettings = MouseSettings.Default;
    private AimPoint _relativeAimCursorPosition;
    private bool _isExitPromptVisible;
    private bool _isRelativeAimInputActive;
    private bool _shouldExit;

    private LoadedAimGamemode _activeGamemode => _workflowState.ActiveGamemode;

    private string _activeDisplayProfileId
    {
        get => _windowState.ActiveDisplayProfileId;
        set => _windowState.ActiveDisplayProfileId = value;
    }

    private AimSessionSettings _activeSettings => _workflowState.ActiveSettings;

    private bool _cursorHidden
    {
        get => _windowState.CursorHidden;
        set => _windowState.CursorHidden = value;
    }

    private LoadedAimGamemode _draftGamemode => _workflowState.DraftGamemode;

    private AimSessionSettings _draftSettings
    {
        get => _workflowState.DraftSettings;
        set => _workflowState.UpdateDraftSettings(value);
    }

    private bool _isBorderlessFullscreen
    {
        get => _windowState.IsBorderlessFullscreen;
        set => _windowState.IsBorderlessFullscreen = value;
    }

    private TrainingPreset? _selectedPreset => _workflowState.SelectedPreset;

    private int _windowedHeight
    {
        get => _windowState.WindowedHeight;
        set => _windowState.WindowedHeight = value;
    }

    private Vector2 _windowedPosition
    {
        get => _windowState.WindowedPosition;
        set => _windowState.WindowedPosition = value;
    }

    private int _windowedWidth
    {
        get => _windowState.WindowedWidth;
        set => _windowState.WindowedWidth = value;
    }

    public TrainerApplication()
    {
        var referenceShellLayout = CreateShellLayout(DefaultWindowWidth, DefaultWindowHeight);
        _logicalArenaViewportWidth = (int)referenceShellLayout.ArenaViewportBounds.Width;
        _logicalArenaViewportHeight = (int)referenceShellLayout.ArenaViewportBounds.Height;
        _gamemodeCatalog = AimGamemodeCatalog.CreateDefault();
        _presetCatalog = TrainingPresetCatalog.Create(_gamemodeCatalog.Gamemodes);
        _workflowState = new TrainerWorkflowState(_gamemodeCatalog, _presetCatalog);
        _appStateStore = new TrainerAppStateStore(GetAppStatePath());
        _runtimeHost = new AimSessionRuntimeHost(
            CreateController(_activeGamemode.Gamemode, _activeSettings),
            _logicalArenaViewportWidth,
            _logicalArenaViewportHeight);
        _frameState = _runtimeHost.Snapshot;
        _historyStore = new JsonSessionHistoryStore(GetHistoryPath());
        _recentSessions = _historyStore.LoadRecent(12).ToList();
        RestoreAppState();
    }

    public void Run()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(DefaultWindowWidth, DefaultWindowHeight, "VrtAimTrainer");
        Raylib.SetExitKey((KeyboardKey)0);
        WindowIconLoader.TrySetWindowIcon();
        CaptureWindowedBounds();
        ApplyDisplayProfile(DisplayProfiles.Single(profile => profile.Id == _activeDisplayProfileId));
        _runtimeHost.Start();

        try
        {
            while (!_shouldExit && !Raylib.WindowShouldClose())
            {
                RunFrame();
            }
        }
        finally
        {
            if (_cursorHidden)
            {
                Raylib.ShowCursor();
            }

            _runtimeHost.Stop();
            Raylib.CloseWindow();
        }
    }

    private static string GetApplicationDataPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VrtAimTrainer");
    }

    private static string GetHistoryPath()
    {
        return Path.Combine(GetApplicationDataPath(), "Sessions");
    }

    private static string GetAppStatePath()
    {
        return Path.Combine(GetApplicationDataPath(), "State", "app-state.json");
    }

    private IAimSessionController CreateController(IAimGamemode gamemode, AimSessionSettings settings)
    {
        return gamemode.CreateController(
            _logicalArenaViewportWidth,
            _logicalArenaViewportHeight,
            settings,
            new SystemRandomSource());
    }

    private void RestoreAppState()
    {
        var appState = _appStateStore.Load();
        if (appState is null || appState.ActiveSettings is null || appState.DraftSettings is null)
        {
            _mouseSettings = MouseSettings.Default;
            _relativeAimCursorPosition = _frameState.CursorPosition;
            _lastPersistedAppState = CreateAppState();
            return;
        }

        _workflowState.Restore(appState);
        _activeDisplayProfileId = DisplayProfiles.Any(profile => profile.Id == appState.ActiveDisplayProfileId)
            ? appState.ActiveDisplayProfileId
            : DisplayProfiles[0].Id;
        _analyticsGamemodeFilterId = ResolveAnalyticsGamemodeFilterId(appState.AnalyticsGamemodeFilterId);
        _mouseSettings = appState.MouseSettings?.Normalize() ?? MouseSettings.Default;
        _runtimeHost.ReplaceController(CreateController(_activeGamemode.Gamemode, _activeSettings));
        _frameState = _runtimeHost.Snapshot;
        _relativeAimCursorPosition = _frameState.CursorPosition;
        _lastPersistedAppState = CreateAppState();
    }

    private TrainerAppState CreateAppState()
    {
        return _workflowState.CreateAppState(
            _activeDisplayProfileId,
            ResolveAnalyticsGamemodeFilterId(_analyticsGamemodeFilterId),
            _mouseSettings);
    }

    private void PersistAppStateIfChanged()
    {
        var appState = CreateAppState();
        if (_lastPersistedAppState == appState)
        {
            return;
        }

        _appStateStore.Save(appState);
        _lastPersistedAppState = appState;
    }

    private void ApplyDraftToRun(bool moveToPlayPage = true)
    {
        _workflowState.ApplyDraftToLive();
        _runtimeHost.ReplaceController(CreateController(_activeGamemode.Gamemode, _activeSettings));
        _lastPersistedSessionId = null;
        if (moveToPlayPage)
        {
            _activePage = ShellPage.Play;
        }
    }

    private void ResetDraftToLive()
    {
        _workflowState.ResetDraftToLive();
    }

    private void ResetDraftToSelectedPreset()
    {
        _workflowState.ResetDraftToSelectedPreset();
        ApplyDraftToRun(false);
    }

    private void ApplyPreset(TrainingPreset preset)
    {
        _workflowState.ApplyPreset(preset);
        ApplyDraftToRun(false);
    }

    private bool HasDraftChanges()
    {
        return _workflowState.HasDraftChanges();
    }

    private bool IsDraftModifiedFromPreset()
    {
        return _workflowState.IsDraftModifiedFromPreset();
    }

    private string GetDraftPresetLabel()
    {
        return _workflowState.GetDraftPresetLabel();
    }

    private void ApplyDraftIfChanged()
    {
        if (!HasDraftChanges())
        {
            return;
        }

        ApplyDraftToRun(false);
    }

    private string ResolveAnalyticsGamemodeFilterId(string? requestedFilterId)
    {
        if (string.Equals(requestedFilterId, AllGamemodesAnalyticsFilterId, StringComparison.OrdinalIgnoreCase))
        {
            return AllGamemodesAnalyticsFilterId;
        }

        if (_gamemodeCatalog.Gamemodes.Any(gamemode => string.Equals(gamemode.Id, requestedFilterId, StringComparison.OrdinalIgnoreCase)))
        {
            return requestedFilterId!;
        }

        return _activeGamemode.Id;
    }

    private IReadOnlyList<SessionResult> GetFilteredAnalyticsSessions()
    {
        var resolvedFilterId = ResolveAnalyticsGamemodeFilterId(_analyticsGamemodeFilterId);
        return string.Equals(resolvedFilterId, AllGamemodesAnalyticsFilterId, StringComparison.OrdinalIgnoreCase)
            ? _recentSessions.ToArray()
            : _recentSessions
                .Where(session => string.Equals(session.GamemodeId, resolvedFilterId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
    }

    private void RunFrame()
    {
        var mousePosition = Raylib.GetMousePosition();
        _ui.BeginFrame(
            mousePosition,
            Raylib.IsMouseButtonPressed(MouseButton.Left),
            Raylib.IsMouseButtonDown(MouseButton.Left),
            Raylib.IsMouseButtonReleased(MouseButton.Left));

        HandleHotkeys();

        var layout = CreateShellLayout(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var arenaTransform = CreateArenaRenderTransform(layout.ArenaViewportBounds);
        var gameplayCursorPosition = ResolveGameplayCursorPosition(mousePosition, arenaTransform);

        _runtimeHost.SetViewport(_logicalArenaViewportWidth, _logicalArenaViewportHeight);
        _runtimeHost.SetCursorPosition(gameplayCursorPosition);
        _frameState = _runtimeHost.Snapshot;
        if (!_isExitPromptVisible)
        {
            HandleSheetStartInput(mousePosition, gameplayCursorPosition, arenaTransform);
        }

        PersistCompletedSession();
        UpdateCursorVisibility(mousePosition, arenaTransform);

        Raylib.BeginDrawing();
        try
        {
            Raylib.ClearBackground(Background);
            if (_isExitPromptVisible)
            {
                _ui.SetInputEnabled(false);
            }

            DrawStatusRail(layout.StatusRailBounds);
            DrawTopTabs(layout.TopTabsBounds);
            DrawArena(layout, arenaTransform);
            DrawNavigationPanel(layout.NavigationPanelBounds, layout.NavigationContentBounds);

            if (_frameState.Phase == SessionPhase.PostGame && _frameState.LastCompletedResult is not null)
            {
                DrawResultsOverlay(layout, _frameState.LastCompletedResult);
            }

            if (_isExitPromptVisible)
            {
                _ui.SetInputEnabled(true);
                DrawExitPrompt(layout.PlayAreaBounds);
            }
        }
        finally
        {
            Raylib.EndDrawing();
        }

        PersistAppStateIfChanged();
    }

    private void HandleSheetStartInput(Vector2 mousePosition, AimPoint localMousePosition, ArenaRenderTransform arenaTransform)
    {
        if (!Raylib.IsMouseButtonPressed(MouseButton.Left) ||
            _frameState.Phase != SessionPhase.PreGame ||
            _frameState.IsStartCountdownActive ||
            !Raylib.CheckCollisionPointRec(mousePosition, arenaTransform.Bounds) ||
            !_frameState.Layout.OuterBounds.Contains(localMousePosition))
        {
            return;
        }

        _runtimeHost.QueueStartCountdown();
    }

}
