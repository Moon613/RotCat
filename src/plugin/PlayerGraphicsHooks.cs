using BepInEx;
using UnityEngine;
using Noise;
using MoreSlugcats;
using RWCustom;
using System.Security;
using System.Security.Permissions;
using System;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using SlugBase.Features;
using SlugBase;

namespace RotCat
{
    public class PlayerGraphicsHooks {
        public static void RotInitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            //base.Logger.LogDebug("Initiating Sprites");
            orig(self, sLeaser, rCam);
            //base.Logger.LogDebug(sLeaser.sprites.Length);
            RotCat.tenticleStuff.TryGetValue(self.player, out var something);
            if (something.isRot) {
                something.faceAtlas = Futile.atlasManager.LoadAtlas("atlases/RotFace");
                Array.Resize<FSprite>(ref sLeaser.sprites, sLeaser.sprites.Length + something.tentacles.Length + something.decorativeTentacles.Length + something.totalCircleSprites + (something.bodyRotSpriteAmount * 2 /*Multiply by 2 for the X sprites for each one*/));
                something.initialBodyRotSprite = sLeaser.sprites.Length - (something.tentacles.Length + something.decorativeTentacles.Length + something.totalCircleSprites + (something.bodyRotSpriteAmount * 2));
                something.initialCircleSprite = sLeaser.sprites.Length - (something.tentacles.Length + something.decorativeTentacles.Length + something.totalCircleSprites);
                something.initialDecoLegSprite = sLeaser.sprites.Length - (something.tentacles.Length + something.decorativeTentacles.Length);
                something.initialLegSprite = sLeaser.sprites.Length - something.tentacles.Length;
                for (int i = 0; i < sLeaser.sprites.Length-something.initialLegSprite; i++) {
                    sLeaser.sprites[something.initialLegSprite + i] = TriangleMesh.MakeLongMeshAtlased((int)something.segments, false, true);
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
            RotCat.tenticleStuff.TryGetValue(self.player, out var something);
            if (something.isRot) {
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
                
                //Colors all additional leg sprites DLL leg color
                for (int i = something.initialLegSprite; i < sLeaser.sprites.Length; i++)
                {
                    for (int j = 0; j < (sLeaser.sprites[i] as TriangleMesh).verticeColors.Length; j++) {
                        (sLeaser.sprites[i] as TriangleMesh).verticeColors[j] = new Color((float)27/255, (float)11/255, j>70? (float)(33+(4*(j-70)))/255 : (float)33/255);  //Add a Mathf.Lerp here so custom colors are easier later.
                    }
                }
                
                //Colors the decorative tentacles
                for (int i = something.initialDecoLegSprite; i < something.initialLegSprite; i++) {
                    for (int j = 0; j < (sLeaser.sprites[i] as TriangleMesh).verticeColors.Length; j++) {
                        (sLeaser.sprites[i] as TriangleMesh).verticeColors[j] = new Color((float)27/255, (float)11/255, j>=5? (float)(33+(4*(j-5)))/255 : (float)(33+(4*(5-j)))/255);//Need fixing, technically doesn't do the right colors
                    }
                }
                
                int nextTentacleSprite = 0;
                foreach(var tentacle in something.tentacles)
                {
                    Vector2 vector = Vector2.Lerp(tentacle.pList[0].prevPosition, tentacle.pList[0].position, timeStacker);
                    vector += Custom.DirVec(Vector2.Lerp(tentacle.pList[1].prevPosition, tentacle.pList[1].position, timeStacker), vector);
                    float d = 2.3f;//width
                    for (int i = 0; i < tentacle.pList.Length; i++)
                    {
                        Vector2 vector2 = tentacle.pList[i].position;
                        Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
                        //base.Logger.LogDebug(vector + " " + vector2);
                        if (i == 0)
                        {
                            (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4, self.player.mainBodyChunk.pos - a * d - camPos);
                            (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4 + 1, self.player.mainBodyChunk.pos + a * d - camPos);
                        }
                        else{
                        (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4, vector - a * d - camPos);
                        (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + a * d - camPos);
                        }
                        (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - a * d - camPos);
                        (sLeaser.sprites[something.initialLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + a * d - camPos);
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
                            (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4, self.player.mainBodyChunk.pos - a * thickness - camPos);
                            (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4 + 1, self.player.mainBodyChunk.pos + a * thickness - camPos);
                        }
                        else {
                        (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4, vector - a * thickness - camPos);
                        (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + a * thickness - camPos);
                        }
                        (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - a * thickness - camPos);
                        (sLeaser.sprites[something.initialDecoLegSprite + nextTentacleSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + a * thickness - camPos);
                        vector = vector2;
                    }
                    nextTentacleSprite += 1;
                }
                
                //Makes tentacles and circles on them invisible if they are retracted into the scug
                for (int i = something.initialCircleSprite; i < sLeaser.sprites.Length; i++) {
                    if ((something.retractionTimer <= -10f && (i < something.initialDecoLegSprite || i >= something.initialLegSprite)) || Input.GetKey(RotCat.staticOptions.tentMovementAutoEnable.Value)) {
                        sLeaser.sprites[i].color = new Color(sLeaser.sprites[i].color.r, sLeaser.sprites[i].color.g, sLeaser.sprites[i].color.b, Mathf.Lerp(0f,1f,something.retractionTimer/10));
                    }
                }
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
                        sLeaser.sprites[i].color = new Color((float)27/255, (float)11/255, (float)253/255);
                    }
                    //sLeaser.sprites[i].color = something.rotEyeColor;
                }
            }
        }
    
        public static void RotAddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer) {
            orig(self, sLeaser, rCam, newContainer);
            RotCat.tenticleStuff.TryGetValue(self.player, out var something);
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
                    for (int i = something.initialLegSprite; i < sLeaser.sprites.Length; i++) {
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