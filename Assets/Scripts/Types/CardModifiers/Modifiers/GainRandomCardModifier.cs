using System;
using System.Collections.Generic;
using Cards;
using Cards.CardEvents;
using Cards.CardList;
using StateManager;
namespace Types.CardModifiers.Modifiers
{
    public class GainRandomCardModifier : AbstractCardModifier
    {
        public GainRandomCardModifier()
        {
            this.ModifierText = "Gain a random uncommon card for the rest of the battle.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            Deck deck = Deck.Instance;
            List<Card> cardData = CardData.GetCardsByRarity(Rarity.Uncommon);
            
            deck.Hand.Add(deck.CreateCardMono(cardData[cardModifierRandom.Next(0, cardData.Count - 1)]));
            return cardEvent;
        }
    }
}