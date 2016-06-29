using UnityEngine;
using System.Collections;
using DaburuTools;
using DaburuTools.Action;

public class CameraManager : MonoBehaviour 
{
	// Static Variables
	private static CameraManager ms_Instance = null;

	// Editable Variables
	[Header("Zoom Properties")]
	[SerializeField] private float m_fZoomDistanceFromTarget = 5f;	// m_fZoomDistanceFromTarget: The distance to the target in zoom mode
	[SerializeField] private float m_fDefaultZoomDistance = 25f;	// m_fDefaultZoomDistance: The default zoom distance

	[Header("Translation Properties")]
	[SerializeField] private float m_fBorderPercentage = 0.25f;		// m_fBorderPercentage: The maximum movement area that the camera can pan in from the edge of the map

	// Un-editable Variables
	private Vector3 m_vec3CameraDirection = Vector3.zero;			// m_vec3CameraDirection: The vector in which the camera is facing
	private SelectionBox m_selectionBoxPrevious = null;

	private bool m_bIsTransiting = false;
	private bool m_bIsZoom = false;

	// Component Variables
	private Transform m_trCamera = null;

	// Private Functions
	// Awake(): is called when the script is initialised
	void Awake()
	{
		// Singleton
		if (ms_Instance == null)
			ms_Instance = this;
		else
			Destroy(this);

		// Component Initialisation
		m_trCamera = transform.GetChild(0);
		if (m_trCamera == null)
			if (m_trCamera.GetComponent<Camera>() == null)
			{
				Debug.LogWarning(name + ".CameraManager.Awake(): Camera child not found!");
				Destroy(this);
				return;
			}

		// Variable Initialisation
		m_vec3CameraDirection = m_trCamera.rotation * Vector3.forward;

		m_bIsZoom = false;

		// Position Initialisation
		m_trCamera.position = -m_vec3CameraDirection * m_fDefaultZoomDistance;
	}

	// Update(): is called once per frame
	void Update()
	{
 		RaycastHit hitRaycast;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitRaycast))
		{
			// if: The hitpoint of the raycast is a selection box
			if (hitRaycast.transform.CompareTag("SelectionBox"))
			{
				// if: There is a previous selection box
				if (m_selectionBoxPrevious != null)
				{
					// if: The current selection box is not the same as the previous selection box
					if (hitRaycast.transform != m_selectionBoxPrevious.transform)
					{
						m_selectionBoxPrevious.ColorNormal();
						m_selectionBoxPrevious = hitRaycast.transform.GetComponent<SelectionBox>();
						m_selectionBoxPrevious.ColorHightlight();
					}
				}
				else
				{
					m_selectionBoxPrevious = hitRaycast.transform.GetComponent<SelectionBox>();
					m_selectionBoxPrevious.ColorHightlight();
				}

				// if: The player clicks on one of the selection box
				if (Input.GetMouseButtonDown(0))
				{
					m_selectionBoxPrevious.TransitionSelect();
					UI_PlayerTurn.Instance.SelectionClick();
				}

			}
		}
		else
		{
			// if: There is a selection box in the previous frame (but not this)
			if (m_selectionBoxPrevious != null)
			{
				m_selectionBoxPrevious.ColorNormal();
				m_selectionBoxPrevious = null;
			}
		}
	}

	// Public Functions
	/// <summary>
	/// Pan the camera towards the targeted area
	/// </summary>
	/// <param name="_vec3PositionOnGround"> The position on the ground it will be looking at </param>
	/// <param name="m_bIsAnimate"> Determine if the pan snaps to target directly or have an Action sequence </param>
	public void LookAt(Vector3 _vec3PositionOnGround, bool _bIsAnimate)
	{
		// if: The camera is currently transiting
		if (m_bIsTransiting)
			return;

		// if: Animation is not specified
		if (!_bIsAnimate)
		{
			m_trCamera.position = transform.position - m_vec3CameraDirection * m_fDefaultZoomDistance;
			transform.position = _vec3PositionOnGround;
			m_bIsZoom = false;
			return;
		}
		else 
		{
			// if: The camera is currently zoomed in, zoom the camera out and recall this method again
			if (m_bIsZoom)
			{
				MoveToAction actZoomOut = new MoveToAction(m_trCamera, Graph.SmoothStep, transform.position - m_vec3CameraDirection * m_fDefaultZoomDistance, 0.25f);
				actZoomOut.OnActionFinish += () =>
				{
					m_bIsTransiting = false;
					m_bIsZoom = false;
					LookAt(_vec3PositionOnGround, _bIsAnimate);
				};

				ActionHandler.RunAction(actZoomOut);
				m_bIsTransiting = true;
				return;
			}

			MoveToAction actMoveToLookAt = new MoveToAction(transform, Graph.SmoothStep, _vec3PositionOnGround, 0.25f);
			actMoveToLookAt.OnActionFinish += () => { m_bIsTransiting = false; };

			ActionHandler.RunAction(actMoveToLookAt);
			m_bIsTransiting = true;
		}
	}

	/// <summary>
	/// Zoom the camera in towards a target
	/// </summary>
	/// <param name="_vec3TargetTransform"> The target transform the camera will be zoom in towards </param>
	/// <param name="_bIsAnimate"> Determine if the pan snaps to target directly or have an Action sequence </param>
	public void ZoomInAt(Transform _vec3TargetTransform, bool _bIsAnimate)
	{
		ZoomInAt(_vec3TargetTransform.position, _bIsAnimate);
	}

	/// <summary>
	/// Zoom the camera in towards a target
	/// </summary>
	/// <param name="_vec3TargetPosition"> The target position the camera will be zoom in towards </param>
	/// <param name="_bIsAnimate"> Determine if the pan snaps to target directly or have an Action sequence </param>
	public void ZoomInAt(Vector3 _vec3TargetPosition, bool _bIsAnimate)
	{
		if (m_bIsTransiting)
			return;

		// if: Animation is not specified
		if (!_bIsAnimate)
		{
			m_trCamera.position = transform.position - m_vec3CameraDirection * m_fZoomDistanceFromTarget;
			transform.position = _vec3TargetPosition;
			m_bIsZoom = true;
			return;
		}
		else
		{
			LocalMoveToAction actZoomInCamera = new LocalMoveToAction(m_trCamera, Graph.SmoothStep, -m_vec3CameraDirection * m_fZoomDistanceFromTarget, 0.25f);
			MoveToAction actLookAtTarget = new MoveToAction(transform, Graph.SmoothStep, _vec3TargetPosition, 0.25f);
			actLookAtTarget.OnActionFinish += () =>
			{
				m_bIsTransiting = false;
				m_bIsZoom = true;
			};

			ActionHandler.RunAction(actZoomInCamera, actLookAtTarget);
			m_bIsTransiting = true;
		}
	}

	// Static Functions
	/// <summary>
	/// Returns the single instance of CameraManager
	/// </summary>
	public static CameraManager Instance { get { return ms_Instance; } }
}
