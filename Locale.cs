using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace ResearchBodies
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Locales : MonoBehaviour
    {
        void Awake() { DontDestroyOnLoad(this); }
        public static List<Locale> locales = new List<Locale>();
        public static Locale currentLocale;
        private Locale precedentLocale; private bool status = true; // true = save, false = check
        void Start()
        {
            ConfigNode[] cfgs = GameDatabase.Instance.GetConfigNodes("RESEARCHBODIES");
            foreach (ConfigNode node in cfgs)
            {
                if (node.GetValue("loadAs") == "locale")
                {
                    Locale l = new Locale(node);
                    locales.Add(l);
                    Log.log("[ResearchBodies] Added locale \"" + l.LocaleId + "\"");
                }
            }

            if (File.Exists("GameData/ResearchBodies/Plugins/PluginData/cacheLocale"))
            {
                StreamReader sr = new StreamReader("GameData/ResearchBodies/Plugins/PluginData/cacheLocale");
                string line = sr.ReadLine();
                foreach (Locale l in locales)
                {
                    if (l.LocaleId == line)
                    {
                        currentLocale = l;
                        Log.log("[ResearchBodies] Loaded " + l.LocaleFull + " from cache");
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
                TextWriter tw = new StreamWriter("GameData/ResearchBodies/Plugins/PluginData/cacheLocale");
                tw.Write(currentLocale.LocaleId);
                tw.Close();
            }

            if (locales.Count == 0)
                Log.log("[ResearchBodies] No locale added !");
            else
                Log.log("[ResearchBodies] Added " + locales.Count.ToString() + " locales");
        }
        public static void Save(Locale l)
        {
            if (File.Exists("GameData/ResearchBodies/Plugins/PluginData/cacheLocale")) File.Delete("GameData/ResearchBodies/Plugins/PluginData/cacheLocale");
            TextWriter tw = new StreamWriter("GameData/ResearchBodies/Plugins/PluginData/cacheLocale");
            tw.Write(l.LocaleId);
            tw.Close();
        }
        public static void LoadDiscoveryMessages()
        {
            if (currentLocale.LocaleId == "en")
            {
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    foreach (ConfigNode.Value value in ConfigNode.Load("GameData/ResearchBodies/database.cfg").GetNode("RESEARCHBODIES").GetNode("ONDISCOVERY").values)
                    {
                        if (value.name == body.GetName())
                            Database.DiscoveryMessage[value.name] = value.value;
                    }
                }
            }
            else
            {
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    if (currentLocale.Values.ContainsKey("discovery_" + body.GetName()) && Database.DiscoveryMessage.ContainsKey(body.GetName()))
                    {
                        Database.DiscoveryMessage[body.GetName()] = currentLocale.Values["discovery_" + body.GetName()];
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
