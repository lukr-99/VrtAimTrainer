using System.Numerics;
using Raylib_cs;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed partial class TrainerApplication
{
    private static ShellLayout CreateShellLayout(int screenWidth, int screenHeight)
    {
        const float outerMargin = 20f;
        const float railGap = 18f;
        const float topOffset = 18f;
        const float bottomMargin = 18f;
        const float tabsHeight = 56f;
        var statusRailWidth = Math.Clamp(screenWidth * 0.19f, 320f, 380f);
        var navigationPanelWidth = Math.Clamp(screenWidth * 0.23f, 380f, 460f);

        var statusRailBounds = new Rectangle(
            outerMargin,
            topOffset,
            statusRailWidth,
            screenHeight - topOffset - bottomMargin);

        var topTabsBounds = new Rectangle(
            statusRailBounds.Right() + railGap,
            topOffset,
            screenWidth - statusRailBounds.Right() - outerMargin - railGap,
            tabsHeight);

        var contentTop = topTabsBounds.Bottom() + railGap;
        var contentHeight = screenHeight - contentTop - bottomMargin;

        var navigationPanelBounds = new Rectangle(
            screenWidth - navigationPanelWidth - outerMargin,
            contentTop,
            navigationPanelWidth,
            contentHeight);

        var playAreaBounds = new Rectangle(
            statusRailBounds.Right() + railGap,
            contentTop,
            navigationPanelBounds.X - statusRailBounds.Right() - (railGap * 2f),
            contentHeight);

        var arenaViewportBounds = new Rectangle(
            playAreaBounds.X + 18f,
            playAreaBounds.Y + 18f,
            playAreaBounds.Width - 36f,
            playAreaBounds.Height - 36f);

        var navigationContentBounds = new Rectangle(
            navigationPanelBounds.X + 16f,
            navigationPanelBounds.Y + 16f,
            navigationPanelBounds.Width - 32f,
            navigationPanelBounds.Height - 32f);

        return new ShellLayout(
            statusRailBounds,
            topTabsBounds,
            playAreaBounds,
            arenaViewportBounds,
            navigationPanelBounds,
            navigationContentBounds);
    }

    private ArenaRenderTransform CreateArenaRenderTransform(Rectangle availableViewportBounds)
    {
        var scale = MathF.Min(
            availableViewportBounds.Width / _logicalArenaViewportWidth,
            availableViewportBounds.Height / _logicalArenaViewportHeight);

        var renderWidth = _logicalArenaViewportWidth * scale;
        var renderHeight = _logicalArenaViewportHeight * scale;
        var bounds = new Rectangle(
            availableViewportBounds.X + ((availableViewportBounds.Width - renderWidth) / 2f),
            availableViewportBounds.Y + ((availableViewportBounds.Height - renderHeight) / 2f),
            renderWidth,
            renderHeight);

        return new ArenaRenderTransform(bounds, scale, _logicalArenaViewportWidth, _logicalArenaViewportHeight);
    }

    private void PersistCompletedSession()
    {
        var result = _frameState.LastCompletedResult;
        if (result is null || _lastPersistedSessionId == result.SessionId)
        {
            return;
        }

        _historyStore.Save(result);
        _lastPersistedSessionId = result.SessionId;
        _recentSessions.Clear();
        _recentSessions.AddRange(_historyStore.LoadRecent(12));
    }

    private void UpdateCursorVisibility(Vector2 mousePosition, ArenaRenderTransform arenaTransform)
    {
        if (_isExitPromptVisible)
        {
            if (_cursorHidden)
            {
                Raylib.ShowCursor();
                _cursorHidden = false;
            }

            return;
        }

        var shouldHideCursor =
            _frameState.Phase != SessionPhase.PostGame &&
            Raylib.CheckCollisionPointRec(mousePosition, arenaTransform.Bounds);

        if (shouldHideCursor == _cursorHidden)
        {
            return;
        }

        if (shouldHideCursor)
        {
            Raylib.HideCursor();
            _cursorHidden = true;
            return;
        }

        Raylib.ShowCursor();
        _cursorHidden = false;
    }

    private void ApplyDisplayProfile(DisplayProfile profile)
    {
        if (profile.IsFullscreen)
        {
            EnterBorderlessFullscreen();
        }
        else
        {
            ExitBorderlessFullscreen();

            if (profile.Width.HasValue && profile.Height.HasValue)
            {
                Raylib.SetWindowSize(profile.Width.Value, profile.Height.Value);
                CenterWindowOnCurrentMonitor(profile.Width.Value, profile.Height.Value);
                CaptureWindowedBounds();
            }
        }

        _activeDisplayProfileId = profile.Id;
    }

    private void CaptureWindowedBounds()
    {
        if (_isBorderlessFullscreen)
        {
            return;
        }

        _windowedWidth = Raylib.GetScreenWidth();
        _windowedHeight = Raylib.GetScreenHeight();
        _windowedPosition = Raylib.GetWindowPosition();
    }

    private void EnterBorderlessFullscreen()
    {
        if (_isBorderlessFullscreen)
        {
            return;
        }

        CaptureWindowedBounds();

        var monitor = Raylib.GetCurrentMonitor();
        var monitorPosition = Raylib.GetMonitorPosition(monitor);
        var monitorWidth = Raylib.GetMonitorWidth(monitor);
        var monitorHeight = Raylib.GetMonitorHeight(monitor);

        Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
        Raylib.SetWindowPosition((int)monitorPosition.X, (int)monitorPosition.Y);
        Raylib.SetWindowSize(monitorWidth, monitorHeight);
        _isBorderlessFullscreen = true;
    }

    private void ExitBorderlessFullscreen()
    {
        if (!_isBorderlessFullscreen)
        {
            return;
        }

        Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
        Raylib.SetWindowSize(_windowedWidth, _windowedHeight);
        Raylib.SetWindowPosition((int)_windowedPosition.X, (int)_windowedPosition.Y);
        _isBorderlessFullscreen = false;
    }

    private void CenterWindowOnCurrentMonitor(int width, int height)
    {
        var monitor = Raylib.GetCurrentMonitor();
        var monitorPosition = Raylib.GetMonitorPosition(monitor);
        var monitorWidth = Raylib.GetMonitorWidth(monitor);
        var monitorHeight = Raylib.GetMonitorHeight(monitor);
        var centeredX = (int)(monitorPosition.X + Math.Max(0f, (monitorWidth - width) / 2f));
        var centeredY = (int)(monitorPosition.Y + Math.Max(0f, (monitorHeight - height) / 2f));
        Raylib.SetWindowPosition(centeredX, centeredY);
    }
}
