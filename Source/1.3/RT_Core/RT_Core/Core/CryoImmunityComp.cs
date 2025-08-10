using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RT_Core
{
    public class CompCryoImmunity : ThingComp
    {
        public CompProperties_CryoImmunity Props
        {
            get
            {
                return (CompProperties_CryoImmunity)this.props;
            }
        }
    }
}

