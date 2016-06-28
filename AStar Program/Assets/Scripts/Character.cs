using UnityEngine;
using System.Collections;

public enum EnumPieceType { Null = 0, Rook, Bishop, Knight, King };
public enum EnumCharacterType { Player = 0, Enemy = 1, Null };

public abstract class Character : MonoBehaviour 
{
	protected int m_nX = -1;
	protected int m_nY = -1;
	protected AStar m_AStar = null;
	protected EnumCharacterType menum_CharacterType;

	/// <summary>
	/// Execute a single turn of the character
	/// </summary>
	public abstract void ExecuteTurn();

	// Getter-Setter Function
	/// <summary>
	/// Return the character type of the current Character
	/// </summary>
	public EnumCharacterType CharacterType { get { return menum_CharacterType; } }

	/// <summary>
	/// Return the current AStarInstance of the character;
	/// </summary>
	public AStar AStarInstance { get { return m_AStar; } }

	/// <summary>
	/// Return the x-coordinates of the character
	/// </summary>
	public int X { get { return m_nX; } set { m_nX = value; } }

	/// <summary>
	/// Return the y-coordinates of the character
	/// </summary>
	public int Y { get { return m_nY; } set { m_nY = value; } }
}
