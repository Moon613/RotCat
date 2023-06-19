using MonoMod.RuntimeDetour;
using System;
using UnityEngine;
using static System.Reflection.BindingFlags;

namespace Chimeric
{
    public class BabyAquapedeHooks
    {
        internal static void Apply()
        {
            On.ArenaCreatureSpawner.IsMajorCreature += (orig, type) => type == CreatureTemplateType.BabyAquapede || orig(type);

            new Hook(typeof(Centipede).GetMethod("get_Small", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.BabyAquapede || orig(self));

            new Hook(typeof(Centipede).GetMethod("get_AquaCenti", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.BabyAquapede || orig(self));

            On.Centipede.GenerateSize += GenerateSize;
            On.CentipedeGraphics.InitiateSprites += InitiateSprites;
        }
        public static float GenerateSize(On.Centipede.orig_GenerateSize orig, AbstractCreature abstrCrit) {
            if (abstrCrit.creatureTemplate.type == CreatureTemplateType.BabyAquapede) {
                return 0f;
            }
            return orig(abstrCrit);
        }
        public static void InitiateSprites(On.CentipedeGraphics.orig_InitiateSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            orig(self, sLeaser, rCam);
            for (int i = 0; i < 2; i++) {
                for (int k = 0; k < self.wingPairs; k++) {
                    Debug.Log($"i: {i} k: {k}");
                    //(sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).MoveVertice(0, new Vector2((sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).vertices[0].x - 120f, (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).vertices[0].y));
                    //sLeaser.sprites[self.WingSprite(i, k)].y += 50f;
                    //sLeaser.sprites[self.WingSprite(i, k)].SetPosition(sLeaser.sprites[self.WingSprite(i, k)].GetPosition()+new Vector2(0, 50f));
                }
            }
        }
    }
}