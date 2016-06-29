using UnityEngine;
using System.Collections;

public class UI_StepArrow : MonoBehaviour 
{
	// Editable Variables
	[SerializeField] private float m_fMovementSpeed = 10f;

	// Component Variables
	private MeshRenderer m_meshRenderer = null;

	// Booleans
	private bool m_bIsAnimate = false;


	// Private Functions
	void Update()
	{
		if (m_bIsAnimate)
		{
			m_meshRenderer.material.mainTextureOffset += new Vector2(0f, 1f - (m_fMovementSpeed * Time.smoothDeltaTime) % 1f);
		}
	}

	// Public Functions
	/// <summary>
	/// Initialise the arrow properties
	/// </summary>
	/// <param name="_vec3FromPosition"> The starting position of the arrow </param>
	/// <param name="_vec3ToPosition"> The ending position of the arrow </param>
	public void Initialisation(Vector3 _vec3FromPosition, Vector3 _vec3ToPosition)
	{
		m_meshRenderer = GetComponent<MeshRenderer>();
		Vector3 vec3Direction = _vec3ToPosition - _vec3FromPosition;

		// Translation Settings
		transform.position = _vec3FromPosition + (vec3Direction / 2f);
		transform.rotation = Quaternion.LookRotation(vec3Direction, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
		transform.localScale += new Vector3(0f, vec3Direction.magnitude - 1f, 0f);
		
		// Texture Translation Settings
		m_meshRenderer.material.mainTextureScale = new Vector2(1f, vec3Direction.magnitude);

		m_bIsAnimate = true;
	}
}
