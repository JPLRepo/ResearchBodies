/*
 * Database.cs
 * (C) Copyright 2016, Jamie Leighton 
 * License Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International
 * http://creativecommons.org/licenses/by-nc-sa/4.0/
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  ResearchBodies is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using RSTUtils;
using UnityEngine;

namespace ResearchBodies
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Database : MonoBehaviour
    {
        public static Database instance;
        //This is the MAIN Dictionary of CelestialBody Information that ResearchBodies uses.
        public Dictionary<CelestialBody, CelestialBodyInfo> CelestialBodies = new Dictionary<CelestialBody, CelestialBodyInfo>();
        //This is a list of Nothing to See here strings loaded from NOTHING node in database.cfg
        public List<string> NothingHere = new List<string>();

        public int chances;
        public int[] StartResearchCosts, ProgressResearchCosts, ScienceRewards;
        public bool enableInSandbox, allowTSlevel1 = false;
        internal bool UseAppLauncher = true;
        internal string[] difficultyStrings;
        //This is a deprecated dictionary that stores Priority #'s against the CelestialBodies. Loaded from PRIORITIES node in database.cfg
        public Dictionary<CelestialBody, int> Priority = new Dictionary<CelestialBody, int>();

        /// <summary>
        /// Tarsier Space Tech Interface fields
        /// </summary>
        internal bool isTSTInstalled = false;
        public List<CelestialBody> BodyList = new List<CelestialBody>();
        

        //This is only called by the Startup Menu GUI to show ignored bodies based on the level passed in. 
        public string GetIgnoredBodies(Level l) 
        {
            string _bodies = Locales.currentLocale.Values["start_availableBodies"] + " : ";
            foreach (CelestialBody body in BodyList.Where(b => CelestialBodies[b].IgnoreData.GetLevel(l) && (b.Radius > 100 || b.name.Contains("TSTGalaxies"))))
            {
                _bodies += body.GetName() + ", ";
            }
            return _bodies;
        }

        public void Awake()
        {
            if (instance != null)
            {
                RSTLogWriter.Log("Singleton instance of Databse already exists. Destroying this one");
                Destroy(this);
            }
            instance = this;
            DontDestroyOnLoad(this);
            difficultyStrings = new string[] { Locales.currentLocale.Values["start_easy"], Locales.currentLocale.Values["start_normal"], Locales.currentLocale.Values["start_medium"], Locales.currentLocale.Values["start_hard"] };


}

void Start()
        {
            isTSTInstalled = Utilities.IsTSTInstalled;
            if (isTSTInstalled)  //If TST assembly is present, initialise TST wrapper.
            {
                if (!TSTWrapper.InitTSTWrapper())
                {
                    isTSTInstalled = false; //If the initialise of wrapper failed set bool to false, we won't be interfacing to TST today.
                }
            }

            Textures.LoadIconAssets();

            //Get a list of CBs
            BodyList = FlightGlobals.Bodies.ToList(); 
            if (isTSTInstalled && TSTWrapper.APITSTReady) //If TST is installed add the TST Galaxies to the list.
            {
                BodyList = BodyList.Concat(TSTWrapper.actualTSTAPI.CBGalaxies).ToList();
            }

            //Process Kopernicus Barycenter's.
            foreach (CelestialBody body in BodyList)
            {
                CelestialBodyInfo bodyinfo = new CelestialBodyInfo(body.GetName());
                if (body.Radius < 100 && !body.name.Contains("TSTGalaxies"))  //This body is a barycenter
                {
                    bodyinfo.KOPbarycenter = true;
                }
                else
                {
                    if (body.referenceBody.Radius < 100) // This Bodies Reference Body has a Radius < 100m. IE: It's Part of a Barycenter.
                    {
                        bodyinfo.KOPrelbarycenterBody = body.referenceBody; //Yeah so what... well we need it for pass 2 below.
                    }
                }
                CelestialBodies.Add(body, bodyinfo);
            }
            //Now we look back through any CBs that were related to a barycenter body.
            foreach (var CB in CelestialBodies.Where(a => a.Value.KOPrelbarycenterBody != null))
            {
                //So does this body have any orbitingBodies?
                //If it does we need to somehow find and link any related Orbit Body.
                foreach (CelestialBody orbitingBody in CB.Key.orbitingBodies)
                {
                    CelestialBody findOrbitBody =
                        FlightGlobals.Bodies.FirstOrDefault(a => a.name.Contains(CB.Key.name) && a.name.Contains(orbitingBody.name) && a.name.Contains("Orbit"));
                    //so if we found the related Orbit body we store it into the CelestialBodies dictionary.
                    if (findOrbitBody != null)
                    {
                        CelestialBodies[orbitingBody].KOPrelbarycenterBody = findOrbitBody;
                    }
                }
            }

            //Load the database.cfg file.
            //===========================
            ConfigNode cfg = ConfigNode.Load(Locales.PathDatabasePath);
            string[] sep = new string[] { " " };

            //Get Costs
            string[] _startResearchCosts;
            _startResearchCosts = cfg.GetNode("RESEARCHBODIES").GetValue("StartResearchCosts").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            StartResearchCosts = new int[] { int.Parse(_startResearchCosts[0]), int.Parse(_startResearchCosts[1]), int.Parse(_startResearchCosts[2]), int.Parse(_startResearchCosts[3]) };

            string[] _progressResearchCosts;
            _progressResearchCosts = cfg.GetNode("RESEARCHBODIES").GetValue("ProgressResearchCosts").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            ProgressResearchCosts = new int[] { int.Parse(_progressResearchCosts[0]), int.Parse(_progressResearchCosts[1]), int.Parse(_progressResearchCosts[2]), int.Parse(_progressResearchCosts[3]) };

            string[] _scienceRewards;
            _scienceRewards = cfg.GetNode("RESEARCHBODIES").GetValue("ScienceRewards").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            ScienceRewards = new int[] { int.Parse(_scienceRewards[0]), int.Parse(_scienceRewards[1]), int.Parse(_scienceRewards[2]), int.Parse(_scienceRewards[3]) };
            
            RSTLogWriter.Log_Debug("Loading Priority database");
            foreach (CelestialBody body in BodyList)
            {
                //Load the priorities - DEPRECATED
                string name = body.GetName();
                foreach (ConfigNode.Value value in cfg.GetNode("RESEARCHBODIES").GetNode("PRIORITIES").values)
                {
                    if (name == value.name)
                    {
                        Priority[body] = int.Parse(value.value);
                        RSTLogWriter.Log_Debug("Priority for body {0} set to {1}.", name , value.value);
                    }
                }
                //Load the ondiscovery values - English only, which then get over-written in Locale.cs
                foreach (ConfigNode.Value value in cfg.GetNode("RESEARCHBODIES").GetNode("ONDISCOVERY").values)
                {
                    if (value.name == name)
                        CelestialBodies[body].discoveryMessage = value.value;
                        //DiscoveryMessage[value.name] = value.value;
                }
            }

            //Load the IgnoreData dictionary.
            RSTLogWriter.Log_Debug("Loading ignore body list from database");
            foreach (ConfigNode.Value value in cfg.GetNode("RESEARCHBODIES").GetNode("IGNORELEVELS").values)
            {
                foreach (CelestialBody body in BodyList)
                {
                    if (body.GetName() == value.name)
                    {
                        string[] args;
                        args = value.value.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        BodyIgnoreData ignoredata = new BodyIgnoreData(bool.Parse(args[0]), bool.Parse(args[1]), bool.Parse(args[2]), bool.Parse(args[3]));
                        CelestialBodies[body].IgnoreData = ignoredata;
                        RSTLogWriter.Log_Debug("Body Ignore Data for {0} : {1}" , body.GetName() , CelestialBodies[body].IgnoreData);
                    }
                }
            }

            LoadModDatabaseNodes();
            
            //Load the NothingHere dictionary from the database config file.
            foreach (ConfigNode.Value value in cfg.GetNode("RESEARCHBODIES").GetNode("NOTHING").values)
            {
                if (value.name == "text")
                    NothingHere.Add(value.value);
            }
            
            //So this is deprecated? Checks all CBs are in the Priority dictionary. Any that aren't are added with priority set to 3.
            foreach (CelestialBody cb in BodyList)
            {
                if (!Priority.Keys.Contains(cb) && !CelestialBodies[cb].ignore)
                {
                    Priority[cb] = 3;
                    RSTLogWriter.Log("Config not found for {0}, priority set to 3." , cb.GetName());
                }
            }

            chances = int.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("chances"));
            RSTLogWriter.Log_Debug("Chances to get a body is set to {0}" , chances);
            enableInSandbox = bool.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("enableInSandbox"));
            allowTSlevel1 = bool.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("allowTrackingStationLvl1"));
            if (cfg.GetNode("RESEARCHBODIES").HasValue("useAppLauncher"))
            {
                UseAppLauncher = bool.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("useAppLauncher"));
            }
            RSTLogWriter.Log_Debug("Loaded gamemode-related information : enable mod in sandbox = {0}, allow tracking with Tracking station lvl 1 = {1}" , enableInSandbox , allowTSlevel1);


            // Load locales for OnDiscovery - Locales are loaded Immediately gamescene. Before this is loaded in MainMenu.
            if (Locales.currentLocale.LocaleId != "en")
            {
                foreach (CelestialBody body in BodyList)
                {
                    if (Locales.currentLocale.Values.ContainsKey("discovery_" + body.GetName()) && CelestialBodies.ContainsKey(body))
                    {
                        CelestialBodies[body].discoveryMessage = Locales.currentLocale.Values["discovery_" + body.GetName()];
                    }
                }
            }
            
            
        }
        
        private void LoadModDatabaseNodes()
        {
            string[] sep = new string[] { " " };
            //Load all Mod supplied database config files.
            RSTLogWriter.Log_Debug("Loading mods databases");
            ConfigNode[] modNodes = GameDatabase.Instance.GetConfigNodes("RESEARCHBODIES");
            foreach (ConfigNode node in modNodes)
            {
                if (node.GetValue("loadAs") == "mod")
                {
                    if (node.HasValue("name"))
                        RSTLogWriter.Log_Debug("Loading {0} configuration", node.GetValue("name"));
                    if (node.HasNode("PRIORITIES"))
                    {
                        foreach (CelestialBody body in BodyList)
                        {
                            string name = body.GetName();
                            foreach (ConfigNode.Value value in node.GetNode("PRIORITIES").values)
                            {
                                if (name == value.name)
                                {
                                    Priority[body] = int.Parse(value.value);
                                    RSTLogWriter.Log_Debug("Priority for body {0} set to {1}", name, value.value);
                                }
                                else if ("*" + name == value.name)
                                {
                                    Priority[body] = int.Parse(value.value);
                                    RSTLogWriter.Log_Debug("Priority for body {0} set to {1}, overriding old value.", name, value.value);
                                }
                            }
                        }
                    }
                    if (node.HasNode("ONDISCOVERY"))
                    {
                        foreach (CelestialBody body in BodyList)
                        {
                            foreach (ConfigNode.Value value in node.GetNode("ONDISCOVERY").values)
                            {
                                if (body.GetName() == "Eeloo")
                                {
                                    // Stop here
                                    RSTLogWriter.Log_Debug("Stop here");
                                }
                                if (value.name == body.GetName() || value.name == "*" + body.GetName())
                                    CelestialBodies[body].discoveryMessage = value.value;
                            }
                        }
                    }
                    if (node.HasNode("IGNORE"))
                    {
                        foreach (ConfigNode.Value value in node.GetNode("IGNORE").values)
                        {
                            if (value.name == "body")
                            {
                                foreach (CelestialBody cb in BodyList)
                                {
                                    if (value.value == cb.GetName())
                                    {
                                        CelestialBodies[cb].IgnoreData.setBodyIgnoreData(false, false, false, false);
                                        RSTLogWriter.Log_Debug("Added {0}  to the ignore list (pre-1.5 method !)", cb.GetName());
                                    }
                                }
                            }
                            else if (value.name == "!body")
                            {
                                foreach (CelestialBody cb in BodyList)
                                {
                                    if (value.value == cb.GetName() && CelestialBodies.ContainsKey(cb))
                                    {
                                        CelestialBodies[cb].IgnoreData.setBodyIgnoreData(true, true, true, true);
                                        RSTLogWriter.Log_Debug("Removed {0}  from the ignore list (pre-1.5 method!)", cb.GetName());
                                    }
                                }
                            }
                        }
                    }
                    if (node.HasNode("IGNORELEVELS"))
                    {
                        foreach (ConfigNode.Value value in node.GetNode("IGNORELEVELS").values)
                        {
                            foreach (CelestialBody body in BodyList)
                            {
                                if (body.GetName() == value.name)
                                {
                                    string[] args;
                                    args = value.value.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                                    CelestialBodies[body].IgnoreData.setBodyIgnoreData(bool.Parse(args[0]), bool.Parse(args[1]), bool.Parse(args[2]), bool.Parse(args[3]));

                                    RSTLogWriter.Log_Debug("Body Ignore Data for {0} : {1}", body.GetName(), CelestialBodies[body].IgnoreData.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
