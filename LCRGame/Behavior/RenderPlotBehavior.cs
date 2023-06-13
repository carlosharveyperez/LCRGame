using LCRGame.Models;
using LCRGame.ViewModels;
using ScottPlot;
using System;
using System.Drawing;
using System.Linq;

namespace LCRGame.Behavior;

public class RenderPlotBehavior : Microsoft.Xaml.Behaviors.Behavior<WpfPlot>
{
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
        plot.AddScatter(gamesX, turnsY, Color.Aquamarine);
        plot.XLabel("Games");
        plot.YLabel("Turns");

        var max = turnsY.Max();
        var maxItem = turnsY.First(item => Math.Abs(item - max) < 0.0001);
        var maxIndex = turnsY.ToList().IndexOf((int)maxItem);

        var marker = plot.AddMarker(maxIndex, max, MarkerShape.filledCircle, 15, Color.Gold, "Longest");
        marker.Text = "Longest";

        var min = turnsY.Min();
        var minItem = turnsY.First(item => Math.Abs(item - min) < 0.0001);
        var minIndex = turnsY.ToList().IndexOf((int)minItem);
        marker = plot.AddMarker(minIndex, min, MarkerShape.filledCircle, 15, Color.Purple, "Shortest");
        marker.Text = "Shortest";

        AssociatedObject.Refresh();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        if (AssociatedObject.DataContext is MainViewModel mvm)
            mvm.RenderPlot -= OnRenderPlot;
    }
}