using UnityEngine;
using System.Collections.Generic;

/*
 * This class holds chess piece gameObjects for a team
 * Instantiated for each team by the ChessGameMgr
 * It can hide or set a piece at a given position
 */

public partial class ChessGameMgr
{
    public class TeamPieces
    {
        private Dictionary<EPieceType, GameObject[]> pieceTypeDict;

        public TeamPieces()
        {
            pieceTypeDict = new Dictionary<EPieceType, GameObject[]>();

            pieceTypeDict.Add(EPieceType.Pawn, new GameObject[BOARD_SIZE]);
            pieceTypeDict.Add(EPieceType.King, new GameObject[1]);
            pieceTypeDict.Add(EPieceType.Queen, new GameObject[1]);
            pieceTypeDict.Add(EPieceType.Bishop, new GameObject[2]);
            pieceTypeDict.Add(EPieceType.Knight, new GameObject[2]);
            pieceTypeDict.Add(EPieceType.Rook, new GameObject[2]);
        }

        public void Hide()
        {
            foreach(KeyValuePair<EPieceType, GameObject[]> kvp in pieceTypeDict)
            {
                foreach (GameObject gao in kvp.Value)
                    gao.SetActive(false);
            }
        }

        private void StorePieceInCategory(GameObject crtPiece, GameObject[] pieceArray)
        {
            int i = 0;
            while (i < pieceArray.Length && pieceArray[i] != null) i++;
            pieceArray[i] = crtPiece;
        }

        public void StorePiece(GameObject crtPiece, EPieceType pieceType)
        {
            StorePieceInCategory(crtPiece, pieceTypeDict[pieceType]);
        }

        private void SetPieceCategoryAt(GameObject[] pieceArray, Vector3 pos)
        {
            int i = 0;
            while (i < pieceArray.Length && pieceArray[i].activeSelf) i++;

            pieceArray[i].SetActive(true);
            pieceArray[i].transform.position = pos;
        }

        public void SetPieceAtPos(EPieceType pieceType, Vector3 pos)
        {
            SetPieceCategoryAt(pieceTypeDict[pieceType], pos);
        }
    }
}
