using UnityEngine;
using System.Collections;
using DaburuTools;
using DaburuTools.Action;	// DaburuTools.Actions: This is in v0.4

public class Enemy : Character 
{
	// Editable Variables
	[SerializeField] private EnumPieceType m_enumPieceType = EnumPieceType.Null;

	// Protected Functions
	public override void ExecuteTurn()
	{
		UI_EnemyTurnTitle.Instance.TransitionEnemy(true, m_enumPieceType.ToString());

		// if: RemoteCallAStar() runs the A-Star pathfind once
		if (!RemoteCallAStar())
			return;

		// if: The next node is the player, use a different attacking animation
		if (m_AStar.StartNode.linkTo == m_AStar.TargetNode)
		{
			MoveToAction action_HitFirst = new MoveToAction(this.transform, m_AStar.TargetNode.position, 0.1f);
			MoveToAction action_HitSecond = new MoveToAction(this.transform, Graph.Exponential, m_AStar.StartNode.position, 0.4f);
			ActionSequence actionSeq_Hit = new ActionSequence(action_HitFirst, action_HitSecond);
			actionSeq_Hit.OnActionFinish += LevelManager.Instance.ExecuteNextTurn;

			ActionHandler.RunAction(actionSeq_Hit);
		}
		// else: Use the move animation
		else
		{
			MoveToAction action_MoveTo = new MoveToAction(this.transform, Graph.InverseExponential, m_AStar.StartNode.linkTo.position, 0.5f);
			action_MoveTo.OnActionFinish += LevelManager.Instance.ExecuteNextTurn;

			ActionHandler.RunAction(action_MoveTo);
		}
	}

	// Private Functions
	// Awake(): is called when the script is initialised
	void Awake()
	{
		m_nX = -1;
		m_nY = -1;
		m_AStar = GetComponent<AStar>();
		menum_CharacterType = EnumCharacterType.Enemy;
		LevelManager.Instance.AddCharacter(this);
	}

	// Public Functions
	/// <summary>
	/// A function to call the AStar of the enemy
	/// </summary>
	public bool RemoteCallAStar()
	{
		// Runs the AStar algorithm at least once
		m_AStar.SetGridConstrain(LevelManager.Instance.LevelData, LevelManager.Instance.GetCharacterConstrain(EnumCharacterType.Enemy));
		if (!m_AStar.Pathfind(true))
		{
			Debug.LogWarning("Enemy.ExecuteTurn(): AStar.Pathfind is returning false!");
			return false;
		}
		
		// Updates the enemies' grids coords
		int[] arr_nGridPos = m_AStar.Position2GridCoords(transform.position);
		m_nX = arr_nGridPos[0];
		m_nY = arr_nGridPos[1];
		return true;
	}

	private float HitFirstFunction(float _x) { return 1.0f - Mathf.Pow(1.0f - _x, 10f); }
}
