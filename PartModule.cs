using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace ResearchBodies
{
    public class ModuleTrackBodies : PartModule
    {
        List<CelestialBody> celestialBodiesNear = new List<CelestialBody>();
        private bool enable = true, showGUI = false, foundBody = false, withParent = false, canResearch = true;
        private string bodyFound, parentBody, nothing;
        // private Rect bodyFoundRect, parentBodyFoundRect;
        public Dictionary<CelestialBody, bool> TrackedBodies = new Dictionary<CelestialBody, bool>();
        public Dictionary<CelestialBody, int> ResearchState = new Dictionary<CelestialBody, int>();
        private Rect windowRect = new Rect(10, 10, 250, 250); // 10,10,250,350
        private System.Random random = new System.Random();
        [KSPField]
        public int difficulty;
        [KSPField]
        public int minAltitude;
        [KSPField]
        public double maxTrackDistance;
        [KSPField]
        public double electricChargeRequest;
        [KSPField]
        public bool landed;
        [KSPField]
        public bool requiresPart;
        [KSPField]
        public string requiredPart;
        [KSPField]
        public int viewAngle, scienceReward;
        private Vector2 scrollViewVector = Vector2.zero;
        // private Texture2D SpaceTexture;

        /// <summary>
        /// Tarsier Space Tech Interface fields
        /// </summary>
        private bool isTSTInstalled = false;
        private List<CelestialBody> TSTCBGalaxies = new List<CelestialBody>();
        private List<CelestialBody> BodyList = new List<CelestialBody>();


        public override void OnAwake()
        {
            base.OnAwake();            
            if (HighLogic.LoadedScene != GameScenes.LOADING && HighLogic.LoadedScene != GameScenes.LOADINGBUFFER)
            {
                isTSTInstalled = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "TarsierSpaceTech");
                if (isTSTInstalled)  //If TST assembly is present, initialise TST wrapper.
                {
                    if (!TSTWrapper.InitTSTWrapper())
                    {
                        isTSTInstalled = false; //If the initialise of wrapper failed set bool to false, we won't be interfacing to TST today.
                    }
                }
                if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX && !Database.enableInSandbox)
                    enable = false;
                LoadConfig();
            }
        }
        public void LoadConfig()
        {
            if (!File.Exists("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg"))
            {
                ConfigNode file = new ConfigNode();
                ConfigNode node = file.AddNode("RESEARCHBODIES");

                BodyList = FlightGlobals.Bodies;
                if (isTSTInstalled && TSTWrapper.APITSTReady)
                {
                    BodyList = BodyList.Concat(TSTWrapper.actualTSTAPI.CBGalaxies).ToList();
                }

                foreach (CelestialBody cb in BodyList)
                {
                    ConfigNode cbCfg = node.AddNode("BODY");
                    cbCfg.AddValue("body", cb.GetName());
                    cbCfg.AddValue("isResearched", "false");
                    cbCfg.AddValue("researchState", "0");
                    TrackedBodies[cb] = false;
                    ResearchState[cb] = 0;
                }
                file.Save("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
            }
            else
            {
                ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");

                BodyList = FlightGlobals.Bodies;
                if (isTSTInstalled && TSTWrapper.APITSTReady)
                {
                    BodyList = BodyList.Concat(TSTWrapper.actualTSTAPI.CBGalaxies).ToList();
                }

                foreach (CelestialBody cb in BodyList)
                {
                    bool fileContainsCB = false;
                    foreach (ConfigNode node in mainnode.GetNode("RESEARCHBODIES").nodes)
                    {
                        if (cb.GetName().Contains(node.GetValue("body")))
                        {
                            if (bool.Parse(node.GetValue("ignore")))
                            {
                                TrackedBodies[cb] = true;
                                ResearchState[cb] = 100;
                            }
                            else
                            {
                                TrackedBodies[cb] = bool.Parse(node.GetValue("isResearched"));
                                if (node.HasValue("researchState"))
                                {
                                    ResearchState[cb] = int.Parse(node.GetValue("researchState"));
                                }
                                else
                                {
                                    ConfigNode cbNode = null;
                                    foreach (ConfigNode cbSettingNode in mainnode.GetNode("RESEARCHBODIES").nodes)
                                    {
                                        if (cbSettingNode.GetValue("body") == cb.GetName())
                                            cbNode = cbSettingNode;
                                    }
                                    cbNode.AddValue("researchState", "0");
                                    mainnode.Save("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                                    ResearchState[cb] = 0;
                                }
                            }
                            fileContainsCB = true;
                        }
                    }
                    if (!fileContainsCB)
                    {
                        ConfigNode newNodeForCB = mainnode.GetNode("RESEARCHBODIES").AddNode("BODY");
                        newNodeForCB.AddValue("body", cb.GetName());
                        newNodeForCB.AddValue("isResearched", "false");
                        newNodeForCB.AddValue("researchState", "0");
                        TrackedBodies[cb] = false; ResearchState[cb] = 0;
                        mainnode.Save("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                    }
                }
            }
        }
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state != StartState.Editor && enable)
            {
                foreach (CelestialBody cb in BodyList)
                {
                    if (TrackedBodies.ContainsKey(cb) && !Database.IgnoreBodies.Contains(cb))
                    {
                        if (!TrackedBodies[cb])
                        {
                            cb.DiscoveryInfo.SetLevel(DiscoveryLevels.Presence);
                        }
                        else if (TrackedBodies[cb] && ResearchState[cb] < 100)
                        {
                            cb.DiscoveryInfo.SetLevel(DiscoveryLevels.Appearance);
                            // cb.SetResourceMap(null);
                        }
                        else
                        {
                            cb.DiscoveryInfo.SetLevel(DiscoveryLevels.Owned);
                        }
                    }
                }
                //RenderingManager.AddToPostDrawQueue(0, OnGUI);
            }
        }
        public void OnGUI()
        {
            if (showGUI) windowRect = GUI.Window(525, windowRect, DrawWindow, Locales.currentLocale.Values["telescope_trackBodies"], HighLogic.Skin.window);
        }
        [KSPEvent(guiName = "Research Bodies", guiActiveEditor = false, guiActive = true)]
        public void ToggleGUI()
        {
            if (!this.vessel.Landed && this.vessel.atmDensity < 0.1 && this.vessel.altitude > minAltitude)
            {
                showGUI = !showGUI;
                // SpaceTexture = Database.RandomSpaceTexture;
            }
            else
                ScreenMessages.PostScreenMessage(string.Format(Locales.currentLocale.Values["telescope_mustBeInSpace"], minAltitude), 3.0f, ScreenMessageStyle.UPPER_CENTER);
        }
        void DrawWindow(int windowID = 525)
        {
            Rect scrollViewRect = new Rect(5, 40, 240, 170);
            GUI.DragWindow(new Rect(0, 0, 250, 32));
            // scrollViewVector = GUI.BeginScrollView(scrollViewRect, scrollViewVector, new Rect(0, 0, 600, 600), HighLogic.Skin.horizontalScrollbar, HighLogic.Skin.verticalScrollbar);
            if (GUI.Button(new Rect(5, 40, 240, 40), Locales.currentLocale.Values["telescope_trackBodies"]/*GameDatabase.Instance.GetTexture("ResearchBodies/images/space", false)*/, HighLogic.Skin.button))
            {
                // SpaceTexture = Database.RandomSpaceTexture;
                nothing = Database.NothingHere[random.Next(Database.NothingHere.Count)];
                // bodyFoundRect = new Rect(5 + random.Next(195), 250 + random.Next(50), 45, 45);
                // parentBodyFoundRect = new Rect(5 + random.Next(205), 250 + random.Next(60), 35, 35);
                if (requiresPart)
                {
                    bool local = false;
                    foreach (Part part in this.vessel.Parts)
                    {
                        if (part.name.Contains(requiredPart))
                            local = true;
                    }
                    if (!local)
                    {
                        canResearch = false;
                        ScreenMessages.PostScreenMessage(string.Format(Locales.currentLocale.Values["telescope_mustHavePart"], requiredPart), 3.0f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                this.part.RequestResource("ElectricCharge", electricChargeRequest);
                difficulty++;
                if ((random.Next(Database.chances + difficulty) == 1 || random.Next(Database.chances + difficulty) == 2) && canResearch)
                {
                    CelestialBody cb;
                    List<CelestialBody> BodiesInView = new List<CelestialBody>();
                    foreach (CelestialBody body in BodyList)
                    {
                        Vector3 hostPos = this.part.transform.position;
                        Vector3 targetPos = body.transform.position;
                        float angle = Vector3.Angle((targetPos - hostPos), this.part.transform.up);
                        if (Vector3d.Distance(body.transform.position, this.vessel.transform.position) <= maxTrackDistance && angle <= viewAngle)
                            BodiesInView.Add(body);
                    }
                    cb = BodiesInView[random.Next(BodiesInView.Count)];
                    if (!TrackedBodies[cb])
                    {
                        foundBody = true;
                        if (HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)
                        {
                            ResearchAndDevelopment.Instance.AddScience(scienceReward, TransactionReasons.None);
                            ScreenMessages.PostScreenMessage("Added " + scienceReward + " science points !");
                        }
                        ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                        if (cb.referenceBody.DiscoveryInfo.Level == DiscoveryLevels.Presence)
                        {
                            TrackedBodies[cb.referenceBody] = true;
                            TrackedBodies[cb] = true;
                            withParent = true;
                            parentBody = cb.referenceBody.GetName();
                            foreach (ConfigNode node in mainnode.GetNode("RESEARCHBODIES").nodes)
                            {
                                if (node.GetValue("body") == cb.referenceBody.GetName())
                                {
                                    node.SetValue("isResearched", "true");
                                }
                                if (node.GetValue("body") == cb.GetName())
                                {
                                    node.SetValue("isResearched", "true");
                                }
                                try
                                {
                                    if (node.GetValue("body") == cb.referenceBody.referenceBody.GetName() && (cb.referenceBody.referenceBody.DiscoveryInfo.Level == DiscoveryLevels.Appearance || cb.referenceBody.referenceBody.DiscoveryInfo.Level == DiscoveryLevels.Presence))
                                    {
                                        node.SetValue("isResearched", "true");
                                    }
                                }
                                catch { }
                            }
                            Log.log("[ResearchBodies] Found body " + cb.GetName() + " orbiting around " + cb.referenceBody.GetName() + " !");
                        }
                        else
                        {
                            TrackedBodies[cb] = true;
                            withParent = false;
                            foreach (ConfigNode node in mainnode.GetNode("RESEARCHBODIES").nodes)
                            {
                                if (node.GetValue("body") == cb.GetName())
                                {
                                    node.SetValue("isResearched", "true");
                                }
                            }
                            Log.log("[ResearchBodies] Found body " + cb.GetName() + " !");
                        }
                        bodyFound = cb.GetName();
                        mainnode.Save("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                        OnAwake(); OnStart(StartState.None);
                    }
                    else { foundBody = false; }
                }
                else { foundBody = false; }
            } //endif button
            // GUI.EndScrollView();
            // GUI.DrawTexture(new Rect(5, 250, 240, 95), SpaceTexture);
            if (foundBody)
            {
                if (withParent)
                {
                    GUI.Label(new Rect(5, 82, 240, 163), Database.DiscoveryMessage[bodyFound] + " \r" + Database.DiscoveryMessage[parentBody], HighLogic.Skin.label);
                    // Graphics.DrawTexture(parentBodyFoundRect, Database.Textures["pointBig"], new Rect(0,0,Database.Textures["pointBig"].width,Database.Textures["pointBig"].height), 0, 0, 0, 0, Database.Colors[Database.GetBodyByName(parentBody)]);
                    // Graphics.DrawTexture(bodyFoundRect, Database.Textures["pointSmall"], new Rect(0, 0, Database.Textures["pointSmall"].width, Database.Textures["pointSmall"].height), 0, 0, 0, 0, Database.Colors[Database.GetBodyByName(bodyFound)]);
                }
                else
                {
                    GUI.Label(new Rect(5, 82, 240, 163), Database.DiscoveryMessage[bodyFound], HighLogic.Skin.label);
                    // Graphics.DrawTexture(bodyFoundRect, Database.Textures["pointBig"], new Rect(0, 0, Database.Textures["pointBig"].width, Database.Textures["pointBig"].height), 0, 0, 0, 0, Database.Colors[Database.GetBodyByName(bodyFound)]);
                }
            }
            else
            {
                GUI.Label(new Rect(5, 82, 240, 163), nothing, HighLogic.Skin.label);
            }
        }
        public void SaveCfg()
        {
            ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
            foreach (CelestialBody body in BodyList)
            {
                foreach (ConfigNode node in mainnode.GetNode("RESEARCHBODIES").nodes)
                {
                    if (body.GetName() == node.GetValue("body"))
                    {
                        if (ResearchState.ContainsKey(body))
                            node.SetValue("researchState", ResearchState[body].ToString());
                        node.SetValue("isResearched", TrackedBodies[body].ToString());
                    }
                }
            }
            mainnode.Save("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
        }
    }

}
