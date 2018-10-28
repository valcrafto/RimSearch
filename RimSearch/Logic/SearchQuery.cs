using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimSearch.Logic
{
    /// <summary>
    /// Represents a search query which can be done. It is built up of smaller parts which evaluate searches and return a list.
    /// </summary>
    public class SearchQuery
    {
        /// <summary>
        /// The supplied search term for this query.
        /// </summary>
        public string searchTerm = "";

        /// <summary>
        /// Collection of things that this query found.
        /// </summary>
        public HashSet<Thing> thingResultSet = new HashSet<Thing>();
        /// <summary>
        /// Collection of pawns that this query found.
        /// </summary>
        public HashSet<Pawn> pawnResultSet = new HashSet<Pawn>();
        /// <summary>
        /// Collection of planet locations that this query found.
        /// </summary>
        public HashSet<WorldObject> worldObjectResultSet = new HashSet<WorldObject>();

        /// <summary>
        /// Look in all maps?
        /// </summary>
        public bool filterAllMaps = false;
        /// <summary>
        /// Look in the planet?
        /// </summary>
        public bool filterWorldMap = false;

        /// <summary>
        /// Look for pawns?
        /// </summary>
        public bool filterPawns = false;
        /// <summary>
        /// Look for pawns in player colony?
        /// </summary>
        public bool filterColony = false;
        /// <summary>
        /// Look for haulable items?
        /// </summary>
        public bool filterItems = false;
        /// <summary>
        /// Get everything?
        /// </summary>
        public bool filterAll = false;

        /// <summary>
        /// Predicates to use while we search through all things.
        /// </summary>
        public List<Predicate<Thing>> queryThingPredicates = new List<Predicate<Thing>>();
        /// <summary>
        /// Predicates to use while we search through planet locations.
        /// </summary>
        public List<Predicate<WorldObject>> queryWorldObjectPredicates = new List<Predicate<WorldObject>>();

        /// <summary>
        /// Debug purposes; The label that was parsed.
        /// </summary>
        public string debugLabel = "";

        /// <summary>
        /// Special flags to look out for.
        /// </summary>
        public static string flagChars = "!-#.*";

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public SearchQuery()
        {

        }

        /// <summary>
        /// Constructor with supplied search term. Immediatly will try to parse it and set all appropiate flags and predicates..
        /// </summary>
        /// <param name="searchTerm">Supplied search term.</param>
        public SearchQuery(string searchTerm)
        {
            this.searchTerm = searchTerm;
            ParseSearchTerm();
        }

        /// <summary>
        /// Parse searchTerm and set all appropiate flags and predicates.
        /// </summary>
        public void ParseSearchTerm()
        {
            StringBuilder labelSearch = new StringBuilder();

            //Parser state machine
            bool parsingFlags = true;
            bool parsingLabel = false;
            int currentChar = 0;

            //Go through all characters in our search term.
            foreach (char c in searchTerm)
            {
                //Are we parsing flags?
                if (parsingFlags)
                {
                    //Stop looking for flags if it encounters a non flag char.
                    if (!flagChars.Contains(c))
                    {
                        parsingFlags = false;
                        parsingLabel = true;
                    }
                    else
                        //Identify flags and set them in the query.
                        switch (c)
                        {
                            case '!': //Filter all maps.
                                filterAllMaps = true;
                                break;

                            // Option disabled as it doesn't work
                            //case '<': //Filter planet locations.
                            //    filterWorldMap = true;
                            //    break;

                            case '-': //Filter map pawns.
                                filterPawns = true;
                                break;

                            case '#': //Filter map pawns by player colony.
                                filterColony = true;
                                break;

                            case '.': //Filter haulable items.
                                filterItems = true;
                                break;

                            case '*': //Get all.
                                filterAll = true;
                                break;

                            default:
                                break;
                        }
                }

                //Add to our label to search, but not first space if this condition is fulfilled.
                if(parsingLabel)
                {
                    labelSearch.Append(c);
                }

                currentChar++;
            }

            //Construct predicates.
            string searchLabel = labelSearch.ToString();

            //Debug
            debugLabel = searchLabel;

            //Predicate which always return true
            if(filterAll)
            {
                //Do not show hidden things
                queryThingPredicates.Add(thing => !thing.Position.Fogged(thing.MapHeld)); //Ensure no cheaty behavior :P

                queryWorldObjectPredicates.Add(worldObject => true);
            }
            else
            {
                //Do not show hidden things
                queryThingPredicates.Add(thing => !thing.Position.Fogged(thing.MapHeld)); //Ensure no cheaty behavior :P

                //Predicate by label
                if (searchLabel != null && searchLabel.Length > 0)
                {
                    queryThingPredicates.Add(delegate (Thing thing)
                    {
                        Pawn pawn = thing as Pawn;

                        if(pawn != null)
                        {
                            if (pawn.KindLabel.ToLower().Contains(searchLabel.ToLower()))
                                return true;

                            if (pawn.kindDef.label.ToLower().Contains(searchLabel.ToLower()))
                                return true;
                        }

                        return thing.Label.ToLower().Contains(searchLabel.ToLower());
                    });

                    queryWorldObjectPredicates.Add(worldObject => worldObject.Label.Contains(searchLabel));
                }
            }

            if (filterColony)
                queryThingPredicates.Add(delegate (Thing thing)
                {
                    Pawn pawn = thing as Pawn;

                    if (pawn != null)
                        return pawn.Faction == Faction.OfPlayer;

                    return true;
                });

            queryThingPredicates.Add(delegate (Thing thing)
            {
                Pawn pawn = thing as Pawn;

                if (pawn != null)
                    return !pawn.InContainerEnclosed;

                return true;
            });
        }

        /// <summary>
        /// Executes this query with current criterias on the game.
        /// </summary>
        public void Execute()
        {
            //Lookup pawns and things.
            if (filterAllMaps)
            {
                //Look in all loaded maps.
                foreach (Map map in Current.Game.Maps)
                {
                    ExecuteMap(map);
                }
            }
            else
            {
                //Look in currently visible map. (If any)
                if(Current.Game.CurrentMap != null)
                    ExecuteMap(Current.Game.CurrentMap);
            }

            //Lookup world objects.
            if (filterWorldMap)
            {
                //Look in world map.
                ExecuteWorldMap(Find.World);
            }
        }

        /// <summary>
        /// Executes query on the supplied map.
        /// </summary>
        /// <param name="map">Supplied map to look in.</param>
        public void ExecuteMap(Map map)
        {
            //Pawns
            if(filterPawns)
            {
                foreach(Pawn pawn in map.mapPawns.AllPawns)
                {
                    ExecutePawn(pawn);
                }
            }

            //Items
            if(filterItems)
            {
                foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
                {
                    ExecuteThing(thing);
                }
            }
        }

        /// <summary>
        /// Executes query on the supplied pawn.
        /// </summary>
        /// <param name="pawn">Pawn to look at.</param>
        public void ExecutePawn(Pawn pawn)
        {
            int fulfilledPredicates = 0;
            //bool predicatesFulfilled = false;

            foreach (Predicate<Thing> predicate in queryThingPredicates)
            {
                bool predicateState = predicate(pawn);
                if (predicateState)
                    fulfilledPredicates++;

                //predicatesFulfilled = predicateState;
                if (!predicateState)
                    break;
            }

            if (fulfilledPredicates >= queryThingPredicates.Count)
                pawnResultSet.Add(pawn);
        }

        /// <summary>
        /// Execute query on supplied thing.
        /// </summary>
        /// <param name="thing">Thing to look at.</param>
        public void ExecuteThing(Thing thing)
        {
            int fulfilledPredicates = 0;
            //bool predicatesFulfilled = false;

            foreach (Predicate<Thing> predicate in queryThingPredicates)
            {
                bool predicateState = predicate(thing);
                if (predicateState)
                    fulfilledPredicates++;

                //predicatesFulfilled = predicateState;
                if (!predicateState)
                    break;
            }

            if (fulfilledPredicates >= queryThingPredicates.Count)
                thingResultSet.Add(thing);
        }

        /// <summary>
        /// Execute query on the supplied planet.
        /// </summary>
        /// <param name="world">Planet to look in.</param>
        public void ExecuteWorldMap(World world)
        {
            foreach(WorldObject worldObject in world.worldObjects.AllWorldObjects)
            {
                ExecuteWorldObject(worldObject);
            }
        }

        /// <summary>
        /// Execute query on the supplied planet location.
        /// </summary>
        /// <param name="worldObject">Planet location to look at.</param>
        public void ExecuteWorldObject(WorldObject worldObject)
        {
            int fulfilledPredicates = 0;
            //bool predicatesFulfilled = false;

            foreach (Predicate<WorldObject> predicate in queryWorldObjectPredicates)
            {
                bool predicateState = predicate(worldObject);
                if (predicateState)
                    fulfilledPredicates++;

                //predicatesFulfilled = predicateState;
                if (!predicateState)
                    break;
            }

            if (fulfilledPredicates >= queryWorldObjectPredicates.Count)
                worldObjectResultSet.Add(worldObject);
        }
    }
}
