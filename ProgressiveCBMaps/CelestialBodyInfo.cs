using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProgressiveCBMaps
{
	public class CelestialBodyInfo
	{
		public CelestialBody Body;
		public MeshRenderer mesh;
		public Texture originalMainTex;
		public Texture originalBumpMap;
		public Texture2D newScaledMap;
		public Texture2D smallScaledMap;
		public bool multi;
		public int pass = -1;
		private float originalalpha;
		public float alpha = 1;
		private float originalshiny;
		public float shiny;
		private int originalvisualHeight;
		public int visualHeight = 128;
		public int rescaleType = 0;
		public int currentDetailLevel;
		public List<EVEWrapper.EVECloudsPQS> cloudsPQS;
	    public float originalCloudsDetailScale;
        private bool originalCloudsDetailScaleSet = false;

		/// <summary>
		/// Contructor. Caches original settings.
		/// </summary>
		/// <param name="body">The Celestial Body</param>
		public CelestialBodyInfo(CelestialBody body)
		{
			Body = body;
			mesh = body.scaledBody.GetComponent<MeshRenderer>();
			if (mesh != null)
			{
                originalMainTex = mesh.material.GetTexture("_MainTex");
                originalBumpMap = mesh.material.GetTexture("_BumpMap");
                var s = mesh.material.GetFloat("_Shininess");
				shiny = s;
				originalshiny = shiny;
				if (originalMainTex != null)
				{
					visualHeight = originalMainTex.height;
					originalvisualHeight = originalMainTex.height;
				}
				else
				{
					visualHeight = originalvisualHeight = 512;
				}
			}
			else
			{
				shiny = originalshiny = 100;
				visualHeight = originalvisualHeight = 512;
			}
			currentDetailLevel = 6;
			cloudsPQS = new List<EVEWrapper.EVECloudsPQS>();
		}

		/// <summary>
		/// Set visual details settings on
		/// </summary>
		/// <param name="_grayscale"></param>
		internal void setVisualOn(bool _grayscale = false)
		{
            if (mesh == null)
                return;
            getMeshTexture(_grayscale);
			OverlayGenerator.Instance.ClearDisplay();
		    //rescaleType = 0;
			rescaleMap();
			mesh.material.SetTexture("_MainTex", smallScaledMap);
			mesh.material.SetFloat("_Shininess", shiny);
		}

		/// <summary>
		/// Set the visual level up one notch.
		/// </summary>
		public void setVisualLevelUp()
		{
			if (currentDetailLevel != 6)
				setVisualLevel(currentDetailLevel + 1);
		}

		/// <summary>
		/// Set the visual level down one notch.
		/// </summary>
		public void setVisualLevelDown()
		{
			if (currentDetailLevel != 0)
				setVisualLevel(currentDetailLevel - 1);
		}

		/// <summary>
		/// Set visual level of the body.
		/// Level 0 = Not visible at all.
		/// Level 1 = lowest level. Grayscale on. Heightmap 64 lowest setting. Shinyness = 0 and Bump map off.
		/// Level 2 = Grayscale On. Heightmap 128 - this may be same as next level depending on original detail level. Shinyness = 0 and Bump map off.
		/// Level 3 = Grayscale On. Quarter original detail, original shinyness and bump map off.
		/// Level 4 = Grayscale On. Half original detail, original shinyness and bump map off.
		/// Level 5 = Grayscale off. Half original detail, original shinyness and bump map off.
		/// Level 6 = Maximum and bump map on.
		/// </summary>
		/// <param name="level">integer 1 through 6</param>
		public void setVisualLevel(int level)
		{
            if (mesh == null || Body.pqsController == null)
                return;

            switch (level)
			{
				case 0:
                    //visualHeight = 64;
					//shiny = 0;
					//setVisualOn(true);
					//setBumpOff();
					//processEVEClouds();
			        if (Body.pqsController != null)
			        {
                        Body.pqsController.DeactivateSphere();
                        Body.pqsController.DisableSphere();
                    }
					mesh.enabled = false;
                    currentDetailLevel = 0;
                    break;

				case 1:
                    if (currentDetailLevel == 0 && Body.pqsController != null)
					{
						Body.pqsController.ActivateSphere();
						Body.pqsController.EnableSphere();
						mesh.enabled = true;
					}

					//visualHeight = 64;
					visualHeight = 32;
					shiny = 0;
					setVisualOn(true);
					setBumpOff();
                    currentDetailLevel = 1;
                    processEVEClouds();
                    break;

				case 2:
                    if (currentDetailLevel == 0 && Body.pqsController != null)
					{
						Body.pqsController.ActivateSphere();
						Body.pqsController.EnableSphere();
						mesh.enabled = true;
					}
					//visualHeight = 128;
					visualHeight = 64;
					shiny = 0;
					setVisualOn(true);
					setBumpOff();
                    currentDetailLevel = 2;
                    processEVEClouds();
                    break;

				case 3:
                    if (currentDetailLevel == 0 && Body.pqsController != null)
					{
						Body.pqsController.ActivateSphere();
						Body.pqsController.EnableSphere();
						mesh.enabled = true;
					}
					visualHeight = originalvisualHeight / 4;
					shiny = originalshiny;
					setVisualOn(true);
					setBumpOff();
                    currentDetailLevel = 3;
                    processEVEClouds();
                    break;

				case 4:
                    if (currentDetailLevel == 0 && Body.pqsController != null)
					{
						Body.pqsController.ActivateSphere();
						Body.pqsController.EnableSphere();
						mesh.enabled = true;
					}
					visualHeight = originalvisualHeight / 2;
					shiny = originalshiny;
					setVisualOn(true);
					setBumpOff();
                    currentDetailLevel = 4;
                    processEVEClouds();
                    break;

				case 5:
                    if (currentDetailLevel == 0 && Body.pqsController != null)
					{
						Body.pqsController.ActivateSphere();
						Body.pqsController.EnableSphere();
						mesh.enabled = true;
					}
					visualHeight = originalvisualHeight / 2;
					shiny = originalshiny;
					setVisualOn(false);
					setBumpOff();
                    currentDetailLevel = 5;
                    processEVEClouds();
                    break;

				case 6:
                    if (currentDetailLevel == 0 && Body.pqsController != null)
					{
						Body.pqsController.ActivateSphere();
						Body.pqsController.EnableSphere();
						mesh.enabled = true;
					}
					visualHeight = originalvisualHeight;
					shiny = originalshiny;
					setVisualOn(false);
					setBumpOn();
                    currentDetailLevel = 6;
                    processEVEClouds();
                    break;
			}
		}

		/// <summary>
		/// Sets the visuals back to the original settings
		/// </summary>
		internal void setVisualOff()
		{
            if (mesh == null)
                return;
            mesh.material.SetFloat("_Shininess", originalshiny);
			mesh.material.SetTexture("_MainTex", originalMainTex);
			mesh.material.SetTexture("_BumpMap", originalBumpMap);
			shiny = originalshiny;
			visualHeight = originalvisualHeight;
			alpha = 1;
			rescaleType = 1;
            currentDetailLevel = 6;
            processEVEClouds();
        }


		internal void processEVEClouds()
		{
			for (int i = 0; i < cloudsPQS.Count; i++)
			{
			    //var temp = cloudsPQS[i].CloudsMaterial;
			    //var temp2 = temp._detailScale;
			    if (!originalCloudsDetailScaleSet)
			    {
                    originalCloudsDetailScale = cloudsPQS[i]._detailScale;
                    originalCloudsDetailScaleSet = true;
                }
			    
                if (currentDetailLevel < 2)
				{
					cloudsPQS[i].enabled = false;
				}
				else
				{
					cloudsPQS[i].enabled = true;
                    /*
				    if (currentDetailLevel == 2)
				    {
				        cloudsPQS[i]._detailScale = originalCloudsDetailScale / 5;
				    }
				    else
				    {
				        if (currentDetailLevel == 3)
				        {
                            cloudsPQS[i]._detailScale = originalCloudsDetailScale / 3;
                        }
				        else
				        {
				            if (currentDetailLevel == 4)
				            {
				                cloudsPQS[i]._detailScale = originalCloudsDetailScale/2;
				            }
				            else
				            {
				                if (currentDetailLevel >= 5)
				                {
				                    cloudsPQS[i]._detailScale = originalCloudsDetailScale;
				                }
				            }
				        }
				    }*/
				}
			}
		}

		/// <summary>
		/// Turns the bump map on
		/// </summary>
		internal void setBumpOn()
		{
			mesh.material.SetTexture("_BumpMap", originalBumpMap);
		}

		/// <summary>
		/// Turns the bump map off
		/// </summary>
		internal void setBumpOff()
		{
			mesh.material.SetTexture("_BumpMap", null);
		}

		/// <summary>
		/// This takes the original visual map that was cached in the constructor method and turns it into a readable texture; 
		/// this is a commonly used method and you can find references to it all over in Unity support forums
		/// </summary>
		/// /// <param name="Gray">Set to true if tex is to be converted to Grayscale</param>
		private void getMeshTexture(bool Gray = false)
		{
			//if (newScaledMap == null)
			newScaledMap = new Texture2D(originalMainTex.width, originalMainTex.height);

			var rt = RenderTexture.GetTemporary(originalMainTex.width, originalMainTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 1);

			blit(rt);

			RenderTexture.active = rt;

			newScaledMap.ReadPixels(new Rect(0, 0, originalMainTex.width, originalMainTex.height), 0, 0);

			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(rt);

			rt = null;

			newScaledMap.Apply();

			if (Gray)
			{
				MakeGrayscale(newScaledMap);
			}
		}

		/// <summary>
		/// Will make the passed in Tex Grayscale by converting the color values to their gray values.
		/// </summary>
		/// <param name="tex"></param>
		private void MakeGrayscale(Texture2D tex)
		{
			var texColors = tex.GetPixels();
			for (int i = 0; i < texColors.Length; i++)
			{
				var grayValue = texColors[i].grayscale;
				texColors[i] = new Color(grayValue, grayValue, grayValue, texColors[i].a);
			}
			tex.SetPixels(texColors);
			tex.Apply();
		}

		/// <summary>
		/// Copy source texture into destination. 
		///  If multi is true will copy for multi-tap shader.
		/// </summary>
		/// <param name="t">destination texture</param>
		private void blit(RenderTexture t)
		{
			if (!multi)
				Graphics.Blit(originalMainTex, t, mesh.material, pass);
			else
				Graphics.BlitMultiTap(originalMainTex, t, mesh.material);
		}

		/// <summary>
		/// This uses the Unity Addon TextureScale to re-size the visual map, it also adjusts the alpha channel by multiplying each pixel by the alpha level you set with the window controls
		/// </summary>
		private void rescaleMap()
		{
			smallScaledMap = newScaledMap;

			if (alpha < 0.95f)
			{
				var pix = smallScaledMap.GetPixels32();

				for (int i = 0; i < pix.Length; i++)
				{
					pix[i].a = (byte)(pix[i].a * alpha);
				}

				smallScaledMap.SetPixels32(pix);
				smallScaledMap.Apply();

				pix = null;
			}
            
			switch (rescaleType)
			{
				case 0:
					TextureScale.Bilinear(smallScaledMap, visualHeight * 2, visualHeight);
					break;
				case 1:
					TextureScale.Point(smallScaledMap, visualHeight * 2, visualHeight);
					break;
				default:
					break;
			}
        }

        
    }

	
}
