using Raylib_cs;

namespace VrtAimTrainer.Desktop.Features.App;

public readonly record struct ShellLayout(
    Rectangle StatusRailBounds,
    Rectangle TopTabsBounds,
    Rectangle PlayAreaBounds,
    Rectangle ArenaViewportBounds,
    Rectangle NavigationPanelBounds,
    Rectangle NavigationContentBounds);
