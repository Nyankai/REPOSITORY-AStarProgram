using UnityEngine;
using System.Collections;
using DaburuTools;
using DaburuTools.Action;	// DaburuTools.Actions: This is in v0.4

public class Enemy : Character 
{

	// Protected Functions
	public override void ExecuteTurn()
	{
		m_AStar.SetGridConstrain(LevelManager.Instance.LevelData, LevelManager.Instance.GetEnemyConstrain());
		if (!m_AStar.Pathfind(true))
		{
			Debug.LogWarning("Enemy.ExecuteTurn(): AStar.Pathfind is returning false!");
			return; 
		}
		if (m_AStar.StartNode.linkTo == m_AStar.TargetNode)
		{
			MoveToAction action_HitFirst = new MoveToAction(this.transform, m_AStar.TargetNode.position, 0.1f);
			MoveToAction action_HitSecond = new MoveToAction(this.transform, Graph.Exponential, m_AStar.StartNode.position, 0.25f);
			ActionSequence actionSeq_Hit = new ActionSequence(action_HitFirst, action_HitSecond);
			actionSeq_Hit.OnActionFinish += LevelManager.Instance.ExecuteNextTurn;

			ActionHandler.RunAction(actionSeq_Hit);
		}
		else
		{
			MoveToAction action_MoveTo = new MoveToAction(this.transform, Graph.InverseExponential, m_AStar.StartNode.linkTo.position, 0.5f);
			action_MoveTo.OnActionFinish += LevelManager.Instance.ExecuteNextTurn;

			ActionHandler.RunAction(action_MoveTo);
		}
	}

	// Private Functions
	void Awake()
	{
		m_AStar = GetComponent<AStar>();
		LevelManager.Instance.AddCharacter(this);
		menum_CharacterType = EnumCharacterType.Enemy;
	}

	private float HitFirstFunction(float _x) { return 1.0f - Mathf.Pow(1.0f - _x, 10f); }
}
