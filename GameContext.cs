using System;

/// Shared state handed to effects so they can touch the game without knowing about GameManager.
public class GameContext
{
    public Deck Deck;
    public Hand PlayerHand;
    public Hand DealerHand;

    public int TargetScore = 21;      // effects may change this mid-round
    public int DealerStandOn = 17;

    public Action<Hand> DealTo;       // draw a card into a hand (fires all events and effects)
}