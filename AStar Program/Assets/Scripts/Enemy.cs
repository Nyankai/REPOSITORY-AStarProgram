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
			// Variables Updates

			// Action Handling
			MoveToAction actJump1 = new MoveToAction(transform, Graph.InverseExponential, m_AStar.TargetNode.position + new Vector3(0f, 2f, 0f), 0.5f);
			actJump1.OnActionStart += () =>
			{
				CameraManager.Instance.ZoomInAt(LevelManager.Instance.PlayerInstance.transform.position + new Vector3(0f, 1f), true);
			};
			MoveToAction actSlam = new MoveToAction(transform, Graph.Exponential, m_AStar.TargetNode.position, 0.1f);
			MoveToAction actJump2 = new MoveToAction(transform, Graph.InverseExponential, m_AStar.StartNode.position, 0.5f);
			MoveToAction actBackToStart = new MoveToAction(transform, Graph.Exponential, m_AStar.StartNode.position, 0.4f);

			ScaleToAction actSquash = new ScaleToAction(LevelManager.Instance.PlayerInstance.transform, Graph.Exponential, new Vector3(1f, 0f, 1f), 0.1f);
			ScaleToAction actInflate = new ScaleToAction(LevelManager.Instance.PlayerInstance.transform, Graph.InverseExponential, Vector3.one, 0.2f);
			ActionParallel actSlamAndSquash = new ActionParallel(actSlam, actSquash);
			actSlamAndSquash.OnActionStart += () =>
			{
				CameraManager.Instance.Shake();
			};
			actSlamAndSquash.OnActionFinish += () =>
			{
				CameraManager.Instance.LookAt(LevelManager.Instance.PlayerInstance.transform.position, true);
			};
			ActionParallel actJumpAndInflate = new ActionParallel(actJump2, actInflate);

			ActionSequence actionSeq_Hit = new ActionSequence(actJump1, actSlamAndSquash, actJumpAndInflate, actBackToStart);
			actionSeq_Hit.OnActionFinish += LevelManager.Instance.ExecuteNextTurn;

			ActionHandler.RunAction(actionSeq_Hit);
		}
		// else: Use the move animation
		else
		{
			// if: The there is no next path, the piece is basically trapped
			if (m_AStar.StartNode.linkTo == null)
				return;

			// Variables Updates
			int[] arr_nCoords = m_AStar.Position2GridCoords(m_AStar.StartNode.linkTo.position);
			m_nX = arr_nCoords[0];
			m_nY = arr_nCoords[1];

			// Action Handling
			MoveToAction action_MoveTo = new MoveToAction(this.transform, Graph.InverseExponential, m_AStar.StartNode.linkTo.position, 0.5f);
			action_MoveTo.OnActionFinish += LevelManager.Instance.ExecuteNextTurn;

			ActionHandler.RunAction(action_MoveTo);
		}
	}

	public override void Kill()
	{
		LevelManager.Instance.RemoveCharacter(this as Character);
	}

	// Public Functions
	// Initialise(): is called when the script is initialised
	public void Initialise()
	{
		m_nX = -1;
		m_nY = -1;
		m_AStar = GetComponent<AStar>();
		menum_CharacterType = EnumCharacterType.Enemy;
		LevelManager.Instance.AddCharacter(this);
	}

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
