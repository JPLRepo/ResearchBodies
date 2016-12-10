using System;
using UnityEngine;
using ContractConfigurator;

namespace ResearchBodies
{
    /*
 * ContractRequirement to provide requirement for player having reached a minimum altitude.
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
            checkOnActiveContract = configNode.HasValue("checkOnActiveContract") ? checkOnActiveContract : true;

            valid &= ConfigNodeUtil.ParseValue<string>(configNode, "facility", x => facility = x, this);
            valid &= ConfigNodeUtil.ParseValue<int>(configNode, "minLevel", x => minLevel = x, this, 1, x => Validation.Between(x, 1, 3));
            //valid &= ConfigNodeUtil.AtLeastOne(configNode, new string[] { "minLevel", "maxLevel" }, this);

            return valid;
        }

        public override void OnLoad(ConfigNode configNode)
        {
            //Get facility
            facility = ConfigNodeUtil.ParseValue<string>(configNode, "facility");
            
            // Get minLevel
            minLevel = ConfigNodeUtil.ParseValue<int>(configNode, "minLevel");
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
            for (int i = 0; i < PSystemSetup.Instance.SpaceCenterFacilities.Length; i++)
            {
                if (PSystemSetup.Instance.SpaceCenterFacilities[i].name == facility)
                {
                    int level = (int)Math.Round(ScenarioUpgradeableFacilities.GetFacilityLevel(facility) *
                    ScenarioUpgradeableFacilities.GetFacilityLevelCount(facility)) + 1;
                    return level == 0 && contract != null && contract.ContractState == Contracts.Contract.State.Active ||
                    level >= minLevel;
                }            
            }
            return false; 
        }
    }
}
