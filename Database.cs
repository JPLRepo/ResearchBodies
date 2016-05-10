using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace ResearchBodies
{
    //[KSPAddon(KSPAddon.Startup.PSystemSpawn, true)]
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Database : MonoBehaviour
    {
        public static Dictionary<CelestialBody, int> Priority = new Dictionary<CelestialBody, int>();
        public static Dictionary<string, string> DiscoveryMessage = new Dictionary<string, string>();
        public static Dictionary<CelestialBody, BodyIgnoreData> IgnoreData = new Dictionary<CelestialBody, BodyIgnoreData>();
        public static Texture2D IconTexture;
        public static List<CelestialBody> IgnoreBodies = new List<CelestialBody>();
        public static List<string> NothingHere = new List<string>();
        // public static Dictionary<CelestialBody, Texture2D> BlurredTextures = new Dictionary<CelestialBody, Texture2D>();
        public static int chances;
        public static int[] StartResearchCosts, ProgressResearchCosts, ScienceRewards;
        public static bool enableInSandbox, allowTSlevel1 = false;

        /// <summary>
        /// Tarsier Space Tech Interface fields
        /// </summary>
        internal bool isTSTInstalled = false;
        internal static List<CelestialBody> TSTCBGalaxies = new List<CelestialBody>();
        public static List<CelestialBody> BodyList = new List<CelestialBody>();


        public static Level GetLevel(int i)
        {
            Level _l = Level.Easy;
            switch (i)
            {
                case 0:
                    _l = Level.Easy;
                    break;
                case 1 :
                    _l = Level.Normal;
                    break;
                case 2:
                    _l = Level.Medium;
                    break;
                case 3:
                    _l = Level.Hard;
                    break;
            }
            return _l;
        }
        public static string GetIgnoredBodies(Level l) 
        {
            string _bodies = Locales.currentLocale.Values["start_availableBodies"] + " : ";
            foreach (CelestialBody body in BodyList.Where(b => IgnoreData[b].GetLevel(l)))
            {
                _bodies += body.GetName() + ", ";
            }
            return _bodies;
        }
        void Start()
        {
            isTSTInstalled = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "TarsierSpaceTech");
            if (isTSTInstalled)  //If TST assembly is present, initialise TST wrapper.
            {
                if (!TSTWrapper.InitTSTWrapper())
                {
                    isTSTInstalled = false; //If the initialise of wrapper failed set bool to false, we won't be interfacing to TST today.
                }
            }

            BodyList = FlightGlobals.Bodies;
            if (isTSTInstalled && TSTWrapper.APITSTReady)
            {
                BodyList = BodyList.Concat(TSTWrapper.actualTSTAPI.CBGalaxies).ToList();
            }

            IconTexture = GameDatabase.Instance.GetTexture("ResearchBodies/images/icon", false);
            ConfigNode cfg = ConfigNode.Load("GameData/ResearchBodies/database.cfg");
            
            string[] _startResearchCosts;
            string[] sep = new string[] { " " };
            _startResearchCosts = cfg.GetNode("RESEARCHBODIES").GetValue("StartResearchCosts").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            StartResearchCosts = new int[] { int.Parse(_startResearchCosts[0]), int.Parse(_startResearchCosts[1]), int.Parse(_startResearchCosts[2]), int.Parse(_startResearchCosts[3]) };

            string[] _progressResearchCosts;
            _progressResearchCosts = cfg.GetNode("RESEARCHBODIES").GetValue("ProgressResearchCosts").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            ProgressResearchCosts = new int[] { int.Parse(_progressResearchCosts[0]), int.Parse(_progressResearchCosts[1]), int.Parse(_progressResearchCosts[2]), int.Parse(_progressResearchCosts[3]) };

            string[] _scienceRewards;
            _scienceRewards = cfg.GetNode("RESEARCHBODIES").GetValue("ScienceRewards").Split(sep, StringSplitOptions.RemoveEmptyEntries);
            ScienceRewards = new int[] { int.Parse(_scienceRewards[0]), int.Parse(_scienceRewards[1]), int.Parse(_scienceRewards[2]), int.Parse(_scienceRewards[3]) };

            Log.log("[ResearchBodies] Loading Priority database");
            foreach (CelestialBody body in BodyList)
            {
                string name = body.GetName();
                foreach (ConfigNode.Value value in cfg.GetNode("RESEARCHBODIES").GetNode("PRIORITIES").values)
                {
                    if (name == value.name)
                    {
                        Priority[body] = int.Parse(value.value);
                        Log.log("[ResearchBodies] Priority for body " + name + " set to " + value.value + ".");
                    }
                }
                foreach (ConfigNode.Value value in cfg.GetNode("RESEARCHBODIES").GetNode("ONDISCOVERY").values)
                {
                    if (value.name == name)
                        DiscoveryMessage[value.name] = value.value;
                }
                if (!DiscoveryMessage.ContainsKey(body.GetName()))
                    DiscoveryMessage[body.GetName()] = "Now tracking " + name + " !";
                // if (cfg.GetNode("RESEARCHBODIES").HasValue("blurredTextures"))
                // {
                //    foreach (string str in Directory.GetFiles(cfg.GetNode("RESEARCHBODIES").GetValue("blurredTextures")))
                //    {
                //        if (str.Contains(body.GetName()))
                //            BlurredTextures[body] = GameDatabase.Instance.GetTexture(cfg.GetNode("RESEARCHBODIES").GetValue("blurredTextures").Replace("GameData/", "") + "/" + body.GetName(), false);
                //    }
                // }
            }
            Log.log("[ResearchBodies] Loading ignore body list from database");
            foreach (ConfigNode.Value value in cfg.GetNode("RESEARCHBODIES").GetNode("IGNORELEVELS").values)
            {
                foreach (CelestialBody body in BodyList)
                {
                    if (body.GetName() == value.name)
                    {
                        string[] args;
                        args = value.value.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        IgnoreData[body] = new BodyIgnoreData(bool.Parse(args[0]), bool.Parse(args[1]), bool.Parse(args[2]), bool.Parse(args[3]));

                        Log.log("Body Ignore Data for " + body.GetName() + " : " + IgnoreData[body].ToString());
                    }
                }
            }
            foreach (CelestialBody body in BodyList)
            {
                if (!IgnoreData.ContainsKey(body))
                {
                    IgnoreData[body] = new BodyIgnoreData(false, false, false, false);
                }
            }
            foreach (ConfigNode.Value value in cfg.GetNode("RESEARCHBODIES").GetNode("NOTHING").values)
            {
                if (value.name == "text")
                    NothingHere.Add(value.value);
            }
            Log.log("[ResearchBodies] Loading mods databases");
            ConfigNode[] modNodes = GameDatabase.Instance.GetConfigNodes("RESEARCHBODIES");
            foreach (ConfigNode node in modNodes)
            {
                if (node.GetValue("loadAs") == "mod")
                {
                    if (node.HasValue("name"))
                        Log.log("[ResearchBodies] Loading " + node.GetValue("name") + " configuration");
                    if (node.HasNode("PRIORITIES"))
                    {
                        foreach (CelestialBody body in BodyList)
                        {
                            string name = body.GetName();
                            foreach (ConfigNode.Value value in node.GetNode("PRIORITIES").values)
                            {
                                if (name == value.name)
                                {
                                    Priority[body] = int.Parse(value.value);
                                    Log.log("[ResearchBodies] Priority for body " + name + " set to " + value.value);
                                }
                                else if ( "*" + name == value.name)
                                {
                                    Priority[body] = int.Parse(value.value);
                                    Log.log("[ResearchBodies] Priority for body " + name + " set to " + value.value + ", overriding old value.");
                                }
                            }
                        }
                    }
                    if (node.HasNode("ONDISCOVERY"))
                    {
                        foreach (CelestialBody body in BodyList)
                        {
                            foreach (ConfigNode.Value value in node.GetNode("ONDISCOVERY").values)
                            {
                                if (value.name == body.GetName() || value.name == "*" + body.GetName())
                                    DiscoveryMessage[value.name] = value.value;
                            }
                            if (!DiscoveryMessage.ContainsKey(body.GetName()))
                                DiscoveryMessage[body.GetName()] = "Now tracking " + body.GetName() + " !";
                        }
                    }
                    //if (node.HasValue("blurredTextures"))
                    //{
                    //    foreach (CelestialBody body in BodyList)
                    //    {
                    //        foreach (string str in Directory.GetFiles(node.GetValue("blurredTextures")))
                    //        {
                    //            if (str.Contains(body.GetName()))
                    //                BlurredTextures[body] = GameDatabase.Instance.GetTexture(node.GetValue("blurredTextures") + "/" + body.GetName(), false);
                    //        }
                    //    }
                    //}
                    if (node.HasNode("IGNORE"))
                    {
                        foreach (ConfigNode.Value value in node.GetNode("IGNORE").values)
                        {
                            if (value.name == "body")
                            {
                                foreach (CelestialBody cb in BodyList)
                                {
                                    if (value.value == cb.GetName())
                                    {
                                        IgnoreData[cb] = new BodyIgnoreData(false, false, false, false);
                                        Log.log("[ResearchBodies] Added " + cb.GetName() + " to the ignore list (pre-1.5 method !)");
                                    }
                                }
                            }
                            else if (value.name == "!body")
                            {
                                foreach (CelestialBody cb in BodyList)
                                {
                                    if (value.value == cb.GetName() && IgnoreBodies.Contains(cb))
                                    {
                                        IgnoreData[cb] = new BodyIgnoreData(true, true, true, true);
                                        Log.log("[ResearchBodies] Removed " + cb.GetName() + " from the ignore list (pre-1.5 method!)");
                                    }
                                }
                            }
                        }
                    }
                    if (node.HasNode("IGNORELEVELS"))
                    {
                        foreach (ConfigNode.Value value in node.GetNode("IGNORELEVELS").values)
                        {
                            foreach (CelestialBody body in BodyList)
                            {
                                if (body.GetName() == value.name)
                                {
                                    string[] args;
                                    args = value.value.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                                    IgnoreData[body] = new BodyIgnoreData(bool.Parse(args[0]), bool.Parse(args[1]), bool.Parse(args[2]), bool.Parse(args[3]));

                                    Log.log("Body Ignore Data for " + body.GetName() + " : " + IgnoreData[body].ToString());
                                }
                            }
                        }
                    }
                }
            }
            foreach (CelestialBody cb in BodyList)
            {
                if (!Priority.Keys.Contains(cb) && !IgnoreBodies.Contains(cb))
                {
                    Priority[cb] = 3;
                    Log.logWarn("[ResearchBodies] Config not found for " + cb.GetName() + ", priority set to 3.");
                }
            }
            chances = int.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("chances"));
            Log.log("[ResearchBodies] Chances to get a body set to " + chances);
            enableInSandbox = bool.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("enableInSandbox"));
            allowTSlevel1 = bool.Parse(cfg.GetNode("RESEARCHBODIES").GetValue("allowTrackingStationLvl1"));
            Log.log("[ResearchBodies] Loaded gamemode-related informations : enable mod in sandbox = " + enableInSandbox + ", allow tracking with Tracking station lvl 1 = " + allowTSlevel1);


            // Load locales for OnDiscovery
            if (Locales.currentLocale.LocaleId != "en")
            {
                foreach (CelestialBody body in BodyList)
                {
                    if (Locales.currentLocale.Values.ContainsKey("discovery_" + body.GetName()) && DiscoveryMessage.ContainsKey(body.GetName()))
                    {
                        DiscoveryMessage[body.GetName()] = Locales.currentLocale.Values["discovery_" + body.GetName()];
                    }
                }
            }
        }

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
    }

    public class BodyIgnoreData
    {
        public bool Easy, Normal, Medium, Hard;
        public BodyIgnoreData(bool easy, bool normal, bool medium, bool hard)
        {
            Easy = easy;
            Normal = normal;
            Medium = medium;
            Hard = hard;
        }

        public bool GetLevel(Level lvl)
        {
            bool x;
            switch (lvl)
            {
                case Level.Easy:
                    x = this.Easy;
                    break;
                case Level.Normal:
                    x = this.Normal;
                    break;
                case Level.Medium:
                    x = this.Medium;
                    break;
                default:
                    x = this.Hard;
                    break;
            }
            return x;
        }
        public override string ToString()
        {
            return this.Easy + " " + this.Normal + " " + this.Medium + " " + this.Hard;
        }
    }

    public enum Level
    {
        Easy,
        Normal,
        Medium,
        Hard
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Log : MonoBehaviour
    {
        static TextWriter Tw;
        void Awake() { DontDestroyOnLoad(this); }
        void Start()
        {
            if (File.Exists("GameData/ResearchBodies/researchbodies.log"))
                File.Delete("GameData/ResearchBodies/researchbodies.log");
            Tw = new StreamWriter("GameData/ResearchBodies/researchbodies.log");
            Tw.WriteLine("ResearchBodies v1.6");
            Tw.WriteLine("Loaded up on " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt") + ".");
            Tw.WriteLine();
            GameEvents.onGameSceneLoadRequested.Add(logSceneSwitch);
        }
        public void OnDestroy()
        {
            Tw.Close();
        }
        public static void log(object obj)
        {
            Debug.Log(obj);
            Tw.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " [LOG] " + obj);
        }
        public static void logWarn(object obj)
        {
            Debug.LogWarning(obj);
            Tw.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " [WRN] " + obj);
        }
        public static void logError(object obj)
        {
            Debug.LogError(obj);
            Tw.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " [ERR] " + obj);
        }
        private void logSceneSwitch(GameScenes scene)
        {
            Tw.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " [KSP] ==== Scene Switch to " + scene.ToString() + " ! ====");
        }
    }
}
