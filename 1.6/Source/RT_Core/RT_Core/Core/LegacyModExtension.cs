﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Core
{
    public class LegacyModExtension : DefModExtension
    {
        public bool allowStackLimitExceed = false;
        public bool hasOwnership = false;
        public bool hasAbilities = false;
    }
}
