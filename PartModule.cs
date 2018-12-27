/*
 * ModuleTrackBodies.cs
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

using System.Collections.Generic;
using System.Text;
using KSP.Localization;
using UnityEngine;
using RSTUtils;
using RSTUtils.Extensions;

namespace ResearchBodies
{
    public class ModuleTrackBodies : PartModule
    {
        private bool showGUI = false, foundBody = false, withParent = false, canResearch = true;
        private bool foundpopup = false;
        private bool checkedEnabledFlag = false;
        private bool foundBodyTooWeak = false;
        private string nothing = "";
        private CelestialBody bodyFound, parentBody;
        private Rect windowRect = new Rect(10, 10, 250, 450); // 10,10,250,350
        private int _partwindowID;
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
        private Vector2 scrollViewBodiesVector = Vector2.zero;
        private List<CelestialBody> BodiesInView = new List<CelestialBody>();
        private List<CelestialBody> BodiesInViewResearched = new List<CelestialBody>();
        private Vector3 hostPos;
        private Vector3 targetPos;
        private float angle;
        private double distance;
        private int startingdifficulty;
        private double searchButtonDisplayTimer;
        private bool searchButtonDisplay = false;
        private bool sceneChangeRequested;


        public override void OnAwake()
        {
            base.OnAwake();            
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                _partwindowID = Utilities.getnextrandomInt();
                startingdifficulty = difficulty;
            }
            sceneChangeRequested = false;
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
        }

        public void OnDestroy()
        {
            GameEvents.onGameSceneLoadRequested.Remove(onGameSceneLoadRequested);
        }

        public void onGameSceneLoadRequested(GameScenes scene)
        {
            sceneChangeRequested = true;
        }

        /// <summary>
        ///     Called by unity every frame.
        /// </summary>
        protected virtual void Update()
        {
            if (sceneChangeRequested) return;
            if (!checkedEnabledFlag && Time.timeSinceLevelLoad > 3.0f && HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                checkedEnabledFlag = true;
                if (!ResearchBodies.Enabled)
                {
                    Events["Research Bodies"].guiActive = false;
                    Events["Research Bodies"].active = false;
                }
            }
            //Pop-up box if something was found.
            if (foundBody && !foundpopup)
            {
                foundpopup = true;
                Vector2 anchormin = new Vector2(0.5f, 0.5f);
                Vector2 anchormax = new Vector2(0.5f, 0.5f);
                string msg = "";
                if (withParent) //And was a parent body also discovered?
                {
                    msg = Localizer.Format("#autoLOC_RBodies_discovery_" + bodyFound.bodyName) ;
                    msg += "\n" + Localizer.Format("#autoLOC_RBodies_discovery_" + parentBody.bodyName);
                }
                else
                {
                    msg = Localizer.Format("#autoLOC_RBodies_discovery_" + bodyFound.bodyName);
                }
                string title = Localizer.Format("#autoLOC_RBodies_00049");
                UISkinDef skin = HighLogic.UISkin;
                DialogGUIBase[] dialogGUIBase = new DialogGUIBase[1];
                dialogGUIBase[0] = new DialogGUIButton("Ok", delegate { foundpopup = false; foundBody = false; });
                PopupDialog.SpawnPopupDialog(anchormin, anchormax,
                    new MultiOptionDialog(title, msg, title, skin, dialogGUIBase), false, HighLogic.UISkin, true,
                    string.Empty);
            }
        }

        public void OnGUI()
        {
            if (HighLogic.LoadedSceneIsFlight && !sceneChangeRequested)
            {
                if (showGUI && ResearchBodies.Enabled)
                {
                    Textures.SetupStyles(); //Load textures if not loaded already

                    GUI.skin = HighLogic.Skin;
                    windowRect.ClampToScreen();
                    windowRect = GUILayout.Window(_partwindowID, windowRect, DrawWindow, Localizer.Format("#autoLOC_RBodies_00026"));
                }
            }
        }

        [KSPEvent(name = "Research Bodies", guiName = "#autoLOC_RBodies_00049", guiActiveEditor = false, guiActive = true)]
        public void ToggleGUI()
        {
            if (!this.vessel.Landed && this.vessel.atmDensity < 0.1 && this.vessel.altitude > minAltitude)
            {
                showGUI = !showGUI;
            }
            else
                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_RBodies_00019", minAltitude.ToString()), 3.0f, ScreenMessageStyle.UPPER_CENTER);
        }

        void DrawWindow(int windowID)
        {
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, Localizer.Format("#autoLOC_RBodies_00050"));
            Rect closeRect = new Rect(windowRect.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.ClosebtnStyle))
            {
                showGUI = !showGUI;
                return;
            }
            GUILayout.BeginVertical();
            scrollViewBodiesVector = GUILayout.BeginScrollView(scrollViewBodiesVector, GUILayout.MaxHeight(200f));
            GUILayout.BeginVertical();
            GUILayout.Label(Localizer.Format("#autoLOC_RBodies_00024"), Textures.sectionTitleStyle); //#autoLOC_RBodies_00024 = Available Bodies
            foreach (var body in Database.instance.CelestialBodies)
            {
                if (body.Value.isResearched)
                {
                    GUILayout.Label(body.Key.GetDisplayName().LocalizeRemoveGender() + " - " + body.Value.researchState.ToString("N0") + "%", Textures.PartListPartStyle);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
            GUILayout.BeginVertical();
            GUILayout.Label(Localizer.Format("#autoLOC_RBodies_00021", electricChargeRequest.ToString()));  //#autoLOC_RBodies_00021 = Each use of the Telescope will use <<1>> Electric Charge
            if (GUILayout.Button(Localizer.Format("#autoLOC_RBodies_00022"))) //#autoLOC_RBodies_00022 = Track Bodies
            {
                foundBody = false;
                withParent = false;
                foundBodyTooWeak = false;
                searchButtonDisplayTimer = Planetarium.GetUniversalTime();
                searchButtonDisplay = true;
                nothing = Database.instance.NothingHere[random.Next(Database.instance.NothingHere.Count)];
                BodiesInView.Clear();
                //Check part dependency.. Unusual, But ok.
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
                        ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_RBodies_00020", requiredPart), 5.0f, ScreenMessageStyle.UPPER_CENTER); //#autoLOC_RBodies_00020 = The vessel must have a <<1>> part attached to it!
                    }
                }
                
                if (canResearch) //Part check is OK
                {
                    var totalElecreceived = Utilities.RequestResource(this.part, "ElectricCharge", electricChargeRequest); //get power
                    if (totalElecreceived >= electricChargeRequest*0.99) //If we got power
                    {
                        var randomMax = Database.instance.chances + difficulty;  //Calculate the randomness (sic) and if we found something. This needs to be replaced with something better.
                        var randomNum = random.Next(randomMax);
                        if (randomNum == 1 || randomNum == 2)
                        {
                            //Scan the list of CB's and find anything in range?
                            foreach (CelestialBody body in Database.instance.BodyList)
                            {
                                hostPos = this.part.transform.position;
                                targetPos = body.transform.position;
                                angle = Vector3.Angle(targetPos - hostPos, this.part.transform.up);
                                distance = Vector3d.Distance(body.transform.position, this.vessel.transform.position);
                                //Is it within the acceptable Angle?
                                if (angle <= viewAngle)
                                {
                                    //Is it within the maximum tracking distance of this part?
                                    if (distance <= maxTrackDistance)
                                    {
                                        if (!IsViewObstructed(this.part.transform, body.transform))
                                        {
                                            BodiesInView.Add(body); //We got one!
                                        }
                                    }
                                    //Too far away.
                                    else
                                    {
                                        foundBodyTooWeak = true;
                                    }
                                }
                            }
                            if (BodiesInView.Count > 0)  //did we find anything?
                            {
                                //Remove any already researched.
                                BodiesInViewResearched.Clear();
                                for (int i = BodiesInView.Count - 1; i >= 0; --i)
                                {
                                    if (Database.instance.CelestialBodies[BodiesInView[i]].isResearched)
                                    {
                                        BodiesInViewResearched.Add(BodiesInView[i]);
                                    }
                                }
                                BodiesInViewResearched.ForEach(id => BodiesInView.Remove(id));
                                if (BodiesInView.Count > 0) //Do we still have any? If so:
                                {
                                    bodyFound = BodiesInView[random.Next(BodiesInView.Count)]; //Randomly pick one.
                                    foundBody = true;
                                    ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_RBodies_00051"), 15f); //#autoLOC_RBodies_00051 = Celestial Body Discovered!
                                    difficulty = startingdifficulty; //Reset the difficulty factor to the starting factor now that we found something.
                                    ResearchBodiesController.FoundBody(scienceReward, bodyFound, out withParent, out parentBody);
                                }
                            }
                        }
                        else
                        {
                            //We didn't find anything. Reduce the difficulty by one until we do.
                            foundBody = false;
                            if (randomMax > 2)
                            {
                                difficulty--;
                            }
                        }
                    }
                    else  // There wasn't enough EC!
                    {
                        ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_RBodies_00048"), 5.0f, ScreenMessageStyle.UPPER_CENTER); //#autoLOC_RBodies_00048 = Not enough Electric Charge to operate the telescope.
                    }
                }
                
            } //endif button
            
            //Populate RB GUI if found.
            if (searchButtonDisplay)
            {
                if (Planetarium.GetUniversalTime() - searchButtonDisplayTimer > 60)
                {
                    searchButtonDisplayTimer = 0;
                    searchButtonDisplay = false;
                }
                //Populate text box on discovery.
                if (foundBody)  //Did we end up finding anything?
                {
                    if (withParent) //And was a parent body also discovered?
                    {
                        GUILayout.Label(Localizer.Format("#autoLOC_RBodies_discovery_" + bodyFound.bodyName));
                        GUILayout.Label(Localizer.Format("#autoLOC_RBodies_discovery_" + parentBody.bodyName));
                    }
                    else
                    {
                        GUILayout.Label(Localizer.Format("#autoLOC_RBodies_discovery_" + bodyFound.bodyName));
                    }
                }
                else  //Nope, didn't find anything.
                {
                    if (foundBodyTooWeak) //Was there something just out of telescope range?
                    {
                        GUILayout.Label(Localizer.Format("#autoLOC_RBodies_00023"), HighLogic.Skin.label); //#autoLOC_RBodies_00023 = A faint signal has been detected but we need a more powerful Telescope.
                    }
                    else  //Nope there was absolutely nothing to see here.
                    {
                        GUILayout.Label(Localizer.Format(nothing), HighLogic.Skin.label);
                    }
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public override string GetInfo()
        {
            if (string.IsNullOrEmpty(cacheautoLOC_RBodies_00093))
            {
                cacheLocalStrings();
            }
            string sb = "";
            sb += cacheautoLOC_RBodies_00093 + "\n";
            sb += Localizer.Format("#autoLOC_RBodies_00094", maxTrackDistance.ToString("F0")) + "\n"; //#autoLOC_RBodies_00094 = Range: <<1>>m
            sb += Localizer.Format("#autoLOC_RBodies_00095", electricChargeRequest.ToString("F0")); //#autoLOC_RBodies_00095 = EC per scan: <<1>>		
            return sb;
        }

        internal bool IsViewObstructed(Transform Origin, Transform Target)
        {
            float distance = Vector3.Distance(Target.position, Origin.position);
            RaycastHit[] hitInfo;
            Vector3 direction = (Target.position - Origin.position).normalized;
            if (RSTLogWriter.debuggingOn)
            {
                Debug.DrawRay(Origin.position, direction*distance, Color.red, 5f);
            }
            hitInfo = Physics.RaycastAll(new Ray(Origin.position, direction), distance, 3245585, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitInfo.Length; i++)
            {
                if (hitInfo[i].transform.name != Target.transform.name)
                {
                    RSTLogWriter.Log_Debug("View Obstructed by {0} , Origin: {1} , Target {2} , Direction {3} , Hit: {4}, Layer: {5}",
                    hitInfo[i].collider.name, Origin.position, Target.position, direction, hitInfo[i].transform.position, hitInfo[i].collider.gameObject.layer);
                    return true;
                }
            }

            RSTLogWriter.Log_Debug("No View obstruction");
            return false;
        }

        #region Localization Tag cache

        private static string cacheautoLOC_RBodies_00093;

        private void cacheLocalStrings()
        {
            cacheautoLOC_RBodies_00093 = Localizer.Format("#autoLOC_RBodies_00093"); // cacheautoLOC_RBodies_00093 = Infrared Telescope

        }

        #endregion
    }
}
