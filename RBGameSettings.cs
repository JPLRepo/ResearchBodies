/*
 * RBGameSettings.cs 
 * License : MIT
 * Copyright (c) 2016 Jamie Leighton 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
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
        
        private bool Enabled;
        private int Difficulty;
        private int ResearchCost;
        private int ProgressResearchCost;
        private int ScienceReward;
        private bool UseAppLauncher;
        private bool DebugLogging;
        private int chances;
        private int[] StartResearchCosts, ProgressResearchCosts, ScienceRewards;
        private bool enableInSandbox;
        private bool allowTSlevel1;
        private bool foundOldSettings;
        

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
            foundOldSettings = false;
        }

        public void Load(ConfigNode node)
        {
            //Does an old save file exist? If not then we load from persistent.SFS config nodes.
            if (!File.Exists("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg"))
            {
                if (node.HasNode(configNodeName)) //Load SFS config nodes
                {
                    ConfigNode RBsettingsNode = node.GetNode(configNodeName);

                    foundOldSettings = RBsettingsNode.TryGetValue("Enabled", ref Enabled);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("Difficulty", ref Difficulty);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("ResearchCost", ref ResearchCost);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("ProgressResearchCost", ref ProgressResearchCost);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("ScienceReward", ref ScienceReward);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("UseAppLauncher", ref UseAppLauncher);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("DebugLogging", ref DebugLogging);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("chances", ref chances);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("enableInSandbox", ref enableInSandbox);
                    foundOldSettings = foundOldSettings || RBsettingsNode.TryGetValue("allowTSlevel1", ref allowTSlevel1);

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
                    //if (Difficulty == 0) //If difficult == 0 user somehow hasn't selected new game difficulty. So show the startup UI.
                    //    ResearchBodiesController.instance.showStartUI = true;
                }
                else  //No config node, so Must be NEW Game!
                {
                    //Enabled = true;
                    //chances = Database.instance.chances;
                    //enableInSandbox = Database.instance.enableInSandbox;
                    //allowTSlevel1 = Database.instance.allowTSlevel1;
                    //ResearchBodiesController.instance.showStartUI = true;
                    
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
                foundOldSettings = true;
                ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                Difficulty = int.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("difficulty") ?? "0"); 
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
                RSTLogWriter.Log("unable to load OLD save file. Old save file deleted");
            }


            //So now we have CB data and we should have bunch of settings that from now on are handled by the stock settings integration.
            //If we did find those settings we overwrite the stock settings intregration values, but after this they won't exist any more for this save as we won't save them again here.
            if (foundOldSettings)
            {
                ResearchBodies.Enabled = Enabled;
                RSTLogWriter.debuggingOn = DebugLogging;
                if (Database.instance != null)
                {
                    Database.instance.RB_SettingsParms.ResearchCost = ResearchCost;
                    Database.instance.RB_SettingsParms.ProgressResearchCost = ProgressResearchCost;
                    Database.instance.RB_SettingsParms.ScienceReward = ScienceReward;
                    Database.instance.RB_SettingsParms.UseAppLToolbar = UseAppLauncher;
                    Database.instance.RB_SettingsParms.DebugLogging = DebugLogging;
                    Database.instance.RB_SettingsParms.DiscoverySeed = chances;
                    Database.instance.RB_SettingsParms.Enabledtslvl1 = allowTSlevel1;
                    Database.instance.ApplySettings();
                }
                else
                {
                    RSTLogWriter.Log("Failed to apply old game settings to new Stock settings integration");
                }

            }
            RSTLogWriter.Log("RBGameSettings Loading Complete");
            RSTLogWriter.Flush();
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

            foreach (var entry in Database.instance.CelestialBodies)
            {
                ConfigNode CBNode = entry.Value.Save(settingsNode);
                RSTUtils.Utilities.Log_Debug("RBGameSettings Saving Entry = {0}", entry.Key.GetName());
                //CBNode.AddValue("body", entry.Key.GetName());
            }
            RSTLogWriter.Log("RBGameSettings Saving Complete");
            RSTLogWriter.Flush();
        }
    }
    
}
