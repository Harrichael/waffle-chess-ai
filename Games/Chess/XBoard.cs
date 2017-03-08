
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
    public bool whiteCheck;
    public bool blackCheck;
    public bool whiteCastleKS;
    public bool whiteCastleQS;
    public bool blackCastleKS;
    public bool blackCastleQS;
    public byte halfMoveClock;
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

    public XBoard Copy()
    {
        return (XBoard)this.MemberwiseClone();
    }

    public bool Equals(XBoard obj)
    {
        return ( (this.whitePawns    == obj.whitePawns)    &&
                 (this.whiteRooks    == obj.whiteRooks)    &&
                 (this.whiteKnights  == obj.whiteKnights)  &&
                 (this.whiteBishops  == obj.whiteBishops)  &&
                 (this.whiteQueens   == obj.whiteQueens)   &&
                 (this.whiteKing     == obj.whiteKing)     &&
                 (this.blackPawns    == obj.blackPawns)    &&
                 (this.blackRooks    == obj.blackRooks)    &&
                 (this.blackKnights  == obj.blackKnights)  &&
                 (this.blackBishops  == obj.blackBishops)  &&
                 (this.blackQueens   == obj.blackQueens)   &&
                 (this.blackKing     == obj.blackKing)     &&
                 (this.whitePieces   == obj.whitePieces)   &&
                 (this.blackPieces   == obj.blackPieces)   &&
                 (this.pieces        == obj.pieces)        &&
                 (this.open          == obj.open)          &&
                 (this.turnIsWhite   == obj.turnIsWhite)   &&
                 (this.whiteCheck    == obj.whiteCheck)    &&
                 (this.blackCheck    == obj.blackCheck)
        );
    }

}

