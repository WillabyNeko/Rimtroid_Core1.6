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
    public class MetroidFeedingOptions
    {
        public string defName;
        public float powerConsumption;
        public float ticksForConsumption;
        public int durabilityDamage;
    }
    public class MetroidFeedingStationOptions : DefModExtension
    {
        public List<MetroidFeedingOptions> options;
    }
}
