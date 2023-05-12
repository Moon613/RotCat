using RWCustom;
using UnityEngine;

namespace RotCat;

public class CreaturePing : CosmeticSprite
{
    public Vector2 soundPos;
    public Color col;
    public float initialRad;
    public float rad;
    public float newRad;
    public float counter;
    public bool contracted;

    public CreaturePing(Vector2 soundPos, Color col, float rad, Room room)
    {
        this.soundPos = soundPos;
        this.col = col;
        this.initialRad = rad;
        this.rad = rad;
        this.counter = 0.01f;
        this.contracted = false;
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!contracted) {
            rad = newRad;
            newRad = Mathf.Lerp(initialRad, 0f, counter);
            counter *= 1.5f;//+= 0.05f;
        }
        if (counter > 1f && !contracted) {
            contracted = true;
            this.Destroy();
        }
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White", true);
        sLeaser.sprites[0].scale = rad;
        sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
        sLeaser.sprites[0].color = new Color(0.08f, 0.2f, 0.9f, 0.4f);
        this.AddToContainer(sLeaser, rCam, RotCat.darkContainer);
        for (int j = 0; j < Random.Range(2, 11); j++) {
            Spark newSpark = new Spark(soundPos, Custom.RNV()*Random.Range(3f, 11f), Color.white, null, 4, 10);
            RotCat.sparkLayering.Add(newSpark, new SparkEx(true));
            rCam.room.AddObject(newSpark);
        }
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].SetPosition(soundPos-camPos);
        sLeaser.sprites[0].scale = Mathf.Lerp(rad, newRad, timeStacker);
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
    }
}