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
    public class RimtroidSettings : ModSettings
    {
        // Evolution
        public static bool allowBerserkChanceMetroidHunger = true;
        public static bool allowWildChanceMetroidBerserk = true;
        public static bool allowBiggerMetroidsToBeTamed;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowBerserkChanceMetroidHunger, "allowBerserkChanceMetroidHunger", true);
            Scribe_Values.Look(ref allowBerserkChanceMetroidHunger, "allowBerserkChanceMetroidHunger", true);
            Scribe_Values.Look(ref allowBiggerMetroidsToBeTamed, "allowBiggerMetroidsToBeTamed");
        }

        [TweakValue("0RT", 0, 500)] public static float evolutionBoxHeight = 110;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Vector2 pos = new Vector2(rect.x, rect.y);
            if (Utils.EvolutionModIsEnabled)
            {
                var evolutionBox = new Rect(pos.x, pos.y, rect.width, evolutionBoxHeight);
                Listing_Standard ls_Evolution = new Listing_Standard();
                ls_Evolution.Begin(evolutionBox);
                ls_Evolution.Label("RT.EvolutionModName".Translate());
                var section = ls_Evolution.BeginSection_NewTemp(75);
                section.CheckboxLabeled("RT.allowBerserkChanceMetroidHunger".Translate(), ref allowBerserkChanceMetroidHunger, "RT.allowBerserkChanceMetroidHungerTooltip".Translate());
                section.CheckboxLabeled("RT.allowWildChanceMetroidBerserk".Translate(), ref allowWildChanceMetroidBerserk, "RT.allowWildChanceMetroidBerserkTooltip".Translate());
                section.CheckboxLabeled("RT.allowBiggerMetroidsToBeTamed".Translate(), ref allowBiggerMetroidsToBeTamed, "RT.allowBiggerMetroidsToBeTamedTooltip".Translate());
                section.End();
                ls_Evolution.End();
                pos.y += evolutionBox.yMax - 30;
            }
            base.Write();
        }

        private Vector2 scrollPosition = Vector2.zero;
    }
}
