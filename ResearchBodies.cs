using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Contracts;
using KSP.UI.Screens;
using RSTUtils;

using System.Reflection;

namespace ResearchBodies
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class ResearchBodies : MonoBehaviour
    {
        /// <summary>
        /// A dictionary that sais if bodies have been tracked using telescope
        /// </summary>
        public static Dictionary<CelestialBody, bool> TrackedBodies = new Dictionary<CelestialBody, bool>();
        /// <summary>
        /// A dictionary that returns the research state of a body in %
        /// </summary>
        public static Dictionary<CelestialBody, int> ResearchState = new Dictionary<CelestialBody, int>();
        private ApplicationLauncherButton appButton = null;
        // float level;  level 1 = 0 , level 2 = 0.5 , level 3 (max) = 1
        private bool enable = true, showGUI = false, showSettings = false, showStartUI = false, showHoverText = false;
        private Rect windowRect = new Rect(10, 90, 700, 550); // 10,10,250,300
        private Rect settingsRect = new Rect(800, 90, 300, 400);
        private Rect startWindow = new Rect(40, 40, 490, 300);
        private Rect hoverwindow = new Rect(0, 0, 160, 80);
        private System.Random random = new System.Random();
        private static float ResearchCost = 10f, ProgressResearchCost = 5f, ScienceReward = 5f;

        private static int _hoverwindowId;
        private static int _RBwindowId;
        private static int _settingswindowId;
        private static int _startwindowId;

        /// <summary>
        /// Tarsier Space Tech Interface fields
        /// </summary>
        internal bool isTSTInstalled = false;
        internal static List<CelestialBody> TSTCBGalaxies = new List<CelestialBody>();
        public static  List<CelestialBody> BodyList = new List<CelestialBody>();

        public static int toolbar = 1;
        public static string[] toolStrings = new string[] { Locales.currentLocale.Values["start_easy"], Locales.currentLocale.Values["start_normal"], Locales.currentLocale.Values["start_medium"], Locales.currentLocale.Values["start_hard"] };

        private const int PortraitWidth = 128;
        private const int PortraitHeight = 128;

        Vector2 scrollViewVector, langSettingsScroll;
        CelestialBody selectedBody = null;
        int scrollHeight = 5, langScrollHeight = 5;
        Dictionary<int, CelestialBody> GetSelectedBodyFromSelection = new Dictionary<int, CelestialBody>();

        #region InstructorVariables
        private KerbalInstructor _instructor;
        private RenderTexture _portrait;
        private Rect _windowRect = new Rect(250f, 250f, 128f, 128f);
        private Dictionary<GUIContent, CharacterAnimationState> _responses;
        #endregion
        
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
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    showStartUI = true;
            }
            else
            {
                ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                toolbar = int.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("difficulty") ?? "0"); // DEPRECATED!

                ResearchCost = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ResearchCost") ?? "10");
                ProgressResearchCost = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ProgressResearchCost") ?? "5");
                ScienceReward = float.Parse(mainnode.GetNode("RESEARCHBODIES").GetValue("ResearchCost") ?? "5");

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
        public void SaveStartSettings(Level l)
        {
            ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
            ConfigNode _node = mainnode.GetNode("RESEARCHBODIES");
            foreach (CelestialBody body in BodyList)
            {
                _node.GetNodes().Single(node => node.GetValue("body") == body.GetName()).AddValue("ignore", Database.IgnoreData[body].GetLevel(l).ToString());
            }
            _node.AddValue("difficulty", toolbar.ToString());

            _node.AddValue("ResearchCost", Convert.ToInt32(ResearchCost).ToString());
            _node.AddValue("ProgressResearchCost", Convert.ToInt32(ProgressResearchCost).ToString());
            _node.AddValue("ScienceReward", Convert.ToInt32(ScienceReward).ToString());

            mainnode.Save("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
            LoadConfig();
            LoadBodyLook();
        }
        private void ToggleGUI()
        {
            if (PSystemSetup.Instance.GetSpaceCenterFacility("TrackingStation").GetFacilityDamage() > 0)
                ScreenMessages.PostScreenMessage(Locales.currentLocale.Values["trackingStation_isDestroyed"], 3.0f, ScreenMessageStyle.UPPER_CENTER);
           /* else if (PSystemSetup.Instance.GetSpaceCenterFacility("TrackingStation").GetFacilityLevel() < 0.5)
            {
                if (Database.allowTSlevel1)
                    showGUI = !showGUI;
                else
                    ScreenMessages.PostScreenMessage(Locales.currentLocale.Values["trackingStation_hasToBeLevel"], 3.0f, ScreenMessageStyle.UPPER_CENTER);
            } */
            else
            {
                showGUI = !showGUI;
                // SpaceTexture = Database.RandomSpaceTexture;
            }
        }
        public void Start()
        {
            _startwindowId = Utilities.getnextrandomInt();
            _hoverwindowId = _startwindowId + 1;
            _RBwindowId = _hoverwindowId + 1;
            _settingswindowId = _RBwindowId + 1;
            
            isTSTInstalled = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "TarsierSpaceTech");
            if (isTSTInstalled)  //If TST assembly is present, initialise TST wrapper.
            {
                if (!TSTWrapper.InitTSTWrapper())
                {
                    isTSTInstalled = false; //If the initialise of wrapper failed set bool to false, we won't be interfacing to TST today.
                }
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                enable = false;
            LoadConfig();



            Resources.FindObjectsOfTypeAll<KerbalInstructor>()
                .ToList()
                .ForEach(instructor => print("Instructor: " + instructor.CharacterName + ", prefab: " + instructor.name));

            _instructor = Create("Instructor_Wernher");

            GameEvents.onGUIRnDComplexSpawn.Add(TurnUIOff);
            GameEvents.onGUIMissionControlSpawn.Add(TurnUIOff);
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                GameEvents.onGUIMissionControlSpawn.Add(CheckContracts);
            if (enable)
            {
                LoadBodyLook();
                appButton = ApplicationLauncher.Instance.AddModApplication(ToggleGUI, ToggleGUI, onHoverOn, onHoverOff, null, null, ApplicationLauncher.AppScenes.SPACECENTER, Database.IconTexture);
            }
            foreach (CelestialBody cb in BodyList)
            {
                if (!Database.IgnoreBodies.Contains(cb) && TrackedBodies[cb])
                    scrollHeight += 37;
            }
            foreach (Locale l in Locales.locales)
                langScrollHeight += 37; 
            
        }

        public void OnDestroy()
        {
            if (_portrait != null)
                _portrait.Release();

            if (_instructor != null)
                Destroy(_instructor.gameObject);
            if (enable)
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
            GameEvents.onGUIRnDComplexDespawn.Remove(TurnUIOff);
            GameEvents.onGUIMissionControlDespawn.Remove(TurnUIOff);
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                GameEvents.onGUIMissionControlDespawn.Remove(CheckContracts);
        }

        private void onHoverOn()
        {
            showHoverText = true;
        }
        private void onHoverOff()
        {
            showHoverText = false;
        }
        private void CheckContracts()
        {
            foreach (Contract contract in ContractSystem.Instance.Contracts)
            {
                foreach (ContractParameter cp in contract.AllParameters.ToList())
                {
                    foreach (CelestialBody body in FlightGlobals.Bodies.Where(b => !TrackedBodies[b]))
                    {
                        if (cp.Title.Contains(body.GetName()))
                        {
                            TryDeclineContract(contract);
                            break;
                        }
                    }
                }
            }
        }
        private void TryDeclineContract(Contract c)
        {
            try
            {
                Log.log("Declined contract \"" + c.Title + "\"");
                c.Decline();
               
            }
            catch (Exception e)
            {
                Log.logError("Unable to decline contract ! " + e);
            }
        }
        public void OnDisable()
        {
            if (enable)
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
            GameEvents.onGUIRnDComplexDespawn.Remove(TurnUIOff);
            GameEvents.onGUIMissionControlDespawn.Remove(TurnUIOff);
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                GameEvents.onGUIMissionControlDespawn.Remove(CheckContracts);
        }
        private void DrawHoverwin(int id)
        {
            GUI.Label(new Rect(3, 28, 194, 70), Locales.currentLocale.Values["misc_researchbodiesLabel"]);
        }
        public void OnGUI() 
        {
            if (showHoverText)
            {
                hoverwindow.xMin = Input.mousePosition.x - 200;
                hoverwindow.yMin = (Screen.height - Input.mousePosition.y) - 100;
                hoverwindow.xMax = Input.mousePosition.x;
                hoverwindow.yMax = (Screen.height - Input.mousePosition.y);
                hoverwindow = KSPUtil.ClampRectToScreen(GUI.Window(_hoverwindowId, hoverwindow, DrawHoverwin, "Research Bodies", HighLogic.Skin.window));
            }
            if (showGUI) 
                windowRect = KSPUtil.ClampRectToScreen(GUI.Window(_RBwindowId, windowRect, DrawWindow, "Research Bodies", HighLogic.Skin.window));
            if (showSettings)
                settingsRect = KSPUtil.ClampRectToScreen(GUI.Window(_settingswindowId, settingsRect, DrawSettings, Locales.currentLocale.Values["misc_settings"], HighLogic.Skin.window));
            if (showStartUI)
                startWindow = KSPUtil.ClampRectToScreen(GUI.Window(_startwindowId, startWindow, DrawStartWindow, "ResearchBodies " + Locales.currentLocale.Values["misc_settings"], HighLogic.Skin.window));
        }
        public static void OnLocaleChanged(Locale target)
        {
            toolStrings = new string[] { target.Values["start_easy"], target.Values["start_normal"], target.Values["start_medium"], target.Values["start_hard"] };
        }
        private void DrawStartWindow(int id)
        {
            GUI.skin = HighLogic.Skin;
            GUI.DragWindow(new Rect(0, 0, 450, 32));
            toolbar = GUI.Toolbar(new Rect(10, 50, 470, 40), toolbar, toolStrings);

            GUI.Label(new Rect(10, 100, 430, 90), Database.GetIgnoredBodies(Database.GetLevel(toolbar)));

            GUI.Label(new Rect(8, 140, 152, 25), "<size=11>" + Locales.currentLocale.Values["start_researchPlanCost"] + "</size>");
            ResearchCost = (float)Math.Round(GUI.HorizontalSlider(new Rect(170, 140, 270, 25), ResearchCost, 10f, 50f));
            GUI.Label(new Rect(445, 140, 30, 25), Convert.ToInt32(ResearchCost + ProgressResearchCost).ToString());

            GUI.Label(new Rect(8, 170, 152, 25), "<size=11>" + Locales.currentLocale.Values["start_researchProgress"] + "</size>");
            ProgressResearchCost = (float)Math.Round(GUI.HorizontalSlider(new Rect(170, 170, 270, 25), ProgressResearchCost, 5f, 15f));
            GUI.Label(new Rect(445, 170, 30, 25), Convert.ToInt32(ProgressResearchCost).ToString());

            GUI.Label(new Rect(8, 200, 152, 25), "<size=11>" + Locales.currentLocale.Values["start_scienceRewards"] + "</size>");
            ScienceReward = (float)Math.Round(GUI.HorizontalSlider(new Rect(170, 200, 270, 25), ScienceReward, 5f, 60f));
            GUI.Label(new Rect(445, 200, 30, 25), Convert.ToInt32(ScienceReward).ToString());

          /*  GUI.Box(new Rect(10, 140, 150, 70), Locales.currentLocale.Values["start_researchPlanCost"] + " : " + (Database.StartResearchCosts[toolbar] + Database.ProgressResearchCosts[toolbar]).ToString() + " " + Locales.currentLocale.Values["start_funds"]);
            GUI.Box(new Rect(170, 140, 150, 70), Locales.currentLocale.Values["start_researchProgress"] + " : " + Database.ProgressResearchCosts[toolbar] + " " + Locales.currentLocale.Values["start_funds"]);
            GUI.Box(new Rect(330, 140, 150, 70), Locales.currentLocale.Values["start_scienceRewards"] + " : " + Database.ScienceRewards[toolbar] + " " + Locales.currentLocale.Values["start_science"]);
            */

            if (GUI.Button(new Rect(140, 220, 200, 40), "OK"))
            {
                SaveStartSettings(Database.GetLevel(toolbar));
                showStartUI = false;
            }
        }
        private void DrawSettings(int id)
        {
            GUI.skin = HighLogic.Skin;
            GUI.DragWindow(new Rect(0, 0, 300, 32));
            GUI.Label(new Rect(10, 36, 280, 32), Locales.currentLocale.Values["misc_lang"]);
            langSettingsScroll = GUI.BeginScrollView(new Rect(10, 73, 280, 117), langSettingsScroll, new Rect(0, 0, 200, langScrollHeight));
            for (int i = 0; i < Locales.locales.Count; i++)
            {
                if (GUI.Button(new Rect(20, i * 32 + 5, 190, 32), Locales.locales[i].LocaleFull))
                {
                    Locales.currentLocale = Locales.locales[i];
                    Locales.Save(Locales.locales[i]);
                    Locales.LoadDiscoveryMessages();
                    OnLocaleChanged(Locales.currentLocale);
                }
            }
            GUI.EndScrollView();
            /* GUI.Label(new Rect(10, 195, 280, 32), Locales.currentLocale.Values["misc_instructor"]);
            if (GUI.Button(new Rect(10, 232, 130, 32), "Wernher von Kerman"))
            {
                if (File.Exists("GameData/ResearchBodies/PluginData/cacheInstructor"))
                {
                    StreamReader sr = new StreamReader("GameData/ResearchBodies/PluginData/cacheInstructor");
                    string line = sr.ReadLine();

                    sr.Close();
                }
            }
            if (GUI.Button(new Rect(150, 232, 130, 32), "Gene Kerman"))
            { } */
        }
        private void DrawWindow(int id) 
        {
            GUI.skin = HighLogic.Skin;
            GUI.DragWindow(new Rect(0, 0, 700, 35));
            #region Wernher_Portrait
            GUI.Box(new Rect(10, 40, 148, 186), string.Empty);
            if ((IsTSlevel1 && Database.allowTSlevel1) || !IsTSlevel1)
            {
                if (Event.current.type == EventType.Repaint)
                    GUI.Label(new Rect(20, 50, 128, 128), _portrait);
                GUI.Label(new Rect(20, 185, 128, 32), "Wernher von Kerman");
            }
            else
            {
                GUI.Label(new Rect(20, 50, 128, 128), Locales.currentLocale.Values["trackingStation_hasToBeLevel"]);
            }
            #endregion
            #region BodyList
            // GUI.Box(new Rect(10, 227, 148, 403), string.Empty);
            scrollViewVector = GUI.BeginScrollView(new Rect(10, 227, 148, 313), scrollViewVector, new Rect(0, 0, 120, scrollHeight), false, false);
            int fromTop = 5; bool isFirst = true;
            foreach (CelestialBody cb in BodyList)
            {
                if (TrackedBodies[cb] && !bool.Parse(BodySaveNode(cb.GetName()).GetValue("ignore")))
                {
                    if (GUI.Button(new Rect(5, fromTop, 110, 32), cb.GetName()))
                    {
                        if (selectedBody == cb)
                            selectedBody = null;
                        else
                            selectedBody = cb;
                        PlayOKEmote();
                    }
                    isFirst = false;
                    if (isFirst)
                        fromTop += 32;
                    else
                        fromTop += 37;
                }
            }
            GUI.EndScrollView();
            #endregion
            #region Research
            GUI.Box(new Rect(168, 40, 522, 500), string.Empty);
            
            if ((IsTSlevel1 && Database.allowTSlevel1) || !IsTSlevel1)
            {
                if (selectedBody == null)
                {
                    if (scrollHeight == 5)
                        GUI.Label(new Rect(198, 50, 502, 50), "<color=orange>" + Locales.currentLocale.Values["archives_empty"] + "</color>");
                    else
                        GUI.Label(new Rect(198, 50, 502, 50), "<color=orange>" + Locales.currentLocale.Values["archives_welcome"] + "</color>");
                }
                else
                {
                    GUI.Label(new Rect(198, 50, 502, 50), "<b><size=35><color=orange>" + selectedBody.GetName() + "</color></size></b>");
                    if (selectedBody.referenceBody != Planetarium.fetch.Sun)
                        GUI.Label(new Rect(198, 90, 150, 70), string.Format(Locales.currentLocale.Values["research_orbiting"], selectedBody.referenceBody.GetName()));
                    else
                        GUI.Label(new Rect(198, 90, 150, 70), Locales.currentLocale.Values["research_orbitingSun"]);
                    GUI.Label(new Rect(350, 50, 330, 90), "<i>" + Database.DiscoveryMessage[selectedBody.GetName()] + "</i>");
                    GUI.Label(new Rect(198, 150, 502, 30), string.Format(Locales.currentLocale.Values["research_researchState"], ResearchState[selectedBody]));
                    if (ResearchState[selectedBody] == 0)
                    {
                        if (GUI.Button(new Rect(188, 190, 502, 32), "<color=green>" + string.Format(Locales.currentLocale.Values["research_launchPlan"], selectedBody.GetName()) + " </color><size=10><i>(" + string.Format(Locales.currentLocale.Values["research_launchPlanCost"], (ResearchCost + ProgressResearchCost).ToString() /* 10 */) + ")</i></size>"))
                        {
                            LaunchResearchPlan(selectedBody);
                            PlayNiceEmote();
                        }
                    }
                    else if (ResearchState[selectedBody] >= 10)
                    {
                        if (GUI.Button(new Rect(188, 190, 502, 32), "<color=red>" + string.Format(Locales.currentLocale.Values["research_stopPlan"], selectedBody.GetName()) + " </color><size=10><i>(" + string.Format(Locales.currentLocale.Values["research_stopPlanGives"], ResearchCost /* 5 */) + ")</i></size>"))
                        {
                            StopResearchPlan(selectedBody);
                            PlayBadEmote();
                        }
                        if (ResearchState[selectedBody] < 40 && ResearchState[selectedBody] >= 10)
                        {
                            if (GUI.Button(new Rect(188, 227, 502, 32), Locales.currentLocale.Values["researchData_aspect"]))
                            {
                                PlayNiceEmote();
                                Research(selectedBody, 10);
                                LoadBodyLook(); // Update looking of bodies
                            }
                        }
                        else if (ResearchState[selectedBody] >= 40 && ResearchState[selectedBody] < 100)
                        {
                            GUI.Label(new Rect(188, 227, 502, 32), "<i><color=green>" + Locales.currentLocale.Values["researchData_aspect"] + " ✓</color></i>", HighLogic.Skin.button);
                            if (GUI.Button(new Rect(188, 264, 502, 32), Locales.currentLocale.Values["researchData_caracteristics"]))
                            {
                                PlayNiceEmote();
                                Research(selectedBody, 10);
                                LoadBodyLook(); // Update looking of bodies

                                //then...
                                if (ResearchState[selectedBody] == 100)
                                {
                                    ScreenMessages.PostScreenMessage(string.Format(Locales.currentLocale.Values["research_isNowFullyResearched_funds"], selectedBody.GetName(), ScienceReward));
                                    ResearchAndDevelopment.Instance.AddScience(ScienceReward, TransactionReasons.None);
                                }
                            }
                        }
                        else if (ResearchState[selectedBody] >= 100)
                        {
                            GUI.Label(new Rect(188, 227, 502, 32), "<i><color=green>" + Locales.currentLocale.Values["researchData_aspect"] + " ✓</color></i>", HighLogic.Skin.button);
                            GUI.Label(new Rect(188, 264, 502, 32), "<i><color=green>" + Locales.currentLocale.Values["researchData_caracteristics"] + " ✓</color></i>", HighLogic.Skin.button);

                            GUI.Label(new Rect(188, 301, 502, 32), "<b>" + string.Format(Locales.currentLocale.Values["research_isNowFullyResearched_sendVessels"], selectedBody.GetName()) + "</b>");

                            // GUI.Label(new Rect(188, 301, 502, 32), "Send a exploration probe to " + selectedBody.GetName() + " : Incomplete", HighLogic.Skin.button);
                            // GUI.Label(new Rect(188, 338, 502, 32), "Run science experiments on " + selectedBody.GetName() + " : Incomplete", HighLogic.Skin.button);
                        }
                    }
                }
            }
            #endregion

            if (GUI.Button(new Rect(550, 498, 130, 32), Locales.currentLocale.Values["misc_settings"]))
                showSettings = !showSettings;
            /*
            if (GUI.Button(new Rect(10, 175, 128, 35), "Play Happy"))
                PlayEmote(5);
            if (GUI.Button(new Rect(10, 217, 128, 35), "Play Disappointed"))
                PlayEmote(10); */
        }

        #region Instructor Functions
        private KerbalInstructor Create(string instructorName)
        {
            var prefab = AssetBase.GetPrefab(instructorName);
            if (prefab == null)
                throw new ArgumentException("Could not find instructor named '" + instructorName + "'");

            var prefabInstance = (GameObject)Instantiate(prefab);
            var instructor = prefabInstance.GetComponent<KerbalInstructor>();

            _portrait = new RenderTexture(PortraitWidth, PortraitWidth, 8);
            instructor.instructorCamera.targetTexture = _portrait;

            _responses = instructor.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(fi => fi.FieldType == typeof(CharacterAnimationState))
                .Where(fi => fi.GetValue(instructor) != null)
                .ToDictionary(fi => new GUIContent(fi.Name), fi => fi.GetValue(instructor) as CharacterAnimationState);

            return instructor;
        }
        
        private void PlayEmote(int emote)
        {
            _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[emote]]);
        }
        private void PlayOKEmote()
        {
            int rand = random.Next(4);
            if (rand == 1)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[2]]);
            else if (rand == 2)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[6]]);
            else
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[7]]);
        }
        private void PlayNiceEmote()
        {
            int rand = random.Next(3);
            if (rand == 1)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[4]]);
            else
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[5]]);
        }
        private void PlayBadEmote()
        {
            int rand = random.Next(5);
            if (rand == 1)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[11]]);
            else if (rand == 2)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[12]]);
            else if (rand == 3)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[13]]);
            else
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[14]]);
        }
        #endregion
        private ConfigNode BodySaveNode(string name)
        {
            ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg"), bodyNode = null;
            foreach (ConfigNode node in mainnode.GetNode("RESEARCHBODIES").nodes)
            {
                if (node.GetValue("body") == name)
                    bodyNode = node;
            }
            return bodyNode;
        }
        public bool IsTSlevel1
        {
            get { return PSystemSetup.Instance.GetSpaceCenterFacility("TrackingStation").GetFacilityLevel() < 0.5; }
        }
        public static bool Research(CelestialBody body, int researchToAdd)
        {
            if (ResearchState[body] < 100 && Funding.Instance.Funds >= ProgressResearchCost)
            {
                ResearchState[body] += researchToAdd;
                Funding.Instance.AddFunds(-ProgressResearchCost, TransactionReasons.None);

                ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                ConfigNode bodyNode = null;
                foreach (ConfigNode node in mainnode.GetNode("RESEARCHBODIES").nodes)
                {
                    if (node.GetValue("body") == body.GetName())
                        bodyNode = node;
                }
                bodyNode.SetValue("researchState", ResearchState[body].ToString());
                mainnode.Save("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");

                return true;
            }
            else
            {
                return false;
            }
        }
        public static void LaunchResearchPlan(CelestialBody cb)
        {
            if (ResearchState[cb] == 0)
            {
                if (Funding.Instance.Funds >= ResearchCost)
                {
                    Funding.Instance.AddFunds(- ResearchCost, TransactionReasons.None);
                    Research(cb, 10);
                }
                else
                    ScreenMessages.PostScreenMessage(string.Format(Locales.currentLocale.Values["launchPlan_notEnoughScience"], cb.GetName()), 3.0f, ScreenMessageStyle.UPPER_CENTER);

            }
            else
                Log.logError(string.Format(Locales.currentLocale.Values["launchPlan_alreadyStarted"], cb.GetName()));
        }
        public static void StopResearchPlan(CelestialBody cb)
        {
            if (ResearchState[cb] >= 10)
            {
                    Funding.Instance.AddFunds(ResearchCost, TransactionReasons.None);

                    ConfigNode mainnode = ConfigNode.Load("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                    ConfigNode bodyNode = null;
                    foreach (ConfigNode node in mainnode.GetNode("RESEARCHBODIES").nodes)
                    {
                        if (node.GetValue("body") == cb.GetName())
                            bodyNode = node;
                    }
                    bodyNode.SetValue("researchState", "0");
                    mainnode.Save("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg");
                    ResearchState[cb] = 0;
            }
            else
                Log.logError(string.Format(Locales.currentLocale.Values["stopPlan_hasntBeenStarted"], cb.GetName()));
        }
        public static void LoadBodyLook()
        {
            foreach (CelestialBody cb in BodyList)
            {
                if (TrackedBodies.ContainsKey(cb) && !Database.IgnoreBodies.Contains(cb))
                {
                    if (!TrackedBodies[cb])
                    {
                        cb.DiscoveryInfo.SetLevel(DiscoveryLevels.Presence);
                    }
                    else if (TrackedBodies[cb] && ResearchState[cb] < 50)
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
        }
        public static void Save()
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
        private void TurnUIOff()
        {
            showGUI = false;
            showSettings = false;
        }
    }
    /*
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class ResearchBodies_Observatory : MonoBehaviour
    {
        Collider ObservatoryCollid;
        public bool observatoryCloned = false;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Log.log("Object : distance : " + hit.distance + ", name: " + hit.transform.name + ", gameObject name : " + hit.transform.gameObject.name);
                }
            }
            if (!observatoryCloned)
            {
                ObservatoryCollid = FindObjectsOfType<Collider>().FirstOrDefault(collider => collider.name.Contains("Observatory_Mesh"));
                if (ObservatoryCollid != null)
                {
                    //  GameObject obj = new GameObject("Observatory_ResearchBodies");
                    Instantiate(ObservatoryCollid.transform, new Vector3(215f, -362f, 460f), ObservatoryCollid.transform.rotation);
                    //   NewObserv.transform.position = new Vector3(215f, -362f, 460f);
                    // NewObserv.transform.parent = null;
                    Log.log("Cloned observatory mesh");
                    observatoryCloned = true;
                }
            }
        }
    } */
}
