using UnityEngine;
using System.Collections;

public class LevelSection : ScriptableObject
{
	// Editor Variables
	[SerializeField] private int m_nLevelSectionSize = 0;
	[SerializeField] private int m_nLevelSectionChance = 1;

	[SerializeField] private float m_fTilePercentage = 0f;
	[SerializeField] private bool[] marr_bLevelSectionRawData;

	private bool m_bIsLevelSectionGenerate = false;
	private bool[,] marr2_bLevelSectionGenerateData;

	// Constructor
	public LevelSection(int _nLevelSection, float _fTilePercentage)
	{
		m_nLevelSectionSize = _nLevelSection;
		m_fTilePercentage = _fTilePercentage;
		marr_bLevelSectionRawData = new bool[m_nLevelSectionSize * m_nLevelSectionSize];
		marr2_bLevelSectionGenerateData = new bool[m_nLevelSectionSize, m_nLevelSectionSize];
	}

	public LevelSection(int _nLevelSection, float _fTilePercentage, bool[] _arr_bLevelSectionData)
	{
		m_nLevelSectionSize = _nLevelSection;
		m_fTilePercentage = _fTilePercentage;
		marr_bLevelSectionRawData = _arr_bLevelSectionData;
		marr2_bLevelSectionGenerateData = new bool[m_nLevelSectionSize, m_nLevelSectionSize];
	}

	// Public Functions

	// Getter-Setter Functions
	/// <summary>
	/// Returns the size of one side of the current level section
	/// </summary>
	public int Size { get { return m_nLevelSectionSize; } }

	/// <summary>
	/// Returns the chance of the current level-section
	/// </summary>
	public int Chance { get { return m_nLevelSectionChance; } }

	/// <summary>
	/// Returns or define the percentage offset of a certain tile check
	/// </summary>
	public float TilePercentage { get { return m_fTilePercentage; } set { m_fTilePercentage = value; } }

	/// <summary>
	/// Returns the raw data (un-generated) of the current level section in a double boolean array
	/// </summary>
	public bool[] RawData { get { return marr_bLevelSectionRawData; } }

	/// <summary>
	/// Returns the generate data of the current level section in a double boolean array
	/// </summary>
	public bool[,] GenerateData { get { return marr2_bLevelSectionGenerateData; } }
}

