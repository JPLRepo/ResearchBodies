/*
 * BodyIgnoreData.cs
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
