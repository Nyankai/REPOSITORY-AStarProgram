using UnityEngine;
using System.Collections;

public enum EnumPieceType { Null = 0, Rook, Bishop, Knight, King };
public enum EnumCharacterType { Player = 0, Enemy = 1 };

public abstract class Character : MonoBehaviour 
{
	protected AStar m_AStar = null;
	protected EnumCharacterType menum_CharacterType;

	/// <summary>
	/// Execute a single turn of the character
	/// </summary>
	public abstract void ExecuteTurn();

	// Getter-Setter Function
	public EnumCharacterType CharacterType { get { return menum_CharacterType; } }
	public AStar AStarInstance { get { return m_AStar; } }
}
