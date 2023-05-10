using RWCustom;
using UnityEngine;

namespace RotCat;

public class EatingRot : CosmeticSprite
{
    private float maxRad;
    private Color bodyColor;
    private Color xColor;

    public EatingRot(float maxRad, Color bodyColor, Color xColor, Vector2 pos) {
        this.maxRad = maxRad;
        this.bodyColor = bodyColor;
        this.xColor = xColor;
        this.pos = pos;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite("roteye", false);
        sLeaser.sprites[0].color = bodyColor;
        sLeaser.sprites[0].scale = 0.1f;
        sLeaser.sprites[1] = new FSprite("roteyeeye", false);
        sLeaser.sprites[1].color = xColor;
        sLeaser.sprites[1].scale = 0.1f;
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (sLeaser.sprites[0].scale < maxRad && sLeaser.sprites[1].scale < maxRad) {
            sLeaser.sprites[0].scale *= 1.4f;
            sLeaser.sprites[1].scale *= 1.4f;
        }
    }
}