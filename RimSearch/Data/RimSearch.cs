using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimSearch.Data
{
    /// <summary>
    /// Container for this mod.
    /// </summary>
    public class RimSearch : Mod
    {
        /// <summary>
        /// Settings container for this mod.
        /// </summary>
        public static RimSearchSettings settings;

        /// <summary>
        /// Obligatory constructor to set the content pack.
        /// </summary>
        /// <param name="content">Content pack for this mod.</param>
        public RimSearch(ModContentPack content) : base(content)
        {

            settings = GetSettings<RimSearchSettings>();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            //Row height to use.
            float rowHeight = (Text.LineHeight * 2.25f) + 2;
            
            float rowOffset = 0f;
            //Default search term.
            {
                //Label
                Rect labelRect = new Rect(inRect);
                labelRect.height = rowHeight;
                labelRect.y += rowOffset;
                string labelText = "RimSearchSettingsDefaultSearchTermLabel".Translate();
                labelRect.width = Text.CalcSize(labelText).x + 2;

                TextAnchor oldAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;

                Widgets.Label(labelRect, labelText);

                Text.Anchor = oldAnchor;

                //Textfield
                Rect textFieldRect = new Rect(labelRect);
                textFieldRect.x += labelRect.width;
                textFieldRect.width = inRect.width - labelRect.width;
                GUI.SetNextControlName("searchBarField");

                //settings.defaultSearchTerm = Widgets.TextField(textFieldRect, settings.defaultSearchTerm);
                settings.defaultSearchTerm = GUI.TextField(textFieldRect, settings.defaultSearchTerm);

                GUI.FocusControl("searchBarField");

            }
        }

        public override string SettingsCategory()
        {
            return "RimSearch";
        }
    }
}
