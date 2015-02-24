﻿using System;
using RimWorld;
using Verse;

namespace A2B
{
    public class BeltSelectorComponent : BeltComponent
    {

        private IntRot nextDest = IntRot.west;
        private bool hasStorageSettings;
        private string _mythingID;
        private IntVec3 _splitterDest;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.LookValue<bool>(ref hasStorageSettings, "hasStorageSettings");
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();

            SlotGroupParent slotParent = parent as SlotGroupParent;
            if (slotParent == null)
            {
                throw new InvalidOperationException("parent is not a SlotGroupParent!");
            }

            // we kinda want to not overwrite custom storage settings every save/load...
            if (!hasStorageSettings)
                slotParent.GetStoreSettings().allowances.DisallowAll();

            hasStorageSettings = true;
        }

        public override IntVec3 GetDestinationForThing(Thing thing)
        {
            // Test the 'selection' idea ...
            SlotGroupParent slotParent = parent as SlotGroupParent;
            if (slotParent == null)
            {
                throw new InvalidOperationException("parent is not a SlotGroupParent!");
            }
            
            var selectionSettings = slotParent.GetStoreSettings();
            if (selectionSettings.AllowedToAccept(thing))
                return this.GetPositionFromRelativeRotation(IntRot.north);

            // A list of destinations - indexing modulo 2 lets us cycle them and avoid
            // long chains of if-statements.
            IntVec3[] dests = {
                this.GetPositionFromRelativeRotation(IntRot.west),
                this.GetPositionFromRelativeRotation(IntRot.east)
            };

            // Determine where we are going in the destination list (and default to left)
            int index = Math.Max(0, Array.FindIndex(dests, dir => (dir == _splitterDest)));

            // Do we have a new item ?
            if (_mythingID == thing.ThingID && IsFreeBelt(_splitterDest))
            {
                return _splitterDest;
            }
            else
            {
                _mythingID = thing.ThingID;

                // Try the next destination
                index = (index + 1) % 2;
                if (IsFreeBelt(dests[index]))
                {
                    _splitterDest = dests[index];
                    return _splitterDest;
                }

                // Try the one after that
                index = (index + 1) % 2;
                if (IsFreeBelt(dests[index]))
                {
                    _splitterDest = dests[index];
                    return _splitterDest;
                }

                // Give up and use our current destination
                return _splitterDest;
            }
        }

        private bool IsFreeBelt(IntVec3 position)
        {
            BeltComponent destBelt = position.GetBeltComponent();
            return (destBelt != null && destBelt.CanAcceptFrom(this));
        }
    }
}