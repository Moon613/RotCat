using System;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Chimeric
{
	public class RotCatBubble : CosmeticSprite
	{
		public RotCatBubble(Player owner, Vector2 dir, float intensity, float stickiness, float extraSpeed)
		{
			this.owner = owner;
			this.stickiness = stickiness;
			this.direction = dir.normalized;
			this.pos = owner.mainBodyChunk.pos;
			this.lastPos = this.pos;
			this.life = 1f;
			this.lifeTime = 10 + Random.Range(0, Random.Range(0, Random.Range(0, Random.Range(0, 200))));
			this.vel = dir + Custom.DegToVec(Random.value * 360f) * (Random.value * Random.value * 12f * Mathf.Pow(intensity, 1.5f) + extraSpeed);
			this.hollowNess = Mathf.Lerp(0.5f, 1.5f, Random.value);
			if (stickiness == 0f)
			{
				this.freeFloating = true;
			}
			else
			{
				this.stuckToOrigin = true;
			}
			this.liberatedOrigin = this.pos;
			this.lastLiberatedOrigin = this.pos;
			if (this.freeFloating)
			{
				this.freeCounter++;
			}
		}
		public override void Update(bool eu)
		{
			this.vel = Vector3.Slerp(this.vel, Custom.DegToVec(Random.value * 360f), 0.2f);
			this.vel += this.direction * Mathf.Sin(this.life * 3.1415927f);
			this.lastLife = this.life;
			this.life -= 1f / (float)this.lifeTime;
			if (this.life <= 0f)
			{
				this.Destroy();
			}
			if (this.freeFloating && Random.value < 0.5f)
			{
				this.hollow = (Random.value < 0.5f);
			}
			if (this.room.GetTile(this.pos).Terrain == Room.Tile.TerrainType.Solid)
			{
				this.lifeTime = Math.Min(1, this.lifeTime - 5);
			}
			bool flag;
			if (ModManager.MSC)
			{
				flag = this.room.PointSubmerged(this.pos);
			}
			else
			{
				flag = (this.pos.y < this.room.FloatWaterLevel(this.pos.x));
			}
			if (flag)
			{
				this.vel *= 0.9f;
				this.vel.y = this.vel.y + 4f;
			}
			if (this.stuckToOrigin)
			{
				Vector2 position = this.owner.mainBodyChunk.pos;
				this.liberatedOriginVel = position - this.liberatedOrigin;
				this.liberatedOrigin = position;
				this.lastLiberatedOrigin = position;
				if (this.life < 0.5f || Random.value < 1f / (10f + this.stickiness * 80f) || !Custom.DistLess(this.pos, position, 10f + 90f * this.stickiness))
				{
					this.stuckToOrigin = false;
				}
			}
			else if (!this.freeFloating)
			{
				this.lastLiberatedOrigin = this.liberatedOrigin;
				this.liberatedOriginVel = Vector2.Lerp(this.liberatedOriginVel, Custom.DirVec(this.liberatedOrigin, this.pos) * Mathf.Lerp(Vector2.Distance(this.liberatedOrigin, this.pos), 10f, 0.5f), 0.7f);
				this.liberatedOrigin += this.liberatedOriginVel;
				if (Custom.DistLess(this.liberatedOrigin, this.pos, 5f))
				{
					this.vel = Vector2.Lerp(this.vel, this.liberatedOriginVel, 0.3f);
					this.lifeTimeWhenFree = this.life;
					this.freeFloating = true;
				}
			}
			base.Update(eu);
		}
		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("LizardBubble0", true);
			this.AddToContainer(sLeaser, rCam, null);
		}
		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 0.625f * Mathf.Lerp(Mathf.Lerp(Mathf.Sin(3.1415927f * this.lastLife), this.lastLife, 0.5f), Mathf.Lerp(Mathf.Sin(3.1415927f * this.life), this.life, 0.5f), timeStacker);
			sLeaser.sprites[0].x = Mathf.Lerp(this.lastPos.x, this.pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(this.lastPos.y, this.pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].color = Color.Lerp(PlayerGraphics.SlugcatColor(owner.slugcatStats.name)/*GetColor*/, Color.blue, Mathf.InverseLerp(2f, 7f, (float)this.freeCounter + timeStacker));
			float num2 = (sLeaser.sprites[0].color.r + sLeaser.sprites[0].color.g + (1f - sLeaser.sprites[0].color.b)) / 3f;
			int num3 = 0;
			if (this.hollow)
			{
				num3 = Custom.IntClamp((int)(Mathf.Pow(Mathf.InverseLerp(this.lifeTimeWhenFree, 0f, this.life), this.hollowNess) * 7f), 1, 7);
			}
			sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("LizardBubble" + num3.ToString());
			if (this.stuckToOrigin || !this.freeFloating)
			{
				Vector2 vector;
				if (this.stuckToOrigin)
				{
					vector = this.owner.mainBodyChunk.pos;
				}
				else
				{
					vector = Vector2.Lerp(this.lastLiberatedOrigin, this.liberatedOrigin, timeStacker);
				}
				float num4 = Vector2.Distance(Vector2.Lerp(this.lastPos, this.pos, timeStacker), vector) / 16f;
				sLeaser.sprites[0].scaleX = Mathf.Min(num, num / Mathf.Lerp(num4, 1f, 0.35f)) * (1f - 0.75f * num2);
				sLeaser.sprites[0].scaleY = Mathf.Max(num, num4 - 0.125f);
				sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(this.lastPos, this.pos, timeStacker), vector);
				sLeaser.sprites[0].anchorY = 0f;
			}
			else
			{
				sLeaser.sprites[0].scaleX = num;
				sLeaser.sprites[0].scaleY = num;
				sLeaser.sprites[0].rotation = 0f;
				sLeaser.sprites[0].anchorY = 0.5f;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Midground");
			}
			foreach (FSprite fsprite in sLeaser.sprites)
			{
				fsprite.RemoveFromContainer();
				newContatiner.AddChild(fsprite);
			}
		}
		public Player owner;
		public float life;
		private float lastLife;
		public int lifeTime;
		private float hollowNess;
		public bool hollow;
		public Vector2 originPoint;
		public bool stuckToOrigin;
		public bool freeFloating;
		public Vector2 liberatedOrigin;
		public Vector2 liberatedOriginVel;
		public Vector2 lastLiberatedOrigin;
		public Vector2 direction;
		public float lifeTimeWhenFree;
		public float stickiness;
		public int freeCounter;
	}
}