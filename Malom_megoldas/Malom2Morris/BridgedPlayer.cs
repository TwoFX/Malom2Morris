/*
 * BridgedPlayer.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the GNU General Public License, Version 3, or, at your option, any later version
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Morris;

namespace Malom2Morris
{
	internal static class Init
	{
		// Dieser Konstruktor wird garantiert exakt einmal aufgerufen, und zwar bevor Do() aufgerufen wird
		static Init()
		{ 
			Malom3.Rules.Main = new Malom3.FrmMain();
			Malom3.Rules.DoNotPlay = true;
			Malom3.Rules.InitRules();
			Malom3.Rules.SetVariant(Malom3.Rules.RuleVariant.Standard);
		}

		public static void Do()
		{
		}
	}

	/// <summary>
	/// Stellt einen <see cref="Malom3.Player"/> durch das Interface <see cref="Morris.IMoveProvider"/> bereit. 
	/// </summary>
	/// <typeparam name="T">Der konkrete Subtyp von <see cref="Malom3.Player"/></typeparam>
    public abstract class BridgedPlayer<T> : IMoveProvider where T : Malom3.Player, new()
    {
		// Anmerkung für den geneigten Leser dieser Klasse: Man sieht es diesem Code eventuell
		// nicht an, aber diese Klasse ist das Resultat vieler Stunden des Brütens über der
		// Funktionsweise des Malom-Quellcodes mit Trace-Tablle und Google Übersetzer für die
		// Kommentare. Die Entwicklung dieser Klasse ist außerdem Schauplatz einer sehr
		// involvierten Bugsuche, die mich für Stunden in die Tiefen von hash.cpp herablockte.
		// Später stellte sich heraus, dass die vermeintliche Speicherkorruption im C++-Code
		// in Wirklichkeit ein Copy/Paste-Fehler in dieser Klasse war.

		// Es soll nicht möglich sein, direkt Instanzen von BridgedPlayer zu erstellen
		protected BridgedPlayer()
		{
			Init.Do();
			player = new T();
		}

		private T player;

		private static Dictionary<Occupation, int> occupationConversion = new Dictionary<Occupation, int>()
		{
			[Occupation.Free] = -1,
			[Occupation.White] = 0,
			[Occupation.Black] = 1
		};

		private static Dictionary<Player, int> playerConversion = new Dictionary<Player, int>()
		{
			[Player.White] = 0,
			[Player.Black] = 1
		};

		// Malom indiziert Felder auch von 0-23, aber nach einem ganz anderen Schema
		// Daher brauchen wir diese Lookup Table
		private static Dictionary<int, int> coordinateConversion = new int[] { 1, 2, 3, 9, 10, 11, 17, 18, 19, 0, 8, 16, 20, 12, 4, 23, 22, 21, 15, 14, 13, 7, 6, 5 }
			.Select((value, index) => new { value, index }).ToDictionary(a => a.index, a => a.value);
		private static Dictionary<int, int> reverseCoordinateConversion = reverse(coordinateConversion);

		private static Dictionary<T2, T1> reverse<T1, T2>(Dictionary<T1, T2> old)
		{
			return old.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
		}

		public GameMove GetNextMove(IReadOnlyGameState state)
		{
			Malom3.GameState otherState = new Malom3.GameState();

			// Alle Informationen in den anderen Zustand übertragen
			otherState.block = false;
			otherState.KLE = false;
			otherState.phase = state.GetPhase(state.NextToMove) == Phase.Placing ? 1 : 2;
			otherState.over = false;
			otherState.LastKLE = 0;
			otherState.SideToMove = playerConversion[state.NextToMove];
			otherState.T = Enumerable.Range(0, GameState.FIELD_SIZE).Select(x => occupationConversion[state.Board[reverseCoordinateConversion[x]]]).ToArray();
			otherState.SetStoneCount = new[] { state.GetStonesPlaced(Player.White), state.GetStonesPlaced(Player.Black) };
			otherState.StoneCount = new[] { state.GetCurrentStones(Player.White), state.GetCurrentStones(Player.Black) };
			// otherState.MoveCount wird von keiner für uns interessanten KI verwendet
			// otherState.block und otherState.winner geben lediglich Informationen nach Spielende a

			Malom3.Move move = player.ToMove(otherState);
			GameMove convertedMove;

			// In Malom gibt die KI immer zunächst einen Zug zurück, der keine Information zum Schlagen enthält.
			// Wenn wir diese benötigen, müssen wir einen weiteren Zug anfordern.

			if (move is Malom3.MoveKorong)
			{
				var mo = move as Malom3.MoveKorong;
				convertedMove = mo == null ? null :GameMove.Move(reverseCoordinateConversion[mo.hon], reverseCoordinateConversion[mo.hov]);
			}
			else
			{
				var mo = move as Malom3.SetKorong;
				convertedMove = mo == null ? null : GameMove.Place(reverseCoordinateConversion[mo.hov]);
			}

			if (state.IsValidMove(convertedMove) == MoveValidity.ClosesMill)
			{
				// Keine Ahnung für das KLE steht, aber es bedeutet, dass der Spieler einen Zug zurückgeben soll, der einen Stein entfernt
				// Oder, in Malom-Terminologie, einen LeveszKorong
				otherState.KLE = true;
				Malom3.LeveszKorong mo = player.ToMove(otherState) as Malom3.LeveszKorong;
				convertedMove = mo == null ? null : convertedMove.WithRemove(reverseCoordinateConversion[mo.hon]);
			}
			Console.WriteLine(convertedMove.ToString());
			return convertedMove;
		}
    }

	// Die tatsächlichen Spielerklassen sind dann lediglich generische Spezialisierungen der BridgedPlayer-Klasse
	// mit den entsprechenden Subtypen von Maom3.Player

	[SelectorName("Malom3: Perfekter Spieler")]
	public class PerfectPlayer : BridgedPlayer<Malom3.PerfectPlayer>
	{
	}

	[SelectorName("Malom3: Heuristische KI")]
	public class MalomAlphaBeta : BridgedPlayer<Malom3.ComputerPlayer>
	{
	}
}
