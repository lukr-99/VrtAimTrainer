using System.Numerics;
using Raylib_cs;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed partial class TrainerApplication
{
    private void HandleHotkeys()
    {
        if (_isExitPromptVisible)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                DismissExitPrompt();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter))
            {
                ConfirmExit();
            }

            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            ShowExitPrompt();
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Tab))
        {
            _activePage = (ShellPage)(((int)_activePage + 1) % Enum.GetValues<ShellPage>().Length);
        }

        if (Raylib.IsKeyPressed(KeyboardKey.F1))
        {
            _activePage = ShellPage.Analytics;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.F11))
        {
            ApplyDisplayProfile(
                _activeDisplayProfileId == "fullscreen"
                    ? DisplayProfiles.Single(profile => profile.Id == "1080p")
                    : DisplayProfiles.Single(profile => profile.IsFullscreen));
        }

        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            _runtimeHost.Reset();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.E))
        {
            _runtimeHost.Finish();
        }
    }

    private AimPoint ResolveGameplayCursorPosition(Vector2 mousePosition, ArenaRenderTransform arenaTransform)
    {
        return _isExitPromptVisible || _frameState.Phase != SessionPhase.InGame
            ? ResolveAbsoluteGameplayCursorPosition(mousePosition, arenaTransform)
            : ResolveRelativeGameplayCursorPosition(mousePosition, arenaTransform);
    }

    private AimPoint ResolveAbsoluteGameplayCursorPosition(Vector2 mousePosition, ArenaRenderTransform arenaTransform)
    {
        if (_isRelativeAimInputActive)
        {
            ReleaseRelativeAimInput(arenaTransform);
        }

        _relativeAimCursorPosition = arenaTransform.ToLogicalPoint(mousePosition).Clamp(_frameState.Layout.OuterBounds);
        return _relativeAimCursorPosition;
    }

    private AimPoint ResolveRelativeGameplayCursorPosition(Vector2 mousePosition, ArenaRenderTransform arenaTransform)
    {
        var anchor = GetArenaInputAnchor(arenaTransform);
        if (!_isRelativeAimInputActive)
        {
            _isRelativeAimInputActive = true;
            _relativeAimCursorPosition = _frameState.CursorPosition.Clamp(_frameState.Layout.OuterBounds);
            Raylib.SetMousePosition((int)anchor.X, (int)anchor.Y);
            return _relativeAimCursorPosition;
        }

        var mouseDelta = mousePosition - anchor;
        if (mouseDelta.LengthSquared() > 0f)
        {
            var logicalDelta = new Vector2(
                mouseDelta.X * _mouseSettings.EffectiveHorizontalSensitivity / arenaTransform.Scale,
                mouseDelta.Y * _mouseSettings.EffectiveVerticalSensitivity / arenaTransform.Scale);

            _relativeAimCursorPosition = _relativeAimCursorPosition
                .Translate(logicalDelta.X, logicalDelta.Y)
                .Clamp(_frameState.Layout.OuterBounds);
        }

        Raylib.SetMousePosition((int)anchor.X, (int)anchor.Y);
        return _relativeAimCursorPosition;
    }

    private void ReleaseRelativeAimInput(ArenaRenderTransform arenaTransform)
    {
        if (!_isRelativeAimInputActive)
        {
            return;
        }

        var screenPosition = arenaTransform.ToScreenPoint(_relativeAimCursorPosition);
        Raylib.SetMousePosition((int)screenPosition.X, (int)screenPosition.Y);
        _isRelativeAimInputActive = false;
    }

    private void ShowExitPrompt()
    {
        if (_isExitPromptVisible)
        {
            return;
        }

        _isExitPromptVisible = true;
        _runtimeHost.SetPaused(true);
    }

    private void DismissExitPrompt()
    {
        if (!_isExitPromptVisible)
        {
            return;
        }

        _isExitPromptVisible = false;
        _runtimeHost.SetPaused(false);
    }

    private void ConfirmExit()
    {
        _shouldExit = true;
    }

    private static Vector2 GetArenaInputAnchor(ArenaRenderTransform arenaTransform)
    {
        return new Vector2(
            arenaTransform.Bounds.X + (arenaTransform.Bounds.Width / 2f),
            arenaTransform.Bounds.Y + (arenaTransform.Bounds.Height / 2f));
    }
}
