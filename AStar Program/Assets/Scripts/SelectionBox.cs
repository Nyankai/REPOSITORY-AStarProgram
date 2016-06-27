using UnityEngine;
using System.Collections;
using DaburuTools.Action;

[RequireComponent(typeof(MeshRenderer))]
public class SelectionBox : MonoBehaviour 
{
	// Un-editable Variables
	private ScaleToAction action_ScaleToStart = null;
	private ScaleToAction action_ScaleToSelect = null;

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
		action_ScaleToStart = new ScaleToAction(transform, Vector3.one, 0.25f);
		action_ScaleToStart.OnActionStart += () => { transform.localScale = new Vector3(2f, 0f, 2f); };

		action_ScaleToSelect = new ScaleToAction(transform, new Vector3(3f, 3f, 3f), 0.25f);
		action_ScaleToSelect.OnActionStart += () => { transform.localScale = Vector3.one; };
		action_ScaleToSelect.OnActionFinish += AnimateSelectFinish;

		AnimateSelect();
	}

	// Update(): is called once per frame
	void Update()
	{
		if (m_bIsAnimateSelect)
		{
			Color colorMaterial = m_meshRenderer.material.color;
			colorMaterial.a = 1f - action_ScaleToSelect.PercentageComplete;
			m_meshRenderer.material.color = colorMaterial;
		}
	}

	// AnimateSelectFinish(): This method will be called when AnimateSelect has finished
	private void AnimateSelectFinish()
	{
		Color colorMaterial = m_meshRenderer.material.color;
		colorMaterial.a = 1f;
		m_meshRenderer.material.color = colorMaterial;
		m_bIsAnimateSelect = false;
	}

	// Public Functions
	// AnimateStart(): Animates the starting part of the selection
	public void AnimateStart()
	{
		action_ScaleToSelect.StopAction(true);
		ActionHandler.RunAction(action_ScaleToStart);
	}

	// AnimateSelect(): Animates the selection when the player selects
	public void AnimateSelect()
	{
		action_ScaleToStart.StopAction(true);
		ActionHandler.RunAction(action_ScaleToSelect);
	}
}
