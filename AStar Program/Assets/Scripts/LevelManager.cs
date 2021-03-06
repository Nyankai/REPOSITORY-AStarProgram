﻿using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour 
{
	// Static Variables
	private static LevelManager ms_levelManager = null;

	// Editable Variables
	[SerializeField] private GameObject mGO_Tiles = null;
	[SerializeField] private Color m_ColorDarkTiles = Color.black;
	[SerializeField] private int m_nEnemyStartCount = 20;
	[SerializeField] private GameObject[] marr_GOEnemyTypes = null;

	// Un-editable Variables
	private bool[,] marr2_levelData = null;				// marr2_levelData: The level data of the current level
	private int m_nLevelLength = 0;						// m_nLevelLength: The length of the level

	private EnumCharacterType menum_currentTurn;
	private int m_nCurrentTurn = -1;
	private List<Character> mList_Character = new List<Character>();
	private Player m_playerInstance = null;

	// Private Functions
	// Awake(): is called at the start when initialized
	void Awake()
	{
		// Singleton
		if (ms_levelManager == null)
			ms_levelManager = this;
		else
			Destroy(this);

		// Generates a random level
		Level.CurrentLevel.Generate();
		marr2_levelData = Level.CurrentLevel.GenerateData;
		// for, for: Spawns the tiles based on their locations
		for (int i = 0; i < marr2_levelData.GetLength(0); i++)
			for (int j = 0; j < marr2_levelData.GetLength(1); j++)
			{
				if (marr2_levelData[j, i])
				{
					GameObject currentTile = Instantiate(mGO_Tiles, new Vector3(j, 0f, i), Quaternion.Euler(90f, 0f, 0f)) as GameObject;
					if ((float)(i + j) % 2f == 1f)
						if (currentTile.GetComponent<SpriteRenderer>() != null)
							currentTile.GetComponent<SpriteRenderer>().color = m_ColorDarkTiles;
					currentTile.transform.SetParent(GameObject.Find("GROUP_Tiles").transform);
				}
			}

		// Variables Definition
		m_nLevelLength = Level.CurrentLevel.TileLength;
	}

	// Start(): Use this for initialisation
	void Start()
	{
		// Generate random enemies
		for (int i = 0; i < m_nEnemyStartCount; i++)
		{
			GameObject GOEnemy = Instantiate(marr_GOEnemyTypes[UnityEngine.Random.Range(0, marr_GOEnemyTypes.Length - 1)], Vector3.zero, Quaternion.identity) as UnityEngine.GameObject;
			GOEnemy.GetComponent<Enemy>().Initialise();
		}

		ExecuteNextTurn();
		CameraManager.Instance.LookAt(PlayerInstance.transform.position, false);
	}

	// Public Functions
	/// <summary>
	/// Adds a character into the turn list, so that the level manager can execute its turn. Since the position of this piece is not defined, it will spawn on a random tile
	/// </summary>
	/// <param name="_characterType"> The character to add into the list </param>
	/// <returns> Returns if the character is added into the list. If false, the character is already in the list</returns>
	public bool AddCharacter(Character _characterType)
	{
		if (mList_Character.Contains(_characterType))
			return false;

		bool bIsFoundPlace = false;
		do
		{
			int x = (int)(UnityEngine.Random.value * (float)m_nLevelLength);
			int y = (int)(UnityEngine.Random.value * (float)m_nLevelLength);
			
			// if: The current character type is enemy
			if (_characterType.CharacterType == EnumCharacterType.Enemy)
			{
				_characterType.AStarInstance.TargetTransform = PlayerInstance.transform;
				// if: There is a tile at position [x, y]
				if (marr2_levelData[x, y])
					// if: The current slot is taken by another piece
					if (GetCharacterConstrain(EnumCharacterType.Null)[x, y])
					{
						_characterType.transform.position = _characterType.AStarInstance.GridIndex2Position(x, y);
						_characterType.X = x;
						_characterType.Y = y;
						bIsFoundPlace = true;
					}
			}
			// else: If the character type is typeof Player
			else
			{
				m_playerInstance = _characterType.GetComponent<Player>();

				// if: There is a tile at position [x, y]
				if (marr2_levelData[x, y])
					// if: The current slot is taken by another piece
					if (GetCharacterConstrain(EnumCharacterType.Null)[x, y])
					{
						_characterType.transform.position = new Vector3((float)x, 0f, (float)y);
						_characterType.X = x;
						_characterType.Y = y;
						bIsFoundPlace = true;
					}
			}

		} while (!bIsFoundPlace);

		mList_Character.Add(_characterType);
		return true;
	}

	/// <summary>
	/// Removes the character from the character array
	/// </summary>
	/// <param name="_characterType"> THe character to remove from the list </param>
	public bool RemoveCharacter(Character _characterType)
	{
		if (mList_Character.Contains(_characterType))
		{
			mList_Character.Remove(_characterType);
			Destroy(_characterType.gameObject);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Removes the character from the character array
	/// </summary>
	/// <param name="_x"> The x coordinates of the character </param>
	/// <param name="_y"> The y coordinates of the character </param>
	/// <returns></returns>
	public bool RemoveCharacter(int _x, int _y)
	{
		Character character = GetCharacter(_x, _y);
		if (character == null)
			return false;
		else
			return RemoveCharacter(character);
	}

	/// <summary>
	/// Execute a next turn. This will be used to start the game as well
	/// </summary>
	public void ExecuteNextTurn()
	{
		// if: This is the first round, allow the player to start first
		if (m_nCurrentTurn == -1)
		{
			for (int i = 0; i < mList_Character.Count; i++)
				if (mList_Character[i].CharacterType == EnumCharacterType.Player)
					m_nCurrentTurn = i;
		}
		else
			m_nCurrentTurn = (m_nCurrentTurn + 1) % mList_Character.Count;

		//Debug.Log(mList_Character.Count);
		mList_Character[m_nCurrentTurn].ExecuteTurn();
	}

	/// <summary>
	/// Inefficiently returns the character's constrain
	/// </summary>
	/// <param name="_enumCharacter"> Specifies a sepcific character type to check for contrain. Returns all character types' constrains if NULL is chosen instead </param>
	/// <returns> Returns the boolean array grid of all the character's constrain </returns>
	public bool[,] GetCharacterConstrain(EnumCharacterType _enumCharacter)
	{
		// arr2_characterConstrain: Create a new constrain and set everywhere to be walkable
		bool[,] arr2_characterConstrain = new bool[m_nLevelLength, m_nLevelLength];
		for (int i = 0; i < arr2_characterConstrain.GetLength(0); i++)
			for (int j = 0; j < arr2_characterConstrain.GetLength(1); j++)
				arr2_characterConstrain[j, i] = true;

		// for: Every registered character in the list...
		for (int i = 0; i < mList_Character.Count; i++)
		{
			// if: No character type is specified
			if (_enumCharacter == EnumCharacterType.Null)
				arr2_characterConstrain[mList_Character[i].X, mList_Character[i].Y] = false;
			// else if: A certain character type is specified
			else if (mList_Character[i].CharacterType == _enumCharacter)
				arr2_characterConstrain[mList_Character[i].X, mList_Character[i].Y] = false;
		}

		return arr2_characterConstrain;
	}

	/// <summary>
	/// Returns the character as the specific coordinates. Returns null if there is no character at the coordinates
	/// </summary>
	/// <param name="_x"> The x coordinates of the character </param>
	/// <param name="_y"> The y coordinates of the character </param>
	/// <returns></returns>
	public Character GetCharacter(int _x, int _y)
	{
		for (int i = 0; i < mList_Character.Count; i++)
		{
			if (mList_Character[i].X == _x)
				if (mList_Character[i].Y == _y)
					return mList_Character[i];
		}
		return null;
	}

	// Getter-Setter Functions
	/// <summary>
	/// Returns the single instance of LevelManager
	/// </summary>
	public static LevelManager Instance { get { return ms_levelManager; } }

	/// <summary>
	/// Returns the data of the current level
	/// </summary>
	public bool[,] LevelData { get { return marr2_levelData; } }

	/// <summary>
	/// Returns the instance of the current player
	/// </summary>
	public Player PlayerInstance { get { return m_playerInstance; } }
}
