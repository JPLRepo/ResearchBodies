/*
 * ResearchBodies.cs
 * (C) Copyright 2016, Jamie Leighton 
 * License Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International
 * http://creativecommons.org/licenses/by-nc-sa/4.0/
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  ResearchBodies is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
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
        //private readonly string globalConfigFilename;
        //private ConfigNode globalNode = new ConfigNode();
        private List<Component> children = new List<Component>();

        private bool _enabled;
        public static bool enabled
        {
            get { return Instance._enabled;  }
            private set { Instance._enabled = value; }
        }

        public ResearchBodies()
        {
            RSTLogWriter.Log("ResearchBodies Constructor");
            Instance = this;
            APIReady = false;
            RBgameSettings = new RBGameSettings();
            //globalConfigFilename = Path.Combine(RSTLogWriter.AssemblyFolder, "PluginData/Config.cfg").Replace("\\", "/");
            //RSTLogWriter.Log("globalConfigFilename = " + globalConfigFilename);
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
            // Load the global settings
            //if (File.Exists(globalConfigFilename))
            //{
            //    globalNode = ConfigNode.Load(globalConfigFilename);
            //    foreach (Savable s in children.Where(c => c is Savable))
            //    {
            //        s.Load(globalNode);
            //    }
            //}
            RSTLogWriter.debuggingOn = RBgameSettings.DebugLogging;
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX && !Database.instance.enableInSandbox)
                RBgameSettings.Enabled = false;
            enabled = RBgameSettings.Enabled;
            APIReady = true;
            if (RSTLogWriter.debuggingOn)
                RSTLogWriter.Log_Debug("Scenario: " + HighLogic.LoadedScene + " OnLoad: \n ");
            //    RSTLogWriter.Log_Debug("Scenario: " + HighLogic.LoadedScene + " OnLoad: \n " + gameNode + "\n" + globalNode);
            else
            {
                RSTLogWriter.Log("ResearchBodies Scenario Onload Completed.");
            }
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

    //internal interface Savable
    //{
    //    void Load(ConfigNode globalNode);

    //    void Save(ConfigNode globalNode);
    //}
}
