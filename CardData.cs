using System.Collections.Generic;
using UnityEngine;

/// One asset per card (standard or special). Treat as read-only at runtime.
[CreateAssetMenu(menuName = "Blackjack/Card", fileName = "NewCard")]
public class CardData : ScriptableObject
{
    public string displayName;
    [TextArea] public string description;
    public Sprite art;

    [Header("Scoring")]
    public int baseValue;//value for score, 1-10, for aces, this would be 1, and ace card effect would be used to change the value to 11 if needed. 
    public bool isAce;          

    [Header("Special effects")]
    public List<CardEffect> effects = new();
}
