using UnityEngine;
using System;
using System.Collections.Generic;

public class AStar : MonoBehaviour 
{
	// Editor Variables
	[SerializeField] private enum EnumPositionInputType { Transform = 0, Coordinates = 1 };
	[SerializeField] private EnumPositionInputType menum_positionInput;

	[SerializeField] private Transform m_startTransform;
	[SerializeField] private Transform m_targetTransform;
	[SerializeField] private Vector3 m_startPosition;
	[SerializeField] private Vector3 m_targetPosition;

	[SerializeField] private Vector3 m_position;
	[SerializeField] private int m_nGridSize = 10;
	[SerializeField] private float m_fSizePerGrid = 1f;

	[SerializeField] private enum EnumNeighbourGridSize { x3 = 0, x5 = 1, x7 = 2, x9 = 3 };
	[SerializeField] private EnumNeighbourGridSize menum_neighbourGridSize = EnumNeighbourGridSize.x5;
	[SerializeField] private bool[] marr_bNeighbourGrid;
	[SerializeField] private Vector2[] marr_neighbourToCheck;

	[SerializeField] private bool m_bIsDisplaySceneGrid = true;

	// Unimplemented Variables 
	//private bool m_bCostByDistance = true;
	//private Vector3 m_rotationOffset = Vector3.zero;

	// Private Variables
	private AStarNode[,] marr_gridData;
	private AStarNode m_startNode = null;
	private AStarNode m_targetNode = null;
	private bool[,] marr2_constrainGrid = null;
	private bool bIsComplete = false;
	private Vector3 mVec3_startPosition;
	private int[] marr_gridPosStart = new int[] { 0, 0 };
	private int[] marr_gridPosTarget = new int[] { 0, 0 };

	// Gizmos Functions
	void OnDrawGizmosSelected()
	{
		if (m_bIsDisplaySceneGrid)
		{
			Vector3 startPosition =
				m_position
				// Subtraction from grid space
				- new Vector3((float)m_nGridSize / 2f * m_fSizePerGrid, 0f, (float)m_nGridSize / 2f * m_fSizePerGrid);

			Gizmos.color = Color.grey;
			// for: every horizontal line...
			for (int i = 0; i < m_nGridSize + 1; i++)
			{
				Gizmos.DrawLine(
					// Left-node of the horizontal line
					Vector3.forward * i * m_fSizePerGrid + startPosition,
					// Right-node of the horizontal line
					Vector3.forward * i * m_fSizePerGrid + Vector3.right * m_fSizePerGrid * m_nGridSize + startPosition);
			}

			// for: every vertical line...
			for (int i = 0; i < m_nGridSize + 1; i++)
			{
				Gizmos.DrawLine(
					// Front-node of the vertical line
					Vector3.right * i * m_fSizePerGrid + startPosition,
					// Back-node of the vertical line
					Vector3.right * i * m_fSizePerGrid + Vector3.forward * m_fSizePerGrid * m_nGridSize + startPosition);
			}

			if (bIsComplete)
			{
				// Display actual path
				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(m_startNode.position, new Vector3(0.25f, 0f, 0.25f));
				if (m_startNode != null)
				{
					for (AStarNode i = m_startNode; i != null; i = i.linkTo)
					{
						if (i.linkTo != null)
						{
							Gizmos.DrawWireCube(i.linkTo.position, new Vector3(0.25f, 0f, 0.25f));
							Gizmos.DrawLine(i.position, i.linkTo.position);
						}
					}
				}
			}

			// Display all checked paths
			Gizmos.color = Color.yellow;
			for (int i = 0; i < m_nGridSize; i++)
				for (int j = 0; j < m_nGridSize; j++)
					if (marr_gridData != null)
						if (marr_gridData[j, i] != null)
						{
							Gizmos.DrawWireCube(marr_gridData[j, i].position, new Vector3(0.8f, 0f, 0.8f));
						}

			startPosition += new Vector3(m_fSizePerGrid / 2f, 0f, m_fSizePerGrid / 2f);

			Gizmos.color = Color.red;
			for (int i = 0; i < m_nGridSize; i++)
				for (int j = 0; j < m_nGridSize; j++)
					if (marr2_constrainGrid != null)
						if (!marr2_constrainGrid[j, i])
					{
						Gizmos.DrawWireCube(startPosition + new Vector3(j * m_fSizePerGrid, 0f, i * m_fSizePerGrid), new Vector3(0.8f, 0f, 0.8f));
					}
		}
	}

	// Public Functions
	/// <summary>
	/// Converts world-coordinates to an available grid position
	/// </summary>
	/// <param name="_position"> The world coordinates to convert to </param>
	/// <returns> Returns the nearest coordinates in the grid in an integer array (length of 2) </returns>
	public int[] Position2GridCoords(Vector3 _position)
	{
		int[] gridCoords = new int[] { -1, -1 };	// gridCoords: The returning int[]
		float fShortestMagnitude = -1f;				// fShortestMagnitude: The shortest magnitude between the target and the nearest grid

		for (int i = 0; i < m_nGridSize; i++)
			for (int j = 0; j < m_nGridSize; j++)
			{
				Vector3 currentGridPosition = mVec3_startPosition + new Vector3(j * m_fSizePerGrid, 0f, i * m_fSizePerGrid);
				float fCurrentMagnitude = Vector3.Magnitude(currentGridPosition - _position);

				if (fShortestMagnitude == -1f || fShortestMagnitude > fCurrentMagnitude)
				{
					gridCoords[0] = j; gridCoords[1] = i;
					fShortestMagnitude = fCurrentMagnitude;
				}
			}

		return gridCoords;
	}

	/// <summary>
	/// Recalculates the entire pathfinding entry - resets all its values
	/// </summary>
	/// <param name="isFindNearest"> Determines if the algorithm cannot find a path, should it return a path nearest to target instead? </param>
	/// <returns> Returns if a path is found from the start to target </returns>
	public bool Pathfind(bool isFindNearest = false)
	{
		// Double-Checking
		if (m_startTransform != null)
			m_startPosition = m_startTransform.position;
		if (m_targetTransform != null)
			m_targetPosition = m_targetTransform.position;

		// Local Variables Initialization
		// Re-initialise the start position
		mVec3_startPosition =
			m_position
			// Subtraction from grid space
			- new Vector3((float)m_nGridSize / 2f, 0f, (float)m_nGridSize / 2f)
			+ new Vector3((float)m_fSizePerGrid / 2f, 0f, (float)m_fSizePerGrid / 2f);

		// if: Checks if the contrain grid is defined or is in the correct size
		if (marr2_constrainGrid == null)
		{
			marr2_constrainGrid = new bool[m_nGridSize, m_nGridSize];
			for (int i = 0; i < m_nGridSize; i++)
				for (int j = 0; j < m_nGridSize; j++)
					marr2_constrainGrid[j, i] = true;
		}
		else if (marr2_constrainGrid.GetLength(0) != m_nGridSize || marr2_constrainGrid.GetLength(1) != m_nGridSize)
		{
			bool[,] marr2_resizeConstrainGrid = new bool[m_nGridSize, m_nGridSize];
			for (int i = 0; i < m_nGridSize; i++)
				for (int j = 0; j < m_nGridSize; j++)
				{
					if (i >= marr2_resizeConstrainGrid.GetLength(0) || j >= marr2_resizeConstrainGrid.GetLength(1))
						marr2_resizeConstrainGrid[j, i] = false;
					else
						marr2_resizeConstrainGrid = marr2_constrainGrid;
				}
			marr2_constrainGrid = marr2_resizeConstrainGrid;
		}

		// Class Variables Re-Initialization
		m_startNode = null;
		m_targetNode = null;
		marr_gridData = new AStarNode[m_nGridSize, m_nGridSize];

		int[] arr_gridPosStart = this.Position2GridCoords(m_startPosition);
		int[] arr_gridPosTarget = this.Position2GridCoords(m_targetPosition);

		// Initialises a new AStarNode as the start and target Node
		m_startNode = new AStarNode(
			mVec3_startPosition
			+ new Vector3(arr_gridPosStart[0] * m_fSizePerGrid, 0f, arr_gridPosStart[1] * m_fSizePerGrid));
		m_targetNode = new AStarNode(
			mVec3_startPosition
			+ new Vector3(arr_gridPosTarget[0] * m_fSizePerGrid, 0f, arr_gridPosTarget[1] * m_fSizePerGrid));

		// Checks if the start and target node is the same
		if (m_startNode.position == m_targetNode.position)
			return false;
		// else: Push it into the grid data array
		else
		{
			m_startNode.x = arr_gridPosStart[0]; m_startNode.y = arr_gridPosStart[1];
			m_targetNode.x = arr_gridPosTarget[0]; m_targetNode.y = arr_gridPosTarget[1];

			marr_gridData[arr_gridPosStart[0], arr_gridPosStart[1]] = m_startNode;
			marr_gridData[arr_gridPosTarget[0], arr_gridPosTarget[1]] = m_targetNode;
		}

		// Starting AStar Sequence
		List<AStarNode> mList_Open = new List<AStarNode>();
		AStarNode currentNode = null;

		m_targetNode.pathCost = 0;
		m_targetNode.heuristicCost = 0;

		m_startNode.pathCost = 0;
		m_startNode.heuristicCost = 0;
		mList_Open.Add(m_startNode);

		// if: There is no neighbours to check
		if (marr_neighbourToCheck.Length == 0)
		{
			Debug.LogWarning("AStar.Pathfind(): Detected there is no neighbours to check! (Stands still forever?)");
			return false;
		}

		// while: The pathfinding have not reached the target node...
		int nCheck = 0;
		while (nCheck <= m_nGridSize * m_nGridSize)
		{
			// if: Check if there is no more path to check - Result: It could be because it cannot reach the end
			if (mList_Open.Count == 0)
			{
				// if: The function does not want to return a nearest path
				if (!isFindNearest)
					return false;
				// else: It cannot reach the end, return a nearest path to it
				else
				{
					// for, for: Finds the nearest node closest to the target
					AStarNode nearestToTargetNode = null;
					for (int i = 0; i < marr_gridData.GetLength(0); i++)
					{
						for (int j = 0; j < marr_gridData.GetLength(1); j++)
						{
							if (marr_gridData[j, i] != null)
							{
								// if: The current node is not the start or the end
								if (marr_gridData[j, i] != m_startNode || marr_gridData[j, i] != m_targetNode)
								{
									if (nearestToTargetNode == null)
									{
										if (marr_gridData[j, i] != null)
											nearestToTargetNode = marr_gridData[j, i];
									}
									else if (Vector3.SqrMagnitude(m_targetNode.position - nearestToTargetNode.position)
										> Vector3.SqrMagnitude(marr_gridData[j, i].position - nearestToTargetNode.position))
									{
										nearestToTargetNode = marr_gridData[j, i];
									}
								}
							}
						}
					}

					//nearestToTargetNode.linkTo = null;

					for (AStarNode i = nearestToTargetNode; i.linkFrom != null; i = i.linkFrom)
					{
						i.linkFrom.linkTo = i;
					}

					bIsComplete = true;
					return true;
				}
			}

			// Finds a new currentNode
			// Note: The current currentNode is still the previous node after this for loop
			AStarNode smallestCostNode = null;
			for (int i = 0; i < mList_Open.Count; i++)
			{
				// else if: There is no smallest cost yet
				if (smallestCostNode == null)
					smallestCostNode = mList_Open[i];
				// else if: The current node's cost is smaller than the smallest
				else if (mList_Open[i].totalCost < smallestCostNode.totalCost)
					smallestCostNode = mList_Open[i];
			}
			mList_Open.Remove(smallestCostNode);

			// for: Since it detected that it has a better path here, update all the previous link nodes to link to this path instead
			for (AStarNode i = smallestCostNode; i != m_startNode; i = i.linkFrom)
				i.linkFrom.linkTo = i;

			currentNode = smallestCostNode;
			// for: Every neighbour to check...
			// At the end of this for loop, all approved neighbours to check will be in the open list
			for (int i = 0; i < marr_neighbourToCheck.Length; i++)
			{
				int nNeighbourX = currentNode.x + (int)marr_neighbourToCheck[i].x;
				int nNeighbourY = currentNode.y + (int)marr_neighbourToCheck[i].y;

				// if: Checks if the neighbour node is outside of the grid range
				if (nNeighbourX >= 0f && nNeighbourX < m_nGridSize &&
					nNeighbourY >= 0f && nNeighbourY < m_nGridSize)
				{
					// if: The current node is not an obstacle <- a.k.a is true in contrain grid
					if (marr2_constrainGrid[nNeighbourX, nNeighbourY])
					{
						// if: The neighbour node is the target
						if (marr_gridData[nNeighbourX, nNeighbourY] == m_targetNode)
						{
							currentNode.linkTo = m_targetNode;
							bIsComplete = true;
							return true;
						}
						// else if: The current array index is empty - it have never found a path to it before
						else if (marr_gridData[nNeighbourX, nNeighbourY] == null)
						{
							// Creates a new Node
							marr_gridData[nNeighbourX, nNeighbourY] = new AStarNode(
								mVec3_startPosition
								+ new Vector3(nNeighbourX * m_fSizePerGrid, 0f, nNeighbourY * m_fSizePerGrid));
							marr_gridData[nNeighbourX, nNeighbourY].x = nNeighbourX;
							marr_gridData[nNeighbourX, nNeighbourY].y = nNeighbourY;

							// Path Cost
							int nCurrentPathCost =
								(int)marr_neighbourToCheck[i].x * (int)marr_neighbourToCheck[i].x
								+ (int)marr_neighbourToCheck[i].y * (int)marr_neighbourToCheck[i].y;
							marr_gridData[nNeighbourX, nNeighbourY].pathCost = currentNode.pathCost + nCurrentPathCost;

							// Heuristic Cost
							marr_gridData[nNeighbourX, nNeighbourY].heuristicCost =
								Mathf.Abs(m_targetNode.x - nNeighbourX) * Mathf.Abs(m_targetNode.x - nNeighbourX)
								+ Mathf.Abs(m_targetNode.y - nNeighbourY) * Mathf.Abs(m_targetNode.y - nNeighbourY);

							// Get the linkFrom AStarNode
							marr_gridData[nNeighbourX, nNeighbourY].linkFrom = currentNode;

							mList_Open.Add(marr_gridData[nNeighbourX, nNeighbourY]);
						}
					}
				}
			}
			nCheck++;
		}

		Debug.LogWarning("AStar.PathFind(): Check more than " + nCheck + " times (more than grid-size). Is targetNode unreachable?");
		return false;
	}

	/// <summary>
	/// Sets the grid constrain
	/// </summary>
	/// <returns> Returns if a new grid constrain is initialise </returns>
	public bool SetGridConstrain(params bool[][,] _gridConstrains)
	{
		for (int i = 0; i < _gridConstrains.Length; i++)
			if (_gridConstrains[i].GetLength(0) > m_nGridSize || _gridConstrains[i].GetLength(1) > m_nGridSize)
			{
				Debug.LogWarning("AStar.SetGridConstrain(): One of the grid constrain length is longer than the grid size! Returns false!");
				return false;
			}

		marr2_constrainGrid = new bool[m_nGridSize, m_nGridSize];
		for (int i = 0; i < m_nGridSize; i++)
			for (int j = 0; j < m_nGridSize; j++)
				marr2_constrainGrid[j, i] = true;

		for (int k = 0; k < _gridConstrains.Length; k++)
		{
			// for, for: Reconstruct a new constrain
			for (int i = 0; i < marr2_constrainGrid.GetLength(0); i++)
				for (int j = 0; j < marr2_constrainGrid.GetLength(1); j++)
				{
					if (!_gridConstrains[k][j, i])
						marr2_constrainGrid[j, i] = false;
				}
		}
		return true;
	}

	public Vector3 GridIndex2Position(int _x, int _y)
	{
		return mVec3_startPosition + new Vector3(_x * m_fSizePerGrid, 0.0f, _y * m_fSizePerGrid);
	}

	// Getter-Setter Functions
	/// <summary>
	/// Returns the grid's contrain. To set the grid's contrain, use SetGridConstrain();
	/// </summary>
	public bool[,] GridConstrain { get { return marr2_constrainGrid; } }

	/// <summary>
	/// Returns the start node of the current AStar instance
	/// </summary>
	public AStarNode StartNode { get { return m_startNode; } }

	/// <summary>
	/// Returns the target node of the current AStar instance
	/// </summary>
	public AStarNode TargetNode { get { return m_targetNode; } }

	/// <summary>
	/// Returns or defines a list of vector2 position that the navigator is able to go to
	/// </summary>
	public Vector2[] NeighboursToCheck { get { return marr_neighbourToCheck; } set { marr_neighbourToCheck = value; } }

	/// <summary>
	/// Define or return the target transform
	/// </summary>
	public Transform TargetTransform { get { return m_targetTransform; } set { m_targetTransform = value; } }
}

public class AStarNode
{
	public Vector3 position;
	public int x;
	public int y;						
	public int pathCost = -1;			// pathCost: The cost from the start to the current node
	public int heuristicCost = -1;		// heuristicCost: The cost from the current node to the target node
	public AStarNode linkTo = null;
	public AStarNode linkFrom = null;

	// Constructor
	public AStarNode(Vector3 _position)
	{
		position = _position;
		x = -1;
		y = -1;
		pathCost = -1;
		heuristicCost = -1;
		linkTo = null;
		linkFrom = null;
	}

	public int totalCost { get { return pathCost + heuristicCost; } }
}
