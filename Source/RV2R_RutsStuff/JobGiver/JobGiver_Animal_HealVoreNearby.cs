﻿using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RV2R_RutsStuff
{
    public class JobGiver_Animal_HealVoreNearby : ThinkNode_JobGiver
    {

        private float radius = 30f;
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Animal_HealVoreNearby jobGiver_Animal_HealVoreNearby = (JobGiver_Animal_HealVoreNearby)base.DeepCopy(resolve);
            jobGiver_Animal_HealVoreNearby.radius = this.radius;
            return jobGiver_Animal_HealVoreNearby;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Predicate<Thing> predicate = delegate (Thing t)
            {
                Pawn pawn3 = (Pawn)t;
                return pawn3.Downed
                    && (pawn3.Faction == pawn.Faction || pawn3.Faction.PlayerRelationKind != FactionRelationKind.Hostile)
                    && (!pawn3.InBed() || (pawn3.health.summaryHealth.SummaryHealthPercent <= 0.6f && !pawn3.health.HasHediffsNeedingTendByPlayer()))
                    && pawn.CanReserve(pawn3, 1, -1, null, false)
                    && !pawn3.IsForbidden(pawn)
                    && (pawn.Map.designationManager.DesignationOn(pawn3) == null || pawn.Map.designationManager.DesignationOn(pawn3, DesignationDefOf.Tame) != null)
                    && !GenAI.InDangerousCombat(pawn)
                    && !GenAI.EnemyIsNear(pawn3, 20f)
                    && !pawn.ShouldBeSlaughtered()
                    && !pawn3.ShouldBeSlaughtered();
            };
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), this.radius, predicate, null, 0, -1, false, RegionType.Set_Passable, false);
            if (pawn2 == null)
                return null;

            List<VoreGoalDef> list = new List<VoreGoalDef> { VoreGoalDefOf.Heal };
            IEnumerable<VorePathDef> validPaths = VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, pawn2, VoreRole.Predator, true, false, false, null, null, null, null, list, null, null, null, null)).ValidPaths;
            if (validPaths.EnumerableNullOrEmpty<VorePathDef>())
            {
                RV2Log.Message("Predator " + pawn.LabelShort + " can't endo-rescue " + pawn2.LabelShort, "Jobs");
                return null;
            }
            VorePathDef vorePathDef = validPaths.RandomElement<VorePathDef>();
            RV2Log.Message("Endo rescue! " + vorePathDef.label, "Jobs");
            VoreJob voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, pawn2);
            voreJob.targetA = pawn2;
            voreJob.VorePath = vorePathDef;
            voreJob.Initiator = pawn;
            voreJob.count = 1;
            return voreJob;
        }
    }
}