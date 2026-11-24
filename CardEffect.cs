using UnityEngine;

/// Base class for special card effects. 
public abstract class CardEffect : ScriptableObject
{
    /// Called right after the card is added to a hand.
    public virtual void OnDrawn(CardData card, Hand hand, GameContext ctx) { }
    public virtual int ModifyValue(CardData card, Hand hand, int value) => value;
    public virtual void OnDiscarded(CardData card, Hand hand, GameContext ctx) { }

    /// Return true on effects the player can trigger with a button. Only one activation is allowed per turn.
    public virtual bool CanActivate(CardData card, Hand hand, GameContext ctx) => false;
    public virtual void OnActivated(CardData card, Hand hand, GameContext ctx) { } // e.g. for a card that can be used once per round with a button

    public virtual void OnBought(CardData card, GameContext ctx, PlayerDeck deck) { }
    public virtual void OnSold(CardData card, GameContext ctx, PlayerDeck deck) { }
    public virtual void OnRoundEnd(CardData card, Hand hand, GameContext ctx) { }
}