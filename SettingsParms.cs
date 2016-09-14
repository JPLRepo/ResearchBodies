using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ResearchBodies
{
    public class ResearchBodies_SettingsParms : GameParameters.CustomParameterNode

    {
        public override string Title { get { return "ResearchBodies Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return true; } }
        public override string Section { get { return "ResearchBodies"; } }
        public override int SectionOrder { get { return 1; } }

        public static string setLanguage = "";
        
        [GameParameters.CustomParameterUI("Language", toolTip = "Select Language")]
        public string language = "";
        
        [GameParameters.CustomParameterUI("ResearchBodies Enabled in this save")]
        public bool RBEnabled = true;

        [GameParameters.CustomStringParameterUI("Test String UI", autoPersistance = true, lines = 5,title = "Celestial Bodies already discovered", toolTip = "Depending on the Difficulty Setting these Bodies will be\n already discovered at the start of the game.")]
        public string CBstring = "";

        [GameParameters.CustomIntParameterUI("Research Plan Start Cost", toolTip = "The cost to start researching a Celestial Body", maxValue = 15000, stepSize = 100, gameMode = GameParameters.GameMode.CAREER, newGameOnly = true)]
        public int ResearchCost = 200;

        [GameParameters.CustomIntParameterUI("Research Progression Step Cost", toolTip = "The cost for each research step on a Celestial Body", maxValue = 20000, stepSize = 100, gameMode = GameParameters.GameMode.CAREER, newGameOnly = true)]
        public int ProgressResearchCost = 200;

        [GameParameters.CustomIntParameterUI("Science Reward on Body Found", toolTip = "The amount of science rewarded for finding a new Celestial Body", maxValue = 150, stepSize = 5, gameMode = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE, newGameOnly = true)]
        public int ScienceReward = 75;
        
        [GameParameters.CustomIntParameterUI("Body Discovery Chance Seed Value", toolTip = "The higher this value the harder it will be to find a new Celestial Body", minValue = 1, maxValue = 6, stepSize = 1, newGameOnly = true)]
        public int DiscoverySeed = 3;

        [GameParameters.CustomParameterUI("Use Stock Application Launcher Icon", toolTip = "If on, the Stock Application launcher will be used,\nif off will use Blizzy Toolbar if installed")]
        public bool UseAppLToolbar = true;

        [GameParameters.CustomParameterUI("Extra Debug Logging", toolTip = "Turn this On to capture lots of extra information into the KSP log for reporting a problem.")]
        public bool DebugLogging = false;

        [GameParameters.CustomParameterUI("Observatory available at T/Station Level 1", toolTip = "If this option is On the Observatory is available when the Tracking station is Level 1,\nif off the Tracking station must be level 2 or 3.")]
        public bool Enabledtslvl1 = false;
        
        
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    ResearchCost = 20;
                    ProgressResearchCost = 10;
                    ScienceReward = 100;
                    CBstring = Database.instance.GetIgnoredBodies(Level.Easy);
                    break;
                case GameParameters.Preset.Normal:
                    ResearchCost = 400;
                    ProgressResearchCost = 200;
                    ScienceReward = 75;
                    CBstring = Database.instance.GetIgnoredBodies(Level.Normal);
                    break;
                case GameParameters.Preset.Moderate:
                    ResearchCost = 700;
                    ProgressResearchCost = 350;
                    ScienceReward = 35;
                    CBstring = Database.instance.GetIgnoredBodies(Level.Medium);
                    break;
                case GameParameters.Preset.Hard:
                    ResearchCost = 1000;
                    ProgressResearchCost = 500;
                    ScienceReward = 10;
                    CBstring = Database.instance.GetIgnoredBodies(Level.Hard);
                    break;
                case GameParameters.Preset.Custom:
                    CBstring = "how do we deal with custom????";
                    break;
            }
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "RBEnabled") //RbEnabled must always be enabled.
                return true;
            if (RBEnabled == false)  //Otherwise it depends on the value of RBEnabled if it's false return false
                return false;
            return true; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "RBEnabled") //RbEnabled must always be enabled.
                return true;
            if (RBEnabled == false)  //Otherwise it depends on the value of RBEnabled if it's false return false
                return false;
            return true; //otherwise return true
        }

        public override IList ValidValues(MemberInfo member)
        {
            if (member.Name == "language")
            {
                List<string> myList = new List<string>();
                for (int i = 0; i < Locales.locales.Count; i++)
                {
                    myList.Add(Locales.locales[i].LocaleFull);
                }
                IList myIlist = myList;
                language = setLanguage;
                return myIlist;
            }
            else
            {
                return null;
            }
            
        
        }
        public override void OnLoad(ConfigNode node)
        {
            node.TryGetValue("language", ref setLanguage);
            if (setLanguage != "")
            {
                Locales.setLocale(setLanguage);
            }
            else
            {
                setLanguage = Locales.currentLocale.LocaleFull;
            }
        }

    }
}
