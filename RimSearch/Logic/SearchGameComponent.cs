using RimSearch.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimSearch.Logic
{
    /// <summary>
    /// Game component which listens key presses.
    /// </summary>
    public class SearchGameComponent : GameComponent
    {
        public Game game;

        //Empty constructor.
        public SearchGameComponent()
        {

        }

        //Mandatory Constructor.
        public SearchGameComponent(Game game)
        {
            this.game = game;
        }

        /// <summary>
        /// Capture keypresses when its time to render the GUI.
        /// </summary>
        public override void GameComponentOnGUI()
        {
            //Check if our keybinding is pressed.
            if(RimSearchDefOf.RimSearch_Search != null && RimSearchDefOf.RimSearch_Search.IsDownEvent)
            {
                //Is our SearchWindow on the stack?
                if(Find.WindowStack.Windows.Count(window => window is SearchWindow) <= 0)
                {
                    //If not open our window on the stack.
                    Find.WindowStack.Add(new SearchWindow());
                }
            }
        }
    }
}
