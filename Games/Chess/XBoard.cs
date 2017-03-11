
using System;
using System.Collections.Generic;
using System.Linq;

using static ChessRules;

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

    public UInt64 enPassTile;
    public bool turnIsWhite;
    public bool whiteCheck;
    public bool blackCheck;
    public bool whiteCastleKS;
    public bool whiteCastleQS;
    public bool blackCastleKS;
    public bool blackCastleQS;
    public byte halfMoveClock;

    private Stack<XAction> actionHistory;

    public XBoard()
    {
        actionHistory = new Stack<XAction>();
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

    public void Apply(XAction action)
    {
        this.actionHistory.Push(action);
        this.enPassTile = 0;
        if (this.turnIsWhite)
        {
            switch (action.pieceType)
            {
                case PieceType.Pawn:
                    this.whitePieces = (this.whitePieces & ~action.srcTile) | action.destTile;
                    if ( (action.srcTile << 16) == action.destTile )
                    {
                        this.enPassTile = action.srcTile << 8;
                    }
                    if (action.promotionType != PieceType.None)
                    {
                        this.whitePawns = this.whitePawns & ~action.srcTile;
                        switch (action.promotionType)
                        {
                            case (PieceType.Queen):
                                this.whiteQueens = this.whiteQueens | action.destTile;
                            break;
                            case (PieceType.Knight):
                                this.whiteKnights = this.whiteKnights | action.destTile;
                            break;
                            case (PieceType.Bishop):
                                this.whiteBishops = this.whiteBishops | action.destTile;
                            break;
                            case (PieceType.Rook):
                                this.whiteRooks = this.whiteRooks | action.destTile;
                            break;
                        }
                    } else {
                        this.whitePawns = (this.whitePawns & ~action.srcTile) | action.destTile;
                    }
                break;
                case PieceType.Rook:
                    this.whiteRooks = (this.whiteRooks & ~action.srcTile) | action.destTile;
                    this.whitePieces = (this.whitePieces & ~action.srcTile) | action.destTile;
                    if ((action.srcTile & whiteKSRookStart) != 0)
                    {
                        this.whiteCastleKS = false;
                    }
                    else if ((action.srcTile & whiteQSRookStart) != 0)
                    {
                        this.whiteCastleQS = false;
                    }
                break;
                case PieceType.Knight:
                    this.whiteKnights = (this.whiteKnights & ~action.srcTile) | action.destTile;
                    this.whitePieces = (this.whitePieces & ~action.srcTile) | action.destTile;
                break;
                case PieceType.Bishop:
                    this.whiteBishops = (this.whiteBishops & ~action.srcTile) | action.destTile;
                    this.whitePieces = (this.whitePieces & ~action.srcTile) | action.destTile;
                break;
                case PieceType.Queen:
                    this.whiteQueens = (this.whiteQueens & ~action.srcTile) | action.destTile;
                    this.whitePieces = (this.whitePieces & ~action.srcTile) | action.destTile;
                break;
                case PieceType.King:
                    this.whiteKing = action.destTile;
                    this.whitePieces = (this.whitePieces & ~action.srcTile) | action.destTile;
                    this.whiteCastleKS = false;
                    this.whiteCastleQS = false;
                break;
                case PieceType.Castle:
                    if ( (action.destTile & whiteKSSpace) != 0 )
                    {
                        this.whiteKing = whiteKSDest;
                        this.whiteRooks = (this.whiteRooks & ~whiteKSRookStart) | whiteKSRookDest;
                        this.whitePieces = (this.whitePieces & ~whiteKingStart & ~whiteKSRookStart) | whiteKSDest | whiteKSRookDest;
                        this.whiteCastleKS = false;
                    } else {
                        this.whiteKing = whiteQSDest;
                        this.whiteRooks = (this.whiteRooks & ~whiteQSRookStart) | whiteQSRookDest;
                        this.whitePieces = (this.whitePieces & ~whiteKingStart & ~whiteQSRookStart) | whiteQSDest | whiteQSRookDest;
                        this.whiteCastleQS = false;
                    }
                break;
            }
            switch (action.attackType)
            {
                case PieceType.Pawn:
                    this.blackPawns = this.blackPawns & ~action.destTile;
                    this.blackPieces = this.blackPieces & ~action.destTile;
                break;
                case PieceType.Rook:
                    this.blackRooks = this.blackRooks & ~action.destTile;
                    this.blackPieces = this.blackPieces & ~action.destTile;
                break;
                case PieceType.Knight:
                    this.blackKnights = this.blackKnights & ~action.destTile;
                    this.blackPieces = this.blackPieces & ~action.destTile;
                break;
                case PieceType.Bishop:
                    this.blackBishops = this.blackBishops & ~action.destTile;
                    this.blackPieces = this.blackPieces & ~action.destTile;
                break;
                case PieceType.Queen:
                    this.blackQueens = this.blackQueens & ~action.destTile;
                    this.blackPieces = this.blackPieces & ~action.destTile;
                break;
                case PieceType.EnPass:
                    this.blackPawns = this.blackPawns & ~(action.destTile >> 8);
                    this.blackPieces = this.blackPieces & ~(action.destTile >> 8);
                break;
            }
        } else { // turn is black
            switch (action.pieceType)
            {
                case PieceType.Pawn:
                    this.blackPieces = (this.blackPieces & ~action.srcTile) | action.destTile;
                    if ( (action.srcTile >> 16) == action.destTile )
                    {
                        this.enPassTile = action.srcTile >> 8;
                    }
                    if (action.promotionType != PieceType.None)
                    {
                        this.blackPawns = this.blackPawns & ~action.srcTile;
                        switch (action.promotionType)
                        {
                            case (PieceType.Queen):
                                this.blackQueens = this.blackQueens | action.destTile;
                            break;
                            case (PieceType.Knight):
                                this.blackKnights = this.blackKnights | action.destTile;
                            break;
                            case (PieceType.Bishop):
                                this.blackBishops = this.blackBishops | action.destTile;
                            break;
                            case (PieceType.Rook):
                                this.blackRooks = this.blackRooks | action.destTile;
                            break;
                        }
                    } else {
                        this.blackPawns = (this.blackPawns & ~action.srcTile) | action.destTile;
                    }
                break;
                case PieceType.Rook:
                    this.blackRooks = (this.blackRooks & ~action.srcTile) | action.destTile;
                    this.blackPieces = (this.blackPieces & ~action.srcTile) | action.destTile;
                    if ((action.srcTile & blackKSRookStart) != 0)
                    {
                        this.blackCastleKS = false;
                    }
                    else if ((action.srcTile & blackQSRookStart) != 0)
                    {
                        this.blackCastleQS = false;
                    }
                break;
                case PieceType.Knight:
                    this.blackKnights = (this.blackKnights & ~action.srcTile) | action.destTile;
                    this.blackPieces = (this.blackPieces & ~action.srcTile) | action.destTile;
                break;
                case PieceType.Bishop:
                    this.blackBishops = (this.blackBishops & ~action.srcTile) | action.destTile;
                    this.blackPieces = (this.blackPieces & ~action.srcTile) | action.destTile;
                break;
                case PieceType.Queen:
                    this.blackQueens = (this.blackQueens & ~action.srcTile) | action.destTile;
                    this.blackPieces = (this.blackPieces & ~action.srcTile) | action.destTile;
                break;
                case PieceType.King:
                    this.blackKing = action.destTile;
                    this.blackPieces = (this.blackPieces & ~action.srcTile) | action.destTile;
                    this.blackCastleKS = false;
                    this.blackCastleQS = false;
                break;
                case PieceType.Castle:
                    if ( (action.destTile & blackKSSpace) != 0 )
                    {
                        this.blackKing = blackKSDest;
                        this.blackRooks = (this.blackRooks & ~blackKSRookStart) | blackKSRookDest;
                        this.blackPieces = (this.blackPieces & ~blackKingStart & ~blackKSRookStart) | blackKSDest | blackKSRookDest;
                        this.blackCastleKS = false;
                    } else {
                        this.blackKing = blackQSDest;
                        this.blackRooks = (this.blackRooks & ~blackQSRookStart) | blackQSRookDest;
                        this.blackPieces = (this.blackPieces & ~blackKingStart & ~blackQSRookStart) | blackQSDest | blackQSRookDest;
                        this.blackCastleQS = false;
                    }
                break;
            }
            switch (action.attackType)
            {
                case PieceType.Pawn:
                    this.whitePawns = this.whitePawns & ~action.destTile;
                    this.whitePieces = this.whitePieces & ~action.destTile;
                break;
                case PieceType.Rook:
                    this.whiteRooks = this.whiteRooks & ~action.destTile;
                    this.whitePieces = this.whitePieces & ~action.destTile;
                break;
                case PieceType.Knight:
                    this.whiteKnights = this.whiteKnights & ~action.destTile;
                    this.whitePieces = this.whitePieces & ~action.destTile;
                break;
                case PieceType.Bishop:
                    this.whiteBishops = this.whiteBishops & ~action.destTile;
                    this.whitePieces = this.whitePieces & ~action.destTile;
                break;
                case PieceType.Queen:
                    this.whiteQueens = this.whiteQueens & ~action.destTile;
                    this.whitePieces = this.whitePieces & ~action.destTile;
                break;
                case PieceType.EnPass:
                    this.whitePawns = this.whitePawns & ~(action.destTile << 8);
                    this.whitePieces = this.whitePieces & ~(action.destTile << 8);
                break;
            }
        }
        this.pieces = this.whitePieces | this.blackPieces;
        this.open = ~this.pieces;
        this.turnIsWhite = !this.turnIsWhite;
        this.whiteCheck = Threats(this, this.whiteKing) != 0;
        this.blackCheck = Threats(this, this.blackKing) != 0;
    }

    public void Undo()
    {
        var action = this.actionHistory.Pop();
        this.turnIsWhite = !this.turnIsWhite;
        this.enPassTile = 0;
        this.whiteCastleKS = (action.castleSettings & 0x1) != 0;
        this.whiteCastleQS = (action.castleSettings & 0x2) != 0;
        this.blackCastleKS = (action.castleSettings & 0x4) != 0;
        this.blackCastleQS = (action.castleSettings & 0x8) != 0;
        if (this.turnIsWhite)
        {
            switch (action.pieceType)
            {
                case PieceType.Pawn:
                    this.whitePieces = (this.whitePieces | action.srcTile) & ~action.destTile;
                    this.whitePawns = this.whitePawns | action.srcTile;
                    switch (action.promotionType)
                    {
                        case PieceType.Queen:
                            this.whiteQueens = this.whiteQueens & ~action.destTile;
                        break;
                        case PieceType.Knight:
                            this.whiteKnights = this.whiteKnights & ~action.destTile;
                        break;
                        case PieceType.Bishop:
                            this.whiteBishops = this.whiteBishops & ~action.destTile;
                        break;
                        case PieceType.Rook:
                            this.whiteRooks = this.whiteRooks & ~action.destTile;
                        break;
                        case PieceType.None:
                            this.whitePawns = this.whitePawns & ~action.destTile;
                        break;
                    }
                break;
                case PieceType.Rook:
                    this.whiteRooks = (this.whiteRooks | action.srcTile) & ~action.destTile;
                    this.whitePieces = (this.whitePieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.Knight:
                    this.whiteKnights = (this.whiteKnights | action.srcTile) & ~action.destTile;
                    this.whitePieces = (this.whitePieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.Bishop:
                    this.whiteBishops = (this.whiteBishops | action.srcTile) & ~action.destTile;
                    this.whitePieces = (this.whitePieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.Queen:
                    this.whiteQueens = (this.whiteQueens | action.srcTile) & ~action.destTile;
                    this.whitePieces = (this.whitePieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.King:
                    this.whiteKing = action.srcTile;
                    this.whitePieces = (this.whitePieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.Castle:
                    this.whiteKing = whiteKingStart;
                    if ( (action.destTile & whiteKSSpace) != 0 )
                    {
                        this.whiteRooks = (this.whiteRooks & ~whiteKSRookDest) | whiteKSRookStart;
                        this.whitePieces = (this.whitePieces | whiteKingStart | whiteKSRookStart) & ~whiteKSDest & ~whiteKSRookDest;
                    } else {
                        this.whiteRooks = (this.whiteRooks & ~whiteQSRookDest) | whiteQSRookStart;
                        this.whitePieces = (this.whitePieces | whiteKingStart | whiteQSRookStart) & ~whiteQSDest & ~whiteQSRookDest;
                    }
                break;
            }
            switch (action.attackType)
            {
                case PieceType.Pawn:
                    this.blackPawns = this.blackPawns | action.destTile;
                    this.blackPieces = this.blackPieces | action.destTile;
                break;
                case PieceType.Rook:
                    this.blackRooks = this.blackRooks | action.destTile;
                    this.blackPieces = this.blackPieces | action.destTile;
                break;
                case PieceType.Knight:
                    this.blackKnights = this.blackKnights | action.destTile;
                    this.blackPieces = this.blackPieces | action.destTile;
                break;
                case PieceType.Bishop:
                    this.blackBishops = this.blackBishops | action.destTile;
                    this.blackPieces = this.blackPieces | action.destTile;
                break;
                case PieceType.Queen:
                    this.blackQueens = this.blackQueens | action.destTile;
                    this.blackPieces = this.blackPieces | action.destTile;
                break;
                case PieceType.EnPass:
                    this.blackPawns = this.blackPawns | (action.destTile >> 8);
                    this.blackPieces = this.blackPieces | (action.destTile >> 8);
                break;
            }
        } else { // turn is black
            switch (action.pieceType)
            {
                case PieceType.Pawn:
                    this.blackPieces = (this.blackPieces | action.srcTile) & ~action.destTile;
                    this.blackPawns = this.blackPawns | action.srcTile;
                    switch (action.promotionType)
                    {
                        case PieceType.Queen:
                            this.blackQueens = this.blackQueens & ~action.destTile;
                        break;
                        case PieceType.Knight:
                            this.blackKnights = this.blackKnights & ~action.destTile;
                        break;
                        case PieceType.Bishop:
                            this.blackBishops = this.blackBishops & ~action.destTile;
                        break;
                        case PieceType.Rook:
                            this.blackRooks = this.blackRooks & ~action.destTile;
                        break;
                        case PieceType.None:
                            this.blackPawns = this.blackPawns & ~action.destTile;
                        break;
                    }
                break;
                case PieceType.Rook:
                    this.blackRooks = (this.blackRooks | action.srcTile) & ~action.destTile;
                    this.blackPieces = (this.blackPieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.Knight:
                    this.blackKnights = (this.blackKnights | action.srcTile) & ~action.destTile;
                    this.blackPieces = (this.blackPieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.Bishop:
                    this.blackBishops = (this.blackBishops | action.srcTile) & ~action.destTile;
                    this.blackPieces = (this.blackPieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.Queen:
                    this.blackQueens = (this.blackQueens | action.srcTile) & ~action.destTile;
                    this.blackPieces = (this.blackPieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.King:
                    this.blackKing = action.srcTile;
                    this.blackPieces = (this.blackPieces | action.srcTile) & ~action.destTile;
                break;
                case PieceType.Castle:
                    this.blackKing = blackKingStart;
                    if ( (action.destTile & blackKSSpace) != 0 )
                    {
                        this.blackRooks = (this.blackRooks & ~blackKSRookDest) | blackKSRookStart;
                        this.blackPieces = (this.blackPieces | blackKingStart | blackKSRookStart) & ~blackKSDest & ~blackKSRookDest;
                    } else {
                        this.blackRooks = (this.blackRooks & ~blackQSRookDest) | blackQSRookStart;
                        this.blackPieces = (this.blackPieces | blackKingStart | blackQSRookStart) & ~blackQSDest & ~blackQSRookDest;
                    }
                break;
            }
            switch (action.attackType)
            {
                case PieceType.Pawn:
                    this.whitePawns = this.whitePawns | action.destTile;
                    this.whitePieces = this.whitePieces | action.destTile;
                break;
                case PieceType.Rook:
                    this.whiteRooks = this.whiteRooks | action.destTile;
                    this.whitePieces = this.whitePieces | action.destTile;
                break;
                case PieceType.Knight:
                    this.whiteKnights = this.whiteKnights | action.destTile;
                    this.whitePieces = this.whitePieces | action.destTile;
                break;
                case PieceType.Bishop:
                    this.whiteBishops = this.whiteBishops | action.destTile;
                    this.whitePieces = this.whitePieces | action.destTile;
                break;
                case PieceType.Queen:
                    this.whiteQueens = this.whiteQueens | action.destTile;
                    this.whitePieces = this.whitePieces | action.destTile;
                break;
                case PieceType.EnPass:
                    this.whitePawns = this.whitePawns | (action.destTile << 8);
                    this.whitePieces = this.whitePieces | (action.destTile << 8);
                break;
            }
        }
        this.pieces = this.whitePieces | this.blackPieces;
        this.open = ~this.pieces;
    }

}

