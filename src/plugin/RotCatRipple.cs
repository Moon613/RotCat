using RWCustom;
using UnityEngine;

namespace RotCat;
public class RotCatRipple : CosmeticSprite
{
	public RotCatRipple(Player owner, Vector2 soundPos, Vector2 vel, float intensity, Color col)
	{
		this.owner = owner;
		this.pos = owner.mainBodyChunk.pos;
		this.lastPos = this.pos;
		this.vel = vel;
		this.intensity = intensity;
		this.soundPos = soundPos;
		this.col = col;
		this.radVel = Mathf.Lerp(0.9f, 2.85f, intensity);
		this.initRad = Mathf.Lerp(1.45f, 4.5f, intensity);
		this.rad = this.initRad;
		this.lastRad = this.initRad;
		this.life = 1f;
		this.lastLife = 0f;
		this.lifeTime = Mathf.Lerp(6f, 30f, Mathf.Pow(intensity, 4f));
	}
	public override void Update(bool eu)
	{
		base.Update(eu);
		this.lastRad = this.rad;
		this.rad += this.radVel;
		this.radVel *= 0.92f;
		this.radVel -= Mathf.InverseLerp(0.6f + 0.3f * this.intensity, 0f, this.life) * Mathf.Lerp(0.2f, 0.6f, this.intensity);
		Vector2 b = this.owner.mainBodyChunk.pos + Custom.DirVec(this.owner.mainBodyChunk.pos, this.soundPos) * 80f * Mathf.Sin(this.life * 3.1415927f);
		this.pos = Vector2.Lerp(this.pos, b, 0.3f * (1f - Mathf.Sin(this.life * 3.1415927f)));
		this.lastLife = this.life;
		this.life = Mathf.Max(0f, this.life - 1f / this.lifeTime);
		if (this.lastLife <= 0f && this.life <= 0f)
		{
			this.Destroy();
		}
	}
	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1]{new FSprite("Futile_White", true)};
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["VectorCircle"];
		this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}
	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Lerp(this.lastPos.x, this.pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(this.lastPos.y, this.pos.y, timeStacker) - camPos.y;
		float between0And1 = Mathf.Lerp(this.lastLife, this.life, timeStacker);
		float between0And0Point75 = Mathf.InverseLerp(0f, 0.75f, between0And1);
		sLeaser.sprites[0].color = Color.Lerp((between0And0Point75 > 0.5f) ? PlayerGraphics.SlugcatColor(owner.slugcatStats.name)/*GetColor*/ : this.blackCol, Color.Lerp(this.blackCol, this.col, 0.5f + 0.5f * this.intensity), Mathf.Sin((between0And0Point75+0.15f) * 3.1415927f));  //Controls the color flashing, per normal Update. Lerps between either the Player's color or black based on between0And0Point75, and not quite white
		float scaleOfSprite = Mathf.Lerp(this.lastRad, this.rad, timeStacker);
		sLeaser.sprites[0].scale = scaleOfSprite / 8f;
		sLeaser.sprites[0].alpha = Mathf.Sin(Mathf.Pow(between0And1, 2f) * 3.1415927f) * 2f / scaleOfSprite;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		this.blackCol = palette.blackColor;
	}
	private Player owner;
	private float rad;
	private float lastRad;
	private float radVel;
	private float initRad;
	private float lifeTime;
	private float lastLife;
	private float life;
	private float intensity;
	private Vector2 soundPos;
	private Color col;
	private Color blackCol;
}