/*
 * CelestialBodyInfo.cs 
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
