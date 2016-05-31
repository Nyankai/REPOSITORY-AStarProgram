using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour 
{
	// Static Variables
	private static Level m_currentLevel = null;
	private static List<Level> mList_Level = new List<Level>();	// mList_Level: Stores all the levels of in the game

	// Editor Variables
	[SerializeField] private int m_nLevelSize = 5;			// m_nLevelSize: The number of section from one side of the current level
	[SerializeField] private int m_nLevelSectionSize = 5;	// m_nLevelSectionSize: The number of tiles in one section

	[SerializeField] private List<LevelSection> mList_LevelSectionType = new List<LevelSection>();
	[SerializeField] private LevelSection[] marr_LevelSectionData = null;

	// Private Variables
	private int m_nTotalChance = 0;
	private bool[,] marr2_bLevelData = null;	// marr2_bLevelData: This will only be used as a completed map
	
	// Private Functions
	void Awake()
	{
		if (m_currentLevel == null)
			m_currentLevel = this;
		else
			Destroy(this);
	}

	// Public Functions
	/// <summary>
	/// Generates a new level
	/// </summary>
	/// <returns> Returns if a level is generated </returns>
	public bool Generate()
	{
		bool[,] levelOutput = null;
		return Generate(out levelOutput);
	}

	/// <summary>
	/// Generates a new level
	/// </summary>
	/// <param name="_levelOutput"> Returns the level generated after the this method runs</param>
	/// <returns> Returns if a level is generated </returns>
	public bool Generate(out bool[,] _levelOutput)
	{
		// Generation Checks
		if (mList_LevelSectionType.Count == 0)
		{
			Debug.LogWarning("Level.Generate(): mList_LevelSectionType size is zero. There is no section to generate!");
			_levelOutput = null;
			return false;
		}
		if (m_nLevelSize <= 0)
		{
			Debug.LogWarning("Level.Generate(): Invalid Level Size (m_nLevelSize = " + m_nLevelSize + "). m_nLevelSize must be more than 0!");
			_levelOutput = null;
			return false;
		}
		if (m_nLevelSectionSize <= 0)
		{
			Debug.LogWarning("Level.Generate(): Invalid Level-section Size (m_nLevelSectionSize = " + m_nLevelSectionSize + "). m_nLevelSection must be more than 0!");
			_levelOutput = null;
			return false;
		}

		// Variables Re-intialization
		marr2_bLevelData = new bool[m_nLevelSize * m_nLevelSectionSize, m_nLevelSize * m_nLevelSectionSize];

		// for: Calculates the totalChance count
		for (int i = 0; i < mList_LevelSectionType.Count; i++)
			m_nTotalChance += mList_LevelSectionType[i].Chance;

		// for, for: each rows and column of the level-section grid
		for (int i = 0; i < m_nLevelSize; i++)
			for (int j = 0; j < m_nLevelSize; j++)
			{
				// Finding a new level-section to spawn
				int nFinalChance = (int)(UnityEngine.Random.value * (float)m_nTotalChance);
				int nCurrentChance = 0;
				LevelSection levelSection = null;

				for (int k = 0; k < mList_LevelSectionType.Count; k++)
				{
					if (nCurrentChance >= nFinalChance)
					{
						levelSection = mList_LevelSectionType[k];
						break;
					}
					nCurrentChance += mList_LevelSectionType[k].Chance;
				}

				if (levelSection == null)
					levelSection = mList_LevelSectionType[mList_LevelSectionType.Count - 1];

				// for, for: each rows and columns of the tile grid
				for (int l = 0; l < m_nLevelSectionSize; l++)
					for (int m = 0; m < m_nLevelSectionSize; m++)
					{
						// if: It is a checked tile in the raw-data array
						if (levelSection.RawData[m + l * m_nLevelSectionSize])
						{
							// if: random is within range (means it will spawn)
							if (UnityEngine.Random.value < 1f - levelSection.TilePercentage / 2f)
								marr2_bLevelData[m + j * m_nLevelSectionSize, l + i * m_nLevelSectionSize] = true;
						}
						// else: it is not a checked tile in the raw-data array
						else
						{
							// if: random is within range (means it will spawn)
							if (UnityEngine.Random.value < levelSection.TilePercentage / 2f)
								marr2_bLevelData[m + j * m_nLevelSectionSize, l + i * m_nLevelSectionSize] = true;
						}
					}
			}

		_levelOutput = marr2_bLevelData;
		return true;
	}

	// Getter-Setter Functions
	/// <summary>
	/// Get the reference of the current level loaded. If a new level is needed, use "INSERT LEVEL GENERATOR NAME" instead
	/// </summary>
	public static Level CurrentLevel { get { return m_currentLevel; } }

	/// <summary>
	/// Returns the number of section in the current level
	/// </summary>
	public int SectionCount { get { return m_nLevelSize * m_nLevelSize; } }

	/// <summary>
	/// Returns the total number of initialisable tiles in the current level
	/// </summary>
	public int TileCount { get { return m_nLevelSize * m_nLevelSize * m_nLevelSectionSize * m_nLevelSectionSize; } }

	public int TileLength { get { return m_nLevelSize * m_nLevelSectionSize; } }

	/// <summary>
	/// Returns the generated level as a double-boolean array
	/// </summary>
	public bool[,] GenerateData { get { return marr2_bLevelData; } }
}