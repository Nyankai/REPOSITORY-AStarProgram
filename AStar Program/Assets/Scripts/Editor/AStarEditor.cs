using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AStar))]
public class AStarEditor : Editor 
{
	// SerializedProperty Variables
	private SerializedProperty mSProp_enumPositionInput;

	private SerializedProperty mSProp_startTransform;
	private SerializedProperty mSProp_targetTransform;
	private SerializedProperty mSProp_startPosition;
	private SerializedProperty mSProp_targetPosition;

	private SerializedProperty mSProp_position;
	private SerializedProperty mSProp_gridSize;
	private SerializedProperty mSProp_sizePerGrid;

	private SerializedProperty mSProp_enumNeighbourGridSize;
	private SerializedProperty mSProp_neighbourGrid;
	private SerializedProperty mSProp_neighbourToCheck;

	private SerializedProperty mSProp_isDisplaySceneGrid;

	// GUIContents Variables
	private GUIContent mGUICont_enumPositionInput 
		= new GUIContent("Definition", "Select how to determine the start and the target point");

	private GUIContent mGUICont_startTransform
		= new GUIContent("Start-Point Transform", "The starting transform of the A*Star algorithm. It will use its transform.position as the starting point.");
	private GUIContent mGUICont_targetTransform
		= new GUIContent("Target-Point Transform", "The target transform of the A*Star algorithm. It will use its transform.position as the target point.");
	private GUIContent mGUICont_startPosition
		= new GUIContent("Start-Point (World Co-ordinates)", "The starting co-ordinates in Unity-world axis of the A*Star algorithm.");
	private GUIContent mGUICont_targetPosition
		= new GUIContent("Target-Point (World Co-ordinates)", "The target co-ordinates in Unity-world axis of the A*Star algorithm.");

	private GUIContent mGUICont_position
		= new GUIContent("Grid Position", "The position of the grid in Unity-world space");
	private GUIContent mGUICont_positionResetButton
		= new GUIContent("Set to Transform Position", "Resets the grid co-ordinates to the current transform's position");
	private GUIContent mGUICont_gridSize
		= new GUIContent("Total Grid Size", "The length of the entire grid");
	private GUIContent mGUICont_sizePerGrid
		= new GUIContent("Size-per-grid", "The length of a single grid unit");

	private GUIContent mGUICont_neighbourGrid
		= new GUIContent("");
	private GUIContent mGUICont_clearNeighbourGrid
		= new GUIContent("Clear Grid", "Press this button to reset the navigation grid below");

	private GUIContent mGUICont_isDisplaySceneGrid
		= new GUIContent("Display Path", "Determines if the path of the algorithm should be displayed");

	// After Inspector Booleans
	private bool isRefreshPosition = false;
	private bool isRefreshVector2Array = false;

	// Private Functions
	// OnEnable(): Finds the property from the script when the inspector initialized
	void OnEnable()
	{
		mSProp_enumPositionInput = serializedObject.FindProperty("menum_positionInput");

		mSProp_startTransform = serializedObject.FindProperty("m_startTransform");
		mSProp_targetTransform = serializedObject.FindProperty("m_targetTransform");
		mSProp_startPosition = serializedObject.FindProperty("m_startPosition");
		mSProp_targetPosition = serializedObject.FindProperty("m_targetPosition");

		mSProp_position = serializedObject.FindProperty("m_position");
		mSProp_gridSize = serializedObject.FindProperty("m_nGridSize");
		mSProp_sizePerGrid = serializedObject.FindProperty("m_fSizePerGrid");

		mSProp_neighbourGrid = serializedObject.FindProperty("marr_bNeighbourGrid");
		mSProp_neighbourToCheck = serializedObject.FindProperty("marr_neighbourToCheck");
		mSProp_enumNeighbourGridSize = serializedObject.FindProperty("menum_neighbourGridSize");

		mSProp_isDisplaySceneGrid = serializedObject.FindProperty("m_bIsDisplaySceneGrid");
	}

	// Public Functions
	// OnInspectorGUI(): 
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.Space();

		// Start and Target Settings
		EditorGUILayout.LabelField("Start and Target Settings", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(mSProp_enumPositionInput, mGUICont_enumPositionInput);
		switch (mSProp_enumPositionInput.enumValueIndex)
		{
			case 0:
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(mSProp_startTransform, mGUICont_startTransform);
				EditorGUILayout.PropertyField(mSProp_targetTransform, mGUICont_targetTransform);
				if (EditorGUI.EndChangeCheck())
					isRefreshPosition = true;
			break;
			case 1:
				EditorGUILayout.PropertyField(mSProp_startPosition, mGUICont_startPosition);
				EditorGUILayout.PropertyField(mSProp_targetPosition, mGUICont_targetPosition);
			break;
			default: break;
		}
		EditorGUILayout.Space();

		// Grid Settings
		EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(mSProp_position, mGUICont_position);
		if (GUILayout.Button(mGUICont_positionResetButton))
			mSProp_position.vector3Value = (target as AStar).transform.position;

		EditorGUILayout.PropertyField(mSProp_gridSize, mGUICont_gridSize);
		EditorGUILayout.PropertyField(mSProp_sizePerGrid, mGUICont_sizePerGrid);
		EditorGUILayout.Space();

		// Navigation Settings
		EditorGUILayout.LabelField("Navigation Settings", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Navigation Contraint");
		
		// Didn't use BeginChangeCheck() because function also detects if the same button is pressed
		int nPreviousSelection = mSProp_enumNeighbourGridSize.enumValueIndex;
		mSProp_enumNeighbourGridSize.enumValueIndex = GUILayout.SelectionGrid(mSProp_enumNeighbourGridSize.enumValueIndex, new string[] { "3x3", "5x5", "7x7", "9x9" }, 4);
		bool bIsClearGridButton = GUILayout.Button(mGUICont_clearNeighbourGrid);
		if (nPreviousSelection != mSProp_enumNeighbourGridSize.enumValueIndex || bIsClearGridButton)
		{
			mSProp_neighbourGrid.ClearArray();
			if (bIsClearGridButton)
			{
				mSProp_neighbourToCheck.ClearArray();
			}
			else
			{
				switch (mSProp_enumNeighbourGridSize.enumValueIndex)
				{
					case 0:
						for (int i = 0; i < 9; i++) mSProp_neighbourGrid.InsertArrayElementAtIndex(mSProp_neighbourGrid.arraySize);
						break;
					case 1:
						for (int i = 0; i < 25; i++) mSProp_neighbourGrid.InsertArrayElementAtIndex(mSProp_neighbourGrid.arraySize);
						break;
					case 2:
						for (int i = 0; i < 49; i++) mSProp_neighbourGrid.InsertArrayElementAtIndex(mSProp_neighbourGrid.arraySize);
						break;
					case 3:
						for (int i = 0; i < 81; i++) mSProp_neighbourGrid.InsertArrayElementAtIndex(mSProp_neighbourGrid.arraySize);
						break;
					default: break;
				}
			}
		}

		int nGridSize = (int)Mathf.Sqrt((float)mSProp_neighbourGrid.arraySize);
		for (int i = 0; i < nGridSize; i++)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			for (int j = 0; j < nGridSize; j++)
			{
				if (i == nGridSize / 2 && j == nGridSize / 2)
					GUILayout.Box("N", GUILayout.MaxWidth(20f), GUILayout.MaxHeight(20f));
				else
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(mSProp_neighbourGrid.GetArrayElementAtIndex(i * nGridSize + j), mGUICont_neighbourGrid, GUILayout.MaxWidth(20f));
					if (EditorGUI.EndChangeCheck())
						isRefreshVector2Array = true;
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		EditorGUILayout.HelpBox("N represents the navigator. Use the checkboxes to determine which next path the navigator is able to take", MessageType.Info);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Debugging Settings", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(mSProp_isDisplaySceneGrid);

		// --------------------------------------------------------- END OF DISPLAY ------------------------------------------------------------
		// Push (Start and Target) Transform edits into (Start and Target) Position edits
		if (isRefreshPosition)
		{
			if (((Transform)mSProp_startTransform.objectReferenceValue) != null)
				mSProp_startPosition.vector3Value = ((Transform)mSProp_startTransform.objectReferenceValue).position;
			if (((Transform)mSProp_targetTransform.objectReferenceValue) != null)
				mSProp_targetPosition.vector3Value = ((Transform)mSProp_targetTransform.objectReferenceValue).position;
			isRefreshPosition = false;
		}

		// Converts bool[] array to Vector2[] array
		if (isRefreshVector2Array)
		{
			mSProp_neighbourToCheck.ClearArray();
			for (int i = 0; i < nGridSize; i++)
			{
				for (int j = 0; j < nGridSize; j++)
				{
					// if: it is checked in the bool[] array
					if (mSProp_neighbourGrid.GetArrayElementAtIndex(i * nGridSize + j).boolValue)
					{
						mSProp_neighbourToCheck.InsertArrayElementAtIndex(mSProp_neighbourToCheck.arraySize);
						mSProp_neighbourToCheck.GetArrayElementAtIndex(mSProp_neighbourToCheck.arraySize - 1).vector2Value
							= new Vector2((float)(j - nGridSize / 2), (float)(nGridSize / 2 - i));
					}
				}
			}
			isRefreshVector2Array = false;
		}

		serializedObject.ApplyModifiedProperties();
	}
}
