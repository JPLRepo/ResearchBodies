/*
 * RBWrapper.cs
 * License : MIT
 * Copyright (c) 2016 Jamie Leighton 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace TarsierSpaceTech
{
    /// <summary>
    /// The Wrapper class to access ResearchBodies
    /// </summary>
    public class RBWrapper
    {
        protected static Type RBAPIType;
        protected static Type RBSCAPIType;
        protected static Type RBDBAPIType;
        protected static Type RBDBCelestialBodyType;
        protected static Object actualRB;
        protected static Object actualRBSC;
        protected static Object actualRBDB;

        /// <summary>
        /// This is the ResearchBodies API object
        ///
        /// SET AFTER INIT
        /// </summary>
        public static RBAPI RBactualAPI;
        

        /// <summary>
        /// Whether we found the ResearchBodies API assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyRBExists { get { return (RBAPIType != null); } }
        public static Boolean AssemblySCExists { get { return (RBSCAPIType != null); } }
        public static Boolean AssemblyDBExists { get { return (RBDBAPIType != null); } }
        
        /// <summary>
        /// Whether we managed to hook the running Instance from the assembly.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean InstanceExists { get { return (actualRB != null); } }
        public static Boolean InstanceSCExists { get { return (actualRBSC != null); } }
        public static Boolean InstanceDBExists { get { return (actualRBDB != null); } }
        
        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _RBWrapped;

        /// <summary>
        /// Whether the objects have been wrapped
        /// </summary>
        public static Boolean APIRBReady { get { return _RBWrapped; } }

        /// <summary>
        /// This method will set up the ResearchBodies object and wrap all the methods/functions
        /// </summary>
        /// <returns>Bool success of method</returns>
        public static Boolean InitRBWrapper()
        {
            //reset the internal objects
            _RBWrapped = false;
            actualRB = null;
            actualRBSC = null;
            actualRBDB = null;

            LogFormatted("Attempting to Grab ResearchBodies Types...");

            //find the base type
            RBAPIType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "ResearchBodies.ResearchBodies");

            if (RBAPIType == null)
            {
                return false;
            }

            LogFormatted("ResearchBodies Version:{0}", RBAPIType.Assembly.GetName().Version.ToString());
            
            //find the base type
            RBSCAPIType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "ResearchBodies.ResearchBodiesController");

            if (RBSCAPIType == null)
            {
                return false;
            }

            //find the base type
            RBDBAPIType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "ResearchBodies.Database");

            if (RBDBAPIType == null)
            {
                return false;
            }

            //find the CelestialBodyInfo type
            RBDBCelestialBodyType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "ResearchBodies.CelestialBodyInfo");

            if (RBDBCelestialBodyType == null)
            {
                return false;
            }

            //now grab the running instances
            LogFormatted("Got Assembly Types, grabbing Instances");
            try
            {
                actualRB = RBAPIType.GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            }
            catch (Exception)
            {
                LogFormatted("No ResearchBodies.ResearchBodies Instance found");
                //throw;
            }

            if (actualRB == null)
            {
                LogFormatted("Failed grabbing ResearchBodies.ResearchBodies Instance");
                return false;
            }

            try
            {
                actualRBSC = RBSCAPIType.GetField("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            }
            catch (Exception)
            {
                LogFormatted("No ResearchBodies.ResearchBodiesController Instance found");
                //throw;
            }

            if (actualRBSC == null)
            {
                LogFormatted("Failed grabbing ResearchBodies.ResearchBodiesController Instance");
                return false;
            }

            try
            {
                actualRBDB = RBDBAPIType.GetField("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            }
            catch (Exception)
            {
                LogFormatted("No ResearchBodies.Database Instance found");
                //throw;
            }

            if (actualRBDB == null)
            {
                LogFormatted("Failed grabbing ResearchBodies.Database Instance");
                return false;
            }

            //If we get this far we can set up the local object and its methods/functions            
            RBactualAPI = new RBAPI(actualRB, actualRBSC, actualRBDB);
            _RBWrapped = true;
            return true;
        }
        
        

        /// <summary>
        /// The Type that is an analogue of the real ResearchBodies. This lets you access all the API-able properties and Methods of ResearchBodies
        /// </summary>
        public class RBAPI
        {
            internal RBAPI(Object actualRB, Object actualRBSC, Object actualRBDB)
            {
                //store the actual object
                APIactualRB = actualRB;
                APIactualRBSC = actualRBSC;
                APIactualRBDB = actualRBDB;

                //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
                //for events we also add a handler

                //WORK OUT THE STUFF WE NEED TO HOOK FOR PEOPLE HERE

                //Methods
                
                enabledMethod = RBAPIType.GetMethod("get_enabled", BindingFlags.Public | BindingFlags.Static);                
                FoundBodyMethod = RBSCAPIType.GetMethod("FoundBody", BindingFlags.Public | BindingFlags.Static);                
                ResearchMethod = RBSCAPIType.GetMethod("Research", BindingFlags.Public | BindingFlags.Static);                
                LaunchResearchPlanMethod = RBSCAPIType.GetMethod("LaunchResearchPlan", BindingFlags.Public | BindingFlags.Static);                
                StopResearchPlanMethod = RBSCAPIType.GetMethod("StopResearchPlan", BindingFlags.Public | BindingFlags.Static);                
                CelestialBodiesField = RBDBAPIType.GetField("CelestialBodies", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);              

            }

            private Object APIactualRB;
            private Object APIactualRBSC;
            private Object APIactualRBDB;

            #region Methods

            private MethodInfo enabledMethod;

            internal bool enabled
            {
                get
                {
                    return (bool)enabledMethod.Invoke(APIactualRB, null);
                }
            }

            private MethodInfo FoundBodyMethod;

            /// <summary>
            /// Set a Celestial Body to isResearched (Found it!)
            /// </summary>
            /// <param name="scienceReward">The amount of science points to reward (is added to the base Science points reward)</param>
            /// <param name="bodyFound">The celestialbody found</param>
            /// <param name="withParent">True if found with parent body, otherwise false</param>
            /// <param name="parentBody">The parent celestial body if found (withParent = True), otherwise returns null</param>
            /// <returns>Bool indicating Success of call</returns>
            internal bool FoundBody(int scienceReward, CelestialBody bodyFound, out bool withParent, out CelestialBody parentBody)
            {
                try
                {
                    withParent = false;
                    parentBody = null;
                    object[] args = new object[] { scienceReward, bodyFound, withParent, parentBody };
                    bool result = (bool)FoundBodyMethod.Invoke(APIactualRBSC, args);
                    withParent = (bool) args[2];
                    parentBody = (CelestialBody) args[3];
                    return result;
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke Research Method");
                    LogFormatted("Exception: {0}", ex);
                    withParent = false;
                    parentBody = null;
                    return false;
                    //throw;
                }
            }

            private MethodInfo ResearchMethod;

            /// <summary>
            /// Add Research points to a celestialbody
            /// </summary>
            /// <param name="body">The celestialbody</param>
            /// /// <param name="researchToAdd">How much research to add for the celestialbody</param>
            /// <returns>Bool indicating Success of call</returns>
            internal bool Research(CelestialBody body, int researchToAdd)
            {
                try
                {
                    return (bool)ResearchMethod.Invoke(APIactualRBSC, new Object[] { body, researchToAdd });
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke Research Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            private MethodInfo LaunchResearchPlanMethod;

            /// <summary>
            /// Start a ResearchPlan for a celestialbody
            /// </summary>
            /// <param name="cb">The celestialbody</param>
            /// <returns>Bool indicating Success of call</returns>
            internal bool LaunchResearchPlan(CelestialBody cb)
            {
                try
                {
                    LaunchResearchPlanMethod.Invoke(APIactualRBSC, new Object[] { cb });
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke LaunchResearchPlan Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            private MethodInfo StopResearchPlanMethod;

            /// <summary>
            /// Stop Research plan for a celestialbody
            /// </summary>
            /// <param name="cb">The celestialbody</param>
            /// <returns>Bool indicating success of call</returns>
            internal bool StopResearchPlan(CelestialBody cb)
            {
                try
                {
                    StopResearchPlanMethod.Invoke(APIactualRBSC, new Object[] { cb });
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to invoke StopResearchPlan Method");
                    LogFormatted("Exception: {0}", ex);
                    return false;
                    //throw;
                }
            }

            #endregion Methods

            private object actualCelestialBodies;
            private FieldInfo CelestialBodiesField;

            /// <summary>
            /// The dictionary of CelestialBodies information
            /// </summary>
            /// <returns>
            /// Dictionary <CelestialBody, CelestialBodyInfo> of ResearchBodies CelestialBody Information
            /// </returns>
            internal Dictionary<CelestialBody, CelestialBodyInfo> CelestialBodies
            {
                get
                {
                    Dictionary<CelestialBody, CelestialBodyInfo> returnvalue = new Dictionary<CelestialBody, CelestialBodyInfo>();
                    if (CelestialBodiesField == null)
                    {
                        LogFormatted("Error getting CelestialBodyInfo - Reflection Method is Null");
                        return returnvalue;
                    }

                    actualCelestialBodies = null;
                    actualCelestialBodies = CelestialBodiesField.GetValue(APIactualRBDB);
                    returnvalue = ExtractCelestialBodiesInfoDict(actualCelestialBodies);
                    return returnvalue;
                }
            }

            /// <summary>
            /// This converts the actualCelestialBodies actual object to a new dictionary for consumption
            /// </summary>
            /// <param name="actualCelestialBodies"></param>
            /// <returns>
            /// Dictionary <CelestialBody, CelestialBodyInfo> of CelestialBody Info
            /// </returns>
            private Dictionary<CelestialBody, CelestialBodyInfo> ExtractCelestialBodiesInfoDict(Object actualCelestialBodies)
            {
                Dictionary<CelestialBody, CelestialBodyInfo> DictToReturn = new Dictionary<CelestialBody, CelestialBodyInfo>();
                try
                {
                    foreach (var item in (IDictionary)actualCelestialBodies)
                    {
                        var typeitem = item.GetType();
                        PropertyInfo[] itemprops = typeitem.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        CelestialBody itemkey = (CelestialBody)itemprops[0].GetValue(item, null);
                        object itemvalue = (object)itemprops[1].GetValue(item, null);
                        CelestialBodyInfo itemCBinfo = new CelestialBodyInfo(itemvalue);
                        DictToReturn[itemkey] = itemCBinfo;
                    }
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to extract CelestialBodies Dictionary: {0}", ex.Message);
                }
                return DictToReturn;
            }
        }

        /// <summary>
        /// The Type that is an analogue of the real ResearchBodies.CelestialBodyInfo class. 
        /// </summary>
        public class CelestialBodyInfo
        {
            internal CelestialBodyInfo(Object a)
            {
                //store the actual object
                APIactualCelestialBodyInfo = a;

                //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
                //for events we also add a handler

                //WORK OUT THE STUFF WE NEED TO HOOK FOR PEOPLE HERE                
                bodyField = RBDBCelestialBodyType.GetField("body", BindingFlags.Public | BindingFlags.Instance);                
                isResearchedField = RBDBCelestialBodyType.GetField("isResearched", BindingFlags.Public | BindingFlags.Instance);                
                researchStateField = RBDBCelestialBodyType.GetField("researchState", BindingFlags.Public | BindingFlags.Instance);                
                ignoreField = RBDBCelestialBodyType.GetField("ignore", BindingFlags.Public | BindingFlags.Instance);                
                priorityField = RBDBCelestialBodyType.GetField("priority", BindingFlags.Public | BindingFlags.Instance);                
                discoveryMessageField = RBDBCelestialBodyType.GetField("discoveryMessage", BindingFlags.Public | BindingFlags.Instance);                
                KOPbarycenterField = RBDBCelestialBodyType.GetField("KOPbarycenter", BindingFlags.Public | BindingFlags.Instance);                
                KOPrelbarycenterBodyField = RBDBCelestialBodyType.GetField("KOPrelbarycenterBody", BindingFlags.Public | BindingFlags.Instance);                
            }

            private Object APIactualCelestialBodyInfo;

            private FieldInfo bodyField;

            /// <summary>
            /// The name of the celestialbody (found)
            /// </summary>
            /// <returns>String value of body field</returns>
            public string body
            {
                get
                {
                    if (bodyField == null)
                        return "";

                    return (string)bodyField.GetValue(APIactualCelestialBodyInfo);
                }
            }

            private FieldInfo isResearchedField;

            /// <summary>
            /// Whether the body has been researched (found)
            /// </summary>
            /// <returns>Bool value of isResearched field</returns>
            public Boolean isResearched
            {
                get
                {
                    if (isResearchedField == null)
                        return false;

                    return (Boolean)isResearchedField.GetValue(APIactualCelestialBodyInfo);
                }
            }

            private FieldInfo researchStateField;

            /// <summary>
            /// Integer value of research percent
            /// </summary>
            /// <returns>Int value of researchState field</returns>
            public int researchState
            {
                get
                {
                    if (researchStateField == null)
                        return 0;

                    return (int)researchStateField.GetValue(APIactualCelestialBodyInfo);
                }
            }

            private FieldInfo ignoreField;

            /// <summary>
            /// Whether the body is ignored or not (available by default) based on difficulty level
            /// </summary>
            /// <returns>Bool value of ignore field</returns>
            public Boolean ignore
            {
                get
                {
                    if (ignoreField == null)
                        return false;

                    return (Boolean)ignoreField.GetValue(APIactualCelestialBodyInfo);
                }
            }

            private FieldInfo priorityField;

            /// <summary>
            /// Integer value of priority - not currently used
            /// </summary>
            /// <returns>Int value of priority field</returns>
            public int priority
            {
                get
                {
                    if (priorityField == null)
                        return 0;

                    return (int)priorityField.GetValue(APIactualCelestialBodyInfo);
                }
            }

            private FieldInfo discoveryMessageField;

            /// <summary>
            /// The discoveryMessage for the celestialbody (when found)
            /// </summary>
            /// <returns>String value of discoveryMessage field</returns>
            public string discoveryMessage
            {
                get
                {
                    if (discoveryMessageField == null)
                        return "";

                    return (string)discoveryMessageField.GetValue(APIactualCelestialBodyInfo);
                }
            }

            private FieldInfo KOPbarycenterField;

            /// <summary>
            /// Whether the body has a Kopernicus Barycenter
            /// </summary>
            /// <returns>Bool value of KOPbarycenter field</returns>
            public Boolean KOPbarycenter
            {
                get
                {
                    if (KOPbarycenterField == null)
                        return false;

                    return (Boolean)KOPbarycenterField.GetValue(APIactualCelestialBodyInfo);
                }
            }

            private FieldInfo KOPrelbarycenterBodyField;

            /// <summary>
            /// The celestialbody that is the Kopernicus Barycenter
            /// </summary>
            /// <returns>CelestialBody value of KOPrelbarycenterBody field</returns>
            public CelestialBody KOPrelbarycenterBody
            {
                get
                {
                    if (KOPrelbarycenterBodyField == null)
                        return null;

                    return (CelestialBody)KOPrelbarycenterBodyField.GetValue(APIactualCelestialBodyInfo);
                }
            }

        }
        
        #region Logging Stuff
        
        /// <summary>
        /// Some Structured logging to the debug file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        internal static void LogFormatted(String Message, params Object[] strParams)
        {
            Message = String.Format(Message, strParams);
            String strMessageLine = String.Format("{0},{2}-{3},{1}",
                DateTime.Now, Message, Assembly.GetExecutingAssembly().GetName().Name,
                MethodBase.GetCurrentMethod().DeclaringType.Name);
            Debug.Log(strMessageLine);
        }

        #endregion Logging Stuff
    }
}
