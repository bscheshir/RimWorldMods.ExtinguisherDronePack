using System;
using Verse;

namespace ExtinguisherDronePack
{
    public class Command_ToggleWithRadius : Command_Toggle
    {
        public Action onHover;

        public override void GizmoUpdateOnMouseover()
        {
            base.GizmoUpdateOnMouseover();
            onHover?.Invoke();
        }
    }
}
