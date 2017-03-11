
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static ChessRules;

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
        var children = LegalMoves(state);
        var bestChild = children[0];
        state.Apply(bestChild);
        var bestVal = DL_Min(state, depth-1, playerIsWhite);
        state.Undo();
        foreach(var child in children)
        {
            state.Apply(child);
            var val = DL_Min(state, depth-1, playerIsWhite);
            state.Undo();
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

        var children = LegalMoves(state);
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
        state.Undo();

        foreach(var child in children.Skip(1))
        {
            state.Apply(child);
            var hVal = DL_Min(state, depth-1, maxWhite);
            state.Undo();

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

        var children = LegalMoves(state);
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
        state.Undo();

        foreach(var child in children.Skip(1))
        {
            state.Apply(child);
            var hVal = DL_Max(state, depth-1, maxWhite);
            state.Undo();

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
        state.Undo();
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
            state.Undo();
        }

        Console.WriteLine("Evaluation: " + bestH);
        return RandomSelect(bestActions);
    }

    public static Int64 Heuristic(XBoard state, bool playerIsWhite)
    {
        // Potential
        UInt64 whiteMaterial = 0;
        UInt64 blackMaterial = 0;
        { // White Material
            whiteMaterial = whiteMaterial + PawnAdv0Material * BitOps.CountBits(state.whitePawns & Rank2);
            whiteMaterial = whiteMaterial + PawnAdv1Material * BitOps.CountBits(state.whitePawns & Rank3);
            whiteMaterial = whiteMaterial + PawnAdv2Material * BitOps.CountBits(state.whitePawns & Rank4);
            whiteMaterial = whiteMaterial + PawnAdv3Material * BitOps.CountBits(state.whitePawns & Rank5);
            whiteMaterial = whiteMaterial + PawnAdv4Material * BitOps.CountBits(state.whitePawns & Rank6);
            whiteMaterial = whiteMaterial + PawnAdv5Material * BitOps.CountBits(state.whitePawns & Rank7);
            whiteMaterial = whiteMaterial - PawnOutmostFilePenalty * BitOps.CountBits(state.whitePawns & (AFile | HFile));
            whiteMaterial = whiteMaterial - PawnOuterFilePenalty * BitOps.CountBits(state.whitePawns & (BFile | GFile));

            whiteMaterial = whiteMaterial + RookMaterial * BitOps.CountBits(state.whiteRooks);
            whiteMaterial = whiteMaterial + KnightMaterial * BitOps.CountBits(state.whiteKnights);
            whiteMaterial = whiteMaterial + BishopMaterial * BitOps.CountBits(state.whiteBishops);
            whiteMaterial = whiteMaterial + QueenMaterial * BitOps.CountBits(state.whiteQueens);
        } // End White Material
        { // Black Material
            blackMaterial = blackMaterial + PawnAdv0Material * BitOps.CountBits(state.blackPawns & Rank7);
            blackMaterial = blackMaterial + PawnAdv1Material * BitOps.CountBits(state.blackPawns & Rank6);
            blackMaterial = blackMaterial + PawnAdv2Material * BitOps.CountBits(state.blackPawns & Rank5);
            blackMaterial = blackMaterial + PawnAdv3Material * BitOps.CountBits(state.blackPawns & Rank4);
            blackMaterial = blackMaterial + PawnAdv4Material * BitOps.CountBits(state.blackPawns & Rank3);
            blackMaterial = blackMaterial + PawnAdv5Material * BitOps.CountBits(state.blackPawns & Rank2);
            blackMaterial = blackMaterial - PawnOutmostFilePenalty * BitOps.CountBits(state.blackPawns & (AFile | HFile));
            blackMaterial = blackMaterial - PawnOuterFilePenalty * BitOps.CountBits(state.blackPawns & (BFile | GFile));

            blackMaterial = blackMaterial + RookMaterial * BitOps.CountBits(state.blackRooks);
            blackMaterial = blackMaterial + KnightMaterial * BitOps.CountBits(state.blackKnights);
            blackMaterial = blackMaterial + BishopMaterial * BitOps.CountBits(state.blackBishops);
            blackMaterial = blackMaterial + QueenMaterial * BitOps.CountBits(state.blackQueens);
        } // End Black Material

        // Position
        UInt64 whitePosition = 0;
        UInt64 blackPosition = 0;

        UInt64 defendedWhitePositions = 0;
        UInt64 threatenedWhitePositions = 0;
        UInt64 defendedBlackPositions = 0;
        UInt64 threatenedBlackPositions = 0;
        { // White Pawns
            UInt64 pawnAttacks = ((state.whitePawns & NotHFile) << 7) | ((state.whitePawns & NotAFile) << 9);
            threatenedBlackPositions = threatenedBlackPositions | (pawnAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (pawnAttacks & state.whitePieces);
            whitePosition = whitePosition + PawnOpenValue * BitOps.CountBits(pawnAttacks & state.open);

            whitePosition = whitePosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.whiteKing   ) * KingPositionValue;
        }
        { // Black Pawns
            UInt64 pawnAttacks = ((state.blackPawns & NotAFile) >> 7) | ((state.blackPawns & NotHFile) >> 9);
            threatenedWhitePositions = threatenedWhitePositions | (pawnAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (pawnAttacks & state.blackPieces);
            blackPosition = blackPosition + PawnOpenValue * BitOps.CountBits(pawnAttacks & state.open);

            blackPosition = blackPosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + PawnAttackValue * BitOps.CountBits(pawnAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + PawnDefenseValue * BitOps.CountBits(pawnAttacks & state.blackKing   ) * KingPositionValue;
        }
        { // White Knights
            UInt64 knightAttacks = 0;
            UInt64 knights = state.whiteKnights;
            UInt64 knight;
            while(knights != 0)
            {
                knight = BitOps.MSB(knights);
                knights = knights - knight;
                knightAttacks = knightAttacks | getKnightAttacks(knight);
            }
            threatenedBlackPositions = threatenedBlackPositions | (knightAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (knightAttacks & state.whitePieces);
            whitePosition = whitePosition + KnightOpenValue * BitOps.CountBits(knightAttacks & state.open);

            whitePosition = whitePosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.whiteKing   ) * KingPositionValue;
        }
        { // Black Knights
            UInt64 knightAttacks = 0;
            UInt64 knights = state.blackKnights;
            UInt64 knight;
            while(knights != 0)
            {
                knight = BitOps.MSB(knights);
                knights = knights - knight;
                knightAttacks = knightAttacks | getKnightAttacks(knight);
            }
            threatenedWhitePositions = threatenedWhitePositions | (knightAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (knightAttacks & state.blackPieces);
            blackPosition = blackPosition + KnightOpenValue * BitOps.CountBits(knightAttacks & state.open);

            blackPosition = blackPosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + KnightAttackValue * BitOps.CountBits(knightAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + KnightDefenseValue * BitOps.CountBits(knightAttacks & state.blackKing   ) * KingPositionValue;
        }
        { // White Bishops
            UInt64 bishopAttacks = 0;
            UInt64 addBishopAttacks;
            UInt64 bishops = state.whiteBishops;
            // UpLeft
            addBishopAttacks = (bishops << 9) & NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks << 9) & NotHFile;
            }
            // End UpLeft
            // UpRight
            addBishopAttacks = (bishops << 7) & NotAFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks << 7) & NotAFile;
            }
            // End UpRight
            // DownLeft
            addBishopAttacks = (bishops >> 7) & NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks >> 7) & NotHFile;
            }
            // End DownLeft
            // DownRight
            addBishopAttacks = (bishops >> 9) & NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks >> 9) & NotHFile;
            }
            // End DownRight

            threatenedBlackPositions = threatenedBlackPositions | (bishopAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (bishopAttacks & state.whitePieces);
            whitePosition = whitePosition + BishopOpenValue * BitOps.CountBits(bishopAttacks & state.open);

            whitePosition = whitePosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.whiteKing   ) * KingPositionValue;
        } // End White Bishops
        { // Black Bishops
            UInt64 bishopAttacks = 0;
            UInt64 addBishopAttacks;
            UInt64 bishops = state.blackBishops;
            // UpLeft
            addBishopAttacks = (bishops << 9) & NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks << 9) & NotHFile;
            }
            // End UpLeft
            // UpRight
            addBishopAttacks = (bishops << 7) & NotAFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks << 7) & NotAFile;
            }
            // End UpRight
            // DownLeft
            addBishopAttacks = (bishops >> 7) & NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks >> 7) & NotHFile;
            }
            // End DownLeft
            // DownRight
            addBishopAttacks = (bishops >> 9) & NotHFile;
            while(addBishopAttacks != 0)
            {
                bishopAttacks = bishopAttacks | addBishopAttacks;
                addBishopAttacks = addBishopAttacks & state.open;
                addBishopAttacks = (addBishopAttacks >> 9) & NotHFile;
            }
            // End DownRight

            threatenedWhitePositions = threatenedWhitePositions | (bishopAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (bishopAttacks & state.blackPieces);
            blackPosition = blackPosition + BishopOpenValue * BitOps.CountBits(bishopAttacks & state.open);

            blackPosition = blackPosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + BishopAttackValue * BitOps.CountBits(bishopAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + BishopDefenseValue * BitOps.CountBits(bishopAttacks & state.blackKing   ) * KingPositionValue;
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
            addRookAttacks = (rooks << 1) & NotHFile;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = (addRookAttacks << 1) & NotHFile;
            }
            // End Left
            // Right
            addRookAttacks = (rooks >> 1) & NotAFile;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = (addRookAttacks >> 1) & NotAFile;
            }
            // End Right

            threatenedBlackPositions = threatenedBlackPositions | (rookAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (rookAttacks & state.whitePieces);
            whitePosition = whitePosition + RookOpenValue * BitOps.CountBits(rookAttacks & state.open);

            whitePosition = whitePosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.whiteKing   ) * KingPositionValue;
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
            addRookAttacks = (rooks << 1) & NotHFile;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = (addRookAttacks << 1) & NotHFile;
            }
            // End Left
            // Right
            addRookAttacks = (rooks >> 1) & NotAFile;
            while(addRookAttacks != 0)
            {
                rookAttacks = rookAttacks | addRookAttacks;
                addRookAttacks = addRookAttacks & state.open;
                addRookAttacks = (addRookAttacks >> 1) & NotAFile;
            }
            // End Right

            threatenedWhitePositions = threatenedWhitePositions | (rookAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (rookAttacks & state.blackPieces);
            blackPosition = blackPosition + RookOpenValue * BitOps.CountBits(rookAttacks & state.open);

            blackPosition = blackPosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + RookAttackValue * BitOps.CountBits(rookAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + RookDefenseValue * BitOps.CountBits(rookAttacks & state.blackKing   ) * KingPositionValue;
        } // End Black Rooks
        { // White Queens
            UInt64 queenAttacks = 0;
            UInt64 addQueenAttacks;
            UInt64 queens = state.whiteQueens;
            // UpLeft
            addQueenAttacks = (queens << 9) & NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 9) & NotHFile;
            }
            // End UpLeft
            // UpRight
            addQueenAttacks = (queens << 7) & NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 7) & NotAFile;
            }
            // End UpRight
            // DownLeft
            addQueenAttacks = (queens >> 7) & NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 7) & NotHFile;
            }
            // End DownLeft
            // DownRight
            addQueenAttacks = (queens >> 9) & NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 9) & NotAFile;
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
            addQueenAttacks = (queens << 1) & NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 1) & NotHFile;
            }
            // End Left
            // Right
            addQueenAttacks = (queens >> 1) & NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 1) & NotAFile;
            }
            // End Right

            threatenedBlackPositions = threatenedBlackPositions | (queenAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (queenAttacks & state.whitePieces);
            whitePosition = whitePosition + QueenOpenValue * BitOps.CountBits(queenAttacks & state.open);

            whitePosition = whitePosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.whiteKing   ) * KingPositionValue;
        } // End White Queens
        { // Black Queens
            UInt64 queenAttacks = 0;
            UInt64 addQueenAttacks;
            UInt64 queens = state.blackQueens;
            // UpLeft
            addQueenAttacks = (queens << 9) & NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 9) & NotHFile;
            }
            // End UpLeft
            // UpRight
            addQueenAttacks = (queens << 7) & NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 7) & NotAFile;
            }
            // End UpRight
            // DownLeft
            addQueenAttacks = (queens >> 7) & NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 7) & NotHFile;
            }
            // End DownLeft
            // DownRight
            addQueenAttacks = (queens >> 9) & NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 9) & NotAFile;
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
            addQueenAttacks = (queens << 1) & NotHFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks << 1) & NotHFile;
            }
            // End Left
            // Right
            addQueenAttacks = (queens >> 1) & NotAFile;
            while(addQueenAttacks != 0)
            {
                queenAttacks = queenAttacks | addQueenAttacks;
                addQueenAttacks = addQueenAttacks & state.open;
                addQueenAttacks = (addQueenAttacks >> 1) & NotAFile;
            }
            // End Right

            threatenedWhitePositions = threatenedWhitePositions | (queenAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (queenAttacks & state.blackPieces);
            blackPosition = blackPosition + QueenOpenValue * BitOps.CountBits(queenAttacks & state.open);

            blackPosition = blackPosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + QueenAttackValue * BitOps.CountBits(queenAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + QueenDefenseValue * BitOps.CountBits(queenAttacks & state.blackKing   ) * KingPositionValue;
        } // End Black Queens
        { // White King
            UInt64 kingAttacks = getKingAttacks(state.whiteKing);
            
            threatenedBlackPositions = threatenedBlackPositions | (kingAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (kingAttacks & state.whitePieces);
            whitePosition = whitePosition + KingOpenValue * BitOps.CountBits(kingAttacks & state.open);

            whitePosition = whitePosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.blackPawns  ) * PawnPositionValue;
            whitePosition = whitePosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.blackRooks  ) * RookPositionValue;
            whitePosition = whitePosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.blackKnights) * KnightPositionValue;
            whitePosition = whitePosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.blackBishops) * BishopPositionValue;
            whitePosition = whitePosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.blackQueens ) * QueenPositionValue;
            whitePosition = whitePosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.blackKing   ) * KingPositionValue;

            whitePosition = whitePosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.whitePawns  ) * PawnPositionValue;
            whitePosition = whitePosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.whiteRooks  ) * RookPositionValue;
            whitePosition = whitePosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.whiteKnights) * KnightPositionValue;
            whitePosition = whitePosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.whiteBishops) * BishopPositionValue;
            whitePosition = whitePosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.whiteQueens ) * QueenPositionValue;
            whitePosition = whitePosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.whiteKing   ) * KingPositionValue;
        } // End White King
        { // Black King
            UInt64 kingAttacks = getKingAttacks(state.blackKing);
            
            threatenedWhitePositions = threatenedWhitePositions | (kingAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (kingAttacks & state.blackPieces);
            blackPosition = blackPosition + KingOpenValue * BitOps.CountBits(kingAttacks & state.open);

            blackPosition = blackPosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.whitePawns  ) * PawnPositionValue;
            blackPosition = blackPosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.whiteRooks  ) * RookPositionValue;
            blackPosition = blackPosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.whiteKnights) * KnightPositionValue;
            blackPosition = blackPosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.whiteBishops) * BishopPositionValue;
            blackPosition = blackPosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.whiteQueens ) * QueenPositionValue;
            blackPosition = blackPosition + KingAttackValue * BitOps.CountBits(kingAttacks & state.whiteKing   ) * KingPositionValue;

            blackPosition = blackPosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.blackPawns  ) * PawnPositionValue;
            blackPosition = blackPosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.blackRooks  ) * RookPositionValue;
            blackPosition = blackPosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.blackKnights) * KnightPositionValue;
            blackPosition = blackPosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.blackBishops) * BishopPositionValue;
            blackPosition = blackPosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.blackQueens ) * QueenPositionValue;
            blackPosition = blackPosition + KingDefenseValue * BitOps.CountBits(kingAttacks & state.blackKing   ) * KingPositionValue;
        } // End Black King

        threatenedWhitePositions = threatenedWhitePositions & ~defendedWhitePositions;
        threatenedBlackPositions = threatenedBlackPositions & ~defendedBlackPositions;
        whitePosition += defendedBonus * BitOps.CountBits(defendedWhitePositions) - threatenedPenalty * BitOps.CountBits(threatenedWhitePositions);
        blackPosition += defendedBonus * BitOps.CountBits(defendedBlackPositions) - threatenedPenalty * BitOps.CountBits(threatenedBlackPositions);
        
        if (playerIsWhite)
        {
            return (Int64)(whiteMaterial + whitePosition - blackMaterial - blackPosition);
        }
        {
            return (Int64)(blackMaterial + blackPosition - whiteMaterial - whitePosition);
        }
    }
}
