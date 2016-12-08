using UnityEngine;
using System.Collections.Generic;

/*
 * The BoardState class stores the internal data values of the board
 * It holds a list of BoardSquare structs that contains info for each square : the type of piece (pawn, king, ... , none) and the team of the piece
 * It also contains methods to get valid moves for each type of piece accoring to the current board configuration
 * It can apply a selected move for a piece and eventually reset its values to default
 */

public partial class ChessGameMgr
{
    public struct BoardPos
    {

        public int X { get; set; }
        public int Y { get; set; }

        //public BoardPos() { X = 0; Y = 0; }
        public BoardPos(int pos) { X = pos % BOARD_SIZE; Y = pos / BOARD_SIZE; }
        public BoardPos(int _x, int _y) { X = _x; Y = _y; }

        public static implicit operator int(BoardPos pos) { return pos.X + pos.Y * BOARD_SIZE; }

        static public int operator +(BoardPos pos1, BoardPos pos2)
        {
            int x = pos1.X + pos2.X;
            int y = pos1.Y + pos2.Y;

            return (x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE) ? new BoardPos(x, y) : -1;
        }

        public int GetRight()
        {
            return (X == BOARD_SIZE - 1) ? -1 : new BoardPos(X + 1, Y);
        }

        public int GetLeft()
        {
            return (X == 0) ? -1 : new BoardPos(X - 1, Y);
        }

        public int GetTop()
        {
            return (Y == BOARD_SIZE - 1) ? -1 : new BoardPos(X, Y + 1);
        }

        public int GetBottom()
        {
            return (Y == 0) ? -1 : new BoardPos(X, Y - 1);
        }
    }

    public class BoardState
    {
        public List<BoardSquare> Squares = null;

        public bool IsValidSquare(int pos, EChessTeam team, int teamFlag)
        {
            if (pos < 0)
                return false;

            bool isTeamValid = ((Squares[pos].Team == EChessTeam.None) && ((teamFlag & (int)ETeamFlag.None) > 0)) ||
                ((Squares[pos].Team != team && Squares[pos].Team != EChessTeam.None) && ((teamFlag & (int)ETeamFlag.Enemy) > 0));

            return isTeamValid;
        }

        public void AddMoveIfValidSquare(EChessTeam team, int from, int to, List<Move> moves, int teamFlag = (int)ETeamFlag.Enemy | (int)ETeamFlag.None)
        {
            if (IsValidSquare(to, team, teamFlag))
            {
                Move move;
                move.From = from;
                move.To = to;
                moves.Add(move);
            }
        }

        public void GetValidKingMoves(EChessTeam team, int pos, List<Move> moves)
        {
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(1, 0)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(1, 1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(0, 1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(-1, 1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(-1, 0)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(-1, -1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(0, -1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(1, -1)), moves);
        }

        public void GetValidQueenMoves(EChessTeam team, int pos, List<Move> moves)
        {
            GetValidRookMoves(team, pos, moves);
            GetValidBishopMoves(team, pos, moves);
        }

        public void GetValidPawnMoves(EChessTeam team, int pos, List<Move> moves)
        {
            int FrontPos = -1, LeftFrontPos = -1, RightFrontPos = -1;
            if (team == EChessTeam.White)
            {
                FrontPos = new BoardPos(pos).GetTop();
                if (FrontPos > 0)
                {
                    LeftFrontPos = new BoardPos(FrontPos).GetLeft();
                    RightFrontPos = new BoardPos(FrontPos).GetRight();
                }
                if (new BoardPos(pos).Y == 1)
                {
                    AddMoveIfValidSquare(team, pos, new BoardPos(FrontPos).GetTop(), moves, (int)ETeamFlag.None);
                }
            }
            else
            {
                FrontPos = new BoardPos(pos).GetBottom();
                if (FrontPos > 0)
                {
                    LeftFrontPos = new BoardPos(FrontPos).GetRight();
                    RightFrontPos = new BoardPos(FrontPos).GetLeft();
                }
                if (new BoardPos(pos).Y == 6)
                {
                    AddMoveIfValidSquare(team, pos, new BoardPos(FrontPos).GetBottom(), moves, (int)ETeamFlag.None);
                }
            }

            AddMoveIfValidSquare(team, pos, FrontPos, moves, (int)ETeamFlag.None);
            AddMoveIfValidSquare(team, pos, LeftFrontPos, moves, (int)ETeamFlag.Enemy);
            AddMoveIfValidSquare(team, pos, RightFrontPos, moves, (int)ETeamFlag.Enemy);
        }

        public void GetValidRookMoves(EChessTeam team, int pos, List<Move> moves)
        {
            bool bBreak = false;
            int TopPos = new BoardPos(pos).GetTop();
            while (!bBreak && TopPos >= 0 && Squares[TopPos].Team != team)
            {
                AddMoveIfValidSquare(team, pos, TopPos, moves);
                bBreak = Squares[TopPos].Team != EChessTeam.None;
                TopPos = new BoardPos(TopPos).GetTop();
            }

            bBreak = false;
            int BottomPos = new BoardPos(pos).GetBottom();
            while (!bBreak && BottomPos >= 0 && Squares[BottomPos].Team != team)
            {
                AddMoveIfValidSquare(team, pos, BottomPos, moves);
                bBreak = Squares[BottomPos].Team != EChessTeam.None;
                BottomPos = new BoardPos(BottomPos).GetBottom();
            }

            bBreak = false;
            int LeftPos = new BoardPos(pos).GetLeft();
            while (!bBreak && LeftPos >= 0 && Squares[LeftPos].Team != team)
            {
                AddMoveIfValidSquare(team, pos, LeftPos, moves);
                bBreak = Squares[LeftPos].Team != EChessTeam.None;
                LeftPos = new BoardPos(LeftPos).GetLeft();
            }

            bBreak = false;
            int RightPos = new BoardPos(pos).GetRight();
            while (!bBreak && RightPos >= 0 && Squares[RightPos].Team != team)
            {
                AddMoveIfValidSquare(team, pos, RightPos, moves);
                bBreak = Squares[RightPos].Team != EChessTeam.None;
                RightPos = new BoardPos(RightPos).GetRight();
            }
        }

        public void GetValidBishopMoves(EChessTeam team, int pos, List<Move> moves)
        {
            bool bBreak = false;
            int TopRightPos = new BoardPos(pos) + new BoardPos(1, 1);
            while (!bBreak && TopRightPos >= 0 && Squares[TopRightPos].Team != team)
            {

                AddMoveIfValidSquare(team, pos, TopRightPos, moves);
                bBreak = Squares[TopRightPos].Team != EChessTeam.None;
                TopRightPos = new BoardPos(TopRightPos) + new BoardPos(1, 1);
            }

            bBreak = false;
            int TopLeftPos = new BoardPos(pos) + new BoardPos(-1, 1);
            while (!bBreak && TopLeftPos >= 0 && Squares[TopLeftPos].Team != team)
            {

                AddMoveIfValidSquare(team, pos, TopLeftPos, moves);
                bBreak = Squares[TopLeftPos].Team != EChessTeam.None;
                TopLeftPos = new BoardPos(TopLeftPos) + new BoardPos(-1, 1);
            }

            bBreak = false;
            int BottomRightPos = new BoardPos(pos) + new BoardPos(1, -1);
            while (!bBreak && BottomRightPos >= 0 && Squares[BottomRightPos].Team != team)
            {

                AddMoveIfValidSquare(team, pos, BottomRightPos, moves);
                bBreak = Squares[BottomRightPos].Team != EChessTeam.None;
                BottomRightPos = new BoardPos(BottomRightPos) + new BoardPos(1, -1);
            }

            bBreak = false;
            int BottomLeftPos = new BoardPos(pos) + new BoardPos(-1, -1);
            while (!bBreak && BottomLeftPos >= 0 && Squares[BottomLeftPos].Team != team)
            {

                AddMoveIfValidSquare(team, pos, BottomLeftPos, moves);
                bBreak = Squares[BottomLeftPos].Team != EChessTeam.None;
                BottomLeftPos = new BoardPos(BottomLeftPos) + new BoardPos(-1, -1);
            }
        }

        public void GetValidKnightMoves(EChessTeam team, int pos, List<Move> moves)
        {
            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(1, 2), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(2, 1), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(-1, 2), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(-2, 1), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(1, -2), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(2, -1), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(-1, -2), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(-2, -1), moves);
        }

        public void GetValidMoves(EChessTeam team, List<Move> moves)
        {
            for (int i = 0; i < BOARD_SIZE * BOARD_SIZE; ++i)
            {
                if (Squares[i].Team == team)
                {
                    switch (Squares[i].Piece)
                    {
                        case EPieceType.King: GetValidKingMoves(team, i, moves); break;
                        case EPieceType.Queen: GetValidQueenMoves(team, i, moves); break;
                        case EPieceType.Pawn: GetValidPawnMoves(team, i, moves); break;
                        case EPieceType.Rook: GetValidRookMoves(team, i, moves); break;
                        case EPieceType.Bishop: GetValidBishopMoves(team, i, moves); break;
                        case EPieceType.Knight: GetValidKnightMoves(team, i, moves); break;
                        default: break;
                    }
                }
            }
        }

        public bool IsValidMove(EChessTeam team, Move move)
        {
            List<Move> validMoves = new List<Move>();
            GetValidMoves(team, validMoves);

            return validMoves.Contains(move);
        }

        public void PlayUnsafeMove(Move move)
        {
            Squares[move.To] = Squares[move.From];

            BoardSquare square = Squares[move.From];
            square.Piece = EPieceType.None;
            square.Team = EChessTeam.None;
            Squares[move.From] = square;
        }

        // approximation : opponent king must be "eaten" to win instead of detecting checkmate state
        public bool DoesTeamLose(EChessTeam team)
        {
            for (int i = 0; i < Squares.Count; ++i)
            {
                if (Squares[i].Team == team && Squares[i].Piece == EPieceType.King)
                {
                    return false;
                }
            }
            return true;
        }

        private void SetPieceAtSquare(int index, EChessTeam team, EPieceType piece)
        {
            if (index > Squares.Count)
                return;
            BoardSquare square = Squares[index];
            square.Piece = piece;
            square.Team = team;
            Squares[index] = square;
        }

        public void Reset()
        {
            if (Squares == null)
            {
                Squares = new List<BoardSquare>();

                // init squares
                for (int i = 0; i < BOARD_SIZE * BOARD_SIZE; i++)
                {
                    BoardSquare square = new BoardSquare();
                    square.Piece = EPieceType.None;
                    square.Team = EChessTeam.None;
                    Squares.Add(square);
                }
            }
            else
            {
                for (int i = 0; i < Squares.Count; ++i)
                {
                    SetPieceAtSquare(i, EChessTeam.None, EPieceType.None);
                }
            }

             // White
            for (int i = BOARD_SIZE; i < BOARD_SIZE*2; ++i)
            {
                SetPieceAtSquare(i, EChessTeam.White, EPieceType.Pawn);
            }
            SetPieceAtSquare(0, EChessTeam.White, EPieceType.Rook);
            SetPieceAtSquare(1, EChessTeam.White, EPieceType.Knight);
            SetPieceAtSquare(2, EChessTeam.White, EPieceType.Bishop);
            SetPieceAtSquare(3, EChessTeam.White, EPieceType.Queen);
            SetPieceAtSquare(4, EChessTeam.White, EPieceType.King);
            SetPieceAtSquare(5, EChessTeam.White, EPieceType.Bishop);
            SetPieceAtSquare(6, EChessTeam.White, EPieceType.Knight);
            SetPieceAtSquare(7, EChessTeam.White, EPieceType.Rook);

            // Black
            for (int i = BOARD_SIZE * (BOARD_SIZE - 2) ; i < BOARD_SIZE * (BOARD_SIZE - 1); ++i)
            {
                SetPieceAtSquare(i, EChessTeam.Black, EPieceType.Pawn);
            }
            int startIndex = BOARD_SIZE * (BOARD_SIZE - 1);
            SetPieceAtSquare(startIndex, EChessTeam.Black, EPieceType.Rook);
            SetPieceAtSquare(startIndex + 1, EChessTeam.Black, EPieceType.Knight);
            SetPieceAtSquare(startIndex + 2, EChessTeam.Black, EPieceType.Bishop);
            SetPieceAtSquare(startIndex + 3, EChessTeam.Black, EPieceType.Queen);
            SetPieceAtSquare(startIndex + 4, EChessTeam.Black, EPieceType.King);
            SetPieceAtSquare(startIndex + 5, EChessTeam.Black, EPieceType.Bishop);
            SetPieceAtSquare(startIndex + 6, EChessTeam.Black, EPieceType.Knight);
            SetPieceAtSquare(startIndex + 7, EChessTeam.Black, EPieceType.Rook);
        }
    }
}

