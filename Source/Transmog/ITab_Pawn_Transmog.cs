﻿using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog
{
    class ITab_Pawn_Transmog : ITab
    {
        Vector2 scrollPosition = Vector2.zero;

        Pawn Pawn => SelPawn ?? (SelThing as Corpse).InnerPawn;
        CompTransmog Preset => Pawn.Preset();
        Texture2D Paint => ContentFinder<Texture2D>.Get("UI/Designators/Paint_Top");

        public ITab_Pawn_Transmog()
        {
            size = new Vector2(504, 400);
            labelKey = "Transmog.Transmog".Translate();
        }

        protected override void FillTab()
        {
            var margin = 16f;
            var inRect = new Rect(margin, 0, size.x - 2 * margin, size.y - margin);
            var height = 32f;
            var width = inRect.width - margin;
            var curY = 40f;
            var gap = 8f;

            Text.Font = GameFont.Small;

            new WidgetRow(inRect.xMin, curY, UIDirection.RightThenDown).Label("Enable".Translate());
            var rowRight = new WidgetRow(inRect.xMax, curY, UIDirection.LeftThenDown);
            if (rowRight.ButtonIcon(Preset.Enabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                Preset.Enabled ^= true;

            curY += height + gap;

            if (Widgets.ButtonText(new Rect(inRect.x, curY, width / 3 - gap, height), "Transmog.CopyFromApparel".Translate()))
                Preset.CopyFromApparel();

            if (Widgets.ButtonText(new Rect(inRect.x + 1 * width / 3 + gap / 2, curY, width / 3 - gap, height), "Add".Translate()))
                Find.WindowStack.Add(new Dialog_AddTransmog(Pawn));

            if (Widgets.ButtonText(new Rect(inRect.x + 2 * width / 3 + gap, curY, width / 3 - gap, height), "Transmog.Preset".Translate()))
                Find.WindowStack.Add(
                    new FloatMenu(
                        PresetManager
                            .presets.Select(
                                preset =>
                                    new FloatMenuOption(
                                        preset.Key,
                                        () =>
                                        {
                                            if (Event.current.shift)
                                                PresetManager.DelPreset(preset.Key);
                                            else
                                                Preset.CopyFromPreset(preset.Value);
                                        }
                                    )
                            )
                            .Append(new FloatMenuOption("Transmog.Save".Translate(), () => Find.WindowStack.Add(new Dialog_SavePreset(Preset))))
                            .ToList()
                    )
                );

            curY += height + gap;

            var scrollviewHeight = Preset.Apparel.Count * height;
            Widgets.BeginScrollView(new Rect(inRect.xMin, curY, width, inRect.height - curY), ref scrollPosition, new Rect(inRect.xMin, curY, width - margin, scrollviewHeight));

            int indexToMoveup = -1;
            int indexToRemove = -1;
            for (var i = 0; i < Preset.transmog.Count; ++i)
            {
                var transmog = Preset.transmog[i];
                var rowRect = new Rect(inRect.x, curY, width, height);

                if (i != 0 && Widgets.ButtonImage(new Rect(inRect.x, rowRect.y, height / 2, height / 2), TexButton.ReorderUp))
                    indexToMoveup = i;
                if (i != Preset.transmog.Count - 1 && Widgets.ButtonImage(new Rect(inRect.x, rowRect.y + height / 2, height / 2, height / 2), TexButton.ReorderDown))
                    indexToMoveup = i + 1;

                Widgets.ThingIcon(new Rect(inRect.x + height * 0.5f + gap, curY, height, height), transmog.GetApparel());
                Widgets.Label(new Rect(inRect.x + height * 1.5f + gap * 2, curY + 5f, width, height - 10f), transmog.GetApparel().def.LabelCap);

                rowRight = new WidgetRow(rowRect.xMax - margin, rowRect.y, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(TexButton.Delete))
                    indexToRemove = i;
                if (rowRight.ButtonIcon(Paint))
                {
                    transmog.Pawn = Pawn;
                    Find.WindowStack.Add(new Dialog_EditTransmog(transmog));
                }
                curY += height;
            }
            if (indexToMoveup != -1)
                Preset.Moveup(indexToMoveup);
            if (indexToRemove != -1)
                Preset.RemoveAt(indexToRemove);
            Widgets.EndScrollView();

            if (Event.current.keyCode == KeyBindingDefOf.Designator_Cancel.MainKey)
            {
                Preset.TryRevert();
                Event.current.Use();
            }
        }
    }
}
