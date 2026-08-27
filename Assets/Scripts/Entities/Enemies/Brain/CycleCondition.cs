using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "CycleCondition", menuName = "Game/Enemy Brain/Conditions/Cycle")]
    public class CycleCondition : EnemyBrainCondition
    {
        [SerializeField, Min(1)] private int optionCount = 2;

        public int OptionCount => Mathf.Max(1, optionCount);

        public override IReadOnlyList<string> GetOutputNames()
        {
            string[] outputNames = new string[OptionCount];
            for (int i = 0; i < outputNames.Length; i++)
                outputNames[i] = GetOutputName(i);

            return outputNames;
        }

        public override string SelectOutput(EnemyTurnContext context, string nodeGuid)
        {
            int selectedIndex = context?.SelectAndAdvanceCycle(nodeGuid, OptionCount) ?? 0;
            return GetOutputName(selectedIndex);
        }

        protected override bool Evaluate(EnemyTurnContext context)
        {
            // Cycle conditions select a numbered output instead of a boolean branch.
            return false;
        }

        private static string GetOutputName(int index)
        {
            return (index + 1).ToString(CultureInfo.InvariantCulture);
        }

        private void OnValidate()
        {
            optionCount = Mathf.Max(1, optionCount);
        }
    }
}
