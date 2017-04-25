// This is where you build your AI for the Chess game.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Joueur.cs.Games.Chess
{
    /// <summary>
    /// This is where you build your AI for the Chess game.
    /// </summary>
    class AI : BaseAI
    {
        #region Properties
        #pragma warning disable 0169 // the never assigned warnings between here are incorrect. We set it for you via reflection. So these will remove it from the Error List.
        #pragma warning disable 0649

        /// <summary>
        /// This is the Game object itself, it contains all the information about the current game
        /// </summary>
        public readonly Chess.Game Game;

        /// <summary>
        /// This is your AI's player. This AI class is not a player, but it should command this Player.
        /// </summary>
        public readonly Chess.Player Player;

        /// <summary>
        /// The Chess AI Engine, implements a fen constructor, OpponentMove and MakeMove
        /// </summary>
        private ChessEngine engine;

        #pragma warning restore 0169
        #pragma warning restore 0649
        #endregion


        #region Methods
        /// <summary>
        /// This returns your AI's name to the game server. Just replace the string.
        /// </summary>
        /// <returns>string of you AI's name.</returns>
        public override string GetName()
        {
            return "Waffle";
        }

        /// <summary>
        /// This is automatically called when the game first starts, once the Game object and all GameObjects have been initialized, but before any players do anything.
        /// </summary>
        /// <remarks>
        /// This is a good place to initialize any variables you add to your AI, or start tracking game objects.
        /// </remarks>
        public override void Start()
        {
            base.Start();
            this.engine = new ChessEngine(this.Game.Fen, this.Player.Color == "White");
            if (this.Player.Color != "White")
            {
                this.engine.Ponder();
            }
        }

        /// <summary>
        /// This is automatically called every time the game (or anything in it) updates.
        /// </summary>
        /// <remarks>
        /// If a function you call triggers an update this will be called before that function returns.
        /// </remarks>
        public override void GameUpdated()
        {
            base.GameUpdated();
        }

        /// <summary>
        /// This is automatically called when the game ends.
        /// </summary>
        /// <remarks>
        /// You can do any cleanup of you AI here, or do custom logging. After this function returns the application will close.
        /// </remarks>
        /// <param name="won">true if your player won, false otherwise</param>
        /// <param name="reason">a string explaining why you won or lost</param>
        public override void Ended(bool won, string reason)
        {
            base.Ended(won, reason);
            this.engine.StopPonder();
        }


        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn.
        /// True means end your turn, False means to keep your turn 
        /// going and re-call this function.</returns>
        public bool RunTurn()
        {
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Time Remaining: " + this.Player.TimeRemaining + " ns");
            var timer = new Stopwatch();
            timer.Start();
            this.engine.StopPonder();
            timer.Stop();
            Console.WriteLine("Time to stop pondering: " + timer.ElapsedMilliseconds + " ms");
            if (this.Game.Moves.Count > 0)
            {
                var lastMove = this.Game.Moves.Last();
                string fromFR = lastMove.FromFile + lastMove.FromRank;
                string toFR = lastMove.ToFile + lastMove.ToRank;
                string promote = lastMove.Promotion;
                Console.WriteLine("Opponents Last Move: " + fromFR + "    " + toFR);
                if (promote == "")
                {
                    promote = "None";
                }
                this.engine.OpponentMove(fromFR, toFR, promote);
            }
            this.engine.Print();
            var moveStrings = this.engine.MakeMove();
            Console.WriteLine("Move: " + moveStrings.Item1 + "    " + moveStrings.Item2);
            var piece = this.Player.Pieces.First(p => (p.File + p.Rank) == moveStrings.Item1);
            piece.Move( moveStrings.Item2[0].ToString(),
                        (int)(moveStrings.Item2[1] - '0'),
                        moveStrings.Item3.Replace("None", "")
            );

            this.engine.Ponder();
            return true;
        }

        /// <summary>
        /// Prints the current board using pretty ASCII art
        /// </summary>
        public void PrintCurrentBoard()
        {
            var dispPieceTile = new Dictionary<string, char>();
            foreach (var piece in this.Game.Pieces)
            {
                char tile = (piece.Type == "Knight") ? 'N' : piece.Type[0];
                if (piece.Owner.Id == "1")
                {
                    tile = Char.ToLower(tile);
                }
                dispPieceTile[piece.File + piece.Rank] = tile;
            }

            Console.WriteLine("   +------------------------+");
            for (int rank = 8; rank >= 1; rank--)
            {
                string str = " " + rank + " |";
                for (int fileOffset = 0; fileOffset < 8; fileOffset++)
                {
                    string file = ((char)('a' + fileOffset)).ToString();
                    char tile;
                    if (!dispPieceTile.TryGetValue(file + rank, out tile))
                    {
                        tile = '.';
                    }
                    str += " " + tile + " ";
                }
                str += "|";
                Console.WriteLine(str);
            }
            Console.WriteLine("   +------------------------+");
            Console.WriteLine("     a  b  c  d  e  f  g  h");
        }

        #endregion
    }
}
