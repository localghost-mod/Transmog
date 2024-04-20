﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Transmog
{
    class CompProperties_Transmog : CompProperties
    {
        public CompProperties_Transmog() => compClass = typeof(CompTransmog);
    }

    class CompTransmog : ThingComp
    {
        bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                Update();
            }
        }
        public List<TransmogApparel> transmog = new List<TransmogApparel>();
        public Stack<List<TransmogApparel>> history = new Stack<List<TransmogApparel>>();
        Pawn Pawn => parent as Pawn;
        public List<Apparel> Apparel => transmog.Select(transmog => transmog.GetApparel()).ToList();

        public void Save() => history.Push(transmog.Select(transmog => transmog.DuplicateForPawn(Pawn)).ToList());

        public void CopyFromApparel()
        {
            var newTransmog = Pawn.apparel.WornApparel.Select(
                apparel =>
                    new TransmogApparel()
                    {
                        Pawn = Pawn,
                        ApparelDef = apparel.def,
                        StyleDef = apparel.StyleDef,
                        Color = apparel.DrawColor
                    }
            );
            if (!transmog.SequenceEqual(newTransmog))
            {
                Save();
                transmog = newTransmog.ToList();
            }
            enabled = true;
            Update();
        }

        public void CopyFromPreset(List<TransmogApparel> preset)
        {
            var newTransmog = preset.Where(apparel => apparel.ApparelDef?.apparel.PawnCanWear(Pawn) ?? false).Select(apparel => apparel.DuplicateForPawn(Pawn));
            if (!transmog.SequenceEqual(newTransmog))
            {
                Save();
                transmog = newTransmog.ToList();
            }
            enabled = true;
            Update();
        }

        public void TryRevert()
        {
            if (history.Count == 0)
                return;
            transmog = history.Pop();
            Update();
        }

        public void Add(TransmogApparel transmog)
        {
            Save();
            this.transmog.Add(transmog);
            Update();
        }

        public void RemoveAt(int index)
        {
            Save();
            transmog.RemoveAt(index);
            Update();
        }

        public void Moveup(int index)
        {
            transmog.Reverse(index - 1, 2);
            Update();
        }

        public void Update() => Pawn.apparel.Notify_ApparelChanged();

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref enabled, "transmogEnabled");
            Scribe_Collections.Look(ref transmog, "transmog");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && transmog != null)
                transmog.ForEach(transmog => transmog.Pawn = Pawn);
        }
    }
}
