
using System;
using System.Collections.Generic;
 
/// The deck the player owns and customises. Seeded once from the starting cards on GameManager.
public class PlayerDeck
{
    readonly List<CardData> cards;
 
    public int MinSize = 52;
    public IReadOnlyList<CardData> Cards => cards;
    public int Count => cards.Count;
    public event Action OnChanged;   // for the deck-editing UI
 
    public PlayerDeck(IEnumerable<CardData> defaults)
    {
        cards = new List<CardData>(defaults);
    }
 
    public void AddCard(CardData card)
    {
        if (canRemove(card)) return;   // don't allow duplicates
        if (card==null) throw new ArgumentNullException(nameof(card));
        cards.Add(card);
        OnChanged?.Invoke();
    }
 
    public bool CanRemove(CardData card) => cards.Contains(card);
 
    /// Removes one copy. Fails if the card isn't in the deck
    public bool RemoveCard(CardData card)
    {
        if (!CanRemove(card)) return false;
        cards.Remove(card);
        OnChanged?.Invoke();
        return true;
    }
 
    public int CountOf(CardData card)
    {
        int n = 0;
        foreach (var c in cards) if (c == card) n++;
        return n;
    }
 
    public void ResetToDefault(IEnumerable<CardData> defaults)
    {
        cards.Clear();
        cards.AddRange(defaults);
        OnChanged?.Invoke();
    }
}