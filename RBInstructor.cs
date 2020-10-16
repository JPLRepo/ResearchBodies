/*
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
//using System.Linq;
using System.Reflection;
using UniLinq;
using UnityEngine;

namespace ResearchBodies
{
    /// <summary>
    /// Instructor: Wernher Von Kerman, prefab: Instructor_Wernher
    /// anim_idle
    /// anim_idle_lookAround
    /// anim_idle_sigh
    /// anim_idle_wonder
    /// anim_true_thumbUp
    /// anim_true_thumbsUp
    /// anim_true_nodA
    /// anim_true_nodB
    /// anim_true_smileA
    /// anim_true_smileB
    /// anim_false_disappointed
    /// anim_false_disagreeA
    /// anim_false_disagreeB
    /// anim_false_disagreeC
    /// anim_false_sadA
    /// </summary>

    public class ResearchBodiesInstructor 
    {
        #region InstructorVariables
        private KerbalInstructor _instructor;
        public KerbalInstructor Instructor { get { return _instructor; } }
        private RenderTexture _portrait;
        public RenderTexture Portrait { get { return _portrait; } }
        private Dictionary<GUIContent, CharacterAnimationState> _responses;
        private const int PortraitWidth = 128;
        private System.Random random = new System.Random();
        public string InstructorName = "";
        #endregion

        #region Instructor Functions
        public ResearchBodiesInstructor(string instructorName)
        {
            GameObject prefab = AssetBase.GetPrefab(instructorName);
            if (prefab == null)
                throw new ArgumentException("Could not find instructor named '" + instructorName + "'");

            if (instructorName == "Instructor_Wernher")
            {
                InstructorName = KSP.Localization.Localizer.Format("#autoLOC_RBodies_00102");
            }
            else if(instructorName == "Strategy_ScienceGuy")
            {
                InstructorName = KSP.Localization.Localizer.Format("#autoLOC_501659");   
            }
            GameObject prefabInstance = UnityEngine.Object.Instantiate(prefab);
            _instructor = prefabInstance.GetComponent<KerbalInstructor>();

            _portrait = new RenderTexture(PortraitWidth, PortraitWidth, 8);
            _instructor.instructorCamera.targetTexture = _portrait;

            _responses = new Dictionary<GUIContent, CharacterAnimationState>();
            FieldInfo[] fields = _instructor.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType == typeof(CharacterAnimationState) && fields[i].GetValue(_instructor) != null)
                {
                    _responses.Add(new GUIContent(fields[i].Name), fields[i].GetValue(_instructor) as CharacterAnimationState);
                }
            }
        }

        public void Destroy()
        {
            if (_portrait != null)
                _portrait.Release();
        }

        public void PlayEmote(int emote)
        {
            _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[emote]]);
        }
        public void PlayOKEmote()
        {
            int rand = random.Next(4);
            if (rand == 1)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[2]]);
            else if (rand == 2)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[6]]);
            else
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[7]]);
        }
        public void PlayNiceEmote()
        {
            int rand = random.Next(3);
            if (rand == 1)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[4]]);
            else
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[5]]);
        }
        public void PlayBadEmote()
        {
            int rand = random.Next(5);
            if (rand == 1)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[11]]);
            else if (rand == 2)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[12]]);
            else if (rand == 3)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[13]]);
            else
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[14]]);
        }
        #endregion
    }
}
