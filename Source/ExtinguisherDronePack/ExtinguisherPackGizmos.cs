using System;
using UnityEngine;
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

    public class Gizmo_FabricatingVerbWrapper : Gizmo
    {
        private readonly Command_VerbTarget innerCommand;
        private readonly CompAutoExtinguisherPack packComp;

        public Gizmo_FabricatingVerbWrapper(Command_VerbTarget innerCommand, CompAutoExtinguisherPack packComp)
        {
            this.innerCommand = innerCommand;
            this.packComp = packComp;
            Order = innerCommand.Order;
        }

        public override float GetWidth(float maxWidth)
        {
            return innerCommand.GetWidth(maxWidth);
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            GizmoResult result = innerCommand.GizmoOnGUI(topLeft, maxWidth, parms);

            if (packComp != null && packComp.IsFabricating)
            {
                float width = GetWidth(maxWidth);
                Rect rect = new Rect(topLeft.x, topLeft.y, width, Height);
                float fillHeight = rect.height * packComp.FabricationProgress;
                Rect fillRect = new Rect(rect.x, rect.yMax - fillHeight, rect.width, fillHeight);
                GUI.DrawTexture(fillRect, ExtinguisherDronePackAssets.ProgressFillTex);

                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, packComp.FabricationSecondsLeft.ToString("0.0") + "s");
                Text.Anchor = TextAnchor.UpperLeft;
            }

            return result;
        }
    }
}