using UnityEngine;
using System.Collections;

public class ObjectPool : MonoBehaviour 
{
	// Editable Variables
	[SerializeField] private GameObject m_GOObject = null;		// m_GOObject: The object that it is going to used as the object pool
	[SerializeField] private int m_nPoolCount = 1;				// m_nPoolCount: The the number of object that can be pooled at once;

	// Uneditable Variables
	private GameObject[] marr_GOObjectPool = null;				// marr_GOObjectPool: The array of object pooling

	// Private Functions
	// Awake(): Use this for initialization
	void Awake()
	{
		// Variable Initialisation
		marr_GOObjectPool = new GameObject[m_nPoolCount];
		for (int i = 0; i < m_nPoolCount; i++)
		{
			marr_GOObjectPool[i] = Instantiate(m_GOObject as UnityEngine.Object, Vector3.zero, Quaternion.identity) as UnityEngine.GameObject;
			marr_GOObjectPool[i].transform.SetParent(this.transform);
			marr_GOObjectPool[i].SetActive(false);
		}
	}

	// Public Functions
	/// <summary>
	/// Returns an available object in the array
	/// </summary>
	/// <param name="_GOObject"> The reference to the object that is going to get from the object pool, returns null if all is used </param>
	/// <returns> Returns there is an object gotten from the object pool </returns>
	public bool GetObject(out GameObject _GOObject)
	{
		_GOObject = GetObjectAndReturn();
		if (_GOObject == null)
			return false;
		else
			return true;
	}

	/// <summary>
	/// Returns an available object in the array
	/// </summary>
	/// <returns> The reference to the object that is going to get from the object pool, returns null if all is used </returns>
	public GameObject GetObjectAndReturn()
	{
		for (int i = 0; i < marr_GOObjectPool.Length; i++)
		{
			if (!marr_GOObjectPool[i].activeSelf)
			{
				marr_GOObjectPool[i].SetActive(true);
				return marr_GOObjectPool[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Pool an object back into the object pool
	/// </summary>
	/// <param name="_GOObject"> The GameObject to be returned to the pool</param>
	/// <returns> Returns if the object is successfully pooled back into the object pool </returns>
	public bool PoolObject(GameObject _GOObject)
	{
		for (int i = 0; i < marr_GOObjectPool.Length; i++)
		{
			if (marr_GOObjectPool[i] == _GOObject)
			{
				marr_GOObjectPool[i].SetActive(false);
				marr_GOObjectPool[i].transform.position = Vector3.zero;
				marr_GOObjectPool[i].transform.rotation = Quaternion.identity;
				return true;
			}
		}
		return false;
	}
}
