using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ResearchBodies
{
    public partial class ResearchBodiesController : MonoBehaviour
    {
        #region InstructorVariables
        private KerbalInstructor _instructor;
        private RenderTexture _portrait;
        //private Rect _windowRect = new Rect(250f, 250f, 128f, 128f);
        private Dictionary<GUIContent, CharacterAnimationState> _responses;

        private const int PortraitWidth = 128;
        //private const int PortraitHeight = 128;
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
