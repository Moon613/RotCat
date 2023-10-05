using UnityEngine;
using RWCustom;
using System;
using Random = UnityEngine.Random;
using static TriangleMesh;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Chimeric
{
    public class DynamoGraphics
    {
        public static void Apply() {
            On.PlayerGraphics.ctor += DynoGrafCtor;
            On.PlayerGraphics.InitiateSprites += DynoInitSprites;
            On.PlayerGraphics.DrawSprites += DynoGrafDrawSprites;
            On.PlayerGraphics.Update += DynoGrafUpdate;
            IL.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSpritesTail;
            On.PlayerGraphics.AddToContainer += DynoAddToContainer;
        }
        public static void DynoGrafDrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo) {
                //Debug.Log($"{sLeaser.sprites[3].color.ToString()}");
                if (something.crawlToRoll) {
                    sLeaser.sprites[1].SetPosition(new Vector2(self.player.bodyChunks[1].pos.x + (5f * self.player.flipDirection), self.player.bodyChunks[1].pos.y + 4.5f) - camPos);
                }
                
                for (int i = 0; i < something.fList.Count; i++) {
                    //Debug.Log($"{sLeaser.sprites?[something.initialFinSprite + i]} and {sLeaser.sprites?[0]}");
                    //Debug.Log(self.player.bodyChunks[0].Rotation);
                    if (something.fList[i].connectedSprite != null) {
                        sLeaser.sprites[something.initialFinSprite + i].SetPosition(Functions.RotateAroundPoint(something.fList[i].connectedSprite.GetPosition(), something.fList[i].posOffset, (-something.fList[i].connectedSprite.rotation)));

                        sLeaser.sprites[something.initialFinSprite + i].rotation = something.fList[i].connectedSprite.rotation+90-(something.fList[i].additionalRotation * (something.fList[i].flipped? -1f : 1f)) * (something.fList[i].connectedSprite == sLeaser.sprites[3]? 1f : -1f);
                    }
                    else if (something.fList[i].connectedTailSegment?.connectedSegment != null) {
                        sLeaser.sprites[something.initialFinSprite + i].SetPosition(Vector2.Lerp(Functions.RotateAroundPoint(something.fList[i].connectedTailSegment.lastPos-camPos, something.fList[i].posOffset, -Custom.VecToDeg((something.fList[i].connectedTailSegment.lastPos-something.fList[i].connectedTailSegment.connectedSegment.lastPos).normalized)), Functions.RotateAroundPoint(something.fList[i].connectedTailSegment.pos-camPos, something.fList[i].posOffset, -Custom.VecToDeg((something.fList[i].connectedTailSegment.pos-something.fList[i].connectedTailSegment.connectedSegment.pos).normalized)), timeStacker));
                            
                        sLeaser.sprites[something.initialFinSprite + i].rotation = Custom.VecToDeg((something.fList[i].connectedTailSegment.pos-something.fList[i].connectedTailSegment.connectedSegment.pos).normalized * -1)-90-(something.fList[i].additionalRotation * (something.fList[i].flipped? 1f : -1f));
                    }
                    //This is mostly unused, will have to update it I ever use it
                    else if (something.fList[i].connectedTailSegment?.connectedSegment == null) {
                        Debug.Log($"LMAO YOU NEED TO UPDATE THIS TO WORK CORRECTLY {i}");
                        Debug.LogWarning("Depreciated code being used!");
                        sLeaser.sprites[something.initialFinSprite + i].SetPosition(Functions.RotateAroundPoint(self.player.bodyChunks[1].pos-camPos, something.fList[i].posOffset, -something.fList[i].connectedSprite.rotation));

                        sLeaser.sprites[something.initialFinSprite + i].rotation = Custom.VecToDeg(self.player.bodyChunks[1].Rotation)+90-(something.fList[i].additionalRotation * (something.fList[i].flipped? 1 : -1));
                    }
                    
                    sLeaser.sprites[something.initialFinSprite + i].color = self.HypothermiaColorBlend(SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2));
                    
                    float addition = self.player.submerged? Mathf.Lerp(something.fList[i].swimRange[0], something.fList[i].swimRange[1], Mathf.Sin(something.fList[i].swimCycle)) : 0;
                    //Debug.Log($"addition is: {addition} and {something.fList[i].swimRange[0]} and {something.fList[i].swimRange[1]} and {Mathf.Sin(something.fList[i].swimCycle)}");
                    something.fList[i].additionalRotation = Mathf.Lerp(something.fList[i].foldRotation, something.fList[i].startAdditionalRotation, (float)something.timeInWater/40f) + addition;
                    
                    if (something.fList[i].corriderTimer > 0) {
                        something.fList[i].additionalRotation = Mathf.Lerp(something.fList[i].foldRotation, -89.5f, something.fList[i].corriderTimer/20f);
                    }
                    //Debug.Log($"startAdditionalRotation: {something.fList[i].startAdditionalRotation} additionalRotation: {something.fList[i].additionalRotation} timeInWater: {something.timeInWater} normal rotation: {sLeaser.sprites[something.initialFinSprite+i].rotation}");
                }
                self.gills.DrawSprites(sLeaser, rCam, timeStacker, camPos);
                Color effectedColor = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2);
                for (int i = self.gills.startSprite + self.gills.scalesPositions.Length - 1; i >= self.gills.startSprite; i--)
                {
                    sLeaser.sprites[i].color = self.gills.baseColor;
                    if (self.gills.colored)
                    {
                        sLeaser.sprites[i + self.gills.scalesPositions.Length].color = Color.Lerp(effectedColor, self.gills.baseColor, self.malnourished / 1.75f);
                    }
                }
            }
        }
        public static void DynoGrafUpdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo) {
                //Debug.Log(self.player.bodyChunks[0].pos.y - self.player.bodyChunks[1].pos.y);
                //Debug.Log(self.player.animation.ToString().ToLower());
                if (self.player.input[0].pckp && self.player.bodyChunks[0].pos.y <= self.player.bodyChunks[1].pos.y+11f && self.player.bodyChunks[0].pos.y >= self.player.bodyChunks[1].pos.y-11f && self.player.animation == Player.AnimationIndex.None && self.player.bodyMode == Player.BodyModeIndex.Crawl) {
                    if (self.tail[self.tail.Length-1].pos.y <= self.player.bodyChunks[1].pos.y+40f) {
                        self.tail[self.tail.Length-1].vel.y = 3f;
                    }
                    float directionOfBody = Mathf.Sign((self.player.bodyChunks[0].pos - self.player.bodyChunks[1].pos).normalized.x);
                    if (directionOfBody == 1) {
                        if (self.tail[self.tail.Length-1].pos.x <= (self.player.bodyChunks[1].pos.x+10f)) {
                            self.tail[self.tail.Length-1].vel.x = 3f;
                        }
                        self.tail[2].vel.x = -1.65f;
                        self.tail[4].vel.x = -1.495f;
                        //self.tail[6].vel.x = -0.8f;
                    }
                    else if (directionOfBody == -1) {
                        if (self.tail[self.tail.Length-1].pos.x >= (self.player.bodyChunks[1].pos.x-10f)) {
                            self.tail[self.tail.Length-1].vel.x = -3f;
                        }
                        self.tail[2].vel.x = 1.65f;
                        self.tail[4].vel.x = 1.495f;
                        //self.tail[6].vel.x = 0.8f;
                    }
                    if (something.canPlayShockSound) {
                        self.player.room.PlaySound(SoundID.Centipede_Electric_Charge_LOOP, self.player.mainBodyChunk.pos, 1f, 1f);
                        something.canPlayShockSound = false;
                    }
                    for (int i = 0; i < self.player.room.abstractRoom.creatures.Count; i++) {
                        if (Plugin.creatureYummersSprites.TryGetValue(self.player.room.abstractRoom.creatures[i].realizedCreature, out var thing)) {
                            thing.shouldFearDynamo = true;
                            if (thing.fearTime == -1) {
                                thing.fearTime = ChimericOptions.scareDuration.Value;
                            }
                            //Debug.Log("Should be fearing dyno");
                        }
                    }
                }
                else if (!something.canPlayShockSound) {
                    something.canPlayShockSound = true;
                    for (int i = 0; i < self.player.room.abstractRoom.creatures.Count; i++) {
                        if (Plugin.creatureYummersSprites.TryGetValue(self.player.room.abstractRoom.creatures[i].realizedCreature, out var thing)) {
                            thing.shouldFearDynamo = false;
                            thing.fearTime = -1;
                            //Debug.Log("Should not be fearing dyno");
                        }
                    }
                }
                
                #region Roll From Crouch Indication
                if (something.crawlToRoll)
                {
                    self.tail[self.tail.Length - 1].vel = Custom.DirVec(self.tail[self.tail.Length - 1].pos, self.player.bodyChunks[1].pos + new Vector2(35f * -self.player.flipDirection, 24f));
                    self.player.allowRoll = 20;
                }
                #endregion
                
                #region Roll Animation for tail and killing things
                if (self.player.animation == Player.AnimationIndex.Roll) {
                    for (int i = 0; i < self.tail.Length; i++)
                    {
                        float startVel = Custom.VecToDeg(Custom.DirVec(self.tail[i].pos, self.tail[i-1].pos));
                        startVel += 45 * -something.initialFlipDirection;
                        self.tail[i].vel = Custom.DegToVec(startVel) * 15;
                        if (self.player.bodyChunks[0].pos.y >= self.player.bodyChunks[1].pos.y && self.player.animation == Player.AnimationIndex.Roll) {
                            self.tail[i].vel.x *= 2.2f;
                            self.tail[i].vel.y -= 0.25f;
                        }
                    }
                    // Absolutely KILLING THINGS bit
                    for (int i = 0; i < self.player.room.abstractRoom.creatures.Count; i++) {
                        Creature? crit = self.player.room.abstractRoom.creatures[i].realizedCreature;
                        foreach (BodyChunk chunk in crit.bodyChunks) {
                            if (Custom.DistLess(chunk.pos, self.tail[self.tail.Length - 2].pos, 55f) && crit != self.player && (crit is not Player || ChimericOptions.FriendlyFire.Value)) {
                                crit.Violence(self.player.mainBodyChunk, null, chunk, null, Creature.DamageType.Blunt, 0.4f, 100f);
                                crit.SetKillTag(self.player.abstractCreature);
                                chunk.vel += new Vector2(ChimericOptions.yeetusMagnitude.Value * self.player.flipDirection, 19f) / (chunk.mass * 3f);
                                Debug.Log($"{crit.Template.type} was launched by Dynamo");
                                Debug.Log($"{chunk.mass} and {chunk.vel}");
                            }
                        }
                    }
                }
                something.initialFlipDirection = self.player.flipDirection;
                #endregion
                self.gills.Update();
            }
        }
        public static void DynoInitSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo) {
                Debug.Log($"Initiate Sprites Dynamo");
                if (sLeaser.sprites[2] is TriangleMesh tail)
                {
                    sLeaser.sprites[2].RemoveFromContainer();

                    Triangle[] array = new Triangle[(self.tail.Length - 1) * 4 + 1];
                    for (int i = 0; i < self.tail.Length - 1; i++)
                    {
                        int num = i * 4;
                        for (int j = 0; j < 4; j++)
                        {
                            array[num + j] = new Triangle(num + j, num + j + 1, num + j + 2);
                        }
                    }
                    array[(self.tail.Length - 1) * 4] = new Triangle((self.tail.Length - 1) * 4, (self.tail.Length - 1) * 4 + 1, (self.tail.Length - 1) * 4 + 2);
                    tail = new TriangleMesh("Futile_White", array, tail.customColor, false);
                    sLeaser.sprites[2] = tail;

                    rCam.ReturnFContainer("Midground").AddChild(tail);
                    tail.MoveBehindOtherNode(sLeaser.sprites[4]);
                }
                if (something.fList.Count != 0) {
                    something.fList.Clear();
                }
                something.fList.Add(new Fin(sLeaser.sprites[3], new Vector2(5f, 0), -15f, 0.7f, 0.42f, false, -70f, new List<float>(2){-30f, 30f}, startSwimCycle:Mathf.PI/2f));
                something.fList.Add(new Fin(sLeaser.sprites[3], new Vector2(5f, -9.5f), -10f, 0.7f, 0.42f, false, -70f, new List<float>(2){45f, -45f}, startSwimCycle:3f*Mathf.PI/4f));
                something.fList.Add(new Fin(sLeaser.sprites[1], new Vector2(4f, 10f), -15f, 0.7f, 0.42f, false, -60f, new List<float>(2){-30f, 30f}, startSwimCycle:Mathf.PI/2f));
                something.fList.Add(new Fin(self.tail[self.tail.Length-1], new Vector2(1f, -5f), -45f, 0.5f, 0.2f, false, -70f, new List<float>(2){45f, -45f}, startSwimCycle:Mathf.PI/6f));
                something.fList.Add(new Fin(self.tail[self.tail.Length-2], new Vector2(2f, -10f), -35f, 0.52f, 0.269f, false, -66f, new List<float>(2){-30f, 30f}));
                something.fList.Add(new Fin(self.tail[self.tail.Length-4], new Vector2(3f, -5f), -35f, 0.585f, 0.33f, false, -62f, new List<float>(2){45f, -45f}, startSwimCycle:2f*Mathf.PI/3f));
                something.fList.Add(new Fin(self.tail[self.tail.Length-5], new Vector2(4f, -5f), -35f, 0.61f, 0.37f, false, -60f, new List<float>(2){-30f, 30f}));
                something.fList.Add(new Fin(self.tail[self.tail.Length-6], new Vector2(5f, -8f), -40f, 0.7f, 0.41f, false, -62f, new List<float>(2){45f, -45f}, startSwimCycle:Mathf.PI/2f));
                something.fList.Add(new Fin(sLeaser.sprites[3], new Vector2(-5f, 0), -15f, 0.7f, 0.42f, true, -70f, new List<float>(2){30f, -30f}, startSwimCycle:Mathf.PI/4f));
                something.fList.Add(new Fin(sLeaser.sprites[3], new Vector2(-5f, -9.5f), -10f, 0.7f, 0.42f, true, -70f, new List<float>(2){-45f, 45f}, startSwimCycle:Mathf.PI/2f));
                something.fList.Add(new Fin(sLeaser.sprites[1], new Vector2(-4f, 10f), -15f, 0.7f, 0.42f, true, -60f, new List<float>(2){30f, -30f}, startSwimCycle:Mathf.PI/3f));
                something.fList.Add(new Fin(self.tail[self.tail.Length-1], new Vector2(-1f, -5f), -45f, 0.5f, 0.2f, true, -70f, new List<float>(2){-45f, 45f}, startSwimCycle:Mathf.PI/4f));
                something.fList.Add(new Fin(self.tail[self.tail.Length-2], new Vector2(-2f, -10f), -35f, 0.52f, 0.269f, true, -66f, new List<float>(2){30f, -30f}));
                something.fList.Add(new Fin(self.tail[self.tail.Length-4], new Vector2(-3f, -5f), -35f, 0.585f, 0.33f, true, -62f, new List<float>(2){-45f, 45f}, startSwimCycle:11f*Mathf.PI/12f));
                something.fList.Add(new Fin(self.tail[self.tail.Length-5], new Vector2(-4f, -5f), -35f, 0.61f, 0.37f, true, -60f, new List<float>(2){30f, -30f}, startSwimCycle:Mathf.PI/2f));
                something.fList.Add(new Fin(self.tail[self.tail.Length-6], new Vector2(-5f, -8f), -40f, 0.7f, 0.41f, true, -62f, new List<float>(2){-45f, 45f}));

                something.initialFinSprite = sLeaser.sprites.Length;
                something.initialGillSprite = sLeaser.sprites.Length+something.fList.Count;
                self.gills = new PlayerGraphics.AxolotlGills(self, something.initialGillSprite);
                Array.Resize<FSprite>(ref sLeaser.sprites, sLeaser.sprites.Length+something.fList.Count+self.gills.numberOfSprites);
                self.gills.InitiateSprites(sLeaser, rCam);
                for (int i = something.initialFinSprite; i < something.initialGillSprite; i++) {
                    sLeaser.sprites[i] = new FSprite("DynamoFin" + Random.Range(3, 5).ToString(), false) {
                        shader = rCam.room.game.rainWorld.Shaders["CicadaWing"],
                        //The scale x is y and scale y is x because cursed reasons (I loaded the sprites in vertically)
                        scaleX = something.fList[i-something.initialFinSprite].scaleX,
                        scaleY = something.fList[i-something.initialFinSprite].scaleY * (something.fList[i-something.initialFinSprite].flipped ? -1 : 1)
                    };
                }
                self.AddToContainer(sLeaser, rCam, null);
            }
        }
        public static void DynoGrafCtor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo) {
                Array.Resize<TailSegment>(ref self.tail, 7);
                self.tail[0] = new TailSegment(self, 6f, 4.1f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 5.6f, 6.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 5f, 8.2f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 4.8f, 8f, self.tail[2], 0.85f, 1f, 0.5f, true);
                self.tail[4] = new TailSegment(self, 3.6f, 8f, self.tail[3], 0.85f, 1f, 0.5f, true);
                self.tail[5] = new TailSegment(self, 2.4f, 8f, self.tail[4], 0.85f, 1f, 0.5f, true);
                self.tail[6] = new TailSegment(self, 1f, 8f, self.tail[5], 0.85f, 1f, 0.5f, true);
            }
        }
        public static void DynoAddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            orig(self, sLeaser, rCam, newContainer);
            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo && sLeaser.sprites.Length > 13 && sLeaser.sprites != null) {
                for (int i = 0; i < sLeaser.sprites?.Length-something.initialFinSprite; i++) {
                    sLeaser.sprites?[something.initialFinSprite + i].RemoveFromContainer();
                    rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites?[something.initialFinSprite + i]);
                    sLeaser.sprites?[something.initialFinSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
                }
            }
        }
        public static void PlayerGraphics_DrawSpritesTail(ILContext il) //Copied from DMS tail method
        {
            var cursor = new ILCursor(il);
            try
            {
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(2), i => i.MatchLdloc(1), i => i.MatchLdcR4(0.5f)))
                {
                    throw new Exception("Failed to match IL for PlayerGraphics_DrawSpritesTail! (first)");
                }
                var label = cursor.DefineLabel();
                cursor.MarkLabel(label);
                if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdcR4(6), i => i.MatchStloc(7)))
                {
                    throw new Exception("Failed to match IL for PlayerGraphics_DrawSpritesTail! (second)");
                }

                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Ldarg_2);
                cursor.Emit(OpCodes.Ldarg_3);
                cursor.Emit(OpCodes.Ldarg, 4);
                cursor.EmitDelegate((PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) =>
                {
                    if (sLeaser.sprites[2] is not TriangleMesh tailSprite)
                    {
                        return false;
                    }
                    if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && !something.isDynamo) {
                        return false;
                    }

                    float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * (float)Math.PI * 2f);
                    float num3 = 1f - 0.2f * self.malnourished;
                    float num4 = self.tail[0].rad;

                    Vector2 drawPosition = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                    Vector2 secondDrawPosition = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                    if (self.player.aerobicLevel > 0.5f)
                    {
                        drawPosition += Custom.DirVec(secondDrawPosition, drawPosition) * Mathf.Lerp(-1f, 1f, num) * Mathf.InverseLerp(0.5f, 1f, self.player.aerobicLevel) * 0.5f;

                    }
                    Vector2 val4 = (secondDrawPosition * 3f + drawPosition) / 4f;

                    for (int i = 0; i < self.tail.Length; i++)
                    {
                        Vector2 posBetweenTailSegmentPosAndLastPos = Vector2.Lerp(self.tail[i].lastPos, self.tail[i].pos, timeStacker);
                        Vector2 val6 = posBetweenTailSegmentPosAndLastPos - val4;
                        Vector2 normalized = val6.normalized;
                        Vector2 perpendicular = Custom.PerpendicularVector(normalized);
                        float num5 = Vector2.Distance(posBetweenTailSegmentPosAndLastPos, val4) / 5f;
                        if (i == 0)
                        {
                            num5 = 0f;
                        }
                        tailSprite.MoveVertice(i * 4, val4 - perpendicular * num4 * num3 + normalized * num5 - camPos);
                        tailSprite.MoveVertice(i * 4 + 1, val4 + perpendicular * num4 * num3 + normalized * num5 - camPos);
                        if (i < self.tail.Length - 1 && i * 4 + 3 < tailSprite.vertices.Length)
                        {
                            tailSprite.MoveVertice(i * 4 + 2, posBetweenTailSegmentPosAndLastPos - perpendicular * self.tail[i].StretchedRad * num3 - normalized * num5 - camPos);
                            tailSprite.MoveVertice(i * 4 + 3, posBetweenTailSegmentPosAndLastPos + perpendicular * self.tail[i].StretchedRad * num3 - normalized * num5 - camPos);
                        }
                        else
                        {
                            tailSprite.MoveVertice(i * 4 + 2, posBetweenTailSegmentPosAndLastPos - camPos);
                        }
                        num4 = self.tail[i].StretchedRad;
                        val4 = posBetweenTailSegmentPosAndLastPos;
                    }
                    return true;
                });

                cursor.Emit(OpCodes.Brtrue, label);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception when matching IL for PlayerGraphics_DrawSpritesTail!");
                Debug.LogException(ex);
                Debug.LogError(il);
                throw;
            }
        }
    }
}