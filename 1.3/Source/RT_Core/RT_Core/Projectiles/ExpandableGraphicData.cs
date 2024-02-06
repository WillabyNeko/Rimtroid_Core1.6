using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace RT_Rimtroid;

public class ExpandableGraphicData
{
	[NoTranslate]
	public Type graphicClass;

	public ShaderTypeDef shaderType;

	public List<ShaderParameter> shaderParameters;

	public Color color = Color.white;

	public Color colorTwo = Color.white;

	public bool drawRotated = true;

	public bool allowFlip = true;

	public float flipExtraRotation;

	public ShadowData shadowData;

	public string texPath;

	public string texPathFadeOut;

	private Material[] cachedMaterials;

	private Material[] cachedMaterialsFadeOut;

	private static Dictionary<string, Material[]> loadedMaterials = new();

	public Material[] Materials
	{
		get
		{
			if (cachedMaterials == null)
			{
				InitMainTextures();
			}
			return cachedMaterials;
		}
	}

	public Material[] MaterialsFadeOut
	{
		get
		{
			if (cachedMaterialsFadeOut == null)
			{
				InitFadeOutTextures();
			}
			return cachedMaterialsFadeOut;
		}
	}

	public void InitMainTextures()
	{
		if (!loadedMaterials.TryGetValue(texPath, out var value))
		{
			List<string> list = (from x in LoadAllFiles(texPath)
				orderby x
				select x).ToList();
			if (list.Count > 0)
			{
				cachedMaterials = new Material[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					Shader shader = ((shaderType != null) ? shaderType.Shader : ShaderDatabase.DefaultShader);
					cachedMaterials[i] = MaterialPool.MatFrom(list[i], shader, color);
				}
			}
			else
			{
				Log.Error("Error loading materials by this path: " + texPath);
			}
			loadedMaterials[texPath] = cachedMaterials;
		}
		else
		{
			cachedMaterials = value;
		}
	}

	public void InitFadeOutTextures()
	{
		if (texPathFadeOut.NullOrEmpty())
		{
			return;
		}
		if (!loadedMaterials.TryGetValue(texPathFadeOut, out var value))
		{
			List<string> list = (from x in LoadAllFiles(texPathFadeOut)
				orderby x
				select x).ToList();
			if (list.Count > 0)
			{
				cachedMaterialsFadeOut = new Material[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					Shader shader = ((shaderType != null) ? shaderType.Shader : ShaderDatabase.DefaultShader);
					cachedMaterialsFadeOut[i] = MaterialPool.MatFrom(list[i], shader, color);
				}
			}
			loadedMaterials[texPathFadeOut] = cachedMaterialsFadeOut;
		}
		else
		{
			cachedMaterialsFadeOut = value;
		}
	}

	public List<string> LoadAllFiles(string folderPath)
	{
		List<string> list = new();
		foreach (ModContentPack item in LoadedModManager.RunningModsListForReading)
		{
			foreach (KeyValuePair<string, FileInfo> item2 in ModContentPack.GetAllFilesForMod(item, "Textures/" + folderPath))
			{
				string fullName = item2.Value.FullName;
				if (fullName.EndsWith(".png"))
				{
					fullName = fullName.Replace("\\", "/");
					fullName = fullName.Substring(fullName.IndexOf("/Textures/") + 10);
					fullName = fullName.Replace(".png", "");
					list.Add(fullName);
				}
			}
		}
		return list;
	}
}
