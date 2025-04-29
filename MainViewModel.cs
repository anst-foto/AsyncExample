using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AsyncExample.ViewModels;

public class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? _cts;
    private bool _isPaused;
    
    #region Properties
    
    private int _min;
    public int Min
    {
        get => _min;
        set
        {
            var val = Math.Abs(value);
            SetProperty(ref _min, val);
        }
    }
    
    private int _max;
    public int Max
    {
        get => _max;
        set
        {
            var val = Math.Abs(value);
            SetProperty(ref _max, val);
        }
    }
    
    private int _value;
    public int Value
    {
        get => _value;
        set
        {
            var val = Math.Abs(value);
            SetProperty(ref _value, val);
        }
    }
    
    #endregion

    #region Commands

    public ICommand CommandStart { get; }
    public ICommand CommandStop { get; }
    public ICommand CommandPause { get; }

    #endregion

    public MainViewModel()
    {
        Min = 1;
        Max = 10;
        
        CommandStart = new LambdaCommand(async void (_) =>
        {
            try
            {
                _cts = new CancellationTokenSource();
                
                var progress = new Progress<int>();
                progress.ProgressChanged += (_, value) => Value = value;

                if (_isPaused)
                {
                    await StartAsync(Value, Max, progress, _cts.Token);
                }
                else
                {
                    await StartAsync(Min, Max, progress, _cts.Token);
                }

                _isPaused = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        });
        
        CommandStop = new LambdaCommand(async void (_) =>
        {
            try
            {
                await _cts?.CancelAsync();
                _isPaused = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        });
        
        CommandPause = new LambdaCommand(async void (_) =>
        {
            try
            {
                await _cts?.CancelAsync();
                _isPaused = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        });
    }

    #region Methods

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