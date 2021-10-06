using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RT_Core;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT_Rimtroid
{
    public class CompEvolutionStage : ThingComp
    {
        public static List<CompEvolutionStage> comps = new List<CompEvolutionStage>();
        public CompProperties_EvolutionStage Props => base.props as CompProperties_EvolutionStage;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!comps.Contains(this))
            {
                comps.Add(this);
            }

            if (!respawningAfterLoad)
            {

                if (parent is Pawn pawn)
                {
                    if (Props.spawnStage != null)
                    {
                        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.spawnStage);
                        if (hediff == null)
                        {
                            var part = Props.partsToAffect != null ? pawn.def.race.body.AllParts.FirstOrDefault(x => x.def == Props.partsToAffect.RandomElement()) : null;
                            hediff = HediffMaker.MakeHediff(Props.spawnStage, pawn, part);
                        }
                        pawn.health.AddHediff(hediff);
                    }

                    if (pawn.RaceProps.hediffGiverSets != null)
                    {
                        foreach (var hediffGiver in pawn.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef set) => set.hediffGivers))
                        {
                            if (hediffGiver is HediffGiver_AfterPeriod)
                            {
                                hediffGiver.OnIntervalPassed(pawn, null);
                            }
                        }
                    }
                }
            }
        }

        public PawnKindDef pawnKindDefToEvolve;
        public int tickConversion;
        public List<HediffDef> hediffWhiteList;
        public Hediff evolutionSource;
        public void TransformPawn(PawnKindDef kindDef)
        {
            //sets position, faction and map
            IntVec3 intv = Metroid.Position;
            Faction faction = Metroid.Faction;
            Map map = Metroid.Map;
            RegionListersUpdater.DeregisterInRegions(Metroid, map);

            //Change Race to Props.raceDef
            if (kindDef != null && kindDef != Metroid.kindDef)
            {
                Metroid.def = kindDef.race;
                Metroid.kindDef = kindDef;
                long ageB = Metroid.ageTracker.AgeBiologicalTicks;
                long ageC = Metroid.ageTracker.AgeChronologicalTicks;
                Metroid.ageTracker = new Pawn_AgeTracker(Metroid);
                Metroid.ageTracker.AgeBiologicalTicks = ageB;
                Metroid.ageTracker.AgeChronologicalTicks = ageC;

                if (Metroid.abilities?.abilities != null)
                {
                    //Remove all framework abilities.
                    foreach (AbilityDef def in Metroid.abilities.abilities.OfType<RT_Core.Ability_Base>().Select(ability => ability.def).ToList())
                    {
                        Metroid.abilities.RemoveAbility(def);
                    }
                }


                CompAbilityDefinition comp = Metroid.TryGetComp<CompAbilityDefinition>();
                if (comp != null)
                {
                    //Remove the old comp
                    Metroid.AllComps.Remove(comp);
                }

                //Try loading CompProperties from the def.
                CompProperties props = kindDef.race.CompDefFor<RT_Core.CompAbilityDefinition>();
                RT_Core.CompAbilityDefinition newComp = null;

                if (props != null)
                {
                    //CompProperties found, so should gain the comp.
                    newComp = (RT_Core.CompAbilityDefinition)Activator.CreateInstance(props.compClass); //Create ThingComp from the loaded CompProperties.
                    newComp.parent = Metroid; //Set Comp parent.
                    Metroid.AllComps.Add(newComp); //Add to pawn's comp list.
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

            RegionListersUpdater.RegisterInRegions(Metroid, map);
            map.mapPawns.UpdateRegistryForPawn(Metroid);

            //decache graphics
            Metroid.Drawer.renderer.graphics.ResolveAllGraphics();

            // remove non whitelisted hediffs
            if (!Metroid.health.hediffSet.hediffs.NullOrEmpty())
            {
                if (!hediffWhiteList.NullOrEmpty())
                {
                    List<Hediff> removeable = Metroid.health.hediffSet.hediffs;
                    for (int num = removeable.Count - 1; num >= 0; num--)
                    {
                        var hediff = removeable[num];
                        if (!hediffWhiteList.Contains(hediff.def) && hediff != evolutionSource)
                        {
                            Metroid.health.RemoveHediff(hediff);
                        }
                    }
                }
                else
                {
                    List<Hediff> removeable = Metroid.health.hediffSet.hediffs;
                    for (int num = removeable.Count - 1; num >= 0; num--)
                    {
                        var item = removeable[num];
                        if (item != evolutionSource)
                        {
                            Metroid.health.RemoveHediff(item);
                        }
                    }
                }
            }

            //save the pawn
            //parent.pawn.ExposeData();
            if (Metroid.Faction != faction)
            {
                Metroid.SetFaction(faction);
            }

            Metroid.needs.food.CurLevel = 1;
            var comp2 = Metroid.TryGetComp<CompEvolutionStage>();
            if (comp2 != null)
            {
                //Remove the old comp
                Metroid.AllComps.Remove(comp2);
            }
            //Try loading CompProperties from the def.
            var props2 = kindDef.race.GetCompProperties<CompProperties_EvolutionStage>();
            CompEvolutionStage newComp2 = null;
            if (props2 != null)
            {
                //CompProperties found, so should gain the comp.
                newComp2 = (CompEvolutionStage)Activator.CreateInstance(props2.compClass); //Create ThingComp from the loaded CompProperties.
                newComp2.parent = Metroid; //Set Comp parent.
                Metroid.AllComps.Add(newComp2); //Add to pawn's comp list.
                newComp2.Initialize(props2); //Initialize it.
            }
            this.pawnKindDefToEvolve = null;
        }



        public int curEvolutionTryCount;
        public float nextEvolutionCheckYears;
        public Pawn Metroid => this.parent as Pawn;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref curEvolutionTryCount, "curEvolutionTryCount");
            Scribe_Values.Look(ref nextEvolutionCheckYears, "nextEvolutionCheckYears");

            Scribe_Defs.Look(ref pawnKindDefToEvolve, "pawnKindDefToEvolve");
            Scribe_Values.Look(ref tickConversion, "tickConversion");
            Scribe_Collections.Look(ref hediffWhiteList, "hediffWhiteList", LookMode.Def);
            Scribe_References.Look(ref evolutionSource, "evolutionSource");
        }
    }
}