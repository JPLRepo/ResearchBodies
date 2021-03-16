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

using System.Collections.Generic;
using UnityEngine;

namespace ResearchBodies
{
    public class VesselSOIInfo
    {
        public const string ConfigNodeName = "VESSELINFO";
        public uint persistentId; //The persistent Id of the vessel
        public double timeEnteredSoi; //The game time the vessel entered the SOI

        public VesselSOIInfo(uint vesselId, double timeEntered = 0)
        {
            persistentId = vesselId;
            timeEnteredSoi = timeEntered;
        }

        public static VesselSOIInfo Load(ConfigNode node)
        {
            uint vesselId = 0u;
            if (!node.TryGetValue("vesselId", ref vesselId))
            {
                return null;
            }
            double timeEntered = 0;
            if (!node.TryGetValue("timeEnteredSOI", ref timeEntered))
            {
                return null;
            }
            VesselSOIInfo info = new VesselSOIInfo(vesselId, timeEntered);
            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("vesselId", persistentId);
            node.AddValue("timeEnteredSOI", timeEnteredSoi);
            return node;
        }
    }

    public static class VesselSOIInfoExt
    {
        public static bool Contains(this VesselSOIInfo value, uint id)
        {
            return value.persistentId == id;
        }
    }
    

    public class CBVesselSOIInfo
    {
        public const string ConfigNodeName = "CELESTIALBODY";
        public string body;   //Body Name
        public List<VesselSOIInfo> vesselSOIInfo;
        
        public CBVesselSOIInfo(string inputbody)
        {
            body = inputbody;
            vesselSOIInfo = new List<VesselSOIInfo>();
        }

        public CBVesselSOIInfo(string inputbody, uint vesselId, double timeEntered)
        {
            body = inputbody;
            vesselSOIInfo = new List<VesselSOIInfo>();
            VesselSOIInfo vesselInf = new VesselSOIInfo(vesselId, timeEntered);
            vesselSOIInfo.Add(vesselInf);
        }

        public bool AddVessel(uint vesselId, double timeEntered)
        {
            if (this.Contains(vesselId))
            {
                return false;
            }
            vesselSOIInfo.Add(new VesselSOIInfo(vesselId, timeEntered));
            return true;
        }

        public bool RemoveVessel(uint vesselId)
        {
            int index;
            if (this.Contains(vesselId, out index))
            {
                vesselSOIInfo.RemoveAt(index);
                return true;
            }

            return false;
        }

        public void SetVesselTimes(double currentTime)
        {
            for (int i = 0; i < vesselSOIInfo.Count; i++)
            {
                vesselSOIInfo[i].timeEnteredSoi = currentTime;
            }
        }

        public static CBVesselSOIInfo Load(ConfigNode node)
        {
            string inputbody = "";
            if (!node.TryGetValue("body", ref inputbody))
            {
                return null;
            }
            CBVesselSOIInfo cbVslInfo = new CBVesselSOIInfo(inputbody);
            ConfigNode[] vesselNodes = node.GetNodes(VesselSOIInfo.ConfigNodeName);
            for (int i = 0; i < vesselNodes.Length; i++)
            {
                VesselSOIInfo vslInfo = VesselSOIInfo.Load(vesselNodes[i]);
                if (vslInfo != null)
                {
                    cbVslInfo.vesselSOIInfo.Add(vslInfo);
                }
            }
            return cbVslInfo;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("body", body);
            for (int i = 0; i < vesselSOIInfo.Count; i++)
            {
                vesselSOIInfo[i].Save(node);
            }
            return node;
        }
    }

    public static class CBVesselSOIInfoExt
    {
        public static bool Contains(this CBVesselSOIInfo value, uint id)
        {
            for (int i = 0; i < value.vesselSOIInfo.Count; i++)
            {
                if (value.vesselSOIInfo[i].persistentId == id)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains(this CBVesselSOIInfo value, uint id, out int index)
        {
            for (int i = 0; i < value.vesselSOIInfo.Count; i++)
            {
                if (value.vesselSOIInfo[i].persistentId == id)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }
}
