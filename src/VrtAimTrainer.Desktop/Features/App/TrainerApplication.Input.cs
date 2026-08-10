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

    /// <summary>
    /// Updates the OS cursor presentation and resolves the logical gameplay cursor for this frame.
    /// During a live run the cursor is locked and aiming is driven by raw, unaccelerated mouse
    /// deltas; elsewhere the cursor maps absolutely to the arena so menus stay usable.
    /// </summary>
    private AimPoint UpdateGameplayInput(Vector2 mousePosition, ArenaRenderTransform arenaTransform)
    {
        ApplyCursorMode(ResolveDesiredCursorMode(mousePosition, arenaTransform), arenaTransform);

        if (_cursorMode == GameplayCursorMode.Locked)
        {
            var (rawX, rawY) = ReadRelativeAimDelta();
            _relativeAimCursorPosition = _mouseSettings.Mode == AimResponseMode.Projected
                ? ApplyProjectedAim(rawX, rawY, _frameState.Layout.OuterBounds)
                : ApplyLinearAim(rawX, rawY, _frameState.Layout.OuterBounds);

            return _relativeAimCursorPosition;
        }

        _relativeAimCursorPosition = arenaTransform.ToLogicalPoint(mousePosition).Clamp(_frameState.Layout.OuterBounds);
        return _relativeAimCursorPosition;
    }

    private AimPoint ApplyLinearAim(int rawDeltaX, int rawDeltaY, AimRectangle bounds)
    {
        // Reset the projected angle so a later switch to Projected re-seeds from the live position.
        _aimAnglesValid = false;
        return RelativeAimMath.ApplyRawDelta(
            _relativeAimCursorPosition,
            rawDeltaX,
            rawDeltaY,
            _mouseSettings.EffectiveHorizontalSensitivity,
            _mouseSettings.EffectiveVerticalSensitivity,
            bounds);
    }

    private AimPoint ApplyProjectedAim(int rawDeltaX, int rawDeltaY, AimRectangle bounds)
    {
        var horizontalFov = _mouseSettings.HorizontalFovDegrees;
        var verticalFov = ProjectedAimMath.VerticalFovDegrees(
            horizontalFov,
            _mouseSettings.AspectWidth,
            _mouseSettings.AspectHeight);

        if (!_aimAnglesValid)
        {
            (_aimYawDegrees, _aimPitchDegrees) = ProjectedAimMath.PointToAngles(
                _relativeAimCursorPosition,
                horizontalFov,
                verticalFov,
                bounds);
            _aimAnglesValid = true;
        }

        var degreesPerCount = ProjectedAimMath.DegreesPerCount(
            _mouseSettings.CentimetersPer360,
            _mouseSettings.MouseDpi);

        var maxYaw = ProjectedAimMath.MaxYawDegrees(horizontalFov);
        var maxPitch = ProjectedAimMath.MaxPitchDegrees(verticalFov);

        _aimYawDegrees = Math.Clamp(
            _aimYawDegrees + (rawDeltaX * degreesPerCount * _mouseSettings.HorizontalScale),
            -maxYaw,
            maxYaw);
        _aimPitchDegrees = Math.Clamp(
            _aimPitchDegrees + (rawDeltaY * degreesPerCount * _mouseSettings.VerticalScale * _mouseSettings.VerticalSign),
            -maxPitch,
            maxPitch);

        return ProjectedAimMath.AnglesToPoint(
            _aimYawDegrees,
            _aimPitchDegrees,
            horizontalFov,
            verticalFov,
            bounds);
    }

    private GameplayCursorMode ResolveDesiredCursorMode(Vector2 mousePosition, ArenaRenderTransform arenaTransform)
    {
        if (_isExitPromptVisible)
        {
            return GameplayCursorMode.Free;
        }

        if (_frameState.Phase == SessionPhase.InGame)
        {
            return GameplayCursorMode.Locked;
        }

        var overArena = _frameState.Phase != SessionPhase.PostGame &&
                        Raylib.CheckCollisionPointRec(mousePosition, arenaTransform.Bounds);
        return overArena ? GameplayCursorMode.HiddenFree : GameplayCursorMode.Free;
    }

    private void ApplyCursorMode(GameplayCursorMode desiredMode, ArenaRenderTransform arenaTransform)
    {
        if (desiredMode == _cursorMode)
        {
            return;
        }

        // Leaving the locked aim: unlock the OS cursor and drop it where the crosshair ended up.
        if (_cursorMode == GameplayCursorMode.Locked)
        {
            Raylib.EnableCursor();
            var screenPosition = arenaTransform.ToScreenPoint(_relativeAimCursorPosition);
            Raylib.SetMousePosition((int)screenPosition.X, (int)screenPosition.Y);
        }

        switch (desiredMode)
        {
            case GameplayCursorMode.Locked:
                _relativeAimCursorPosition = _frameState.CursorPosition.Clamp(_frameState.Layout.OuterBounds);
                _aimAnglesValid = false; // re-seed projected yaw/pitch from the live position on run start
                Raylib.DisableCursor();
                _rawMouseInput.EnsureRegistered();
                _rawMouseInput.DrainAndDiscard();
                break;

            case GameplayCursorMode.HiddenFree:
                Raylib.EnableCursor();
                Raylib.HideCursor();
                break;

            case GameplayCursorMode.Free:
                Raylib.EnableCursor();
                break;
        }

        _cursorMode = desiredMode;
    }

    private (int X, int Y) ReadRelativeAimDelta()
    {
        if (_rawMouseInput.IsAvailable)
        {
            return _rawMouseInput.Drain();
        }

        // Fallback when raw input could not be registered: raylib's delta (may include OS acceleration).
        var delta = Raylib.GetMouseDelta();
        return ((int)MathF.Round(delta.X), (int)MathF.Round(delta.Y));
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
}
