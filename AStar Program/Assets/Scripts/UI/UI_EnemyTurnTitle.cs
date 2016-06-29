using UnityEngine;
using UnityEngine.UI;
using DaburuTools;
using DaburuTools.Action;

public class UI_EnemyTurnTitle : MonoBehaviour 
{
	// Static Variables
	private static UI_EnemyTurnTitle ms_Instance = null;

	// Editable Variables
	[Header("Color Properties")]
	[SerializeField] private Color m_colorTitle = Color.white;		// m_colorTitle: The color of the title
	[SerializeField] private Color m_colorBackground = Color.white;	// m_colorBackground: The color of the background (Note that alpha will be ignored)

	[Header("Translation Properties")]
	[SerializeField] private float m_fTitleToHide = 100f;
	[SerializeField] private float m_fEnemyToHide = 100f;
	[SerializeField] private float m_fBackgroundLongToHide = 90f;
	[SerializeField] private float m_fBackgroundSideToHide = 123f;
	[SerializeField] private float m_fNextElementDelay = 0.1f;

	// Un-editable Variables
	private Text m_textTitle = null;
	private Text m_textEnemy = null;
	private Image m_imageBackground1 = null;
	private Image m_imageBackground2 = null;
	private Transform[] marr_trTitleElement = null;
	private Vector3[] marr_vec3DisplayPosition = null;
	private Vector3[] marr_vec3HidePosition = null;

	// Private Functions
	void Awake()
	{
		// Singleton
		if (ms_Instance == null)
			ms_Instance = this;
		else
			Destroy(this);
	}

	// Awake(): is called when the script is first initialized
	void Start()
	{
		// Component Variables
		marr_trTitleElement = new Transform[transform.childCount];
		marr_vec3DisplayPosition = new Vector3[transform.childCount];
		marr_vec3HidePosition = new Vector3[transform.childCount];
		for (int i = 0; i < marr_trTitleElement.Length; i++)
		{
			// Stores the UI elements into an array for ease of access
			marr_trTitleElement[i] = transform.GetChild(i);
			float fYHideHeight = 0f;
			switch (i)
			{
				case 0:
					m_imageBackground1 = marr_trTitleElement[i].GetComponent<Image>();
					fYHideHeight = m_fBackgroundLongToHide;
					break;
				case 1:
					m_imageBackground2 = marr_trTitleElement[i].GetComponent<Image>();
					fYHideHeight = m_fBackgroundSideToHide;
					break;
				case 2:
					m_textTitle = marr_trTitleElement[i].GetComponent<Text>();
					fYHideHeight = m_fTitleToHide;
					break;
				case 3:
					m_textEnemy = marr_trTitleElement[i].GetComponent<Text>();
					fYHideHeight = m_fEnemyToHide;
					break;
				default:
					Debug.LogWarning(name + ".UI_PlayerTurnTitle.Start(): Child count more than expected. New child in scope?");
					break;
			}

			// Position Initialisation
			marr_vec3DisplayPosition[i] = marr_trTitleElement[i].localPosition;
			marr_vec3HidePosition[i] = marr_trTitleElement[i].localPosition - new Vector3(0f, fYHideHeight, 0f);
		}

		// Setting-up UI
		TitleColor = m_colorTitle;
		m_textEnemy.color = m_colorTitle;
		SetBackgroundColor(m_colorBackground);

		TransitionEnter(true);
	}

	// Public Functions
	/// <summary>
	/// Perform the entering animation of the title "Enemy's Turn"
	/// </summary>
	/// <param name="_bIsAnimate"> Determine if the transition should be animated (or snapped into place) </param>
	public void TransitionEnter(bool _bIsAnimate)
	{
		// for: Every title element...
		for (int i = 0; i < marr_trTitleElement.Length; i++)
		{
			// if: Animation is selected
			if (_bIsAnimate)
			{
				// Snap the element into position
				TransitionExit(false);

				// Execute an action
				DelayAction actDelay = new DelayAction(i * m_fNextElementDelay);
				LocalMoveToAction actMoveEnter = new LocalMoveToAction(marr_trTitleElement[i], Graph.Bobber, marr_vec3DisplayPosition[i], 0.25f);
				ActionSequence actSequence = new ActionSequence(actDelay, actMoveEnter);

				ActionHandler.RunAction(actSequence);
			}
			else
			{
				marr_trTitleElement[i].localPosition = marr_vec3DisplayPosition[i];
			}
		}
	}

	/// <summary>
	/// Perform the exiting animation of the title "Enemy's Turn"
	/// </summary>
	/// <param name="_bIsAnimate"> Determine if the transition should be animated (or snapped into place) </param>
	public void TransitionExit(bool _bIsAnimate)
	{
		// for: Every title element...
		for (int i = 0; i < marr_trTitleElement.Length; i++)
		{
			// if: Animation is selected
			if (_bIsAnimate)
			{
				// Snap the element into position
				TransitionEnter(false);

				// Execute an action
				DelayAction actDelay = new DelayAction(i * m_fNextElementDelay);
				LocalMoveToAction actMoveExit = new LocalMoveToAction(marr_trTitleElement[i], Graph.Bobber, marr_vec3HidePosition[i], 0.25f);
				ActionSequence actSequence = new ActionSequence(actDelay, actMoveExit);

				ActionHandler.RunAction(actSequence);
			}
			else
			{
				marr_trTitleElement[i].localPosition = marr_vec3HidePosition[i];
			}
		}
	}

	/// <summary>
	/// Perform the swap animation for the enemy's name
	/// </summary>
	/// <param name="_bIsAnimate"> Determine if the transition should be animated (or snapped into place) </param>
	public void TransitionEnemy(bool _bIsAnimate, string _strName)
	{
		if (_bIsAnimate)
		{
			LocalMoveToAction actDropDown = new LocalMoveToAction(marr_trTitleElement[3], Graph.Exponential, marr_vec3HidePosition[3], 0.1f);
			LocalMoveToAction actRiseUp = new LocalMoveToAction(marr_trTitleElement[3], Graph.InverseExponential, marr_vec3DisplayPosition[3], 0.1f);
			actDropDown.OnActionFinish += () =>
			{
				m_textEnemy.text = _strName;
			};
			ActionSequence actSequence = new ActionSequence(actDropDown, actRiseUp);
			ActionHandler.RunAction(actSequence);
		}
		else
		{
			m_textEnemy.text = _strName;
		}
	}

	/// <summary>
	/// Change the background color
	/// </summary>
	/// <param name="_colorBackground"> The new background color </param>
	public void SetBackgroundColor(Color _colorBackground)
	{
		_colorBackground.a = m_imageBackground1.color.a;
		m_imageBackground1.color = _colorBackground;
		m_imageBackground2.color = _colorBackground;
	}

	// Static Functions
	/// <summary>
	/// Returns the single instance of UI_PlayerTurnTitle
	/// </summary>
	public static UI_EnemyTurnTitle Instance { get { return ms_Instance; } }

	// Getter-Setter Functions
	/// <summary>
	/// Returns or determine the color for the title "Player's Turn"
	/// </summary>
	public Color TitleColor { get { return m_textTitle.color; } set { m_textTitle.color = value; } }

	/// <summary>
	/// Returns the background color of the title. Use SetBackgroundColor() to set the color instead
	/// </summary>
	public Color BackgroundColor { get { return m_imageBackground1.color; } }
}
