using RimWorld;
using RT_Rimtroid;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Metamorphosis
{
    // Token: 0x02000241 RID: 577
    public class HediffCompProperties_RimtroidCore : HediffCompProperties
    {
        public HediffCompProperties_RimtroidCore()
        {
            this.compClass = typeof(HediffComp_RimtroidCore);
        }

        public List<MetroidWhitelistDef> whitelists = new List<MetroidWhitelistDef>();
        public ThingDef huskDef = null;
        public List<RimtroidEvolutionPath> PossibleEvolutionPaths = new List<RimtroidEvolutionPath>();
        public HediffDef stuntedHediffDef; //Hediff which if it exists, the pawn shouldn't transform.
    }

    public class HediffComp_RimtroidCore : HediffComp
    {
        public HediffCompProperties_RimtroidCore Props
        {
            get
            {
                return (HediffCompProperties_RimtroidCore)this.props;
            }
        }

        //If it ever gains the 'stunted' hediff while the transformation is in progress, cancel it and remove the transformation hediff.
        //public override bool CompShouldRemove => base.CompShouldRemove || Pawn.health.hediffSet.GetFirstHediffOfDef(Props.stuntedHediffDef) != null;

        private void TransformPawn(PawnKindDef kindDef, bool changeDef = true, bool keep = false)
        {
            //sets position, faction and map
            IntVec3 intv = parent.pawn.Position;
            Faction faction = parent.pawn.Faction;
            Map map = parent.pawn.Map;
            RegionListersUpdater.DeregisterInRegions(parent.pawn, map);

            //Change Race to Props.raceDef
            if (changeDef && kindDef != null && kindDef != parent.pawn.kindDef)
            {
                parent.pawn.def = kindDef.race;
                parent.pawn.kindDef = kindDef;
                long ageB = Pawn.ageTracker.AgeBiologicalTicks;
                long ageC = Pawn.ageTracker.AgeChronologicalTicks;
                Pawn.ageTracker = new Pawn_AgeTracker(Pawn);
                Pawn.ageTracker.AgeBiologicalTicks = ageB;
                Pawn.ageTracker.AgeChronologicalTicks = ageC;

                //Remove all framework abilities.
                foreach (AbilityDef def in Pawn.abilities.abilities.OfType<RT_Core.Ability_Base>().Select(ability => ability.def).ToList())
                {
                    Pawn.abilities.RemoveAbility(def);
                }

                RT_Core.CompAbilityDefinition comp = Pawn.TryGetComp<RT_Core.CompAbilityDefinition>();
                if (comp != null)
                {
                    //Remove the old comp
                    Pawn.AllComps.Remove(comp);
                }

                //Try loading CompProperties from the def.
                CompProperties props = kindDef.race.CompDefFor<RT_Core.CompAbilityDefinition>();
                RT_Core.CompAbilityDefinition newComp = null;

                if (props != null)
                {
                    //CompProperties found, so should gain the comp.
                    newComp = (RT_Core.CompAbilityDefinition)Activator.CreateInstance(props.compClass); //Create ThingComp from the loaded CompProperties.
                    newComp.parent = Pawn; //Set Comp parent.
                    Pawn.AllComps.Add(newComp); //Add to pawn's comp list.
                    newComp.Initialize(props); //Initialize it.
                }

                if (newComp != null)
                {
                    //Optionally, carry the data over.
                    if (comp != null)
                    {
                        //[NOTE] To carry over the values, make sure you change both damageTotal and killCounter from private to public in CompAbilityDefinition.
                        //newComp.damageTotal = comp.damageTotal;
                        //newComp.killCounter = comp.killCounter;
                    }

                    //Tick the comp to force it to process/add abilities.
                    newComp.CompTickRare();
                }
            }

            RegionListersUpdater.RegisterInRegions(parent.pawn, map);
            map.mapPawns.UpdateRegistryForPawn(parent.pawn);

            //decache graphics
            parent.pawn.Drawer.renderer.graphics.ResolveAllGraphics();

            // remove non whitelisted hediffs
            if (!Pawn.health.hediffSet.hediffs.NullOrEmpty())
            {
                if (!Props.whitelists.NullOrEmpty())
                {
                    foreach (MetroidWhitelistDef list in Props.whitelists)
                    {
                        if (parent.pawn.health.hediffSet.hediffs.Any(x => !list.whitelist.Contains(x.def) && x != this.parent))
                        {
                            List<Hediff> removeable = parent.pawn.health.hediffSet.hediffs.Where(x => !list.whitelist.Contains(x.def) && x != this.parent).ToList();
                            foreach (Hediff item in removeable)
                            {
                                if (item != this.parent)
                                {
                                    Pawn.health.RemoveHediff(item);
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<Hediff> removeable = parent.pawn.health.hediffSet.hediffs;
                    foreach (Hediff item in removeable)
                    {
                        if (item != this.parent)
                        {
                            Pawn.health.RemoveHediff(item);
                        }
                    }
                }
            }

            //save the pawn
            //parent.pawn.ExposeData();
            if (parent.pawn.Faction != faction)
            {
                parent.pawn.SetFaction(faction);
            }
            //spawn Husk if set
            if (Props.huskDef != null)
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(Props.huskDef), parent.pawn.Position, parent.pawn.Map);
            }

            parent.pawn.needs.food.CurLevel = 1;
            var comp2 = Pawn.TryGetComp<CompEvolutionStage>();
            if (comp2 != null)
            {
                //Remove the old comp
                Pawn.AllComps.Remove(comp2);
            }
            //Try loading CompProperties from the def.
            var props2 = kindDef.race.GetCompProperties<CompProperties_EvolutionStage>();
            CompEvolutionStage newComp2 = null;
            if (props2 != null)
            {
                //CompProperties found, so should gain the comp.
                newComp2 = (CompEvolutionStage)Activator.CreateInstance(props2.compClass); //Create ThingComp from the loaded CompProperties.
                newComp2.parent = Pawn; //Set Comp parent.
                Pawn.AllComps.Add(newComp2); //Add to pawn's comp list.
                newComp2.Initialize(props2); //Initialize it.
            }
        }
        

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            //If the pawn has the 'stunted' hediff.
            if (Pawn.health.hediffSet.GetFirstHediffOfDef(Props.stuntedHediffDef) != null)
            {
                //Don't try to transform.
                return;
            }

            RimtroidEvolutionPath path = null;
            if (Props.PossibleEvolutionPaths.Any(x => x.triggerDef != null && Pawn.health.hediffSet.HasHediff(x.triggerDef)))
            {
                path = Props.PossibleEvolutionPaths.First(x => x.triggerDef != null && Pawn.health.hediffSet.HasHediff(x.triggerDef));
            }
            else if (Props.PossibleEvolutionPaths.Any(x => x.triggerDef == null))
            {
                path = Props.PossibleEvolutionPaths.FindAll(x => x.triggerDef == null).RandomElement();
            }
            if (path != null)
            {
                if (Pawn.ageTracker.AgeBiologicalYearsFloat > path.Age)
                {
                    TransformPawn(path.Kind);
                }
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToDisappear, "ticksToDisappear", 0, false);
        }

        public override string CompDebugString()
        {
            return "ticksToDisappear: " + this.ticksToDisappear;
        }

        public int ticksToDisappear = 60;
    }

    public class RimtroidEvolutionPath
    {
        public HediffDef triggerDef;
        public float Age = 0f;
        public PawnKindDef Kind;
    }
    public class MetroidWhitelistDef : Def
    {
        public List<HediffDef> whitelist = new List<HediffDef>();
    }
}
