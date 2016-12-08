using UnityEngine;
using System.Collections.Generic;

/*
 * This singleton manages the whole chess game
 *  - board data (see BoardState class)
 *  - piece models instantiation
 *  - player interactions (piece grab, drag and release)
 *  - AI update calls (see UpdateAITurn and ChessAI class)
 */

public partial class ChessGameMgr : MonoBehaviour {

    #region singleton
    static ChessGameMgr instance = null;
    public static ChessGameMgr Instance {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ChessGameMgr>();
            return instance;
        }
    }
    #endregion

    private ChessAI chessAI = null;
    private Transform boardTransform = null;
    private static int BOARD_SIZE = 8;
    private int pieceLayerMask;
    private int boardLayerMask;

    #region enums
    public enum EPieceType : uint
    {
        None = 0,
        King,
        Queen,
        Rook,
        Knight,
        Bishop,
        Pawn
    }

    public enum EChessTeam
    {
        White = 0,
        Black,
        None
    }

    public enum ETeamFlag : uint
    {
        None = 1 << 0,
        Friend = 1 << 1,
        Enemy = 1 << 2
    }
    #endregion

    #region structs and classes
    public struct BoardSquare
    {
        public EPieceType Piece;
        public EChessTeam Team;
    }

    public struct Move
    {
        public int From;
        public int To;

        public override bool Equals(object o)
        {
            try
            {
                return (bool)(this == (Move)o);
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return From + To;
        }

        public static bool operator == (Move move1, Move move2)
        {
            return move1.From == move2.From && move1.To == move2.To;
        }

        public static bool operator != (Move move1, Move move2)
        {
            return move1.From != move2.From || move1.To != move2.To;
        }
    }

    #endregion

    #region chess game methods

    BoardState boardState = null;
    public BoardState GetBoardState() { return boardState; }

    EChessTeam teamTurn;

    List<uint> scores;

    public delegate void PlayerTurnEvent(bool isWhiteMove);
    public event PlayerTurnEvent OnPlayerTurn = null;

    public delegate void ScoreUpdateEvent(uint whiteScore, uint blackScore);
    public event ScoreUpdateEvent OnScoreUpdated = null;

    public void PrepareGame(bool resetScore = true)
    {
        chessAI = ChessAI.Instance;

        // Start game
        boardState.Reset();
        teamTurn = EChessTeam.White;
        if (scores == null)
        {
            scores = new List<uint>();
            scores.Add(0);
            scores.Add(0);
        }
        if (resetScore)
        {
            scores.Clear();
            scores.Add(0);
            scores.Add(0);
        }
    }

    public void PlayTurn(Move move)
    {
        if (boardState.IsValidMove(teamTurn, move))
        {
            boardState.PlayUnsafeMove(move);

            EChessTeam otherTeam = (teamTurn == EChessTeam.White) ? EChessTeam.Black : EChessTeam.White;
            if (boardState.DoesTeamLose(otherTeam))
            {
                scores[(int)teamTurn]++;
                if (OnScoreUpdated != null)
                    OnScoreUpdated(scores[0], scores[1]);

                PrepareGame(false);
            }
            else
            {
                teamTurn = otherTeam;
            }
            // raise event
            if (OnPlayerTurn != null)
                OnPlayerTurn(teamTurn == EChessTeam.White);
        }
    }

    public bool IsPlayerTurn()
    {
        return teamTurn == EChessTeam.White;
    }

    public BoardSquare GetSquare(int pos)
    {
        return boardState.Squares[pos];
    }

    public uint GetScore(EChessTeam team)
    {
        return scores[(int)team];
    }

    private void UpdateBoardPiece(Transform pieceTransform, int destPos)
    {
        pieceTransform.position = GetWorldPos(destPos);
    }

    private Vector3 GetWorldPos(int pos)
    {
        Vector3 piecePos = boardTransform.position;
        piecePos.y += zOffset;
        piecePos.x = -widthOffset + pos % BOARD_SIZE;
        piecePos.z = -widthOffset + pos / BOARD_SIZE;

        return piecePos;
    }

    private int GetBoardPos(Vector3 worldPos)
    {
        int xPos = Mathf.FloorToInt(worldPos.x + widthOffset) % BOARD_SIZE;
        int zPos = Mathf.FloorToInt(worldPos.z + widthOffset);

        return xPos + zPos * BOARD_SIZE;
    }

    #endregion

    #region MonoBehaviour

    private TeamPieces[] teamPiecesArray = new TeamPieces[2];
    private float zOffset = 0.5f;
    private float widthOffset = 3.5f;

    // Use this for initialization
    void Start () {

        pieceLayerMask = 1 << LayerMask.NameToLayer("Piece");
        boardLayerMask = 1 << LayerMask.NameToLayer("Board");

        boardTransform = GameObject.FindGameObjectWithTag("Board").transform;

        LoadPiecesPrefab();

        boardState = new BoardState();

        PrepareGame();

        CreatePieces();

        if (OnPlayerTurn != null)
            OnPlayerTurn(teamTurn == EChessTeam.White);
        if (OnScoreUpdated != null)
            OnScoreUpdated(scores[0], scores[1]);
    }

	// Update is called once per frame
	void Update () {

        if (teamTurn == EChessTeam.White)
            UpdatePlayerTurn();
        else
            UpdateAITurn();
    }
    #endregion

    #region pieces

    Dictionary<EPieceType, GameObject> WhitePiecesPrefab = new Dictionary<EPieceType, GameObject>();
    Dictionary<EPieceType, GameObject> BlackPiecesPrefab = new Dictionary<EPieceType, GameObject>();

    void LoadPiecesPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhitePawn");
        WhitePiecesPrefab.Add(EPieceType.Pawn, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKing");
        WhitePiecesPrefab.Add(EPieceType.King, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteQueen");
        WhitePiecesPrefab.Add(EPieceType.Queen, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteBishop");
        WhitePiecesPrefab.Add(EPieceType.Bishop, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKnight");
        WhitePiecesPrefab.Add(EPieceType.Knight, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteRook");
        WhitePiecesPrefab.Add(EPieceType.Rook, prefab);

        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackPawn");
        BlackPiecesPrefab.Add(EPieceType.Pawn, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKing");
        BlackPiecesPrefab.Add(EPieceType.King, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackQueen");
        BlackPiecesPrefab.Add(EPieceType.Queen, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackBishop");
        BlackPiecesPrefab.Add(EPieceType.Bishop, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKnight");
        BlackPiecesPrefab.Add(EPieceType.Knight, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackRook");
        BlackPiecesPrefab.Add(EPieceType.Rook, prefab);
    }

    void CreatePieces()
    {
        // Instantiate all pieces according to board data
        teamPiecesArray[0] = new TeamPieces();
        teamPiecesArray[1] = new TeamPieces();

        Dictionary<EPieceType, GameObject> crtTeamPrefabs = null;
        int crtPos = 0;
        foreach (BoardSquare square in boardState.Squares)
        {
            crtTeamPrefabs = (square.Team == EChessTeam.White) ? WhitePiecesPrefab : BlackPiecesPrefab;
            if (square.Piece != EPieceType.None)
            {
                GameObject crtPiece = Instantiate<GameObject>(crtTeamPrefabs[square.Piece]);
                teamPiecesArray[(int)square.Team].StorePiece(crtPiece, square.Piece);

                // set position
                Vector3 piecePos = boardTransform.position;
                piecePos.y += zOffset;
                piecePos.x = -widthOffset + crtPos % BOARD_SIZE;
                piecePos.z = -widthOffset + crtPos / BOARD_SIZE;
                crtPiece.transform.position = piecePos;
            }
            crtPos++;
        }
    }

    void UpdatePieces()
    {
        teamPiecesArray[0].Hide();
        teamPiecesArray[1].Hide();

        for (int i = 0; i < boardState.Squares.Count; i++)
        {
            BoardSquare square = boardState.Squares[i];
            if (square.Team == EChessTeam.None)
                continue;

            int teamId = (int)square.Team;
            EPieceType pieceType = square.Piece;

            teamPiecesArray[teamId].SetPieceAtPos(pieceType, GetWorldPos(i));
        }
    }

    #endregion

    #region gameplay

    Transform grabbed = null;
    float maxDistance = 100f;
    int startPos = 0;
    int destPos = 0;

    void UpdateAITurn()
    {
        Move move = chessAI.ComputeMove();
        PlayTurn(move);

        UpdatePieces();
    }

    void UpdatePlayerTurn()
    {
        if (Input.GetMouseButton(0))
        {
            if (grabbed)
                ComputeDrag();
            else
                ComputeGrab();
        }
        else if (grabbed != null)
        {
            // find matching square when releasing grabbed piece
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
            {
                grabbed.root.position = hit.transform.position + Vector3.up * zOffset;
            }

            destPos = GetBoardPos(grabbed.root.position);
            if (startPos != destPos)
            {
                Move move = new Move();
                move.From = startPos;
                move.To = destPos;

                PlayTurn(move);

                UpdatePieces();
            }
            else
            {
                grabbed.root.position = GetWorldPos(startPos);
            }
            grabbed = null;
        }
    }

    void ComputeDrag()
    {
        // drag grabbed piece on board
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
        {
            grabbed.root.position = hit.point;
        }
    }

    void ComputeGrab()
    {
        // grab a new chess piece from board
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxDistance, pieceLayerMask))
        {
            int pos = GetBoardPos(hit.transform.position);
            if (GetSquare(pos).Team == teamTurn)
            {
                grabbed = hit.transform;
                startPos = pos;
            }
        }
    }

    #endregion
}
