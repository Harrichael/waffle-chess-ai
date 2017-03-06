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
        /// <returns>Represents if you want to end your turn.
        /// True means end your turn, False means to keep your turn 
        /// going and re-call this function.</returns>
        public bool RunTurn()
        {
            this.PrintCurrentBoard();
            if (this.Game.Moves.Count > 0)
            {
                Console.WriteLine("Opponent's Last Move: '" + this.Game.Moves.Last().San + "'");
            }
            Console.WriteLine("Time Remaining: " + this.Player.TimeRemaining + " ns");

            var action = this.getTurnAction();

            var srcRF = this.tileToRF(action.srcTile);
            var piece = this.Player.Pieces.First(p => (p.File + p.Rank) == srcRF);
            var destRF = this.tileToRF(action.destTile);
            piece.Move( "" + destRF[0],
                        (int)(destRF[1] - '0'),
                        action.promotionType.ToString().Replace("None", "")
                      );

            return true;
        }

        /// <summary>
        /// Lets abstract a turn into just generating an action
        /// </summary>
        private XAction getTurnAction()
        {
            var state = this.createState();
            var actions = ChessRules.LegalMoves(state);
            return Strategy.RandomSelect(actions);
        }

        /// <summary>
        /// This function is called once a turn to transform Joueur API into our chess representation
        /// </summary>
        XBoard createState()
        {
            var state = new XBoard();
            foreach (var piece in this.Game.Pieces)
            {
                var tile = this.rfToTile(piece.File + piece.Rank);
                switch (piece.Owner.Id)
                {
                    // black
                    case "1":
                        switch (piece.Type)
                        {
                            case "Pawn":
                               state.blackPawns |= tile; break;
                            case "Rook":
                               state.blackRooks |= tile; break;
                            case "Knight":
                               state.blackKnights |= tile; break;
                            case "Bishop":
                               state.blackBishops |= tile; break;
                            case "Queen":
                               state.blackQueens |= tile; break;
                            case "King":
                               state.blackKing = tile; break;
                            default:
                                throw new System.Exception("We had an invalid piece type here");
                        }
                    break;
                    // white
                    default:
                        switch (piece.Type)
                        {
                            case "Pawn":
                               state.whitePawns |= tile; break;
                            case "Rook":
                               state.whiteRooks |= tile; break;
                            case "Knight":
                               state.whiteKnights |= tile; break;
                            case "Bishop":
                               state.whiteBishops |= tile; break;
                            case "Queen":
                               state.whiteQueens |= tile; break;
                            case "King":
                               state.whiteKing = tile; break;
                            default:
                                throw new System.Exception("We had an invalid piece type here");
                        }
                    break;
                }
            }
            state.turnIsWhite = this.Game.CurrentPlayer.Color == "White";
            state.inCheck = this.Game.CurrentPlayer.InCheck;
            state.enPassTile = this.rfToTile(this.Game.Fen.Split(' ')[3]);
            var castleString = this.Game.Fen.Split(' ')[2];
            state.whiteCastleKS = castleString.Contains("K");
            state.whiteCastleQS = castleString.Contains("Q");
            state.blackCastleKS = castleString.Contains("k");
            state.blackCastleQS = castleString.Contains("q");
            state.updatePieces();
            return state;
        }

        /// <summary>
        /// Helper for creating state, reverse of tileToRF
        /// </summary>
        private UInt64 rfToTile(string rf)
        {
            switch (rf)
            {
                case "-": return 0;
                case "h1": return 1;
                case "g1": return 2;
                case "f1": return 4;
                case "e1": return 8;
                case "d1": return 16;
                case "c1": return 32;
                case "b1": return 64;
                case "a1": return 128;
                case "h2": return 256;
                case "g2": return 512;
                case "f2": return 1024;
                case "e2": return 2048;
                case "d2": return 4096;
                case "c2": return 8192;
                case "b2": return 16384;
                case "a2": return 32768;
                case "h3": return 65536;
                case "g3": return 131072;
                case "f3": return 262144;
                case "e3": return 524288;
                case "d3": return 1048576;
                case "c3": return 2097152;
                case "b3": return 4194304;
                case "a3": return 8388608;
                case "h4": return 16777216;
                case "g4": return 33554432;
                case "f4": return 67108864;
                case "e4": return 134217728;
                case "d4": return 268435456;
                case "c4": return 536870912;
                case "b4": return 1073741824;
                case "a4": return 2147483648;
                case "h5": return 4294967296;
                case "g5": return 8589934592;
                case "f5": return 17179869184;
                case "e5": return 34359738368;
                case "d5": return 68719476736;
                case "c5": return 137438953472;
                case "b5": return 274877906944;
                case "a5": return 549755813888;
                case "h6": return 1099511627776;
                case "g6": return 2199023255552;
                case "f6": return 4398046511104;
                case "e6": return 8796093022208;
                case "d6": return 17592186044416;
                case "c6": return 35184372088832;
                case "b6": return 70368744177664;
                case "a6": return 140737488355328;
                case "h7": return 281474976710656;
                case "g7": return 562949953421312;
                case "f7": return 1125899906842624;
                case "e7": return 2251799813685248;
                case "d7": return 4503599627370496;
                case "c7": return 9007199254740992;
                case "b7": return 18014398509481984;
                case "a7": return 36028797018963968;
                case "h8": return 72057594037927936;
                case "g8": return 144115188075855872;
                case "f8": return 288230376151711744;
                case "e8": return 576460752303423488;
                case "d8": return 1152921504606846976;
                case "c8": return 2305843009213693952;
                case "b8": return 4611686018427387904;
                case "a8": return 9223372036854775808;
            }
            Console.WriteLine("Invalid RF: " + rf);
            throw new System.Exception();
        }

        /// <summary>
        /// Some precomputation for transforming a bitboard action into Joueur API
        /// </summary>
        private string tileToRF(UInt64 tile)
        {
            switch(tile)
            {
                case 1: return "h1";
                case 2: return "g1";
                case 4: return "f1";
                case 8: return "e1";
                case 16: return "d1";
                case 32: return "c1";
                case 64: return "b1";
                case 128: return "a1";
                case 256: return "h2";
                case 512: return "g2";
                case 1024: return "f2";
                case 2048: return "e2";
                case 4096: return "d2";
                case 8192: return "c2";
                case 16384: return "b2";
                case 32768: return "a2";
                case 65536: return "h3";
                case 131072: return "g3";
                case 262144: return "f3";
                case 524288: return "e3";
                case 1048576: return "d3";
                case 2097152: return "c3";
                case 4194304: return "b3";
                case 8388608: return "a3";
                case 16777216: return "h4";
                case 33554432: return "g4";
                case 67108864: return "f4";
                case 134217728: return "e4";
                case 268435456: return "d4";
                case 536870912: return "c4";
                case 1073741824: return "b4";
                case 2147483648: return "a4";
                case 4294967296: return "h5";
                case 8589934592: return "g5";
                case 17179869184: return "f5";
                case 34359738368: return "e5";
                case 68719476736: return "d5";
                case 137438953472: return "c5";
                case 274877906944: return "b5";
                case 549755813888: return "a5";
                case 1099511627776: return "h6";
                case 2199023255552: return "g6";
                case 4398046511104: return "f6";
                case 8796093022208: return "e6";
                case 17592186044416: return "d6";
                case 35184372088832: return "c6";
                case 70368744177664: return "b6";
                case 140737488355328: return "a6";
                case 281474976710656: return "h7";
                case 562949953421312: return "g7";
                case 1125899906842624: return "f7";
                case 2251799813685248: return "e7";
                case 4503599627370496: return "d7";
                case 9007199254740992: return "c7";
                case 18014398509481984: return "b7";
                case 36028797018963968: return "a7";
                case 72057594037927936: return "h8";
                case 144115188075855872: return "g8";
                case 288230376151711744: return "f8";
                case 576460752303423488: return "e8";
                case 1152921504606846976: return "d8";
                case 2305843009213693952: return "c8";
                case 4611686018427387904: return "b8";
                case 9223372036854775808: return "a8";
            }
            Console.WriteLine("Invalid tile: " + tile);
            throw new System.Exception();
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
