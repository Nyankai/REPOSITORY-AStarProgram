using UnityEngine;
using System.Collections;
using DaburuTools;
using DaburuTools.Action;

public class Player : Character
{
	// Uneditable Functions
	private EnumPieceType[] m_enumCards = null;
	private int m_nNextAvailableSlot = 4;

	private bool[,] marr2_gridData = null;
	private bool m_bIsExecuteTurn = false;

	// Protected Functions
	public override void ExecuteTurn()
	{
		UI_EnemyTurnTitle.Instance.TransitionExit(true);
		DrawCard(1);

		UI_PlayerTurn.Instance.BeginSequence();
	}

	public override void Kill()
	{
		LevelManager.Instance.RemoveCharacter(this as Character);
	}

	// Private Functions
	// Awake(): is called at the start of a program
	void Awake()
	{
		m_nX = -1;
		m_nY = -1;
		marr2_gridData = LevelManager.Instance.LevelData;
		menum_CharacterType = EnumCharacterType.Player;
		LevelManager.Instance.AddCharacter(this);

		m_enumCards = new EnumPieceType[5];
		for (int i = 0; i < m_enumCards.Length; i++)
			m_enumCards[i] = EnumPieceType.Null;
		DrawCard(2);
	}

	// Public Functions
	/// <summary>
	/// Draw cards for the player's hand
	/// </summary>
	/// <returns> Returns the number of cards that is actually added to the player's hand </returns>
	public int DrawCard()
	{
		return DrawCard(1);
	}

	/// <summary>
	/// Draw cards for the player's hand
	/// </summary>
	/// <param name="_nCardToDraw"> The number of cards to draw </param>
	/// <returns> Returns the number of cards that is actually added to the player's hand </returns>
	public int DrawCard(int _nCardToDraw)
	{
		// Moves all the cards to the front
		MoveCardsToFront();

		// if: There is no available card slot
		if (m_nNextAvailableSlot == -1)
			return 0;

		int nNewCards = 0;
		for (int i = 0; i < _nCardToDraw; i++)
		{
			m_enumCards[m_nNextAvailableSlot] = (EnumPieceType)UnityEngine.Random.Range(1, 5);
			m_nNextAvailableSlot++;
			nNewCards++;
			// if: There is no more available slots
			if (m_nNextAvailableSlot == m_enumCards.Length)
			{
				m_nNextAvailableSlot = -1;
				return nNewCards;
			}
		}
		return nNewCards;
	}

	/// <summary>
	/// Moves all the cards to the front of the deck
	/// </summary>
	public void MoveCardsToFront()
	{
		int nNextEmptySlot = -1;
		for (int i = 0; i < m_enumCards.Length; i++)
		{
			// if: The current slot is an empty slot
			if (m_enumCards[i] == EnumPieceType.Null)
			{
				m_nNextAvailableSlot = i;
				// for: Traverse through the rest of the array and find a card
				for (int j = i; j < m_enumCards.Length; j++)
				{
					// if: It is not an empty card, swap the card
					if (m_enumCards[j] != EnumPieceType.Null)
					{
						m_enumCards[i] = m_enumCards[j];
						m_enumCards[j] = EnumPieceType.Null;
						break;
					}
					// else if: it is the last card and there is no more empty card left
					else if (j == m_enumCards.Length - 1)
						return;
				}
			}
		}
		// Determine the next empty slot as a class scope
		m_nNextAvailableSlot = nNextEmptySlot;
	}

	// Getter-Setter Functions
	/// <summary>
	/// Returns the player's card deck
	/// </summary>
	public EnumPieceType[] CardDeck { get { return m_enumCards; } set { m_enumCards = value; } }
}
