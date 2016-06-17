using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RSTUtils;
using UnityEngine;

namespace ResearchBodies
{
    public class RBGameSettings
    {
        private const string configNodeName = "RBGameSettings";
        
        public bool Enabled;
        //public List<CelestialBody> BodyList { get; set; }
        public Dictionary<string, CelestialBodyInfo> RBCelestialBodyInfo;
        public int Difficulty;
        public int ResearchCost;
        public int ProgressResearchCost;
        public int ScienceReward;
        public bool UseAppLauncher;
        public bool DebugLogging;
        public int chances;
        public int[] StartResearchCosts, ProgressResearchCosts, ScienceRewards;
        public bool enableInSandbox; 
        public bool allowTSlevel1;
        

        public RBGameSettings()
        {
            Enabled = true;
            Difficulty = 0;
            ResearchCost = 10;
            ProgressResearchCost = 5;
            ScienceReward = 5;
            //BodyList = new List<CelestialBody>();
            RBCelestialBodyInfo = new Dictionary<string, CelestialBodyInfo>();
            UseAppLauncher = true;
            DebugLogging = false;
            chances = 3;
            enableInSandbox = false;
            allowTSlevel1 = false;
        }

        public void Load(ConfigNode node)
        {
            //BodyList.Clear();
            //BodyList = FlightGlobals.Bodies.ToList(); 
            //if (Utilities.IsTSTInstalled && TSTWrapper.APITSTReady) //If TST is installed add the TST Galaxies to the list.
            //{
            //    BodyList = BodyList.Concat(TSTWrapper.actualTSTAPI.CBGalaxies).ToList();
            //}
            RBCelestialBodyInfo.Clear();
            //No old save file exists? so we load from SFS config nodes?
            if (!File.Exists("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg"))
            {
                if (node.HasNode(configNodeName)) //Load SFS config nodes
                {
                    ConfigNode RBsettingsNode = node.GetNode(configNodeName);

                    node.TryGetValue("Enabled", ref Enabled);
                    node.TryGetValue("Difficulty", ref Difficulty);
                    node.TryGetValue("ResearchCost", ref ResearchCost);
                    node.TryGetValue("ProgressResearchCost", ref ProgressResearchCost);
                    node.TryGetValue("ScienceReward", ref ScienceReward);
                    node.TryGetValue("UseAppLauncher", ref UseAppLauncher);
                    node.TryGetValue("DebugLogging", ref DebugLogging);
                    node.TryGetValue("chances", ref chances);
                    node.TryGetValue("enableInSandbox", ref enableInSandbox);
                    node.TryGetValue("allowTSlevel1", ref allowTSlevel1);

                    var bodyNodes = RBsettingsNode.GetNodes(CelestialBodyInfo.ConfigNodeName);
                    foreach (ConfigNode bodyNode in bodyNodes)
                    {
                        if (bodyNode.HasValue("body"))
                        {
                            string id = bodyNode.GetValue("body");
                            CelestialBodyInfo bodyInfo = CelestialBodyInfo.Load(bodyNode);
                            CelestialBody CB = FlightGlobals.Bodies.FirstOrDefault(a => a.GetName() == id);
                            if (CB != null)
                            {
                                RBCelestialBodyInfo[id] = bodyInfo;
                            }
                        }
                    }
                }
                else  //No config node, so Must be NEW Game!
                {
                    Enabled = true;
                    chances = Database.chances;
                    enableInSandbox = Database.enableInSandbox;
                    allowTSlevel1 = Database.allowTSlevel1;
                    ResearchBodiesController.instance.showStartUI = true;
                    
                    foreach (CelestialBody body in Database.BodyList)
                    {
                        CelestialBodyInfo bodyInfo = new CelestialBodyInfo(body.GetName());
                        RBCelestialBodyInfo[body.GetName()] = bodyInfo;
                    }
                }
                
            }
            else //OLD Save file found, convert to persistent.sfs confignode and delete file.
            {
                RSTLogWriter.Log("Converting Old Save file to new persistent.sfs config node - Loading old save file");
                ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                ResearchBodiesController.toolbar = int.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("difficulty") ?? "0"); // DEPRECATED!

                ResearchBodiesController.ResearchCost = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ResearchCost") ?? "10");
                ResearchBodiesController.ProgressResearchCost = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ProgressResearchCost") ?? "5");
                ResearchBodiesController.ScienceReward = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ResearchCost") ?? "5");

                //BodyList = FlightGlobals.Bodies.ToList(); 
                //if (Utilities.IsModInstalled("TarsierSpaceTech") && TSTWrapper.APITSTReady)
                //{
                //    BodyList = BodyList.Concat(TSTWrapper.actualTSTAPI.CBGalaxies).ToList();
                //}
                foreach (CelestialBody cb in Database.BodyList)
                {
                    bool fileContainsCB = false;
                    foreach (ConfigNode oldnode in mainnode.GetNode("RESEARCHBODIES").nodes)
                    {
                        if (cb.GetName().Contains(oldnode.GetValue("body")))
                        {
                            if (bool.Parse(oldnode.GetValue("ignore")))
                            {
                                ResearchBodiesController.TrackedBodies[cb] = true;
                                ResearchBodiesController.ResearchState[cb] = 100;
                            }
                            else
                            {
                                ResearchBodiesController.TrackedBodies[cb] = bool.Parse(oldnode.GetValue("isResearched"));
                                if (oldnode.HasValue("researchState"))
                                {
                                    ResearchBodiesController.ResearchState[cb] = int.Parse(oldnode.GetValue("researchState"));
                                }
                                else
                                {
                                    ResearchBodiesController.ResearchState[cb] = 0;
                                }
                            }
                            fileContainsCB = true;
                        }
                    }
                    if (!fileContainsCB)
                    {
                        ResearchBodiesController.TrackedBodies[cb] = false;
                        ResearchBodiesController.ResearchState[cb] = 0;
                    }
                }
                File.Delete("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                RSTLogWriter.Log("Converted Old Save file to new persistent.sfs config node - Loading/Conversion complete. Old save file deleted");
            }

            //Now we check we have a Dictionary entry for all the bodies in the BodyList.
            foreach (CelestialBody body in Database.BodyList)
            {
                if (!RBCelestialBodyInfo.ContainsKey(body.GetName()))
                {
                    CelestialBodyInfo bodyInfo = new CelestialBodyInfo(body.GetName());
                    RBCelestialBodyInfo[body.GetName()] = bodyInfo;
                }
            }
            //Now we load locale strings for the OnDiscovery text
            if (Locales.currentLocale.LocaleId != "en")
            {
                foreach (CelestialBody body in Database.BodyList)
                {
                    if (Locales.currentLocale.Values.ContainsKey("discovery_" + body.GetName()) && RBCelestialBodyInfo.ContainsKey(body.GetName()))
                    {
                        RBCelestialBodyInfo[body.GetName()].discoveryMessage = Locales.currentLocale.Values["discovery_" + body.GetName()];
                        //DiscoveryMessage[body.GetName()] = Locales.currentLocale.Values["discovery_" + body.GetName()];
                    }
                }
            }

            RSTUtils.Utilities.Log_Debug("RBGameSettings Loading Complete");
        }

        public void Save(ConfigNode node)
        {
            var settingsNode = node.HasNode(configNodeName) ? node.GetNode(configNodeName) : node.AddNode(configNodeName);

            settingsNode.AddValue("Enabled", Enabled);
            settingsNode.AddValue("Difficulty", Difficulty);
            settingsNode.AddValue("ResearchCost", ResearchCost);
            settingsNode.AddValue("ProgressResearchCost", ProgressResearchCost);
            settingsNode.AddValue("ScienceReward", ScienceReward);
            settingsNode.AddValue("UseAppLauncher", UseAppLauncher);
            settingsNode.AddValue("DebugLogging", DebugLogging);
            settingsNode.AddValue("chances", chances);
            settingsNode.AddValue("enableInSandbox", enableInSandbox);
            settingsNode.AddValue("allowTSlevel1", allowTSlevel1);

            foreach (var entry in RBCelestialBodyInfo)
            {
                ConfigNode vesselNode = entry.Value.Save(settingsNode);
                RSTUtils.Utilities.Log_Debug("RBGameSettings Saving Entry = {0}", entry.Key);
                vesselNode.AddValue("body", entry.Key);
            }
            RSTUtils.Utilities.Log_Debug("RBGameSettings Saving Complete");
        }
    }
    
}
