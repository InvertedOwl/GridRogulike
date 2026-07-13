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
    // Uncommon
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

    // -- Player Displacement --
    // Common
    [CardDefinition("Force")]
    public sealed class Force : CardDefinition
    {
        public override string DisplayName => "Force";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 5),
            new PushPlayerAwayFromTargetAction(1, "basic", null, 1)
        );
    }
    // Rare
    [CardDefinition("Warp")]
    public sealed class Warp : CardDefinition
    {
        public override string DisplayName => "Warp";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.RandomEnemy);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new SwapWithEntityAction(0, "basic", null),
            new ScrapCurrentCardAction(0, "basic", null)
        );
    }

    // -- Direct Defense --
    // Common
    [CardDefinition("Block")]
    [StartingDeck(StartingDecks.basic, 4)]
    public sealed class Block : CardDefinition
    {
        public override string DisplayName => "Block";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.Self);
        public override bool CanShowInShop => false;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 6)
        );
    }
    [CardDefinition("Heavy Shield")]
    public sealed class HeavyShield : CardDefinition
    {
        public override string DisplayName => "Heavy Shield";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.Self);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(2, "basic", null, 14)
        );
    }

    // -- Conditional Defense --
    // Common
    [CardDefinition("Close Encounter")]
    public sealed class CloseEncounter : CardDefinition
    {
        public override string DisplayName => "Close Encounter";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.Self);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 3),
            new ShieldFromSurroundedAction(0, "basic", null)
        );
    }
    // Uncommon
    [CardDefinition("Fall Back")]
    public sealed class FallBack : CardDefinition
    {
        public override string DisplayName => "Fall Back";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.Self);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 5),
            new ShieldIfDoneDamageAction(0, "basic", null, 5, 10)
        );
    }
    
    // -- Conversion --
    // Uncommon
    [CardDefinition("Dropped")]
    public sealed class Dropped : CardDefinition
    {
        public override string DisplayName => "Dropped";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldDetonationAction(1, "basic", null),
            new RemoveAllShieldAction(0, "basic", null)
        );
    }
    // Rare
    [CardDefinition("Quick Footed")]
    public sealed class QuickFooted : CardDefinition
    {
        public override string DisplayName => "Quick Footed";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackFromTilesMovedThisCombatAction(1, "basic", null)
        );
    }
    
    
    // -- Draw and Filtering -- 
    // Uncommon
    [CardDefinition("Quick Draw")]
    public sealed class QuickDraw : CardDefinition
    {
        public override string DisplayName => "Quick Draw";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.Self);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new RaiseCostAction(0, "basic", null),
            new DrawCardAction(0, "basic", null, 2)
        );
    }
    
    // -- Draw and Filtering -- 
    // Uncommon
    [CardDefinition("Opening Act")]
    public sealed class OpeningAct : CardDefinition
    {
        public override string DisplayName => "Opening Act";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.Self);
        public override bool CanShowInShop => true;
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(0, "basic", null, 4),
            new FirstCardEnergyCardAction(0,  "basic", null, 1),
            new ScrapCurrentCardAction(0, "basic", null)
        );
    }
}
