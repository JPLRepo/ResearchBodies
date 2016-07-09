/*
 * TSTWrapper.cs
 * (C) Copyright 2015, Jamie Leighton
 * RepoSoftTech 
 * This code is subject and covered by the Creative Commons (CC-BY-NC-SA 4.0) license.
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. 
 * This project is in no way associated with nor endorsed by Squad.
 *
 *  This is free software: you can redistribute it and/or modify
 *  it under the terms of the MIT License 
 *
 *  This software is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 *
 *  You should have received a copy of the License along with this software. 
 *  If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.
 *
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ResearchBodies
{
    /// <summary>
    /// The Wrapper class to access TarsierSpaceTechnologies
    /// </summary>
    public class TSTWrapper
    {
        protected static System.Type TSTGalaxiesAPIType;        
        protected static Object actualTSTGalaxies;
        
        /// <summary>
        /// This is the TST API object
        ///
        /// SET AFTER INIT
        /// </summary>
        public static TSTAPI actualTSTAPI;       

        /// <summary>
        /// Whether we found the TST API assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyTSTExists { get { return (TSTGalaxiesAPIType != null); } }        

        /// <summary>
        /// Whether we managed to hook the running Instance from the assembly.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean InstanceSCExists { get { return (actualTSTGalaxies != null); } }
        
        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _TSTWrapped = false;        

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APITSTReady { get { return _TSTWrapped; } }        

        /// <summary>
        /// This method will set up the TST object and wrap all the methods/functions
        /// </summary>        
        /// <returns>Bool indicating success</returns>
        public static Boolean InitTSTWrapper()
        {
            //reset the internal objects
            _TSTWrapped = false;
            actualTSTAPI = null;
            actualTSTGalaxies = null;
            LogFormatted("Attempting to Grab TST Types...");

            //find the base type
            TSTGalaxiesAPIType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "TarsierSpaceTech.TSTGalaxies");

            if (TSTGalaxiesAPIType == null)
            {
                return false;
            }

            LogFormatted("TarsierSpaceTech Version:{0}", TSTGalaxiesAPIType.Assembly.GetName().Version.ToString());

            //now grab the running instance
            LogFormatted("Got Assembly Types, grabbing Instances");
            try
            {
                actualTSTGalaxies = TSTGalaxiesAPIType.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).GetValue(null); 
            }
            catch (Exception)
            {
                LogFormatted("No TarsierSpaceTech Galaxies Instance found");                
            }

            if (actualTSTGalaxies == null)
            {
                LogFormatted("Failed grabbing Instance");
                return false;
            }
                        
            //If we get this far we can set up the local object and its methods/functions
            LogFormatted("Got Instance, Creating Wrapper Objects");
            actualTSTAPI = new TSTAPI(actualTSTGalaxies);

            _TSTWrapped = true;
            return true;
        }

        
        /// <summary>
        /// The Type that is an analogue of the real TarsierSpaceTech Galaxies. This lets you access all the API-able properties and Methods of TarsierSpaceTech Galaxies class
        /// </summary>
        public class TSTAPI
        {
            internal TSTAPI(Object a)
            {
                //store the actual object
                APIactualTST = a;

                //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
                //for events we also add a handler

                //WORK OUT THE STUFF WE NEED TO HOOK FOR PEOPLE HERE
                
                LogFormatted("Getting CBGalaxies field");
                TSTCBGalaxiesField = TSTGalaxiesAPIType.GetProperty("CBGalaxies", BindingFlags.Public | BindingFlags.Static);
                TSTCBGAlaxiesGetMethod = TSTCBGalaxiesField.GetGetMethod(true);
                LogFormatted_DebugOnly("TSTCBGalaxiesField Success: " + (TSTCBGalaxiesField != null).ToString());
                LogFormatted_DebugOnly("TSTCBGAlaxiesGetMethodSuccess: " + (TSTCBGAlaxiesGetMethod != null).ToString());
            }

            private Object APIactualTST;            
            private PropertyInfo TSTCBGalaxiesField;
            private MethodInfo TSTCBGAlaxiesGetMethod;

            /// <summary>
            /// This is the TST Galaxies List of galaxies in CelstialBody class format.
            /// </summary>
            /// <returns>List<CelestialBody> containing list of TST Galaxies</returns>
            public List<CelestialBody> CBGalaxies
            {
                get
                {
                    if (TSTCBGalaxiesField == null)
                        return new List<CelestialBody>();

                    return (List<CelestialBody>)TSTCBGAlaxiesGetMethod.Invoke(APIactualTST, null);
                }
            }            
        }
        

        #region Logging Stuff

        /// <summary>
        /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void LogFormatted_DebugOnly(String Message, params Object[] strParams)
        {
            LogFormatted(Message, strParams);
        }

        /// <summary>
        /// Some Structured logging to the debug file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        internal static void LogFormatted(String Message, params Object[] strParams)
        {
            Message = String.Format(Message, strParams);
            String strMessageLine = String.Format("{0},{2}-{3},{1}",
                DateTime.Now, Message, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            UnityEngine.Debug.Log(strMessageLine);
        }

        #endregion Logging Stuff
    }
}