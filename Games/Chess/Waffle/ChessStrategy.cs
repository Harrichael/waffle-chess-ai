
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static ChessRules;

public static class ChessStrategy
{
    private static Int64 WinEval = Int64.MaxValue;
    private static Int64 LoseEval = Int64.MinValue;

    private static Random rand = new Random();
    static byte materialWeight = 13;
    static byte positionWeight = 2;

    static byte QueenMaterial  = 120;
    static byte RookMaterial   = 70;
    static byte BishopMaterial = 60;
    static byte KnightMaterial = 55;

    static byte threatenedPenalty = 80;
    static byte defendedBonus = 25;

    static byte PawnAdv0Material = 10;
    static byte PawnAdv1Material = 15;
    static byte PawnAdv2Material = 20;
    static byte PawnAdv3Material = 25;
    static byte PawnAdv4Material = 30;
    static byte PawnAdv5Material = 45;
    static byte PawnOutmostFilePenalty = 50;
    static byte PawnOuterFilePenalty = 20;

    static byte KingOpenValue   = 2;
    static byte QueenOpenValue  = 7;
    static byte RookOpenValue   = 4;
    static byte BishopOpenValue = 13;
    static byte KnightOpenValue = 6;
    static byte PawnOpenValue   = 5;

    /* Attack */
    static byte PawnAttackPawnValue   = 25;
    static byte PawnAttackRookValue   = 90;
    static byte PawnAttackKnightValue = 90;
    static byte PawnAttackBishopValue = 20;
    static byte PawnAttackQueenValue  = 100;
    static byte PawnAttackKingValue   = 30;

    static byte RookAttackPawnValue   = 30;
    static byte RookAttackRookValue   = 10;
    static byte RookAttackKnightValue = 50;
    static byte RookAttackBishopValue = 50;
    static byte RookAttackQueenValue  = 35;
    static byte RookAttackKingValue   = 70;

    static byte KnightAttackPawnValue   = 20;
    static byte KnightAttackRookValue   = 60;
    static byte KnightAttackKnightValue = 20;
    static byte KnightAttackBishopValue = 60;
    static byte KnightAttackQueenValue  = 90;
    static byte KnightAttackKingValue   = 110;

    static byte BishopAttackPawnValue   = 10;
    static byte BishopAttackRookValue   = 60;
    static byte BishopAttackKnightValue = 55;
    static byte BishopAttackBishopValue = 30;
    static byte BishopAttackQueenValue  = 40;
    static byte BishopAttackKingValue   = 70;

    static byte QueenAttackPawnValue   = 15;
    static byte QueenAttackRookValue   = 10;
    static byte QueenAttackKnightValue = 70;
    static byte QueenAttackBishopValue = 10;
    static byte QueenAttackQueenValue  = 25;
    static byte QueenAttackKingValue   = 90;

    static byte KingAttackPawnValue   = 60;
    static byte KingAttackRookValue   = 60;
    static byte KingAttackKnightValue = 60;
    static byte KingAttackBishopValue = 60;
    static byte KingAttackQueenValue  = 10;

    /* Defend */
    static byte PawnDefendPawnValue   = 50;
    static byte PawnDefendRookValue   = 35;
    static byte PawnDefendKnightValue = 50;
    static byte PawnDefendBishopValue = 35;
    static byte PawnDefendQueenValue  = 80;
    static byte PawnDefendKingValue   = 20;

    static byte RookDefendPawnValue   = 10;
    static byte RookDefendRookValue   = 60;
    static byte RookDefendKnightValue = 40;
    static byte RookDefendBishopValue = 40;
    static byte RookDefendQueenValue  = 40;
    static byte RookDefendKingValue   = 40;

    static byte KnightDefendPawnValue   = 50;
    static byte KnightDefendRookValue   = 50;
    static byte KnightDefendKnightValue = 50;
    static byte KnightDefendBishopValue = 50;
    static byte KnightDefendQueenValue  = 50;
    static byte KnightDefendKingValue   = 10;

    static byte BishopDefendPawnValue   = 20;
    static byte BishopDefendRookValue   = 60;
    static byte BishopDefendKnightValue = 60;
    static byte BishopDefendBishopValue = 40;
    static byte BishopDefendQueenValue  = 50;
    static byte BishopDefendKingValue   = 10;

    static byte QueenDefendPawnValue   = 10;
    static byte QueenDefendRookValue   = 40;
    static byte QueenDefendKnightValue = 60;
    static byte QueenDefendBishopValue = 40;
    static byte QueenDefendQueenValue  = 80;
    static byte QueenDefendKingValue   = 50;

    static byte KingDefendPawnValue   = 30;
    static byte KingDefendRookValue   = 50;
    static byte KingDefendKnightValue = 10;
    static byte KingDefendBishopValue = 10;
    static byte KingDefendQueenValue  = 10;

    public static T RandomSelect<T>(List<T> sequence)
    {
        return sequence[rand.Next(sequence.Count())];
    }

    public static XAction IDL_Minimax(XBoard state, int depth, bool playerIsWhite)
    {
        Tuple<XAction, Int64> action_eval = DL_Minimax(state, 1, playerIsWhite);
        if (action_eval.Item2 == WinEval)
        {
            return action_eval.Item1;
        }
        for (int d = 2; d <= depth; d++)
        {
            action_eval = DL_Minimax(state, d, playerIsWhite);
            if (action_eval.Item2 == WinEval)
            {
                return action_eval.Item1;
            }
        }
        return action_eval.Item1;
    }

    public static Tuple<XAction, Int64> DL_Minimax(XBoard state, int depth, bool playerIsWhite)
    {
        var children = LegalMoves(state);
        var bestChild = children[0];
        state.Apply(bestChild);
        var bestVal = DL_Min(state, depth-1, playerIsWhite);
        Console.Write("(" + ChessEngine.tileToFR(bestChild.srcTile) + "-" + ChessEngine.tileToFR(bestChild.destTile) + " " + bestVal + " " + state.whiteCheck + " " + state.blackCheck + ")\t");
        state.Undo();
        foreach(var child in children.Skip(1))
        {
            state.Apply(child);
            var val = DL_Min(state, depth-1, playerIsWhite);
        Console.Write("(" + ChessEngine.tileToFR(child.srcTile) + "-" + ChessEngine.tileToFR(child.destTile) + " " + val + " " + state.whiteCheck + " " + state.blackCheck + ")\t");
            state.Undo();
            if (val > bestVal)
            {
                bestChild = child;
                bestVal = val;
            }
        }
        Console.WriteLine("");

        Console.WriteLine("Final Evaluation: " + bestVal);
        return Tuple.Create(bestChild, bestVal);
    }

    private static Int64 DL_Max(XBoard state, int depth, bool maxWhite)
    {
        if (state.stateHistory.ContainsKey(state.zobristHash))
        {
            if (state.stateHistory[state.zobristHash] >= 2)
            {
                return 0;
            }
        }

        var numPieces = BitOps.CountBits(state.pieces);
        if (numPieces == 2)
        {
            return 0;
        }

        if (numPieces == 3)
        {
            if ( (state.whiteKnights | state.whiteBishops | state.blackKnights | state.blackBishops) != 0)
            {
                return 0;
            }
        }

        if ( !(state.whiteCheck || state.blackCheck) && (depth == 0)) // Optimization, don't calculate children on noncheckmate
        {
            if (state.halfMoveClock >= 100)
            {
                return 0;
            }
            return Heuristic(state, maxWhite);
        }

        var children = LegalMoves(state);
        if (children.Count() == 0) // Is terminal
        {
            if (state.whiteCheck || state.blackCheck) // Check Mate
            {
                return LoseEval;
            } else { // Stalemate
                return 0;
            }
        }

        if (state.halfMoveClock >= 100)
        {
            return 0;
        }

        if (depth == 0)
        {
            return Heuristic(state, maxWhite);
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
        if (state.stateHistory.ContainsKey(state.zobristHash))
        {
            if (state.stateHistory[state.zobristHash] >= 2)
            {
                return 0;
            }
        }

        var numPieces = BitOps.CountBits(state.pieces);
        if (numPieces == 2)
        {
            return 0;
        }

        if (numPieces == 3)
        {
            if ( (state.whiteKnights | state.whiteBishops | state.blackKnights | state.blackBishops) != 0)
            {
                return 0;
            }
        }

        if ( !(state.whiteCheck || state.blackCheck) && (depth == 0)) // Optimization, don't calculate children on noncheckmate
        {
            if (state.halfMoveClock >= 100)
            {
                return 0;
            }
            return Heuristic(state, maxWhite);
        }

        var children = LegalMoves(state);
        if (children.Count() == 0) // Is terminal
        {
            if (state.whiteCheck || state.blackCheck) // Check Mate
            {
                return WinEval;
            } else { // Stalemate
                return 0;
            }
        }

        if (state.halfMoveClock >= 100)
        {
            return 0;
        }

        if (depth == 0)
        {
            return Heuristic(state, maxWhite);
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
        byte turnWhitePenalty = (byte)((state.turnIsWhite) ? 1 : 2);
        byte turnBlackPenalty = (byte)((state.turnIsWhite) ? 2 : 1);

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

            whitePosition = whitePosition + PawnAttackPawnValue   * BitOps.CountBits(pawnAttacks & state.blackPawns  );
            whitePosition = whitePosition + PawnAttackRookValue   * BitOps.CountBits(pawnAttacks & state.blackRooks  );
            whitePosition = whitePosition + PawnAttackKnightValue * BitOps.CountBits(pawnAttacks & state.blackKnights);
            whitePosition = whitePosition + PawnAttackBishopValue * BitOps.CountBits(pawnAttacks & state.blackBishops);
            whitePosition = whitePosition + PawnAttackQueenValue  * BitOps.CountBits(pawnAttacks & state.blackQueens );
            whitePosition = whitePosition + PawnAttackKingValue   * BitOps.CountBits(pawnAttacks & state.blackKing   );

            whitePosition = whitePosition + PawnDefendPawnValue   * BitOps.CountBits(pawnAttacks & state.whitePawns  );
            whitePosition = whitePosition + PawnDefendRookValue   * BitOps.CountBits(pawnAttacks & state.whiteRooks  );
            whitePosition = whitePosition + PawnDefendKnightValue * BitOps.CountBits(pawnAttacks & state.whiteKnights);
            whitePosition = whitePosition + PawnDefendBishopValue * BitOps.CountBits(pawnAttacks & state.whiteBishops);
            whitePosition = whitePosition + PawnDefendQueenValue  * BitOps.CountBits(pawnAttacks & state.whiteQueens );
            whitePosition = whitePosition + PawnDefendKingValue   * BitOps.CountBits(pawnAttacks & state.whiteKing   );
        }
        { // Black Pawns
            UInt64 pawnAttacks = ((state.blackPawns & NotAFile) >> 7) | ((state.blackPawns & NotHFile) >> 9);
            threatenedWhitePositions = threatenedWhitePositions | (pawnAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (pawnAttacks & state.blackPieces);
            blackPosition = blackPosition + PawnOpenValue * BitOps.CountBits(pawnAttacks & state.open);

            blackPosition = blackPosition + PawnAttackPawnValue    * BitOps.CountBits(pawnAttacks & state.whitePawns  );
            blackPosition = blackPosition + PawnAttackRookValue    * BitOps.CountBits(pawnAttacks & state.whiteRooks  );
            blackPosition = blackPosition + PawnAttackKnightValue  * BitOps.CountBits(pawnAttacks & state.whiteKnights);
            blackPosition = blackPosition + PawnAttackBishopValue  * BitOps.CountBits(pawnAttacks & state.whiteBishops);
            blackPosition = blackPosition + PawnAttackQueenValue   * BitOps.CountBits(pawnAttacks & state.whiteQueens );
            blackPosition = blackPosition + PawnAttackKingValue    * BitOps.CountBits(pawnAttacks & state.whiteKing   );

            blackPosition = blackPosition + PawnDefendPawnValue   * BitOps.CountBits(pawnAttacks & state.blackPawns  );
            blackPosition = blackPosition + PawnDefendRookValue   * BitOps.CountBits(pawnAttacks & state.blackRooks  );
            blackPosition = blackPosition + PawnDefendKnightValue * BitOps.CountBits(pawnAttacks & state.blackKnights);
            blackPosition = blackPosition + PawnDefendBishopValue * BitOps.CountBits(pawnAttacks & state.blackBishops);
            blackPosition = blackPosition + PawnDefendQueenValue  * BitOps.CountBits(pawnAttacks & state.blackQueens );
            blackPosition = blackPosition + PawnDefendKingValue   * BitOps.CountBits(pawnAttacks & state.blackKing   );
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

            whitePosition = whitePosition + KnightAttackPawnValue   * BitOps.CountBits(knightAttacks & state.blackPawns  );
            whitePosition = whitePosition + KnightAttackRookValue   * BitOps.CountBits(knightAttacks & state.blackRooks  );
            whitePosition = whitePosition + KnightAttackKnightValue * BitOps.CountBits(knightAttacks & state.blackKnights);
            whitePosition = whitePosition + KnightAttackBishopValue * BitOps.CountBits(knightAttacks & state.blackBishops);
            whitePosition = whitePosition + KnightAttackQueenValue  * BitOps.CountBits(knightAttacks & state.blackQueens );
            whitePosition = whitePosition + KnightAttackKingValue   * BitOps.CountBits(knightAttacks & state.blackKing   );

            whitePosition = whitePosition + KnightDefendPawnValue   * BitOps.CountBits(knightAttacks & state.whitePawns  );
            whitePosition = whitePosition + KnightDefendRookValue   * BitOps.CountBits(knightAttacks & state.whiteRooks  );
            whitePosition = whitePosition + KnightDefendKnightValue * BitOps.CountBits(knightAttacks & state.whiteKnights);
            whitePosition = whitePosition + KnightDefendBishopValue * BitOps.CountBits(knightAttacks & state.whiteBishops);
            whitePosition = whitePosition + KnightDefendQueenValue  * BitOps.CountBits(knightAttacks & state.whiteQueens );
            whitePosition = whitePosition + KnightDefendKingValue   * BitOps.CountBits(knightAttacks & state.whiteKing   );
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

            blackPosition = blackPosition + KnightAttackPawnValue   * BitOps.CountBits(knightAttacks & state.whitePawns  );
            blackPosition = blackPosition + KnightAttackRookValue   * BitOps.CountBits(knightAttacks & state.whiteRooks  );
            blackPosition = blackPosition + KnightAttackKnightValue * BitOps.CountBits(knightAttacks & state.whiteKnights);
            blackPosition = blackPosition + KnightAttackBishopValue * BitOps.CountBits(knightAttacks & state.whiteBishops);
            blackPosition = blackPosition + KnightAttackQueenValue  * BitOps.CountBits(knightAttacks & state.whiteQueens );
            blackPosition = blackPosition + KnightAttackKingValue   * BitOps.CountBits(knightAttacks & state.whiteKing   );

            blackPosition = blackPosition + KnightDefendPawnValue   * BitOps.CountBits(knightAttacks & state.blackPawns  );
            blackPosition = blackPosition + KnightDefendRookValue   * BitOps.CountBits(knightAttacks & state.blackRooks  );
            blackPosition = blackPosition + KnightDefendKnightValue * BitOps.CountBits(knightAttacks & state.blackKnights);
            blackPosition = blackPosition + KnightDefendBishopValue * BitOps.CountBits(knightAttacks & state.blackBishops);
            blackPosition = blackPosition + KnightDefendQueenValue  * BitOps.CountBits(knightAttacks & state.blackQueens );
            blackPosition = blackPosition + KnightDefendKingValue   * BitOps.CountBits(knightAttacks & state.blackKing   );
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

            whitePosition = whitePosition + BishopAttackPawnValue   * BitOps.CountBits(bishopAttacks & state.blackPawns  );
            whitePosition = whitePosition + BishopAttackRookValue   * BitOps.CountBits(bishopAttacks & state.blackRooks  );
            whitePosition = whitePosition + BishopAttackKnightValue * BitOps.CountBits(bishopAttacks & state.blackKnights);
            whitePosition = whitePosition + BishopAttackBishopValue * BitOps.CountBits(bishopAttacks & state.blackBishops);
            whitePosition = whitePosition + BishopAttackQueenValue  * BitOps.CountBits(bishopAttacks & state.blackQueens );
            whitePosition = whitePosition + BishopAttackKingValue   * BitOps.CountBits(bishopAttacks & state.blackKing   );

            whitePosition = whitePosition + BishopDefendPawnValue   * BitOps.CountBits(bishopAttacks & state.whitePawns  );
            whitePosition = whitePosition + BishopDefendRookValue   * BitOps.CountBits(bishopAttacks & state.whiteRooks  );
            whitePosition = whitePosition + BishopDefendKnightValue * BitOps.CountBits(bishopAttacks & state.whiteKnights);
            whitePosition = whitePosition + BishopDefendBishopValue * BitOps.CountBits(bishopAttacks & state.whiteBishops);
            whitePosition = whitePosition + BishopDefendQueenValue  * BitOps.CountBits(bishopAttacks & state.whiteQueens );
            whitePosition = whitePosition + BishopDefendKingValue   * BitOps.CountBits(bishopAttacks & state.whiteKing   );
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

            blackPosition = blackPosition + BishopAttackPawnValue   * BitOps.CountBits(bishopAttacks & state.whitePawns  );
            blackPosition = blackPosition + BishopAttackRookValue   * BitOps.CountBits(bishopAttacks & state.whiteRooks  );
            blackPosition = blackPosition + BishopAttackKnightValue * BitOps.CountBits(bishopAttacks & state.whiteKnights);
            blackPosition = blackPosition + BishopAttackBishopValue * BitOps.CountBits(bishopAttacks & state.whiteBishops);
            blackPosition = blackPosition + BishopAttackQueenValue  * BitOps.CountBits(bishopAttacks & state.whiteQueens );
            blackPosition = blackPosition + BishopAttackKingValue   * BitOps.CountBits(bishopAttacks & state.whiteKing   );

            blackPosition = blackPosition + BishopDefendPawnValue   * BitOps.CountBits(bishopAttacks & state.blackPawns  );
            blackPosition = blackPosition + BishopDefendRookValue   * BitOps.CountBits(bishopAttacks & state.blackRooks  );
            blackPosition = blackPosition + BishopDefendKnightValue * BitOps.CountBits(bishopAttacks & state.blackKnights);
            blackPosition = blackPosition + BishopDefendBishopValue * BitOps.CountBits(bishopAttacks & state.blackBishops);
            blackPosition = blackPosition + BishopDefendQueenValue  * BitOps.CountBits(bishopAttacks & state.blackQueens );
            blackPosition = blackPosition + BishopDefendKingValue   * BitOps.CountBits(bishopAttacks & state.blackKing   );
        } // End Black Bishops
        { // White Rooks
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

            whitePosition = whitePosition + RookAttackPawnValue   * BitOps.CountBits(rookAttacks & state.blackPawns  );
            whitePosition = whitePosition + RookAttackRookValue   * BitOps.CountBits(rookAttacks & state.blackRooks  );
            whitePosition = whitePosition + RookAttackKnightValue * BitOps.CountBits(rookAttacks & state.blackKnights);
            whitePosition = whitePosition + RookAttackBishopValue * BitOps.CountBits(rookAttacks & state.blackBishops);
            whitePosition = whitePosition + RookAttackQueenValue  * BitOps.CountBits(rookAttacks & state.blackQueens );
            whitePosition = whitePosition + RookAttackKingValue   * BitOps.CountBits(rookAttacks & state.blackKing   );

            whitePosition = whitePosition + RookDefendPawnValue   * BitOps.CountBits(rookAttacks & state.whitePawns  );
            whitePosition = whitePosition + RookDefendRookValue   * BitOps.CountBits(rookAttacks & state.whiteRooks  );
            whitePosition = whitePosition + RookDefendKnightValue * BitOps.CountBits(rookAttacks & state.whiteKnights);
            whitePosition = whitePosition + RookDefendBishopValue * BitOps.CountBits(rookAttacks & state.whiteBishops);
            whitePosition = whitePosition + RookDefendQueenValue  * BitOps.CountBits(rookAttacks & state.whiteQueens );
            whitePosition = whitePosition + RookDefendKingValue   * BitOps.CountBits(rookAttacks & state.whiteKing   );
        } // End White Rooks
        { // Black Rooks
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

            blackPosition = blackPosition + RookAttackPawnValue   * BitOps.CountBits(rookAttacks & state.whitePawns  );
            blackPosition = blackPosition + RookAttackRookValue   * BitOps.CountBits(rookAttacks & state.whiteRooks  );
            blackPosition = blackPosition + RookAttackKnightValue * BitOps.CountBits(rookAttacks & state.whiteKnights);
            blackPosition = blackPosition + RookAttackBishopValue * BitOps.CountBits(rookAttacks & state.whiteBishops);
            blackPosition = blackPosition + RookAttackQueenValue  * BitOps.CountBits(rookAttacks & state.whiteQueens );
            blackPosition = blackPosition + RookAttackKingValue   * BitOps.CountBits(rookAttacks & state.whiteKing   );

            blackPosition = blackPosition + RookDefendPawnValue   * BitOps.CountBits(rookAttacks & state.blackPawns  );
            blackPosition = blackPosition + RookDefendRookValue   * BitOps.CountBits(rookAttacks & state.blackRooks  );
            blackPosition = blackPosition + RookDefendKnightValue * BitOps.CountBits(rookAttacks & state.blackKnights);
            blackPosition = blackPosition + RookDefendBishopValue * BitOps.CountBits(rookAttacks & state.blackBishops);
            blackPosition = blackPosition + RookDefendQueenValue  * BitOps.CountBits(rookAttacks & state.blackQueens );
            blackPosition = blackPosition + RookDefendKingValue   * BitOps.CountBits(rookAttacks & state.blackKing   );
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

            whitePosition = whitePosition + QueenAttackPawnValue   * BitOps.CountBits(queenAttacks & state.blackPawns  );
            whitePosition = whitePosition + QueenAttackRookValue   * BitOps.CountBits(queenAttacks & state.blackRooks  );
            whitePosition = whitePosition + QueenAttackKnightValue * BitOps.CountBits(queenAttacks & state.blackKnights);
            whitePosition = whitePosition + QueenAttackBishopValue * BitOps.CountBits(queenAttacks & state.blackBishops);
            whitePosition = whitePosition + QueenAttackQueenValue  * BitOps.CountBits(queenAttacks & state.blackQueens );
            whitePosition = whitePosition + QueenAttackKingValue   * BitOps.CountBits(queenAttacks & state.blackKing   );

            whitePosition = whitePosition + QueenDefendPawnValue   * BitOps.CountBits(queenAttacks & state.whitePawns  );
            whitePosition = whitePosition + QueenDefendRookValue   * BitOps.CountBits(queenAttacks & state.whiteRooks  );
            whitePosition = whitePosition + QueenDefendKnightValue * BitOps.CountBits(queenAttacks & state.whiteKnights);
            whitePosition = whitePosition + QueenDefendBishopValue * BitOps.CountBits(queenAttacks & state.whiteBishops);
            whitePosition = whitePosition + QueenDefendQueenValue  * BitOps.CountBits(queenAttacks & state.whiteQueens );
            whitePosition = whitePosition + QueenDefendKingValue   * BitOps.CountBits(queenAttacks & state.whiteKing   );
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

            blackPosition = blackPosition + QueenAttackPawnValue   * BitOps.CountBits(queenAttacks & state.whitePawns  );
            blackPosition = blackPosition + QueenAttackRookValue   * BitOps.CountBits(queenAttacks & state.whiteRooks  );
            blackPosition = blackPosition + QueenAttackKnightValue * BitOps.CountBits(queenAttacks & state.whiteKnights);
            blackPosition = blackPosition + QueenAttackBishopValue * BitOps.CountBits(queenAttacks & state.whiteBishops);
            blackPosition = blackPosition + QueenAttackQueenValue  * BitOps.CountBits(queenAttacks & state.whiteQueens );
            blackPosition = blackPosition + QueenAttackKingValue   * BitOps.CountBits(queenAttacks & state.whiteKing   );

            blackPosition = blackPosition + QueenDefendPawnValue   * BitOps.CountBits(queenAttacks & state.blackPawns  );
            blackPosition = blackPosition + QueenDefendRookValue   * BitOps.CountBits(queenAttacks & state.blackRooks  );
            blackPosition = blackPosition + QueenDefendKnightValue * BitOps.CountBits(queenAttacks & state.blackKnights);
            blackPosition = blackPosition + QueenDefendBishopValue * BitOps.CountBits(queenAttacks & state.blackBishops);
            blackPosition = blackPosition + QueenDefendQueenValue  * BitOps.CountBits(queenAttacks & state.blackQueens );
            blackPosition = blackPosition + QueenDefendKingValue   * BitOps.CountBits(queenAttacks & state.blackKing   );
        } // End Black Queens
        { // White King
            UInt64 kingAttacks = getKingAttacks(state.whiteKing);
            
            threatenedBlackPositions = threatenedBlackPositions | (kingAttacks & state.blackPieces);
            defendedWhitePositions = defendedWhitePositions | (kingAttacks & state.whitePieces);
            whitePosition = whitePosition + KingOpenValue * BitOps.CountBits(kingAttacks & state.open);

            whitePosition = whitePosition + KingAttackPawnValue   * BitOps.CountBits(kingAttacks & state.blackPawns  );
            whitePosition = whitePosition + KingAttackRookValue   * BitOps.CountBits(kingAttacks & state.blackRooks  );
            whitePosition = whitePosition + KingAttackKnightValue * BitOps.CountBits(kingAttacks & state.blackKnights);
            whitePosition = whitePosition + KingAttackBishopValue * BitOps.CountBits(kingAttacks & state.blackBishops);
            whitePosition = whitePosition + KingAttackQueenValue  * BitOps.CountBits(kingAttacks & state.blackQueens );

            whitePosition = whitePosition + KingDefendPawnValue   * BitOps.CountBits(kingAttacks & state.whitePawns  );
            whitePosition = whitePosition + KingDefendRookValue   * BitOps.CountBits(kingAttacks & state.whiteRooks  );
            whitePosition = whitePosition + KingDefendKnightValue * BitOps.CountBits(kingAttacks & state.whiteKnights);
            whitePosition = whitePosition + KingDefendBishopValue * BitOps.CountBits(kingAttacks & state.whiteBishops);
            whitePosition = whitePosition + KingDefendQueenValue  * BitOps.CountBits(kingAttacks & state.whiteQueens );
        } // End White King
        { // Black King
            UInt64 kingAttacks = getKingAttacks(state.blackKing);
            
            threatenedWhitePositions = threatenedWhitePositions | (kingAttacks & state.whitePieces);
            defendedBlackPositions = defendedBlackPositions | (kingAttacks & state.blackPieces);
            blackPosition = blackPosition + KingOpenValue * BitOps.CountBits(kingAttacks & state.open);

            blackPosition = blackPosition + KingAttackPawnValue   * BitOps.CountBits(kingAttacks & state.whitePawns  );
            blackPosition = blackPosition + KingAttackRookValue   * BitOps.CountBits(kingAttacks & state.whiteRooks  );
            blackPosition = blackPosition + KingAttackKnightValue * BitOps.CountBits(kingAttacks & state.whiteKnights);
            blackPosition = blackPosition + KingAttackBishopValue * BitOps.CountBits(kingAttacks & state.whiteBishops);
            blackPosition = blackPosition + KingAttackQueenValue  * BitOps.CountBits(kingAttacks & state.whiteQueens );

            blackPosition = blackPosition + KingDefendPawnValue   * BitOps.CountBits(kingAttacks & state.blackPawns  );
            blackPosition = blackPosition + KingDefendRookValue   * BitOps.CountBits(kingAttacks & state.blackRooks  );
            blackPosition = blackPosition + KingDefendKnightValue * BitOps.CountBits(kingAttacks & state.blackKnights);
            blackPosition = blackPosition + KingDefendBishopValue * BitOps.CountBits(kingAttacks & state.blackBishops);
            blackPosition = blackPosition + KingDefendQueenValue  * BitOps.CountBits(kingAttacks & state.blackQueens );
        } // End Black King

        threatenedWhitePositions = threatenedWhitePositions & ~defendedWhitePositions;
        threatenedBlackPositions = threatenedBlackPositions & ~defendedBlackPositions;
        whitePosition += defendedBonus * BitOps.CountBits(defendedWhitePositions) - threatenedPenalty * BitOps.CountBits(threatenedWhitePositions) * turnWhitePenalty;
        blackPosition += defendedBonus * BitOps.CountBits(defendedBlackPositions) - threatenedPenalty * BitOps.CountBits(threatenedBlackPositions) * turnBlackPenalty;
        
        if (playerIsWhite)
        {
            return (Int64)(materialWeight*(whiteMaterial - blackMaterial) + positionWeight*(whitePosition - blackPosition));
        }
        {
            return (Int64)(materialWeight*(blackMaterial - whiteMaterial) + positionWeight*(blackPosition - whitePosition));
        }
    }
}
