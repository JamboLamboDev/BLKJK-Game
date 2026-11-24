using System.Collections.Generic;

public class Hand
{
    readonly List<CardData> cards = new();

    public IReadOnlyList<CardData> Cards => cards;

    public void Add(CardData card) => cards.Add(card);
    public void Clear() => cards.Clear();
    public bool Contains(CardData card) => cards.Contains(card);

    public int Score(int target = 21)
    {
        int total = 0, aces = 0;
        foreach (var c in cards)
        {
            int v = c.baseValue;
            foreach (var e in c.effects)
                if (e != null) v = e.ModifyValue(c, this, v);  
                if (c.isAce && 11 + total <= target && v < 11) v = 11;  // if the card is an ace, and the value is less than 11, and adding 11 would not bust, then use 11 instead of its original value
            total += v;
        }
        return total;
    }

    public bool IsBust(int target = 21) => Score(target) > target;
    public bool IsBlackjack(int target = 21) => cards.Count == 2 && Score(target) == target;
}
