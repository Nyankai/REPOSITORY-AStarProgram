using UnityEngine;
using System.Collections.Generic;

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
	/// Returns an available object in the object pool
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
	/// Returns an available object in the object pool
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
	/// Returns a number of available object in the object pool
	/// </summary>
	/// <param name="_arr_GOObjects"> The reference to the objects that is going to get from the object pool </param>
	/// <returns> Returns if it retrieves the same number of objects as requested </returns>
	public bool GetObjects(int _nGetCount, out GameObject[] _arr_GOObjects)
	{
		_arr_GOObjects = GetObjectsAndReturn(_nGetCount);
		if (_nGetCount == _arr_GOObjects.Length)
			return true;
		else
			return false;
	}

	/// <summary>
	/// Returns a number of available object in the object pool
	/// </summary>
	/// <param name="_nGetCount"> The number of objects to be retrived </param>
	/// <returns> Returns the objects in an array </returns>
	public GameObject[] GetObjectsAndReturn(int _nGetCount)
	{
		if (_nGetCount <= 0)
			return null;

		List<GameObject> list_GOObjects = new List<GameObject>();
		for (int i = 0; i < marr_GOObjectPool.Length; i++)
		{
			if (!marr_GOObjectPool[i].activeSelf)
			{
				marr_GOObjectPool[i].SetActive(true);
				list_GOObjects.Add(marr_GOObjectPool[i]);

				// if: The list reaches the number of items needed
				if (list_GOObjects.Count == _nGetCount)
					return list_GOObjects.ToArray();
			}
		}
		Debug.Log(name + ".ObjectPool.GetObjectsAndReturn(): You have exhausted the pool! (Consider increasing the limit?)");
		return list_GOObjects.ToArray();
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
			if (marr_GOObjectPool[i].GetInstanceID() == _GOObject.GetInstanceID())
			{
				marr_GOObjectPool[i].SetActive(false);
				marr_GOObjectPool[i].transform.position = Vector3.zero;
				marr_GOObjectPool[i].transform.rotation = Quaternion.identity;
				marr_GOObjectPool[i].transform.localScale = Vector3.one;
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Pool all the objects back into the object pool
	/// </summary>
	public void PoolAllObjects()
	{
		for (int i = 0; i < marr_GOObjectPool.Length; i++)
		{
			marr_GOObjectPool[i].SetActive(false);
			marr_GOObjectPool[i].transform.position = Vector3.zero;
			marr_GOObjectPool[i].transform.rotation = Quaternion.identity;
			marr_GOObjectPool[i].transform.localScale = Vector3.one;
		}
	}
}
