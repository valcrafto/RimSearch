using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimSearch.Data
{
    /// <summary>
    /// Settings for this mod.
    /// </summary>
    public class RimSearchSettings : ModSettings
    {
        /// <summary>
        /// Default search term to use.
        /// </summary>
        public string defaultSearchTerm = "-.";

        public override void ExposeData()
        {
            Scribe_Values.Look(ref defaultSearchTerm, "defaultSearchTerm", "-.");
        }
    }
}
