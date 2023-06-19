using UnityEngine;

namespace Chimeric
{
    public class EatingRot : CosmeticSprite
    {
        public float maxRad;
        public Color bodyColor;
        public Color xColor;
        public FSprite sprite;
        public Creature crit;
        public bool hideSpritesInPipe = false;
        public int indexInArray;

        public EatingRot(float maxRad, Color bodyColor, Color xColor, Vector2 pos, FSprite sprite, Creature crit, int indexInArray) {
            this.maxRad = maxRad;
            this.bodyColor = bodyColor;
            this.xColor = xColor;
            this.sprite = sprite;
            this.crit = crit;
            this.indexInArray = indexInArray;
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("roteye", false);
            sLeaser.sprites[0].color = bodyColor;
            sLeaser.sprites[0].scale = 0.025f;
            sLeaser.sprites[0].SetPosition(pos);
            sLeaser.sprites[1] = new FSprite("roteyeeye", false);
            sLeaser.sprites[1].color = xColor;
            sLeaser.sprites[1].scale = 0.03f;
            sLeaser.sprites[1].SetPosition(pos);
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].SetPosition(sprite.GetPosition());
            sLeaser.sprites[1].SetPosition(sprite.GetPosition());
            sLeaser.sprites[0].MoveInFrontOfOtherNode(sprite);
            sLeaser.sprites[1].MoveInFrontOfOtherNode(sLeaser.sprites[0]);
            //Debug.Log($"Sprite positions are: {sLeaser.sprites[0].GetPosition()}");
            if (sLeaser.sprites[0].scale < maxRad && sLeaser.sprites[1].scale < maxRad) {
                sLeaser.sprites[0].scale *= 1.08f;
                sLeaser.sprites[1].scale *= 1.08f;
            }
            if (sprite.isVisible) {
                sLeaser.sprites[0].isVisible = true;
                sLeaser.sprites[1].isVisible = true;
                //Debug.Log("Sprites are visible");
            }
            //Debug.Log(hideSpritesInPipe);
            if (!sprite.isVisible || hideSpritesInPipe) {
                sLeaser.sprites[0].isVisible = false;
                sLeaser.sprites[1].isVisible = false;
                //Debug.Log("Sprites are not visible");
            }
        }
        public override void Update(bool eu)
        {
            if (crit.slatedForDeletetion) {
                Destroy();
            }
            base.Update(eu);
        }
        public void ReassignSprites(FSprite sprite)
        {
            this.sprite = sprite;
        }
    }
}