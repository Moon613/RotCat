using MonoMod.RuntimeDetour;
using RWCustom;
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
            On.CentipedeGraphics.DrawSprites += DrawSprites;
        }
        public static float GenerateSize(On.Centipede.orig_GenerateSize orig, AbstractCreature abstrCrit) {
            if (abstrCrit.creatureTemplate.type == CreatureTemplateType.BabyAquapede) {
                return 0f;
            }
            return orig(abstrCrit);
        }
        public static void DrawSprites(On.CentipedeGraphics.orig_DrawSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.centipede.Template.type == CreatureTemplateType.BabyAquapede) {
                for (int i = 0; i < 2; i++) {
                    for (int k = 0; k < self.wingPairs; k++) {
                        Vector2 vector10;
                        if (k == 0)
                        {
                            vector10 = Custom.DirVec(self.ChunkDrawPos(0, timeStacker), self.ChunkDrawPos(1, timeStacker));
                        }
                        else
                        {
                            vector10 = Custom.DirVec(self.ChunkDrawPos(k - 1, timeStacker), self.ChunkDrawPos(k, timeStacker));
                        }
                        Vector2 vector11 = Custom.PerpendicularVector(vector10) / 2f;
                        Vector2 vector12 = self.RotatAtChunk(k, timeStacker);
                        Vector2 vector13 = self.WingPos(i, k, vector10, vector11, vector12, timeStacker);
                        Vector2 vector14 = self.ChunkDrawPos(k, timeStacker) + self.centipede.bodyChunks[k].rad * ((i == 0) ? -1f : 1f) * vector11 * vector12.y;
                        
                        Vector2 lhs = Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector13, vector14) + Custom.VecToDeg(vector12));
                        float num12 = Mathf.InverseLerp(0.85f, 1f, Vector2.Dot(lhs, Custom.DegToVec(45f))) * Mathf.Abs(Vector2.Dot(Custom.DegToVec(45f + Custom.VecToDeg(vector12)), vector10));
                        Vector2 lhs2 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector14, vector13) + Custom.VecToDeg(vector12));
                        float b2 = Mathf.InverseLerp(0.85f, 1f, Vector2.Dot(lhs2, Custom.DegToVec(45f))) * Mathf.Abs(Vector2.Dot(Custom.DegToVec(45f + Custom.VecToDeg(vector12)), -vector10));
                        num12 = Mathf.Pow(Mathf.Max(num12, b2), 0.5f);
                        float d7 = 1.75f;
                        (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).MoveVertice(1, vector13 + vector10 * d7 - camPos);
                        (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).MoveVertice(0, vector13 - vector10 * d7 - camPos);
                        (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).MoveVertice(2, vector14 + vector10 * d7 - camPos);
                        (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).MoveVertice(3, vector14 - vector10 * d7 - camPos);
                        (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).verticeColors[0] = Custom.HSL2RGB(0.99f - 0.4f * Mathf.Pow(num12, 2f), 1f, 0.5f + 0.5f * num12, 0.5f + 0.5f * num12);
                        (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).verticeColors[1] = Custom.HSL2RGB(0.99f - 0.4f * Mathf.Pow(num12, 2f), 1f, 0.5f + 0.5f * num12, 0.5f + 0.5f * num12);
                        (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).verticeColors[2] = Color.Lerp(new Color(self.blackColor.r, self.blackColor.g, self.blackColor.b), new Color(1f, 1f, 1f), 0.5f * num12);
                        (sLeaser.sprites[self.WingSprite(i, k)] as CustomFSprite).verticeColors[3] = Color.Lerp(new Color(self.blackColor.r, self.blackColor.g, self.blackColor.b), new Color(1f, 1f, 1f), 0.5f * num12);
                    }
                }
            }
        }
    }
}