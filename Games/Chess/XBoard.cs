
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
    private static readonly UInt64 Rank7 = 0x00FF000000000000;
    private static readonly UInt64 Rank8 = 0xFF00000000000000;

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
            UInt64 pawnsNotAFile;
            UInt64 pawnsNotHFile;
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
                pawnsNotAFile = pawns & NotAFile;
                pawnsNotHFile = pawns & NotHFile;

                pawnAdvances = (pawns << 8) & state.open;
                advancingPawns = pawns & (pawnAdvances >> 8);
                dPawnAdvances = ((advancingPawns & Rank2) << 16) & state.open;
                dAdvancingPawns = pawns & (dPawnAdvances >> 16);

                pawnLeftAttacks = (pawnsNotAFile << 9) & state.blackPieces;
                attackingLeftPawns = pawns & (pawnLeftAttacks >> 9);
                pawnRightAttacks = (pawnsNotHFile << 7) & state.blackPieces;
                attackingRightPawns = pawns & (pawnRightAttacks >> 7);
            }
            else
            {
                pawns = state.blackPawns;
                pawnsNotAFile = pawns & NotAFile;
                pawnsNotHFile = pawns & NotHFile;

                pawnAdvances = (pawns >> 8) & state.open;
                advancingPawns = pawns & (pawnAdvances << 8);
                dPawnAdvances = ((advancingPawns & Rank7) >> 16) & state.open;
                dAdvancingPawns = pawns & (dPawnAdvances << 16);

                pawnLeftAttacks = (pawnsNotAFile >> 7) & state.whitePieces;
                attackingLeftPawns = pawns & (pawnLeftAttacks << 7);
                pawnRightAttacks = (pawnsNotHFile >> 9) & state.whitePieces;
                attackingRightPawns = pawns & (pawnRightAttacks << 9);
            }

            UInt64[] tempSrcs  = { advancingPawns, dAdvancingPawns, attackingLeftPawns, attackingRightPawns };
            UInt64[] tempDests = { pawnAdvances,   dPawnAdvances,   pawnLeftAttacks,    pawnRightAttacks    };
            for (var i = 0; i < 4; i++)
            {
                tempSrc = tempSrcs[i];
                tempDest = tempDests[i];
                src = MSB(tempSrc);
                dest = MSB(tempDest);
                while(src != 0)
                {
                    if ( (dest & Rank1 | dest & Rank8) == 0)
                    {
                        neighbors.Add( new XAction(src, dest) );
                    } else {
                        neighbors.Add( new XAction(src, dest, "Queen") );
                        neighbors.Add( new XAction(src, dest, "Knight") );
                        neighbors.Add( new XAction(src, dest, "Bishop") );
                        neighbors.Add( new XAction(src, dest, "Rook") );
                    }
    
                    tempSrc = tempSrc - src;
                    tempDest = tempDest - dest;
                    src = MSB(tempSrc);
                    dest = MSB(tempDest);
                }
            }
        }

        return neighbors;
    }
}
