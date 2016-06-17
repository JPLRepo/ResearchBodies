using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Contracts;
using KSP.UI.Screens;
using RSTUtils;

namespace ResearchBodies
{
    //[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public partial class ResearchBodiesController : MonoBehaviour
    {
        /// <summary>
        /// A dictionary that records if bodies have been tracked using telescope
        /// </summary>
        public static Dictionary<CelestialBody, bool> TrackedBodies = new Dictionary<CelestialBody, bool>();
        /// <summary>
        /// A dictionary that returns the research state of a body in %
        /// </summary>
        public static Dictionary<CelestialBody, int> ResearchState = new Dictionary<CelestialBody, int>();

        //private ApplicationLauncherButton appButton = null;
        // float level;  level 1 = 0 , level 2 = 0.5 , level 3 (max) = 1
        
        public static float ResearchCost = 10f, ProgressResearchCost = 5f, ScienceReward = 5f;

        public static List<CelestialBody> BodyList = new List<CelestialBody>();

        /// <summary>
        /// Tarsier Space Tech Interface fields
        /// </summary>
        internal bool isTSTInstalled = false;
        internal bool isPCBMInstalled = false;

        internal static int toolbar = 1;
        internal static string[] toolStrings = new string[] { Locales.currentLocale.Values["start_easy"], Locales.currentLocale.Values["start_normal"], Locales.currentLocale.Values["start_medium"], Locales.currentLocale.Values["start_hard"] };

        public static ResearchBodiesController instance;

        public void LoadConfig()
        {
            if (!File.Exists("saves/" + HighLogic.SaveFolder + "/researchbodies.cfg"))
            {
                ConfigNode file = new ConfigNode();
                ConfigNode node = file.AddNode("RESEARCHBODIES");

                BodyList = FlightGlobals.Bodies.ToList(); 
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

                BodyList = FlightGlobals.Bodies.ToList();           
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
                            if (node.HasValue("ignore"))
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
                            }
                            else
                            {
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
                        newNodeForCB.AddValue("ignore", "false");
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
            SetBodyDiscoveryLevels();
        }
        
        public void Awake()
        {
            instance = this;
            _startwindowId = Utilities.getnextrandomInt();
            _hoverwindowId = Utilities.getnextrandomInt();
            _RBwindowId = Utilities.getnextrandomInt();
            _settingswindowId = Utilities.getnextrandomInt();

            RBMenuAppLToolBar = new AppLauncherToolBar("ResearchBodies", "ResearchBodies",
                Textures.PathToolbarIconsPath + "/RBToolBaricon",
                ApplicationLauncher.AppScenes.SPACECENTER,
                (Texture)Textures.ApplauncherIcon, (Texture)Textures.ApplauncherIcon,
                GameScenes.SPACECENTER);
        }

        public void Start()
        {
            isTSTInstalled = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "TarsierSpaceTech");
            if (isTSTInstalled)  //If TST assembly is present, initialise TST wrapper.
            {
                if (!TSTWrapper.InitTSTWrapper())
                {
                    isTSTInstalled = false; //If the initialise of wrapper failed set bool to false, we won't be interfacing to TST today.
                }
            }

            isPCBMInstalled = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "ProgressiveCBMaps");
            if (isPCBMInstalled)  //If Progressive CB Maps assembly is present, initialise PCBM wrapper.
            {
                if (!PCBMWrapper.APIPCBMReady)
                {
                    isPCBMInstalled = false; //If the initialise of wrapper failed set bool to false, we won't be interfacing to PCBM today.
                }
            }

            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                enable = false;
            //LoadConfig();
            
            //Create Instructor
            Resources.FindObjectsOfTypeAll<KerbalInstructor>()
                .ToList()
                .ForEach(instructor => print("Instructor: " + instructor.CharacterName + ", prefab: " + instructor.name));

            _instructor = Create("Instructor_Wernher");
            
            //Register for Contract On offerred so we can remove ones that are for bodies not yet tracked.
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                GameEvents.Contract.onOffered.Add(CheckContracts);

            //If RB is enabled set initial Discovery Levels of CBs and call ProgressiveCBMaps to set their graphics levels.
            if (enable)
            {
                SetBodyDiscoveryLevels();
                
                if (!ToolbarManager.ToolbarAvailable && !Database.UseAppLauncher)
                {
                    Database.UseAppLauncher = true;
                }
                RBMenuAppLToolBar.Start(Database.UseAppLauncher);
                GameEvents.onGUIRnDComplexSpawn.Add(TurnUIOff);
                GameEvents.onGUIMissionControlSpawn.Add(TurnUIOff);
                GameEvents.onGUIAstronautComplexSpawn.Add(TurnUIOff);
                GameEvents.onGUIAdministrationFacilitySpawn.Add(TurnUIOff);
                GameEvents.onGUIRnDComplexDespawn.Add(TurnUIOn);
                GameEvents.onGUIMissionControlDespawn.Add(TurnUIOn);
                GameEvents.onGUIAstronautComplexDespawn.Add(TurnUIOn);
                GameEvents.onGUIAdministrationFacilityDespawn.Add(TurnUIOn);
                Utilities.setScaledScreen();
            }
        }

        public void OnDestroy()
        {
            if (_portrait != null)
                _portrait.Release();

            if (_instructor != null)
                Destroy(_instructor.gameObject);
            if (enable)
                RBMenuAppLToolBar.Destroy();
            //ApplicationLauncher.Instance.RemoveModApplication(appButton);
            GameEvents.onGUIRnDComplexDespawn.Remove(TurnUIOff);
            GameEvents.onGUIMissionControlDespawn.Remove(TurnUIOff);
            GameEvents.onGUIAstronautComplexSpawn.Remove(TurnUIOff);
            GameEvents.onGUIAdministrationFacilitySpawn.Remove(TurnUIOff);
            GameEvents.onGUIRnDComplexDespawn.Remove(TurnUIOn);
            GameEvents.onGUIMissionControlDespawn.Remove(TurnUIOn);
            GameEvents.onGUIAstronautComplexDespawn.Remove(TurnUIOn);
            GameEvents.onGUIAdministrationFacilityDespawn.Remove(TurnUIOn);
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                GameEvents.Contract.onOffered.Remove(CheckContracts);
        }
        
        /// <summary>
        /// Called by GameEvent onOffered. Will check contract parameters for reference to untracked bodies.
        /// If it finds a reference it will Withdraw the contract.
        /// </summary>
        /// <param name="contract"></param>
        private void CheckContracts(Contract contract)
        {
            
            foreach (ContractParameter cp in contract.AllParameters.ToList())
            {

                foreach (CelestialBody body in BodyList) //.Where(b => !TrackedBodies[b]))
                {
                    if (Database.CelestialBodies.ContainsKey(body))
                    {
                        if (!TrackedBodies[body] && cp.Title.Contains(body.GetName()))
                        {
                            TryWithDrawContract(contract);
                            break;
                        }
                    }
                }
            }
            
        }
        private void TryWithDrawContract(Contract c)
        {
            try
            {
                RSTLogWriter.Log("WithDrew contract \"{0}\"" , c.Title);
                c.Withdraw(); //Changed to Withdraw - this will not penalize reputation.
            }
            catch (Exception e)
            {
                RSTLogWriter.Log("Unable to Withraw contract ! {0}" , e);
            }
        }

        /// <summary>
        /// Returns True if the Trackstation is Level 1 otherwise False.
        /// </summary>
        public bool IsTSlevel1
        {
            get { return PSystemSetup.Instance.GetSpaceCenterFacility("TrackingStation").GetFacilityLevel() < 0.5; }
        }

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
                RSTLogWriter.Log(string.Format(Locales.currentLocale.Values["launchPlan_alreadyStarted"], cb.GetName()));
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
                RSTLogWriter.Log(string.Format(Locales.currentLocale.Values["stopPlan_hasntBeenStarted"], cb.GetName()));
        }

        /// <summary>
        /// Set Discovery Levels of the Bodies
        /// None = 0, Presence = 1 (Object has been detected in tracking station), Name = 4 (Object has been tracked), StateVectors = 8 (Object is currently tracked),
        /// Appearance = 16 (Unlocks mass and type fields; intended for discoverable CelestialBodies?)
        /// </summary>
        public static void SetBodyDiscoveryLevels()
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
                        try
                        {
                            if (Database.CelestialBodies[cb.referenceBody].KOPbarycenter)
                                cb.referenceBody.DiscoveryInfo.SetLevel(DiscoveryLevels.Appearance);
                            if (Database.CelestialBodies[cb].KOPrelbarycenterBody != null)
                                Database.CelestialBodies[cb].KOPrelbarycenterBody.DiscoveryInfo.SetLevel(DiscoveryLevels.Appearance);
                        }
                        catch (Exception)
                        {// throw;
                        }
                        // cb.SetResourceMap(null);
                    }
                    else
                    {
                        cb.DiscoveryInfo.SetLevel(DiscoveryLevels.Owned);
                        try
                        {
                            if (Database.CelestialBodies[cb.referenceBody].KOPbarycenter)
                                cb.referenceBody.DiscoveryInfo.SetLevel(DiscoveryLevels.Owned);
                            if (Database.CelestialBodies[cb].KOPrelbarycenterBody != null)
                                Database.CelestialBodies[cb].KOPrelbarycenterBody.DiscoveryInfo.SetLevel(DiscoveryLevels.Owned);
                        }
                        catch (Exception)
                        {// throw;
                        }
                    }
                    
                }
            }
        }

        /// <summary>
        /// Set Body Graphics levels in ProgressiveCBMaps
        /// </summary>
        public void SetBodyProgressiveCBMaps()
        {
            
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
