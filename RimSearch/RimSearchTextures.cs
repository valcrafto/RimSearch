using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimSearch
{
    /// <summary>
    /// Workaround to get our textures.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class RimSearchTextures
    {
        /// <summary>
        /// Empty static constructor.
        /// </summary>
        static RimSearchTextures()
        {

        }

        /// <summary>
        /// Looking glass icon.
        /// </summary>
        public static readonly Texture2D ExpandingIcons = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenInspector", true);
    }
}
