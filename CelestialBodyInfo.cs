using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResearchBodies
{
    public class CelestialBodyInfo
    {
        public const string ConfigNodeName = "BODY";

        public string body;
        public bool isResearched;
        public int researchState;
        public bool ignore;
        public int priority;
        public string discoveryString;
        public bool easy;
        public bool normal;
        public bool medium;
        public bool hard;
        public bool KOPbarycenter;
        public CelestialBody KOPrelbarycenterBody;
        

        public CelestialBodyInfo(string inputbody)
        {
            body = inputbody;
            isResearched = false;
            researchState = 0;
            ignore = false;
            priority = 3;
            discoveryString = "";
            easy = normal = medium = hard = false;
            KOPbarycenter = false;
            KOPrelbarycenterBody = null;
        }

        public static CelestialBodyInfo Load(ConfigNode node)
        {
            string inputbody = "";
            node.TryGetValue("body", ref inputbody);

            CelestialBodyInfo info = new CelestialBodyInfo(inputbody);

            node.TryGetValue("isResearched", ref info.isResearched);
            node.TryGetValue("researchState", ref info.researchState);
            node.TryGetValue("ignore", ref info.ignore);
            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("body", body);
            node.AddValue("isResearched", isResearched);
            node.AddValue("researchState", researchState);
            node.AddValue("ignore", ignore);
            return node;
        }
    }
}
