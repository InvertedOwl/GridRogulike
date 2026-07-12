using System.Collections.Generic;
using Cards.Actions;
using Util;
using Types;

namespace Cards.CardList
{



    // -- Developer cards --
    [CardDefinition("DeveloperAttack")]
    [StartingDeck(StartingDecks.developer, 2)]
    public sealed class DeveloperAttackDefinition : CardDefinition
    {
        public override string DisplayName => "Developer Attack";
        public override Rarity Rarity => Rarity.Developer;
        public override CardSet CardSet => CardSet.Developer;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.EveryEnemy);
        public override bool CanShowInShop => false;
        public override List<AbstractAction> BuildActions() => Actions(new AttackAllAction(0, "basic", null, 1000));
    }

    [CardDefinition("DeveloperPush")]
    [StartingDeck(StartingDecks.developer, 2)]
    public sealed class DeveloperPushDefinition : CardDefinition
    {
        public override string DisplayName => "Developer Push";
        public override Rarity Rarity => Rarity.Developer;
        public override CardSet CardSet => CardSet.Developer;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override bool CanShowInShop => false;
        public override List<AbstractAction> BuildActions() => Actions(new PushAllEnemiesAwayAction(0, "basic", null, 1000));
    }

    [CardDefinition("DeveloperShield")]
    [StartingDeck(StartingDecks.developer, 2)]
    public sealed class DeveloperShieldDefinition : CardDefinition
    {
        public override string DisplayName => "Developer Shield";
        public override Rarity Rarity => Rarity.Developer;
        public override CardSet CardSet => CardSet.Developer;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override bool CanShowInShop => false;
        public override List<AbstractAction> BuildActions() => Actions(new ShieldAction(0, "basic", null, 1000));
    }





    // -- Direct Offense --
    // Common
    [CardDefinition("Slash")]
    [StartingDeck(StartingDecks.basic, 4)]
    public sealed class Slash : CardDefinition
    {
        public override string DisplayName => "Quick Strike";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override bool CanShowInShop => false;
        public override List<AbstractAction> BuildActions() => Actions(new AttackAction(1, "basic", null, 6));
    }
    [CardDefinition("Lance")]
    public sealed class Lance : CardDefinition
    {
        public override string DisplayName => "Lance";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 2);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(new AttackAction(1, "basic", null, 4));
    }

    // -- Conditional offense --
    // Common
    [CardDefinition("Planted")]
    public sealed class Planted : CardDefinition
    {
        public override string DisplayName => "Planted";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 5),
            new AttackIfNotMovedAction(0, "basic", null, 4)
            );
    }
    // Uncommon
    [CardDefinition("Swift")]
    public sealed class Swift : CardDefinition
    {
        public override string DisplayName => "Swift";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 4),
            new AttackFromSteps(0, "basic", null)
        );
    }
    // Rare
    [CardDefinition("Surrounded")]
    public sealed class Surrounded : CardDefinition
    {
        public override string DisplayName => "Surrounded";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 10),
            new AttackIfSurroundedAction(1, "basic", null, 10)
        );
    }
    
    // -- Area Offense --
    // Wild Swipe
    [CardDefinition("Wild Swipe")]
    public sealed class WildSwipe : CardDefinition
    {
        public override string DisplayName => "Wild Swipe";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.EveryEnemy, 1);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackRadiusAction(2, "basic", null, 1, 9)
        );
    }
}
