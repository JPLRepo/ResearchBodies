/*
 * Locale.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RSTUtils;

namespace ResearchBodies
{
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, true)]
    public class Locales : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public static List<Locale> locales = new List<Locale>();
        public static Locale currentLocale;
        //internal static String PathcacheLocalePath = System.IO.Path.Combine(RSTLogWriter.AssemblyFolder, "PluginData/cacheLocale").Replace("\\", "/");
        internal static String PathDatabasePath = System.IO.Path.Combine(RSTLogWriter.AssemblyFolder.Substring(0, RSTLogWriter.AssemblyFolder.IndexOf("/ResearchBodies/") + 16), "database.cfg").Replace("\\", "/");

        public void Start()
        {
            ConfigNode[] cfgs = GameDatabase.Instance.GetConfigNodes("RESEARCHBODIES");
            foreach (ConfigNode node in cfgs)
            {
                if (node.GetValue("loadAs") == "locale")
                {
                    Locale l = new Locale(node);
                    locales.Add(l);
                    RSTLogWriter.Log("Added locale \"{0}\"", l.LocaleId);
                }
            }

            if (locales.Count == 0)
                RSTLogWriter.Log("No locale added !");
            else
                RSTLogWriter.Log("Added {0}  locales", locales.Count);
            RSTLogWriter.Flush();
        }
        
        /// <summary>
        /// Set the Locale (Language) If a language string is passed in it will attempt to find a locale with that name and set the locale to that.
        /// If it cannot find it will default to English.
        /// If HighLogic.CurrentGame is not null it will try to use the Custom Settings Parameter to set the Locale.
        /// Otherwise it will again default to English.
        /// If the Database.Instance has started will re-load the Celestial Body Discovery messages
        /// </summary>
        /// <param name="language"></param>
        public static void setLocale(string language)
        {
            if (HighLogic.CurrentGame != null)
            {
                if (language == "")
                {
                    language = HighLogic.CurrentGame.Parameters.CustomParams<ResearchBodies_SettingsParms>().language;
                }
                foreach (Locale l in locales)
                {
                    if (l.LocaleFull == language)
                        currentLocale = l;
                }
            }

            if (currentLocale == null)
            {
                if (language != "")
                {
                    foreach (Locale l in locales)
                    {
                        if (l.LocaleFull == language)
                            currentLocale = l;
                    }
                    if (currentLocale != null)
                        return;
                }
                foreach (Locale l in locales)
                {
                    if (l.LocaleId == "en")
                        currentLocale = l;
                }
            }
            if (Database.instance != null)
                LoadDiscoveryMessages();
        }

        public static void LoadDiscoveryMessages()
        {
            if (currentLocale.LocaleId == "en")
            {
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    foreach (
                        ConfigNode.Value value in
                            ConfigNode.Load(PathDatabasePath)
                                .GetNode("RESEARCHBODIES")
                                .GetNode("ONDISCOVERY")
                                .values)
                    {
                        if (value.name == body.GetName() && Database.instance.CelestialBodies.ContainsKey(body))
                        {
                            Database.instance.CelestialBodies[body].discoveryMessage = value.value;
                        }

                    }
                }
            }
            else
            {
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    if (currentLocale.Values.ContainsKey("discovery_" + body.GetName()) &&
                        Database.instance.CelestialBodies.ContainsKey(body))
                    {
                        Database.instance.CelestialBodies[body].discoveryMessage =
                            currentLocale.Values["discovery_" + body.GetName()];
                    }
                }
            }
        }
    }

    

    public class Locale
    {
        public string LocaleFull;
        public string LocaleId;
        private Dictionary<string, string> _values;
        public Dictionary<string, string> Values
        {
            get { return this._values; }
            set { this._values = value; }
        }

        public Locale(ConfigNode baseCfg)
        {
            this._values = new Dictionary<string, string>();
            foreach (ConfigNode.Value value in baseCfg.GetNode("VALUES").values)
            {
                if (!value.value.Contains('['))
                    this._values[value.name] = value.value;
                else
                {
                    this._values[value.name] = value.value.Replace("[", "{").Replace("]", "}");
                }
                this.LocaleFull = baseCfg.GetValue("LocaleFull");
                this.LocaleId = baseCfg.GetValue("LocaleId");
            }
        }
    }
}
