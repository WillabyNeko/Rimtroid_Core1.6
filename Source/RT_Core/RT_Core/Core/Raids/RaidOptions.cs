using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Core
{
    public class RaidOptions : DefModExtension
    {
        public PawnGroupMaker pawnGroup;
        public int minimumPawnCount;
        public int fixedRaidPoints = -1;
        public float raidPointsMultiplier = -1f;
        public FactionDef raidFaction;
        public RaidStrategyDef raidStrategy;
        public PawnsArrivalModeDef raidArrival;
    }
}