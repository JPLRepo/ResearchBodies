/**
* REPOSoftTech KSP Utilities
* (C) Copyright 2015, Jamie Leighton
*
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
* 
*
* License : MIT
* Copyright (c) 2016 Jamie Leighton 
* Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
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
            string logFileName = AssemblyFolder + "/PluginData/" + AssemblyName + ".log";
            if (File.Exists(logFileName))
            {
                DateTime dateTime = File.GetCreationTime(logFileName);
                string dateTimeFileName = AssemblyFolder + "/PluginData/" + AssemblyName + dateTime.ToString("MMddyyyyHHmmssfff") + ".log";
                if (File.Exists(dateTimeFileName))
                {
                    File.Delete(dateTimeFileName);
                }
                File.Copy(logFileName, dateTimeFileName);
                File.Delete(logFileName);
            }
            Tw = new StreamWriter(logFileName);
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

        public static void Flush()
        {
            if (Tw == null)
                return;

            Tw.Flush();
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
