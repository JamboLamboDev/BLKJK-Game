using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State { Idle, PlayerTurn, DealerTurn, RoundOver, Defeat }
    public enum RoundResult { PlayerWin, DealerWin, Push, PlayerBlackjack }

    [SerializeField] List<CardData> startingCards;   // the default deck; drag your card assets in here
    [SerializeField] int startingChips = 100;

    // ---- Set settings ----
    public int TargetChips = 200;          //the player loses if they fail to reach this amount before the end of a set.
    public int RoundsPerSet = 5;
    public int TargetIncreasePerSet = 100; // how much TargetChips grows each time a set is cleared (placeholder value)

    // UI listens to these; no game rules belong in the UI.
    public event Action<Hand, CardData> OnCardDrawn;
    public event Action<State> OnStateChanged;
    public event Action<RoundResult> OnRoundEnded;
    public event Action<int> OnSetCleared;   // passes the number of the set just cleared
    public event Action OnDefeat;            // placeholder: fired when the player misses TargetChips at the end of a set

    public PlayerDeck PlayerDeck { get; private set; }
    public Hand PlayerHand { get; } = new();
    public Hand DealerHand { get; } = new();
    public int Chips { get; private set; }
    public int CurrentSet { get; private set; } = 1;
    public int RoundsPlayedInSet { get; private set; }
    public State CurrentState { get; private set; } = State.Idle;
    public bool IsRoundActive => CurrentState == State.PlayerTurn || CurrentState == State.DealerTurn;
    public bool CanEditDeck => !IsRoundActive && CurrentState != State.Defeat;

    Deck deck;
    GameContext ctx;

    void Awake()
    {
        PlayerDeck = new PlayerDeck(startingCards);
        Chips = startingChips;
        ctx = new GameContext
        {
            PlayerHand = PlayerHand,
            DealerHand = DealerHand,
            DealTo = DealTo
        };
    }

    // ---------- Round flow ----------

    public void StartRound()
    {
        if (IsRoundActive || CurrentState == State.Defeat) return;

        PlayerHand.Clear();
        DealerHand.Clear();
        ctx.TargetScore = 21;

        deck = new Deck(PlayerDeck.Cards);
        ctx.Deck = deck;

        DealTo(PlayerHand);
        DealTo(DealerHand);
        DealTo(PlayerHand);
        DealTo(DealerHand);

        SetState(State.PlayerTurn);

        if (PlayerHand.IsBust(ctx.TargetScore)) EndRound();
        else if (PlayerHand.IsBlackjack(ctx.TargetScore)) Stand();
    }

    public void Hit()
    {
        if (CurrentState != State.PlayerTurn) return;

        DealTo(PlayerHand);
        if (PlayerHand.IsBust(ctx.TargetScore)) EndRound();
    }

    public void Stand()
    {
        if (CurrentState != State.PlayerTurn) return;

        SetState(State.DealerTurn);
        // Synchronous for now. Turn into a coroutine later if the UI needs to animate each draw.
        while (DealerHand.Score(ctx.TargetScore) < ctx.DealerStandOn &&
               !DealerHand.IsBust(ctx.TargetScore) &&
               deck.Count > 0)
        {
            DealTo(DealerHand);
        }
        EndRound();
    }

    void EndRound()
    {
        FireRoundEndEffects(PlayerHand);
        FireRoundEndEffects(DealerHand);

        SetState(State.RoundOver);
        OnRoundEnded?.Invoke(Resolve());

        RoundsPlayedInSet++;
        if (RoundsPlayedInSet >= RoundsPerSet) EndSet();   // after the round result so the UI still sees the final hands
    }

    RoundResult Resolve()
    {
        int t = ctx.TargetScore;
        if (PlayerHand.IsBust(t)) return RoundResult.DealerWin;
        if (DealerHand.IsBust(t)) return RoundResult.PlayerWin;

        bool pbj = PlayerHand.IsBlackjack(t);
        bool dbj = DealerHand.IsBlackjack(t);
        if (pbj && !dbj) return RoundResult.PlayerBlackjack;
        if (dbj && !pbj) return RoundResult.DealerWin;

        int p = PlayerHand.Score(t);
        int d = DealerHand.Score(t);
        if (p > d) return RoundResult.PlayerWin;
        if (p < d) return RoundResult.DealerWin;
        return RoundResult.Push;
    }

    // ---------- Drawing ----------

    void DealTo(Hand hand)
    {
        var card = deck.Draw();
        if (card == null) return;

        hand.Add(card);
        OnCardDrawn?.Invoke(hand, card);

        foreach (var e in card.effects)
            if (e != null) e.OnDrawn(card, hand, ctx);
    }

    void FireRoundEndEffects(Hand hand)
    {
        foreach (var card in new List<CardData>(hand.Cards))   // copy: effects may change the hand
            foreach (var e in card.effects)
                if (e != null) e.OnRoundEnd(card, hand, ctx);
    }

    void SetState(State s)
    {
        CurrentState = s;
        OnStateChanged?.Invoke(s);
    }

    // ---------- Deck customisation (no prices here; the shop system will handle those) ----------

    public bool RemoveCardFromDeck(CardData card)
    {
        if (!CanEditDeck) return false;
        return PlayerDeck.RemoveCard(card);
    }

    public bool AddCardToDeck(CardData card)
    {
        if (!CanEditDeck) return false;
        PlayerDeck.AddCard(card);
        return true;
    }

    // ---------- Chips ----------

    public void ChangeChips(int delta)   // positive = gain, negative = spend
    {
        Chips = Mathf.Max(0, Chips + delta);
    }

    // ---------- Sets ----------

    void EndSet()
    {
        if (Chips >= TargetChips)
        {
            int cleared = CurrentSet;
            CurrentSet++;
            RoundsPlayedInSet = 0;
            TargetChips += TargetIncreasePerSet;   // chips carry over into the next set
            OnSetCleared?.Invoke(cleared);
        }
        else
        {
            EnterDefeat();
        }
    }

    // PLACEHOLDER: nothing happens here yet beyond switching state and firing the event.
    // There is also no way out of Defeat yet (no restart/reset).
    void EnterDefeat()
    {
        SetState(State.Defeat);
        OnDefeat?.Invoke();
    }
}

    // to-do add scoring on a win, gaining chips. Some cards might boost it, and you gain more chips the closer to a blackjack. 

