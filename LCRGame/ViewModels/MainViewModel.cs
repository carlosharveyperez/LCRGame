using CommunityToolkit.Mvvm.Input;
using LCRGame.Framework;
using LCRGame.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LCRGame.ViewModels;

public class MainViewModel : ValidationBase
{
    public MainViewModel()
    {
        PlayCommand = new AsyncRelayCommand(OnPlay);
        CancelCommand = new RelayCommand(OnCancel);

        Presets.Add(new GameInput(0, 0));
        Presets.Add(new GameInput(3, 100));
        Presets.Add(new GameInput(4, 100));
        Presets.Add(new GameInput(5, 100));
        Presets.Add(new GameInput(5, 1000));
        Presets.Add(new GameInput(5, 10000));
        Presets.Add(new GameInput(5, 100000));
        Presets.Add(new GameInput(6, 100));
        Presets.Add(new GameInput(7, 100));

        SelectedPreset = Presets[1];
    }

    public ICommand PlayCommand { get; }

    public ICommand CancelCommand { get; }

    public event Action<GameResult> RenderPlot;

    public ObservableCollection<GameInput> Presets { get; } = new();

    public ObservableCollection<PlayerResult> PlayerResults { get; } = new();

    private GameInput _selectedPreset;
    public GameInput SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            SetProperty(ref _selectedPreset, value);
            OnPropertyChanged(nameof(IsCustomSettingsSelected));
            OnSelectedPresetChanged();
        }
    }

    private string _players;
    public string Players
    {
        get => _players;
        set => SetProperty(ref _players, value);
    }

    private string _games;
    public string Games
    {
        get => _games;
        set => SetProperty(ref _games, value);
    }

    private bool _isSimulationRunning;
    public bool IsSimulationRunning
    {
        get => _isSimulationRunning;
        set => SetProperty(ref _isSimulationRunning, value);
    }

    public bool IsCustomSettingsSelected => SelectedPreset.ToString() == Constants.CustomSettings;

    private CancellationTokenSource CancellationTokenSource { get; set; }

    protected override string OnValidateProperty(string propertyName)
    {
        if (!IsCustomSettingsSelected) return string.Empty;

        if (propertyName == nameof(Players))
        {
            if (!int.TryParse(Players, out int result))
                return "Invalid Number of Players!";

            if (result < 3)
                return "At least 3 players must play.";

        }
        else if (propertyName == nameof(Games))
        {
            if (!int.TryParse(Games, out int result))
                return "Invalid Number of Games!";

            if (result <= 0)
                return "At least one game must be played.";
        }

        return string.Empty;
    }

    private void OnSelectedPresetChanged()
    {
        if (IsCustomSettingsSelected) return;

        // We are using a preset 
        Players = string.Format($"{SelectedPreset.Players:#,0}");
        Games = string.Format($"{SelectedPreset.Games:0,0}");
    }

    private List<string> ValidateAllProperties()
    {
        var invalidProps = new List<string>();
        var testProp = new List<string> { nameof(Players), nameof(Games) };
        foreach (var prop in testProp)
        {
            var result = OnValidateProperty(prop);
            if (!string.IsNullOrEmpty(result))
                invalidProps.Add(result);
        }

        return invalidProps;
    }

    private async Task OnPlay()
    {
        var invalidProp = ValidateAllProperties();
        if (invalidProp.Any())
        {
            string errors = GetFormattedErrors("The following errors were found:", invalidProp);
            var parentWindow = Application.Current.MainWindow;
            if (parentWindow != null)
                MessageBox.Show(parentWindow, errors, "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var current = IsCustomSettingsSelected ?
            new GameInput(Convert.ToInt32(Players), Convert.ToInt32(Games)) : SelectedPreset;

        IsSimulationRunning = true;
        var sim = new Simulator();
        CancellationTokenSource = new CancellationTokenSource();

        bool firePlotUpdate = true;
        GameResult gr = null;
        try
        {
            gr = await sim.Run(current, CancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            firePlotUpdate = false;
            Debug.WriteLine(e.Message);
        }

        if (firePlotUpdate)
        {
            RenderPlot?.Invoke(gr);

            PlayerResults.Clear();
            for (int i = 0; i < current.Players; i++)
            {
                var pr = new PlayerResult();
                PlayerResults.Add(pr);
                if (gr?.WinnerId == i + 1)
                    pr.Label = $"{i + 1} Winner";
                else
                    pr.Label = $"{i + 1}";
            }
        }

        IsSimulationRunning = false;
    }

    private void OnCancel()
    {
        CancellationTokenSource?.Cancel();
    }
}