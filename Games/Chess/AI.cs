// This is where you build your AI for the Chess game.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        }


        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn. True means end your turn, False means to keep your turn going and re-call this function.</returns>
        public bool RunTurn()
        {
            // Here is where you'll want to code your AI.

            // We've provided sample code that:
            //    1) prints the board to the console
            //    2) prints the opponent's last move to the console
            //    3) prints how much time remaining this AI has to calculate moves
            //    4) makes a random (and probably invalid) move.

            // 1) print the board to the console
            this.PrintCurrentBoard();

            // 2) print the opponent's last move to the console
            if (this.Game.Moves.Count > 0) {
                Console.WriteLine("Opponent's Last Move: '" + this.Game.Moves.Last().San + "'");
            }

            // 3) print how much time remaining this AI has to calculate moves
            Console.WriteLine("Time Remaining: " + this.Player.TimeRemaining + " ns");

            // 4) make a random (and probably invalid) move.
            var rand = new Random();
            var randomPiece = this.Player.Pieces[rand.Next(this.Player.Pieces.Count)];
            string randomFile = "" + (char)(((int)"a"[0]) + rand.Next(0, 8));
            int randomRank = rand.Next(0, 8) + 1;
            randomPiece.Move(randomFile, randomRank);

            return true; // to signify we are done with our turn.
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
