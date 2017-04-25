
using System;
using System.Collections.Generic;
using System.Linq;

public class XAction
{
    public UInt64 srcTile;
    public UInt64 destTile;
    public PieceType pieceType;
    public PieceType attackType;
    public PieceType promotionType;

    public XAction( UInt64 src,
                    UInt64 dest,
                    PieceType type,
                    PieceType attack=PieceType.None,
                    PieceType promote=PieceType.None
                  )
    {
        this.srcTile = src;
        this.destTile = dest;
        this.pieceType = type;
        this.attackType = attack;
        this.promotionType = promote;
    }

    public override int GetHashCode()
    {
        return ( this.srcTile.GetHashCode() * 23 +
                 this.destTile.GetHashCode() * 17 +
                 (int)this.promotionType * 67 +
                 (int)this.pieceType * 13 +
                 (int)this.attackType * 7
               );
    }

    public bool Equals(XAction action)
    {
        return ( (this.srcTile == action.srcTile)             &&
                 (this.destTile == action.destTile)           &&
                 (this.promotionType == action.promotionType) &&
                 (this.pieceType == action.pieceType)         &&
                 (this.attackType == action.attackType)
        );
    }
}

public class XActionUndoData
{
    public UInt64 zobristHash;
    public byte castleSettings;
    public byte halfMoveClock;
    public bool whiteCheck;
    public bool blackCheck;
    public XActionUndoData(UInt64 zobristHash, byte castleSettings, byte halfMoveClock, bool whiteCheck, bool blackCheck)
    {
        this.zobristHash = zobristHash;
        this.castleSettings = castleSettings;
        this.halfMoveClock = halfMoveClock;
        this.whiteCheck = whiteCheck;
        this.blackCheck = blackCheck;
    }
}

