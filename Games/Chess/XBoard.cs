
using System;
using System.Collections.Generic;
using System.Linq;


public class XBoard
{
    public UInt64 whitePawns;
    public UInt64 whiteRooks;
    public UInt64 whiteKnights;
    public UInt64 whiteBishops;
    public UInt64 whiteQueens;
    public UInt64 whiteKing;

    public UInt64 blackPawns;
    public UInt64 blackRooks;
    public UInt64 blackKnights;
    public UInt64 blackBishops;
    public UInt64 blackQueens;
    public UInt64 blackKing;

    public UInt64 whitePieces;
    public UInt64 blackPieces;
    public UInt64 pieces;
    public UInt64 open;

    public bool turnIsWhite;
    public UInt64 enPassTile;

    public XBoard()
    {
        //instance variables auto assigned to 0
    }

    public void updatePieces()
    {
        this.whitePieces = ( this.whitePawns   | this.whiteRooks   |
                             this.whiteKnights | this.whiteBishops |
                             this.whiteQueens  | this.whiteKing      );

        this.blackPieces = ( this.blackPawns   | this.blackRooks   |
                             this.blackKnights | this.blackBishops |
                             this.blackQueens  | this.blackKing      );
        
        this.pieces = this.whitePieces | this.blackPieces;
        this.open = ~this.pieces;
    }
}

static class ChessRules
{
    private static readonly UInt64 Rank1 = 0x00000000000000FF;
    private static readonly UInt64 Rank2 = 0x000000000000FF00;
    private static readonly UInt64 Rank3 = 0x0000000000FF0000;
    private static readonly UInt64 Rank4 = 0x00000000FF000000;
    private static readonly UInt64 Rank5 = 0x000000FF00000000;
    private static readonly UInt64 Rank6 = 0x0000FF0000000000;
    private static readonly UInt64 Rank7 = 0x00FF000000000000;
    private static readonly UInt64 Rank8 = 0xFF00000000000000;

    private static readonly UInt64[] Ranks = {Rank1, Rank2, Rank3, Rank4, Rank5, Rank6, Rank7, Rank8};

    private static readonly UInt64 AFile = 0x8080808080808080;
    private static readonly UInt64 BFile = 0x4040404040404040;
    private static readonly UInt64 CFile = 0x2020202020202020;
    private static readonly UInt64 DFile = 0x1010101010101010;
    private static readonly UInt64 EFile = 0x0808080808080808;
    private static readonly UInt64 FFile = 0x0404040404040404;
    private static readonly UInt64 GFile = 0x0202020202020202;
    private static readonly UInt64 HFile = 0x0101010101010101;

    private static readonly UInt64[] Files = {AFile, BFile, CFile, DFile, EFile, FFile, GFile, HFile};

    private static readonly UInt64 NotAFile = ~AFile;
    private static readonly UInt64 NotHFile = ~HFile;

    public static UInt64 MSB(UInt64 input)
    {
        if (input == 0) return 0;
    
        UInt64 msb = 1;
    
        if ((input >> 32) == 0) 
        {
            input = input << 32;
        } else {
            msb = msb << 32;
        }
        if ((input >> 48) == 0) 
        {
            input = input << 16;
        } else {
            msb = msb << 16;
        }
        if ((input >> 56) == 0) 
        {
            input = input << 8;
        } else {
            msb = msb << 8;
        }
        if ((input >> 60) == 0) 
        {
            input = input << 4;
        } else {
            msb = msb << 4;
        }
        if ((input >> 62) == 0) 
        {
            input = input << 2;
        } else {
            msb = msb << 2;
        }
        if ((input >> 63) == 0) 
        {
            input = input << 1;
        } else {
            msb = msb << 1;
        }
    
        return msb;
    }

    public static List<XAction> LegalMoves(XBoard state)
    {
        var neighbors = new List<XAction>();
        { // Handle Pawns
            UInt64 pawns;
            UInt64 pawnAdvances;
            UInt64 advancingPawns;
            UInt64 dPawnAdvances;
            UInt64 dAdvancingPawns;
            UInt64 pawnLeftAttacks;
            UInt64 pawnRightAttacks;
            UInt64 attackingLeftPawns;
            UInt64 attackingRightPawns;
            UInt64 tempSrc;
            UInt64 tempDest;
            UInt64 src;
            UInt64 dest;
    
            if (state.turnIsWhite)
            {
                pawns = state.whitePawns;
                var attackTiles = state.blackPieces | state.enPassTile;
                var pawnsNotAFile = pawns & NotAFile;
                var pawnsNotHFile = pawns & NotHFile;

                pawnAdvances = (pawns << 8) & state.open;
                advancingPawns = pawns & (pawnAdvances >> 8);
                dPawnAdvances = ((advancingPawns & Rank2) << 16) & state.open;
                dAdvancingPawns = pawns & (dPawnAdvances >> 16);

                pawnLeftAttacks = (pawnsNotAFile << 9) & attackTiles;
                attackingLeftPawns = pawns & (pawnLeftAttacks >> 9);
                pawnRightAttacks = (pawnsNotHFile << 7) & attackTiles;
                attackingRightPawns = pawns & (pawnRightAttacks >> 7);
            }
            else
            {
                pawns = state.blackPawns;
                var attackTiles = state.whitePieces | state.enPassTile;
                var pawnsNotAFile = pawns & NotAFile;
                var pawnsNotHFile = pawns & NotHFile;

                pawnAdvances = (pawns >> 8) & state.open;
                advancingPawns = pawns & (pawnAdvances << 8);
                dPawnAdvances = ((advancingPawns & Rank7) >> 16) & state.open;
                dAdvancingPawns = pawns & (dPawnAdvances << 16);

                pawnLeftAttacks = (pawnsNotAFile >> 7) & attackTiles;
                attackingLeftPawns = pawns & (pawnLeftAttacks << 7);
                pawnRightAttacks = (pawnsNotHFile >> 9) & attackTiles;
                attackingRightPawns = pawns & (pawnRightAttacks << 9);
            }

            UInt64[] tempSrcs  = { advancingPawns, dAdvancingPawns, attackingLeftPawns, attackingRightPawns };
            UInt64[] tempDests = { pawnAdvances,   dPawnAdvances,   pawnLeftAttacks,    pawnRightAttacks    };
            for (var i = 0; i < 4; i++)
            {
                tempSrc = tempSrcs[i];
                tempDest = tempDests[i];
                while(tempSrc != 0)
                {
                    src = MSB(tempSrc);
                    dest = MSB(tempDest);
                    tempSrc = tempSrc - src;
                    tempDest = tempDest - dest;
                    if ( (dest & (Rank1 | Rank8)) == 0)
                    {
                        neighbors.Add( new XAction(src, dest) );
                    } else {
                        neighbors.Add( new XAction(src, dest, "Queen") );
                        neighbors.Add( new XAction(src, dest, "Knight") );
                        neighbors.Add( new XAction(src, dest, "Bishop") );
                        neighbors.Add( new XAction(src, dest, "Rook") );
                    }

                }
            }
        } // End Pawns
        { // Handle Knights
            UInt64 opponentPieces;
            UInt64 knights;
            UInt64 knight;
            UInt64 knightAttacks;
            UInt64 knightAttack;

            if (state.turnIsWhite)
            {
                knights = state.whiteKnights;
                opponentPieces = state.blackPieces;
            } else {
                knights = state.blackKnights;
                opponentPieces = state.whitePieces;
            }

            while(knights != 0)
            {
                knight = MSB(knights);
                knights = knights - knight;
                knightAttacks = getKnightAttacks(knight) & (state.open | opponentPieces);
                while(knightAttacks != 0)
                {
                    knightAttack = MSB(knightAttacks);
                    knightAttacks = knightAttacks - knightAttacks;
                    neighbors.Add( new XAction(knight, knightAttack) );
                }
            }

        } // End Knights
        { // Handle King
            UInt64 opponentPieces;
            UInt64 king;
            UInt64 kingAttacks;
            UInt64 kingAttack;

            if (state.turnIsWhite)
            {
                king = state.whiteKing;
                opponentPieces = state.blackPieces;
            } else {
                king = state.blackKing;
                opponentPieces = state.whitePieces;
            }
            kingAttacks = getKingAttacks(king) & (state.open | opponentPieces);
            while(kingAttacks != 0)
            {
                kingAttack = MSB(kingAttacks);
                kingAttacks = kingAttacks - kingAttack;
                neighbors.Add( new XAction(king, kingAttack) );
            }
        } // End King
        { // Handle Rooks and QueenRooks
            UInt64 opponentPieces;
            UInt64 rooks;
            UInt64 rook;
            UInt64 rookAttacks = 0;
            UInt64 rookAttack;
            UInt64 validMove;

            if (state.turnIsWhite)
            {
                rooks = state.whiteRooks | state.whiteQueens;
                opponentPieces = state.blackPieces;
            } else {
                rooks = state.blackRooks | state.blackQueens;
                opponentPieces = state.whitePieces;
            }

            validMove = state.open | opponentPieces;

            while(rooks != 0)
            {
                rook = MSB(rooks);
                rooks = rooks - rook;

                // up
                rookAttack = rook << 8;
                while((rookAttack & validMove) != 0)
                {
                    rookAttacks = rookAttacks | rookAttack;
                    rookAttack = rookAttack << 8;
                    if ((rookAttack & opponentPieces) != 0) break;
                }
                // down
                rookAttack = rook >> 8;
                while((rookAttack & validMove) != 0)
                {
                    rookAttacks = rookAttacks | rookAttack;
                    rookAttack = rookAttack >> 8;
                    if ((rookAttack & opponentPieces) != 0) break;
                }
                // left
                rookAttack = rook << 1;
                while((rookAttack & validMove & NotHFile) != 0)
                {
                    rookAttacks = rookAttacks | rookAttack;
                    rookAttack = rookAttack << 1;
                    if ((rookAttack & opponentPieces) != 0) break;
                }
                // right
                rookAttack = rook >> 1;
                while((rookAttack & validMove & NotAFile) != 0)
                {
                    rookAttacks = rookAttacks | rookAttack;
                    rookAttack = rookAttack >> 1;
                    if ((rookAttack & opponentPieces) != 0) break;
                }

                while(rookAttacks != 0)
                {
                    rookAttack = MSB(rookAttacks);
                    rookAttacks = rookAttacks - rookAttack;
                    neighbors.Add( new XAction(rook, rookAttack) );
                }
            }
        } // End Rooks and QueenRooks

        return neighbors;
    } // End LegalMoves

    /// <summary>
    /// Some precomputation for king movement
    /// </summary>
    private static UInt64 getKingAttacks(UInt64 king)
    {
        switch (king)
        {
            case 1: return 770;
            case 2: return 1797;
            case 4: return 3594;
            case 8: return 7188;
            case 16: return 14376;
            case 32: return 28752;
            case 64: return 57504;
            case 128: return 49216;
            case 256: return 197123;
            case 512: return 460039;
            case 1024: return 920078;
            case 2048: return 1840156;
            case 4096: return 3680312;
            case 8192: return 7360624;
            case 16384: return 14721248;
            case 32768: return 12599488;
            case 65536: return 50463488;
            case 131072: return 117769984;
            case 262144: return 235539968;
            case 524288: return 471079936;
            case 1048576: return 942159872;
            case 2097152: return 1884319744;
            case 4194304: return 3768639488;
            case 8388608: return 3225468928;
            case 16777216: return 12918652928;
            case 33554432: return 30149115904;
            case 67108864: return 60298231808;
            case 134217728: return 120596463616;
            case 268435456: return 241192927232;
            case 536870912: return 482385854464;
            case 1073741824: return 964771708928;
            case 2147483648: return 825720045568;
            case 4294967296: return 3307175149568;
            case 8589934592: return 7718173671424;
            case 17179869184: return 15436347342848;
            case 34359738368: return 30872694685696;
            case 68719476736: return 61745389371392;
            case 137438953472: return 123490778742784;
            case 274877906944: return 246981557485568;
            case 549755813888: return 211384331665408;
            case 1099511627776: return 846636838289408;
            case 2199023255552: return 1975852459884544;
            case 4398046511104: return 3951704919769088;
            case 8796093022208: return 7903409839538176;
            case 17592186044416: return 15806819679076352;
            case 35184372088832: return 31613639358152704;
            case 70368744177664: return 63227278716305408;
            case 140737488355328: return 54114388906344448;
            case 281474976710656: return 216739030602088448;
            case 562949953421312: return 505818229730443264;
            case 1125899906842624: return 1011636459460886528;
            case 2251799813685248: return 2023272918921773056;
            case 4503599627370496: return 4046545837843546112;
            case 9007199254740992: return 8093091675687092224;
            case 18014398509481984: return 16186183351374184448;
            case 36028797018963968: return 13853283560024178688;
            case 72057594037927936: return 144959613005987840;
            case 144115188075855872: return 362258295026614272;
            case 288230376151711744: return 724516590053228544;
            case 576460752303423488: return 1449033180106457088;
            case 1152921504606846976: return 2898066360212914176;
            case 2305843009213693952: return 5796132720425828352;
            case 4611686018427387904: return 11592265440851656704;
            case 9223372036854775808: return 9277415232383221760;
            default:
                Console.WriteLine("Incorrect king position");
                throw new System.Exception();
        }
    }

    /// <summary>
    /// Some precomputation for knight movement
    /// </summary>
    private static UInt64 getKnightAttacks(UInt64 knight)
    {
        switch (knight)
        {
            case 1: return 132096;
            case 2: return 329728;
            case 4: return 659712;
            case 8: return 1319424;
            case 16: return 2638848;
            case 32: return 5277696;
            case 64: return 10489856;
            case 128: return 4202496;
            case 256: return 33816580;
            case 512: return 84410376;
            case 1024: return 168886289;
            case 2048: return 337772578;
            case 4096: return 675545156;
            case 8192: return 1351090312;
            case 16384: return 2685403152;
            case 32768: return 1075839008;
            case 65536: return 8657044482;
            case 131072: return 21609056261;
            case 262144: return 43234889994;
            case 524288: return 86469779988;
            case 1048576: return 172939559976;
            case 2097152: return 345879119952;
            case 4194304: return 687463207072;
            case 8388608: return 275414786112;
            case 16777216: return 2216203387392;
            case 33554432: return 5531918402816;
            case 67108864: return 11068131838464;
            case 134217728: return 22136263676928;
            case 268435456: return 44272527353856;
            case 536870912: return 88545054707712;
            case 1073741824: return 175990581010432;
            case 2147483648: return 70506185244672;
            case 4294967296: return 567348067172352;
            case 8589934592: return 1416171111120896;
            case 17179869184: return 2833441750646784;
            case 34359738368: return 5666883501293568;
            case 68719476736: return 11333767002587136;
            case 137438953472: return 22667534005174272;
            case 274877906944: return 45053588738670592;
            case 549755813888: return 18049583422636032;
            case 1099511627776: return 145241105196122112;
            case 2199023255552: return 362539804446949376;
            case 4398046511104: return 725361088165576704;
            case 8796093022208: return 1450722176331153408;
            case 17592186044416: return 2901444352662306816;
            case 35184372088832: return 5802888705324613632;
            case 70368744177664: return 11533718717099671552;
            case 140737488355328: return 4620693356194824192;
            case 281474976710656: return 288234782788157440;
            case 562949953421312: return 576469569871282176;
            case 1125899906842624: return 1224997833292120064;
            case 2251799813685248: return 2449995666584240128;
            case 4503599627370496: return 4899991333168480256;
            case 9007199254740992: return 9799982666336960512;
            case 18014398509481984: return 1152939783987658752;
            case 36028797018963968: return 2305878468463689728;
            case 72057594037927936: return 1128098930098176;
            case 144115188075855872: return 2257297371824128;
            case 288230376151711744: return 4796069720358912;
            case 576460752303423488: return 9592139440717824;
            case 1152921504606846976: return 19184278881435648;
            case 2305843009213693952: return 38368557762871296;
            case 4611686018427387904: return 4679521487814656;
            case 9223372036854775808: return 9077567998918656;
            default:
                Console.WriteLine("Incorrect knight position");
                throw new System.Exception();
        }
    }
}
