using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour 
{
	// Static Variables
	private static LevelManager ms_levelManager = null;

	// Editable Variables
	[SerializeField] private GameObject mGO_Tiles = null;
	[SerializeField] private Color m_ColorDarkTiles = Color.black;

	// Un-editable Variables
	private bool[,] marr2_levelData = null;				// marr2_levelData: The level data of the current level
	private bool[,] marr2_tileOccupiedAtAwake = null;	// marr2_tileOccupiedAtAwake: This is used for initialisation at the start of the game
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
		marr2_tileOccupiedAtAwake = new bool[m_nLevelLength, m_nLevelLength];
	}

	// Start(): Use this for initialisation
	void Start()
	{
		// for: Every registered character of type Enemy, calculates the AStar
		//for (int i = 0; i < mList_Character.Count; i++)
		//	if (mList_Character[i].CharacterType == EnumCharacterType.Enemy)
		//		(mList_Character[i] as UnityEngine.Object as System.Object as Enemy).RemoteCallAStar();
		string str = "| ";
		for (int i = 0; i < mList_Character.Count; i++)
			str += (mList_Character[i].gameObject.name + " | ");

		Debug.Log(str);
			ExecuteNextTurn();
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

			if (_characterType.CharacterType == EnumCharacterType.Enemy)
			{
				// if: There is a tile at position [x, y]
				if (marr2_levelData[x, y])
					if (!marr2_tileOccupiedAtAwake[x, y])
					{
						_characterType.transform.position = _characterType.AStarInstance.GridIndex2Position(x, y);
						bIsFoundPlace = true;
					}
			}
			// else: If the character type is typeof Player
			else
				bIsFoundPlace = true;

		} while (!bIsFoundPlace);

		if (_characterType.CharacterType == EnumCharacterType.Player)
			m_playerInstance = _characterType.GetComponent<Player>();

		mList_Character.Add(_characterType);
		return true;
	}

	/// <summary>
	/// Execute a next turn. This will be used to start the game as well
	/// </summary>
	public void ExecuteNextTurn()
	{
		// if: This is the first round, allow the player to start first
		if (m_nCurrentTurn == -1)
		{
			//for (int i = 0; i < mList_Character.Count; i++)
			//	if (mList_Character[i].CharacterType == EnumCharacterType.Player)
			//		m_nCurrentTurn = i;
			m_nCurrentTurn = 0;
		}
		else
			m_nCurrentTurn = (m_nCurrentTurn + 1) % mList_Character.Count;

		//Debug.Log(mList_Character.Count);
		mList_Character[m_nCurrentTurn].ExecuteTurn();
	}

	/// <summary>
	/// Inefficiently returns the enemy's constrain
	/// </summary>
	/// <returns></returns>
	public bool[,] GetEnemyConstrain()
	{
		bool[,] arr2_characterConstrain = new bool[m_nLevelLength, m_nLevelLength];
		for (int i = 0; i < arr2_characterConstrain.GetLength(0); i++)
			for (int j = 0; j < arr2_characterConstrain.GetLength(1); j++)
				arr2_characterConstrain[j, i] = true;

		for (int i = 0; i < mList_Character.Count; i++)
			if (mList_Character[i].CharacterType == EnumCharacterType.Enemy)
			{
				int[] arr_gridPos = mList_Character[i].AStarInstance.Position2GridCoords(mList_Character[i].transform.position);
				arr2_characterConstrain[arr_gridPos[0], arr_gridPos[1]] = false;
			}

		return arr2_characterConstrain;
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
