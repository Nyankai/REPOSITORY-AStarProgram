using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
[CanEditMultipleObjects]
public class LevelEditor : Editor 
{
	// Editor Instances
	private Editor m_EditorLevelSection;

	// SerializedProperty Variables
	private SerializedProperty mSProp_nLevelSize;
	private SerializedProperty mSProp_nLevelSectionSize;

	private SerializedProperty mSProp_LevelSectionType;

	// GUIContent Variables
	private GUIContent mGUICont_LevelSize
		= new GUIContent("Level Size", "The number of level-section on one side of the level");
	private GUIContent mGUICont_LevelSectionSize
		= new GUIContent("Level-Section Size", "The length of one side of a level-section");

	private GUIContent mGUICont_newLevelSectionTypes
		= new GUIContent("Create New Level Section", "Click this to create a new level-section type");

	private GUIContent mGUICont_deleteLevelSectionType
		= new GUIContent("Delete Type", "Click this to delete this level-section type");
	private GUIContent mGUICont_fTilePercentage
		= new GUIContent("Tile Percentage", "The chance of the toggled and non-toggled spawning a tile");

	// Boolean Variables
	private bool m_bIsLevelSectionSize = false;
	private bool m_bIsLevelSectionTypeCreate = false;
	private bool m_bIsLevelSectionTypeDelete = false;

	private int m_nIsLevelSectionTypeDeleteIndex = 0;

	// Private Variables
	void OnEnable()
	{
		// SerializedProperty Initialization
		mSProp_nLevelSize = serializedObject.FindProperty("m_nLevelSize");
		mSProp_nLevelSectionSize = serializedObject.FindProperty("m_nLevelSectionSize");

		mSProp_LevelSectionType = serializedObject.FindProperty("mList_LevelSectionType");

		// Editor Intialization
		m_EditorLevelSection = null;
	}

	public override void OnInspectorGUI()
	{
		if (Application.isPlaying)
		{
			EditorGUILayout.HelpBox("Editing of script is disabled when in play mode", MessageType.Info);
			return;
		}

		serializedObject.Update();
		EditorGUILayout.Space();

		// Level-Section Size
		EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(mSProp_nLevelSize, mGUICont_LevelSize);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(mSProp_nLevelSectionSize, mGUICont_LevelSectionSize);
		if (EditorGUI.EndChangeCheck())
		{
			m_bIsLevelSectionSize = true;
		}
		EditorGUILayout.HelpBox("Changing the size of the level-section will delete ALL of your current level-sections!", MessageType.Warning);
		EditorGUILayout.Space();

		// Level-Section List
		EditorGUILayout.LabelField("Level-Section List", EditorStyles.boldLabel);
		if (GUILayout.Button(mGUICont_newLevelSectionTypes))
		{
			m_bIsLevelSectionTypeCreate = true;
		}

		for (int i = 0; i < mSProp_LevelSectionType.arraySize; i++)
		{
			if (mSProp_LevelSectionType.GetArrayElementAtIndex(i).objectReferenceValue == null)
				Debug.Log(":D");
			else
			{
				m_EditorLevelSection = Editor.CreateEditor(
					(mSProp_LevelSectionType.GetArrayElementAtIndex(i).objectReferenceValue));

				GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Level-Section Type " + (i + 1), EditorStyles.boldLabel);
					GUI.color = Color.yellow;
					if (GUILayout.Button(mGUICont_deleteLevelSectionType))
					{
						m_bIsLevelSectionTypeDelete = true;
						m_nIsLevelSectionTypeDeleteIndex = i;
					}
					GUI.color = Color.white;
				GUILayout.EndHorizontal();
				m_EditorLevelSection.OnInspectorGUI();
				EditorGUILayout.Space();
			}
		}

		// --------------------------------------------------------- END OF DISPLAY ------------------------------------------------------------
		// if: Level-Section Change Check
		if (m_bIsLevelSectionSize)
		{
			// if, else if: Clamps the values
			if (mSProp_nLevelSectionSize.intValue < 1)
				mSProp_nLevelSectionSize.intValue = 1;
			else if (mSProp_nLevelSectionSize.intValue > 10)
				mSProp_nLevelSectionSize.intValue = 10;

			mSProp_LevelSectionType.ClearArray();
			if (mSProp_LevelSectionType != null)
				for (int i = 0; i < mSProp_LevelSectionType.arraySize; i++)
				{
					SerializedProperty sPropRef_LevelSectionSize = mSProp_LevelSectionType.GetArrayElementAtIndex(i).serializedObject.FindProperty("m_nLevelSectionSize");
					sPropRef_LevelSectionSize.intValue = mSProp_nLevelSectionSize.intValue;
				}

			m_bIsLevelSectionSize = false;
		}

		if (m_bIsLevelSectionTypeCreate)
		{
			mSProp_LevelSectionType.InsertArrayElementAtIndex(mSProp_LevelSectionType.arraySize);

			mSProp_LevelSectionType.GetArrayElementAtIndex(mSProp_LevelSectionType.arraySize - 1).objectReferenceValue
				= new LevelSection(mSProp_nLevelSectionSize.intValue, 0f) as System.Object as UnityEngine.Object;
			m_bIsLevelSectionTypeCreate = false;
		}

		if (m_bIsLevelSectionTypeDelete)
		{
			mSProp_LevelSectionType.DeleteArrayElementAtIndex(m_nIsLevelSectionTypeDeleteIndex);
			mSProp_LevelSectionType.DeleteArrayElementAtIndex(m_nIsLevelSectionTypeDeleteIndex);
			m_bIsLevelSectionTypeDelete = false;
		}

		serializedObject.ApplyModifiedProperties();
	}
}
