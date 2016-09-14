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
using System.Linq;
using System.Reflection;
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

    public partial class ResearchBodiesController : MonoBehaviour
    {
        #region InstructorVariables
        private KerbalInstructor _instructor;
        private RenderTexture _portrait;
        private Dictionary<GUIContent, CharacterAnimationState> _responses;
        private const int PortraitWidth = 128;
        private System.Random random = new System.Random();
        #endregion

        #region Instructor Functions
        private KerbalInstructor Create(string instructorName)
        {
            var prefab = AssetBase.GetPrefab(instructorName);
            if (prefab == null)
                throw new ArgumentException("Could not find instructor named '" + instructorName + "'");

            var prefabInstance = (GameObject)Instantiate(prefab);
            var instructor = prefabInstance.GetComponent<KerbalInstructor>();

            _portrait = new RenderTexture(PortraitWidth, PortraitWidth, 8);
            instructor.instructorCamera.targetTexture = _portrait;

            _responses = instructor.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(fi => fi.FieldType == typeof(CharacterAnimationState))
                .Where(fi => fi.GetValue(instructor) != null)
                .ToDictionary(fi => new GUIContent(fi.Name), fi => fi.GetValue(instructor) as CharacterAnimationState);

            return instructor;
        }

        private void PlayEmote(int emote)
        {
            _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[emote]]);
        }
        private void PlayOKEmote()
        {
            int rand = random.Next(4);
            if (rand == 1)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[2]]);
            else if (rand == 2)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[6]]);
            else
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[7]]);
        }
        private void PlayNiceEmote()
        {
            int rand = random.Next(3);
            if (rand == 1)
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[4]]);
            else
                _instructor.PlayEmote(_responses[_responses.Keys.ToArray()[5]]);
        }
        private void PlayBadEmote()
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
