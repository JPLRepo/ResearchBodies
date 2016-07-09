/*
 * RBGameSettings.cs
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
using System.IO;
using System.Linq;
using RSTUtils;

namespace ResearchBodies
{
    public class RBGameSettings
    {
        private const string configNodeName = "RBGameSettings";
        
        public bool Enabled;
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
            UseAppLauncher = true;
            DebugLogging = false;
            chances = 3;
            enableInSandbox = false;
            allowTSlevel1 = false;
        }

        public void Load(ConfigNode node)
        {
            //Does ano old save file exist? If not then we load from persistent.SFS config nodes.
            if (!File.Exists("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg"))
            {
                if (node.HasNode(configNodeName)) //Load SFS config nodes
                {
                    ConfigNode RBsettingsNode = node.GetNode(configNodeName);

                    RBsettingsNode.TryGetValue("Enabled", ref Enabled);
                    RBsettingsNode.TryGetValue("Difficulty", ref Difficulty);
                    RBsettingsNode.TryGetValue("ResearchCost", ref ResearchCost);
                    RBsettingsNode.TryGetValue("ProgressResearchCost", ref ProgressResearchCost);
                    RBsettingsNode.TryGetValue("ScienceReward", ref ScienceReward);
                    RBsettingsNode.TryGetValue("UseAppLauncher", ref UseAppLauncher);
                    RBsettingsNode.TryGetValue("DebugLogging", ref DebugLogging);
                    RBsettingsNode.TryGetValue("chances", ref chances);
                    RBsettingsNode.TryGetValue("enableInSandbox", ref enableInSandbox);
                    RBsettingsNode.TryGetValue("allowTSlevel1", ref allowTSlevel1);

                    var bodyNodes = RBsettingsNode.GetNodes(CelestialBodyInfo.ConfigNodeName);
                    foreach (ConfigNode bodyNode in bodyNodes)
                    {
                        if (bodyNode.HasValue("body"))
                        {
                            CelestialBodyInfo bodyInfo = CelestialBodyInfo.Load(bodyNode);
                            CelestialBody CB = FlightGlobals.Bodies.FirstOrDefault(a => a.GetName() == bodyInfo.body);
                            if (CB != null)
                            {
                                Database.instance.CelestialBodies[CB].isResearched = bodyInfo.isResearched;
                                Database.instance.CelestialBodies[CB].researchState = bodyInfo.researchState;
                                Database.instance.CelestialBodies[CB].ignore = bodyInfo.ignore;
                            }
                        }
                    }
                }
                else  //No config node, so Must be NEW Game!
                {
                    Enabled = true;
                    chances = Database.instance.chances;
                    enableInSandbox = Database.instance.enableInSandbox;
                    allowTSlevel1 = Database.instance.allowTSlevel1;
                    ResearchBodiesController.instance.showStartUI = true;
                    
                    foreach (CelestialBody body in Database.instance.BodyList)
                    {
                        CelestialBodyInfo bodyInfo = new CelestialBodyInfo(body.GetName());
                        Database.instance.CelestialBodies[body].isResearched = bodyInfo.isResearched;
                        Database.instance.CelestialBodies[body].researchState = bodyInfo.researchState;
                        Database.instance.CelestialBodies[body].ignore = bodyInfo.ignore;
                    }
                }
                
            }
            else //OLD Save file found, convert to persistent.sfs confignode and delete file.
            {
                RSTLogWriter.Log("Converting Old Save file to new persistent.sfs config node - Loading old save file");
                ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                Difficulty = int.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("difficulty") ?? "0"); // DEPRECATED!

                ResearchCost = int.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ResearchCost") ?? "10");
                ProgressResearchCost = int.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ProgressResearchCost") ?? "5");
                ScienceReward = int.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ResearchCost") ?? "5");
                
                foreach (CelestialBody cb in Database.instance.BodyList)
                {
                    bool fileContainsCB = false;
                    foreach (ConfigNode oldnode in mainnode.GetNode("RESEARCHBODIES").nodes)
                    {
                        if (cb.GetName().Contains(oldnode.GetValue("body")))
                        {
                            if (bool.Parse(oldnode.GetValue("ignore")))
                            {
                                Database.instance.CelestialBodies[cb].isResearched = true;
                                Database.instance.CelestialBodies[cb].researchState = 100;
                            }
                            else
                            {
                                Database.instance.CelestialBodies[cb].isResearched = bool.Parse(oldnode.GetValue("isResearched"));
                                if (oldnode.HasValue("researchState"))
                                {
                                    Database.instance.CelestialBodies[cb].researchState = int.Parse(oldnode.GetValue("researchState"));
                                }
                                else
                                {
                                    Database.instance.CelestialBodies[cb].researchState = 0;
                                }
                            }
                            fileContainsCB = true;
                        }
                    }
                    if (!fileContainsCB)
                    {
                        Database.instance.CelestialBodies[cb].isResearched = false;
                        Database.instance.CelestialBodies[cb].researchState = 0;
                    }
                }
                File.Delete("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                RSTLogWriter.Log("Converted Old Save file to new persistent.sfs config node - Loading/Conversion complete. Old save file deleted");
            }

            RSTLogWriter.Log("RBGameSettings Loading Complete");
        }

        public void Save(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
                settingsNode.ClearData();
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }
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

            foreach (var entry in Database.instance.CelestialBodies)
            {
                ConfigNode CBNode = entry.Value.Save(settingsNode);
                RSTUtils.Utilities.Log_Debug("RBGameSettings Saving Entry = {0}", entry.Key.GetName());
                //CBNode.AddValue("body", entry.Key.GetName());
            }
            RSTLogWriter.Log("RBGameSettings Saving Complete");
        }
    }
    
}
