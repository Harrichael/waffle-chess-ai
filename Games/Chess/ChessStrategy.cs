
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ChessStrategy
{

    private static Random rand = new Random();
    static byte QueenMaterial  = 125;
    static byte RookMaterial   = 70;
    static byte BishopMaterial = 60;
    static byte KnightMaterial = 55;

    static byte threatenedPenalty = 20;
    static byte defendedBonus = 5;

    static byte PawnAdv0Material = 10;
    static byte PawnAdv1Material = 20;
    static byte PawnAdv2Material = 40;
    static byte PawnAdv3Material = 45;
    static byte PawnAdv4Material = 50;
    static byte PawnAdv5Material = 55;
    static byte PawnOutmostFilePenalty = 30;
    static byte PawnOuterFilePenalty = 20;

    static byte KingPositionValue   = 12;
    static byte QueenPositionValue  = 9;
    static byte RookPositionValue   = 5;
    static byte BishopPositionValue = 3;
    static byte KnightPositionValue = 3;
    static byte PawnPositionValue   = 2;

    static byte KingOpenValue   = 1;
    static byte QueenOpenValue  = 3;
    static byte RookOpenValue   = 4;
    static byte BishopOpenValue = 10;
    static byte KnightOpenValue = 1;
    static byte PawnOpenValue   = 3;

    static byte KingAttackValue   = 1;
    static byte QueenAttackValue  = 6;
    static byte RookAttackValue   = 5;
    static byte BishopAttackValue = 9;
    static byte KnightAttackValue = 8;
    static byte PawnAttackValue   = 12;

    static byte KingDefenseValue   = 1;
    static byte QueenDefenseValue  = 4;
    static byte RookDefenseValue   = 5;
    static byte BishopDefenseValue = 7;
    static byte KnightDefenseValue = 2;
    static byte PawnDefenseValue   = 10;

    public static T RandomSelect<T>(List<T> sequence)
    {
        return sequence[rand.Next(sequence.Count())];
    }

    public static XAction DL_Minimax(XBoard state, int depth, bool playerIsWhite)
    {
        var children = ChessRules.LegalMoves(state);
        var bestChild = children[0];
        state.Apply(bestChild);
        var bestVal = DL_Min(state, depth-1, playerIsWhite);
        state.Undo(bestChild);
        foreach(var child in children)
        {
            state.Apply(child);
            var val = DL_Min(state, depth-1, playerIsWhite);
            state.Undo(child);
            if (val > bestVal)
            {
                bestChild = child;
                bestVal = val;
            }
        }
        Console.WriteLine("Final Evaluation: " + bestVal);
        return bestChild;
    }

    private static Int64 DL_Max(XBoard state, int depth, bool maxWhite)
    {
        if (depth == 0)
        {
            return Heuristic(state, maxWhite);
        }

        var children = ChessRules.LegalMoves(state);
        if (children.Count() == 0) // Is terminal
        {
            if (state.turnIsWhite && state.whiteCheck) // Check Mate
            {
                return Int64.MinValue;
            } else if (state.blackCheck && !state.turnIsWhite)
            {
                return Int64.MinValue;
            } else { // Stalemate
                return 0;
            }
        }

        state.Apply(children[0]);
        var maxH = DL_Min(state, depth-1, maxWhite);
        state.Undo(children[0]);

        foreach(var child in children.Skip(1))
        {
            state.Apply(child);
            var hVal = DL_Min(state, depth-1, maxWhite);
            state.Undo(child);

            if (hVal > maxH)
            {
                maxH = hVal;
            }
        }
        return maxH;
    }

    private static Int64 DL_Min(XBoard state, int depth, bool maxWhite)
    {
        if (depth == 0)
        {
            return Heuristic(state, maxWhite);
        }

        var children = ChessRules.LegalMoves(state);
        if (children.Count() == 0) // Is terminal
        {
            if (state.whiteCheck && state.turnIsWhite) // Check Mate
            {
                return Int64.MaxValue;
            } else if (state.blackCheck && !state.turnIsWhite)
            {
                return Int64.MaxValue;
            } else { // Stalemate
                return 0;
            }
        }

        state.Apply(children[0]);
        var minH = DL_Max(state, depth-1, maxWhite);
        state.Undo(children[0]);

        foreach(var child in children.Skip(1))
        {
            state.Apply(child);
            var hVal = DL_Max(state, depth-1, maxWhite);
            state.Undo(child);

            if (hVal < minH)
            {
                minH = hVal;
            }
        }
        return minH;
    }

    public static XAction HeuristicSelect(XBoard state, List<XAction> sequence, bool playerIsWhite)
    {
        var bestActions = new List<XAction>();
        Int64 bestH;

        state.Apply(sequence[0]);
        bestH = Heuristic(state, playerIsWhite);
        bestActions.Add(sequence[0]);
        state.Undo(sequence[0]);
        foreach (var action in sequence.Skip(1))
        {
            state.Apply(action);
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
            state.Undo(action);
        }

        Console.WriteLine("Evaluation: " + bestH);
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
        UInt64 whiteMaterial = 0;
        UInt64 blackMaterial = 0;
        { // White Material
            whiteMaterial = whiteMaterial + PawnAdv0Material * CountBits(state.whitePawns & ChessRules.Rank2);
            whiteMaterial = whiteMaterial + PawnAdv1Material * CountBits(state.whitePawns & ChessRules.Rank3);
            whiteMaterial = whiteMaterial + PawnAdv2Material * CountBits(state.whitePawns & ChessRules.Rank4);
            whiteMaterial = whiteMaterial + PawnAdv3Material * CountBits(state.whitePawns & ChessRules.Rank5);
            whiteMaterial = whiteMaterial + PawnAdv4Material * CountBits(state.whitePawns & ChessRules.Rank6);
            whiteMaterial = whiteMaterial + PawnAdv5Material * CountBits(state.whitePawns & ChessRules.Rank7);
            whiteMaterial = whiteMaterial - PawnOutmostFilePenalty * CountBits(state.whitePawns & (ChessRules.AFile | ChessRules.HFile));
            whiteMaterial = whiteMaterial - PawnOuterFilePenalty * CountBits(state.whitePawns & (ChessRules.BFile | ChessRules.GFile));

            whiteMaterial = whiteMaterial + RookMaterial * CountBits(state.whiteRooks);
            whiteMaterial = whiteMaterial + KnightMaterial * CountBits(state.whiteKnights);
            whiteMaterial = whiteMaterial + BishopMaterial * CountBits(state.whiteBishops);
            whiteMaterial = whiteMaterial + QueenMaterial * CountBits(state.whiteQueens);
        } // End White Material
        { // Black Material
            blackMaterial = blackMaterial + PawnAdv0Material * CountBits(state.blackPawns & ChessRules.Rank7);
            blackMaterial = blackMaterial + PawnAdv1Material * CountBits(state.blackPawns & ChessRules.Rank6);
            blackMaterial = blackMaterial + PawnAdv2Material * CountBits(state.blackPawns & ChessRules.Rank5);
            blackMaterial = blackMaterial + PawnAdv3Material * CountBits(state.blackPawns & ChessRules.Rank4);
            blackMaterial = blackMaterial + PawnAdv4Material * CountBits(state.blackPawns & ChessRules.Rank3);
            blackMaterial = blackMaterial + PawnAdv5Material * CountBits(state.blackPawns & ChessRules.Rank2);
            blackMaterial = blackMaterial - PawnOutmostFilePenalty * CountBits(state.blackPawns & (ChessRules.AFile | ChessRules.HFile));
            blackMaterial = blackMaterial - PawnOuterFilePenalty * CountBits(state.blackPawns & (ChessRules.BFile | ChessRules.GFile));

            blackMaterial = blackMaterial + RookMaterial * CountBits(state.blackRooks);
            blackMaterial = blackMaterial + KnightMaterial * CountBits(state.blackKnights);
            blackMaterial = blackMaterial + BishopMaterial * CountBits(state.blackBishops);
            blackMaterial = blackMaterial + QueenMaterial * CountBits(state.blackQueens);
        } // End Black Material

        // Position
        UInt64 whitePosition = 0;
        UInt64 blackPosition = 0;

        UInt64 defendedWhitePositions = 0;
        UInt64 threatenedWhitePositions = 0;
        UInt64 defendedBlackPositions = 0;
        UInt64 threatenedBlackPositions = 0;
        { // White Pawns
            UInt64 pawnAttacks = ((state.whitePawns & ChessRules.NotHFile) << 7) | ((state.whitePawns & ChessRules.NotAFile) << 9);
            threatenedBlackPositions = threatenedBlackPositions | (pawnAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (pawnAttacks & state.whitePieces);
            whitePosition = whitePosition + PawnOpenValue * CountBits(pawnAttacks & state.open);

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
            threatenedWhitePositions = threatenedWhitePositions | (pawnAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (pawnAttacks & state.blackPieces);
            blackPosition = blackPosition + PawnOpenValue * CountBits(pawnAttacks & state.open);

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
            threatenedBlackPositions = threatenedBlackPositions | (knightAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (knightAttacks & state.whitePieces);
            whitePosition = whitePosition + KnightOpenValue * CountBits(knightAttacks & state.open);

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
            threatenedWhitePositions = threatenedWhitePositions | (knightAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (knightAttacks & state.blackPieces);
            blackPosition = blackPosition + KnightOpenValue * CountBits(knightAttacks & state.open);

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

            threatenedBlackPositions = threatenedBlackPositions | (bishopAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (bishopAttacks & state.whitePieces);
            whitePosition = whitePosition + BishopOpenValue * CountBits(bishopAttacks & state.open);

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

            threatenedWhitePositions = threatenedWhitePositions | (bishopAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (bishopAttacks & state.blackPieces);
            blackPosition = blackPosition + BishopOpenValue * CountBits(bishopAttacks & state.open);

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

            threatenedBlackPositions = threatenedBlackPositions | (rookAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (rookAttacks & state.whitePieces);
            whitePosition = whitePosition + RookOpenValue * CountBits(rookAttacks & state.open);

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

            threatenedWhitePositions = threatenedWhitePositions | (rookAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (rookAttacks & state.blackPieces);
            blackPosition = blackPosition + RookOpenValue * CountBits(rookAttacks & state.open);

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
        { // White Queens
            UInt64 queenAttacks = 0;
            UInt64 addQueenAttacks;
            UInt64 queens = state.whiteQueens;
            // UpLeft
            addQueenAttacks = (queens << 9) & ChessRules.NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 9) & ChessRules.NotHFile;
            }
            // End UpLeft
            // UpRight
            addQueenAttacks = (queens << 7) & ChessRules.NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 7) & ChessRules.NotAFile;
            }
            // End UpRight
            // DownLeft
            addQueenAttacks = (queens >> 7) & ChessRules.NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 7) & ChessRules.NotHFile;
            }
            // End DownLeft
            // DownRight
            addQueenAttacks = (queens >> 9) & ChessRules.NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 9) & ChessRules.NotAFile;
            }
            // End DownRight
            // Up
            addQueenAttacks = queens << 8;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = addQueenAttacks << 8;
            }
            // End Up
            // Down
            addQueenAttacks = queens >> 8;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = addQueenAttacks >> 8;
            }
            // End Down
            // Left
            addQueenAttacks = (queens << 1) & ChessRules.NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 1) & ChessRules.NotHFile;
            }
            // End Left
            // Right
            addQueenAttacks = (queens >> 1) & ChessRules.NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 1) & ChessRules.NotAFile;
            }
            // End Right

            threatenedBlackPositions = threatenedBlackPositions | (queenAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (queenAttacks & state.whitePieces);
            whitePosition = whitePosition + QueenOpenValue * CountBits(queenAttacks & state.open);

            whitePosition = whitePosition + QueenAttackValue * CountBits(queenAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + QueenAttackValue * CountBits(queenAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + QueenAttackValue * CountBits(queenAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + QueenAttackValue * CountBits(queenAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + QueenAttackValue * CountBits(queenAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + QueenAttackValue * CountBits(queenAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + QueenDefenseValue * CountBits(queenAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * CountBits(queenAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * CountBits(queenAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * CountBits(queenAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * CountBits(queenAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * CountBits(queenAttacks & state.whiteKing   ) * KingPositionValue;
        } // End White Queens
        { // Black Queens
            UInt64 queenAttacks = 0;
            UInt64 addQueenAttacks;
            UInt64 queens = state.blackQueens;
            // UpLeft
            addQueenAttacks = (queens << 9) & ChessRules.NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 9) & ChessRules.NotHFile;
            }
            // End UpLeft
            // UpRight
            addQueenAttacks = (queens << 7) & ChessRules.NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 7) & ChessRules.NotAFile;
            }
            // End UpRight
            // DownLeft
            addQueenAttacks = (queens >> 7) & ChessRules.NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 7) & ChessRules.NotHFile;
            }
            // End DownLeft
            // DownRight
            addQueenAttacks = (queens >> 9) & ChessRules.NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 9) & ChessRules.NotAFile;
            }
            // End DownRight
            // Up
            addQueenAttacks = queens << 8;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = addQueenAttacks << 8;
            }
            // End Up
            // Down
            addQueenAttacks = queens >> 8;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = addQueenAttacks >> 8;
            }
            // End Down
            // Left
            addQueenAttacks = (queens << 1) & ChessRules.NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 1) & ChessRules.NotHFile;
            }
            // End Left
            // Right
            addQueenAttacks = (queens >> 1) & ChessRules.NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 1) & ChessRules.NotAFile;
            }
            // End Right

            threatenedWhitePositions = threatenedWhitePositions | (queenAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (queenAttacks & state.blackPieces);
            blackPosition = blackPosition + QueenOpenValue * CountBits(queenAttacks & state.open);

            blackPosition = blackPosition + QueenAttackValue * CountBits(queenAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + QueenAttackValue * CountBits(queenAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + QueenAttackValue * CountBits(queenAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + QueenAttackValue * CountBits(queenAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + QueenAttackValue * CountBits(queenAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + QueenAttackValue * CountBits(queenAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + QueenDefenseValue * CountBits(queenAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * CountBits(queenAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * CountBits(queenAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * CountBits(queenAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * CountBits(queenAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * CountBits(queenAttacks & state.blackKing   ) * KingPositionValue;
        } // End Black Queens
        { // White King
            UInt64 kingAttacks = ChessRules.getKingAttacks(state.whiteKing);
            
            threatenedBlackPositions = threatenedBlackPositions | (kingAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (kingAttacks & state.whitePieces);
            whitePosition = whitePosition + KingOpenValue * CountBits(kingAttacks & state.open);

            whitePosition = whitePosition + KingAttackValue * CountBits(kingAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + KingAttackValue * CountBits(kingAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + KingAttackValue * CountBits(kingAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + KingAttackValue * CountBits(kingAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + KingAttackValue * CountBits(kingAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + KingAttackValue * CountBits(kingAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + KingDefenseValue * CountBits(kingAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + KingDefenseValue * CountBits(kingAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + KingDefenseValue * CountBits(kingAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + KingDefenseValue * CountBits(kingAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + KingDefenseValue * CountBits(kingAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + KingDefenseValue * CountBits(kingAttacks & state.whiteKing   ) * KingPositionValue;
        } // End White King
        { // Black King
            UInt64 kingAttacks = ChessRules.getKingAttacks(state.blackKing);
            
            threatenedWhitePositions = threatenedWhitePositions | (kingAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (kingAttacks & state.blackPieces);
            blackPosition = blackPosition + KingOpenValue * CountBits(kingAttacks & state.open);

            blackPosition = blackPosition + KingAttackValue * CountBits(kingAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + KingAttackValue * CountBits(kingAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + KingAttackValue * CountBits(kingAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + KingAttackValue * CountBits(kingAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + KingAttackValue * CountBits(kingAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + KingAttackValue * CountBits(kingAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + KingDefenseValue * CountBits(kingAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + KingDefenseValue * CountBits(kingAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + KingDefenseValue * CountBits(kingAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + KingDefenseValue * CountBits(kingAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + KingDefenseValue * CountBits(kingAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + KingDefenseValue * CountBits(kingAttacks & state.blackKing   ) * KingPositionValue;
        } // End Black King

        threatenedWhitePositions = threatenedWhitePositions & ~defendedWhitePositions;
        threatenedBlackPositions = threatenedBlackPositions & ~defendedBlackPositions;
        whitePosition += defendedBonus * CountBits(defendedWhitePositions) - threatenedPenalty * CountBits(threatenedWhitePositions);
        blackPosition += defendedBonus * CountBits(defendedBlackPositions) - threatenedPenalty * CountBits(threatenedBlackPositions);
        
        if (playerIsWhite)
        {
            return (Int64)(whiteMaterial + whitePosition - blackMaterial - blackPosition);
        }
        {
            return (Int64)(blackMaterial + blackPosition - whiteMaterial - whitePosition);
        }
    }
}
