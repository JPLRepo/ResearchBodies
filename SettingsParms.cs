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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using KSP.Localization;
using UnityEngine;

namespace ResearchBodies
{
    public class ResearchBodies_SettingsParms : GameParameters.CustomParameterNode

    {
        public override string Title { get { return Localizer.Format("#autoLOC_RBodies_00025"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return true; } }
        public override string Section { get { return "ResearchBodies"; } }
        public override string DisplaySection { get { return Localizer.Format("#autoLOC_RBodies_00026"); } }
        public override int SectionOrder { get { return 1; } }
        
        [GameParameters.CustomParameterUI("#autoLOC_RBodies_00027")] //#autoLOC_RBodies_00027 = ResearchBodies Enabled in this save
        public bool RBEnabled = true;

        [GameParameters.CustomParameterUI("#autoLOC_RBodies_00028", toolTip = "#autoLOC_RBodies_00029", newGameOnly = true)] //#autoLOC_RBodies_00028 = Planet Visibility #autoLOC_RBodies_00029 = Setting this difficulty determines which bodies\nare automatically discovered at the start of the game.
        public Level difficulty = Level.Normal;

        [GameParameters.CustomStringParameterUI("Test String UI", autoPersistance = true, lines = 5,title = "", toolTip = "#autoLOC_RBodies_00031")] //#autoLOC_RBodies_00030 = Celestial Bodies already discovered #autoLOC_RBodies_00031 = Depending on the Difficulty Setting these Bodies will be\n already discovered at the start of the game.
        public string CBstring = "#autoLOC_RBodies_00030";

        [GameParameters.CustomIntParameterUI("#autoLOC_RBodies_00032", toolTip = "#autoLOC_RBodies_00033", maxValue = 500000, stepSize = 1000, gameMode = GameParameters.GameMode.CAREER, newGameOnly = true)] //#autoLOC_RBodies_00032 = Research plan start cost #autoLOC_RBodies_00033 = The cost to start a research plan
        public int ResearchCost = 70000;

        [GameParameters.CustomIntParameterUI("#autoLOC_RBodies_00034", toolTip = "#autoLOC_RBodies_00035", maxValue = 500000, stepSize = 1000, gameMode = GameParameters.GameMode.CAREER, newGameOnly = true)] //#autoLOC_RBodies_00034 = Research progression step cost #autoLOC_RBodies_00035 = The cost to progress research
        public int ProgressResearchCost = 45000;

        [GameParameters.CustomIntParameterUI("#autoLOC_RBodies_00036", toolTip = "#autoLOC_RBodies_00037", maxValue = 1000, stepSize = 10, gameMode = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE, newGameOnly = true)] //#autoLOC_RBodies_00036 = Science reward on body found #autoLOC_RBodies_00037 = How much science you get when you find a celestial body
        public int ScienceReward = 150;
        
        [GameParameters.CustomIntParameterUI("#autoLOC_RBodies_00038", toolTip = "#autoLOC_RBodies_00039", minValue = 1, maxValue = 6, stepSize = 1, newGameOnly = true)] //#autoLOC_RBodies_00038 = Body Discovery Chance Seed Value #autoLOC_RBodies_00039 = The higher this value the harder it will be to find a new Celestial Body
        public int DiscoverySeed = 3;
              
        [GameParameters.CustomParameterUI("#autoLOC_RBodies_00043", toolTip = "#autoLOC_RBodies_00044")] //#autoLOC_RBodies_00043 = Extra Debug Logging #autoLOC_RBodies_00044 = Turn this On to capture lots of extra information into the KSP log for reporting a problem.
        public bool DebugLogging = false;

        [GameParameters.CustomParameterUI("#autoLOC_RBodies_00045", toolTip = "#autoLOC_RBodies_00046")] //#autoLOC_RBodies_00045 = Observatory available at T/S Level 1 #autoLOC_RBodies_00046 = If this option is On the Observatory is available when the Tracking station is Level 1,\nif off the Tracking station must be level 2 or 3.
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
            return true; //otherwise return true
        }
    }
}
