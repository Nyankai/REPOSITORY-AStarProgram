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
	[SerializeField] private float m_fSelectedHeight = 0f;				// m_fSelectedHeight: The height of the card when it is selected
	[SerializeField] private float m_fDisplayHeight = 32f;				// m_fDisplayHeight: The height of the card when it is displaying
	[SerializeField] private float m_fHideHeight = 64f;					// m_fHideHeight: The height of the card when it is hidden

	[Header("Animation Properties")]
	[SerializeField] private float m_fNextCardAppearDelay = 0.25f;		// m_fNextCardAppearDelay: The time (in seconds) till the next card is shown in the introduction

	[Header("Tile Selection Properties")]
	[SerializeField] private ObjectPool m_OPSelection = null;

	// Un-Editable Variables
	private Button[] marr_buttonCards = null;
	private Text[] marr_textHeader = null;		// marr_textHeader: The array of the cards' headers
	private Text[] marr_textSubheader = null;	// marr_textSubheader: The array of the cards' sub-headers
	private Transform[] marr_trCards = null;	// marr_trCards: The array of the cards' transforms 
	private float m_fDefaultYHeight = 0f;		// m_fDefaultYHeight: The initial height of each card before the game runs will determine the default Y height

	private int m_nCurrentSelectCard = -1;
	
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

		TransitionExit(false);
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

	// Public Functions
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

		// if: The card selected is the same card as the previously selected
		if (m_nCurrentSelectCard == _nButtonIndex)
		{
			ChangeCardColor(_nButtonIndex, m_colorCard, m_colorCardHighlight);
			m_nCurrentSelectCard = -1;

			CameraManager.Instance.LookAt(CameraManager.Instance.transform.position, true);
			return;
		}
		// else if: The card selected previously is not the same as this card
		else if (m_nCurrentSelectCard != -1)
		{
			ChangeCardColor(m_nCurrentSelectCard, m_colorCard, m_colorCardHighlight);
			m_nCurrentSelectCard = -1;
		}
		// else: (same condition with else if) There is no card selected initialy

		ChangeCardColor(_nButtonIndex, m_colorCardSelect, m_colorCardSelectHightlight);
		m_nCurrentSelectCard = _nButtonIndex;

		CameraManager.Instance.ZoomInAt(LevelManager.Instance.PlayerInstance.transform, true);
	}

	/// <summary>
	/// Re-display all the cards, which includes the intro sequence
	/// </summary>
	public void BeginSequence()
	{
		UI_PlayerTurnTitle.Instance.TransitionEnter(true);
	}

	/// <summary>
	/// Perform the card-entering transition
	/// </summary>
	/// <param name="_bIsAnimate"> Determine if the transition should be animated (or snapped into place) </param>
	public void TransitionEnter(bool _bIsAnimate)
	{
		TransitionExit(false);

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
					marr_textSubheader[i].text = "POS";

					// Set the current card to the hidden position
					vec3NewCardPosition = marr_trCards[i].localPosition;
					vec3NewCardPosition.y = m_fDefaultYHeight - m_fHideHeight;
					break;
				default:
					// Set-up the card's display
					marr_textHeader[i].text = enumCards[i].ToString();

					// Set the current card to the display position
					vec3NewCardPosition = marr_trCards[i].localPosition;
					vec3NewCardPosition.y = m_fDefaultYHeight - m_fDisplayHeight;
					break;
			}

			// if: Animation is allowed
			if (_bIsAnimate)
			{
				DelayAction actDelay = new DelayAction(i * m_fNextCardAppearDelay);
				LocalMoveToAction actMoveToPosition = new LocalMoveToAction(marr_trCards[i], Graph.Bobber, vec3NewCardPosition, 0.5f);
				ActionSequence actSequence = new ActionSequence(actDelay, actMoveToPosition);
				ActionHandler.RunAction(actSequence);
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
	public void TransitionExit(bool _bIsAnimate)
	{
		// for: Every card...
		for (int i = 0; i < m_nCardCount; i++)
		{
			// Set-up the card's display
			marr_textHeader[i].text = "NAME";
			marr_textSubheader[i].text = "POS";

			// Set the current card to the hidden position
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
	
}
