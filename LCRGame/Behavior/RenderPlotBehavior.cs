using LCRGame.Framework;
using LCRGame.Models;
using LCRGame.ViewModels;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LCRGame.Behavior;

// The ScottPlot control is not Model View Model friendly.
// This behavior helps to bridge the gap.
public class RenderPlotBehavior : Microsoft.Xaml.Behaviors.Behavior<WpfPlot>
{
    private int ColorIndex { get; set; }

    private List<Color> PlotColors { get; } = new List<Color>()
        { Color.Cyan, Color.DeepSkyBlue, Color.DarkSalmon, Color.Green, Color.MediumBlue };

    protected override void OnAttached()
    {
        if (AssociatedObject.DataContext is MainViewModel mvm)
            mvm.RenderPlot += OnRenderPlot;
        else
            AssociatedObject.Loaded += AssociatedObject_Loaded;
    }

    private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (AssociatedObject.DataContext is MainViewModel mvm)
            mvm.RenderPlot += OnRenderPlot;
    }

    private void OnRenderPlot(GameResult gameResult)
    {
        var gamesX = Enumerable.Range(1, gameResult.Games.Count).Select(item => (double)item).ToArray();
        var turnsY = gameResult.Games.Select(g => (double)g.Turns).ToArray();

        var plot = AssociatedObject.Plot;
        plot.Clear();
        plot.AddScatter(gamesX, turnsY, PlotColors[ColorIndex], 1, 5, MarkerShape.filledCircle, LineStyle.Solid, Constants.Game);
        ColorIndex++;
        if (ColorIndex == 5) ColorIndex = 0;

        plot.XLabel(Constants.XAxisLabel);
        plot.YLabel(Constants.YAxisLabel);

        var max = turnsY.Max();
        var maxItem = turnsY.First(item => Math.Abs(item - max) < 0.0001);
        var maxIndex = turnsY.ToList().IndexOf((int)maxItem);

        var marker = plot.AddMarker(maxIndex, max, MarkerShape.filledCircle, 15, Color.Gold);
        marker.Text = $"{Constants.Longest} {max}";

        var min = turnsY.Min();
        var minItem = turnsY.First(item => Math.Abs(item - min) < 0.0001);
        var minIndex = turnsY.ToList().IndexOf((int)minItem);
        marker = plot.AddMarker(minIndex, min, MarkerShape.filledCircle, 15, Color.Purple);
        marker.Text = $"{Constants.Shortest} {min}";

        var averageY = Enumerable.Repeat(gameResult.AverageGame, gameResult.Games.Count)
            .Select(item => item).ToArray();
        plot.AddScatter(gamesX, averageY, Color.Green, 1, 0, MarkerShape.filledCircle, LineStyle.Solid, Constants.Average);

        var legend = plot.Legend();
        legend.Location = Alignment.UpperRight;

        AssociatedObject.Refresh();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        if (AssociatedObject.DataContext is MainViewModel mvm)
            mvm.RenderPlot -= OnRenderPlot;
    }
}