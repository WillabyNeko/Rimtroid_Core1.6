using System;

using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RT_Core
{
    public class RT_Flight : ThingComp
    {




        public RT_FloatingComp Props
        {
            get
            {
                return (RT_FloatingComp)this.props;
            }
        }




    }
}