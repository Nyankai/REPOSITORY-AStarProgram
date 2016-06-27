using UnityEngine;
using System.Collections;

public class Movement
{
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
