using System;
using System.Collections.Generic;
using System.Linq;
/*
 * Locale.cs
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
using UnityEngine;
using System.IO;
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
        private Locale precedentLocale;
        private bool status = true; // true = save, false = check
        internal static String PathcacheLocalePath = System.IO.Path.Combine(RSTLogWriter.AssemblyFolder, "PluginData/cacheLocale").Replace("\\", "/");
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
                    RSTLogWriter.Log_Debug("Added locale \"{0}\"", l.LocaleId);
                }
            }

            if (File.Exists(PathcacheLocalePath))
            {
                StreamReader sr = new StreamReader(PathcacheLocalePath);
                string line = sr.ReadLine();
                foreach (Locale l in locales)
                {
                    if (l.LocaleId == line)
                    {
                        currentLocale = l;
                        RSTLogWriter.Log_Debug("Loaded {0}  from cache", l.LocaleFull);
                    }
                }
                sr.Close();
            }
            else
            {
                foreach (Locale l in locales)
                {
                    if (l.LocaleId == "en")
                        currentLocale = l;
                }
                TextWriter tw = new StreamWriter(PathcacheLocalePath);
                tw.Write(currentLocale.LocaleId);
                tw.Close();
            }

            if (locales.Count == 0)
                RSTLogWriter.Log_Debug("No locale added !");
            else
                RSTLogWriter.Log_Debug("Added {0}  locales", locales.Count);
        }

        public static void Save(Locale l)
        {
            if (File.Exists(PathcacheLocalePath))
                File.Delete(PathcacheLocalePath);
            TextWriter tw = new StreamWriter(PathcacheLocalePath);
            tw.Write(l.LocaleId);
            tw.Close();
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
        Dictionary<string, string> _values;

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

        public Dictionary<string, string> Values
        {
            get { return this._values; }
            set { this._values = value; }
        }

        public string LocaleFull;
        public string LocaleId;

    }
    
}
