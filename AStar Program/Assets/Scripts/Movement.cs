using UnityEngine;
using System.Collections;

public class Movement
{
	// Static Functions
	/// <summary>
	/// Returns the movement type based on specified
	/// </summary>
	/// <param name="_enumPiece"> The movement of the piece type to be retrieved </param>
	/// <returns> Returns an array of movements </returns>
	public static int[,] GetMovementType(EnumPieceType _enumPiece)
	{
		switch (_enumPiece)
		{
 			case EnumPieceType.Bishop: return msarr2_nBishop;
			case EnumPieceType.King: return msarr2_nKing;
			case EnumPieceType.Knight: return msarr2_nKnight;
			case EnumPieceType.Rook: return msarr2_nRook;
			default: return null;
		}
	}

	// Getter-Setter Functions
	public static int[,] msarr2_nRook = new int[,] {
		{1, 0}, {0, 1}, {-1, 0}, {0, -1}
	};

	public static int[,] msarr2_nBishop = new int[,] {
		{1, 1}, {1, -1}, {-1, 1}, {-1, -1}
	};

	public static int[,] msarr2_nKnight = new int[,] {
		{2, 1}, {-2, 1}, {2, -1}, {-2, -1}, {1, 2}, {-1, 2}, {1, -2}, {-1, -2}
	};

	public static int[,] msarr2_nKing = new int[,] {
		{1, 1}, {-1, 1}, {1, -1}, {-1, -1}, {1, 0}, {-1, 0}, {0, 1}, {0, -1}
	};
}
