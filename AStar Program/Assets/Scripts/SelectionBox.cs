using UnityEngine;
using System.Collections;
using DaburuTools;
using DaburuTools.Action;

[RequireComponent(typeof(MeshRenderer))]
public class SelectionBox : MonoBehaviour 
{
	// Editable Variables
	[Header("Color Properties")]
	[SerializeField] private Color m_colorNormal = Color.white;
	[SerializeField] private Color m_colorHightlight = Color.white;

	// Un-editable Variables
	private int m_nX;
	private int m_nY;

	// Booleans
	private bool m_bIsAnimateSelect = false;

	// Component Variables
	private MeshRenderer m_meshRenderer = null;

	// Private Functions
	// Awake(): is called when the script first initialised
	void Awake ()
	{
		// Component Initialisation
		m_meshRenderer = GetComponent<MeshRenderer>();

		// Variables Initialisation
		m_nX = -1;
		m_nY = -1;
		m_bIsAnimateSelect = false;

		ColorNormal();
	}

	// Update(): is called once per frame
	void Update()
	{
		if (m_bIsAnimateSelect)
		{
			Color colorMaterial = m_meshRenderer.material.color;
			colorMaterial.a = 1f - transform.localScale.x / 3f;
			m_meshRenderer.material.color = colorMaterial;
		}
	}

	// Public Functions
	/// <summary>
	/// Perform a transition-in selection
	/// </summary>
	public void TransitionEnter()
	{
		ScaleToAction actScaleEnter = new ScaleToAction(transform, Graph.InverseExponential, Vector3.one, 0.25f);
		actScaleEnter.OnActionStart += () => { transform.localScale = new Vector3(2f, 0f, 2f); };
		ActionHandler.RunAction(actScaleEnter);
	}

	/// <summary>
	/// Perform a transition-out selection
	/// </summary>
	public void TransitionExit()
	{
		if (m_bIsAnimateSelect)
			return;

		ScaleToAction actScaleExit = new ScaleToAction(transform, Graph.Exponential, new Vector3(1f, 0f, 1f), 0.25f);
		actScaleExit.OnActionStart += () => { transform.localScale = Vector3.one; };
		ActionHandler.RunAction(actScaleExit);
	}

	/// <summary>
	/// Perform a transition successful selection
	/// </summary>
	public void TransitionSelect()
	{
		ScaleToAction actScaleSelect = new ScaleToAction(transform, new Vector3(3f, 3f, 3f), 0.25f);
		actScaleSelect.OnActionStart += () => 
		{ 
			transform.localScale = Vector3.one;
			m_bIsAnimateSelect = false;
		};
		actScaleSelect.OnActionFinish += () => { UI_PlayerTurn.Instance.ObjectPool_SelectionBox.PoolAllObjects(); };
		ActionHandler.RunAction(actScaleSelect);
	}

	/// <summary>
	/// Set the current color of the selection to the normal color
	/// </summary>
	public void ColorNormal()
	{
		m_meshRenderer.material.SetColor("_Color", m_colorNormal);
	}

	/// <summary>
	/// Set the current color of the selection to the normal color
	/// </summary>
	public void ColorHightlight()
	{
		m_meshRenderer.material.SetColor("_Color", m_colorHightlight);
	}

	/// <summary>
	/// Define the x and y grid coords
	/// </summary>
	/// <param name="_x"> The value of x </param>
	/// <param name="_y"> THe value of y </param>
	public void SetGridCoords(int _x, int _y)
	{
		m_nX = _x;
		m_nY = _y;
		transform.position = LevelManager.Instance.PlayerInstance.transform.position + new Vector3((float)m_nX, 0f, (float)m_nY);
	}

	// Getter-Setter Functions
	/// <summary>
	/// Define or return the x-position on the grid
	/// </summary>
	public int X { get { return m_nX; } set { m_nX = value; } }

	/// <summary>
	/// Define or return the y-position on the grid
	/// </summary>
	public int Y { get { return m_nY; } set { m_nY = value; } }
}
