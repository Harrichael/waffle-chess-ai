
using System;
using System.Collections.Generic;
using System.Linq;

public static class Zobrist
{
    private static Random rand = new Random(0x16C8E3B0);

    private static UInt64 zobristKeyGen()
    {
        return (UInt64)rand.Next(Int32.MinValue, Int32.MaxValue) | ((UInt64)rand.Next(Int32.MinValue, Int32.MaxValue) << 32);
    }

    private static Dictionary<UInt64, UInt64> zobristBoardKeyGen()
    {
        return Enumerable.Range(0, 64).ToDictionary(i => (UInt64)1 << i, i => zobristKeyGen());
    }

    public static Dictionary<UInt64, UInt64> whitePawn   = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> whiteRook   = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> whiteKnight = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> whiteBishop = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> whiteQueen  = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> whiteKing   = zobristBoardKeyGen();

    public static Dictionary<UInt64, UInt64> blackPawn   = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> blackRook   = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> blackKnight = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> blackBishop = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> blackQueen  = zobristBoardKeyGen();
    public static Dictionary<UInt64, UInt64> blackKing   = zobristBoardKeyGen();

    public static Dictionary<UInt64, UInt64> enPass = zobristBoardKeyGen(); // TODO: use file! locality caching could be bad here! 

    public static UInt64 turnIsWhite = zobristKeyGen();
    public static UInt64 whiteCastleKS = zobristKeyGen();
    public static UInt64 whiteCastleQS = zobristKeyGen();
    public static UInt64 blackCastleKS = zobristKeyGen();
    public static UInt64 blackCastleQS = zobristKeyGen();
}
