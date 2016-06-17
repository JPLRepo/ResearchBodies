
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace ResearchBodies
{
    /// <summary>
    /// The Wrapper class to access Progressive CB Maps
    /// </summary>
    public class PCBMWrapper
    {
        protected static System.Type PCBMAPIType;
        protected static Object actualPCBMAPIType;
        internal static System.Type PCBMCelestialBodyInfoType;

        /// <summary>
        /// This is the TST API object
        ///
        /// SET AFTER INIT
        /// </summary>
        public static PCBMAPI actualPCBMAPI;

        /// <summary>
        /// Whether we found the Progressive CB Maps API assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyPCBMExists { get { return (PCBMAPIType != null); } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _PCBMWrapped = false;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIPCBMReady { get { return _PCBMWrapped; } }

        /// <summary>
        /// This method will set up the Progressive CB Maps object and wrap all the methods/functions
        /// </summary>        
        /// <returns>Bool indicating success</returns>
        public static Boolean InitPCBMWrapper()
        {
            //reset the internal objects
            _PCBMWrapped = false;
            actualPCBMAPI = null;
            LogFormatted("Attempting to Grab Progressive CB Maps Types...");

            //find the base type
            PCBMAPIType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "ProgressiveCBMaps.VisualMaps");

            if (PCBMAPIType == null)
            {
                return false;
            }

            LogFormatted("Progressive CB Maps Version:{0}", PCBMAPIType.Assembly.GetName().Version.ToString());

            //now grab the running instance
            LogFormatted("Got Assembly Types, grabbing Instances");
            try
            {
                actualPCBMAPIType = PCBMAPIType.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).GetValue(null);
            }
            catch (Exception)
            {
                LogFormatted("No Progressive CB Maps Instance found");
            }

            if (actualPCBMAPIType == null)
            {
                LogFormatted("Failed grabbing Instance");
                return false;
            }

            //find the CelestialBodyInfo Type
            PCBMCelestialBodyInfoType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "ProgressiveCBMaps.CelestialBodyInfo");

            if (PCBMCelestialBodyInfoType == null)
            {
                return false;
            }

            //If we get this far we can set up the local object and its methods/functions
            LogFormatted("Got Instance, Creating Wrapper Objects");
            actualPCBMAPI = new PCBMAPI(actualPCBMAPIType);

            _PCBMWrapped = true;
            return true;
        }


        /// <summary>
        /// The Type that is an analogue of the real Progressive CB Maps. This lets you access all the API-able properties and Methods of Progressive CB Maps VisualMaps class
        /// </summary>
        public class PCBMAPI
        {
            internal PCBMAPI(Object a)
            {
                //store the actual object
                APIactualPCBM = a;

                //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
                //for events we also add a handler

                //WORK OUT THE STUFF WE NEED TO HOOK FOR PEOPLE HERE

                
                LogFormatted("Getting CBVisualMapsInfo Object");
                CBVisualMapsInfoMethod = PCBMAPIType.GetMethod("get_CBVisualMapsInfo", BindingFlags.Public | BindingFlags.Instance);
                actualCBVisualMapsInfo = CBVisualMapsInfoMethod.Invoke(APIactualPCBM, null);
                LogFormatted("Success: " + (actualCBVisualMapsInfo != null).ToString());
            }

            private Object APIactualPCBM;
            private PropertyInfo TSTCBGalaxiesField;
            private MethodInfo TSTCBGAlaxiesGetMethod;

            #region CBVisualMapsInfo

            private object actualCBVisualMapsInfo;
            private MethodInfo CBVisualMapsInfoMethod;

            /// <summary>
            /// The dictionary of CBVisualMapsInfo that are currently active in game
            /// </summary>
            /// <returns>
            /// Dictionary <CelestialBody, PCBMCelestialBodyInfo>
            /// </returns>
            internal Dictionary<CelestialBody, PCBMCelestialBodyInfo> CBVisualMapsInfo
            {
                get
                {
                    Dictionary<CelestialBody, PCBMCelestialBodyInfo> returnvalue = new Dictionary<CelestialBody, PCBMCelestialBodyInfo>();
                    if (CBVisualMapsInfoMethod == null)
                    {
                        LogFormatted("Error getting CBVisualMapsInfo - Reflection Method is Null");
                        return returnvalue;
                    }

                    actualCBVisualMapsInfo = null;
                    actualCBVisualMapsInfo = CBVisualMapsInfoMethod.Invoke(APIactualPCBM, null);
                    returnvalue = ExtractCBVisualMapsInfoDict(actualCBVisualMapsInfo);
                    return returnvalue;
                }
            }

            /// <summary>
            /// This converts the actualFrozenKerbals actual object to a new dictionary for consumption
            /// </summary>
            /// <param name="actualCBVisualMapsInfo"></param>
            /// <returns>
            /// Dictionary <string, KerbalInfo> of Frozen Kerbals
            /// </returns>
            private Dictionary<CelestialBody, PCBMCelestialBodyInfo> ExtractCBVisualMapsInfoDict(Object actualCBVisualMapsInfo)
            {
                Dictionary<CelestialBody, PCBMCelestialBodyInfo> DictToReturn = new Dictionary<CelestialBody, PCBMCelestialBodyInfo>();
                try
                {
                    foreach (var item in (IDictionary)actualCBVisualMapsInfo)
                    {
                        var typeitem = item.GetType();
                        PropertyInfo[] itemprops = typeitem.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        CelestialBody itemkey = (CelestialBody)itemprops[0].GetValue(item, null);
                        object itemvalue = (object)itemprops[1].GetValue(item, null);
                        PCBMCelestialBodyInfo itemkerbalinfo = new PCBMCelestialBodyInfo(itemvalue);
                        DictToReturn[itemkey] = itemkerbalinfo;
                    }
                }
                catch (Exception ex)
                {
                    LogFormatted("Unable to extract FrozenKerbals Dictionary: {0}", ex.Message);
                }
                return DictToReturn;
            }

            #endregion CBVisualMapsInfo
        }

        /// <summary>
        /// The Value Class of the CBVisualMapsInfo Dictionary that is an analogue of the real CBVisualMapsInfo Dictionary in the VisualMaps Class.
        /// </summary>
        public class PCBMCelestialBodyInfo
        {
            internal PCBMCelestialBodyInfo(Object a)
            {
                actualCelestialBodyInfo = a;
                BodyField = PCBMCelestialBodyInfoType.GetField("Body");
                currentDetailLevelField = PCBMCelestialBodyInfoType.GetField("currentDetailLevel");
                setVisualLevelMethod = PCBMCelestialBodyInfoType.GetMethod("setVisualLevel", BindingFlags.Public | BindingFlags.Instance);
            }
            
            private Object actualCelestialBodyInfo;

            private FieldInfo BodyField;

            /// <summary>
            /// The CelestialBody
            /// </summary>
            public CelestialBody Body
            {
                get { return (CelestialBody)BodyField.GetValue(actualCelestialBodyInfo); }
            }

            private FieldInfo currentDetailLevelField;

            /// <summary>
            /// The CBs mesh
            /// </summary>
            public int currentDetailLevel
            {
                get { return (int)currentDetailLevelField.GetValue(actualCelestialBodyInfo); }
            }

            private MethodInfo setVisualLevelMethod;

            public bool setVisualLevel(int level)
            {
                try
                {
                    setVisualLevelMethod.Invoke(actualCelestialBodyInfo, new System.Object[] { level });
                    return true;
                }
                catch (Exception ex)
                {
                    LogFormatted("Arrggg: {0}", ex.Message);
                    return false;
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
