using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSTUtils;
using RSTUtils.Extensions;
using UnityEngine;

namespace ResearchBodies
{
    public partial class ResearchBodies : MonoBehaviour
    {
        private AppLauncherToolBar RBMenuAppLToolBar;
        private Vector2 InstructorscrollViewVector = Vector2.zero;
        private Vector2 ResearchscrollViewVector = Vector2.zero;
        private Vector2 scrollViewVector = Vector2.zero;
        private Vector2 langSettingsScroll = Vector2.zero;
        private CelestialBody selectedBody = null;
        //private Dictionary<int, CelestialBody> GetSelectedBodyFromSelection = new Dictionary<int, CelestialBody>();
        private bool enable = true, showGUI = false, showSettings = false, showStartUI = false;
        private Rect windowRect = new Rect(10, 90, 700, 550); // 10,10,250,300
        private Rect settingsRect = new Rect(800, 90, 350, 240);
        private Rect startWindow = new Rect(40, 40, 490, 300);
        private Rect hoverwindow = new Rect(0, 0, 160, 80);

        private static int _hoverwindowId;
        private static int _RBwindowId;
        private static int _settingswindowId;
        private static int _startwindowId;
        private string tmpToolTip;
        private bool haveTrackedBodies = false;

        private void TurnUIOff()
        {
            if (RBMenuAppLToolBar != null)
            {
                showGUI = RBMenuAppLToolBar.GuiVisible;
                showSettings = false;
                RBMenuAppLToolBar.GuiVisible = false;
            }
        }

        private void TurnUIOn()
        {
            if (RBMenuAppLToolBar != null)
            {
                RBMenuAppLToolBar.GuiVisible = showGUI;
            }
        }

        public void OnGUI()
        {
            if (!enable) return;
            try
            {
                if (!Textures.StylesSet) Textures.SetupStyles();
            }
            catch (Exception ex)
            {
                RSTLogWriter.Log("Unable to set GUI Styles to draw the GUI");
                RSTLogWriter.Log("Exception: {0}", ex);
            }

            GUI.skin = HighLogic.Skin;

            if (RBMenuAppLToolBar.ShowHoverText)
            {
                hoverwindow.xMin = Input.mousePosition.x - 200;
                hoverwindow.yMin = (Screen.height - Input.mousePosition.y) - 100;
                hoverwindow.xMax = Input.mousePosition.x;
                hoverwindow.yMax = (Screen.height - Input.mousePosition.y);
                hoverwindow.ClampInsideScreen();
                hoverwindow = GUI.Window(_hoverwindowId, hoverwindow, DrawHoverwin, "Research Bodies", HighLogic.Skin.window);
            }

            if (showStartUI)
            {
                startWindow.ClampInsideScreen();
                startWindow = GUILayout.Window(_startwindowId, startWindow, DrawStartWindow, "ResearchBodies " + Locales.currentLocale.Values["misc_settings"]);
            }

            if (!RBMenuAppLToolBar.GuiVisible || RBMenuAppLToolBar.gamePaused || RBMenuAppLToolBar.hideUI) return;

            if (PSystemSetup.Instance.GetSpaceCenterFacility("TrackingStation").GetFacilityDamage() > 0)
            {
                ScreenMessages.PostScreenMessage(Locales.currentLocale.Values["trackingStation_isDestroyed"], 3.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            
            try
            {
                if (RBMenuAppLToolBar.GuiVisible)
                {
                    windowRect.ClampInsideScreen();
                    windowRect = GUILayout.Window(_RBwindowId, windowRect, DrawWindow, "Research Bodies");
                }

                if (showSettings)
                {
                    settingsRect.ClampInsideScreen();
                    settingsRect = GUILayout.Window(_settingswindowId, settingsRect, DrawSettings, Locales.currentLocale.Values["misc_settings"]);
                }

                

                Utilities.DrawToolTip();
            }
            catch (Exception ex)
            {
                RSTLogWriter.Log("Unable to draw GUI");
                RSTLogWriter.Log("Exception: {0}", ex);
            }

        }

        private void DrawHoverwin(int id)
        {
            GUI.Label(new Rect(3, 28, 194, 70), Locales.currentLocale.Values["misc_researchbodiesLabel"]);
        }

        public static void OnLocaleChanged(Locale target)
        {
            toolStrings = new string[] { target.Values["start_easy"], target.Values["start_normal"], target.Values["start_medium"], target.Values["start_hard"] };
        }
        private void DrawStartWindow(int id)
        {

            GUILayout.BeginVertical();
            toolbar = GUILayout.Toolbar(toolbar, toolStrings);

            GUILayout.Label(Database.GetIgnoredBodies((Level) toolbar));

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("<size=11>" + Locales.currentLocale.Values["start_researchPlanCost"] + "</size>", Locales.currentLocale.Values["start_researchPlanCostTT"]), GUILayout.Width(152));
            ResearchCost = (float)Math.Round(GUILayout.HorizontalSlider(ResearchCost, 10f, 50f, GUILayout.Width(270)));
            GUILayout.Label(Convert.ToInt32(ResearchCost + ProgressResearchCost).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("<size=11>" + Locales.currentLocale.Values["start_researchProgress"] + "</size>", Locales.currentLocale.Values["start_researchProgressTT"]), GUILayout.Width(152));
            ProgressResearchCost = (float)Math.Round(GUILayout.HorizontalSlider(ProgressResearchCost, 5f, 15f, GUILayout.Width(270)));
            GUILayout.Label(Convert.ToInt32(ProgressResearchCost).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("<size=11>" + Locales.currentLocale.Values["start_scienceRewards"] + "</size>", Locales.currentLocale.Values["start_scienceRewardsTT"]), GUILayout.Width(152));
            ScienceReward = (float)Math.Round(GUILayout.HorizontalSlider(ScienceReward, 5f, 60f, GUILayout.Width(270)));
            GUILayout.Label(Convert.ToInt32(ScienceReward).ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            /*  GUI.Box(new Rect(10, 140, 150, 70), Locales.currentLocale.Values["start_researchPlanCost"] + " : " + (Database.StartResearchCosts[toolbar] + Database.ProgressResearchCosts[toolbar]).ToString() + " " + Locales.currentLocale.Values["start_funds"]);
              GUI.Box(new Rect(170, 140, 150, 70), Locales.currentLocale.Values["start_researchProgress"] + " : " + Database.ProgressResearchCosts[toolbar] + " " + Locales.currentLocale.Values["start_funds"]);
              GUI.Box(new Rect(330, 140, 150, 70), Locales.currentLocale.Values["start_scienceRewards"] + " : " + Database.ScienceRewards[toolbar] + " " + Locales.currentLocale.Values["start_science"]);
              */

            if (GUILayout.Button("OK", GUILayout.Width(200)))
            {
                SaveStartSettings((Level)toolbar);
                showStartUI = false;
            }
            GUILayout.EndVertical();
            Utilities.SetTooltipText();
            GUI.DragWindow();
        }
        private void DrawSettings(int id)
        {
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, "Close Window");
            Rect closeRect = new Rect(settingsRect.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.ClosebtnStyle))
            {
                showSettings = false;
                return;
            }
            
            GUILayout.BeginVertical();
            langSettingsScroll = GUILayout.BeginScrollView(langSettingsScroll);
            GUILayout.BeginVertical();
            GUILayout.Box(Locales.currentLocale.Values["misc_lang"], Textures.sectionTitleStyle);
            for (int i = 0; i < Locales.locales.Count; i++)
            {
                if (GUILayout.Button(Locales.locales[i].LocaleFull))
                {
                    Locales.currentLocale = Locales.locales[i];
                    Locales.Save(Locales.locales[i]);
                    Locales.LoadDiscoveryMessages();
                    OnLocaleChanged(Locales.currentLocale);
                }
            }

            bool _inputAppL = Database.UseAppLauncher;
            if (!ToolbarManager.ToolbarAvailable)
            {
                GUI.enabled = false;
                tmpToolTip = Locales.currentLocale.Values["settings_useAppLTT_TBNA"];
            }
            else
            {
                tmpToolTip = Locales.currentLocale.Values["settings_useAppLTT_TBA"];
            }

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent(Locales.currentLocale.Values["settings_useAppL"], tmpToolTip), Textures.statusStyle, GUILayout.Width(250));
            _inputAppL = GUILayout.Toggle(_inputAppL, "", GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
            GUILayout.EndHorizontal();
            if (Database.UseAppLauncher != _inputAppL)
            {
                Database.UseAppLauncher = _inputAppL;
                RBMenuAppLToolBar.chgAppIconStockToolBar(Database.UseAppLauncher);
            }
            GUI.enabled = true;

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
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
            GUILayout.EndVertical();
            Utilities.SetTooltipText();
            GUI.DragWindow();
        }
        private void DrawWindow(int id)
        {
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, "Close Window");
            Rect closeRect = new Rect(windowRect.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.ClosebtnStyle))
            {
                RBMenuAppLToolBar.onAppLaunchToggle();
                showSettings = false;
                return;
            }
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginVertical();
            #region Wernher_Portrait Panel 1

            InstructorscrollViewVector = GUILayout.BeginScrollView(InstructorscrollViewVector, GUILayout.Width(148), GUILayout.Height(186));
            GUILayout.BeginVertical();
            if ((IsTSlevel1 && Database.allowTSlevel1) || !IsTSlevel1)
            {
                if (Event.current.type == EventType.Repaint)
                    GUILayout.Box(_portrait, GUILayout.Width(128), GUILayout.Height(128)); 
                else
                {
                    GUILayout.Box(string.Empty, GUILayout.Width(128), GUILayout.Height(128));
                }
                GUILayout.Label("Wernher von Kerman", GUILayout.Width(128)); 
            }
            else
            {
                GUILayout.Label(Locales.currentLocale.Values["trackingStation_hasToBeLevel"], GUILayout.Width(128), GUILayout.Height(128)); 
            }
            #endregion
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            #region BodyList Panel 2
            scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, GUILayout.Width(148), GUILayout.Height(313));
            GUILayout.BeginVertical();
            haveTrackedBodies = false;
            foreach (CelestialBody cb in BodyList)
            {
                if (TrackedBodies[cb] && !bool.Parse(BodySaveNode(cb.GetName()).GetValue("ignore")))
                {
                    if (GUILayout.Button(cb.GetName(), GUILayout.Width(110))) //new Rect(5, fromTop, 110, 32),
                    {
                        if (selectedBody == cb)
                            selectedBody = null;
                        else
                            selectedBody = cb;
                        PlayOKEmote();
                    }
                    haveTrackedBodies = true;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            if (GUILayout.Button(Locales.currentLocale.Values["misc_settings"], GUILayout.Width(130), GUILayout.Height(32))) 
                showSettings = !showSettings;
            #endregion
            GUILayout.EndVertical();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            #region Research Panel 3
            ResearchscrollViewVector = GUILayout.BeginScrollView(ResearchscrollViewVector, GUILayout.Width(522), GUILayout.Height(530));
            GUILayout.BeginVertical();
            if ((IsTSlevel1 && Database.allowTSlevel1) || !IsTSlevel1)
            {
                if (selectedBody == null)
                {
                    if (!haveTrackedBodies)
                        GUILayout.Label("<color=orange>" + Locales.currentLocale.Values["archives_empty"] + "</color>", GUILayout.Width(502)); 
                    else
                        GUILayout.Label("<color=orange>" + Locales.currentLocale.Values["archives_welcome"] + "</color>", GUILayout.Width(502)); //GUILayout
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<b><size=35><color=orange>" + selectedBody.GetName() + "</color></size></b>", GUILayout.Width(150)); 
                    GUILayout.Label("<i>" + Database.DiscoveryMessage[selectedBody.GetName()] + "</i>", GUILayout.Width(300)); 
                    GUILayout.EndHorizontal();
                    
                    if (selectedBody.referenceBody != Planetarium.fetch.Sun)
                        GUILayout.Label(string.Format(Locales.currentLocale.Values["research_orbiting"], selectedBody.referenceBody.GetName()), GUILayout.Width(150)); 
                    else
                        GUILayout.Label(Locales.currentLocale.Values["research_orbitingSun"], GUILayout.Width(150)); 
                    
                    
                    GUILayout.Label(string.Format(Locales.currentLocale.Values["research_researchState"], ResearchState[selectedBody]), GUILayout.Width(502));
                    if (ResearchState[selectedBody] == 0)
                    {
                        if (GUILayout.Button("<color=green>" + string.Format(Locales.currentLocale.Values["research_launchPlan"], selectedBody.GetName()) + " </color><size=10><i>(" + string.Format(Locales.currentLocale.Values["research_launchPlanCost"], (ResearchCost + ProgressResearchCost).ToString() /* 10 */) + ")</i></size>", GUILayout.Width(502)))  
                        {
                            LaunchResearchPlan(selectedBody);
                            PlayNiceEmote();
                        }
                    }
                    else if (ResearchState[selectedBody] >= 10)
                    {
                        //GUILayout.BeginHorizontal();
                        if (GUILayout.Button("<color=red>" + string.Format(Locales.currentLocale.Values["research_stopPlan"], selectedBody.GetName()) + " </color><size=10><i>(" + string.Format(Locales.currentLocale.Values["research_stopPlanGives"], ResearchCost /* 5 */) + ")</i></size>", GUILayout.Width(502)))  
                        {
                            StopResearchPlan(selectedBody);
                            PlayBadEmote();
                        }
                        if (ResearchState[selectedBody] < 40 && ResearchState[selectedBody] >= 10)
                        {
                            if (GUILayout.Button(Locales.currentLocale.Values["researchData_aspect"], GUILayout.Width(502))) 
                            {
                                PlayNiceEmote();
                                Research(selectedBody, 10);
                                SetBodyDiscoveryLevels(); // Update Body Discovery Levels
                            }
                        }
                        else if (ResearchState[selectedBody] >= 40 && ResearchState[selectedBody] < 100)
                        {
                            GUILayout.Label("<i><color=green>" + Locales.currentLocale.Values["researchData_aspect"] + " ✓</color></i>", GUILayout.Width(502)); 
                            if (GUILayout.Button(Locales.currentLocale.Values["researchData_characteristics"], GUILayout.Width(502))) 
                            {
                                PlayNiceEmote();
                                Research(selectedBody, 10);
                                SetBodyDiscoveryLevels(); // Update Body Discovery Levels

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
                            GUILayout.Label("<i><color=green>" + Locales.currentLocale.Values["researchData_aspect"] + " ✓</color></i>", GUILayout.Width(502)); //new Rect(188, 227, 502, 32), 
                            GUILayout.Label("<i><color=green>" + Locales.currentLocale.Values["researchData_characteristics"] + " ✓</color></i>", GUILayout.Width(502)); //new Rect(188, 264, 502, 32), 

                            GUILayout.Label("<b>" + string.Format(Locales.currentLocale.Values["research_isNowFullyResearched_sendVessels"], selectedBody.GetName()) + "</b>", GUILayout.Width(502)); //new Rect(188, 301, 502, 32), 

                            // GUI.Label(new Rect(188, 301, 502, 32), "Send a exploration probe to " + selectedBody.GetName() + " : Incomplete", HighLogic.Skin.button);
                            // GUI.Label(new Rect(188, 338, 502, 32), "Run science experiments on " + selectedBody.GetName() + " : Incomplete", HighLogic.Skin.button);
                        }
                        //GUILayout.EndHorizontal();
                    }
                }
            }
            #endregion
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            Utilities.SetTooltipText();
            GUI.DragWindow();
            /*
            if (GUI.Button(new Rect(10, 175, 128, 35), "Play Happy"))
                PlayEmote(5);
            if (GUI.Button(new Rect(10, 217, 128, 35), "Play Disappointed"))
                PlayEmote(10); */
        }

    }
}
