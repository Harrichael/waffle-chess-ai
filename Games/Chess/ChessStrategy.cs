
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ChessStrategy
{

    private static Random rand = new Random();
    static byte QueenVal  = 9;
    static byte RookVal   = 5;
    static byte BishopVal = 3;
    static byte KnightVal = 3;
    static byte PawnVal   = 1;

    public static T RandomSelect<T>(List<T> sequence)
    {
        return sequence[rand.Next(sequence.Count())];
    }

    public static XAction HeuristicSelect(XBoard state, List<XAction> sequence, bool playerIsWhite)
    {
        var bestActions = new List<XAction>();
        Int64 bestH;

        ChessRules.Apply(state, sequence[0]);
        bestH = Heuristic(state, playerIsWhite);
        bestActions.Add(sequence[0]);
        ChessRules.Undo(state, sequence[0]);
        foreach (var action in sequence.Skip(1))
        {
            ChessRules.Apply(state, action);
            Int64 val = Heuristic(state, playerIsWhite);
            if (val > bestH)
            {
                bestH = val;
                bestActions.Clear();
                bestActions.Add(action);
            }
            else if (val == bestH)
            {
                bestActions.Add(action);
            }
            ChessRules.Undo(state, action);
        }

        return RandomSelect(bestActions);
    }

    public static UInt64 CountBits(UInt64 val)
    {
        UInt64 count = 0;
        while (val != 0)
        {
            count = count + 1;
            val = val & (val - 1);
        }
        return count;
    }

    public static Int64 Heuristic(XBoard state, bool playerIsWhite)
    {
        UInt64 whiteMaterial;
        UInt64 blackMaterial;

        { // White Material
            whiteMaterial = PawnVal * CountBits(state.whitePawns);
            whiteMaterial = whiteMaterial + RookVal * CountBits(state.whiteRooks);
            whiteMaterial = whiteMaterial + KnightVal * CountBits(state.whiteKnights);
            whiteMaterial = whiteMaterial + BishopVal * CountBits(state.whiteBishops);
            whiteMaterial = whiteMaterial + QueenVal * CountBits(state.whiteQueens);
        } // End White Material
        { // Black Material
            blackMaterial = PawnVal * CountBits(state.blackPawns);
            blackMaterial = blackMaterial + RookVal * CountBits(state.blackRooks);
            blackMaterial = blackMaterial + KnightVal * CountBits(state.blackKnights);
            blackMaterial = blackMaterial + BishopVal * CountBits(state.blackBishops);
            blackMaterial = blackMaterial + QueenVal * CountBits(state.blackQueens);
        } // End Black Material
        
        if (playerIsWhite)
        {
            return (Int64)(whiteMaterial - blackMaterial);
        }
        {
            return (Int64)(blackMaterial - whiteMaterial);
        }
    }
}
