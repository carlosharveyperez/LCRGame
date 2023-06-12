﻿using LCRGame.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LCRGame.Framework;

public class Simulator
{
    private int MaxDice { get; } = 3;
    private bool IsDebugging { get; } = true;

    public async Task<GameResult> Run(GameInput gi, CancellationToken ct)
    {
        return await Task.Run(() => RunCore(gi, ct), ct);
    }

    private GameResult RunCore(GameInput gi, CancellationToken ct)
    {
        var games = new List<Game>();
        for (int i = 1; i <= gi.Games; i++)
        {
            var (game, players) = CreateModels(i, gi, games);
            CreateRelationships(players);

            if (IsDebugging)
                Debug.WriteLine($"Playing Game:{i}, Players:{gi.Players}");
            PlayGame(players, game, ct);
        }

        // Create Game results
        var gr = new GameResult
        {
            ShortestGame = games.Select(g => g.Turns).Min(),
            LongestGame = games.Select(g => g.Turns).Max(),
            AverageGame = games.Select(g => g.Turns).Average()
        };

        for (int i = 0; i < games.Count; i++)
        {
            Debug.WriteLine($"Winner for Game:{i}, Winner:{games[i].WinnerId}");
        }

        // Calculate winner
        gr.WinnerId = MaxRepeating(games);
        gr.GamesWonByWinner = games.Count(g => g.WinnerId == gr.WinnerId);
        gr.WinningPercentage = gr.GamesWonByWinner / (double)games.Count * 100;

        return gr;
    }

    private int MaxRepeating(List<Game> games)
    {
        var winners = games.Select(g => g.WinnerId).ToList();
        var count = winners.Count;
        for (int i = 0; i < count; i++)
            winners[winners[i] % count] += count;

        // Find index of the maximum repeating element
        int max = winners[0], result = 0;
        for (int i = 1; i < count; i++)
        {
            if (winners[i] > max)
            {
                max = winners[i];
                result = i;
            }
        }

        return result;
    }

    private (Game, List<Player>) CreateModels(int id, GameInput gi, List<Game> games)
    {
        // Create current game
        var game = new Game { Id = id };
        games.Add(game);

        // Create all players
        var players = new List<Player>();
        for (int j = 1; j <= gi.Players; j++)
        {
            var player = new Player(j, MaxDice);
            players.Add(player);
        }

        return (game, players);
    }

    private static void CreateRelationships(List<Player> players)
    {
        // Create Left, Right relationships
        int counter = 0;
        foreach (var player in players)
        {
            if (counter == 0)
            {
                player.LeftPlayer = players[^1];
                player.RightPlayer = players[counter + 1];
            }
            else if (counter < players.Count - 1)
            {
                player.LeftPlayer = players[counter - 1];
                player.RightPlayer = players[counter + 1];
            }
            else
            {
                player.LeftPlayer = players[counter - 1];
                player.RightPlayer = players[0];
            }
            counter++;
        }
    }

    private void PlayGame(List<Player> players, Game game, CancellationToken ct)
    {
        // Play until only one player has chips
        var playerWithChips = new List<Player>();
        playerWithChips.AddRange(players);

        while (playerWithChips.Count != 1)
        {
            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();

            for (int l = 0; l < players.Count; l++)
            {
                var player = players[l];
                if (player.Chips > 0)
                {
                    RollDice(player, game);
                    game.Turns++;
                }

                if (player.Chips == 0) playerWithChips.Remove(player);

                if (playerWithChips.Count == 1)
                {
                    game.WinnerId = playerWithChips.First().Id;
                    break;
                }
            }
        }
    }

    private void RollDice(Player player, Game game)
    {
        int dice = Math.Min(player.Chips, MaxDice);
        for (int i = 0; i < dice; i++)
        {
            var r = new Random();
            int result = r.Next(1, 7);
            if (result == 1)
            {
                // Left result
                player.LeftPlayer.Chips++;
                player.Chips--;
            }
            else if (result == 3)
            {
                // Center result
                game.Chips++;
                player.Chips--;
            }
            else if (result == 5)
            {
                // Right result
                player.RightPlayer.Chips++;
                player.Chips--;
            }
        }
    }
}