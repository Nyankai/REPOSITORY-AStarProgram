using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelSection))]
[CanEditMultipleObjects]
public class LevelSectionEditor : Editor
{
	// SerializedProperty Variables
	public SerializedProperty mSProp_LevelSectionSize;
	public SerializedProperty mSProp_LevelSectionChance;

	private SerializedProperty mSProp_TilePercentage;
	private SerializedProperty mSProp_LevelSectionRawData;

	// GUIContent Variables
	private GUIContent mGUICont_LevelSectionSize
		= new GUIContent("Level-Section Size", "The array size of the current level-section");
	private GUIContent mGUICont_LevelSectionChance
		= new GUIContent("Spawning Chance", "The percentage of spawning this tile will based on the percentage from the total spawning chance combined");

	private GUIContent mGUICont_LevelSectionRawData
		= new GUIContent("", "Check this to determine if there should be a tile spawning here");
	private GUIContent mGUICont_TilePercentage
		= new GUIContent("Spawning Offset Rate", "Determine how much the checked and unchecked tiles affects the spawn rate");

	// GUIStyle Variables
	private GUIStyle mGUISty_Rect = new GUIStyle();

	// Boolean Checks
	private bool m_bIsLevelSectionSize = false;

	void OnEnable()
	{
		// SerializedProperty Initialization
		if (serializedObject == null)
			return;

		mSProp_LevelSectionSize = serializedObject.FindProperty("m_nLevelSectionSize");
		mSProp_LevelSectionChance = serializedObject.FindProperty("m_nLevelSectionChance");
		mSProp_LevelSectionRawData = serializedObject.FindProperty("marr_bLevelSectionRawData");
		mSProp_TilePercentage = serializedObject.FindProperty("m_fTilePercentage");

		// GUIStyle Initialization
		mGUISty_Rect.margin = new RectOffset(4, 1, 4, 1);
		mGUISty_Rect.padding = new RectOffset(1, 1, 1, 1);
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		Rect outerRect = EditorGUILayout.BeginVertical(mGUISty_Rect);
		EditorGUI.DrawRect(outerRect, new Color(0.4f, 0.4f, 0.4f));
			Rect innerRect = EditorGUILayout.BeginVertical(mGUISty_Rect);
			EditorGUI.DrawRect(innerRect, new Color(0.6f, 0.6f, 0.6f));

				// Level-Section Grid Size
				EditorGUILayout.LabelField("Grid-Section Settings", EditorStyles.boldLabel);
				GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Section Grid Size:", EditorStyles.label);
					EditorGUI.BeginChangeCheck();
					GUILayout.Box(mSProp_LevelSectionSize.intValue.ToString(), GUILayout.MinWidth(40f));
					if (EditorGUI.EndChangeCheck())
						m_bIsLevelSectionSize = false;
				GUILayout.EndHorizontal();
				
				// Level-Section Grid
				int nLSSize = ((int)Mathf.Sqrt(mSProp_LevelSectionRawData.arraySize));
				if (nLSSize != 0)
				{
					for (int i = 0; i < nLSSize; i++)
					{
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						for (int j = 0; j < nLSSize; j++)
						{
							EditorGUILayout.PropertyField(
								mSProp_LevelSectionRawData.GetArrayElementAtIndex(i * nLSSize + j),
								mGUICont_LevelSectionRawData,
								GUILayout.MaxWidth(20f));
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
					}
				}

				EditorGUILayout.HelpBox("Check the boxes to determine which tile will spawn in a level-section", MessageType.Info);
				EditorGUILayout.Space();

				// Level-Section Chance
				EditorGUILayout.LabelField("Spawn Chance Settings", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(mSProp_LevelSectionChance, mGUICont_LevelSectionChance, GUILayout.MinHeight(20f));

				EditorGUILayout.Slider(mSProp_TilePercentage, 0f, 1f, mGUICont_TilePercentage, GUILayout.MinHeight(20f));

				float fTilePercentage = mSProp_TilePercentage.floatValue;
				float fCheckedRate = 100f - fTilePercentage * 50f;
				float fUncheckedRate = fTilePercentage * 50f;
				GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Checked Tiles Spawn Rate:", EditorStyles.label);
					GUILayout.Box((int)fCheckedRate + "%", GUILayout.MinWidth(40f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Unchecked Tiles Spawn Rate:", EditorStyles.label);
					GUILayout.Box((int)fUncheckedRate + "%", GUILayout.MinWidth(40f));
				GUILayout.EndHorizontal();
				EditorGUILayout.Space();

			EditorGUILayout.EndVertical();
		EditorGUILayout.EndVertical();

		// --------------------------------------------------------- END OF DISPLAY ------------------------------------------------------------
		if (mSProp_LevelSectionSize.intValue * mSProp_LevelSectionSize.intValue != mSProp_LevelSectionRawData.arraySize)
		{
			// Reinitialises the array
			mSProp_LevelSectionRawData.ClearArray();
			for (int i = 0; i < mSProp_LevelSectionSize.intValue * mSProp_LevelSectionSize.intValue; i++)
				mSProp_LevelSectionRawData.InsertArrayElementAtIndex(0);

			m_bIsLevelSectionSize = false;
		}

		if (mSProp_LevelSectionChance.intValue <= 0)
		{
			mSProp_LevelSectionChance.intValue = 1;
		}

		serializedObject.ApplyModifiedProperties();
	}
}
