/*
 * CelestialBodyInfo.cs
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

namespace ResearchBodies
{
    public class CelestialBodyInfo
    {
        public const string ConfigNodeName = "BODY";

        public string body;   //Body Name
        public bool isResearched;  //True if this body is researched
        public int researchState;  //This is the current discoveryinfo state
        public bool ignore;        //Set to true at start of new game if this body is auto-discovered based on the difficulty level, otherwise it's false.
        public int priority;       //Priority - isn't being used? Use it for contracts?
        public string discoveryMessage;   //The message when we discover this body
        public BodyIgnoreData IgnoreData; //Use when setting the difficulty at start of new game.
        public bool KOPbarycenter;        //True if this body is actually a Kopernicus barycenter
        public CelestialBody KOPrelbarycenterBody;  //Will be null unless this body's parent is a Kopernicus barycenter, then it will be set to that barycenter
        

        public CelestialBodyInfo(string inputbody)
        {
            body = inputbody;
            isResearched = false;
            researchState = 0;
            ignore = false;
            priority = 3;
            discoveryMessage = "Now tracking " + inputbody + " !";
            IgnoreData = new BodyIgnoreData(false, false, false, false);
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
