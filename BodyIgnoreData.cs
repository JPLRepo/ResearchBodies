/*
 * BodyIgnoreData.cs
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

        public void setBodyIgnoreData(bool easy, bool normal, bool medium, bool hard)
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
}
