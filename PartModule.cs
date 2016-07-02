using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using RSTUtils;
using RSTUtils.Extensions;

namespace ResearchBodies
{
    public class ModuleTrackBodies : PartModule
    {
        private bool enable = true, showGUI = false, foundBody = false, withParent = false, canResearch = true;
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


        public override void OnAwake()
        {
            base.OnAwake();            
            if (HighLogic.LoadedScene != GameScenes.LOADING && HighLogic.LoadedScene != GameScenes.LOADINGBUFFER)
            {
                if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX && !Database.instance.enableInSandbox)
                    enable = false;
                _partwindowID = Utilities.getnextrandomInt();
                startingdifficulty = difficulty;
            }
        }

        public void OnGUI()
        {
            if (showGUI)
            {
                GUI.skin = HighLogic.Skin;
                windowRect.ClampToScreen();
                windowRect = GUILayout.Window(_partwindowID, windowRect, DrawWindow, Locales.currentLocale.Values["telescope_trackBodies"]);
            }
        }

        [KSPEvent(guiName = "Research Bodies", guiActiveEditor = false, guiActive = true)]
        public void ToggleGUI()
        {
            if (!this.vessel.Landed && this.vessel.atmDensity < 0.1 && this.vessel.altitude > minAltitude)
            {
                showGUI = !showGUI;
            }
            else
                ScreenMessages.PostScreenMessage(string.Format(Locales.currentLocale.Values["telescope_mustBeInSpace"], minAltitude), 3.0f, ScreenMessageStyle.UPPER_CENTER);
        }

        void DrawWindow(int windowID)
        {
            GUIContent closeContent = new GUIContent(Textures.BtnRedCross, "Close Window");
            Rect closeRect = new Rect(windowRect.width - 21, 4, 16, 16);
            if (GUI.Button(closeRect, closeContent, Textures.ClosebtnStyle))
            {
                showGUI = !showGUI;
                return;
            }
            GUILayout.BeginVertical();
            scrollViewBodiesVector = GUILayout.BeginScrollView(scrollViewBodiesVector, GUILayout.MaxHeight(200f));
            GUILayout.BeginVertical();
            GUILayout.Label(Locales.currentLocale.Values["start_availableBodies"], Textures.sectionTitleStyle);
            foreach (var body in Database.instance.CelestialBodies)
            {
                if (body.Value.isResearched)
                {
                    GUILayout.Label(body.Key.GetName(), Textures.PartListPartStyle);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
            GUILayout.BeginVertical();
            GUILayout.Label(string.Format(Locales.currentLocale.Values["telescope_trackBodies_EC"], electricChargeRequest));
            if (GUILayout.Button(Locales.currentLocale.Values["telescope_trackBodies"]))
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
                        ScreenMessages.PostScreenMessage(string.Format(Locales.currentLocale.Values["telescope_mustHavePart"], requiredPart), 3.0f, ScreenMessageStyle.UPPER_CENTER);
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
                                        BodiesInView.Add(body);  //We got one!
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
                                    ScreenMessages.PostScreenMessage("Celestial Body Discovered !", 5f);
                                    difficulty = startingdifficulty; //Reset the difficulty factor to the starting factor now that we found something.
                                    if (HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)  //If not sandbox add the Science Points reward!
                                    {
                                        var sciencePtsReward = scienceReward + ResearchBodies.Instance.RBgameSettings.ScienceReward;
                                        ResearchAndDevelopment.Instance.AddScience(sciencePtsReward, TransactionReasons.None);
                                        ScreenMessages.PostScreenMessage("Added " + sciencePtsReward + " science points !", 5f);
                                    }
                                    //Check if the referencebody is also not known. If so, we discover both the body and it's referencebody (parent).
                                    if (bodyFound.referenceBody.DiscoveryInfo.Level == DiscoveryLevels.Presence)
                                    {
                                        if (Database.instance.CelestialBodies.ContainsKey(bodyFound.referenceBody.referenceBody))
                                            Database.instance.CelestialBodies[bodyFound.referenceBody].isResearched = true;
                                        if (Database.instance.CelestialBodies.ContainsKey(bodyFound))
                                            Database.instance.CelestialBodies[bodyFound].isResearched = true;
                                        withParent = true;
                                        parentBody = bodyFound.referenceBody;
                                        //check for Barycenter as well.
                                        if (bodyFound.referenceBody.referenceBody != null)
                                        {
                                            if ((bodyFound.referenceBody.referenceBody.DiscoveryInfo.Level == DiscoveryLevels.Appearance 
                                                || bodyFound.referenceBody.referenceBody.DiscoveryInfo.Level == DiscoveryLevels.Presence) &&
                                                Database.instance.CelestialBodies.ContainsKey(bodyFound.referenceBody.referenceBody))
                                            {
                                                Database.instance.CelestialBodies[bodyFound.referenceBody.referenceBody].isResearched = true;
                                            }
                                        }
                                        RSTLogWriter.Log("Found body {0} orbiting around {1} !", bodyFound.GetName(),bodyFound.referenceBody.GetName());
                                    }
                                    else //No parent or parent is already discovered. So we just found this body.
                                    {
                                        if (Database.instance.CelestialBodies.ContainsKey(bodyFound))
                                            Database.instance.CelestialBodies[bodyFound].isResearched = true;
                                        withParent = false;
                                        RSTLogWriter.Log("Found body {0} !", bodyFound.GetName());
                                    }
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
                        ScreenMessages.PostScreenMessage(string.Format(Locales.currentLocale.Values["ec_notEnough"]), 3.0f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                
            } //endif button
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
                        GUILayout.Label(Database.instance.CelestialBodies[bodyFound].discoveryMessage);
                        GUILayout.Label (Database.instance.CelestialBodies[parentBody].discoveryMessage);
                    }
                    else
                    {
                        GUILayout.Label(Database.instance.CelestialBodies[bodyFound].discoveryMessage);
                    }
                }
                else  //Nope, didn't find anything.
                {
                    if (foundBodyTooWeak) //Was there something just out of telescope range?
                    {
                        GUILayout.Label(Locales.currentLocale.Values["telescope_weaksignal"], HighLogic.Skin.label);
                    }
                    else  //Nope there was absolutely nothing to see here.
                    {
                        GUILayout.Label(nothing, HighLogic.Skin.label);
                    }
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
