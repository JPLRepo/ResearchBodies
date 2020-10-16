﻿/*
 * CCContractExtensions.cs
 * (C) Copyright 2016, Jamie Leighton  
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
using UnityEngine;
#if (CC)
using ContractConfigurator;
#endif
using Contracts;
using RSTUtils;
using Random = UnityEngine.Random;

#if (CC)
namespace ResearchBodies
{
    /*
    * ContractRequirement to provide requirement for player having reached a minimum level for the Observatory Facility.
    */
    public class ObservatoryLevelRequirement : ContractRequirement
    {
        protected int minLevel { get; set; }
        protected string facility { get; set; }

        public override bool LoadFromConfig(ConfigNode configNode)
        {
            // Load base class
            bool valid = base.LoadFromConfig(configNode);

            // Check on active contracts too
            checkOnActiveContract = false;

            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "facility", x => facility = x, this);
            valid &= ConfigNodeUtil.ParseValue<int>(configNode, "minLevel", x => minLevel = x, this, 1, x => Validation.Between(x, 1, 3));
            //valid &= ConfigNodeUtil.AtLeastOne(configNode, new string[] { "minLevel", "maxLevel" }, this);

            return valid;
        }

        public override void OnLoad(ConfigNode configNode)
        {
            //Get facility
            facility = ConfigNodeUtil.ParseValue<string>(configNode, "facility", "Observatory");
            // Get minLevel
            minLevel = ConfigNodeUtil.ParseValue<int>(configNode, "minLevel", 1);
        }

        public override void OnSave(ConfigNode configNode)
        {
            configNode.AddValue("facility", facility);
            configNode.AddValue("minLevel", minLevel);
        }

        protected override string RequirementText()
        {
            return "The Observatory must be level " + minLevel;
        }

        public override bool RequirementMet(ConfiguredContract contract)
        {
            if (contract == null)
                return false;
            if (contract.ContractState == Contract.State.Active)
                return true;
            for (int i = 0; i < PSystemSetup.Instance.SpaceCenterFacilities.Length; i++)
            {
                if (PSystemSetup.Instance.SpaceCenterFacilities[i].name == facility)
                {
                    int level = Mathf.RoundToInt(ScenarioUpgradeableFacilities.GetFacilityLevel(facility) + 1);
                    return level >= minLevel;
                }            
            }
            RSTLogWriter.Log("Observatory NOT found....");
            return false; 
        }
    }
    /*
    * ContractParamater to generate the Duration of the Search the Skies contract based on the level of the Observatory Facility.
    */
    public class RBSearchSkiesDurationFactory : ParameterFactory
    {
        protected string facility;
        protected string preWaitText;
        protected string waitingText;
        protected string completionText;
        protected ContractConfigurator.Parameters.Duration.StartCriteria startCriteria;
        protected List<string> parameter;

        public override bool Load(ConfigNode configNode)
        {
            // Load base class
            bool valid = base.Load(configNode);
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "facility", x => facility = x, this, "Observatory");
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "preWaitText", x => preWaitText = x, this, "The observatory will search the skies for a period of time.");
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "waitingText", x => waitingText = x, this, "Searching the Skies.");
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "completionText", x => completionText = x, this, "The search is over.");
            valid &= ConfigNodeUtil.ParseValue<ContractConfigurator.Parameters.Duration.StartCriteria>(configNode, "startCriteria", x => startCriteria = x, this, ContractConfigurator.Parameters.Duration.StartCriteria.CONTRACT_ACCEPTANCE);
            valid &= ConfigNodeUtil.ParseValue<List<string>>(configNode, "parameter", x => parameter = x, this, new List<string>());
            valid &= ConfigNodeUtil.ValidateExcludedValue(configNode, "title", this);

            return valid;
        }

        public override ContractParameter Generate(Contract contract)
        {
            //ContractConfigurator.Duration value = new ContractConfigurator.Duration();
            double durationValue = 2;
            for (int i = 0; i < PSystemSetup.Instance.SpaceCenterFacilities.Length; i++)
            {
                if (PSystemSetup.Instance.SpaceCenterFacilities[i].name == facility)
                {
                    int level = Mathf.RoundToInt(ScenarioUpgradeableFacilities.GetFacilityLevel(facility) + 1);
                    if (level == 1)
                    {
                        durationValue = 1;
                        break;
                    }
                    if (level == 2)
                    {
                        durationValue = 0.5;
                        break;
                    }
                }
            }
            durationValue *= KSPUtil.dateTimeFormatter.Year;

            return new ContractConfigurator.Parameters.Duration(durationValue, preWaitText, waitingText, completionText, startCriteria, parameter);
        }
    }
    /*
    * ContractParamater to generate the Duration of the Research a CB contract based on the level of the Observatory Facility.
    */
    public class RBResearchBodiesDurationFactory : ParameterFactory
    {
        protected string facility { get; set; }
        protected string preWaitText;
        protected string waitingText;
        protected string completionText;
        protected ContractConfigurator.Parameters.Duration.StartCriteria startCriteria;
        protected List<string> parameter;

        public override bool Load(ConfigNode configNode)
        {
            // Load base class
            bool valid = base.Load(configNode);
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "facility", x => facility = x, this);
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "preWaitText", x => preWaitText = x, this, (string)null);
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "waitingText", x => waitingText = x, this, (string)null);
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "completionText", x => completionText = x, this, (string)null);
            valid &= ConfigNodeUtil.ParseValue<ContractConfigurator.Parameters.Duration.StartCriteria>(configNode, "startCriteria", x => startCriteria = x, this, ContractConfigurator.Parameters.Duration.StartCriteria.CONTRACT_ACCEPTANCE);
            valid &= ConfigNodeUtil.ParseValue<List<string>>(configNode, "parameter", x => parameter = x, this, new List<string>());
            valid &= ConfigNodeUtil.ValidateExcludedValue(configNode, "title", this);

            return valid;
        }

        public override ContractParameter Generate(Contract contract)
        {
            //ContractConfigurator.Duration value = new ContractConfigurator.Duration();
            double durationValue = 1;
            for (int i = 0; i < PSystemSetup.Instance.SpaceCenterFacilities.Length; i++)
            {
                if (PSystemSetup.Instance.SpaceCenterFacilities[i].name == facility)
                {
                    int level = Mathf.RoundToInt(ScenarioUpgradeableFacilities.GetFacilityLevel(facility) + 1);
                    if (level == 1)
                    {
                        durationValue = 0.5;
                        break;
                    }
                    if (level == 2)
                    {
                        durationValue = 0.25;
                        break;
                    }
                }
            }
            durationValue *= KSPUtil.dateTimeFormatter.Year;

            return new ContractConfigurator.Parameters.Duration(durationValue, preWaitText, waitingText, completionText, startCriteria, parameter);
        }
    }

    /*
    * ContractRequirement to check that there must be Unresearched/Discovered Celestial Bodies.
    */
    public class RBUndiscoveredBodiesRequirement : ContractRequirement
    {

        protected string host { get; set; }

        public override bool LoadFromConfig(ConfigNode configNode)
        {
            // Load base class
            bool valid = base.LoadFromConfig(configNode);
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "host", x => host = x, this, "Observatory");
            // Check on active contracts too
            checkOnActiveContract = false;
            
            return valid;
        }

        public override void OnLoad(ConfigNode configNode)
        {
            //Get host
            host = ConfigNodeUtil.ParseValue<string>(configNode, "host", "Observatory");
        }

        public override void OnSave(ConfigNode configNode)
        {
            configNode.AddValue("host", host);
        }

        protected override string RequirementText()
        {
            if (host == "Observatory")
                return "There must be Bodies yet to be discovered within the Observatories range";
            else
                return "There must be Bodies yet to be discovered within a telescopes range";
        }

        public override bool RequirementMet(ConfiguredContract contract)
        {
            foreach (CelestialBody body in Database.instance.BodyList)
            {
                if (!Database.instance.CelestialBodies[body].isResearched)
                {
                    if (host == "Observatory")
                    {
                        if (RBRange.WithinObsRange(body.transform))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (FlightGlobals.fetch != null)
                        {
                            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
                            {
                                double range = 0;
                                if (RBRange.VesselHasModuleTrackBodies(FlightGlobals.Vessels[i], out range))
                                {
                                    if (RBRange.WithinVslRange(FlightGlobals.Vessels[i], body.transform))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
    
    /*
    * ContractRequirement to check that there must be Celestial Bodies that are discovered but have not have 100% Research completed.
    */
    public class RBResearchBodiesRequirement : ContractRequirement
    {
        protected string host { get; set; }

        public override bool LoadFromConfig(ConfigNode configNode)
        {
            // Load base class
            bool valid = base.LoadFromConfig(configNode);
            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "host", x => host = x, this, "Observatory");
            // Check on active contracts too
            checkOnActiveContract = false;

            return valid;
        }

        public override void OnLoad(ConfigNode configNode)
        {
            //Get host
            host = ConfigNodeUtil.ParseValue<string>(configNode, "host", "Observatory");
        }

        public override void OnSave(ConfigNode configNode)
        {
            configNode.AddValue("host", host);
        }

        protected override string RequirementText()
        {
            if (host == "Observatory")
                return
                    "There must be Bodies yet to have their research completed that are within range of the Observatory";
            else
                return "There must be Bodies yet to have their research completed that are within range of a Telescope";
        }

        public override bool RequirementMet(ConfiguredContract contract)
        {
            foreach (CelestialBody body in Database.instance.BodyList)
            {
                if (Database.instance.CelestialBodies[body].isResearched && Database.instance.CelestialBodies[body].researchState < 100)
                {
                    if (host == "Observatory")
                    {
                        if (RBRange.WithinObsRange(body.transform))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (FlightGlobals.fetch != null)
                        {
                            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
                            {
                                double range = 0;
                                if (RBRange.VesselHasModuleTrackBodies(FlightGlobals.Vessels[i], out range))
                                {
                                    if (RBRange.WithinVslRange(FlightGlobals.Vessels[i], body.transform))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }

    /*
    * Search the Skies Contract Behaviour Factory.
    */
    public class RBSearchSkiesBehaviourFactory : BehaviourFactory
    {
        public override bool Load(ConfigNode configNode)
        {
            // Load base class
            bool valid = base.Load(configNode);

            // Load class specific data
            //     ADD YOUR LOGIC HERE

            return valid;
        }

        public override ContractBehaviour Generate(ConfiguredContract contract)
        {
            return new RBSearchSkiesBehaviour();
        }
    }
    /*
    * Search the Skies Contract Behaviour Class.
    * On contact completion we check for an Un-discovered Celestial Body and discovery it!
    */
    public class RBSearchSkiesBehaviour : ContractBehaviour
    {
        protected override void OnAccepted() { }
        protected override void OnCancelled() { }

        protected override void OnCompleted()
        {
            List<CelestialBody> PotentialDiscovery = new List<CelestialBody>();
            foreach (CelestialBody body in Database.instance.BodyList)
            {
                if (!Database.instance.CelestialBodies[body].isResearched && RBRange.WithinObsRange(body.transform))
                {
                    PotentialDiscovery.Add(body);
                }
            }
            if (PotentialDiscovery.Count > 0)
            {
                int discovered = Random.Range(0, PotentialDiscovery.Count - 1);
                bool withParent;
                CelestialBody parentBody;
                ResearchBodiesController.FoundBody(Database.instance.RB_SettingsParms.ScienceReward, PotentialDiscovery[discovered], out withParent, out parentBody);
                return;
            }
            Vector2 anchormin = new Vector2(0.5f, 0.5f);
            Vector2 anchormax = new Vector2(0.5f, 0.5f);
            string msg = "Despite our best efforts, we were unable to find anything intersting in the sky.";
            string title = "Research Bodies";
            UISkinDef skin = HighLogic.UISkin;
            DialogGUIBase[] dialogGUIBase = new DialogGUIBase[1];
            dialogGUIBase[0] = new DialogGUIButton("Ok", delegate { });
            PopupDialog.SpawnPopupDialog(anchormin, anchormax,
                new MultiOptionDialog(title, msg, title, skin, dialogGUIBase), false, HighLogic.UISkin, true,
                string.Empty);
        }
        protected override void OnDeadlineExpired() { }
        protected override void OnDeclined() { }
        protected override void OnFailed() { }
        protected override void OnFinished() { }
        protected override void OnGenerateFailed() { }
        protected override void OnOffered() { }
        protected override void OnOfferExpired() { }
        protected override void OnParameterStateChange(ContractParameter param) { }
        protected override void OnRegister() { }
        protected override void OnUnregister() { }
        protected override void OnUpdate() { }
        protected override void OnWithdrawn() { }
        protected override void OnLoad(ConfigNode configNode) { }
        protected override void OnSave(ConfigNode configNode) { }
    }
    /*
    * Research a CB Contract Behaviour Factory.
    */
    public class RBResearchBodiesBehaviourFactory : BehaviourFactory
    {
        public override bool Load(ConfigNode configNode)
        {
            // Load base class
            bool valid = base.Load(configNode);

            // Load class specific data
            //     ADD YOUR LOGIC HERE

            return valid;
        }

        public override ContractBehaviour Generate(ConfiguredContract contract)
        {
            return new RBResearchBodiesBehaviour();
        }
    }
    /*
    * Research a CB Contract Behaviour Class.
    * On contact completion we check for Discovered Celestial Body that is not completely Researched and add research points to it!
    */
    public class RBResearchBodiesBehaviour : ContractBehaviour
    {
        protected override void OnAccepted() { }
        protected override void OnCancelled() { }

        protected override void OnCompleted()
        {
            foreach (CelestialBody body in Database.instance.BodyList)
            {
                if (Database.instance.CelestialBodies[body].isResearched && Database.instance.CelestialBodies[body].researchState < 100)
                {
                    if (RBRange.WithinObsRange(body.transform))
                    {
                        ResearchBodiesController.Research(body, 10);
                        return;
                    }
                }
            }
            Vector2 anchormin = new Vector2(0.5f, 0.5f);
            Vector2 anchormax = new Vector2(0.5f, 0.5f);
            string msg = "Despite our best efforts, we were unable to find anything new that's interesting in the sky.";
            string title = "Research Bodies";
            UISkinDef skin = HighLogic.UISkin;
            DialogGUIBase[] dialogGUIBase = new DialogGUIBase[1];
            dialogGUIBase[0] = new DialogGUIButton("Ok", delegate { });
            PopupDialog.SpawnPopupDialog(anchormin, anchormax,
                new MultiOptionDialog(title, msg, title, skin, dialogGUIBase), false, HighLogic.UISkin, true,
                string.Empty);
        }
        protected override void OnDeadlineExpired() { }
        protected override void OnDeclined() { }
        protected override void OnFailed() { }
        protected override void OnFinished() { }
        protected override void OnGenerateFailed() { }
        protected override void OnOffered() { }
        protected override void OnOfferExpired() { }
        protected override void OnParameterStateChange(ContractParameter param) { }
        protected override void OnRegister() { }
        protected override void OnUnregister() { }
        protected override void OnUpdate() { }
        protected override void OnWithdrawn() { }
        protected override void OnLoad(ConfigNode configNode) { }
        protected override void OnSave(ConfigNode configNode) { }
    }

    /// <summary>
    /// Check Range static class
    /// </summary>
    //todo This whole class needs to be factored in better and re-written, but we are not in just get the mod updated mode.
    public static class RBRange
    {
        /// <summary>
        /// Checks if targetTransform is within range of the Observatory or not based on it's level
        /// </summary>
        /// <param name="targetTransform">target CB transform</param>
        /// <returns>true or false</returns>
        public static bool WithinObsRange(Transform targetTransform)
        {
            if (ResearchBodies_Observatory.spacecentertransform == null)
            {
                RSTUtils.RSTLogWriter.Log("Cannot determine range from Observatory as KSC transform is null");
                return false;
            }
            //Calculate distance from Observatory to CB target body.
            Vector3 hostPos = ResearchBodies_Observatory.spacecentertransform.position;
            Vector3 targetPos = targetTransform.position;
            //double angle = Vector3.Angle(targetPos - hostPos, ResearchBodies_Observatory.spacecentertransform.up);
            double distance = Vector3d.Distance(targetTransform.position, ResearchBodies_Observatory.spacecentertransform.position);

            //Get the current Observatory level and set the range of the Observatory from the database (settings).
            int obslevel = 1;
            for (int i = 0; i < PSystemSetup.Instance.SpaceCenterFacilities.Length; i++)
            {
                if (PSystemSetup.Instance.SpaceCenterFacilities[i].name == "Observatory")
                {
                    obslevel = Mathf.RoundToInt(ScenarioUpgradeableFacilities.GetFacilityLevel("Observatory") + 1);
                    break;
                }
            }
            double obsrange = 0;
            if (obslevel == 1) obsrange = Database.instance.Observatorylvl1Range;
            if (obslevel == 2) obsrange = Database.instance.Observatorylvl2Range;

            //If it's within range return true, otherwise false.
            if (distance <= obsrange)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if targetTransform is within range of the Vessel
        /// </summary>
        /// <param name="targetTransform">target CB transform</param>
        /// <returns>true or false</returns>
        public static bool WithinVslRange(Vessel vessel, Transform targetTransform)
        {
            //Find the range of the partmodule, if we can't fail.
            double obsrange = 0.0f;
            bool found = VesselHasModuleTrackBodies(vessel, out obsrange);
            
            if (!found)
            {
                RSTUtils.RSTLogWriter.Log("Cannot determine range for vessel");
                return false;
            }
            //Calculate distance from Vessel to CB target body.
            Vector3 hostPos = vessel.transform.position;
            Vector3 targetPos = targetTransform.position;
            //double angle = Vector3.Angle(targetPos - hostPos, vessel.transform.up);
            double distance = Vector3d.Distance(targetTransform.position, vessel.transform.position);

            //If it's within range return true, otherwise false.
            if (distance <= obsrange)
            {
                return true;
            }

            return false;
        }

        public static bool VesselHasModuleTrackBodies(Vessel vessel, out double range)
        {
            bool found = false;
            range = 0;
            if (vessel.loaded)
            {
                for (int i = 0; i < vessel.parts.Count; i++)
                {
                    for (int j = 0; j < vessel.parts[i].Modules.Count; j++)
                    {
                        if (vessel.parts[i].Modules[j].moduleName == "ModuleTrackBodies")
                        {
                            ModuleTrackBodies module = vessel.parts[i].Modules[j] as ModuleTrackBodies;
                            if (module != null)
                            {
                                found = true;
                                range = module.maxTrackDistance;
                                break;
                            }
                        }
                    }
                    if (found) break;
                }
            }
            else
            {
                for (int i = 0; i < vessel.protoVessel.protoPartSnapshots.Count; i++)
                {
                    for (int j = 0; j < vessel.protoVessel.protoPartSnapshots[i].modules.Count; j++)
                    {
                        if (vessel.protoVessel.protoPartSnapshots[i].modules[j].moduleName == "ModuleTrackBodies")
                        {
                            ConfigNode moduleValues = vessel.protoVessel.protoPartSnapshots[i].modules[j].moduleValues;
                            moduleValues.TryGetValue("maxTrackDistance", ref range);
                            if (range == 0)
                            {
                                range = 40000000000; //Default if not found.
                            }
                            found = true;
                            break;                            
                        }
                    }
                    if (found) break;
                }
            }
            return found;
        }
    }   
}
#endif
