namespace VrtAimTrainer.Desktop.Features.App;

/// <summary>How the OS cursor is presented while the trainer is running.</summary>
public enum GameplayCursorMode
{
    /// <summary>Normal, visible OS cursor (menus, results, quit prompt).</summary>
    Free,

    /// <summary>Cursor is free to move but hidden — used while hovering the sheet before a run starts.</summary>
    HiddenFree,

    /// <summary>Cursor is locked and hidden; aiming is driven by raw relative motion during a live run.</summary>
    Locked,
}
