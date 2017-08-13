
// The following Class is derived from Kerbal Alarm Clock mod. Which is licensed under:
// The MIT License(MIT) Copyright(c) 2014, David Tregoning
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using UnityEngine;
using RSTUtils;

namespace ResearchBodies
{
    internal static class Textures
    {              
        internal static Texture2D ObsWinBgnd = new Texture2D(640, 425, TextureFormat.ARGB32, false);
        internal static Texture2D TooltipBox = new Texture2D(10, 10, TextureFormat.ARGB32, false);
        internal static Texture2D BtnRedCross = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResize = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResizeHeight = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D BtnResizeWidth = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        internal static Texture2D SpriteObservatory = new Texture2D(256, 256, TextureFormat.ARGB32, false);
              
        internal static String PathIconsPath = System.IO.Path.Combine(RSTLogWriter.AssemblyFolder.Substring(0, RSTLogWriter.AssemblyFolder.IndexOf("/ResearchBodies/") + 16), "Icons").Replace("\\", "/");
        internal static String PathToolbarIconsPath = PathIconsPath.Substring(PathIconsPath.ToLower().IndexOf("/gamedata/") + 10);
        
        internal static void LoadIconAssets()
        {
            try
            {                
                LoadImageFromFile(ref TooltipBox, "RBToolTipBox.png", PathIconsPath);
                LoadImageFromFile(ref ObsWinBgnd, "RBObservBackGround.png", PathIconsPath);
                LoadImageFromFile(ref BtnRedCross, "RBbtnRedCross.png", PathIconsPath);
                LoadImageFromFile(ref BtnResize, "RBbtnResize.png", PathIconsPath);
                LoadImageFromFile(ref BtnResizeHeight, "RBbtnResizeHeight.png", PathIconsPath);
                LoadImageFromFile(ref BtnResizeWidth, "RBbtnResizeWidth.png", PathIconsPath);
                LoadImageFromFile(ref SpriteObservatory, "SpriteObservPicker.png", PathIconsPath);
            }
            catch (Exception)
            {
                RSTLogWriter.Log("ResearchBodies Failed to Load Textures - are you missing a file?");
            }
        }

        public static Boolean LoadImageFromFile(ref Texture2D tex, String fileName, String folderPath = "")
        {            
            Boolean blnReturn = false;
            try
            {
                if (folderPath == "") folderPath = PathIconsPath;

                //File Exists check
                if (System.IO.File.Exists(String.Format("{0}/{1}", folderPath, fileName)))
                {
                    try
                    {                        
                        tex.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", folderPath, fileName)));
                        blnReturn = true;
                    }
                    catch (Exception ex)
                    {
                        RSTLogWriter.Log("ResearchBodies Failed to load the texture:" + folderPath + "(" + fileName + ")");
                        RSTLogWriter.Log(ex.Message);
                    }
                }
                else
                {
                    RSTLogWriter.Log("ResearchBodies Cannot find texture to load:" + folderPath + "(" + fileName + ")");                    
                }


            }
            catch (Exception ex)
            {
                RSTLogWriter.Log("ResearchBodies Failed to load (are you missing a file):" + folderPath + "(" + fileName + ")");
                RSTLogWriter.Log(ex.Message);                
            }
            return blnReturn;
        }

        internal static GUIStyle ResizeStyle, ClosebtnStyle;
        internal static GUIStyle sectionTitleStyle, subsystemButtonStyle, statusStyle, warningStyle, PartListStyle, PartListPartStyle;
        internal static GUIStyle scrollStyle, resizeStyle;
        internal static GUISkin ObsSkin;

        internal static bool StylesSet = false;

        internal static void SetupStyles()
        {
            if (HighLogic.Skin != null)
            {
                GUI.skin = HighLogic.Skin;
                ObsSkin = HighLogic.Skin;
                ObsSkin.window.normal.background = ObsWinBgnd;
                ObsSkin.window.active.background = ObsWinBgnd;
                ObsSkin.window.onActive.background = ObsWinBgnd;
                ObsSkin.window.onNormal.background = ObsWinBgnd;
                ObsSkin.window.onFocused.background = ObsWinBgnd;
                Debug.Log("Highlogic.Skin Applied");
            }
            else
            {
                
            }


            //Init styles

            Utilities._TooltipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                stretchHeight = true,
                wordWrap = true,
                border = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(4, 4, 6, 4),
                alignment = TextAnchor.MiddleCenter
            };
            Utilities._TooltipStyle.normal.background = TooltipBox;
            Utilities._TooltipStyle.normal.textColor = new Color32(207, 207, 207, 255);
            Utilities._TooltipStyle.hover.textColor = Color.blue;

            ClosebtnStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 20,
                fixedHeight = 20,
                fontSize = 14,
                fontStyle = FontStyle.Normal
            };
            ClosebtnStyle.active.background = GUI.skin.toggle.onNormal.background;
            ClosebtnStyle.onActive.background = ClosebtnStyle.active.background;
            ClosebtnStyle.padding = Utilities.SetRectOffset(ClosebtnStyle.padding, 3);

            ResizeStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 20,
                fixedHeight = 20,
                fontSize = 14,
                fontStyle = FontStyle.Normal
            };
            ResizeStyle.onActive.background = ClosebtnStyle.active.background;
            ResizeStyle.padding = Utilities.SetRectOffset(ClosebtnStyle.padding, 3);

            //Init styles
            sectionTitleStyle = new GUIStyle(GUI.skin.label);
            sectionTitleStyle.alignment = TextAnchor.MiddleCenter;
            sectionTitleStyle.stretchWidth = true;
            sectionTitleStyle.fontStyle = FontStyle.Bold;

            statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.alignment = TextAnchor.MiddleLeft;
            statusStyle.stretchWidth = true;
            statusStyle.normal.textColor = Color.white;

            warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.alignment = TextAnchor.MiddleLeft;
            warningStyle.stretchWidth = true;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.normal.textColor = Color.red;

            subsystemButtonStyle = new GUIStyle(GUI.skin.toggle);
            subsystemButtonStyle.margin.top = 0;
            subsystemButtonStyle.margin.bottom = 0;
            subsystemButtonStyle.padding.top = 0;
            subsystemButtonStyle.padding.bottom = 0;

            scrollStyle = new GUIStyle(GUI.skin.scrollView);

            PartListStyle = new GUIStyle(GUI.skin.label);
            PartListStyle.alignment = TextAnchor.MiddleLeft;
            PartListStyle.stretchWidth = false;
            PartListStyle.normal.textColor = Color.yellow;

            PartListPartStyle = new GUIStyle(GUI.skin.label);
            PartListPartStyle.alignment = TextAnchor.LowerLeft;
            PartListPartStyle.stretchWidth = false;
            PartListPartStyle.normal.textColor = Color.white;

            resizeStyle = new GUIStyle(GUI.skin.button);
            resizeStyle.alignment = TextAnchor.MiddleCenter;
            resizeStyle.padding = new RectOffset(1, 1, 1, 1);

            StylesSet = true;

        }
    }
}
