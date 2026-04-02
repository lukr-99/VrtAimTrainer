using System.Diagnostics;
using VrtAimTrainer.Core.Features.Gameplay;

namespace VrtAimTrainer.Desktop.Features.App;

public sealed class AimSessionRuntimeHost : IDisposable
{
    private const int TickDelayMilliseconds = 4;
    private readonly object _gate = new();
    private IAimSessionController _controller;
    private double? _countdownEndsAtSeconds;
    private bool _countdownRequested;
    private AimPoint _latestCursorPosition;
    private bool _finishRequested;
    private double _pausedAtClockSeconds;
    private bool _pauseRequested;
    private bool _paused;
    private double _totalPausedSeconds;
    private bool _resetRequested;
    private IAimSessionController? _replacementController;
    private AimSessionStateSnapshot _snapshot;
    private bool _stopRequested;
    private Thread? _workerThread;
    private Stopwatch? _clock;
    private int _viewportHeight;
    private int _viewportWidth;

    public AimSessionRuntimeHost(IAimSessionController controller, int viewportWidth, int viewportHeight)
    {
        _controller = controller;
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
        _controller.ResizeViewport(viewportWidth, viewportHeight);
        _snapshot = AimSessionStateSnapshot.From(_controller.State);
    }

    public AimSessionStateSnapshot Snapshot => Volatile.Read(ref _snapshot);

    public void Start()
    {
        if (_workerThread is not null)
        {
            return;
        }

        _clock = Stopwatch.StartNew();
        _workerThread = new Thread(RunLoop)
        {
            IsBackground = true,
            Name = "VrtAimTrainer.SessionRuntime",
        };

        _workerThread.Start();
    }

    public void Stop()
    {
        Thread? workerThread;
        lock (_gate)
        {
            if (_workerThread is null)
            {
                return;
            }

            _stopRequested = true;
            workerThread = _workerThread;
            _workerThread = null;
        }

        workerThread.Join();
    }

    public void SetViewport(int viewportWidth, int viewportHeight)
    {
        lock (_gate)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;

            if (_workerThread is not null)
            {
                return;
            }

            _controller.ResizeViewport(viewportWidth, viewportHeight);
            PublishSnapshot();
        }
    }

    public void SetCursorPosition(AimPoint cursorPosition)
    {
        lock (_gate)
        {
            _latestCursorPosition = cursorPosition;

            if (_workerThread is null)
            {
                PublishSnapshot();
            }
        }
    }

    public void ReplaceController(IAimSessionController controller)
    {
        lock (_gate)
        {
            _countdownEndsAtSeconds = null;
            _countdownRequested = false;
            if (_workerThread is null)
            {
                _controller = controller;
                _controller.ResizeViewport(_viewportWidth, _viewportHeight);
                PublishSnapshot();
                return;
            }

            _replacementController = controller;
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            _countdownEndsAtSeconds = null;
            _countdownRequested = false;
            if (_workerThread is null)
            {
                _controller.Reset();
                PublishSnapshot();
                return;
            }

            _resetRequested = true;
        }
    }

    public void Finish()
    {
        lock (_gate)
        {
            _countdownEndsAtSeconds = null;
            _countdownRequested = false;
            if (_workerThread is null)
            {
                _controller.Finish(0d);
                PublishSnapshot();
                return;
            }

            _finishRequested = true;
        }
    }

    public void SetPaused(bool paused)
    {
        lock (_gate)
        {
            _pauseRequested = paused;
            if (_workerThread is not null)
            {
                return;
            }

            _paused = paused;
        }
    }

    public void QueueStartCountdown()
    {
        lock (_gate)
        {
            if (_controller.State.Phase != SessionPhase.PreGame || _countdownEndsAtSeconds is not null)
            {
                return;
            }

            if (_workerThread is null)
            {
                _countdownEndsAtSeconds = 3d;
                PublishSnapshot();
                return;
            }

            _countdownRequested = true;
        }
    }

    public void Dispose()
    {
        Stop();
    }

    private void RunLoop()
    {
        while (true)
        {
            var clockSeconds = _clock?.Elapsed.TotalSeconds ?? 0d;
            IAimSessionController controller;
            IAimSessionController? replacementController;
            AimPoint cursorPosition;
            double? countdownEndsAtSeconds;
            var paused = false;
            var resetRequested = false;
            var finishRequested = false;
            var viewportWidth = 0;
            var viewportHeight = 0;

            lock (_gate)
            {
                if (_stopRequested)
                {
                    return;
                }

                replacementController = _replacementController;
                _replacementController = null;
                resetRequested = _resetRequested;
                _resetRequested = false;
                finishRequested = _finishRequested;
                _finishRequested = false;
                cursorPosition = _latestCursorPosition;
                viewportWidth = _viewportWidth;
                viewportHeight = _viewportHeight;
                controller = replacementController ?? _controller;

                if (replacementController is not null)
                {
                    _controller = replacementController;
                }

                if (_pauseRequested != _paused)
                {
                    if (_pauseRequested)
                    {
                        _paused = true;
                        _pausedAtClockSeconds = clockSeconds;
                    }
                    else
                    {
                        _totalPausedSeconds += Math.Max(0d, clockSeconds - _pausedAtClockSeconds);
                        _pausedAtClockSeconds = 0d;
                        _paused = false;
                    }
                }

                paused = _paused;

                if (_countdownRequested &&
                    _countdownEndsAtSeconds is null &&
                    controller.State.Phase == SessionPhase.PreGame)
                {
                    _countdownEndsAtSeconds = GetTimelineSeconds(clockSeconds) + 3d;
                }

                _countdownRequested = false;
                countdownEndsAtSeconds = _countdownEndsAtSeconds;
            }

            var timestampSeconds = GetTimelineSeconds(clockSeconds);

            controller.ResizeViewport(viewportWidth, viewportHeight);
            if (paused)
            {
                PublishSnapshot(timestampSeconds, countdownEndsAtSeconds);
                Thread.Sleep(TickDelayMilliseconds);
                continue;
            }

            if (resetRequested)
            {
                controller.Reset();
            }

            if (finishRequested)
            {
                controller.Finish(timestampSeconds);
            }

            var startedThisTick = false;
            if (countdownEndsAtSeconds is double countdownEndSeconds &&
                controller.State.Phase == SessionPhase.PreGame &&
                timestampSeconds >= countdownEndSeconds)
            {
                startedThisTick = controller.Start(timestampSeconds);
                countdownEndsAtSeconds = null;

                lock (_gate)
                {
                    _countdownEndsAtSeconds = null;
                }
            }

            if (!startedThisTick)
            {
                controller.Update(new AimFrameInput(timestampSeconds, cursorPosition));
            }

            PublishSnapshot(timestampSeconds, countdownEndsAtSeconds);
            Thread.Sleep(TickDelayMilliseconds);
        }
    }

    private void PublishSnapshot()
    {
        Volatile.Write(ref _snapshot, AimSessionStateSnapshot.From(_controller.State));
    }

    private void PublishSnapshot(double timestampSeconds, double? countdownEndsAtSeconds)
    {
        var snapshot = AimSessionStateSnapshot.From(_controller.State);
        if (countdownEndsAtSeconds is double countdownEndSeconds && snapshot.Phase == SessionPhase.PreGame)
        {
            var remainingSeconds = Math.Max(0d, countdownEndSeconds - timestampSeconds);
            if (remainingSeconds > 0d)
            {
                snapshot = snapshot with
                {
                    IsStartCountdownActive = true,
                    StartCountdownValue = Math.Clamp((int)Math.Ceiling(remainingSeconds), 1, 3),
                    StartCountdownRemainingSeconds = remainingSeconds,
                };
            }
        }

        Volatile.Write(ref _snapshot, snapshot);
    }

    private double GetTimelineSeconds(double clockSeconds)
    {
        return _paused
            ? Math.Max(0d, _pausedAtClockSeconds - _totalPausedSeconds)
            : Math.Max(0d, clockSeconds - _totalPausedSeconds);
    }
}
