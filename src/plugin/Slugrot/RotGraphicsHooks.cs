using UnityEngine;
using RWCustom;
using System;
using SlugBase;
using System.Linq;

namespace Chimeric
{
    public static class RotGraphicsHooks {
        public static void RotInitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            //base.Logger.LogDebug("Initiating Sprites");
            orig(self, sLeaser, rCam);
            //base.Logger.LogDebug(sLeaser.sprites.Length);
            Plugin.tenticleStuff.TryGetValue(self.player, out var something);
            if (something.isRot) {
                something.rotEyeColor = new Color((float)27/255, (float)11/255, (float)253/255);
                if (self.useJollyColor) {
                    something.rotEyeColor = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
                }
                else if (!PlayerGraphics.CustomColorsEnabled()) {
                    SlugBaseCharacter.TryGet(SlugBaseCharacter.Registry.Keys.Where(name => name.value == "slugrot").ToList()[0], out SlugBaseCharacter chara);
                    SlugBase.Features.PlayerFeatures.CustomColors.TryGet(chara, out SlugBase.DataTypes.ColorSlot[] colors);
                    something.rotEyeColor = colors[2].GetColor(self.player.playerState.playerNumber);
                }
                else if (PlayerGraphics.CustomColorsEnabled()) {
                    something.rotEyeColor = PlayerGraphics.CustomColorSafety(2);
                }
                something.faceAtlas = Futile.atlasManager.LoadAtlas("atlases/RotFace");
                Array.Resize<FSprite>(ref sLeaser.sprites, sLeaser.sprites.Length + something.tentacles.Length + something.decorativeTentacles.Length + something.totalCircleSprites + (something.bodyRotSpriteAmount * 2 /*Multiply by 2 for the X sprites for each one*/));
                something.initialBodyRotSprite = sLeaser.sprites.Length - (something.tentacles.Length + something.decorativeTentacles.Length + something.totalCircleSprites + (something.bodyRotSpriteAmount * 2));
                something.initialCircleSprite = sLeaser.sprites.Length - (something.tentacles.Length + something.decorativeTentacles.Length + something.totalCircleSprites);
                something.initialDecoLegSprite = sLeaser.sprites.Length - (something.tentacles.Length + something.decorativeTentacles.Length);
                something.initialLegSprite = sLeaser.sprites.Length - something.tentacles.Length;
                something.endOfsLeaser = sLeaser.sprites.Length;
                for (int i = 0; i < something.endOfsLeaser-something.initialLegSprite; i++) {
                    sLeaser.sprites[something.initialLegSprite + i] = TriangleMesh.MakeLongMeshAtlased(something.segments, false, true);
                }
                for (int i = 0; i < something.initialLegSprite-something.initialDecoLegSprite; i++) {
                    sLeaser.sprites[something.initialDecoLegSprite + i] = TriangleMesh.MakeLongMeshAtlased((int)something.decorationSegments, false, true);
                }
                for (int i = 0; i < something.initialDecoLegSprite-something.initialCircleSprite; i++) {
                    int length = something.initialDecoLegSprite-something.initialCircleSprite;
                    int posInTentList = i<(length/4)?  0:i<(length/2)?  1:i<(3*length/4)?  2:3;
                    int correctPos = i<(length/4)?  i:i<(length/2)?  i-(length/4):i<(3*length/4)?  i-(length/2):i-(3*length/4);//Wildly assumes this will always work
                    sLeaser.sprites[something.initialCircleSprite + i] = new FSprite("Circle20", false);//Maybe make a list of the sizes I want bumps to be for use here
                    //base.Logger.LogDebug("Circle Sprite Editing");
                    //base.Logger.LogDebug(length);
                    //base.Logger.LogDebug(i);
                    //base.Logger.LogDebug(posInTentList);
                    //base.Logger.LogDebug(correctPos);
                    sLeaser.sprites[something.initialCircleSprite + i].scale = something.tentacles[posInTentList].cList[correctPos].scale;
                    sLeaser.sprites[something.initialCircleSprite + i].scaleX = something.tentacles[posInTentList].cList[correctPos].scaleX;
                    sLeaser.sprites[something.initialCircleSprite + i].scaleY = something.tentacles[posInTentList].cList[correctPos].scaleY;
                    //sLeaser.sprites[something.initialCircleSprite + i].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
                }
                for (int i = 0; i < something.initialCircleSprite-something.initialBodyRotSprite; i++) {
                    if (i < (something.initialCircleSprite-something.initialBodyRotSprite)/2) {
                        sLeaser.sprites[something.initialBodyRotSprite + i] = new FSprite("roteye", false);
                    }
                    else {
                        sLeaser.sprites[something.initialBodyRotSprite + i] = new FSprite("roteyeeye", false);
                    }
                }
                something.rList = new BodyRot[something.bodyRotSpriteAmount];    //Operates much the same as the previous code for Circles, but takes different parameters and applies them to the rot bits on the body
                something.rList[0] = new BodyRot(sLeaser.sprites[3], sLeaser.sprites[1], new Vector2(-5f, 7f), 0.12f);
                something.rList[1] = new BodyRot(sLeaser.sprites[3], sLeaser.sprites[1], new Vector2(5.25f, 7.4f), 0.13f);
                something.rList[2] = new BodyRot(sLeaser.sprites[3], sLeaser.sprites[1], new Vector2(0.1f, 8.3f), 0.14f);
                something.rList[3] = new BodyRot(sLeaser.sprites[3], sLeaser.sprites[1], new Vector2(-3.2f, 10.2f), 0.09f);
                something.rList[4] = new BodyRot(sLeaser.sprites[3], sLeaser.sprites[1], new Vector2(-0.5f, 12.8f), 0.1f);
                something.rList[5] = new BodyRot(sLeaser.sprites[3], sLeaser.sprites[1], new Vector2(3.9f, 10.8f), 0.09f);
                something.rList[6] = new BodyRot(sLeaser.sprites[3], sLeaser.sprites[1], new Vector2(-4.8f, 11.8f), 0.11f);
                something.rList[7] = new BodyRot(sLeaser.sprites[3], sLeaser.sprites[1], new Vector2(4.2f, 17f), 0.095f);
                self.AddToContainer(sLeaser, rCam, null);
            }
        }
        public static void RotDrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            Plugin.tenticleStuff.TryGetValue(self.player, out var something);
            if (something.isRot) {
                if (Plugin.vignetteEffect != null && self.player.room != null && ChimericOptions.enableVignette.Value && self.player.room.game.IsStorySession && self.player.room.game.StoryCharacter.value == "slugrot") {
                    Functions.UpdateVignette(self.player.room.game.rainWorld, self.player, Plugin.vignetteEffect.color, camPos);
                }
                //base.Logger.LogDebug(self.player.flipDirection);
                Functions.DrawFace(something, sLeaser, sLeaser.sprites[9]?.element?.name);
                //sLeaser.sprites[something.initialCircleSprite-1].SetPosition(self.player.mainBodyChunk.pos + ((self.player.mainBodyChunk.pos-self.player.bodyChunks[1].pos).normalized * -6) - rCam.pos);
                //sLeaser.sprites[something.initialCircleSprite-1].color = Color.white;
                FSprite[] tentacle1Circles = new FSprite[0];
                FSprite[] tentacle2Circles = new FSprite[0];
                FSprite[] tentacle3Circles = new FSprite[0];
                FSprite[] tentacle4Circles = new FSprite[0];
                var length = something.initialDecoLegSprite - something.initialCircleSprite;
                //base.Logger.LogDebug(length);
                for (float i = 0; i < length; i+=1) {   //Assigns the circle sprites into groups based on which leg they are connected to, so looping through them later is simpler. At least, this way made sense when I first did it
                    if (i+1 <= length/4) {
                        Array.Resize<FSprite>(ref tentacle1Circles, tentacle1Circles.Length+1);
                        tentacle1Circles[tentacle1Circles.Length-1] = sLeaser.sprites[(int)i+something.initialCircleSprite];
                    }
                    else if (i+1 <= length/2) {
                        Array.Resize<FSprite>(ref tentacle2Circles, tentacle2Circles.Length+1);
                        tentacle2Circles[tentacle2Circles.Length-1] = sLeaser.sprites[(int)i+something.initialCircleSprite];
                    }
                    else if (i+1 <= length*3/4) {
                        Array.Resize<FSprite>(ref tentacle3Circles, tentacle3Circles.Length+1);
                        tentacle3Circles[tentacle3Circles.Length-1] = sLeaser.sprites[(int)i+something.initialCircleSprite];
                    }
                    else if (i+1 <= length) {
                        Array.Resize<FSprite>(ref tentacle4Circles, tentacle4Circles.Length+1);
                        tentacle4Circles[tentacle4Circles.Length-1] = sLeaser.sprites[(int)i+something.initialCircleSprite];
                    }
                }
                Functions.DrawTentacleCircles(something, camPos, tentacle1Circles, tentacle2Circles, tentacle3Circles, tentacle4Circles);
                
                //Colors all additional leg sprites DLL leg color, or the custom color chosen
                Color initialColor = sLeaser.sprites[0].color;
                if (!rCam.room.game.IsArenaSession && PlayerGraphics.CustomColorsEnabled()) {
                    initialColor = PlayerGraphics.CustomColorSafety(0);
                }

                for (int i = something.initialLegSprite; i < something.endOfsLeaser; i++)
                {
                    for (int j = 0; j < (sLeaser.sprites[i] as TriangleMesh)?.verticeColors.Length; j++) {
                        if (j <= 70 && sLeaser.sprites[i] is TriangleMesh triMesh) {
                            triMesh.verticeColors[j] = initialColor;
                        }
                        else if (j > 70 && sLeaser.sprites[i] is TriangleMesh triMesh1) {
                            /*if (r > g && r > b) {
                                r = (float)(33+(4*(j-70)))/255;
                            }
                            else if (g > r && g > b) {
                                g = (float)(33+(4*(j-70)))/255;
                            }
                            else if (b > r && b > g) {
                                b = (float)(33+(4*(j-70)))/255;
                            }
                            Mathf.Clamp(r, 0f, 1f);
                            Mathf.Clamp(g, 0f, 1f);
                            Mathf.Clamp(b, 0f, 1f);*/
                            triMesh1.verticeColors[j] = Color.Lerp(initialColor, something.rotEyeColor, Mathf.Pow(j-70f,1.5f)/Mathf.Pow(30f,1.5f));//new Color(r, g, b);
                        }
                    }
                }
                
                //Colors the decorative tentacles
                for (int i = something.initialDecoLegSprite; i < something.initialLegSprite; i++) {
                    for (int j = 0; j < (sLeaser.sprites[i] as TriangleMesh)?.verticeColors.Length; j++) {
                        if (j <= 20 && sLeaser.sprites[i] is TriangleMesh triMesh) {
                            triMesh.verticeColors[j] = Color.Lerp(initialColor, something.rotEyeColor, Mathf.Pow(j,1.5f)/Mathf.Pow(20f,1.5f)); //new Color((float)27/255, (float)11/255, j>=5? (float)(33+(4*(j-5)))/255 : (float)(33+(4*(5-j)))/255);//Need fixing, technically doesn't do the right colors
                        }
                        else if (sLeaser.sprites[i] is TriangleMesh triMesh1) {
                            triMesh1.verticeColors[j] = Color.Lerp(something.rotEyeColor, initialColor, Mathf.Pow(j-20f,1.5f)/Mathf.Pow(20f,1.5f));
                        }
                    }
                }
                
                int nextTentacleSprite = 0;
                foreach(var tentacle in something.tentacles)
                {
                    Vector2 vector = Vector2.Lerp(tentacle.pList[0].prevPosition, tentacle.pList[0].position, timeStacker);
                    vector += Custom.DirVec(Vector2.Lerp(tentacle.pList[1].prevPosition, tentacle.pList[1].position, timeStacker), vector);
                    float width = 2.35f;//width
                    for (int i = 0; i < tentacle.pList.Length; i++)
                    {
                        Vector2 vector2 = tentacle.pList[i].position;
                        Vector2 perpendicularVector = Custom.PerpendicularVector((vector - vector2).normalized);
                        //base.Logger.LogDebug(vector + " " + vector2);
                        if (i == 0)
                        {
                            (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4, self.player.mainBodyChunk.pos - perpendicularVector * width - camPos);
                            (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4 + 1, self.player.mainBodyChunk.pos + perpendicularVector * width - camPos);
                        }
                        else {
                            (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4, vector - perpendicularVector * width - camPos);
                            (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4 + 1, vector + perpendicularVector * width - camPos);
                        }
                        (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4 + 2, vector2 - perpendicularVector * width - camPos);
                        (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4 + 3, vector2 + perpendicularVector * width - camPos);
                        vector = vector2;
                    }
                    nextTentacleSprite += 1;
                }
                nextTentacleSprite = 0;
                foreach(var tentacle in something.decorativeTentacles)
                {
                    Vector2 vector = Vector2.Lerp(tentacle.pList[0].prevPosition, tentacle.pList[0].position, timeStacker);
                    vector += Custom.DirVec(Vector2.Lerp(tentacle.pList[1].prevPosition, tentacle.pList[1].position, timeStacker), vector);
                    const float thickness = 2f;
                    for (int i = 0; i < tentacle.pList.Length; i++)
                    {
                        Vector2 vector2 = tentacle.pList[i].position;
                        Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
                        //base.Logger.LogDebug(vector + " " + vector2);
                        if (i == 0) {
                            (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4, self.player.mainBodyChunk.pos - a * thickness - camPos);
                            (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4 + 1, self.player.mainBodyChunk.pos + a * thickness - camPos);
                        }
                        else {
                        (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4, vector - a * thickness - camPos);
                        (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4 + 1, vector + a * thickness - camPos);
                        }
                        (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4 + 2, vector2 - a * thickness - camPos);
                        (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh)?.MoveVertice(i * 4 + 3, vector2 + a * thickness - camPos);
                        vector = vector2;
                    }
                    nextTentacleSprite += 1;
                }
                
                //Makes tentacles and circles on them invisible if they are retracted into the scug
                /*for (int i = something.initialCircleSprite; i < something.endOfsLeaser; i++) {
                    if ((something.retractionTimer <= -10f && (i < something.initialDecoLegSprite || i >= something.initialLegSprite))) {
                        sLeaser.sprites[i].color = new Color(sLeaser.sprites[i].color.r, sLeaser.sprites[i].color.g, sLeaser.sprites[i].color.b, Mathf.Lerp(0f,1f,something.retractionTimer/10));
                    }
                }*/
                //Generates the Body Rot Bulbs, sets the scale, and colors them
                for (int i = something.initialBodyRotSprite; i < something.initialCircleSprite; i++) {  //k is used in order to go over the rList twice, since it holds the data for the bulbs, and the bulb and X sprites are separate in order to allow for custom colors for both
                    int halfLength = ((something.initialCircleSprite - something.initialBodyRotSprite)/2) + something.initialBodyRotSprite;
                    int k = i - something.initialBodyRotSprite;
                    k -= i<halfLength? 0:halfLength-something.initialBodyRotSprite;
                    //base.Logger.LogDebug("The current i & k indexies are:");
                    //base.Logger.LogDebug(i);
                    //base.Logger.LogDebug(k);
                    Vector2 vector = something.rList[k].chunk2.GetPosition() - something.rList[k].chunk1.GetPosition();
                    Vector2 vecNormalized = vector.normalized;
                    Vector2 perpendicularVector = Custom.PerpendicularVector(vector);
                    sLeaser.sprites[i].SetPosition(something.rList[k].chunk1.GetPosition() + (vecNormalized * something.rList[k].offset.y) + (perpendicularVector * something.rList[k].offset.x));
                    sLeaser.sprites[i].scale = something.rList[k].scale;
                    if (k < i - something.initialBodyRotSprite) {
                        sLeaser.sprites[i].color = something.rotEyeColor;
                    }
                    //sLeaser.sprites[i].color = something.rotEyeColor;
                }
            }
        }
        public static void RotAddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer) {
            orig(self, sLeaser, rCam, newContainer);
            Plugin.tenticleStuff.TryGetValue(self.player, out var something);
            if (something.isRot) {
                //base.Logger.LogDebug("sLeaser length");
                //base.Logger.LogDebug(sLeaser.sprites.Length);
                if (sLeaser.sprites.Length > 13) {
                    FContainer foregroundContainer = rCam.ReturnFContainer("Foreground");
                    FContainer MidContainer = rCam.ReturnFContainer("Midground");
                    for (int i = something.initialBodyRotSprite; i < something.initialCircleSprite; i++) {
                        FSprite spriteLol = sLeaser.sprites[i];
                        foregroundContainer.RemoveChild(spriteLol);
                        MidContainer.AddChild(spriteLol);
                        spriteLol.MoveBehindOtherNode(sLeaser.sprites[something.initialDecoLegSprite]);
                    }
                    for (int i = something.initialCircleSprite; i < something.initialDecoLegSprite; i++) {
                        FSprite spriteLol = sLeaser.sprites[i];
                        foregroundContainer.RemoveChild(spriteLol);
                        MidContainer.AddChild(spriteLol);
                        spriteLol.MoveBehindOtherNode(sLeaser.sprites[0]);
                        //spriteLol.MoveInFrontOfOtherNode(sLeaser.sprites[something.initialLegSprite+3]);
                    }
                    for (int i = something.initialDecoLegSprite; i < something.initialLegSprite; i++) {
                        FSprite spriteLol = sLeaser.sprites[i];
                        foregroundContainer.RemoveChild(spriteLol);
                        MidContainer.AddChild(spriteLol);
                        spriteLol.MoveBehindOtherNode(sLeaser.sprites[1]);
                    }
                    for (int i = something.initialLegSprite; i < something.endOfsLeaser; i++) {
                        FSprite spriteLol = sLeaser.sprites[i];
                        foregroundContainer.RemoveChild(spriteLol);
                        MidContainer.AddChild(spriteLol);
                        spriteLol.MoveBehindOtherNode(sLeaser.sprites[something.initialCircleSprite]);
                    }
                }
            }
        }
    }
}