using UnityEngine;
using System.Collections;
using DaburuTools;
using DaburuTools.Action;

public class Player : Character
{
	// Uneditable Functions
	private int m_nGridX = -1;
	private int m_nGridY = -1;

	private bool[,] marr2_gridData = null;
	private bool m_bIsExecuteTurn = false;

	// Protected Functions
	public override void ExecuteTurn()
	{
		m_bIsExecuteTurn = true;
	}

	// Private Functions
	void Awake()
	{
		Debug.Log(LevelManager.Instance.AddCharacter(this));
		marr2_gridData = LevelManager.Instance.LevelData;
		menum_CharacterType = EnumCharacterType.Player;
	}

	void Update()
	{
		if (!m_bIsExecuteTurn)
			return;


		m_bIsExecuteTurn = false;
	}
}
