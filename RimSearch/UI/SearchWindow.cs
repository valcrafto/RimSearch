using RimSearch.Logic;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimSearch.UI
{
    /// <summary>
    /// Window for performing search in.
    /// </summary>
    public class SearchWindow : Window
    {
        /// <summary>
        /// Initial requested size.
        /// </summary>
        public override Vector2 InitialSize => new Vector2(640, 480);

        /// <summary>
        /// Current search term for the query to use.
        /// </summary>
        public string searchTerm = Data.RimSearch.settings.defaultSearchTerm;

        /// <summary>
        /// Is the search term dirty?
        /// </summary>
        public bool searchTermDirty = false;
        /// <summary>
        /// How many ticks since the search term was last dirty.
        /// </summary>
        public int ticksSinceLastSearchTermEdit = 0;

        /// <summary>
        /// How many ticks before automatically starting a search.
        /// </summary>
        public static int ticksToPassBeforeStartingSearch = 40;
        /// <summary>
        /// Instruction string.
        /// </summary>
        public static string instructionString = "";

        /// <summary>
        /// Our current search query results.
        /// </summary>
        public SearchQuery searchQuery = null;

        /// <summary>
        /// Working variable for the scroll window.
        /// </summary>
        public Vector2 resultsAreaScroll = new Vector2();

        /// <summary>
        /// Constructor.
        /// </summary>
        public SearchWindow()
        {
            optionalTitle = "RimSearch";

            preventCameraMotion = false;
            absorbInputAroundWindow = false;
            draggable = true;
            doCloseX = true;

            //Set instruction string.
            instructionString = "RimSearchInformation".Translate();
        }

        /// <summary>
        /// Draw the window and perform logic.
        /// </summary>
        /// <param name="inRect"></param>
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            //Setup the help rect.
            Rect searchInfoRect = new Rect(inRect);
            searchInfoRect.height = Text.CalcHeight(instructionString, searchInfoRect.width) + 2;

            //Draw the help information.
            Widgets.Label(searchInfoRect, instructionString);

            Rect searchBarRect = new Rect(inRect);
            searchBarRect.y += searchInfoRect.height;
            searchBarRect.height = Text.LineHeight + 2;
            //searchBarRect.width = searchBarRect.width/4f;


            //Draw search bar widget.
            string oldSearchTerm = searchTerm;
            
            searchTerm = Widgets.TextField(searchBarRect, searchTerm);
            //searchTerm = SearchGUI.FormattedTextField(searchBarRect, searchTerm);
            

            //If search terms do not match; 
            if (searchTerm != oldSearchTerm)
            {
                searchTermDirty = true;
                ticksSinceLastSearchTermEdit = 0;
            }

            //TO-DO; Can bypass time needed to wait by pressing Enter.

            //If our search term is dirty and ticks needed to pass exceeded the value.
            if(searchTermDirty)
            {
                ticksSinceLastSearchTermEdit++;

                if (ticksSinceLastSearchTermEdit >= ticksToPassBeforeStartingSearch)
                {
                    //Reset dirty status.
                    searchTermDirty = false;

                    //Make our search query.
                    ConstructSearchQuery();
                    searchQuery.Execute();
                }
            }

            //Draw the search query (if any)
            if(searchQuery != null)
            {
                //Setup our scroll view outer rect.
                Rect outerRect = new Rect(searchBarRect);
                outerRect.y += searchBarRect.height + 2;
                outerRect.height = inRect.height - outerRect.y - 2;

                //Setup our scroll view inner rect.
                Rect innerRect = new Rect(outerRect);
                innerRect.width -= 16f;

                //How high a row can be.
                float rowHeight = (Text.LineHeight * 2.25f) + 2;

                //Height of world objects search result section.
                float worlObjectsHeight = searchQuery.worldObjectResultSet.Count > 0 ? ((searchQuery.worldObjectResultSet.Count + 1) * rowHeight) : 0;
                //Height of pawns search result section.
                float pawnsHeight = searchQuery.pawnResultSet.Count > 0 ? ((searchQuery.pawnResultSet.Count + 1) * rowHeight) : 0;
                //Height of things search result section.
                float thingsHeight = searchQuery.thingResultSet.Count > 0 ? ((searchQuery.thingResultSet.Count + 1) * rowHeight) : 0;

                //Set inner view height to appropiate height.
                innerRect.height = worlObjectsHeight + pawnsHeight + thingsHeight+ rowHeight;

                Widgets.BeginScrollView(outerRect, ref resultsAreaScroll, innerRect, true);

                Rect rowRect = new Rect(innerRect);
                rowRect.height = rowHeight;

                //Pawns
                if(searchQuery.pawnResultSet.Count > 0)
                {
                    Text.Font = GameFont.Medium;
                    Text.Anchor = TextAnchor.MiddleLeft;

                    //Section label
                    Rect sectionRect = new Rect(rowRect);
                    sectionRect.x += 4f;
                    sectionRect.width -= 4f;

                    Widgets.Label(sectionRect, "RimSearchResultsPawns".Translate(searchQuery.pawnResultSet.Count));
                    Widgets.DrawLineHorizontal(rowRect.x, rowRect.y + rowRect.height, rowRect.width);

                    Text.Font = GameFont.Small;

                    //Next row
                    rowRect.y += rowHeight;

                    foreach(Pawn pawn in searchQuery.pawnResultSet)
                    {
                        //Draw pawn stuff.
                        Widgets.DrawHighlightIfMouseover(rowRect);

                        //Draw portrait
                        Rect portraitRect = new Rect(rowRect);
                        portraitRect.width = portraitRect.height;

                        Widgets.ThingIcon(portraitRect, pawn);

                        //Draw name
                        Rect labelRect = new Rect(rowRect);
                        labelRect.width = labelRect.width - portraitRect.width - (rowHeight * 2);
                        labelRect.x += portraitRect.width;

                        Widgets.Label(labelRect, pawn.LabelCap);

                        //Draw go-to button
                        Rect goToRect = new Rect(rowRect);
                        goToRect.width = goToRect.height;
                        goToRect.x = labelRect.x + labelRect.width;

                        Rect goToRectIcon = goToRect.ContractedBy(goToRect.width / 4f);

                        if (Widgets.ButtonImage(goToRectIcon, RimSearchTextures.ExpandingIcons))
                        {
                            //Close this dialog.
                            //Close(false);

                            //Switch visible map if needed.
                            if (Current.Game.CurrentMap != pawn.Map)
                                Current.Game.CurrentMap = pawn.Map;

                            //Focus camera on thing.
                            Find.CameraDriver.JumpToCurrentMapLoc(pawn.Position);

                            //Select the thing.
                            Find.Selector.Select(pawn);

                            break;
                        }

                        //Draw info card button
                        Rect infoCardRect = new Rect(rowRect);
                        infoCardRect.width = infoCardRect.height;
                        infoCardRect.x = goToRect.x + goToRect.width;

                        Widgets.InfoCardButton(infoCardRect.x + (infoCardRect.width / 2f) - 12f, infoCardRect.y + (infoCardRect.height / 2f) - 12f, pawn);

                        //Draw Tooltip
                        TooltipHandler.TipRegion(rowRect, pawn.MainDesc(true));

                        //Next row
                        rowRect.y += rowHeight;
                    }
                }

                //Things
                if (searchQuery.thingResultSet.Count > 0)
                {
                    Text.Font = GameFont.Medium;
                    Text.Anchor = TextAnchor.MiddleLeft;

                    //Section label
                    Rect sectionRect = new Rect(rowRect);
                    sectionRect.x += 4f;
                    sectionRect.width -= 4f;

                    Widgets.Label(sectionRect, "RimSearchResultsThings".Translate(searchQuery.thingResultSet.Count));
                    Widgets.DrawLineHorizontal(rowRect.x, rowRect.y + rowRect.height, rowRect.width);

                    Text.Font = GameFont.Small;

                    //Next row
                    rowRect.y += rowHeight;

                    foreach (Thing thing in searchQuery.thingResultSet)
                    {
                        //Draw thing stuff.
                        Widgets.DrawHighlightIfMouseover(rowRect);

                        //Draw portrait
                        Rect portraitRect = new Rect(rowRect);
                        portraitRect.width = portraitRect.height;

                        Widgets.ThingIcon(portraitRect, thing);

                        //Draw name
                        Rect labelRect = new Rect(rowRect);
                        labelRect.width = labelRect.width - portraitRect.width - (rowHeight * 2);
                        labelRect.x += portraitRect.width;

                        Widgets.Label(labelRect, thing.LabelCap);

                        //Draw go-to button
                        Rect goToRect = new Rect(rowRect);
                        goToRect.width = goToRect.height;
                        goToRect.x = labelRect.x + labelRect.width;

                        Rect goToRectIcon = goToRect.ContractedBy(goToRect.width / 4f);

                        if (Widgets.ButtonImage(goToRectIcon, RimSearchTextures.ExpandingIcons))
                        {
                            //Close this dialog.
                            //Close(false);

                            //Switch visible map if needed.
                            if (Current.Game.CurrentMap != thing.Map)
                                Current.Game.CurrentMap = thing.Map;

                            //Focus camera on thing.
                            Find.CameraDriver.JumpToCurrentMapLoc(thing.Position);

                            //Select the thing.
                            Find.Selector.Select(thing);

                            break;
                        }

                        //Draw info card button
                        Rect infoCardRect = new Rect(rowRect);
                        infoCardRect.width = infoCardRect.height;
                        infoCardRect.x = goToRect.x + goToRect.width;

                        Widgets.InfoCardButton(infoCardRect.x + (infoCardRect.width / 2f) - 12f, infoCardRect.y + (infoCardRect.height / 2f) - 12f, thing);

                        //Draw Tooltip
                        string description = thing.DescriptionDetailed;
                        if(description != null)
                            TooltipHandler.TipRegion(rowRect, description);

                        //Next row
                        rowRect.y += rowHeight;
                    }
                }

                //World locations
                if (searchQuery.worldObjectResultSet.Count > 0)
                {
                    Text.Font = GameFont.Medium;
                    Text.Anchor = TextAnchor.MiddleLeft;

                    //Section label
                    Rect sectionRect = new Rect(rowRect);
                    sectionRect.x += 4f;
                    sectionRect.width -= 4f;

                    Widgets.Label(sectionRect, "RimSearchResultsPlanetLocations".Translate(searchQuery.worldObjectResultSet.Count));
                    Widgets.DrawLineHorizontal(rowRect.x, rowRect.y + rowRect.height, rowRect.width);

                    Text.Font = GameFont.Small;

                    //Next row
                    rowRect.y += rowHeight;

                    foreach (WorldObject worldObject in searchQuery.worldObjectResultSet)
                    {
                        //Draw world object stuff.
                        Widgets.DrawHighlightIfMouseover(rowRect);

                        //Draw portrait
                        Rect portraitRect = new Rect(rowRect);
                        portraitRect.width = portraitRect.height;

                        //Widgets.ThingIcon(portraitRect, worldObject);
                        Color oldColor = GUI.color;

                        GUI.color = worldObject.ExpandingIconColor;
                        /* if(!worldObject.def.texture.NullOrEmpty())
                             Widgets.DrawTextureFitted(portraitRect, ContentFinder<Texture2D>.Get(worldObject.def.texture, true), 0.75f);
                         else
                             Widgets.DrawTextureFitted(portraitRect, ContentFinder<Texture2D>.Get("World/WorldObjects/DefaultFactionBase", true), 0.75f);
                         */
                        GUI.color = oldColor;

                        //Draw name
                        Rect labelRect = new Rect(rowRect);
                        labelRect.width = labelRect.width - portraitRect.width - (rowHeight * 2);
                        labelRect.x += portraitRect.width;

                        Widgets.Label(labelRect, worldObject.LabelCap);

                        //Draw go-to button
                        Rect goToRect = new Rect(rowRect);
                        goToRect.width = goToRect.height;
                        goToRect.x = labelRect.x + labelRect.width;

                        Rect goToRectIcon = goToRect.ContractedBy(goToRect.width / 4f);

                        if (Widgets.ButtonImage(goToRectIcon, RimSearchTextures.ExpandingIcons))
                        {
                            //Close this dialog.
                            //Close(false);

                            //Switch to world view if needed.
                            CameraJumper.TryShowWorld();

                            //Jump to the world object.
                            Find.WorldCameraDriver.JumpTo(worldObject.Tile);

                            break;
                        }

                        //Draw info card button
                        Rect infoCardRect = new Rect(rowRect);
                        infoCardRect.width = infoCardRect.height;
                        infoCardRect.x = goToRect.x + goToRect.width;

                        Widgets.InfoCardButton(infoCardRect.x + (infoCardRect.width / 2f) - 12f, infoCardRect.y + (infoCardRect.height / 2f) - 12f, worldObject);

                        //Draw Tooltip
                        string description = worldObject.GetDescription();
                        if (description != null)
                            TooltipHandler.TipRegion(rowRect, description);

                        //Next row
                        rowRect.y += rowHeight;
                    }
                }

                //Draw select all button
                Rect selectAll = new Rect(rowRect);
                selectAll.width = 150f;
                selectAll.height -= 10f;
                selectAll.x += 300f;
                
                if (Widgets.ButtonText(selectAll, "RimSearchselectAllButton".Translate(searchQuery.worldObjectResultSet.Count), true, true, true))
                {
                    foreach (Pawn pawn in searchQuery.pawnResultSet)
                        Find.Selector.Select(pawn);

                    foreach (Thing thing in searchQuery.thingResultSet)
                        Find.Selector.Select(thing);

                    foreach (WorldObject worldObject in searchQuery.worldObjectResultSet)
                        Find.Selector.Select(worldObject);

                    Close(true);
                }


                Text.Anchor = TextAnchor.UpperLeft;

                Widgets.EndScrollView();

               

            }
        }

        /// <summary>
        /// Constructs the search query from our searchTerm.
        /// </summary>
        public void ConstructSearchQuery()
        {
            SearchQuery newQuery = new SearchQuery(searchTerm);
            searchQuery = newQuery;
        }
    }
}
