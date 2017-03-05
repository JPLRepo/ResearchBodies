/*
 * SettingsParms.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RSTUtils;
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

        [GameParameters.CustomParameterUI("Planet Visibility", toolTip = "Setting this difficulty determines which bodies\nare automatically discovered at the start of the game.", newGameOnly = true)]
        public Level difficulty = Level.Normal;

        [GameParameters.CustomStringParameterUI("Test String UI", autoPersistance = true, lines = 5,title = "Celestial Bodies already discovered", toolTip = "Depending on the Difficulty Setting these Bodies will be\n already discovered at the start of the game.")]
        public string CBstring = "";

        [GameParameters.CustomIntParameterUI("Research Plan Start Cost", toolTip = "The cost to start researching a Celestial Body", maxValue = 500000, stepSize = 1000, gameMode = GameParameters.GameMode.CAREER, newGameOnly = true)]
        public int ResearchCost = 70000;

        [GameParameters.CustomIntParameterUI("Research Progression Step Cost", toolTip = "The cost for each research step on a Celestial Body", maxValue = 500000, stepSize = 1000, gameMode = GameParameters.GameMode.CAREER, newGameOnly = true)]
        public int ProgressResearchCost = 45000;

        [GameParameters.CustomIntParameterUI("Science Reward on Body Found", toolTip = "The amount of science rewarded for finding a new Celestial Body", maxValue = 1000, stepSize = 10, gameMode = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE, newGameOnly = true)]
        public int ScienceReward = 150;
        
        [GameParameters.CustomIntParameterUI("Body Discovery Chance Seed Value", toolTip = "The higher this value the harder it will be to find a new Celestial Body", minValue = 1, maxValue = 6, stepSize = 1, newGameOnly = true)]
        public int DiscoverySeed = 3;

        //[GameParameters.CustomParameterUI("Use Stock Application Launcher Icon", toolTip = "If on, the Stock Application launcher will be used,\nif off will use Blizzy Toolbar if installed")]
        //public bool UseAppLToolbar = true;

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
                    ResearchCost = 20000;
                    ProgressResearchCost = 20000;
                    ScienceReward = 200;
                    difficulty = Level.Easy;
                    CBstring = Database.instance.GetIgnoredBodies(difficulty);
                    break;
                case GameParameters.Preset.Normal:
                    ResearchCost = 70000;
                    ProgressResearchCost = 45000;
                    ScienceReward = 150;
                    difficulty = Level.Normal;
                    CBstring = Database.instance.GetIgnoredBodies(difficulty);
                    break;
                case GameParameters.Preset.Moderate:
                    ResearchCost = 100000;
                    ProgressResearchCost = 90000;
                    ScienceReward = 50;
                    difficulty = Level.Medium;
                    CBstring = Database.instance.GetIgnoredBodies(difficulty);
                    break;
                case GameParameters.Preset.Hard:
                    ResearchCost = 200000;
                    ProgressResearchCost = 150000;
                    ScienceReward = 20;
                    difficulty = Level.Hard;
                    CBstring = Database.instance.GetIgnoredBodies(difficulty);
                    break;
                case GameParameters.Preset.Custom:
                    CBstring = "how do we deal with custom????";
                    break;
            }
        }
        /*
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "RBEnabled") //RbEnabled must always be enabled.
                return true;
            if (RBEnabled == false)  //Otherwise it depends on the value of RBEnabled if it's false return false
                return false;
            return true; //otherwise return true
        }
        */
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "RBEnabled") //RbEnabled must always be enabled.
                return true;
            if (RBEnabled == false)
                return false;
            if (member.Name == "difficulty")
            {
                CBstring = Database.instance.GetIgnoredBodies(difficulty);
                if (HighLogic.LoadedScene != GameScenes.MAINMENU)
                {
                    return false;
                }
            }
            //if (member.Name == "UseAppLToolbar")
            //{
            //    if (!ToolbarManager.ToolbarAvailable)
            //    {
            //        UseAppLToolbar = true;
            //        return false;
            //    }
            //}
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
            return null;
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
                Locales.setLocale("");
                setLanguage = Locales.currentLocale.LocaleFull;
            }
        }

    }
}
