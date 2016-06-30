using UnityEngine;
using UnityEngine.UI;
using DaburuTools;
using DaburuTools.Action;

public class UI_PlayerTurn : MonoBehaviour 
{
	// Static Variables
	public static UI_PlayerTurn ms_Instance = null;					// ms_Instance: The single instance of this class

	// Editable Variables
	[Header("Color Properties")]
	[SerializeField] private Color m_colorCard = Color.white;			// m_colorCard: The color of the card
	[SerializeField] private Color m_colorCardHighlight = Color.white;	// m_colorCardHighlight: The color of the card highlight
	[SerializeField] private Color m_colorCardSelect = Color.white;		// m_colorCardSelect: The color of the card when selected
	[SerializeField] private Color m_colorCardSelectHightlight = Color.white;
	[SerializeField] private Color m_colorHeader = Color.white;			// m_colorHeader: The color of the card's header
	[SerializeField] private Color m_colorSubheader = Color.white;		// m_colorSubheader: The color of the card's sub-header

	[Header("Card Properties")]
	[SerializeField] private int m_nCardCount = 5;						// m_nCardCount: The number of cards on the screen (hard-code)
	[SerializeField] private int m_nMaximumStep = 3;					// m_nMaximumStep: The maximum number of steps the player can take each turn
	[SerializeField] private float m_fSelectedHeight = 0f;				// m_fSelectedHeight: The height of the card when it is selected
	[SerializeField] private float m_fDisplayHeight = 32f;				// m_fDisplayHeight: The height of the card when it is displaying
	[SerializeField] private float m_fHideHeight = 64f;					// m_fHideHeight: The height of the card when it is hidden

	[Header("Animation Properties")]
	[SerializeField] private float m_fNextCardAppearDelay = 0.25f;		// m_fNextCardAppearDelay: The time (in seconds) till the next card is shown in the introduction

	[Header("Object Pooling Properties")]
	[SerializeField] private ObjectPool m_OPSelection = null;			// m_OPSelection: The reference to the selection boxes' object pool 
	[SerializeField] private ObjectPool m_OPStep = null;				// m_OPStep: The reference to the steps object pool
	[SerializeField] private ObjectPool m_OPStepArrow = null;

	[Header("Other Buttons Properties")]
	[SerializeField] private Button m_buttonUndo = null;				// m_buttonUndo: The reference to the "Undo" button (which is not a child of this gameObject)
	[SerializeField] private Button m_buttonEndTurn = null;				// m_buttonEndTurn: The reference to the "EndTurn" button (which is not a child of this gameObject)

	// Un-Editable Variables
	private Button[] marr_buttonCards = null;	// marr_buttonCards: The array of reference to the cards (as typeof Button)
	private Text[] marr_textHeader = null;		// marr_textHeader: The array of reference to the cards' header (as typeof Text)
	private Text[] marr_textSubheader = null;	// marr_textSubheader: The array of the cards' sub-headers
	private Transform[] marr_trCards = null;	// marr_trCards: The array of the cards' transforms 
	private float m_fDefaultYHeight = 0f;		// m_fDefaultYHeight: The initial height of each card before the game runs will determine the default Y height

	private int m_nCurrentSelectCard = -1;
	private int m_nCurrentStep = 0;
	private Vector3[] marr_vec3StepPosition = null;		// marr_vec3StepPosition: The array to store all the step's position (0 represents the initial position of the player)
	private GameObject[] marr_GOSelection = null;
	private int[] marr_nCardOrder = null;

	// Private Functions
	// Awake(): is called when the script is first initialised
	void Awake()
	{
		if (ms_Instance == null)
			ms_Instance = this;
		else
			Destroy(this);
	}

	// Start(): Use this for initialisation
	void Start()
	{
		// Variable Initialisation
		marr_buttonCards = new Button[m_nCardCount];
		marr_textHeader = new Text[m_nCardCount];
		marr_textSubheader = new Text[m_nCardCount];
		marr_trCards = new Transform[m_nCardCount];

		marr_nCardOrder = new int[5];
		marr_vec3StepPosition = new Vector3[m_nMaximumStep + 1];

		// for: Every card...
		for (int i = 0; i < m_nCardCount; i++)
		{
			// Component Initialisation within each card
			Transform currentChildCard = transform.GetChild(i);
			marr_buttonCards[i] = currentChildCard.GetComponent<Button>();
			marr_textHeader[i] = currentChildCard.GetChild(0).GetComponent<Text>();
			marr_textSubheader[i] = currentChildCard.GetChild(1).GetComponent<Text>();
			marr_trCards[i] = marr_buttonCards[i].transform;

			// Update button colors
			ChangeCardColor(i, m_colorCard, m_colorCardHighlight);
			marr_textHeader[i].color = m_colorHeader;
			marr_textSubheader[i].color = m_colorSubheader;
		}

		// m_fDefaultYHeight: The initial height of each card before the game runs will determine the default Y height
		m_fDefaultYHeight = marr_buttonCards[0].transform.localPosition.y;

		// Other Buttons Initialisation
		m_buttonUndo.colors = ChangeButtonColor(m_buttonUndo, m_colorCard, m_colorCardSelect);
		m_buttonEndTurn.colors = ChangeButtonColor(m_buttonUndo, m_colorCard, m_colorCardSelect);

		TransitionExit(false, false);
	}

	// ChangeCardColor(): Change the card's color
	private void ChangeCardColor(int _nIndex, Color _colorNormal, Color _colorHighlight)
	{
		if (_nIndex < 0 || _nIndex >= m_nCardCount)
		{
			Debug.LogWarning(name + ".UI_PlayerTurn.ChangeCardColor(): _nIndex is out of range! _nIndex: " + _nIndex);
			return;
		}

		ColorBlock colorBlock = marr_buttonCards[_nIndex].colors;
		colorBlock.normalColor = _colorNormal;
		colorBlock.highlightedColor = _colorHighlight;
		colorBlock.pressedColor = _colorNormal;
		marr_buttonCards[_nIndex].colors = colorBlock;
	}

	// ChangeButtonColor(): Change the button's color
	private ColorBlock ChangeButtonColor(Button _button, Color _colorNormal, Color _colorHighlight)
	{
		ColorBlock colorBlock = _button.colors;
		colorBlock.normalColor = _colorNormal;
		colorBlock.highlightedColor = _colorHighlight;
		colorBlock.pressedColor = _colorNormal;
		return colorBlock;
	}

	// Public Functions
	/// <summary>
	/// Re-display all the cards, which includes the intro sequence
	/// </summary>
	public void BeginSequence()
	{
		// Variable Initialisation when it is the player's turn
		m_nCurrentSelectCard = -1;
		m_nCurrentStep = 0;
		marr_GOSelection = null;
		for (int i = 0; i < marr_nCardOrder.Length; i++)
			marr_nCardOrder[i] = -1;
		for (int i = 0; i < marr_vec3StepPosition.Length; i++)
			marr_vec3StepPosition[i] = Vector3.zero;
		marr_vec3StepPosition[0] = LevelManager.Instance.PlayerInstance.transform.position;

		UI_PlayerTurnTitle.Instance.TransitionEnter(true);
	}

	/// <summary>
	/// End the sequence.
	/// </summary>
	public void EndSequence()
	{
		// Ending Variables
		m_OPSelection.PoolAllObjects();
		TransitionExit(true, true);
		for (int i = 0; i < marr_nCardOrder.Length; i++)
			if (marr_nCardOrder[i] != -1)
				LevelManager.Instance.PlayerInstance.CardDeck[i] = EnumPieceType.Null;

		// Player Action Handling
		bool[,] arr2_bEnemyConstrain = LevelManager.Instance.GetCharacterConstrain(EnumCharacterType.Enemy);

		ActionSequence actSequence = new ActionSequence();
		for (int i = 1; i <= m_nCurrentStep; i++)
		{
			// Converts to grid space
			int[] arr_nCoords = new int[2];
			arr_nCoords[0] = Mathf.RoundToInt(marr_vec3StepPosition[i].x);
			arr_nCoords[1] = Mathf.RoundToInt(marr_vec3StepPosition[i].z);

			// if: There is no enemy constrain in the current co-ordinates
			if (arr2_bEnemyConstrain[arr_nCoords[0], arr_nCoords[1]])
			{
				MoveToAction actMoveToNext = new MoveToAction(LevelManager.Instance.PlayerInstance.transform, Graph.InverseExponential, marr_vec3StepPosition[i], 0.25f);
				actSequence.Add(actMoveToNext);
			}
			// else: Kill the enemy
			else
			{
				Character characterTarget = LevelManager.Instance.GetCharacter(arr_nCoords[0], arr_nCoords[1]);
				MoveToAction actJump = new MoveToAction(LevelManager.Instance.PlayerInstance.transform, Graph.InverseExponential, marr_vec3StepPosition[i] + new Vector3(0f, 2f, 0f), 0.5f);
				actJump.OnActionStart += () =>
				{
					CameraManager.Instance.ZoomInAt(characterTarget.transform, true);
				};
				MoveToAction actSlam = new MoveToAction(LevelManager.Instance.PlayerInstance.transform, Graph.Exponential, marr_vec3StepPosition[i], 0.1f);
				DelayAction actDelay = new DelayAction(0.5f);

				ScaleToAction actSquash = new ScaleToAction(characterTarget.transform, Graph.Exponential, new Vector3(1f, 0f, 1f), 0.1f);
				ActionParallel actSlamAndSquash = new ActionParallel(actSlam, actSquash);
				actSlamAndSquash.OnActionStart += CameraManager.Instance.Shake;
				actSlamAndSquash.OnActionFinish += () => 
				{ 
					characterTarget.Kill();
					CameraManager.Instance.LookAt(LevelManager.Instance.PlayerInstance.transform.position, true);
				};
				actSequence.Add(actJump, actSlamAndSquash, actDelay);
				//CameraManager.Instance.ZoomInAt(marr_vec3StepPosition[i], true);
			}
		}
		actSequence.OnActionFinish += () => 
		{
			// After-animations Variables Handling
			LevelManager.Instance.PlayerInstance.X = Mathf.RoundToInt(marr_vec3StepPosition[m_nCurrentStep].x);
			LevelManager.Instance.PlayerInstance.Y = Mathf.RoundToInt(marr_vec3StepPosition[m_nCurrentStep].z);
			CameraManager.Instance.LookAt(marr_vec3StepPosition[m_nCurrentStep], true);
			m_OPStep.PoolAllObjects();
			m_OPStepArrow.PoolAllObjects();

			UI_EnemyTurnTitle.Instance.TransitionEnter(true);
			LevelManager.Instance.ExecuteNextTurn();
		};

		ActionHandler.RunAction(actSequence);
	}

	/// <summary>
	/// CALLED ONLY BY THE UNDO BUTTON. Performs an undo function
	/// </summary>
	public void Undo()
	{
		// for: Every element in the card order...
		for (int i = 0; i < marr_nCardOrder.Length; i++)
		{
			// if: Reduce all positions from the order by 1
			if (marr_nCardOrder[i] == (m_nCurrentStep - 1))
				marr_nCardOrder[i] = -1;
		}
		if (m_nCurrentStep > 0)
		{
			m_OPSelection.PoolAllObjects();
			m_nCurrentStep--;
			UpdateStep();
			CameraManager.Instance.LookAt(marr_vec3StepPosition[m_nCurrentStep], true);
		}
		TransitionEnter(true, false);
	}

	/// <summary>
	/// Updates the steps to be displayed
	/// </summary>
	public void UpdateStep()
	{
		m_OPStep.PoolAllObjects();
		m_OPStepArrow.PoolAllObjects();

		// if: There is no new steps
		if (m_nCurrentStep == 0)
			return;

		GameObject[] arr_GOSteps = m_OPStep.GetObjectsAndReturn(m_nCurrentStep);
		for (int i = 1; i <= m_nCurrentStep; i++)
		{
			arr_GOSteps[i - 1].transform.position = marr_vec3StepPosition[i] + new Vector3(0f, 0.1f, 0f);
			arr_GOSteps[i - 1].transform.rotation = Quaternion.Euler(90f, 0f, 0f);

			GameObject GOArrowStep = m_OPStepArrow.GetObjectAndReturn();
			GOArrowStep.GetComponent<UI_StepArrow>().Initialisation(marr_vec3StepPosition[i - 1], marr_vec3StepPosition[i]);
		}
	}

	/// <summary>
	/// CALLED ONLY BE THE CARDS. This method handles what happens on the button presses
	/// </summary>
	/// <param name="_nButtonIndex"> The index position of the button, based on child hiearchy </param>
	public void ButtonClick(int _nButtonIndex)
	{
		// if: Error checking
		if (_nButtonIndex < 0 || _nButtonIndex >= m_nCardCount)
		{
			Debug.LogWarning(name + ".UI_PlayerTurn.ButtonClick(): parameter is out of range! m_nButtonIndex = " + _nButtonIndex);
			return;
		}

		// if: The current card have been registered as a step
		if (marr_nCardOrder[_nButtonIndex] != -1)
			return;

		// if: The player have reached the maximum number of turns
		if (m_nCurrentStep == m_nMaximumStep)
			return;

		// if: The card selected is the same card as the previously selected
		if (m_nCurrentSelectCard == _nButtonIndex)
		{
			ChangeCardColor(_nButtonIndex, m_colorCard, m_colorCardHighlight);
			m_nCurrentSelectCard = -1;
			m_OPSelection.PoolAllObjects();

			CameraManager.Instance.LookAt(marr_vec3StepPosition[m_nCurrentStep], true);
			return;
		}
		// else if: The card selected previously is not the same as this card
		else if (m_nCurrentSelectCard != -1)
		{
			ChangeCardColor(m_nCurrentSelectCard, m_colorCard, m_colorCardHighlight);
			m_nCurrentSelectCard = -1;
			m_OPSelection.PoolAllObjects();
		}
		// else: (same condition with else if) There is no card selected initialy
		ChangeCardColor(_nButtonIndex, m_colorCardSelect, m_colorCardSelectHightlight);
		m_nCurrentSelectCard = _nButtonIndex;

		CameraManager.Instance.ZoomInAt(marr_vec3StepPosition[m_nCurrentStep], true);

		// Selection Handling
		// arr_nMovements: The selection boxes in local grid from the target
		int[,] arr_nMovements = Movement.GetMovementType(LevelManager.Instance.PlayerInstance.CardDeck[m_nCurrentSelectCard]);
		marr_GOSelection = m_OPSelection.GetObjectsAndReturn(arr_nMovements.GetLength(0));
		// for: Every selection tile...
		for (int i = 0; i < marr_GOSelection.Length; i++)
		{
			// Converts the current step world position to grid coords
			int[] arr_nCurrentStepCoords = new int[2];
			arr_nCurrentStepCoords[0] = Mathf.RoundToInt(marr_vec3StepPosition[m_nCurrentStep].x);
			arr_nCurrentStepCoords[1] = Mathf.RoundToInt(marr_vec3StepPosition[m_nCurrentStep].z);

			// worldX, worldY: Selection box in the world-grid
			int worldX = arr_nMovements[i, 0] + arr_nCurrentStepCoords[0];
			int worldY = arr_nMovements[i, 1] + arr_nCurrentStepCoords[1];

			// if, if: The selection box is within the minimum and maximum level map
			if (worldX >= 0 && worldX < Level.CurrentLevel.TileLength)
			{
				if (worldY >= 0 && worldY < Level.CurrentLevel.TileLength)
				{
					// if: The selection box is in the generated level map
					if (LevelManager.Instance.LevelData[worldX, worldY])
					{
						// Set its grid-coords
						marr_GOSelection[i].GetComponent<SelectionBox>().SetGridCoords(arr_nMovements[i, 0], arr_nMovements[i, 1]);
						marr_GOSelection[i].GetComponent<SelectionBox>().TransitionEnter();
						continue;
					}
				}
			}
			// Here is when the selection boxs are out of range
			m_OPSelection.PoolObject(marr_GOSelection[i]);
		}
	}

	/// <summary>
	/// CALLED ONLY BY THE SELECTION. This method handles what happens after the user have selected a selection
	/// </summary>
	public void SelectionClick(Vector3 _vec3NewPosition)
	{
		// for: Every selection, perform exit-transition (This will not override the select-transition)
		for (int i = 0; i < marr_GOSelection.Length; i++)
			marr_GOSelection[i].GetComponent<SelectionBox>().TransitionExit();
		marr_GOSelection = null;

		// Change the card color back to normal
		ChangeCardColor(m_nCurrentSelectCard, m_colorCard, m_colorCardHighlight);
		// Updates the card step array
		marr_nCardOrder[m_nCurrentSelectCard] = m_nCurrentStep;
		// Deselects all the cards
		m_nCurrentSelectCard = -1;

		// Update the step count
		m_nCurrentStep++;
		// Add new step position for step count
		marr_vec3StepPosition[m_nCurrentStep] = _vec3NewPosition;
		UpdateStep();
		CameraManager.Instance.LookAt(marr_vec3StepPosition[m_nCurrentStep], true);
		TransitionEnter(true, false);
	}

	/// <summary>
	/// Perform the card-entering transition
	/// </summary>
	/// <param name="_bIsAnimate"> Determine if the transition should be animated (or snapped into place) </param>
	/// <param name="_bIsDelay"> Determine if there should be delay between each card that is transiting in </param>
	public void TransitionEnter(bool _bIsAnimate, bool _bIsDelay)
	{
		EnumPieceType[] enumCards = LevelManager.Instance.PlayerInstance.CardDeck;
		for (int i = 0; i < m_nCardCount; i++)
		{
			// vec3NewCardPosition: Determine where the new card position will be, regardless of the previous position
			Vector3 vec3NewCardPosition = Vector3.zero;
			switch (enumCards[i])
			{
				case EnumPieceType.Null:
					// Set-up the card's display
					marr_textHeader[i].text = "NAME";
					marr_textSubheader[i].text = "...";

					// Set the current card to the hidden position
					vec3NewCardPosition = marr_trCards[i].localPosition;
					vec3NewCardPosition.y = m_fDefaultYHeight - m_fHideHeight;
					break;
				default:
					// Set-up the card's display
					marr_textHeader[i].text = enumCards[i].ToString();

					// if: The current card does not have a card order
					if (marr_nCardOrder[i] == -1)
					{
						// Set the current card to the display position
						vec3NewCardPosition = marr_trCards[i].localPosition;
						vec3NewCardPosition.y = m_fDefaultYHeight - m_fDisplayHeight;
					}
					else
					{
						// switch: Based on the card order to determine card's sub-header data
						switch (marr_nCardOrder[i])
						{
							case 0:
								marr_textSubheader[i].text = "1st"; 
								break;
							case 1:
								marr_textSubheader[i].text = "2nd";
								break;
							case 2:
								marr_textSubheader[i].text = "3rd";
								break;
							default:
								Debug.LogWarning(name + ".UI_PlayerTurn.TransitionEnter(): Not registered marr_nCardOrder[i]! marr_nCardOrder[i]: " + marr_nCardOrder[i]);
								break;
						}

						// Set the current card to the display position
						vec3NewCardPosition = marr_trCards[i].localPosition;
						vec3NewCardPosition.y = m_fDefaultYHeight - m_fSelectedHeight;
					}
					break;
			}

			// if: Animation is allowed
			if (_bIsAnimate)
			{
				// if: Delay is allowed
				if (_bIsDelay)
				{
					DelayAction actDelay = new DelayAction(i * m_fNextCardAppearDelay);
					LocalMoveToAction actMoveToPosition = new LocalMoveToAction(marr_trCards[i], Graph.Bobber, vec3NewCardPosition, 0.5f);
					ActionSequence actSequence = new ActionSequence(actDelay, actMoveToPosition);

					ActionHandler.RunAction(actSequence);
				}
				else
				{
					LocalMoveToAction actMoveToPosition = new LocalMoveToAction(marr_trCards[i], Graph.Bobber, vec3NewCardPosition, 0.5f);

					ActionHandler.RunAction(actMoveToPosition);
				}
			}
			else
			{
				marr_trCards[i].localPosition = vec3NewCardPosition;
			}
		}
	}

	/// <summary>
	/// Perform a card-exiting transition
	/// </summary>
	/// <param name="_bIsAnimate"> Determine if the transition should be animated (or snapped into place) </param>
	public void TransitionExit(bool _bIsAnimate, bool _bIsTransitTitle)
	{
		// if: Title is not transiting
		if (_bIsTransitTitle)
		{
			UI_PlayerTurnTitle.Instance.TransitionExit(_bIsAnimate);
		}

		// for: Every card...
		for (int i = 0; i < m_nCardCount; i++)
		{
			// Set the current card to the hidden position
			ChangeCardColor(i, m_colorCard, m_colorCardHighlight);
			Vector3 vec3NewHidePosition = marr_trCards[i].localPosition;
			vec3NewHidePosition.y = m_fDefaultYHeight - m_fHideHeight;

			// if: Animation is allowed
			if (_bIsAnimate)
			{
				LocalMoveToAction actMoveToPosition = new LocalMoveToAction(marr_trCards[i], Graph.Dipper, vec3NewHidePosition, 0.5f);
				ActionHandler.RunAction(actMoveToPosition);
			}
			else
			{
				marr_trCards[i].localPosition = vec3NewHidePosition;
			}
		}
	}

	// Static Functions
	/// <summary>
	/// Returns the single instance of the UI_PlayerTurn
	/// </summary>
	public static UI_PlayerTurn Instance { get { return ms_Instance; } }

	// Getter-Setter Functions
	/// <summary>
	/// Returns a reference to the selection boxes' object pool
	/// </summary>
	public ObjectPool ObjectPool_SelectionBox { get { return m_OPSelection; } }
	
	/// <summary>
	/// Returns a reference to the steps' object pool
	/// </summary>
	public ObjectPool ObjectPool_Step { get { return m_OPStep; } }

	public Vector3 CurrentStepPosition { get { return marr_vec3StepPosition[m_nCurrentStep]; } }
}
