﻿using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog
{
    class ITab_Pawn_Transmog : ITab
    {
        Vector2 scrollPosition = Vector2.zero;
        CompTransmog Preset => SelPawn.Preset();
        Texture2D Paint => ContentFinder<Texture2D>.Get("UI/Designators/Paint_Top");

        public ITab_Pawn_Transmog()
        {
            size = new Vector2(504, 400);
            labelKey = "Transmog.Transmog".Translate();
        }

        public override bool IsVisible => SelPawn.IsColonist;

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
                Find.WindowStack.Add(new Dialog_AddTransmog(SelPawn));

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

            var scrollviewHeight = Preset.ApparelCount * height;
            Widgets.BeginScrollView(new Rect(inRect.xMin, curY, width, inRect.height - curY), ref scrollPosition, new Rect(inRect.xMin, curY, width - margin, scrollviewHeight));

            TransmogApparel apparelToRemove = null;
            foreach (var apparel in Preset.apparel)
            {
                var rowRect = new Rect(inRect.x, curY, width, height);

                Widgets.ThingIcon(new Rect(inRect.x, curY, height, height), apparel.GetApparel());
                Widgets.Label(new Rect(inRect.x + height + gap, curY + 5f, width, height - 10f), apparel.GetApparel().def.LabelCap);

                rowRight = new WidgetRow(rowRect.xMax - margin, rowRect.y, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(TexButton.Delete))
                    apparelToRemove = apparel;
                if (rowRight.ButtonIcon(Paint))
                    Find.WindowStack.Add(new Dialog_EditTransmog(apparel));
                curY += height;
            }
            if (apparelToRemove != null)
                Preset.Remove(apparelToRemove);
            Widgets.EndScrollView();
        }
    }
}