﻿/*
 * EveWrapper.cs
 * (C) Copyright 2016, Jamie Leighton 
* License : MIT
 * Copyright (c) 2016 Jamie Leighton 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 */
using System;
//using System.Linq;
using System.Reflection;

namespace ProgressiveCBMaps
{
    /// <summary>
    /// The Wrapper class to access EVE
    /// </summary>
    public class EVEWrapper
    {
        internal static System.Type EVECloudsPQSType;
        internal static System.Type EVECloudsMaterialType;
        
        /// <summary>
        /// Whether we found the EVE API assembly in the loadedassemblies.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return EVECloudsPQSType != null; } }

        /// <summary>
        /// Whether we managed to hook the running Instance from the assembly.
        ///
        /// SET AFTER INIT
        /// </summary>
        public static Boolean InstanceExists { get { return EVECloudsPQSType != null; } }

        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance.
        ///
        /// SET AFTER INIT
        /// </summary>
        private static Boolean _EVEWrapped;

        /// <summary>
        /// Whether the object has been wrapped
        /// </summary>
        public static Boolean APIReady { get { return _EVEWrapped; } }

        /// <summary>
        /// This method will set up the EVE object and wrap all the methods/functions
        /// </summary>
        /// <returns></returns>
        public static Boolean InitEVEWrapper()
        {
            //reset the internal objects
            _EVEWrapped = false;
            EVECloudsPQSType = null;
            LogFormatted_DebugOnly("Attempting to Grab EVE Types...");

            //find the base type
            EVECloudsPQSType = getType("Atmosphere.CloudsPQS");
            if (EVECloudsPQSType == null)
            {
                return false;
            }

            EVECloudsMaterialType = getType("Atmosphere.CloudsMaterial");
            if (EVECloudsMaterialType == null)
            {
                return false;
            }

            LogFormatted("EVE Version:{0}", EVECloudsPQSType.Assembly.GetName().Version.ToString());
            
            _EVEWrapped = true;
            return true;
        }

        internal static Type getType(string name)
        {
            Type type = null;
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>

            {
                if (t.FullName == name)
                    type = t;
            }
            );

            if (type != null)
            {
                return type;
            }
            return null;
        }

        /// <summary>
        /// The Type that is an analogue of the real Remote Tech. This lets you access all the API-able properties and Methods of Remote Tech
        /// </summary>
        public class EVECloudsPQS
        { 
            internal EVECloudsPQS(Object a)
            {
                //store the actual object
                actualEVECloudsPQS =a;

                //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
                //for events we also add a handler

                //WORK OUT THE STUFF WE NEED TO HOOK FOR PEOPLE HERE
                //Methods
                LogFormatted_DebugOnly("Getting get_enabled Method");
                    getenabledMethod = EVECloudsPQSType.GetMethod("get_enabled", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (getenabledMethod != null).ToString());

                LogFormatted_DebugOnly("Getting set_enabled Method");
                setenabledMethod = EVECloudsPQSType.GetMethod("set_enabled", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (setenabledMethod != null).ToString());

                LogFormatted_DebugOnly("Getting CelestialBody field");
                CelestialBodyField = EVECloudsPQSType.GetField("celestialBody", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (CelestialBodyField != null).ToString());

                LogFormatted_DebugOnly("Getting CloudsMaterial field");
                CloudsMaterialField = EVECloudsPQSType.GetField("cloudsMaterial", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                LogFormatted_DebugOnly("Success: " + (CloudsMaterialField != null).ToString());
            }

            private Object actualEVECloudsPQS;
            
            private MethodInfo getenabledMethod;
            private MethodInfo setenabledMethod;

            /// <summary>
            /// If the clouds are enabled or not
            /// </summary>
            /// <returns>Success of call</returns>
            internal bool enabled
            {
                get
                {
                    try
                    {
                        return (bool)getenabledMethod.Invoke(actualEVECloudsPQS, null);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke EVE get_enabled Method");
                        LogFormatted("Exception: {0}", ex);
                        return false;
                        //throw;
                    }
                }

                set
                {
                    try
                    {
                        setenabledMethod.Invoke(actualEVECloudsPQS, new object[] { value }); 
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to invoke EVE set_enabled Method");
                        LogFormatted("Exception: {0}", ex);
                        //throw;
                    }
                }
            }

            private FieldInfo CelestialBodyField;

            /// <summary>
            /// If the Cryostat is Disabled or not(=active)
            /// </summary>
            public CelestialBody celestialBody
            {
                get
                {

                    try
                    {
                        return (CelestialBody)CelestialBodyField.GetValue(actualEVECloudsPQS);
                    }
                    catch (Exception ex)
                    {
                        LogFormatted("Unable to get EVE CelestialBody field");
                        LogFormatted("Exception: {0}", ex);
                        return null;
                        //throw;
                    }
                }
            }

            private FieldInfo CloudsMaterialField;
            
            public float _detailScale
            {
                get { return (float) GetFieldValue(actualEVECloudsPQS, "cloudsMaterial._DetailScale"); }

                set
                {
                    object obj = CloudsMaterialField.GetValue(actualEVECloudsPQS);
                    //object second = obj.GetType().GetField("cloudsMaterial").GetValue(obj);
                    obj.GetType().GetField("_DetailScale", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).SetValue(obj, value);
                }
            }

            public static object GetFieldValue(object src, string propName)
            {

                if (propName.Contains("."))//complex type nested
                {
                    var temp = propName.Split(new char[] { '.' }, 2);
                    return GetFieldValue(GetFieldValue(src, temp[0]), temp[1]);
                }
                else
                {
                    var prop = src.GetType().GetField(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    return prop != null ? prop.GetValue(src) : null;
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
