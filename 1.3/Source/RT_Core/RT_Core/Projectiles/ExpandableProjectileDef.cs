using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RT_Rimtroid;

public class ExpandableProjectileDef : ThingDef
{
	public int lifeTimeDuration = 100;

	public float widthScaleFactor = 1f;

	public float heightScaleFactor = 1f;

	public Vector3 startingPositionOffset = Vector3.zero;

	public float totalSizeScale = 1f;

	public new ExpandableGraphicData graphicData;

	public int tickFrameRate = 1;

	public int finalTickFrameRate = 0;

	public int tickDamageRate = 60;

	public float minDistanceToAffect;

	public bool disableVanillaDamageMethod;

	public bool dealsDamageOnce;

	public bool reachMaxRangeAlways;

	public bool stopWhenHit = true;

	public List<string> stopWhenHitAt = new();

	public ExpandableProjectileShapeDef fixedShape;

	public float minWidth;

	protected override void ResolveIcon()
	{
		base.ResolveIcon();
		uiIcon = graphicData.Materials[0].mainTexture as Texture2D;
	}

	public override void PostLoad()
	{
		base.PostLoad();
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			graphicData.InitMainTextures();
			graphicData.InitFadeOutTextures();
		});
	}

	public override IEnumerable<string> ConfigErrors()
	{
		return base.ConfigErrors();
	}
}
