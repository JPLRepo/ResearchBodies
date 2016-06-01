using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RSTUtils;

namespace ResearchBodies
{
    public class RBGameSettings
    {
        private const string configNodeName = "RBGameSettings";

        public bool Enabled;
        public List<CelestialBody> BodyList { get; set; }
        public Dictionary<CelestialBody, CelestialBodyInfo> RBCelestialBodyInfo;
        public int Difficulty;
        public float ResearchCost;
        public float ProgressResearchCost;
        public float ScienceReward;



        public RBGameSettings()
        {
            Enabled = true;
            Difficulty = 0;
            ResearchCost = 10;
            ProgressResearchCost = 5;
            ScienceReward = 5;
            BodyList = new List<CelestialBody>();
            RBCelestialBodyInfo = new Dictionary<CelestialBody, CelestialBodyInfo>();
        }

        public void Load(ConfigNode node)
        {
            BodyList.Clear();
            BodyList = FlightGlobals.Bodies.ToList(); 
            if (Utilities.IsTSTInstalled && TSTWrapper.APITSTReady) //If TST is installed add the TST Galaxies to the list.
            {
                BodyList = BodyList.Concat(TSTWrapper.actualTSTAPI.CBGalaxies).ToList();
            }
            RBCelestialBodyInfo.Clear();
            if (!File.Exists("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg"))
            {
                if (node.HasNode(configNodeName))
                {
                    ConfigNode RBsettingsNode = node.GetNode(configNodeName);

                    node.TryGetValue("Enabled", ref Enabled);
                    node.TryGetValue("Difficulty", ref Difficulty);
                    node.TryGetValue("ResearchCost", ref ResearchCost);
                    node.TryGetValue("ProgressResearchCost", ref ProgressResearchCost);
                    node.TryGetValue("ScienceReward", ref ScienceReward);
                    
                    var bodyNodes = RBsettingsNode.GetNodes(CelestialBodyInfo.ConfigNodeName);
                    foreach (ConfigNode bodyNode in bodyNodes)
                    {
                        if (bodyNode.HasValue("body"))
                        {
                            
                            CelestialBodyInfo bodyInfo = CelestialBodyInfo.Load(bodyNode);
                            CelestialBody CB = FlightGlobals.Bodies.FirstOrDefault(a => a.GetName() == bodyInfo.body);
                            if (CB != null)
                            {
                                RBCelestialBodyInfo[CB] = bodyInfo;
                            }
                        }
                    }
                }
                else  //No config node, so create default.
                {
                    foreach (CelestialBody body in BodyList)
                    {
                        CelestialBodyInfo bodyInfo = new CelestialBodyInfo(body.GetName());
                        RBCelestialBodyInfo[body] = bodyInfo;
                    }
                }
                //Now we check we have a Dictionary entry for all the bodies in the BodyList.
                foreach (CelestialBody body in BodyList)
                {
                    
                }
            }
            else //OLD Save file found, convert to persistent.sfs confignode and delete file.
            {
                RSTLogWriter.Log("Converting Old Save file to new persistent.sfs config node - Loading old save file");
                ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                ResearchBodies.toolbar = int.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("difficulty") ?? "0"); // DEPRECATED!

                ResearchBodies.ResearchCost = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ResearchCost") ?? "10");
                ResearchBodies.ProgressResearchCost = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ProgressResearchCost") ?? "5");
                ResearchBodies.ScienceReward = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ResearchCost") ?? "5");

                BodyList = FlightGlobals.Bodies.ToList(); 
                if (Utilities.IsModInstalled("TarsierSpaceTech") && TSTWrapper.APITSTReady)
                {
                    BodyList = BodyList.Concat(TSTWrapper.actualTSTAPI.CBGalaxies).ToList();
                }
                foreach (CelestialBody cb in BodyList)
                {
                    bool fileContainsCB = false;
                    foreach (ConfigNode oldnode in mainnode.GetNode("RESEARCHBODIES").nodes)
                    {
                        if (cb.GetName().Contains(oldnode.GetValue("body")))
                        {
                            if (bool.Parse(oldnode.GetValue("ignore")))
                            {
                                ResearchBodies.TrackedBodies[cb] = true;
                                ResearchBodies.ResearchState[cb] = 100;
                            }
                            else
                            {
                                ResearchBodies.TrackedBodies[cb] = bool.Parse(oldnode.GetValue("isResearched"));
                                if (oldnode.HasValue("researchState"))
                                {
                                    ResearchBodies.ResearchState[cb] = int.Parse(oldnode.GetValue("researchState"));
                                }
                                else
                                {
                                    ResearchBodies.ResearchState[cb] = 0;
                                }
                            }
                            fileContainsCB = true;
                        }
                    }
                    if (!fileContainsCB)
                    {
                        ResearchBodies.TrackedBodies[cb] = false;
                        ResearchBodies.ResearchState[cb] = 0;
                    }
                }
                File.Delete("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                RSTLogWriter.Log("Converted Old Save file to new persistent.sfs config node - Loading/Conversion complete. Old save file deleted");
            }
            RSTUtils.Utilities.Log_Debug("RBGameSettings Loading Complete");
        }

        public void Save(ConfigNode node)
        {
            var settingsNode = node.HasNode(configNodeName) ? node.GetNode(configNodeName) : node.AddNode(configNodeName);

            settingsNode.AddValue("Enabled", Enabled);

            foreach (var entry in KnownVessels)
            {
                ConfigNode vesselNode = entry.Value.Save(settingsNode);
                RSTUtils.Utilities.Log_Debug("RBGameSettings Saving Guid = {0}", entry.Key.ToString());
                vesselNode.AddValue("Guid", entry.Key);
            }
            RSTUtils.Utilities.Log_Debug("RBGameSettings Saving Complete");
        }
    }
    
}
