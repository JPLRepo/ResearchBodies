/*
 * ResearchBodies.cs
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
using UnityEngine;
using RSTUtils;

namespace ResearchBodies
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT,
        GameScenes.TRACKSTATION)]
    public class ResearchBodies : ScenarioModule
    {
        public static ResearchBodies Instance;
        public static bool APIReady;
        internal RBGameSettings RBgameSettings;
        private List<Component> children = new List<Component>();

        private bool _enabled;
        public static bool Enabled
        {
            get { return Instance._enabled;  }
            internal set { Instance._enabled = value; }
        }

        public ResearchBodies()
        {
            RSTLogWriter.Log("ResearchBodies Constructor");
            if (Instance != null)
            {
                RSTLogWriter.Log("Instance exists, destroying Usurper");
                Destroy(this);
            }
            Instance = this;
            APIReady = false;
            RBgameSettings = new RBGameSettings();
        }

        public override void OnAwake()
        {
            RSTLogWriter.Log("OnAwake in " + HighLogic.LoadedScene);
            base.OnAwake();

            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                RSTLogWriter.Log("Adding SpaceCenterManager");
                var RBC = gameObject.AddComponent<ResearchBodiesController>();
                children.Add(RBC);
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                RSTLogWriter.Log("Adding FlightManager");
                var RBC = gameObject.AddComponent<ResearchBodiesController>();
                children.Add(RBC);
            }
            else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                RSTLogWriter.Log("Adding EditorController");
                var RBC = gameObject.AddComponent<ResearchBodiesController>();
                children.Add(RBC);
            }
            else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                RSTLogWriter.Log("Adding TrackingStationController");
                var RBC = gameObject.AddComponent<ResearchBodiesController>();
                children.Add(RBC);
            }
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            RBgameSettings.Load(gameNode);
            if (Database.instance.RB_SettingsParms != null)
                RSTLogWriter.debuggingOn = Database.instance.RB_SettingsParms.DebugLogging;
            APIReady = true;
            if (RSTLogWriter.debuggingOn)
                RSTLogWriter.Log_Debug("Scenario: " + HighLogic.LoadedScene + " OnLoad: \n ");
            //    RSTLogWriter.Log_Debug("Scenario: " + HighLogic.LoadedScene + " OnLoad: \n " + gameNode + "\n" + globalNode);
            else
            {
                RSTLogWriter.Log("ResearchBodies Scenario Onload Completed.");
            }
            RSTLogWriter.Flush();
        }

        public override void OnSave(ConfigNode gameNode)
        {
            //APIReady = false;
            base.OnSave(gameNode);
            RBgameSettings.Save(gameNode);
            //foreach (Savable s in children.Where(c => c is Savable))
            //{
            //    s.Save(globalNode);
            //}
            //globalNode.Save(globalConfigFilename);
            if (RSTLogWriter.debuggingOn)
                RSTLogWriter.Log_Debug("Scenario: " + HighLogic.LoadedScene + " OnSave: \n");
            //    RSTLogWriter.Log_Debug("Scenario: " + HighLogic.LoadedScene + " OnSave: \n" + gameNode + "\n" + globalNode);
            else
            {
                RSTLogWriter.Log("ResearchBodies Scenario OnSave completed.");
            }
            RSTLogWriter.Flush();
        }

        protected void OnGameSceneLoadRequested(GameScenes gameScene)
        {
            RSTLogWriter.Log("Game scene load requested: " + gameScene);
        }

        protected void OnDestroy()
        {
            RSTLogWriter.Log("OnDestroy");
            Instance = null;
            APIReady = false;
            foreach (Component child in children)
            {
                RSTLogWriter.Log("ResearchBodies Child Destroy for " + child.name);
                Destroy(child);
            }
            children.Clear();
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
        }
    }
}
