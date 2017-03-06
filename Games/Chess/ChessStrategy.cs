
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ChessStrategy
{

    private static Random rand = new Random();
    static byte QueenMaterial  = 90;
    static byte RookMaterial   = 50;
    static byte BishopMaterial = 35;
    static byte KnightMaterial = 30;
    static byte PawnMaterial   = 10;
    static byte KingPositionValue   = 12;
    static byte QueenPositionValue  = 9;
    static byte RookPositionValue   = 5;
    static byte BishopPositionValue = 3;
    static byte KnightPositionValue = 3;
    static byte PawnPositionValue   = 2;
    static byte OpenPositionValue   = 1;
    static byte KingAttackValue   = 1;
    static byte QueenAttackValue  = 3;
    static byte RookAttackValue   = 5;
    static byte BishopAttackValue = 9;
    static byte KnightAttackValue = 9;
    static byte PawnAttackValue   = 12;
    static byte KingDefenseValue   = 1;
    static byte QueenDefenseValue  = 3;
    static byte RookDefenseValue   = 5;
    static byte BishopDefenseValue = 9;
    static byte KnightDefenseValue = 9;
    static byte PawnDefenseValue   = 12;

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
        // Potential
        UInt64 whiteMaterial;
        UInt64 blackMaterial;
        { // White Material
            whiteMaterial = PawnMaterial * CountBits(state.whitePawns);
            whiteMaterial = whiteMaterial + RookMaterial * CountBits(state.whiteRooks);
            whiteMaterial = whiteMaterial + KnightMaterial * CountBits(state.whiteKnights);
            whiteMaterial = whiteMaterial + BishopMaterial * CountBits(state.whiteBishops);
            whiteMaterial = whiteMaterial + QueenMaterial * CountBits(state.whiteQueens);
        } // End White Material
        { // Black Material
            blackMaterial = PawnMaterial * CountBits(state.blackPawns);
            blackMaterial = blackMaterial + RookMaterial * CountBits(state.blackRooks);
            blackMaterial = blackMaterial + KnightMaterial * CountBits(state.blackKnights);
            blackMaterial = blackMaterial + BishopMaterial * CountBits(state.blackBishops);
            blackMaterial = blackMaterial + QueenMaterial * CountBits(state.blackQueens);
        } // End Black Material

        // Position
        UInt64 whitePosition = 0;
        UInt64 blackPosition = 0;
        { // White Pawns
            UInt64 pawnAttacks = ((state.whitePawns & ChessRules.NotHFile) << 7) | ((state.whitePawns & ChessRules.NotAFile) << 9);
            whitePosition = whitePosition + PawnAttackValue * CountBits(pawnAttacks & state.open        ) * OpenPositionValue;

            whitePosition = whitePosition + PawnAttackValue * CountBits(pawnAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + PawnAttackValue * CountBits(pawnAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + PawnAttackValue * CountBits(pawnAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + PawnAttackValue * CountBits(pawnAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + PawnAttackValue * CountBits(pawnAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + PawnAttackValue * CountBits(pawnAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + PawnDefenseValue * CountBits(pawnAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * CountBits(pawnAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * CountBits(pawnAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * CountBits(pawnAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * CountBits(pawnAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * CountBits(pawnAttacks & state.whiteKing   ) * KingPositionValue;
        }
        { // Black Pawns
            UInt64 pawnAttacks = ((state.blackPawns & ChessRules.NotAFile) >> 7) | ((state.blackPawns & ChessRules.NotHFile) >> 9);
            blackPosition = blackPosition + PawnAttackValue * CountBits(pawnAttacks & state.open        ) * OpenPositionValue;

            blackPosition = blackPosition + PawnAttackValue * CountBits(pawnAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + PawnAttackValue * CountBits(pawnAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + PawnAttackValue * CountBits(pawnAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + PawnAttackValue * CountBits(pawnAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + PawnAttackValue * CountBits(pawnAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + PawnAttackValue * CountBits(pawnAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + PawnDefenseValue * CountBits(pawnAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * CountBits(pawnAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * CountBits(pawnAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * CountBits(pawnAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * CountBits(pawnAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * CountBits(pawnAttacks & state.blackKing   ) * KingPositionValue;
        }
        { // White Knights
            UInt64 knightAttacks = 0;
            UInt64 knights = state.whiteKnights;
            UInt64 knight;
            while(knights != 0)
            {
                knight = ChessRules.MSB(knights);
                knights = knights - knight;
                knightAttacks = knightAttacks | ChessRules.getKnightAttacks(knight);
            }
            whitePosition = whitePosition + KnightAttackValue * CountBits(knightAttacks & state.open        ) * OpenPositionValue;

            whitePosition = whitePosition + KnightAttackValue * CountBits(knightAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + KnightAttackValue * CountBits(knightAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + KnightAttackValue * CountBits(knightAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + KnightAttackValue * CountBits(knightAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + KnightAttackValue * CountBits(knightAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + KnightAttackValue * CountBits(knightAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + KnightDefenseValue * CountBits(knightAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * CountBits(knightAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * CountBits(knightAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * CountBits(knightAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * CountBits(knightAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * CountBits(knightAttacks & state.whiteKing   ) * KingPositionValue;
        }
        { // Black Knights
            UInt64 knightAttacks = 0;
            UInt64 knights = state.blackKnights;
            UInt64 knight;
            while(knights != 0)
            {
                knight = ChessRules.MSB(knights);
                knights = knights - knight;
                knightAttacks = knightAttacks | ChessRules.getKnightAttacks(knight);
            }
            blackPosition = blackPosition + KnightAttackValue * CountBits(knightAttacks & state.open        ) * OpenPositionValue;

            blackPosition = blackPosition + KnightAttackValue * CountBits(knightAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + KnightAttackValue * CountBits(knightAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + KnightAttackValue * CountBits(knightAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + KnightAttackValue * CountBits(knightAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + KnightAttackValue * CountBits(knightAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + KnightAttackValue * CountBits(knightAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + KnightDefenseValue * CountBits(knightAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * CountBits(knightAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * CountBits(knightAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * CountBits(knightAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * CountBits(knightAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * CountBits(knightAttacks & state.blackKing   ) * KingPositionValue;
        }
        { // White Bishops
            UInt64 bishopAttacks = 0;
            UInt64 addBishopAttacks;
            UInt64 bishops = state.whiteBishops;
            UInt64 bishop;
            // UpLeft
            addBishopAttacks = (bishops << 9) & ChessRules.NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks << 9) & ChessRules.NotHFile;
            }
            // End UpLeft
            // UpRight
            addBishopAttacks = (bishops << 7) & ChessRules.NotAFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks << 7) & ChessRules.NotAFile;
            }
            // End UpRight
            // DownLeft
            addBishopAttacks = (bishops >> 7) & ChessRules.NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks >> 7) & ChessRules.NotHFile;
            }
            // End DownLeft
            // DownRight
            addBishopAttacks = (bishops >> 9) & ChessRules.NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks >> 9) & ChessRules.NotHFile;
            }
            // End DownRight

            whitePosition = whitePosition + BishopAttackValue * CountBits(bishopAttacks & state.open        ) * OpenPositionValue;

            whitePosition = whitePosition + BishopAttackValue * CountBits(bishopAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + BishopAttackValue * CountBits(bishopAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + BishopAttackValue * CountBits(bishopAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + BishopAttackValue * CountBits(bishopAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + BishopAttackValue * CountBits(bishopAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + BishopAttackValue * CountBits(bishopAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + BishopDefenseValue * CountBits(bishopAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * CountBits(bishopAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * CountBits(bishopAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * CountBits(bishopAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * CountBits(bishopAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * CountBits(bishopAttacks & state.whiteKing   ) * KingPositionValue;
        } // End White Bishops
        { // Black Bishops
            UInt64 bishopAttacks = 0;
            UInt64 addBishopAttacks;
            UInt64 bishops = state.blackBishops;
            UInt64 bishop;
            // UpLeft
            addBishopAttacks = (bishops << 9) & ChessRules.NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks << 9) & ChessRules.NotHFile;
            }
            // End UpLeft
            // UpRight
            addBishopAttacks = (bishops << 7) & ChessRules.NotAFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks << 7) & ChessRules.NotAFile;
            }
            // End UpRight
            // DownLeft
            addBishopAttacks = (bishops >> 7) & ChessRules.NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks >> 7) & ChessRules.NotHFile;
            }
            // End DownLeft
            // DownRight
            addBishopAttacks = (bishops >> 9) & ChessRules.NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks >> 9) & ChessRules.NotHFile;
            }
            // End DownRight

            blackPosition = blackPosition + BishopAttackValue * CountBits(bishopAttacks & state.open        ) * OpenPositionValue;

            blackPosition = blackPosition + BishopAttackValue * CountBits(bishopAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + BishopAttackValue * CountBits(bishopAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + BishopAttackValue * CountBits(bishopAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + BishopAttackValue * CountBits(bishopAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + BishopAttackValue * CountBits(bishopAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + BishopAttackValue * CountBits(bishopAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + BishopDefenseValue * CountBits(bishopAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * CountBits(bishopAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * CountBits(bishopAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * CountBits(bishopAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * CountBits(bishopAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * CountBits(bishopAttacks & state.blackKing   ) * KingPositionValue;
        } // End Black Bishops
        { // White Rooks
            UInt64 validMove = state.open | state.blackPieces;
            UInt64 rookAttacks = 0;
            UInt64 addRookAttacks;
            UInt64 rooks = state.whiteRooks;
            UInt64 rook;
            // Up
            addRookAttacks = rooks << 8;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = addRookAttacks << 8;
            }
            // End Up
            // Down
            addRookAttacks = rooks >> 8;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = addRookAttacks >> 8;
            }
            // End Down
            // Left
            addRookAttacks = (rooks << 1) & ChessRules.NotHFile;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = (addRookAttacks << 1) & ChessRules.NotHFile;
            }
            // End Left
            // Right
            addRookAttacks = (rooks >> 1) & ChessRules.NotAFile;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = (addRookAttacks >> 1) & ChessRules.NotAFile;
            }
            // End Right

            whitePosition = whitePosition + RookAttackValue * CountBits(rookAttacks & state.open        ) * OpenPositionValue;

            whitePosition = whitePosition + RookAttackValue * CountBits(rookAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + RookAttackValue * CountBits(rookAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + RookAttackValue * CountBits(rookAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + RookAttackValue * CountBits(rookAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + RookAttackValue * CountBits(rookAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + RookAttackValue * CountBits(rookAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + RookDefenseValue * CountBits(rookAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + RookDefenseValue * CountBits(rookAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + RookDefenseValue * CountBits(rookAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + RookDefenseValue * CountBits(rookAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + RookDefenseValue * CountBits(rookAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + RookDefenseValue * CountBits(rookAttacks & state.whiteKing   ) * KingPositionValue;
        } // End White Rooks
        { // Black Rooks
            UInt64 validMove = state.open | state.whitePieces;
            UInt64 rookAttacks = 0;
            UInt64 addRookAttacks;
            UInt64 rooks = state.blackRooks;
            UInt64 rook;
            // Up
            addRookAttacks = rooks << 8;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = addRookAttacks << 8;
            }
            // End Up
            // Down
            addRookAttacks = rooks >> 8;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = addRookAttacks >> 8;
            }
            // End Down
            // Left
            addRookAttacks = (rooks << 1) & ChessRules.NotHFile;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = (addRookAttacks << 1) & ChessRules.NotHFile;
            }
            // End Left
            // Right
            addRookAttacks = (rooks >> 1) & ChessRules.NotAFile;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = (addRookAttacks >> 1) & ChessRules.NotAFile;
            }
            // End Right

            blackPosition = blackPosition + RookAttackValue * CountBits(rookAttacks & state.open        ) * OpenPositionValue;

            blackPosition = blackPosition + RookAttackValue * CountBits(rookAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + RookAttackValue * CountBits(rookAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + RookAttackValue * CountBits(rookAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + RookAttackValue * CountBits(rookAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + RookAttackValue * CountBits(rookAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + RookAttackValue * CountBits(rookAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + RookDefenseValue * CountBits(rookAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + RookDefenseValue * CountBits(rookAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + RookDefenseValue * CountBits(rookAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + RookDefenseValue * CountBits(rookAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + RookDefenseValue * CountBits(rookAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + RookDefenseValue * CountBits(rookAttacks & state.blackKing   ) * KingPositionValue;
        } // End Black Rooks
        
        if (playerIsWhite)
        {
            return (Int64)(whiteMaterial + whitePosition - blackMaterial - blackPosition);
        }
        {
            return (Int64)(blackMaterial + blackPosition - whiteMaterial - whitePosition);
        }
    }
}
