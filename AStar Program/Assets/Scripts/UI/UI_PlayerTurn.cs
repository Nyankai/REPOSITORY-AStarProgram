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

	// Un-Editable Variables
	private Button[] marr_buttonCards = null;
	private Text[] marr_textHeader = null;
	private Text[] marr_textSubheader = null;
	private float m_fInitialY = 0f;
	private int m_nCurrentSelectCard = -1;

	private DelayAction[] marr_actDelay = null;
	private LocalMoveToAction[] marr_actMoveToHide = null;
	private LocalMoveToAction[] marr_actMoveToDisplay = null;
	private LocalMoveToAction[] marr_actMoveToSelect = null;
	private ActionSequence[] marr_actIntroSequence = null;
	
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

		marr_actDelay = new DelayAction[m_nCardCount];
		marr_actMoveToHide = new LocalMoveToAction[m_nCardCount];
		marr_actMoveToDisplay = new LocalMoveToAction[m_nCardCount];
		marr_actMoveToSelect = new LocalMoveToAction[m_nCardCount];
		marr_actIntroSequence = new ActionSequence[m_nCardCount];

		// for: Initialisation within each card
		for (int i = 0; i < m_nCardCount; i++)
		{
			// Component Initialisation within each card
			Transform currentChildCard = transform.GetChild(i);
			marr_buttonCards[i] = currentChildCard.GetComponent<Button>();
			marr_textHeader[i] = currentChildCard.GetChild(0).GetComponent<Text>();
			marr_textSubheader[i] = currentChildCard.GetChild(1).GetComponent<Text>();
			m_fInitialY = marr_buttonCards[i].transform.localPosition.y;

			// Update button colors
			ChangeCardColor(i, m_colorCard, m_colorCardHighlight);

			// Update header colors
			marr_textHeader[i].color = m_colorHeader;

			// Update sub-header colors
			marr_textSubheader[i].color = m_colorSubheader;

			// ALL SETTINGS FOR CARDS ACTIONS IS HANDLED HERE <----------------------------------------------------------------------
			marr_actDelay[i] = new DelayAction(i * m_fNextCardAppearDelay);

			Vector3 vec3NewLocalPosition = currentChildCard.localPosition;

			// Hidden Cards Actions
			vec3NewLocalPosition.y = m_fInitialY - m_fHideHeight;
			marr_actMoveToHide[i] = new LocalMoveToAction(currentChildCard, Graph.Bobber, vec3NewLocalPosition, 0.5f);

			// Displaying Cards Actions
			vec3NewLocalPosition.y = m_fInitialY - m_fDisplayHeight;
			marr_actMoveToDisplay[i] = new LocalMoveToAction(currentChildCard, Graph.Bobber, vec3NewLocalPosition, 0.5f);

			// Selection Cards Actions
			vec3NewLocalPosition.y = m_fInitialY - m_fSelectedHeight;
			marr_actMoveToSelect[i] = new LocalMoveToAction(currentChildCard, Graph.Bobber, vec3NewLocalPosition, 0.5f);

			// Introduction Sequential Cards Actions
			vec3NewLocalPosition.y = m_fInitialY - m_fHideHeight;
			marr_actIntroSequence[i] = new ActionSequence(marr_actDelay[i], marr_actMoveToDisplay[i]);
			marr_actIntroSequence[i].OnActionStart += () => { currentChildCard.localPosition = vec3NewLocalPosition; };
		}

		// Use the current position of each button as the initial height of each cards
		m_fInitialY = marr_buttonCards[0].transform.localPosition.y;
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

		// if: There is a card selected previously, deselect this card
		if (m_nCurrentSelectCard != -1)
		{
			ChangeCardColor(m_nCurrentSelectCard, m_colorCard, m_colorCardHighlight);
			m_nCurrentSelectCard = -1;
		}

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
	/// <param name="_bIsAnimate"> Determine if the </param>
	public void TransitionEnter(bool _bIsAnimate)
	{
		EnumPieceType[] enumCards = LevelManager.Instance.PlayerInstance.CardDeck;
		for (int i = 0; i < m_nCardCount; i++)
		{
			Vector3 vec3NewPosition = Vector3.zero;
			switch (enumCards[i])
			{
				case EnumPieceType.Null:
					marr_textHeader[i].text = "NAME";
					marr_textSubheader[i].text = "POS";

					vec3NewPosition = marr_buttonCards[i].transform.localPosition;
					vec3NewPosition.y = m_fInitialY - m_fHideHeight;
					marr_buttonCards[i].transform.localPosition = vec3NewPosition;
					break;
				default:
					marr_textHeader[i].text = enumCards[i].ToString();
					ActionHandler.RunAction(marr_actIntroSequence[i]);
					break;
			}
		}
	}

	public void TransitionExit()
	{
		
	}

	// Static Functions
	/// <summary>
	/// Returns the single instance of the UI_PlayerTurn
	/// </summary>
	public static UI_PlayerTurn Instance { get { return ms_Instance; } }

	// Getter-Setter Functions
	
}
