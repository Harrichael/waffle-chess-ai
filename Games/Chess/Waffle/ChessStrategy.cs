
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using static ChessRules;

public class TTEntry
{
    public XAction action;
    public int depth;
    public Int64 eval;

    public TTEntry(XAction action, int depth, Int64 eval)
    {
        this.action = action;
        this.depth = depth;
        this.eval = eval;
    }
}


public static class ChessStrategy
{
/*
TODO

Symmetrical Attacks and turns
King Attacks to threatened squares
Attackers who have multiple targets and threaten king
*/
    private static Int64 WinEval = (Int64.MaxValue-1);
    private static Int64 LoseEval = -WinEval;

    private static Random rand = new Random();
    static readonly byte potentialWeight = 2;
    static readonly byte materialWeight = 5;
    static readonly byte staticWeight = 3;
    static readonly byte positionWeight = 1;

    /* Material */
    static readonly byte QueenMaterial  = 180;
    static readonly byte RookMaterial   = 85;
    static readonly byte BishopMaterial = 70;
    static readonly byte KnightMaterial = 60;
    static readonly byte PawnMaterial   = 12;
    static readonly byte BishopPairBonus = 40;

    /* Static */
    static readonly uint QueenExistenceBonus = 500;
    static readonly byte CastlePotentialBonus = 20;
    static readonly byte CastleBonus = 50;
    static readonly byte CastleTurnCutOff = 80;
    static readonly byte PawnDifferenceBonus = 15;

    static readonly byte[] PawnSquareTable = {
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
        10, 10, 17, 27, 27, 17, 10, 10,
        12, 13, 15, 26, 26, 15, 14, 12,
        8 , 10, 10, 17, 17, 10, 10, 8 ,
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0
    };

    static readonly byte[] RookSquareTable = {
        20, 20, 20, 20, 20, 20, 20, 20,
        20, 20, 20, 20, 20, 20, 20, 20,
        20, 20, 20, 20, 20, 20, 20, 20,
        20, 20, 20, 20, 20, 20, 20, 20,
        20, 20, 20, 20, 20, 20, 20, 20,
        20, 20, 20, 20, 20, 20, 20, 20,
        20, 20, 20, 20, 20, 20, 20, 20,
        20, 20, 20, 20, 20, 20, 20, 20
    };

    static readonly byte[] KingSquareTable = {
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
        0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
        25, 25, 10, 10, 10, 10, 25, 25,
        25, 30, 20, 10, 10, 20, 30, 25
    };

    /* Lines of Attack */
    static readonly byte threatenedPenalty = 50;
    static readonly byte defendedBonus = 30;
    static readonly byte turnSafeMult = 2;
    static readonly byte turnThreatMult = 6;

    /* Mobility */
    static readonly byte KingOpenValue   = 1;
    static readonly byte QueenOpenValue  = 7;
    static readonly byte RookOpenValue   = 6;
    static readonly byte BishopOpenValue = 13;
    static readonly byte KnightOpenValue = 6;
    static readonly byte PawnOpenValue   = 5;

    /* Attack */
    static readonly byte PawnAttackPawnValue   = 70;
    static readonly byte PawnAttackRookValue   = 100;
    static readonly byte PawnAttackKnightValue = 90;
    static readonly byte PawnAttackBishopValue = 110;
    static readonly byte PawnAttackQueenValue  = 140;
    static readonly byte PawnAttackKingValue   = 30;

    static readonly byte RookAttackPawnValue   = 30;
    static readonly byte RookAttackRookValue   = 10;
    static readonly byte RookAttackKnightValue = 50;
    static readonly byte RookAttackBishopValue = 35;
    static readonly byte RookAttackQueenValue  = 50;
    static readonly byte RookAttackKingValue   = 70;

    static readonly byte KnightAttackPawnValue   = 25;
    static readonly byte KnightAttackRookValue   = 60;
    static readonly byte KnightAttackKnightValue = 20;
    static readonly byte KnightAttackBishopValue = 60;
    static readonly byte KnightAttackQueenValue  = 90;
    static readonly byte KnightAttackKingValue   = 96;

    static readonly byte BishopAttackPawnValue   = 8;
    static readonly byte BishopAttackRookValue   = 80;
    static readonly byte BishopAttackKnightValue = 65;
    static readonly byte BishopAttackBishopValue = 30;
    static readonly byte BishopAttackQueenValue  = 40;
    static readonly byte BishopAttackKingValue   = 55;

    static readonly byte QueenAttackPawnValue   = 15;
    static readonly byte QueenAttackRookValue   = 55;
    static readonly byte QueenAttackKnightValue = 30;
    static readonly byte QueenAttackBishopValue = 10;
    static readonly byte QueenAttackQueenValue  = 25;
    static readonly byte QueenAttackKingValue   = 80;

    static readonly byte KingAttackPawnValue   = 60;
    static readonly byte KingAttackRookValue   = 60;
    static readonly byte KingAttackKnightValue = 60;
    static readonly byte KingAttackBishopValue = 60;

    /* Defend */
    static readonly byte PawnDefendPawnValue   = 55;
    static readonly byte PawnDefendRookValue   = 35;
    static readonly byte PawnDefendKnightValue = 50;
    static readonly byte PawnDefendBishopValue = 35;
    static readonly byte PawnDefendQueenValue  = 80;
    static readonly byte PawnDefendKingValue   = 20;

    static readonly byte RookDefendPawnValue   = 10;
    static readonly byte RookDefendRookValue   = 60;
    static readonly byte RookDefendKnightValue = 40;
    static readonly byte RookDefendBishopValue = 40;
    static readonly byte RookDefendQueenValue  = 40;
    static readonly byte RookDefendKingValue   = 40;

    static readonly byte KnightDefendPawnValue   = 45;
    static readonly byte KnightDefendRookValue   = 50;
    static readonly byte KnightDefendKnightValue = 50;
    static readonly byte KnightDefendBishopValue = 50;
    static readonly byte KnightDefendQueenValue  = 55;
    static readonly byte KnightDefendKingValue   = 70;

    static readonly byte BishopDefendPawnValue   = 20;
    static readonly byte BishopDefendRookValue   = 45;
    static readonly byte BishopDefendKnightValue = 45;
    static readonly byte BishopDefendBishopValue = 30;
    static readonly byte BishopDefendQueenValue  = 30;
    static readonly byte BishopDefendKingValue   = 10;

    static readonly byte QueenDefendPawnValue   = 10;
    static readonly byte QueenDefendRookValue   = 40;
    static readonly byte QueenDefendKnightValue = 60;
    static readonly byte QueenDefendBishopValue = 40;
    static readonly byte QueenDefendQueenValue  = 80;
    static readonly byte QueenDefendKingValue   = 50;

    static readonly byte KingDefendPawnValue   = 40;
    static readonly byte KingDefendRookValue   = 25;
    static readonly byte KingDefendKnightValue = 10;
    static readonly byte KingDefendBishopValue = 10;
    static readonly byte KingDefendQueenValue  = 10;

    /* Search variables */
    static Dictionary<XAction, int> HistoryTable = new Dictionary<XAction, int>();
    static Dictionary<UInt64, TTEntry> TranspositionTable = new Dictionary<UInt64, TTEntry>();
    static bool continueSearch = true;

    public static void StopSearch()
    {
        continueSearch = false;
    }

    public static void ContinueSearch()
    {
        continueSearch = true;
    }

    private static void UpdateTT(UInt64 zobristHash, XAction action, int depth, Int64 eval)
    {
        if (TranspositionTable.ContainsKey(zobristHash))
        {
            var entry = TranspositionTable[zobristHash];
            if (entry.depth <= depth)
            {
                entry.action = action;
                entry.eval = eval;
                entry.depth = depth;
            }
        } else {
            var entry = new TTEntry(action, depth, eval);
            TranspositionTable[zobristHash] = entry;
        }
    }

    public static T RandomSelect<T>(List<T> sequence)
    {
        return sequence[rand.Next(sequence.Count())];
    }

    public static XAction ID_ABMinimax(XBoard state, int q_depth, bool playerIsWhite)
    {
        int depth = 1;
        Tuple<XAction, Int64> action_eval = DL_ABMinimax(state, depth, q_depth, playerIsWhite);
        if (action_eval.Item2 == WinEval)
        {
            return action_eval.Item1;
        }
        while (continueSearch)
        {
            depth += 1;
            action_eval = DL_ABMinimax(state, depth, q_depth, playerIsWhite);
            if (action_eval.Item2 == WinEval)
            {
                return action_eval.Item1;
            }
        }
        return action_eval.Item1;
    }

    public static XAction TLID_ABMinimax(XBoard state, int limitMS, int q_depth, bool playerIsWhite)
    {
        Stopwatch timer = new Stopwatch();
        int depth = 1;
        timer.Start();
        Tuple<XAction, Int64> action_eval = DL_ABMinimax(state, depth, q_depth, playerIsWhite);
        if (action_eval.Item2 == WinEval)
        {
            timer.Stop();
            return action_eval.Item1;
        }
        while ((timer.ElapsedMilliseconds < limitMS) && continueSearch)
        {
            depth += 1;
            action_eval = DL_ABMinimax(state, depth, q_depth, playerIsWhite);
            if (action_eval.Item2 == WinEval)
            {
                timer.Stop();
                return action_eval.Item1;
            }
        }
        timer.Stop();
        return action_eval.Item1;
    }

    public static Tuple<XAction, Int64> DL_ABMinimax(XBoard state, int depth, int q_depth, bool playerIsWhite)
    {
        if (TranspositionTable.ContainsKey(state.zobristHash))
        {
            var entry = TranspositionTable[state.zobristHash];
            if (entry.depth >= depth && entry.action != null)
            {
                Console.WriteLine("Transposition Table!");
                return Tuple.Create(entry.action, entry.eval);
            }
        }

        Int64 alpha = LoseEval;
        Int64 beta = WinEval;
        var children = OrderedLegalMoves(state);
        var bestChild = children[0];
        state.Apply(bestChild);
        Int64 bestVal;
        if (playerIsWhite != state.turnIsWhite)
        {
            bestVal = DL_ABMin(state, depth-1, q_depth, playerIsWhite, alpha, beta);
            alpha = bestVal;
        } else {
            bestVal = DL_ABMax(state, depth-1, q_depth, playerIsWhite, alpha, beta);
            beta = bestVal;
        }
        Console.Write("(" + ChessEngine.tileToFR(bestChild.srcTile) + "-" + ChessEngine.tileToFR(bestChild.destTile) + " " + bestVal + " " + state.whiteCheck + " " + state.blackCheck + ")\t");
        state.Undo();
        Int64 val;
        foreach(var child in children.Skip(1))
        {
            state.Apply(child);
            if (playerIsWhite != state.turnIsWhite)
            {
                val = DL_ABMin(state, depth-1, q_depth, playerIsWhite, alpha, beta);
            } else {
                val = DL_ABMax(state, depth-1, q_depth, playerIsWhite, alpha, beta);
            }
            Console.Write("(" + ChessEngine.tileToFR(child.srcTile) + "-" + ChessEngine.tileToFR(child.destTile) + " " + val + " " + state.whiteCheck + " " + state.blackCheck + ")\t");
            state.Undo();
            if (playerIsWhite == state.turnIsWhite)
            {
                if (val > bestVal)
                {
                    bestChild = child;
                    bestVal = val;
                    alpha = bestVal;
                }
            } else {
                if (val < bestVal)
                {
                    bestChild = child;
                    bestVal = val;
                    beta = bestVal;
                }
            }
        }
        Console.WriteLine("");

        Console.WriteLine("Final Evaluation: " + bestVal);

        UpdateTT(state.zobristHash, bestChild, depth, bestVal);
        return Tuple.Create(bestChild, bestVal);
    }

    private static Int64 DL_ABMax(XBoard state, int depth, int q_depth, bool maxWhite, Int64 alpha, Int64 beta)
    {
        if (state.stateHistory.ContainsKey(state.zobristHash))
        {
            if (state.stateHistory[state.zobristHash] >= 2)
            {
                return 0;
            }
        }

        if (TranspositionTable.ContainsKey(state.zobristHash))
        {
            var entry = TranspositionTable[state.zobristHash];
            if (entry.depth >= depth)
            {
                return entry.eval;
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

        var children = OrderedLegalMoves(state);
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

        var quiet = !(state.whiteCheck || state.blackCheck);

        if (depth == 0 && (q_depth <= 0 || quiet))
        {
            return Heuristic(state, maxWhite);
        }

        if (!continueSearch)
        {
            if (TranspositionTable.ContainsKey(state.zobristHash))
            {
                return TranspositionTable[state.zobristHash].eval;
            } else {
                return Heuristic(state, maxWhite);
            }
        }

        if (depth == 0)
        {
            // If we are here, this state is nonquiecessent.
            // If we += 1 on depth, we get same behaviour but better transposition table usage
            depth += 1;
            q_depth -= 1;
        }

        state.Apply(children[0]);
        var maxH = DL_ABMin(state, depth-1, q_depth, maxWhite, alpha, beta);
        state.Undo();
        if (maxH > alpha)
        {
            alpha = maxH;
            if (maxH >= beta)
            {
                if (!HistoryTable.ContainsKey(children[0]))
                {
                    HistoryTable[children[0]] = 0;
                }
                HistoryTable[children[0]] += (2 << depth) * q_depth;
                UpdateTT(state.zobristHash, children[0], depth, maxH);
                return beta;
            }
        }

        var bestAction = children[0];
        foreach(var child in children.Skip(1))
        {
            state.Apply(child);
            var hVal = DL_ABMin(state, depth-1, q_depth, maxWhite, alpha, beta);
            state.Undo();

            if (hVal > maxH)
            {
                maxH = hVal;
                bestAction = child;
                if (maxH > alpha)
                {
                    alpha = maxH;
                    if (maxH >= beta)
                    {
                        if (!HistoryTable.ContainsKey(child))
                        {
                            HistoryTable[child] = 0;
                        }
                        HistoryTable[child] += (2 << depth) * q_depth;
                        UpdateTT(state.zobristHash, child, depth, maxH);
                        return beta;
                    }
                }
            }
        }
        UpdateTT(state.zobristHash, bestAction, depth, maxH);
        return maxH;
    }

    private static Int64 DL_ABMin(XBoard state, int depth, int q_depth, bool maxWhite, Int64 alpha, Int64 beta)
    {
        if (state.stateHistory.ContainsKey(state.zobristHash))
        {
            if (state.stateHistory[state.zobristHash] >= 2)
            {
                return 0;
            }
        }

        if (TranspositionTable.ContainsKey(state.zobristHash))
        {
            var entry = TranspositionTable[state.zobristHash];
            if (entry.depth >= depth)
            {
                return entry.eval;
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

        var children = OrderedLegalMoves(state);
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

        var quiet = !(state.whiteCheck || state.blackCheck);

        if (depth == 0 && (q_depth <= 0 || quiet))
        {
            return Heuristic(state, maxWhite);
        }

        if (!continueSearch)
        {
            if (TranspositionTable.ContainsKey(state.zobristHash))
            {
                return TranspositionTable[state.zobristHash].eval;
            } else {
                return Heuristic(state, maxWhite);
            }
        }

        if (depth == 0)
        {
            // If we are here, this state is nonquiecessent.
            // If we += 1 on depth, we get same behaviour but better transposition table usage
            depth += 1;
            q_depth -= 1;
        }

        state.Apply(children[0]);
        var minH = DL_ABMax(state, depth-1, q_depth, maxWhite, alpha, beta);
        state.Undo();
        if (minH < beta)
        {
            beta = minH;
            if (minH < alpha)
            {
                if (!HistoryTable.ContainsKey(children[0]))
                {
                    HistoryTable[children[0]] = 0;
                }
                HistoryTable[children[0]] += (2 << depth) * q_depth;
                UpdateTT(state.zobristHash, children[0], depth, minH);
                return alpha;
            }
        }

        var bestAction = children[0];
        foreach(var child in children.Skip(1))
        {
            state.Apply(child);
            var hVal = DL_ABMax(state, depth-1, q_depth, maxWhite, alpha, beta);
            state.Undo();

            if (hVal < minH)
            {
                minH = hVal;
                bestAction = child;
                if (minH < beta)
                {
                    beta = minH;
                    if (minH < alpha)
                    {
                        if (!HistoryTable.ContainsKey(child))
                        {
                            HistoryTable[child] = 0;
                        }
                        HistoryTable[child] += (2 << depth) * q_depth;
                        UpdateTT(state.zobristHash, child, depth, minH);
                        return alpha;
                    }
                }
            }
        }
        UpdateTT(state.zobristHash, bestAction, depth, minH);
        return minH;
    }

    private static List<XAction> OrderedLegalMoves(XBoard state)
    {
        return LegalMoves(state)
            .OrderBy(n => TranspositionTable.ContainsKey(state.zobristHash) && TranspositionTable[state.zobristHash].action != n)
            .ThenBy(n => n.attackType)
            .ThenByDescending(n => HistoryTable.GetValueOrDefault(n, 0))
            .ThenBy(n => rand.Next())
            .ToList();
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
        byte turnWhitePenalty = (byte)((state.turnIsWhite) ? turnSafeMult : turnThreatMult);
        byte turnBlackPenalty = (byte)((state.turnIsWhite) ? turnThreatMult : turnSafeMult);
        UInt64 pieces;
        UInt64 piece;

        // Potential
        UInt64 whiteMaterial = 0;
        UInt64 whiteStatic = 0;
        UInt64 blackMaterial = 0;
        UInt64 blackStatic = 0;
        {
            UInt64 whiteNumPawns = BitOps.CountBits(state.whitePawns);
            UInt64 blackNumPawns = BitOps.CountBits(state.blackPawns);
            whiteStatic += PawnDifferenceBonus * (whiteNumPawns - blackNumPawns);
            blackStatic += PawnDifferenceBonus * (blackNumPawns - whiteNumPawns);
            { // White Potential
                pieces = state.whitePawns;
                while (pieces != 0)
                {
                    piece = BitOps.MSB(pieces);
                    pieces -= piece;
    
                    whiteStatic += PawnSquareTable[64 - BitOps.bbIndex(piece)];
                }
                pieces = state.whiteRooks;
                while (pieces != 0)
                {
                    piece = BitOps.MSB(pieces);
                    pieces -= piece;
    
                    whiteStatic += RookSquareTable[64 - BitOps.bbIndex(piece)];
                }
                whiteStatic += KingSquareTable[64 - BitOps.bbIndex(state.whiteKing)];
    
                whiteMaterial += PawnMaterial * whiteNumPawns;
                whiteMaterial += RookMaterial * BitOps.CountBits(state.whiteRooks);
                whiteMaterial += KnightMaterial * BitOps.CountBits(state.whiteKnights);
                whiteMaterial += BishopMaterial * BitOps.CountBits(state.whiteBishops);
                whiteMaterial += QueenMaterial * BitOps.CountBits(state.whiteQueens);
                if (state.whiteQueens != 0)
                {
                    whiteStatic += QueenExistenceBonus;
                }
                if (state.whiteCastleKS)
                {
                    whiteStatic += CastlePotentialBonus;
                }
                if (state.whiteCastleQS)
                {
                    whiteStatic += CastlePotentialBonus;
                }
                if (!state.whiteCastleKS && !state.whiteCastleQS && state.stateHistory.Count() < CastleTurnCutOff)
                {
                    if (state.whiteKing == whiteKSDest || state.whiteKing == whiteQSDest || ((state.whiteRooks & (whiteKSRookDest | whiteQSRookDest)) != 0))
                    {
                        whiteStatic += CastleBonus;
                    }
                }
                if (BitOps.CountBits(state.whiteBishops) >= 2) {
                    whiteMaterial += BishopPairBonus;
                }
            } // End White Potential
            { // Black Potential
                pieces = state.blackPawns;
                while (pieces != 0)
                {
                    piece = BitOps.MSB(pieces);
                    pieces -= piece;
    
                    blackStatic += PawnSquareTable[BitOps.bbIndex(piece) - 1];
                }
                pieces = state.blackRooks;
                while (pieces != 0)
                {
                    piece = BitOps.MSB(pieces);
                    pieces -= piece;
    
                    blackStatic += RookSquareTable[BitOps.bbIndex(piece) - 1];
                }
                blackStatic += KingSquareTable[BitOps.bbIndex(state.blackKing) - 1];
    
                blackMaterial += PawnMaterial * blackNumPawns;
                blackMaterial += RookMaterial * BitOps.CountBits(state.blackRooks);
                blackMaterial += KnightMaterial * BitOps.CountBits(state.blackKnights);
                blackMaterial += BishopMaterial * BitOps.CountBits(state.blackBishops);
                blackMaterial += QueenMaterial * BitOps.CountBits(state.blackQueens);
                if (state.blackQueens != 0)
                {
                    blackStatic += QueenExistenceBonus;
                }
                if (state.blackCastleKS)
                {
                    blackStatic += CastlePotentialBonus;
                }
                if (state.blackCastleQS)
                {
                    blackStatic += CastlePotentialBonus;
                }
                if (!state.blackCastleKS && !state.blackCastleQS && state.stateHistory.Count() < CastleTurnCutOff)
                {
                    if (state.blackKing == blackKSDest || state.blackKing == blackQSDest || ((state.blackRooks & (blackKSRookDest | blackQSRookDest)) != 0))
                    {
                        blackStatic += CastleBonus;
                    }
                }
                if (BitOps.CountBits(state.blackBishops) >= 2) {
                    blackMaterial += BishopPairBonus;
                }
            } // End Black Potential
        }

        UInt64 whitePotential = materialWeight * whiteMaterial + staticWeight * whiteStatic;
        UInt64 blackPotential = materialWeight * blackMaterial + staticWeight * blackStatic;

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
        
        Int64 eval;
        if (playerIsWhite)
        {
            eval = (Int64)(potentialWeight*(whitePotential - blackPotential) + positionWeight*(whitePosition - blackPosition));
        } else {
            eval = (Int64)(potentialWeight*(blackPotential - whitePotential) + positionWeight*(blackPosition - whitePosition));
        }
        UpdateTT(state.zobristHash, null, 0, eval);
        return eval;
    }
}
