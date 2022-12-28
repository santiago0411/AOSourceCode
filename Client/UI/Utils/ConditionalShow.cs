using System;
using UnityEngine;

namespace AOClient.UI.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConditionalShow : PropertyAttribute
    {
        public readonly string ConditionalSourceField;
        public readonly int ConditionValue;

        public ConditionalShow(string conditionalSourceField, int conditionValue)
        {
            ConditionalSourceField = conditionalSourceField;
            ConditionValue = conditionValue;
        }
    }
}