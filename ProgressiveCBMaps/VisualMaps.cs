/*
 * VisualMaps.cs
 * (C) Copyright 2016, Jamie Leighton 
 * License Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International
 * http://creativecommons.org/licenses/by-nc-sa/4.0/
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  ProgressiveCBMaps is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 *
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using RSTUtils;

namespace ProgressiveCBMaps
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class VisualMaps : MonoBehaviour
	{

		public static VisualMaps Instance;

		private Rect WindowRect;
		private int WindowID;
		private Vector2 scrollViewVector = Vector2.zero;
		private Dictionary<CelestialBody, CelestialBodyInfo> celestialBodyInfo = new Dictionary<CelestialBody, CelestialBodyInfo>();
		private KeyValuePair<CelestialBody, CelestialBodyInfo> selectedBody;
		private string[] BtnNames;
		private int TargetSelection = 0;
		private bool showUI = false;
		private bool EVEInitialised = false;

		public Dictionary<CelestialBody, CelestialBodyInfo> CBVisualMapsInfo
		{
			get
			{
				return Instance.celestialBodyInfo;
			}
		}

		private void Start()
		{
			if (Instance != null)
				Destroy(this);
			else
				Instance = this;
			DontDestroyOnLoad(this);

			WindowID = UnityEngine.Random.Range(1000, 2000000);
			
			foreach (CelestialBody CB in FlightGlobals.Bodies)
			{
				CelestialBodyInfo CBinfo = new CelestialBodyInfo(CB);
				celestialBodyInfo[CB] = CBinfo;
			}
				
			BtnNames = new string[FlightGlobals.Bodies.Count];

			for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
			{
				BtnNames[i] = FlightGlobals.Bodies[i].GetName();
			}
			//}
		}

		private void OnDestroy()
		{
			foreach (var body in celestialBodyInfo)
			{
				body.Value.setVisualOff();
			}
		}

		private void InitEVE()
		{
			EVEWrapper.InitEVEWrapper();

			if (EVEWrapper.APIReady)
			{
				//success

				UnityEngine.Object[] cloudspqs = FindObjectsOfType(EVEWrapper.EVECloudsPQSType);
				for (int i = 0; i < cloudspqs.Length; i++)
				{
					EVEWrapper.EVECloudsPQS cloudentry = new EVEWrapper.EVECloudsPQS(cloudspqs[i]);
					if (cloudentry.celestialBody != null)
					{
						if (celestialBodyInfo.ContainsKey(cloudentry.celestialBody))
						{
							celestialBodyInfo[cloudentry.celestialBody].cloudsPQS.Add(cloudentry);
							if (celestialBodyInfo[cloudentry.celestialBody].currentDetailLevel < 4)
							{
								cloudentry.enabled = false;
							}
						}
					}
				}
				EVEInitialised = true;
			}
		}

		/// <summary>
		/// Watch for changes to the celestial body we are lookin at
		/// </summary>
		private void Update()
		{
			if ((HighLogic.LoadedSceneIsFlight && FlightGlobals.ready) || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.Backslash))
				{
					showUI = !showUI;
				}
				
				//foreach (var CB in celestialBodyInfo)
				//{
				//	if (CB.Value.mesh.isVisible)
				//	{
				//		Debug.Log("CB " + CB.Key.GetName() + " is visible");
				//	}
				//}
				if (Utilities.IsEVEInstalled && !EVEInitialised)
				{
					InitEVE();
				}
			}
		}
        #if DEBUG
		private void OnGUI()
		{
			if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				return;
			}
			if (showUI)
				WindowRect = GUILayout.Window(WindowID, WindowRect, drawButtons, "Progressive CB Maps");
		}
        #endif

		/// <summary>
		/// Draw the window and a bunch of control buttons
		/// </summary>
		/// <param name="id"></param>
		private void drawButtons(int id)
		{
			GUI.skin = GUI.skin = HighLogic.Skin;
			GUILayout.BeginVertical();
			scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, GUILayout.Height(150));
			GUILayout.BeginVertical();
			TargetSelection = GUILayout.SelectionGrid(TargetSelection, BtnNames, 1);
			GUILayout.EndVertical();
			GUILayout.EndScrollView();

			if (selectedBody.Key != celestialBodyInfo.ElementAt(TargetSelection).Key)
			{
				selectedBody = celestialBodyInfo.ElementAt(TargetSelection);
			}

			GUILayout.BeginHorizontal();
			
			//Toggle the low resolution visual map
			if (GUILayout.Button("Visual On"))
			{
				//if (twice)
				//{
				//	getMeshTexture();
				//	body.SetResourceMap(newScaledMap);
				//}
				//else
				//{
					//Destroy(newScaledMap);
					
					//getMeshTexture();

					//OverlayGenerator.Instance.ClearDisplay();

					//rescaleMap();

					//mesh.material.SetTexture("_MainTex", (Texture)smallScaledMap);
					//mesh.material.SetFloat("_Shininess", shiny);
				//}

				selectedBody.Value.setVisualOn(false);
			}

			if (GUILayout.Button("Visual Off"))
			{
				//if (twice)
				//{
				//	Destroy(newScaledMap);
				//}
				//else
				//	mesh.material.SetTexture("_MainTex", oldMainTex);
				selectedBody.Value.setVisualOff();
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			//Toggle the normal map
			if (GUILayout.Button("Bump On"))
			{
				//mesh.material.SetTexture("_BumpMap", oldBumpMap);
				selectedBody.Value.setBumpOn();
			}

			if (GUILayout.Button("Bump Off"))
			{
				//mesh.material.SetTexture("_BumpMap", null);
				selectedBody.Value.setBumpOff();
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			//Toggle the low resolution visual grayscale map
			if (GUILayout.Button("GrayScale On"))
			{
				//if (twice)
				//{
				//    getMeshTexture(true);
				//    body.SetResourceMap(newScaledMap);
				//}
				//else
				//{
					//Destroy(newScaledMap);

				//    getMeshTexture(true);

				//    OverlayGenerator.Instance.ClearDisplay();

					//rescaleMap();

					//mesh.material.SetTexture("_MainTex", (Texture)newScaledMap);
					//mesh.material.SetFloat("_Shininess", shiny);
				//}
				selectedBody.Value.setVisualOn(true);
			}

			if (GUILayout.Button("GrayScale Off"))
			{
				//if (twice)
				//{
				//    Destroy(newScaledMap);
				//}
				//else
				//    mesh.material.SetTexture("_MainTex", oldMainTex);
				selectedBody.Value.setVisualOn(false);
			}

			GUILayout.EndHorizontal();

			GUILayout.Label("Current Detail Level: " + selectedBody.Value.currentDetailLevel);
			GUILayout.Label("Set Preset Level:");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("0", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevel(0);
			}
			if (GUILayout.Button("1", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevel(1);
			}
			if (GUILayout.Button("2", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevel(2);
			}
			if (GUILayout.Button("3", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevel(3);
			}
			if (GUILayout.Button("4", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevel(4);
			}
			if (GUILayout.Button("5", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevel(5);
			}
			if (GUILayout.Button("6", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevel(6);
			}
			if (GUILayout.Button("-", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevelDown();
			}
			if (GUILayout.Button("+", GUILayout.Width(18)))
			{
				selectedBody.Value.setVisualLevelUp();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.Label("Visual Map Height:");

			GUILayout.FlexibleSpace();

			//Change the visual map scaling resolution
			if (GUILayout.Button("-", GUILayout.Width(18)))
			{
				selectedBody.Value.visualHeight = Math.Max(32, selectedBody.Value.visualHeight / 2);
			}
			GUILayout.Label(selectedBody.Value.visualHeight.ToString(), GUILayout.Width(45));
			if (GUILayout.Button("+", GUILayout.Width(18)))
			{
				selectedBody.Value.visualHeight = Math.Min(4096, selectedBody.Value.visualHeight * 2);
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.Label("Scale Type:");

			GUILayout.FlexibleSpace();

			//Change the texture scaling method; between Point and Bilinear
			if (GUILayout.Button("-", GUILayout.Width(18)))
			{
				selectedBody.Value.rescaleType = Math.Max(0, selectedBody.Value.rescaleType - 1);
			}
			GUILayout.Label(selectedBody.Value.rescaleType.ToString(), GUILayout.Width(45));
			if (GUILayout.Button("+", GUILayout.Width(18)))
			{
				selectedBody.Value.rescaleType = Math.Min(1, selectedBody.Value.rescaleType + 1);
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.Label("Alpha:");

			GUILayout.FlexibleSpace();

			//Adjust the visual map alpha channel; used by ocean planets
			if (GUILayout.Button("-", GUILayout.Width(18)))
			{
				selectedBody.Value.alpha = (float)Math.Max(0, selectedBody.Value.alpha - 0.05);
			}
			GUILayout.Label(selectedBody.Value.alpha.ToString("P0"), GUILayout.Width(45));
			if (GUILayout.Button("+", GUILayout.Width(18)))
			{
				selectedBody.Value.alpha = (float)Math.Min(1, selectedBody.Value.alpha + 0.05);
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.Label("Shiny:");

			GUILayout.FlexibleSpace();

			//I think this has something to do with specularity
			if (GUILayout.Button("-", GUILayout.Width(18)))
			{
				selectedBody.Value.shiny = (float)Math.Max(0, selectedBody.Value.shiny - 0.05);
			}
			GUILayout.Label(selectedBody.Value.shiny.ToString("P0"), GUILayout.Width(45));
			if (GUILayout.Button("+", GUILayout.Width(18)))
			{
				selectedBody.Value.shiny = (float)Math.Min(1, selectedBody.Value.shiny + 0.05);
			}

			GUILayout.EndHorizontal();

			selectedBody.Value.multi = GUILayout.Toggle(selectedBody.Value.multi, "Multi-Tap");

			//Reset the visual map to the cached version
			if (GUILayout.Button("Reset Main"))
			{
				selectedBody.Value.setVisualOff();
			}
			GUILayout.EndVertical();
			GUI.DragWindow();
		}	
		
	}
}
