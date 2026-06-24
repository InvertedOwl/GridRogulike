using System.Collections.Generic;
using Cards.Actions;
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





    // -- Attack cards --
    // Common
    [CardDefinition("Quick Strike")]
    [StartingDeck(StartingDecks.basic, 4)]
    public sealed class QuickStrikeDefinition : CardDefinition
    {
        public override string DisplayName => "Quick Strike";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override bool CanShowInShop => false;
        public override List<AbstractAction> BuildActions() => Actions(new AttackAction(1, "basic", null, 6));
    }

    [CardDefinition("Heavy Strike")]
    public sealed class HeavyStrikeDefinition : CardDefinition
    {
        public override string DisplayName => "Heavy Strike";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override List<AbstractAction> BuildActions() => Actions(new AttackAction(1, "basic", null, 9));
    }

    [CardDefinition("Lance")]
    public sealed class LanceDefinition : CardDefinition
    {
        public override string DisplayName => "Lance";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 2);
        public override List<AbstractAction> BuildActions() => Actions(new AttackAction(2, "basic", null, 6));
    }

    [CardDefinition("Sweep")]
    public sealed class SweepDefinition : CardDefinition
    {
        public override string DisplayName => "Sweep";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.EveryEnemy);
        public override List<AbstractAction> BuildActions() => Actions(new AttackAllAction(4, "basic", null, 6));
    }

    [CardDefinition("Close Sweep")]
    public sealed class CloseSweepDefinition : CardDefinition
    {
        public override string DisplayName => "Close Sweep";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new AttackRadiusAction(1, "basic", null, 1, 8));
    }

    [CardDefinition("Tap")]
    public sealed class TapDefinition : CardDefinition
    {
        public override string DisplayName => "Tap";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 2),
            new ApplyStatusToEntityAction(0, "basic", null, StatusApplicationType.Dizzy, 2));
    }

    // Uncommon
    [CardDefinition("Slash")]
    public sealed class SlashDefinition : CardDefinition
    {
        public override string DisplayName => "Slash";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 8),
            new AttackRadiusAction(1, "basic", null, 1, 4));
    }

    [CardDefinition("Wild Swing")]
    public sealed class WildSwingDefinition : CardDefinition
    {
        public override string DisplayName => "Wild Swing";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 12),
            new MoveRandomAction(0, "basic", null));
    }

    [CardDefinition("Shove")]
    public sealed class ShoveDefinition : CardDefinition
    {
        public override string DisplayName => "Shove";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 7),
            new PushEnemyAwayAction(1, "basic", null, 1));
    }

    [CardDefinition("Opening Act")]
    public sealed class OpeningActDefinition : CardDefinition
    {
        public override string DisplayName => "Opening Act";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(0, "basic", null, 4),
            new FirstCardEnergyCardAction(0, "basic", null, 1));
    }

    [CardDefinition("Reaching Guard")]
    public sealed class ReachingGuardDefinition : CardDefinition
    {
        public override string DisplayName => "Reaching Guard";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 5),
            new RangedSelfAction(1, "", null, 1));
    }

    // Rare
    [CardDefinition("Cleaver")]
    public sealed class CleaverDefinition : CardDefinition
    {
        public override string DisplayName => "Cleaver";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.EveryEnemy);
        public override List<AbstractAction> BuildActions() => Actions(new AttackAllAction(2, "basic", null, 8));
    }

    [CardDefinition("Wave")]
    public sealed class WaveDefinition : CardDefinition
    {
        public override string DisplayName => "Wave";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(1, "basic", null, 8),
            new PushAllEnemiesAwayAction(0, "basic", null, 1));
    }

    [CardDefinition("Sniper")]
    public sealed class SniperDefinition : CardDefinition
    {
        public override string DisplayName => "Sniper";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 2);
        public override List<AbstractAction> BuildActions() => Actions(new AttackAction(2, "basic", null, 18));
    }

    [CardDefinition("Execution")]
    public sealed class ExecutionDefinition : CardDefinition
    {
        public override string DisplayName => "Execution";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.AnyEnemy, 1);
        public override List<AbstractAction> BuildActions() => Actions(
            new AttackAction(2, "basic", null, 24),
            new GainEnergyIfPreviousEventDefeatedEnemyAction(2, "basic", null, 2));
    }




    // -- Block cards --
    // Common
    [CardDefinition("Guard")]
    [StartingDeck(StartingDecks.basic, 4)]
    public sealed class GuardDefinition : CardDefinition
    {
        public override string DisplayName => "Guard";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override bool CanShowInShop => false;
        public override List<AbstractAction> BuildActions() => Actions(new ShieldAction(1, "basic", null, 6));
    }

    [CardDefinition("Reinforce")]
    public sealed class ReinforceDefinition : CardDefinition
    {
        public override string DisplayName => "Reinforce";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new ShieldAction(1, "basic", null, 9));
    }

    [CardDefinition("Delayed Block")]
    public sealed class DelayedBlockDefinition : CardDefinition
    {
        public override string DisplayName => "Delayed Block";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new DelayedShieldAction(1, "basic", null, 18));
    }

    [CardDefinition("Heavy Guard")]
    public sealed class HeavyGuardDefinition : CardDefinition
    {
        public override string DisplayName => "Heavy Guard";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new ShieldAction(2, "basic", null, 22));
    }

    [CardDefinition("Drunken Guard")]
    public sealed class DrunkenGuardDefinition : CardDefinition
    {
        public override string DisplayName => "Drunken Guard";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 14),
            new MoveRandomAction(1, "basic", null));
    }

    [CardDefinition("Great Root")]
    public sealed class GreatRootDefinition : CardDefinition
    {
        public override string DisplayName => "Great Root";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 20),
            new RootSelfAction(0, "basic", null));
    }

    // Uncommon
    [CardDefinition("Shield Burst")]
    public sealed class ShieldBurstDefinition : CardDefinition
    {
        public override string DisplayName => "Shield Burst";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.EveryEnemy);
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(2, "basic", null, 8),
            new AttackAllAction(2, "basic", null, 4));
    }

    [CardDefinition("Repel")]
    public sealed class RepelDefinition : CardDefinition
    {
        public override string DisplayName => "Repel";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 8),
            new PushNearbyEnemiesAwayAction(1, "basic", null, 1));
    }

    [CardDefinition("Spiked Shield")]
    public sealed class SpikedShieldDefinition : CardDefinition
    {
        public override string DisplayName => "Spiked Shield";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 14),
            new AttackRadiusAction(1, "basic", null, 1, 3));
    }

    [CardDefinition("Turtle")]
    public sealed class TurtleDefinition : CardDefinition
    {
        public override string DisplayName => "Turtle";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new ShieldIfNoMoveAction(1, "basic", null, 24));
    }

    // Rare
    [CardDefinition("Double Double")]
    public sealed class DoubleDoubleDefinition : CardDefinition
    {
        public override string DisplayName => "Double Double";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new DoubleShieldAction(1, "basic", null));
    }

    [CardDefinition("Capitalism")]
    public sealed class CapitalismDefinition : CardDefinition
    {
        public override string DisplayName => "Capitalism";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new GainShieldForMoneyAction(1, "basic", null));
    }

    [CardDefinition("Iron Fortress")]
    public sealed class IronFortressDefinition : CardDefinition
    {
        public override string DisplayName => "Iron Fortress";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldAction(1, "basic", null, 120),
            new ShieldCarryoverStatusSelfAction(0, "basic", null, 10),
            new RootSelfForCombatAction(0, "basic", null));
    }

    [CardDefinition("Shield Detonation")]
    public sealed class ShieldDetonationDefinition : CardDefinition
    {
        public override string DisplayName => "Shield Detonation";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => new TargetDefinition(TargetType.EveryEnemy);
        public override List<AbstractAction> BuildActions() => Actions(
            new ShieldDetonationAction(2, "basic", null),
            new RemoveAllShieldAction(1, "basic", null));
    }

    [CardDefinition("Bullish")]
    public sealed class BullishDefinition : CardDefinition
    {
        public override string DisplayName => "Bullish";
        public override Rarity Rarity => Rarity.Rare;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new TripleShieldAction(1, "basic", null),
            new LoseQuarterMoneyAction(0, "basic", null));
    }




    // -- Step cards --
    // Common
    [CardDefinition("Retreat")]
    public sealed class RetreatDefinition : CardDefinition
    {
        public override string DisplayName => "Retreat";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override bool CanShowInShop => false;

        public override List<AbstractAction> BuildActions() => Actions(
            new PushPlayerAwayFromTargetAction(1, "basic", null, 1));
    }

    // Uncommon
    [CardDefinition("Homesick")]
    public sealed class HomesickDefinition : CardDefinition
    {
        public override string DisplayName => "Homesick";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new TeleportToStartingTileAction(1, "basic", null));
    }





    // -- Draw and resource cards --
    // Common
    [CardDefinition("Messy Draw")]
    public sealed class MessyDrawDefinition : CardDefinition
    {
        public override string DisplayName => "Messy Draw";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new DiscardCardsAction(0, "basic", null, 1),
            new DrawCardAction(1, "basic", null, 2));
    }

    [CardDefinition("Reset Button")]
    public sealed class ResetButtonDefinition : CardDefinition
    {
        public override string DisplayName => "Reset Button";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new DiscardHandAction(0, "basic", null),
            new DrawCardAction(1, "basic", null, 3),
            new GainEnergyAction(1, "basic", null, 1));
    }

    [CardDefinition("Toss")]
    public sealed class TossDefinition : CardDefinition
    {
        public override string DisplayName => "Toss";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new DiscardCardsAction(1, "basic", null, 2),
            new DrawCardAction(1, "basic", null, 4));
    }

    [CardDefinition("Moment")]
    public sealed class MomentDefinition : CardDefinition
    {
        public override string DisplayName => "Moment";
        public override Rarity Rarity => Rarity.Common;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new GainEnergyAction(1, "basic", null, 2),
            new DamageSelfAction(0, "basic", null, 5));
    }

    // Uncommon
    [CardDefinition("Quick Draw")]
    public sealed class QuickDrawDefinition : CardDefinition
    {
        public override string DisplayName => "Quick Draw";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(
            new RaiseCostAction(0, "basic", null),
            new DrawCardAction(0, "basic", null, 3));
    }

    [CardDefinition("Power Surge")]
    public sealed class PowerSurgeDefinition : CardDefinition
    {
        public override string DisplayName => "Power Surge";
        public override Rarity Rarity => Rarity.Uncommon;
        public override CardSet CardSet => CardSet.Base;
        public override TargetDefinition TargetDefinition => TargetDefinition.None;
        public override List<AbstractAction> BuildActions() => Actions(new GainEnergyAction(5, "basic", null, 6));
    }
}
