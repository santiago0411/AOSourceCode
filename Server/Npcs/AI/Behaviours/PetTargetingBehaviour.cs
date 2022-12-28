using System;

namespace AO.Npcs.AI.Behaviours
{
    public sealed class PetTargetingBehaviour : TargetingBehaviourBase
    {
        public override INpcAITarget CurrentTarget { get; protected set; }
        public override Action OnTargetFound { get; set; }
        public override Action<INpcAITarget> OnTargetMoved { get; set; }
        public override Action OnCurrentTargetInvalidated { get; set; }

        public void SetTarget(INpcAITarget target, bool overrideCurrentTarget)
        {
            if (overrideCurrentTarget || !IsCurrentTargetValid())
                SetNewTarget(target);
        }

        public override void TryFindNewTarget()
        {
            // Pets don't try to find a new target
        }
    }
}