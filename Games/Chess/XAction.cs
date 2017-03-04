
using System;
using System.Collections.Generic;
using System.Linq;

public class XAction
{
    public UInt64 srcTile;
    public UInt64 destTile;
    public string promotionType;

    public XAction() {}
    public XAction(UInt64 src, UInt64 dest, string promote="")
    {
        this.srcTile = src;
        this.destTile = dest;
        this.promotionType = promote;
    }
}

