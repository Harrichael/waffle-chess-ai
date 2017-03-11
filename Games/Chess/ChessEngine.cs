
using System;
using System.Collections.Generic;
using System.Linq;

public class ChessEngine
{
    private XBoard board;
    private bool aiIsWhite;

    /// <summary>
    /// Initialize the engine with the board state, does not need to be standard start
    /// </summary>
    public ChessEngine(string fen, bool aiIsWhite)
    {
        this.board = new XBoard();
        this.aiIsWhite = aiIsWhite;

        // Initialize XBoard instance
        var fenFields = fen.Split(' ');
        int rank = 7; // 0 indexed, use ChessRules.Ranks array
        int file = 0; // 0 indexed, use ChessRules.Files array
        foreach (char c in fenFields[0])
        {
            file = Math.Min(file, 7);
            UInt64 tile = ChessRules.Ranks[rank] & ChessRules.Files[file];
            switch (c)
            {
                // White
                case 'P':
                    this.board.whitePawns |= tile;
                break;
                case 'R':
                    this.board.whiteRooks |= tile;
                break;
                case 'N':
                    this.board.whiteKnights |= tile;
                break;
                case 'B':
                    this.board.whiteBishops |= tile;
                break;
                case 'Q':
                    this.board.whiteQueens |= tile;
                break;
                case 'K':
                    this.board.whiteKing = tile;
                break;
                // Black
                case 'p':
                    this.board.blackPawns |= tile;
                break;
                case 'r':
                    this.board.blackRooks |= tile;
                break;
                case 'n':
                    this.board.blackKnights |= tile;
                break;
                case 'b':
                    this.board.blackBishops |= tile;
                break;
                case 'q':
                    this.board.blackQueens |= tile;
                break;
                case 'k':
                    this.board.blackKing = tile;
                break;
                // Special character handling
                case'/':
                    file = -1; // will be incremented to 0 at end of loop-body
                    rank -= 1;
                break;
                default:
                    file += c - '0';
                break;
            }

            file += 1;
        } // End foreach

        this.board.turnIsWhite = fenFields[1] == "w";
        this.board.whiteCastleKS = fenFields[2].Contains("K");
        this.board.whiteCastleQS = fenFields[2].Contains("Q");
        this.board.blackCastleKS = fenFields[2].Contains("k");
        this.board.blackCastleQS = fenFields[2].Contains("q");
        this.board.enPassTile = this.frToTile(fenFields[3]);
        this.board.halfMoveClock = byte.Parse(fenFields[4]);
        this.board.updatePieces();
        this.board.whiteCheck = false; // TODO: check threats
        this.board.blackCheck = false; // TODO: check threats

    } // End fen constructor method

    public void Print()
    {
        var dispPieceTile = new Dictionary<string, char>();
        UInt64[] whitePieceBBs = {this.board.whitePawns, this.board.whiteRooks, this.board.whiteKnights, this.board.whiteBishops, this.board.whiteQueens, this.board.whiteKing};
        UInt64[] blackPieceBBs = {this.board.blackPawns, this.board.blackRooks, this.board.blackKnights, this.board.blackBishops, this.board.blackQueens, this.board.blackKing};
        PieceType[] pts = {PieceType.Pawn, PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King};
        UInt64 pieces;
        UInt64 piece;
        for (var i = 0; i < 6; i++)
        {
            char c = (pts[i].ToString() == "Knight") ? 'N' : pts[i].ToString()[0];
            pieces = whitePieceBBs[i];
            while(pieces != 0)
            {
                piece = ChessRules.MSB(pieces);
                pieces = pieces - piece;
                dispPieceTile[this.tileToFR(piece)] = c;
            }
            c = (pts[i].ToString() == "Knight") ? 'n' : pts[i].ToString().ToLower()[0];
            pieces = blackPieceBBs[i];
            while(pieces != 0)
            {
                piece = ChessRules.MSB(pieces);
                pieces = pieces - piece;
                dispPieceTile[this.tileToFR(piece)] = c;
            }
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
    } // End Print

    /// <summary>
    /// Call when opponent makes move. Trusts caller to call when actually is time for opponent
    /// </summary>
    public void OpponentMove(string fromFR, string toFR, string promote)
    {
        UInt64 srcTile = this.frToTile(fromFR);
        UInt64 destTile = this.frToTile(toFR);
        PieceType movedPiece = this.pieceAtTile(srcTile);
        if (movedPiece == PieceType.King)
        {
            // Might be a castle, castle is in implicit super-piece representation of king and rook
            if ((srcTile & ChessRules.kingStarts) != 0 && (destTile & ChessRules.kingDests) != 0)
            {
                movedPiece = PieceType.Castle;
            }
        }
        PieceType attackedPiece = this.pieceAtTile(destTile);
        PieceType promotePiece = (PieceType)Enum.Parse(typeof(PieceType), promote);
        byte castleSettings = (byte)( (Convert.ToByte(this.board.whiteCastleKS))      &
                                      (Convert.ToByte(this.board.whiteCastleQS) << 1) &
                                      (Convert.ToByte(this.board.blackCastleKS) << 2) &
                                      (Convert.ToByte(this.board.blackCastleQS) << 3)
                                    );

        var action = new XAction(
            srcTile,
            destTile,
            castleSettings,
            movedPiece,
            attackedPiece,
            promotePiece
        );
        
        this.board.Apply(action);
    } // End Opponent Move

    public Tuple<string, string, string> MakeMove()
    {
        var action = ChessStrategy.DL_Minimax(this.board, 2, this.aiIsWhite);
        this.board.Apply(action);
        return Tuple.Create( this.tileToFR(action.srcTile),
                             this.tileToFR(action.destTile),
                             action.promotionType.ToString()
        );
    } // End Make Move

    /// <summary>
    /// Helper for reading fen
    /// </summary>
    private UInt64 frToTile(string fr)
    {
        switch (fr)
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
        Console.WriteLine("Invalid FR: " + fr);
        throw new System.Exception();
    } // End file rank to tile method

    /// <summary>
    /// Some precomputation for transforming a bitboard action into Joueur API
    /// </summary>
    private string tileToFR(UInt64 tile)
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
    } // End tile to file rank method

    private PieceType pieceAtTile(UInt64 tile)
    {
        if ((tile & (this.board.whitePawns | this.board.blackPawns)) != 0)
        {
            return PieceType.Pawn;
        } else if ((tile & (this.board.whiteRooks | this.board.blackRooks)) != 0)
        {
            return PieceType.Rook;
        } else if ((tile & (this.board.whiteKnights | this.board.blackKnights)) != 0)
        {
            return PieceType.Knight;
        } else if ((tile & (this.board.whiteBishops | this.board.blackBishops)) != 0)
        {
            return PieceType.Bishop;
        } else if ((tile & (this.board.whiteQueens | this.board.blackQueens)) != 0)
        {
            return PieceType.Queen;
        } else if ((tile & (this.board.whiteKing | this.board.blackKing)) != 0)
        {
            return PieceType.King;
        } else if ((tile & this.board.enPassTile) != 0)
        {
            return PieceType.EnPass;
        } else {
            return PieceType.None;
        }
    } // End piece at tile method

}
