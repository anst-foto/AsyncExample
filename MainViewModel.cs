using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;

namespace AsyncExample.ViewModels;

public class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? _cts;
    private readonly Progress<int> _progress;
    
    private bool _isPaused;
    
    #region Properties
    
    private int _min;
    public int Min
    {
        get => _min;
        set
        {
            var val = Math.Abs(value);
            this.RaiseAndSetIfChanged(ref _min, val);
        }
    }
    
    private int _max;
    public int Max
    {
        get => _max;
        set
        {
            var val = Math.Abs(value);
            this.RaiseAndSetIfChanged(ref _max, val);
        }
    }
    
    private int _value;
    public int Value
    {
        get => _value;
        set
        {
            var val = Math.Abs(value);
            this.RaiseAndSetIfChanged(ref _value, val);
        }
    }
    
    private bool _isRunning;
    private bool IsRunning
    {
        get => _isRunning;
        set => this.RaiseAndSetIfChanged(ref _isRunning, value);
    }
    
    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> CommandStart { get; }
    public ReactiveCommand<Unit, Unit> CommandStop { get; }
    public ReactiveCommand<Unit, Unit> CommandPause { get; }

    #endregion

    public MainViewModel()
    {
        Min = 1;
        Max = 10;

        Value = Min;
        
        _progress = new Progress<int>();
        _progress.ProgressChanged += (_, value) => Value = value;

        var canExecuteStart = this.WhenAnyValue(
            vm => vm.Min,
            vm => vm.Max,
            vm => vm.IsRunning,
            (min, max, isRunning) => min < max && !isRunning );
        var canExecuteStopAndPause = this.WhenAnyValue(vm => vm.IsRunning);
        
        CommandStart = ReactiveCommand.CreateFromTask(Start, canExecuteStart);
        CommandStop = ReactiveCommand.CreateFromTask(Stop, canExecuteStopAndPause);
        CommandPause = ReactiveCommand.CreateFromTask(Pause, canExecuteStopAndPause);
    }

    #region Methods

    private async Task Start()
    {
        try
        {
            IsRunning = true;
            
            _cts = new CancellationTokenSource();
                
            if (_isPaused)
            {
                await StartAsync(Value, Max, _progress, _cts.Token);
            }
            else
            {
                await StartAsync(Min, Max, _progress, _cts.Token);
            }
            
            _isPaused = false;
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

    private async Task Stop()
    {
        try
        {
            _isPaused = false;
            IsRunning = !IsRunning;
            
            await _cts!.CancelAsync();
        }
        catch (Exception e)
        {
            // ignore
        }
        finally
        {
            _cts!.Dispose();
        }
    }

    private async Task Pause()
    {
        try
        {
            _isPaused = true;
            IsRunning = !IsRunning;
            
            await _cts!.CancelAsync();
        }
        catch (Exception e)
        {
            // ignore
        }
        finally
        {
            _cts!.Dispose();
        }
    }
    
    private async Task StartAsync(int begin, int end, IProgress<int>? progress = null, CancellationToken token = default)
    {
        for (var i = begin; i <= end; i++)
        {
            token.ThrowIfCancellationRequested();
            
            progress?.Report(i);
            await Task.Delay(1000, token);
        }
    }

    #endregion
}