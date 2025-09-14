namespace Types.CardModifiers.Conditions
{
    public class AlwaysCardCondition: AbstractCardCondition
    {
        public AlwaysCardCondition()
        {
            this.ConditionText = "Always";
        }
        
        public override bool Condition()
        {
            return true;
        }
    }
}