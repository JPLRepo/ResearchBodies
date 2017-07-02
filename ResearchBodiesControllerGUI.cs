/*
 * ResearchBodiesControllerGUI.cs
 * (C) Copyright 2016, Jamie Leighton 
 * Original code by KSP forum User simon56modder.
 * License : MIT 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Original code was developed by 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 */
using System;
using System.Collections.Generic;
using RSTUtils;
using RSTUtils.Extensions;
using UnityEngine;
using KSP.Localization;

namespace ResearchBodies
{
    public partial class ResearchBodiesController : MonoBehaviour
    {
        //internal AppLauncherToolBar RBMenuAppLToolBar;
        private Vector2 InstructorscrollViewVector = Vector2.zero;
        private Vector2 ResearchscrollViewVector = Vector2.zero;
        private Vector2 scrollViewVector = Vector2.zero;
        private CelestialBody selectedBody = null;
        internal bool enable = true, showGUI = false;
        private Rect windowRect = new Rect(10, 90, 700, 550);
        
        private static int _RBwindowId;
        private string tmpToolTip;
        private bool haveTrackedBodies = false;
        private int difficulty;

        //debug vars
        private bool showObsdebugUI = false;
        private bool ObsGlobal = true;
        private bool moveUpgradeable = true;
        private bool Obsx50 = false;
        private bool Obsx10 = false;
        private bool Obsx1 = false;
        private bool Obsx01 = false;
        private bool Obsx001 = false;

        private bool ObsLvl1 = true;
        private bool ObsLvl2 = false;
        private bool ObsLvl3 = false;
        private Transform tmpTransform;
        internal bool French;

        //private static string LOCK_ID = "ResearchBodies_KeyBinder";

        public void onScreenResolutionModified(int width, int height)
        {
            Utilities.setScaledScreen();
            windowRect = new Rect(1, 1, Utilities.scaledScreenWidth  - 2, Utilities.scaledScreenHeight - 2);
        }

        #region Debug Facility
        #if DEBUGFACILITY
        private void Update()
        {

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.Z))
                {
                    showObsdebugUI = !showObsdebugUI;
                }
                if (showObsdebugUI)
                {
                    float variance = 0.1f;
                    if (Obsx001) variance = 0.01f;
                    if (Obsx01) variance = 0.1f;
                    if (Obsx1) variance = 1f;
                    if (Obsx10) variance = 10f;
                    if (Obsx50) variance = 50f;

                    //X Axis
                    if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.I))
                    {
                        if (ObsGlobal)
                        {
                            if (moveUpgradeable)
                                tmpTransform.position =
                                new Vector3(
                                    tmpTransform.position.x - variance,
                                    tmpTransform.position.y,
                                    tmpTransform.position.z);
                            else
                                ResearchBodies_Observatory.SpaceCenterObservatory.BuildingTransform.position =
                                new Vector3(
                                    ResearchBodies_Observatory.SpaceCenterObservatory.BuildingTransform.position.x - variance,
                                    ResearchBodies_Observatory.SpaceCenterObservatory.BuildingTransform.position.y,
                                    ResearchBodies_Observatory.SpaceCenterObservatory.BuildingTransform.position.z);
                        }
                        else
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.localPosition =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.x - variance,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.y,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.z);
                            else
                                tmpTransform.localPosition =
                               new Vector3(
                                   tmpTransform.localPosition.x - variance,
                                   tmpTransform.localPosition.y,
                                   tmpTransform.localPosition.z);

                        }
                    }
                    if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.K))
                    {
                        if (ObsGlobal)
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.position =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.x + variance,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.y,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.z);
                            else
                                tmpTransform.position =
                                new Vector3(
                                    tmpTransform.position.x + variance,
                                    tmpTransform.position.y,
                                    tmpTransform.position.z);
                        }
                        else
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.localPosition =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.x + variance,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.y,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.z);
                            else
                                tmpTransform.localPosition =
                                new Vector3(
                                    tmpTransform.localPosition.x + variance,
                                    tmpTransform.localPosition.y,
                                    tmpTransform.localPosition.z);
                        }
                    }
                    //Y Axis
                    if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.J))
                    {
                        if (ObsGlobal)
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.position =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.x,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.y + variance,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.z);
                            else
                                tmpTransform.position =
                                new Vector3(
                                    tmpTransform.position.x,
                                    tmpTransform.position.y + variance,
                                    tmpTransform.position.z);
                        }
                        else
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.localPosition =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.x,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.y + variance,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.z);
                            else
                                tmpTransform.localPosition =
                                new Vector3(
                                    tmpTransform.localPosition.x,
                                    tmpTransform.localPosition.y + variance,
                                    tmpTransform.localPosition.z);
                        }
                    }
                    if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.L))
                    {
                        if (ObsGlobal)
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.position =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.x,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.y - variance,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.z);
                            else
                                tmpTransform.position =
                                new Vector3(
                                    tmpTransform.position.x,
                                    tmpTransform.position.y - variance,
                                    tmpTransform.position.z);
                        }
                        else
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.localPosition =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.x,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.y - variance,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.z);
                            else
                                tmpTransform.localPosition =
                                new Vector3(
                                    tmpTransform.localPosition.x,
                                    tmpTransform.localPosition.y - variance,
                                    tmpTransform.localPosition.z);
                        }
                    }
                    //Z Axis
                    if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.H))
                    {
                        if (ObsGlobal)
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.position =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.x,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.y,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.z + variance);
                            else
                                tmpTransform.position =
                                new Vector3(
                                    tmpTransform.position.x,
                                    tmpTransform.position.y,
                                    tmpTransform.position.z + variance);
                        }
                        else
                        {
                            if (moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.localPosition =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.x,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.y,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.z + variance);
                            else
                                tmpTransform.localPosition =
                                new Vector3(
                                    tmpTransform.localPosition.x,
                                    tmpTransform.localPosition.y,
                                    tmpTransform.localPosition.z + variance);
                        }
                    }
                    if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.N))
                    {
                        if (ObsGlobal)
                        {
                            if(moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.position =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.x,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.y,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.position.z - variance);
                            else
                                tmpTransform.position =
                                new Vector3(
                                    tmpTransform.position.x,
                                    tmpTransform.position.y,
                                    tmpTransform.position.z - variance);
                        }
                        else
                        {
                            if(moveUpgradeable)
                                ResearchBodies_Observatory.upgradeablefacility.transform.localPosition =
                                new Vector3(
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.x,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.y,
                                    ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.z - variance);
                            else
                                tmpTransform.localPosition =
                                new Vector3(
                                    tmpTransform.localPosition.x,
                                    tmpTransform.localPosition.y,
                                    tmpTransform.localPosition.z - variance);
                        }
                    }
                }
            }

        }
        #endif
        #endregion

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

            GUI.skin = Textures.ObsSkin;
            #if DEBUGFACILITY
            if (showObsdebugUI)
            {
                observRect = GUILayout.Window(_RBwindowId + 1, observRect, DrawObservDebug, "Research Bodies");
            }
            #endif

            //if (!RBMenuAppLToolBar.GuiVisible || RBMenuAppLToolBar.gamePaused || RBMenuAppLToolBar.hideUI) return;
            if (!showGUI) return;

            if (PSystemSetup.Instance.GetSpaceCenterFacility("TrackingStation").GetFacilityDamage() > 0)
            {
                ScreenMessages.PostScreenMessage(Locales.FmtLocaleString("#autoLOC_RBodies_00018"), 3.0f,
                    ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            try
            {
                windowRect.ClampInsideScreen();
                windowRect = GUILayout.Window(_RBwindowId, windowRect, DrawWindow, "Research Bodies");
                Utilities.DrawToolTip();
            }
            catch (Exception ex)
            {
                RSTLogWriter.Log("Unable to draw GUI");
                RSTLogWriter.Log("Exception: {0}", ex);
            }

        }
        
        private void DrawWindow(int id)
        {
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, "Close Window");
            Rect closeRect = new Rect(windowRect.width - 65, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.ClosebtnStyle))
            {
                ResearchBodies_Observatory.SpaceCenterObservatory.DeActivateObservatory_SC_Facility();
                return;
            }
            GUILayout.BeginVertical();

            #region Left Side Screen
            //Screen Left Side Starts
            GUILayout.BeginHorizontal();
            GUILayout.BeginArea(new Rect((Utilities.scaledScreenWidth / 2) - 500, 50, 710, 800));
            GUILayout.BeginHorizontal(GUILayout.Width(700));
            GUILayout.BeginVertical();
            GUILayout.BeginVertical();

            #region Wernher_Portrait Panel 1

            InstructorscrollViewVector = GUILayout.BeginScrollView(InstructorscrollViewVector, GUILayout.Width(148),
                GUILayout.Height(186));
            GUILayout.BeginVertical();
            if ((IsTSlevel1 && Database.instance.allowTSlevel1) || !IsTSlevel1)
            {
                if (Event.current.type == EventType.Repaint)
                    GUILayout.Box(instructor_Werner.Portrait, GUILayout.Width(128), GUILayout.Height(128));
                else
                {
                    GUILayout.Box(string.Empty, GUILayout.Width(128), GUILayout.Height(128));
                }
                GUILayout.Label("Wernher von Kerman", GUILayout.Width(128));
            }
            else
            {
                GUILayout.Label(Locales.FmtLocaleString("#autoLOC_RBodies_00017"), GUILayout.Width(128),
                    GUILayout.Height(128));
            }

#endregion

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            #region BodyList Panel 2

            scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, GUILayout.Width(148), GUILayout.Height(342));
            GUILayout.BeginVertical();
            haveTrackedBodies = false;
            foreach (KeyValuePair<CelestialBody, CelestialBodyInfo> cb in Database.instance.CelestialBodies)
            {
                //if (cb.Value.isResearched && !cb.Value.ignore)
                if (cb.Value.isResearched)
                {
                    if (GUILayout.Button(cb.Key.GetName(), GUILayout.Width(110)))
                    {
                        if (selectedBody == cb.Key)
                            selectedBody = null;
                        else
                            selectedBody = cb.Key;
                        instructor_Werner.PlayOKEmote();
                    }
                    if (!cb.Value.ignore)
                        haveTrackedBodies = true;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

#endregion

            GUILayout.EndVertical();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            #region Research Panel 3

            ResearchscrollViewVector = GUILayout.BeginScrollView(ResearchscrollViewVector, GUILayout.Width(550),
                GUILayout.Height(530));
            GUILayout.BeginVertical();
            if ((IsTSlevel1 && Database.instance.allowTSlevel1) || !IsTSlevel1)
            {
                if (selectedBody == null)
                {
                    if (!haveTrackedBodies)
                        GUILayout.Label("<color=orange>" + Locales.FmtLocaleString("#autoLOC_RBodies_00001") + "</color>",
                            GUILayout.Width(530));
                    else
                        GUILayout.Label(
                            "<color=orange>" + Locales.FmtLocaleString("#autoLOC_RBodies_00001") + "</color>",
                            GUILayout.Width(530)); //GUILayout
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<b><size=35><color=orange>" + selectedBody.GetName() + "</color></size></b>",
                        GUILayout.Width(150));
                    GUILayout.Label("<i>" + (French ? Database.instance.CelestialBodies[selectedBody].discoveryMessage : Localizer.Format("#autoLOC_RBodies_discovery_" + selectedBody.bodyName)) + "</i>",
                        GUILayout.Width(300));
                    GUILayout.EndHorizontal();

                    if (selectedBody.referenceBody != Planetarium.fetch.Sun)
                        GUILayout.Label(Locales.FmtLocaleString("#autoLOC_RBodies_00003", selectedBody.referenceBody.displayName), GUILayout.Width(150));
                    else
                        GUILayout.Label(Locales.FmtLocaleString("#autoLOC_RBodies_00004"), GUILayout.Width(150));


                    GUILayout.Label(
                        Locales.FmtLocaleString("#autoLOC_RBodies_00005", Database.instance.CelestialBodies[selectedBody].researchState.ToString()), GUILayout.Width(502));
                    if (Database.instance.CelestialBodies[selectedBody].researchState == 0)
                    {
                        if (
                            GUILayout.Button(
                                "<color=#0ef907>" +
                                Locales.FmtLocaleString("#autoLOC_RBodies_00006",
                                    selectedBody.displayName) + " </color><size=10><i>(" +
                                Locales.FmtLocaleString("#autoLOC_RBodies_00007",
                                    (Database.instance.RB_SettingsParms.ResearchCost +
                                     Database.instance.RB_SettingsParms.ProgressResearchCost).ToString() /* 10 */) +
                                ")</i></size>", GUILayout.Width(502)))
                        {
                            LaunchResearchPlan(selectedBody);
                            instructor_Werner.PlayNiceEmote();
                        }
                    }
                    else if (Database.instance.CelestialBodies[selectedBody].researchState >= 1)
                    {
                        if (!Database.instance.CelestialBodies[selectedBody].ignore)
                        {
                            if (
                                GUILayout.Button(
                                    "<color=red>" +
                                    Locales.FmtLocaleString("#autoLOC_RBodies_00008",
                                        selectedBody.displayName) + " </color><size=10><i>(" +
                                    Locales.FmtLocaleString("#autoLOC_RBodies_00009",
                                        Database.instance.RB_SettingsParms.ResearchCost.ToString() /* 5 */) + ")</i></size>",
                                    GUILayout.Width(502)))
                            {
                                StopResearchPlan(selectedBody);
                                instructor_Werner.PlayBadEmote();
                            }
                        }
                        if (Database.instance.CelestialBodies[selectedBody].researchState < 40 &&
                            Database.instance.CelestialBodies[selectedBody].researchState >= 1)
                        {
                            //if (GUILayout.Button(Locales.FmtLocaleString("#autoLOC_RBodies_00010"),
                            //    GUILayout.Width(502)))
                            //{
                            //    instructor_Werner.PlayNiceEmote();
                            //    Research(selectedBody, 10);
                            //}
                        }
                        else if (Database.instance.CelestialBodies[selectedBody].researchState >= 40 &&
                                 Database.instance.CelestialBodies[selectedBody].researchState < 100)
                        {
                            GUILayout.Label(
                                "<i><color=#0ef907>" + Locales.FmtLocaleString("#autoLOC_RBodies_00010") +
                                " ✓</color></i>", GUILayout.Width(502));
                            //if (GUILayout.Button(Locales.FmtLocaleString("#autoLOC_RBodies_00011"),
                            //    GUILayout.Width(502)))
                            //{
                            //    instructor_Werner.PlayNiceEmote();
                            //    Research(selectedBody, 10);
                            //}
                        }
                        else if (Database.instance.CelestialBodies[selectedBody].researchState >= 100)
                        {
                            GUILayout.Label(
                                "<i><color=#0ef907>" + Locales.FmtLocaleString("#autoLOC_RBodies_00010") +
                                " ✓</color></i>", GUILayout.Width(502)); //new Rect(188, 227, 502, 32), 
                            GUILayout.Label(
                                "<i><color=#0ef907>" + Locales.FmtLocaleString("#autoLOC_RBodies_00011") +
                                " ✓</color></i>", GUILayout.Width(502)); //new Rect(188, 264, 502, 32), 

                            GUILayout.Label(
                                "<b>" +
                                Locales.FmtLocaleString("#autoLOC_RBodies_00013", selectedBody.GetName()) + "</b>", GUILayout.Width(502));
                                //new Rect(188, 301, 502, 32), 

                            // GUI.Label(new Rect(188, 301, 502, 32), "Send a exploration probe to " + selectedBody.GetName() + " : Incomplete", HighLogic.Skin.button);
                            // GUI.Label(new Rect(188, 338, 502, 32), "Run science experiments on " + selectedBody.GetName() + " : Incomplete", HighLogic.Skin.button);
                        }
                    }
                }
            }

#endregion

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUILayout.EndHorizontal();
            //Screen Left Side Ends.
            #endregion

            #region Right Side Screen
            /*GUILayout.BeginHorizontal();
            GUILayout.BeginArea(new Rect((Utilities.scaledScreenWidth / 2) + 215, 50, 310, 800));
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            
            GUILayout.BeginVertical();

            #region Linus_Portrait Panel 1

            InstructorscrollViewVector = GUILayout.BeginScrollView(InstructorscrollViewVector, GUILayout.Width(148),
                GUILayout.Height(186));
            GUILayout.BeginVertical();
            if ((IsTSlevel1 && Database.instance.allowTSlevel1) || !IsTSlevel1)
            {
                if (Event.current.type == EventType.Repaint)
                    GUILayout.Box(instructor_Linus.Portrait, GUILayout.Width(128), GUILayout.Height(128));
                else
                {
                    GUILayout.Box(string.Empty, GUILayout.Width(128), GUILayout.Height(128));
                }
                GUILayout.Label("Linus Kerman", GUILayout.Width(128));
            }
            else
            {
                GUILayout.Label(Locales.FmtLocaleString("#autoLOC_RBodies_00017"), GUILayout.Width(128),
                    GUILayout.Height(128));
            }
            
            #endregion

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label("Label1", GUILayout.Width(128));
            GUILayout.EndVertical();
            GUILayout.Label("Label2", GUILayout.Width(128));
            GUILayout.Label("Label3", GUILayout.Width(128));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUILayout.EndHorizontal();
            */
            #endregion

            GUILayout.EndVertical();
            Utilities.SetTooltipText();
            //GUI.DragWindow();
        }

        private void DrawObservDebug(int id)
        {
            GUI.skin = GUI.skin = HighLogic.Skin;
            GUILayout.BeginVertical();
            scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, GUILayout.Height(450));
            GUILayout.BeginVertical();
            
            var tmpBool = GUILayout.Toggle(ObsGlobal, "Global");
            ObsGlobal = tmpBool;
            var tmpBool2 = GUILayout.Toggle(moveUpgradeable, "Move Upgradeable");
            moveUpgradeable = tmpBool2;
            GUILayout.BeginHorizontal();
            var tmp2Bool = GUILayout.Toggle(Obsx50, "x50", GUILayout.Width(100));
            Obsx50 = tmp2Bool;
            var tmp3Bool = GUILayout.Toggle(Obsx10, "x10", GUILayout.Width(100));
            Obsx10 = tmp3Bool;
            var tmp4Bool = GUILayout.Toggle(Obsx1, "x1", GUILayout.Width(100));
            Obsx1 = tmp4Bool;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            var tmp15Bool = GUILayout.Toggle(Obsx01, "x0.1", GUILayout.Width(100));
            Obsx01 = tmp15Bool;
            var tmp16Bool = GUILayout.Toggle(Obsx001, "x0.01", GUILayout.Width(100));
            Obsx001 = tmp16Bool;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.enabled = true;
            if (ResearchBodies_Observatory.upgradeablefacility.UpgradeLevels[0].facilityInstance == null)
            {
                GUI.enabled = false;
                ObsLvl1 = false;
            }
            var tmp5Bool = GUILayout.Toggle(ObsLvl1, "lvl1", GUILayout.Width(100));
            ObsLvl1 = tmp5Bool;
            GUI.enabled = true;

            if (ResearchBodies_Observatory.upgradeablefacility.UpgradeLevels[1].facilityInstance == null)
            {
                GUI.enabled = false;
                ObsLvl2 = false;
            }
            var tmp6Bool = GUILayout.Toggle(ObsLvl2, "lvl2", GUILayout.Width(100));
            ObsLvl2 = tmp6Bool;
            GUI.enabled = true;

            if (ResearchBodies_Observatory.upgradeablefacility.UpgradeLevels[2].facilityInstance == null)
            {
                GUI.enabled = false;
                ObsLvl3 = false;
            }
            var tmp7Bool = GUILayout.Toggle(ObsLvl3, "lvl3", GUILayout.Width(100));
            ObsLvl3 = tmp7Bool;
            GUI.enabled = true;

            tmpTransform = ResearchBodies_Observatory.upgradeablefacility.transform;
            if (ObsLvl1)
            {
                tmpTransform =
                    ResearchBodies_Observatory.upgradeablefacility.UpgradeLevels[0].facilityInstance.transform;
            }
            if (ObsLvl2)
            {
                tmpTransform =
                    ResearchBodies_Observatory.upgradeablefacility.UpgradeLevels[1].facilityInstance.transform;
            }
            if (ObsLvl3)
            {
                tmpTransform =
                    ResearchBodies_Observatory.upgradeablefacility.UpgradeLevels[2].facilityInstance.transform;
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Upgradeable", GUILayout.Width(250));
            GUILayout.Label("Facility Instance");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Position X " + ResearchBodies_Observatory.upgradeablefacility.transform.position.x, GUILayout.Width(250));
            if (ObsLvl1 || ObsLvl2 || ObsLvl3)
                GUILayout.Label("Position X " + tmpTransform.position.x);
            else
                GUILayout.Label("N/A");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Position Y " + ResearchBodies_Observatory.upgradeablefacility.transform.position.y, GUILayout.Width(250));
            if (ObsLvl1 || ObsLvl2 || ObsLvl3)
                GUILayout.Label("Position Y " + tmpTransform.position.y);
            else
                GUILayout.Label("N/A");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Position Z " + ResearchBodies_Observatory.upgradeablefacility.transform.position.z, GUILayout.Width(250));
            if (ObsLvl1 || ObsLvl2 || ObsLvl3)
                GUILayout.Label("Position Z " + tmpTransform.position.z);
            else
                GUILayout.Label("N/A");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Local X " + ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.x, GUILayout.Width(250));
            if (ObsLvl1 || ObsLvl2 || ObsLvl3)
                GUILayout.Label("Local X " + tmpTransform.localPosition.x);
            else
                GUILayout.Label("N/A");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Local Y " + ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.y, GUILayout.Width(250));
            if (ObsLvl1 || ObsLvl2 || ObsLvl3)
                GUILayout.Label("Local Y " + tmpTransform.localPosition.y);
            else
                GUILayout.Label("N/A");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Local Z " + ResearchBodies_Observatory.upgradeablefacility.transform.localPosition.z, GUILayout.Width(250));
            if (ObsLvl1 || ObsLvl2 || ObsLvl3)
                GUILayout.Label("Local Z " + tmpTransform.localPosition.z);
            else
                GUILayout.Label("N/A");
            GUILayout.EndHorizontal();
           
          
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
