using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AsyncExample.ViewModels;

public class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? _cts;
    
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
                await StartAsync(progress, _cts.Token);
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
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        });
    }

    #region Methods

    private async Task StartAsync(IProgress<int>? progress = null, CancellationToken token = default)
    {
        for (var i = Min; i <= Max; i++)
        {
            token.ThrowIfCancellationRequested();
            
            progress?.Report(i);
            await Task.Delay(1000, token);
        }
    }

    #endregion
}