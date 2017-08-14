/**
* REPOSoftTech 
* (C) Copyright 2016, Jamie Leighton 
* License : All Rights Reserved. 
* This Code file/Class is All Rights Reserved until further notice.
* Original code was developed by 
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using RSTUtils;
using KSP.UI;
using KSP.UI.Screens.SpaceCenter;
using KSPAssets;
using KSPAssets.Loaders;


namespace ResearchBodies
{
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, true)]
    public class ResearchBodies_Observatory : MonoBehaviour
    {
        public static bool SpaceCenterFacilitySetupSuccess = false;
        public static bool SpaceCenterFacilitySpawned = false;
        public static ResearchBodies_Observatory Instance;
        public static Observatory SpaceCenterObservatory;
        public string facilityName = "Observatory";
        
        private PSystemSetup.SpaceCenterFacility trackingstation;
        private PSystemSetup.SpaceCenterFacility observatoryspacecenterfacility;
        private SpaceCenterBuilding[] buildings;
        
        internal static Upgradeables.UpgradeableFacility upgradeablefacility;
        private Upgradeables.UpgradeableFacility[] upgradeablefacilities;
        private Upgradeables.UpgradeableFacility ResearchAndDevUpgradable;
        internal Transform KSCtransform;
        private Transform trackingstationtransform;
        internal static Transform spacecentertransform;
        private GameObject ObservatoryGo;
        private DestructibleBuilding[] destructibles;
        private PQSCity[] pqscities;
        private GameObject ObservLevel0Prefab;
        private GameObject ObservLevel1Prefab;
        private bool ObservLevel0PrefabLoaded = false;
        private bool ObservLevel1PrefabLoaded = false;
        private Transform WreckTransform;
        private NestedPrefabSpawner[] prefabspawner;
        private NestedPrefabSpawner level0prefabspawner;
        private NestedPrefabSpawner level1prefabspawner;
        private DestructibleBuilding newlvl0destructible;
        private DestructibleBuilding newlvl1destructible;
        private Vector3 groundBaseOffset = new Vector3(15f, -9f, -10f); //new Vector3(0f, 0f, 0f);//new Vector3(15f, -9f, -10f);
        private float Observatorylvl1Range = 0f;
        private float Observatorylvl2Range = 0f;
        private bool lvlStatsNotSet = true;

        /// <summary>
        /// Check and establish singleton.
        /// Setup the SpaceCenterFacility at PsystemSpawn.
        /// Create GameEvent call backs for post PsystemSpawn and Whenever a GameScene is loaded.
        /// </summary>
        public void Awake()
        {
            //Check singleton. Destroy any usurper.
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);

            RSTLogWriter.Log("Observatory Awake - Construction Begins");
            //We only have to attach to the PsystemSetup ONCE.
            if (!SpaceCenterFacilitySetupSuccess)
            {
                SetupSpaceCenterFacility(facilityName);
            }

            pqscities = Resources.FindObjectsOfTypeAll<PQSCity>();
            upgradeablefacilities = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableFacility>();
            destructibles = Resources.FindObjectsOfTypeAll<DestructibleBuilding>();
            prefabspawner = Resources.FindObjectsOfTypeAll<NestedPrefabSpawner>();
            // Add a handler so that we can do post spawn.  
            PSystemManager.Instance.OnPSystemReady.Add(PostPsystemSpawn);
            //PostPsystemSpawn();
            // Add a handler for when Game Scenes are changed.
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
        }

        /// <summary>
        /// Creates a new PSystemSetup.SpaceCenterFacility and embeds it into PsystemSetup.facilities.
        /// </summary>
        /// <param name="FacilityName">String containing the name of our new facility</param>
        public void SetupSpaceCenterFacility(string FacilityName)
        {
            try
            {
                //First we get the facilities from PSystemSetup 
                PSystemSetup.SpaceCenterFacility[] facilities = PSystemSetup.Instance.SpaceCenterFacilities;
                //dumpPQSCities();
                // Now we find the TrackingStation facility as we are going to use it's pqsName for our new facility.
                trackingstation = PSystemSetup.Instance.GetSpaceCenterFacility("TrackingStation");
                
                //Now create a new SpaceCenterFacility and set it up
                observatoryspacecenterfacility = new PSystemSetup.SpaceCenterFacility();
                observatoryspacecenterfacility.name = FacilityName;
                observatoryspacecenterfacility.facilityName = "";
                observatoryspacecenterfacility.facilityTransformName = "KSC/SpaceCenter/" + FacilityName; //We need to create this transform which we do later.
                observatoryspacecenterfacility.pqsName = trackingstation.pqsName; //and the pqsName from the trackingstation.
                observatoryspacecenterfacility.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[0];
            
                //copy in our new SpaceCenterFacility into the facilities
                int numfacilities = facilities.Length;
                PSystemSetup.SpaceCenterFacility[] newFacilities = new PSystemSetup.SpaceCenterFacility[numfacilities + 1];
                for (int i = 0; i < numfacilities; ++i)
                {
                    newFacilities[i] = facilities[i];
                }
                newFacilities[numfacilities] = observatoryspacecenterfacility;
                PSystemSetup.Instance.SpaceCenterFacilities = newFacilities;
                SpaceCenterFacilitySetupSuccess = true;
            }
            catch (Exception ex)
            {
                //error
                Debug.Log("Failed to setup SpaceCenterFacility.");
                Debug.Log(ex.Message);
            }
        }

        /// <summary>
        /// Post Psystem Spawn point. So all the PQS should now exist.
        /// Here we will Instantiate our new UpgradeableFacility for the Observatory.
        /// We also setup the prefab for level 0 and level 1.
        /// Having this here may cause issues for Mods that have multiple Launch Sites. But probably we need to do more for those mods anyway.
        /// </summary>
        public void PostPsystemSpawn()
        {

            try
            {
                RSTLogWriter.Log("Observatory Construction - Post Psystem Spawn Point Begins");
                if (!SpaceCenterFacilitySetupSuccess)
                {
                    RSTLogWriter.Log("Failed to Inject Observatory SpaceCenter Facility cannot continue setup of this Facility");
                    return;
                }
                GetFacilityStrings();
                GameObject tempGo = new GameObject();
                pqscities = Resources.FindObjectsOfTypeAll<PQSCity>();
                for (int i = 0; i < pqscities.Length; i++)
                {
                    if (pqscities[i].name == "KSC") //This should be the home PQS.
                    {
                        //Find the SpaceCenter Transform.
                        //Find the TrackingStation Transform as we are going to use some of it's values.
                        KSCtransform = pqscities[i].gameObject.transform;
                        spacecentertransform = pqscities[i].gameObject.transform.Find("SpaceCenter");
                        trackingstationtransform = pqscities[i].gameObject.transform.Find("SpaceCenter/TrackingStation");
                        break;
                    }
                }
                if (KSCtransform == null)
                {
                    RSTLogWriter.Log("Failed to find KSC Transform cannot continue creating Facility");
                    return;
                }
                if (spacecentertransform == null)
                {
                    RSTLogWriter.Log("Failed to find SpaceCenter Transform cannot continue creating Facility");
                    return;
                }
                if (trackingstationtransform == null)
                {
                    RSTLogWriter.Log("Failed to find TrackingStation Transform cannot continue creating Facility");
                    return;
                }
                //Get all the UpgradeableFacility objects.
                upgradeablefacilities = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableFacility>();
                //Find the ResearchAndDevelopment object and copy it to a new UpgradeableFacility for the Observatory.
                for (int j = 0; j < upgradeablefacilities.Length; j++)
                {

                    if (upgradeablefacilities[j].name == "ResearchAndDevelopment")
                    {
                        ResearchAndDevUpgradable = upgradeablefacilities[j];
                        //print("R&D upgradible level 2 dump");
                        //Utilities.DumpGameObjectHierarchy(ResearchAndDevUpgradable.UpgradeLevels[2].facilityPrefab);
                        break;
                    }
                }
                //IF we didn't find it, we can't continue.
                if (ResearchAndDevUpgradable == null)
                {
                    RSTLogWriter.Log("Failed to find ResearchAndDevelopment Upgradable cannot continue creating Facility");
                    return;
                }

                //Copy The R&D UpgradeableFacility to a new UpgradeableFacility for the Observatory.
                //Remove all the upgrade levels, we just want the last one which has the observatory.
                //Then remove all the components that aren't part of the Observatory from the prefab.//Instantiate our new Facility.
                upgradeablefacility = Instantiate(ResearchAndDevUpgradable, spacecentertransform.position,trackingstationtransform.rotation) as Upgradeables.UpgradeableFacility;
                //upgradeablefacility = new Upgradeables.UpgradeableFacility();
                upgradeablefacility.name = facilityName;
                upgradeablefacility.id = "SpaceCenter/" + facilityName;
                upgradeablefacility.transform.NestToParent(spacecentertransform);
                //Using the spacecentertransform position, re-position our Observatory location.
                //Vector3 newglobalposition = new Vector3(121.125f, -214.7833f, 217.875f);
                //upgradeablefacility.transform.position = newglobalposition;
                Vector3 newposition = new Vector3(-246.6104f, 24.34455f, -216.4676f);
                //newposition.x = -245.2505f; //-214.1f; //+= 222.3579f;// 147f;//144f;//150f;//135f;//145f; //160f; //140f; 
                //newposition.y = -24.37455f; // 14.8f; //-= 24.46509f;
                //newposition.z = -219.6775f;// -261.9f; //+= 237.8429f;//37.5f;//38f;//33f; //63f; 
                upgradeablefacility.transform.localPosition = newposition;
                upgradeablefacility.transform.rotation = trackingstationtransform.rotation;
                tempGo.transform.position = upgradeablefacility.transform.position;
                tempGo.transform.rotation = upgradeablefacility.transform.rotation;
                tempGo.transform.localPosition = upgradeablefacility.transform.localPosition;

                //Create a New Array for Upgrade Levels - Two Levels for now.
                Upgradeables.UpgradeableObject.UpgradeLevel[] newupgradelevels = new Upgradeables.UpgradeableObject.UpgradeLevel[2];

                //Setup Level 0
                newupgradelevels[0] = new Upgradeables.UpgradeableObject.UpgradeLevel();

                //Get the Prefab model for our Observatory Level0
                //ObservLevel1Prefab = GameDatabase.Instance.GetModel("Assets/RB_observatory.prefab");
                ObservLevel0Prefab = GameDatabase.Instance.GetModel("REPOSoftTech/ResearchBodies/Assets/RB_Observatorylvl1");
                DontDestroyOnLoad(ObservLevel0Prefab);
                if (ObservLevel0Prefab == null)
                {
                    RSTLogWriter.Log("Failed to Load Observatory Level1 Prefab Model");
                    //Just use the level 1 prefab model
                    newupgradelevels[0].facilityPrefab = Instantiate(ResearchAndDevUpgradable.UpgradeLevels[2].facilityPrefab, upgradeablefacility.transform.position, upgradeablefacility.transform.rotation) as GameObject;
                    cleanStockObservatory(newupgradelevels[0].facilityPrefab, newposition);
                }
                else
                {
                    //ObservLevel0Prefab.tag = "RB_Obsvtry";
                    newupgradelevels[0].facilityPrefab = Instantiate(ObservLevel0Prefab.gameObject, upgradeablefacility.transform.position, upgradeablefacility.transform.rotation) as GameObject;
                    //Offset Observatory Level 0 prefab onto the concrete slab base
                    newupgradelevels[0].facilityPrefab.transform.position = upgradeablefacility.transform.position;
                    newupgradelevels[0].facilityPrefab.transform.rotation = upgradeablefacility.transform.rotation;
                    newupgradelevels[0].facilityPrefab.transform.localPosition = upgradeablefacility.transform.position;
                    newupgradelevels[0].facilityPrefab.transform.localRotation = upgradeablefacility.transform.rotation;
                    var targetBuildingTransform = newupgradelevels[0].facilityPrefab.transform.FindChild("RB_observatory");
                    targetBuildingTransform.localPosition = new Vector3(21.03f, 0.37f, 4.35f);
                    Quaternion rotation = Quaternion.Euler(0f, 248.023f, 0f);
                    targetBuildingTransform.localRotation = rotation;
                    //newupgradelevels[0].facilityPrefab.tag = "KSC_RB_Obs";
                    newupgradelevels[0].facilityPrefab.AddComponentWithInit<CrashObjectName>(f =>
                    {
                        f.name = facilityName;
                        f.objectName = facilityName;
                    });
                    ObservLevel0PrefabLoaded = true;
                }
                
                //Setup Level 0
                newupgradelevels[0].facilityPrefab.name = "KSC_Observatory_Lvl0";
                newupgradelevels[0].levelCost = 30000f;
                newupgradelevels[0].levelText = ScriptableObject.CreateInstance<KSCUpgradeableLevelText>();// new KSCUpgradeableLevelText();
                newupgradelevels[0].levelText.facility = SpaceCenterFacility.TrackingStation; //We can't extend the enum
                newupgradelevels[0].levelText.textBase = Locales.FmtLocaleString("#autoLOC_RBodies_00097") + "\n";
                string rngString1 = KSP.Localization.Localizer.Format("<<1>>", Observatorylvl1Range.ToString("F0"));
                newupgradelevels[0].levelText.textBase += Locales.FmtLocaleString("#autoLOC_RBodies_00098", rngString1);
                newupgradelevels[0].levelText.linePrefix = "* ";
                newupgradelevels[0].levelStats = new Upgradeables.KSCFacilityLevelText();
                newupgradelevels[0].levelStats.linePrefix = newupgradelevels[0].levelText.linePrefix;
                newupgradelevels[0].levelStats.facility = SpaceCenterFacility.TrackingStation;
                newupgradelevels[0].levelStats.textBase = newupgradelevels[0].levelText.textBase;
                DontDestroyOnLoad(newupgradelevels[0].facilityPrefab);

                //Setup Level 1
                newupgradelevels[1] = new Upgradeables.UpgradeableObject.UpgradeLevel();

                //Get the Prefab model for our Observatory Level1
                //ObservLevel1Prefab = GameDatabase.Instance.GetModel("Assets/RB_observatory.prefab");
                ObservLevel1Prefab = GameDatabase.Instance.GetModel("REPOSoftTech/ResearchBodies/Assets/RB_Observatorylvl2");
                DontDestroyOnLoad(ObservLevel1Prefab);
                if (ObservLevel1Prefab == null)
                {
                    RSTLogWriter.Log("Failed to Load Observatory Level2 Prefab Model");
                    //Just use the level 1 prefab model
                    newupgradelevels[1].facilityPrefab = Instantiate(ResearchAndDevUpgradable.UpgradeLevels[2].facilityPrefab, upgradeablefacility.transform.position, upgradeablefacility.transform.rotation) as GameObject;
                    cleanStockObservatory(newupgradelevels[1].facilityPrefab, newposition);
                }
                else
                {
                    //ObservLevel1Prefab.tag = "RB_Obsvtry";
                    newupgradelevels[1].facilityPrefab = Instantiate(ObservLevel1Prefab.gameObject, upgradeablefacility.transform.position, upgradeablefacility.transform.rotation) as GameObject;
                    //Offset Observatory Level 0 prefab onto the concrete slab base
                    newupgradelevels[1].facilityPrefab.transform.position = upgradeablefacility.transform.position;
                    newupgradelevels[1].facilityPrefab.transform.rotation = upgradeablefacility.transform.rotation;
                    newupgradelevels[1].facilityPrefab.transform.localPosition = upgradeablefacility.transform.localPosition;
                    newupgradelevels[1].facilityPrefab.transform.localRotation = upgradeablefacility.transform.rotation;
                    var targetBuildingTransform = newupgradelevels[1].facilityPrefab.transform.FindChild("RB_observatory");
                    targetBuildingTransform.localPosition = new Vector3(26.25f, 0.39f, 7.93f);
                    Quaternion rotation = Quaternion.Euler(0f, -111.97f, 0f);
                    targetBuildingTransform.localRotation = rotation;
                    //newupgradelevels[1].facilityPrefab.tag = "KSC_RB_Obs";
                    newupgradelevels[1].facilityPrefab.AddComponentWithInit<CrashObjectName>(f =>
                    {
                        f.name = facilityName;
                        f.objectName = facilityName;
                    });
                    ObservLevel1PrefabLoaded = true;
                }

                //Setup Level 1
                newupgradelevels[1].facilityPrefab.name = "KSC_Observatory_Lvl1";
                newupgradelevels[1].levelCost = 300000f;
                newupgradelevels[1].levelText = ScriptableObject.CreateInstance<KSCUpgradeableLevelText>();// new KSCUpgradeableLevelText();
                newupgradelevels[1].levelText.facility = SpaceCenterFacility.TrackingStation; //We can't extend the enum
                newupgradelevels[1].levelText.textBase = Locales.FmtLocaleString("#autoLOC_RBodies_00097") + "\n";
                string rngString2 = KSP.Localization.Localizer.Format("<<1>>", Observatorylvl2Range.ToString("F0"));
                newupgradelevels[1].levelText.textBase += Locales.FmtLocaleString("#autoLOC_RBodies_00098", rngString2);
                newupgradelevels[1].levelText.linePrefix = "* ";
                newupgradelevels[1].levelStats = new Upgradeables.KSCFacilityLevelText();
                newupgradelevels[1].levelStats.linePrefix = newupgradelevels[1].levelText.linePrefix;
                newupgradelevels[1].levelStats.facility = SpaceCenterFacility.TrackingStation;
                newupgradelevels[1].levelStats.textBase = newupgradelevels[1].levelText.textBase;
                DontDestroyOnLoad(newupgradelevels[1].facilityPrefab);
                /*
                //Setup Level 2
                newupgradelevels[2] = new Upgradeables.UpgradeableObject.UpgradeLevel();
                //Instantiate a copy of the R&D prefab (because we want to borrow the Observatory Meshes from it.
                newupgradelevels[2].facilityPrefab = Instantiate(ResearchAndDevUpgradable.UpgradeLevels[2].facilityPrefab, upgradeablefacility.transform.position, upgradeablefacility.transform.rotation) as GameObject;
                //Now go through our new level 2 prefab and Destroy everything we don't want. So we are only keeping the Observatory and MarkerAnchor GameObjects.
                cleanStockObservatory(newupgradelevels[2].facilityPrefab, newposition);
                newupgradelevels[2].facilityPrefab.transform.position = upgradeablefacility.transform.position;
                newupgradelevels[2].facilityPrefab.transform.rotation = upgradeablefacility.transform.rotation;
                newupgradelevels[2].facilityPrefab.transform.localPosition = upgradeablefacility.transform.localPosition;
                newupgradelevels[2].facilityPrefab.tag = "KSC_RB_Obs";
                newupgradelevels[2].facilityPrefab.name = "KSC_Observatory_Lvl2";
                newupgradelevels[2].levelCost = 600000f;
                newupgradelevels[2].levelText = ScriptableObject.CreateInstance<KSCUpgradeableLevelText>();// new KSCUpgradeableLevelText();
                newupgradelevels[2].levelText.facility = SpaceCenterFacility.TrackingStation; //We can't extend the enum
                newupgradelevels[2].levelText.textBase = "Test string level 2 Observatory";
                newupgradelevels[2].levelText.linePrefix = "* ";
                newupgradelevels[2].levelStats = new Upgradeables.KSCFacilityLevelText();
                newupgradelevels[2].levelStats.linePrefix = newupgradelevels[2].levelText.linePrefix;
                newupgradelevels[2].levelStats.facility = SpaceCenterFacility.TrackingStation;
                newupgradelevels[2].levelStats.textBase = newupgradelevels[2].levelText.textBase;
                DontDestroyOnLoad(newupgradelevels[2].facilityPrefab);
                */
                //Set the UpgradeLevels for our new Facility.
                upgradeablefacility.UpgradeLevels = newupgradelevels;

                /*print("dump newUpgradelevels upgradeablefacility facilityprefab");
                for (int s = 0; s < upgradeablefacility.UpgradeLevels.Length; s++)
                {
                    print("upgrade level " + s);
                    Utilities.DumpGameObjectHierarchy(upgradeablefacility.UpgradeLevels[s].facilityPrefab.gameObject);
                }*/
                var tempfacilityprefab = Instantiate(ResearchAndDevUpgradable.UpgradeLevels[2].facilityPrefab, upgradeablefacility.transform.position, upgradeablefacility.transform.rotation) as GameObject;
                //Now go through our new level 2 prefab and Destroy everything we don't want. So we are only keeping the Observatory and MarkerAnchor GameObjects.
                cleanStockObservatory(tempfacilityprefab, newposition);
                //Now we have to finish setting up the Level0 Prefab copying from the level1 Prefab
                if (ObservLevel0PrefabLoaded)
                {
                    //Copy Destructiblebuilding component from level 2 to level 0 
                    //destructibles = upgradeablefacility.UpgradeLevels[2].facilityPrefab.GetComponentsInChildren<DestructibleBuilding>(true);
                    destructibles = tempfacilityprefab.GetComponentsInChildren<DestructibleBuilding>(true);
                    if (destructibles.Length == 1)
                    {
                        //print("Observatory prefab level 0 destructible dump");
                        //Utilities.DumpGameObjectHierarchy(upgradeablefacility.UpgradeLevels[0].facilityPrefab);
                        //Utilities.DumpGameObjectHierarchy(destructibles[0].gameObject);

                        var targetBuildingTransform = upgradeablefacility.UpgradeLevels[0].facilityPrefab.transform.FindChild("RB_observatory");
                        var collapseObjectReference = upgradeablefacility.UpgradeLevels[0].facilityPrefab.transform.FindChild("RB_observatory/RB_Observatorylvl0");
                        var fxTargetReference = upgradeablefacility.UpgradeLevels[0].facilityPrefab.transform.FindChild("RB_observatory/FxTarget");
                        if (targetBuildingTransform != null && collapseObjectReference != null && fxTargetReference != null)
                        {
                            addDestructibleComponent(destructibles[0], newlvl0destructible, targetBuildingTransform, collapseObjectReference.gameObject, fxTargetReference, tempGo, 20000f);
                        }
                        else
                        {
                            RSTLogWriter.Log("Failed to add DestructibleComponent");
                            if (targetBuildingTransform == null)
                                RSTLogWriter.Log("targetBuildingTransform is not found");
                            if (collapseObjectReference == null)
                                RSTLogWriter.Log("collapseObjectReference is not found");
                            if (fxTargetReference == null)
                                RSTLogWriter.Log("fxTargetReference is not found");
                        }
                    }


                    //Add NestedPrefabSpawner component to wreckSpawner transform.
                    prefabspawner = Resources.FindObjectsOfTypeAll<NestedPrefabSpawner>();
                    for (int x = 0; x < prefabspawner.Length; x++)
                    {
                        if (prefabspawner[x].Prefabs[0].prefab.name == ("Wreck_RubblePileSmall"))
                        {
                            WreckTransform = newupgradelevels[0].facilityPrefab.transform.FindChild("RB_observatory/wreck/wreckSpawner");
                            if (WreckTransform != null)
                            {
                                // WreckTransform.gameObject.AddComponent<NestedPrefabSpawner>(prefabspawner[x]);
                                WreckTransform.gameObject.AddComponent<NestedPrefabSpawner>();
                                level0prefabspawner = WreckTransform.GetComponent<NestedPrefabSpawner>();
                                if (level0prefabspawner != null)
                                {
                                    level0prefabspawner.Prefabs = new List<NestedPrefabSpawner.NestedPrefab>();

                                    var newwreckprefab = Instantiate(prefabspawner[x].Prefabs[0].prefab.gameObject) as GameObject;
                                    var nestedprefab = new NestedPrefabSpawner.NestedPrefab();
                                    nestedprefab.prefab = newwreckprefab;
                                    nestedprefab.tgtTransform = WreckTransform;
                                    level0prefabspawner.Prefabs.Add(nestedprefab);
                                    level0prefabspawner.allowedLayers = prefabspawner[x].allowedLayers;
                                    level0prefabspawner.allowedTags = prefabspawner[x].allowedTags;
                                    level0prefabspawner.startDelay = prefabspawner[x].startDelay;
                                    DontDestroyOnLoad(newwreckprefab);
                                    newwreckprefab.gameObject.transform.NestToParent(tempGo.transform);

                                }
                            }
                            break;
                        }
                    }
                }




                //Now we have to finish setting up the Level0 Prefab copying from the level1 Prefab
                if (ObservLevel1PrefabLoaded)
                {
                    //Copy Destructiblebuilding component from level 2 to level 0
                    destructibles = tempfacilityprefab.GetComponentsInChildren<DestructibleBuilding>(true);
                    if (destructibles.Length == 1)
                    {
                        //print("Observatory prefab level 1 destructible dump");
                        //Utilities.DumpGameObjectHierarchy(destructibles[0].gameObject);
                        //print("Observatory prefab level 0 destructible dump");
                        //Utilities.DumpGameObjectHierarchy(upgradeablefacility.UpgradeLevels[1].facilityPrefab);

                        var targetBuildingTransform = upgradeablefacility.UpgradeLevels[1].facilityPrefab.transform.FindChild("RB_observatory");
                        var collapseObjectReference = upgradeablefacility.UpgradeLevels[1].facilityPrefab.transform.FindChild("RB_observatory/RB_Observatorylvl1");
                        var fxTargetReference = upgradeablefacility.UpgradeLevels[1].facilityPrefab.transform.FindChild("RB_observatory/FxTarget");
                        if (targetBuildingTransform != null && collapseObjectReference != null && fxTargetReference != null)
                        {
                            addDestructibleComponent(destructibles[0], newlvl1destructible, targetBuildingTransform, collapseObjectReference.gameObject, fxTargetReference, tempGo, 200000f);
                        }
                        else
                        {
                            RSTLogWriter.Log("Failed to add DestructibleComponent");
                            if (targetBuildingTransform == null)
                                RSTLogWriter.Log("targetBuildingTransform is not found");
                            if (collapseObjectReference == null)
                                RSTLogWriter.Log("collapseObjectReference is not found");
                            if (fxTargetReference == null)
                                RSTLogWriter.Log("fxTargetReference is not found");
                        }
                    }


                    //Add NestedPrefabSpawner component to wreckSpawner transform.
                    prefabspawner = Resources.FindObjectsOfTypeAll<NestedPrefabSpawner>();
                    for (int x = 0; x < prefabspawner.Length; x++)
                    {
                        if (prefabspawner[x].Prefabs[0].prefab.name == ("Wreck_RubblePile"))
                        {
                            WreckTransform = newupgradelevels[1].facilityPrefab.transform.FindChild("RB_observatory/wreck/wreckSpawner");
                            if (WreckTransform != null)
                            {
                                // WreckTransform.gameObject.AddComponent<NestedPrefabSpawner>(prefabspawner[x]);
                                WreckTransform.gameObject.AddComponent<NestedPrefabSpawner>();
                                level1prefabspawner = WreckTransform.GetComponent<NestedPrefabSpawner>();
                                if (level1prefabspawner != null)
                                {
                                    level1prefabspawner.Prefabs = new List<NestedPrefabSpawner.NestedPrefab>();

                                    var newwreckprefab = Instantiate(prefabspawner[x].Prefabs[0].prefab.gameObject) as GameObject;
                                    var nestedprefab = new NestedPrefabSpawner.NestedPrefab();
                                    nestedprefab.prefab = newwreckprefab;
                                    nestedprefab.tgtTransform = WreckTransform;
                                    level1prefabspawner.Prefabs.Add(nestedprefab);
                                    level1prefabspawner.allowedLayers = prefabspawner[x].allowedLayers;
                                    level1prefabspawner.allowedTags = prefabspawner[x].allowedTags;
                                    level1prefabspawner.startDelay = prefabspawner[x].startDelay;
                                    DontDestroyOnLoad(newwreckprefab);
                                    newwreckprefab.gameObject.transform.NestToParent(tempGo.transform);

                                }
                            }
                            break;
                        }
                    }
                }

                DontDestroyOnLoad(upgradeablefacility);

                //Now we just need a nice new piece of ground Underneath our new Observatory. 
                //It just so happens that the piece of ground under MissionControl building Level 0 fits very nicely.
                //So we find it and add it to our new UpgradeableFacility prefab.
                for (int h = 0; h < upgradeablefacilities.Length; h++)
                {
                    if (upgradeablefacilities[h].name == "MissionControl")
                    {
                        Component[] comp = upgradeablefacilities[h].UpgradeLevels[0].facilityPrefab.GetComponents<Component>();
                        if (comp.Length > 0)
                        {
                            int count = comp[0].transform.childCount;
                            for (int k = count - 1; k >= 0; --k)
                            {
                                GameObject child = comp[0].transform.GetChild(k).gameObject;

                                if (child.tag.Contains("KSC_Mission_Control_Grounds"))
                                {
                                    print("Adding to prefab 0: " + child.name);
                                    addPrefabGroundBase(child,upgradeablefacility.UpgradeLevels[0].facilityPrefab,upgradeablefacility.transform, groundBaseOffset);
                                    print("Adding to prefab 1: " + child.name);
                                    addPrefabGroundBase(child, upgradeablefacility.UpgradeLevels[1].facilityPrefab, upgradeablefacility.transform, groundBaseOffset);
                                    //print("Adding to prefab 2: " + child.name);
                                    //addPrefabGroundBase(child, upgradeablefacility.UpgradeLevels[2].facilityPrefab, upgradeablefacility.transform, groundBaseOffset);
                                }
                            }
                        }
                        break;
                    }
                }


                //We actually want everything we have created that is a prefab to be activeself but not activeinhierarchy. I can't figure out another way to do this.
                //So we set everything to active then set tempGo which is a temporary Go that the prefabs are attached to to not active.
                //This will set all the prefabs to activeself but not activeinhierarchy.
                //Set the new prefab level to active. This sets all the objects to activeinhierarchy and activeself.
                //upgradeablefacility.UpgradeLevels[0].facilityPrefab.SetActive(true);
                //Nest the upgradeables to the tempGo as they are prefabs.
                //upgradeablefacility.UpgradeLevels[0].facilityPrefab.transform.NestToParent(tempGo.transform);
                upgradeablefacility.UpgradeLevels[0].facilityPrefab.transform.SetParent(tempGo.transform);
                upgradeablefacility.UpgradeLevels[0].facilityPrefab.transform.localPosition =
                    upgradeablefacility.transform.localPosition;
                upgradeablefacility.UpgradeLevels[0].facilityPrefab.SetActive(true);
                //upgradeablefacility.UpgradeLevels[1].facilityPrefab.transform.NestToParent(tempGo.transform);
                upgradeablefacility.UpgradeLevels[1].facilityPrefab.transform.SetParent(tempGo.transform);
                upgradeablefacility.UpgradeLevels[1].facilityPrefab.transform.localPosition =
                    upgradeablefacility.transform.localPosition;
                upgradeablefacility.UpgradeLevels[1].facilityPrefab.SetActive(true);

                //upgradeablefacility.UpgradeLevels[2].facilityPrefab.transform.NestToParent(tempGo.transform);
                //upgradeablefacility.UpgradeLevels[2].facilityPrefab.transform.SetParent(tempGo.transform);
                //upgradeablefacility.UpgradeLevels[2].facilityPrefab.transform.localPosition =
                //    upgradeablefacility.transform.localPosition;
                tempGo.SetActive(false);
                DontDestroyOnLoad(tempGo);
                if (level0prefabspawner != null)
                {
                    level0prefabspawner.Prefabs[0].Despawn();
                    if (newlvl0destructible != null)
                        newlvl0destructible.CollapsibleObjects[0].replacementObject.SetActive(false);
                    WreckTransform = newupgradelevels[0].facilityPrefab.transform.FindChild("RB_observatory/wreck/wreckSpawner");
                    if (WreckTransform != null)
                        WreckTransform.gameObject.SetActive(true);
                }
                if (level1prefabspawner != null)
                {
                    level1prefabspawner.Prefabs[0].Despawn();
                    if (newlvl1destructible != null)
                        newlvl1destructible.CollapsibleObjects[0].replacementObject.SetActive(false);
                    WreckTransform = newupgradelevels[1].facilityPrefab.transform.FindChild("RB_observatory/wreck/wreckSpawner");
                    if (WreckTransform != null)
                        WreckTransform.gameObject.SetActive(true);
                }

                upgradeablefacility.SetupLevels();
                /*
                if (level0PrefabOffset != Vector3.zero)
                {
                    Vector3 UpgradeOrigPos = upgradeablefacility.FacilityTransform.localPosition;
                    Vector3 newPrefabPos = UpgradeOrigPos;
                    newPrefabPos.x = level0PrefabOffset.x;
                    newPrefabPos.y = level0PrefabOffset.y;
                    newPrefabPos.z = level0PrefabOffset.z;
                    upgradeablefacility.FacilityTransform.localPosition = newPrefabPos;
                    upgradeablefacility.UpgradeLevels[0].Setup(upgradeablefacility);
                    upgradeablefacility.FacilityTransform.localPosition = UpgradeOrigPos;
                }
                if (level1PrefabOffset != Vector3.zero)
                {
                    Vector3 UpgradeOrigPos = upgradeablefacility.FacilityTransform.localPosition;
                    Vector3 newPrefabPos = UpgradeOrigPos;
                    newPrefabPos.x = level1PrefabOffset.x;
                    newPrefabPos.y = level1PrefabOffset.y;
                    newPrefabPos.z = level1PrefabOffset.z;
                    upgradeablefacility.FacilityTransform.localPosition = newPrefabPos;
                    upgradeablefacility.UpgradeLevels[1].Setup(upgradeablefacility);
                    upgradeablefacility.FacilityTransform.localPosition = UpgradeOrigPos;
                }*/
                      

                //print("Observatory prefab level 0 dump");
                //Utilities.DumpGameObjectHierarchy(upgradeablefacility.UpgradeLevels[0].facilityPrefab.gameObject);

                //print("Observatory prefab level 1 dump");
                //Utilities.DumpGameObjectHierarchy(upgradeablefacility.UpgradeLevels[1].facilityPrefab.gameObject);
                SpaceCenterFacilitySpawned = true;
                //dumpPQSCities();
                RSTLogWriter.Log("Observatory Construction - Post Psystem Spawn Point Ends");
                RSTLogWriter.Flush();
            }
            catch (Exception ex)
            {
                // error
                RSTLogWriter.Log("Failed to setup SpaceCenterFacility in PostPsystemSpawn.");
                RSTLogWriter.Log(ex.Message);
                RSTLogWriter.Flush();
            }
        }

        private void GetFacilityStrings()
        {
            ConfigNode cfg = ConfigNode.Load(Locales.PathDatabasePath);
            string[] sep = new string[] { " " };

            Observatorylvl1Range = float.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("observatorylvl1range"));
            Observatorylvl2Range = float.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("observatorylvl2range"));
        }
        
        /// <summary>
        /// This will cleanup the Stock Observatory prefab after we have cloned it from the RnD center.
        /// It will remove all the other buildings from the RnD centre and leave us with a clean Observatory.
        /// </summary>
        /// <param name="prefab">This is the RnD prefab that has been cloned that we want to clean up</param>
        /// <param name="newposition">This is the Vector3 position we want it located to</param>
        private void cleanStockObservatory(GameObject prefab, Vector3 newposition)
        {
            //Now go through our new level 1 prefab and Destroy everything we don't want. So we are only keeping the Observatory and MarkerAnchor GameObjects.
            try
            {
                Component[] comp = prefab.GetComponents<Component>();
                if (comp.Length > 0)
                {
                    int count = comp[0].transform.childCount;
                    for (int k = count - 1; k >= 0;  --k)
                    {
                        GameObject child = comp[0].transform.GetChild(k).gameObject;

                        if (!child.tag.Contains("Observatory") && !child.name.Contains("MarkerAnchor"))
                        {
                            print("Destroy from prefab: " + child.name);
                            DestroyImmediate(child);
                        }
                        else
                        {
                            if (child.name.Contains("Observatory"))
                            {
                                child.transform.position = newposition;
                                //child.tag = "KSC_RB_Obs";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // error
                RSTLogWriter.Log("Failed to cleanStockObservatory.");
                RSTLogWriter.Log(ex.Message);
                RSTLogWriter.Flush();
            }
        }

        /// <summary>
        /// Add DestructibleBuilding component to prefab.
        /// The settings from the sourceBuilding are copied to the prefab DestructibleBuilding component.
        /// </summary>
        /// <param name="sourceBuilding">This is the source DestructibleBuilding we are going to use as a template</param>
        /// <param name="targetBuildingTransform">This is the Transform we are going to add the DestructibleBuilding component to</param>
        /// <param name="targetBuildingCollapseObject">This is a reference to the parent GameObject of our prefab</param>
        /// <param name="fxTargetReference">This is a reference to the Transform we are going to set the FXTarget to</param>
        /// <param name="tempGo">This is a reference to the temporary GameObject we attach all our Instantiated gOs to</param>
        private void addDestructibleComponent(DestructibleBuilding sourceBuilding, DestructibleBuilding destBuilding, Transform targetBuildingTransform, GameObject targetBuildingCollapseObject, Transform fxTargetReference, GameObject tempGo, float repairCost)
        {
            try
            {
                targetBuildingTransform.gameObject.AddComponent<DestructibleBuilding>();
                destBuilding = targetBuildingTransform.GetComponent<DestructibleBuilding>();
                if (destBuilding != null)
                {
                    destBuilding.DemolitionFXPrefab = Instantiate(sourceBuilding.DemolitionFXPrefab);
                    destBuilding.RepairFXPrefab = Instantiate(sourceBuilding.RepairFXPrefab);
                    destBuilding.DemolitionFXPrefab.gameObject.SetActive(true);
                    destBuilding.RepairFXPrefab.gameObject.SetActive(true);
                    destBuilding.RepairCost = repairCost;
                    destBuilding.CollapseReputationHit = sourceBuilding.CollapseReputationHit;
                    destBuilding.FacilityDamageFraction = sourceBuilding.FacilityDamageFraction;
                    destBuilding.id = "";
                    destBuilding.FxTarget = fxTargetReference;

                    destBuilding.CollapsibleObjects = new DestructibleBuilding.CollapsibleObject[1];
                    destBuilding.CollapsibleObjects[0] = new DestructibleBuilding.CollapsibleObject();
                    destBuilding.CollapsibleObjects[0].collapseObject = new GameObject();
                    destBuilding.CollapsibleObjects[0].collapseObject = targetBuildingCollapseObject;
                    destBuilding.CollapsibleObjects[0].collapseBehaviour = sourceBuilding.CollapsibleObjects[0].collapseBehaviour;
                    destBuilding.CollapsibleObjects[0].collapseDuration = sourceBuilding.CollapsibleObjects[0].collapseDuration;
                    destBuilding.CollapsibleObjects[0].repairDuration = sourceBuilding.CollapsibleObjects[0].repairDuration;
                    destBuilding.DemolitionFXPrefab.gameObject.transform.NestToParent(tempGo.transform);
                    destBuilding.RepairFXPrefab.gameObject.transform.NestToParent(tempGo.transform);

                    
                    WreckTransform = targetBuildingTransform.FindChild("wreck");
                    destBuilding.CollapsibleObjects[0].replacementObject = WreckTransform.gameObject;
                }
            }
            catch (Exception ex)
            {
                // error
                RSTLogWriter.Log("Failed to Attach DestructibleBuilding in PostPsystemSpawn.");
                RSTLogWriter.Log(ex.Message);
            }
        }

        /// <summary>
        /// This will add a PrefabGroundBase to a prefab (so we have a flat base under our facility.
        /// </summary>
        /// <param name="groundbase">This is the prefab groundBase we want to add.</param>
        /// <param name="prefab">This is the prefab we want to attach the groundBase object to</param>
        /// <param name="attachposition">This is the transform position and rotation we will use for the groundBase</param>
        /// <param name="offset">This is any offset to the attachposition we want to apply, otherwise pass in vector3.zero</param>
        private void addPrefabGroundBase(GameObject groundbase, GameObject prefab, Transform attachposition, Vector3 offset)
        {
            try
            {
                var tmpGo = Instantiate(groundbase, attachposition.position, attachposition.rotation) as GameObject;
                tmpGo.transform.SetParent(prefab.transform);

                //We need to re-position it slightly if offset != zero
                tmpGo.transform.position = attachposition.position;
                tmpGo.transform.localPosition = offset;
                //tmpGo.tag = "KSC_RB_Obs";
                DontDestroyOnLoad(tmpGo);
            }
            catch (Exception ex)
            {
                // error
                RSTLogWriter.Log("Failed to addPrefabGroundBase in PostPsystemSpawn.");
                RSTLogWriter.Log(ex.Message);
                RSTLogWriter.Flush();
            }
        }

        /// <summary>
        /// Called whenever a Scene/Level is Loaded. But we only need to do this when the scene is the SpaceCenter.
        /// We have to create our "KSC/SpaceCenter/Observatory" transform, because it seems to disappear between Scenes.
        /// We then need to add our SpaceCenterBuilding to it (via the Observatory class).
        /// </summary>
        /// <param name="scene"></param>
        public void onLevelWasLoaded(GameScenes scene)
        {
            //If scene is SpaceCenter we care. Otherwise we do not.
            if (scene == GameScenes.SPACECENTER)
            {
                RSTLogWriter.Log("Observatory SpaceCenter Scene Loading...");
                if (!SpaceCenterFacilitySetupSuccess || !SpaceCenterFacilitySpawned)
                {
                    RSTLogWriter.Log("Failed to Inject Observatory SpaceCenter Facility cannot continue setup of this Facility");
                    return;
                }

                if (lvlStatsNotSet)
                {
                    upgradeablefacility.UpgradeLevels[0].levelText.textBase = Locales.FmtLocaleString("#autoLOC_RBodies_00097") + "\n";
                    string rngString1 = KSP.Localization.Localizer.Format("<<1>>", Database.instance.Observatorylvl1Range.ToString("F0"));
                    upgradeablefacility.UpgradeLevels[0].levelText.textBase += Locales.FmtLocaleString("#autoLOC_RBodies_00098", rngString1);
                    upgradeablefacility.UpgradeLevels[0].levelText.linePrefix = "* ";
                    upgradeablefacility.UpgradeLevels[0].levelStats = new Upgradeables.KSCFacilityLevelText();
                    upgradeablefacility.UpgradeLevels[0].levelStats.linePrefix = upgradeablefacility.UpgradeLevels[0].levelText.linePrefix;
                    upgradeablefacility.UpgradeLevels[0].levelStats.facility = SpaceCenterFacility.TrackingStation;
                    upgradeablefacility.UpgradeLevels[0].levelStats.textBase = upgradeablefacility.UpgradeLevels[0].levelText.textBase;

                    upgradeablefacility.UpgradeLevels[1].levelText.textBase = Locales.FmtLocaleString("#autoLOC_RBodies_00097") + "\n";
                    string rngString2 = KSP.Localization.Localizer.Format("<<1>>", Database.instance.Observatorylvl2Range.ToString("F0"));
                    upgradeablefacility.UpgradeLevels[1].levelText.textBase += Locales.FmtLocaleString("#autoLOC_RBodies_00098", rngString2);
                    upgradeablefacility.UpgradeLevels[1].levelText.linePrefix = "* ";
                    upgradeablefacility.UpgradeLevels[1].levelStats = new Upgradeables.KSCFacilityLevelText();
                    upgradeablefacility.UpgradeLevels[1].levelStats.linePrefix = upgradeablefacility.UpgradeLevels[1].levelText.linePrefix;
                    upgradeablefacility.UpgradeLevels[1].levelStats.facility = SpaceCenterFacility.TrackingStation;
                    upgradeablefacility.UpgradeLevels[1].levelStats.textBase = upgradeablefacility.UpgradeLevels[1].levelText.textBase;
                    lvlStatsNotSet = false;
                }

                RSTLogWriter.Log_Debug("Scene SpaceCenter");
                //Scan the PQS for our home planet.
                pqscities = Resources.FindObjectsOfTypeAll<PQSCity>();
                for (int i = 0; i < pqscities.Length; i++)
                {
                    if (pqscities[i].name == "KSC")
                    {
                        //Get the SpaceCenter Transform and the TrackingStation Transform
                        spacecentertransform = pqscities[i].gameObject.transform.Find("SpaceCenter");
                        trackingstationtransform = pqscities[i].gameObject.transform.Find("SpaceCenter/TrackingStation");

                        ObservatoryGo = createFacilityTransform("Observatory", upgradeablefacility);
                        //error Msg if ObservatoryGo is Null
                        if (ObservatoryGo == null)
                        {
                            RSTLogWriter.Log("Failed to Create the Observatory Transform.");
                            return;
                        }
                        
                        //Add our Facility SpaceCenterBuilding object - needs to be an attached component of the SpaceCenterFacility
                        //Use extension AddComponent to populate the values of the component as it's added.
                        trackingstation = PSystemSetup.Instance.GetSpaceCenterFacility("TrackingStation");
                        observatoryspacecenterfacility = PSystemSetup.Instance.GetSpaceCenterFacility("Observatory");
                        //check our SpaceCenterbuilding object doesn't already exist (between scene changes). If it does not exist, we Create it.
                        if (SpaceCenterObservatory == null)
                        {
                            SpaceCenterObservatory =
                                ObservatoryGo.gameObject.AddComponentWithInit<Observatory>(f =>
                                {
                                    f.name = "Observatory";
                                    f.facilityName = "Observatory";
                                    f.buildingInfoName = Locales.FmtLocaleString("#autoLOC_RBodies_00096"); //"Observatory"
                                    f.buildingDescription = Locales.FmtLocaleString("#autoLOC_RBodies_00099"); // "This is the Observatory where you can view information about the Celestial Bodies in the sky and observe and conduct research on them.";
                                });
                            //Copy the TrackingStationTransform position to our new building position and rotation.
                            SpaceCenterObservatory.gameObject.transform.position =
                                upgradeablefacility.transform.position; // trackingstationtransform.position;
                            SpaceCenterObservatory.gameObject.transform.rotation =
                                upgradeablefacility.transform.rotation; //trackingstationtransform.rotation;
                                
                            //We need a tooltipPrefab, so we just get one and copy it to our new Building object.
                            buildings = Resources.FindObjectsOfTypeAll<SpaceCenterBuilding>();
                            if (buildings.Length > 1)
                            {
                                SpaceCenterObservatory.tooltipPrefab = Instantiate(buildings[1].tooltipPrefab);
                                SpaceCenterObservatory.TooltipPrefabType = Instantiate(buildings[1].TooltipPrefabType);
                            }

                            DontDestroyOnLoad(SpaceCenterObservatory);
                        }
                        break;
                    }
                }
                //Setup the BuildingPicker UI objects.
                setupBuildingPicker();
                RSTLogWriter.Log("Observatory SpaceCenter Scene Loading Completed.");
                RSTLogWriter.Flush();
            }
        }

        /// <summary>
        /// Create new Facility Transform that the spawned current level model is childed to.
        /// </summary>
        /// <param name="facilityName">A string containing the Name of the new Facility</param>
        /// <param name="facilityParent">The UpgradeableFaciity object that the new transform will be childed to</param>
        /// <returns>GameObject for the Facility Transform, otherwise returns null.</returns>
        private GameObject createFacilityTransform(string facilityName, Upgradeables.UpgradeableFacility facilityParent)
        {
            try
            {
                if (trackingstationtransform != null && facilityParent != null && ObservatoryGo == null)
                {
                    ObservatoryGo = new GameObject(facilityName);
                    ObservatoryGo.transform.name = facilityName;
                    ObservatoryGo.layer = 15;
                    ObservatoryGo.transform.SetParent(facilityParent.gameObject.transform, true);
                    ObservatoryGo.transform.position = trackingstationtransform.position;
                    ObservatoryGo.transform.rotation = trackingstationtransform.rotation;
                    DontDestroyOnLoad(ObservatoryGo);
                    return ObservatoryGo;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                // error
                RSTLogWriter.Log("Failed to create Facility Transform in onLevelWasLoaded.");
                RSTLogWriter.Log(ex.Message);
                RSTLogWriter.Flush();
                return null;
            }
        }



        private void dumpPQSCities()
        {
            pqscities = Resources.FindObjectsOfTypeAll<PQSCity>();
            print("PQSCities");
            for (int u = 0; u < pqscities.Length; u++)
            {
                if (pqscities[u].name == "KSC")
                {
                    print("City " + pqscities[u].name);
                    Utilities.DumpGameObjectHierarchy(pqscities[u].gameObject);
                }
            }
        }

        /// <summary>
        /// This will set up the BuildingPicker to include the new Observatory Facility.
        /// </summary>
        private void setupBuildingPicker()
        {
            //Setup the BuildingPicker instance to include our new building.
            KSP.UI.Screens.SpaceCenter.BuildingPicker buildingpicker = FindObjectOfType<KSP.UI.Screens.SpaceCenter.BuildingPicker>();
            if (buildingpicker != null)
            {
                int numfaciltyInfo = buildingpicker.faciltyInfos.Length;
                //Check the Observatory Picker does not already exist
                bool found = false;
                for (int i = 0; i < numfaciltyInfo; i++)
                {
                    if (buildingpicker.faciltyInfos[i].name == "Observatory")
                    {
                        found = true;
                        break;
                    }
                }

                //If it does not exist, create one.
                if (!found)
                {
                    buildings = UnityEngine.Object.FindObjectsOfType<SpaceCenterBuilding>();
                    //We need something here to align the array order to the order that is returned by Object.FindObjectsOfType<SpaceCenterBuilding>();
                    var newbuildingpickerarray = new KSP.UI.Screens.SpaceCenter.BuildingPicker.FacilityUIInfo[buildings.Length];
                    for (int h = 0; h < buildings.Length; h++)
                    {
                        if (buildings[h].name.Contains("Observatory"))
                        {
                            var newFacilityUIInfo = CreateNewFacilityUIInfo(Textures.SpriteObservatory, "Observatory");
                            newbuildingpickerarray[h] = newFacilityUIInfo;
                        }
                        else
                        {
                            var index = Array.FindIndex(buildingpicker.faciltyInfos, z => z.name == buildings[h].name);
                            if (index != -1)
                            {
                                newbuildingpickerarray[h] = buildingpicker.faciltyInfos[index];
                            }
                        }
                    }
                    //Re-assign the facilityInfos to include the Observatory
                    buildingpicker.faciltyInfos = newbuildingpickerarray;
                }
            }
            else
            {
                RSTLogWriter.Log("BuildingPicker was found to be null. That's an error");
                RSTLogWriter.Flush();
            }
        }

        /// <summary>
        /// Create a new KSP.UI.Screens.SpaceCenter.BuildingPicker.FacilityUIInfo
        /// It will create a new SpriteSet and attach them to the object
        /// </summary>
        /// <returns> KSP.UI.Screens.SpaceCenter.BuildingPicker.FacilityUIInfo object</returns>
        private KSP.UI.Screens.SpaceCenter.BuildingPicker.FacilityUIInfo CreateNewFacilityUIInfo(Texture2D SpriteTexture, string PickerName)
        {
            try
            {
                var newFacilityUIInfo = new KSP.UI.Screens.SpaceCenter.BuildingPicker.FacilityUIInfo();
                newFacilityUIInfo.name = PickerName;
                //Create a SpriteSet and assign 
                newFacilityUIInfo.spriteSet = new KSP.UI.ButtonSpritesMgr.ButtonSprites();
                UnityEngine.Sprite spritenormal = UnityEngine.Sprite.Create(SpriteTexture, new Rect(0, 128, 128, 128),new Vector2(0.5f, 0.5f), 100f);
                spritenormal.name = PickerName + "_Normal_Sprite";
                UnityEngine.Sprite spritedisabled = UnityEngine.Sprite.Create(SpriteTexture, new Rect(128, 0, 128, 128),new Vector2(0.5f, 0.5f), 100f);
                spritedisabled.name = PickerName + "_Disabled_Sprite";
                UnityEngine.Sprite spritehover = UnityEngine.Sprite.Create(SpriteTexture, new Rect(128, 128, 128, 128),new Vector2(0.5f, 0.5f), 100f);
                spritehover.name = PickerName + "_Hover_Sprite";
                UnityEngine.Sprite spriteactive = UnityEngine.Sprite.Create(SpriteTexture, new Rect(128, 128, 128, 128),new Vector2(0.5f, 0.5f), 100f);
                spriteactive.name = PickerName + "_Active_Sprite";
                var sprites = new UnityEngine.Sprite[] {spritenormal, spritehover, spriteactive, spritedisabled};
                var spriteset = new KSP.UI.ButtonSpritesMgr.ButtonSprites();
                spriteset.Sprites = sprites;
                newFacilityUIInfo.spriteSet = spriteset;
                return newFacilityUIInfo;
            }
            catch (Exception ex)
            {
                // error
                RSTLogWriter.Log("Failed to CreateNewFacilityUIInfo.");
                RSTLogWriter.Log(ex.Message);
                RSTLogWriter.Flush();
                return null;
            }
        }


        
    }

    /// <summary>
    /// This is our Observatory SpaceCenterBuilding.
    /// When the user clicks it, we open the ResearchBodies Observatory Menu.
    /// </summary>
    public class Observatory : SpaceCenterBuilding
    {

        private AudioSource observatorytheme;
        //private AudioSource astroComplextheme;
        private string astroComplexTrackName;
        private Animation telescope_anim;
        private bool telescope_anim_active = false;
        public UICanvasPrefab OBScreenPrefab;

        public override bool IsOpen()
        {
            return (HighLogic.CurrentGame.Mode != Game.Modes.SCENARIO_NON_RESUMABLE);
        }

        /// <summary>
        /// When clicked, we need to check the Tracking Station level allows us access to the Observatory or not.
        /// If it does not, display a pop-up. If it does, turn on the Observatory GUI window.
        /// </summary>
        protected override void OnClicked()
        {
            InputLockManager.SetControlLock(ControlTypes.KSC_ALL | ControlTypes.TIMEWARP | ControlTypes.UI_MAIN, "ResearchBodies_SC_Observatory");
            //If RB is disabled facility is closed.
            if (HighLogic.CurrentGame != null)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<ResearchBodies_SettingsParms>().RBEnabled)
                {
                    Vector2 anchormin = new Vector2(0.5f, 0.5f);
                    Vector2 anchormax = new Vector2(0.5f, 0.5f);
                    string msg = Locales.FmtLocaleString("#autoLOC_RBodies_00100"); //"This Facility is closed.\nResearchBodies is Disabled in this save.";
                    string title = Locales.FmtLocaleString("#autoLOC_RBodies_00096"); // "Observatory";
                    UISkinDef skin = HighLogic.UISkin;
                    DialogGUIBase[] dialogGUIBase = new DialogGUIBase[1];
                    dialogGUIBase[0] = new DialogGUIButton(KSP.Localization.Localizer.Format("#autoLOC_417274"), delegate //"Ok"
                    {
                        InputLockManager.RemoveControlLock("ResearchBodies_SC_Observatory");
                    });
                    PopupDialog.SpawnPopupDialog(anchormin, anchormax, new MultiOptionDialog(title, msg, title, skin, dialogGUIBase), false, HighLogic.UISkin, true, string.Empty);
                    return;
                }
            }

            //If the Tracking Station is Level 1 and game settings do not allow Observatory at Level 1 
            //We display a pop-up and not the Observatory Window.
            if (ResearchBodiesController.instance.IsTSlevel1 && !Database.instance.allowTSlevel1)
            {
                Vector2 anchormin = new Vector2(0.5f, 0.5f);
                Vector2 anchormax = new Vector2(0.5f, 0.5f);
                string msg = Locales.FmtLocaleString("#autoLOC_RBodies_00101"); // "This Facility is closed.\nTrackingStation must be Level 2 or 3.";
                string title = Locales.FmtLocaleString("#autoLOC_RBodies_00096"); //"Observatory";
                UISkinDef skin = HighLogic.UISkin;
                DialogGUIBase[] dialogGUIBase = new DialogGUIBase[1];
                dialogGUIBase[0] = new DialogGUIButton(KSP.Localization.Localizer.Format("#autoLOC_417274"), delegate //"Ok"
                {
                    InputLockManager.RemoveControlLock("ResearchBodies_SC_Observatory");
                });
                PopupDialog.SpawnPopupDialog(anchormin, anchormax, new MultiOptionDialog(title, msg, title, skin, dialogGUIBase), false, HighLogic.UISkin, true, string.Empty);
                return;
            }
            //Activate the Observatory - processes sound and locks SC UI
            ActivateObservatory_SC_Facility();
            //Turn the Observatory Window ON.
            //ResearchBodiesController.instance.RBMenuAppLToolBar.GuiVisible = true;
            ResearchBodiesController.instance.showGUI = true;
        }

        /// <summary>
        /// OnStart Override will do Observatory Facility Sound Set-Up
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            setupSceneSound();
            setuptelescopeAnim();
        }
        
        /// <summary>
        /// Sets Up Facility theme music
        /// </summary>
        private void setupSceneSound()
        {
            //Setup our new Music
            observatorytheme = gameObject.AddComponent<AudioSource>();
            observatorytheme.clip = GameDatabase.Instance.GetAudioClip("REPOSoftTech/ResearchBodies/Sounds/observatory_music");
            observatorytheme.volume = 1;
            observatorytheme.panStereo = 0;
            observatorytheme.spatialBlend = 0;
            observatorytheme.rolloffMode = AudioRolloffMode.Linear;
            observatorytheme.Stop();

            //Store the AstroComplex music as we are going to use it
            GameDatabase.Instance.databaseAudio.Add(MusicLogic.fetch.astroComplexAmbience);
            astroComplexTrackName = MusicLogic.fetch.astroComplexAmbience.name;
        }

        private void setuptelescopeAnim()
        {
            var targetTransform = base.Facility.UpgradeLevels[0].facilityPrefab.transform.FindChild("RB_observatory");
            if (targetTransform != null)
            {
                telescope_anim = targetTransform.GetComponent<Animation>();
                if (telescope_anim != null)
                {
                    telescope_anim_active = true;
                    telescope_anim.Play("Open");
                }
            }
        }

        /// <summary>
        /// Activates the facility theme music and locks the UI
        /// </summary>
        private void ActivateObservatory_SC_Facility()
        {
            if (OBScreenPrefab != null)
                UIMasterController.Instance.AddCanvas(this.OBScreenPrefab, true);

            //Switch the music
            MusicLogic.fetch.astroComplexAmbience = observatorytheme.clip;
            MusicLogic.fetch.PauseWithCrossfade(MusicLogic.AdditionalThemes.AstronautComplex);
            
            //Lock the SC UI quit button.
            //InputLockManager.SetControlLock(ControlTypes.KSC_ALL | ControlTypes.TIMEWARP | ControlTypes.UI_MAIN, "ResearchBodies_SC_Observatory");
            KSP.UI.UIWarpToNextMorning[] nextMorningUI = (KSP.UI.UIWarpToNextMorning[]) FindObjectsOfType(typeof(KSP.UI.UIWarpToNextMorning));
            if (nextMorningUI.Length > 0)
            {
                nextMorningUI[0].button.enabled = false;
            }
            KSP.UI.Screens.UISpaceCenter.Instance.quitBtn.Lock();
            KSP.UI.Screens.ApplicationLauncher.Instance.Hide();
            base.HighLightBuilding(false);
        }

        /// <summary>
        /// DeActivites the facility theme music and unlocks the UI
        /// </summary>
        public void DeActivateObservatory_SC_Facility()
        {
            //Switch the music
            MusicLogic.fetch.astroComplexAmbience = GameDatabase.Instance.GetAudioClip(astroComplexTrackName);
            MusicLogic.fetch.UnpauseWithCrossfade();
            
            //Un-Lock the SC UI quit button.
            InputLockManager.SetControlLock(ControlTypes.None, "ResearchBodies_SC_Observatory");
            KSP.UI.UIWarpToNextMorning[] nextMorningUI = (KSP.UI.UIWarpToNextMorning[])FindObjectsOfType(typeof(KSP.UI.UIWarpToNextMorning));
            if (nextMorningUI.Length > 0)
            {
                nextMorningUI[0].button.enabled = true;
            }
            KSP.UI.Screens.ApplicationLauncher.Instance.Show();
            base.StartCoroutine(CallbackUtil.DelayedCallback(520, new Callback(UnlockQuitBtn)));
            ResearchBodiesController.instance.showGUI = false;
            if (OBScreenPrefab != null)
                UIMasterController.Instance.RemoveCanvas(this.OBScreenPrefab);
        }

        public void UnlockQuitBtn()
        {
            KSP.UI.Screens.UISpaceCenter.Instance.quitBtn.Unlock();
        }
    }
}
