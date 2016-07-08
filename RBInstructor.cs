/*
 * RBInstructor.cs
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
