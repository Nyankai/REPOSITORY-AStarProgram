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
		DrawCard(1);
		UI_PlayerTurn.Instance.BeginSequence();
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
			switch (m_enumCards[i])
			{
 				case EnumPieceType.Null:
					// if: There is no empty slot found, then this will be the new empty slot
					if (nNextEmptySlot == -1)
						nNextEmptySlot = i;
					break;
				default:
					// if: There is no new empty slots
					if (nNextEmptySlot == -1)
						continue;

					// Moves the card to the new empty slot
					m_enumCards[nNextEmptySlot] = m_enumCards[i];
					m_enumCards[i] = EnumPieceType.Null;
					nNextEmptySlot = -1;

					// for: Finds a new empty slot (Note: Although the current slot is empty, there might be other empty slots infront)
					for (int j = 0; j < m_enumCards.Length; j++)
						if (m_enumCards[i] == EnumPieceType.Null)
						{
							nNextEmptySlot = i;
							break;
						}
					break;
			}
		}
		// Determine the next empty slot as a class scope
		m_nNextAvailableSlot = nNextEmptySlot;
	}

	// Getter-Setter Functions
	/// <summary>
	/// Returns the player's card deck
	/// </summary>
	public EnumPieceType[] CardDeck { get { return m_enumCards; } }
}
