/**
* REPOSoftTech KSP Utilities
* (C) Copyright 2015, Jamie Leighton
*
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
* 
*
* Licensed under the Attribution-NonCommercial-ShareAlike (CC BY-NC-SA 4.0) creative commons license. 
* See <https://creativecommons.org/licenses/by-nc-sa/4.0/> for full details (except where else specified in this file).
*
*/
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using TextWriter = System.IO.TextWriter;

namespace RSTUtils
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class RSTLogWriter : MonoBehaviour
    {
        private static TextWriter Tw;

        /// <summary>
        /// Name of the Assembly that is running this MonoBehaviour
        /// </summary>
        internal static String AssemblyName
        { get { return Assembly.GetExecutingAssembly().GetName().Name; } }

        /// <summary>
        /// Full Path of the executing Assembly
        /// </summary>
        internal static String AssemblyLocation
        { get { return Assembly.GetExecutingAssembly().Location.Replace("\\", "/"); } }

        /// <summary>
        /// Folder containing the executing Assembly
        /// </summary>
        internal static String AssemblyFolder
        { get { return Path.GetDirectoryName(AssemblyLocation).Replace("\\", "/"); } }



        internal static bool debuggingOn = false;

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            if (File.Exists(AssemblyFolder + "/PluginData/" + AssemblyName + ".log"))
                File.Delete(AssemblyFolder + "/PluginData/" + AssemblyName + ".log");
            Tw = new StreamWriter(AssemblyFolder + "/PluginData/" + AssemblyName + ".log");
            Tw.WriteLine(AssemblyName + Assembly.GetExecutingAssembly().GetName().Version);
            Tw.WriteLine("Loaded up on " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt") + ".");
            Tw.WriteLine();
            GameEvents.onGameSceneLoadRequested.Add(logSceneSwitch);

        }
        public void OnDestroy()
        {
            if (Tw != null) Tw.Close();
            GameEvents.onGameSceneLoadRequested.Remove(logSceneSwitch);
        }
       
        private void logSceneSwitch(GameScenes scene)
        {
            if (Tw != null) Tw.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " [KSP] ==== Scene Switch to " + scene.ToString() + " ! ====");
        }

        /// <summary>
        /// Logging to the debug file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>	

        internal static void Log_Debug(String Message, params object[] strParams)
        {
            if (debuggingOn)
            {
                Log("DEBUG: " + Message, strParams);
            }
            else
            {
                Message = String.Format(Message, strParams);                  // This fills the params into the message
                String strMessageLine = String.Format("{0},{2},{1}",
                    DateTime.Now, Message,
                    AssemblyName);                                           // This adds our standardised wrapper to each line
                if (Tw != null) Tw.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " [LOG] " + strMessageLine);
            }
        }

        /// <summary>
        /// Logging to the log file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>

        internal static void Log(String Message, params object[] strParams)
        {
            Message = String.Format(Message, strParams);                  // This fills the params into the message
            String strMessageLine = String.Format("{0},{2},{1}",
                DateTime.Now, Message,
                AssemblyName);                                           // This adds our standardised wrapper to each line
            Debug.Log(strMessageLine);                        // And this puts it in the log
            if (Tw != null) Tw.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " [LOG] " + strMessageLine);
        }
    }
}
