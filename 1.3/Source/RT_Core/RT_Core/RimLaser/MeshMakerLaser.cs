using System.Collections.Generic;
using UnityEngine;

namespace RT_Core;

public static class MeshMakerLaser
{
	private static int textureSeamPrecision = 256;

	private static int geometrySeamPrecision = 512;

	private static Dictionary<int, Mesh> cachedMeshes = new();

	public static Mesh Mesh(float st, float sv)
	{
		if (st < 0f)
		{
			st = 0f;
		}
		if (st > 0.5f)
		{
			st = 0.5f;
		}
		if (sv < 0f)
		{
			sv = 0f;
		}
		if (sv > 0.5f)
		{
			sv = 0.5f;
		}
		int num = (int)(st / 0.5f * (float)textureSeamPrecision);
		int num2 = (int)(sv / 0.5f * (float)geometrySeamPrecision);
		int key = num2 + (textureSeamPrecision + 1) * geometrySeamPrecision;
		if (cachedMeshes.TryGetValue(key, out var value))
		{
			return value;
		}
		st = 0.5f * (float)num / (float)textureSeamPrecision;
		sv = 0.5f * (float)num2 / (float)geometrySeamPrecision;
		float y = 1f - st;
		float num3 = 0.5f - sv;
		Vector3[] vertices = new Vector3[8]
		{
			new(-0.5f, 0f, -0.5f),
			new(-0.5f, 0f, 0f - num3),
			new(0.5f, 0f, 0f - num3),
			new(0.5f, 0f, -0.5f),
			new(-0.5f, 0f, num3),
			new(0.5f, 0f, num3),
			new(-0.5f, 0f, 0.5f),
			new(0.5f, 0f, 0.5f)
		};
		Vector2[] uv = new Vector2[8]
		{
			new(0f, 0f),
			new(0f, st),
			new(1f, st),
			new(1f, 0f),
			new(0f, y),
			new(1f, y),
			new(0f, 1f),
			new(1f, 1f)
		};
		int[] triangles = new int[18]
		{
			0, 1, 2, 0, 2, 3, 1, 4, 5, 1,
			5, 2, 4, 6, 7, 4, 7, 5
		};
		value = new Mesh();
		value.name = "NewLaserMesh()";
		value.vertices = vertices;
		value.uv = uv;
		value.SetTriangles(triangles, 0);
		value.RecalculateNormals();
		value.RecalculateBounds();
		cachedMeshes[key] = value;
		return value;
	}
}
