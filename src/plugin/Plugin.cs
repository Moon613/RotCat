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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RotCat;

[BepInPlugin("rotcat", "RotCat", "0.0.2")]
public class RotCat : BaseUnityPlugin
{
    bool init = false;
    public static ConditionalWeakTable<Player, PlayerEx> tenticleStuff = new();
    public RotCatOptions Options;
    public static RotCatOptions staticOptions;
    bool configWorking = false;
    public SlugcatStats.Name Name;
    public SlugBaseCharacter Character;
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += Init;
        
        On.Player.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            tenticleStuff.Add(self, new PlayerEx());
            tenticleStuff.TryGetValue(self, out var something);
            if (self.slugcatStats.name.ToString() == "slugrot") {
                something.isRot = true;
            }
            if (something.isRot) {
                var playerNumber = !self.room.game.IsArenaSession && self.playerState.playerNumber == 0 ? -1 : self.playerState.playerNumber;
                if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "slugrot", true, out var extEnum))
                {
                    Name = extEnum as SlugcatStats.Name;
                }
                SlugBaseCharacter.TryGet(Name, out Character);
                if (Character.Features.TryGet(PlayerFeatures.CustomColors, out var customColors)) {
                    something.rotEyeColor = customColors[2].GetColor(playerNumber);
                }
                self.abstractCreature.tentacleImmune = true;
                something.totalCircleSprites = something.circleAmmount * 4;
                something.tentacleOne = new Line();
                something.tentacleTwo = new Line();
                something.tentacleThree = new Line();
                something.tentacleFour = new Line();
                something.tentacles[0] = something.tentacleOne;
                something.tentacles[1] = something.tentacleTwo;
                something.tentacles[2] = something.tentacleThree;
                something.tentacles[3] = something.tentacleFour;
                something.decorativeTentacles[0] = new Line();
                something.decorativeTentacles[1] = new Line();
                something.randomPosOffest = new Vector2[something.decorativeTentacles.Length*2];
                for (int i = 0; i < something.randomPosOffest.Length; i++) {
                    if (i%2==0) {
                        something.randomPosOffest[i] = new Vector2(Random.Range(0,-8f),Random.Range(-4f,1f));
                        //something.randomPosOffest[i] = new Vector2(-8f,1f);
                    }
                    else {
                        something.randomPosOffest[i] = new Vector2(Random.Range(8f,0),Random.Range(-15f,-4f));
                        //something.randomPosOffest[i] = new Vector2(8f, -15f);
                    }
                }
                //Adds points and sticks to the grabby tentacles
                foreach (var tentacle in something.tentacles) {
                    tentacle.pList = new Point[something.segments];
                    for (int i = 0; i < something.segments; i++) {
                        tentacle.pList[i] = new Point(new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-1-i), i==0?true:false);
                        tentacle.pList[i].prevPosition = new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-i);
                    }
                    tentacle.sList = new Stick[something.segments-1];
                    for (int i = 0; i < tentacle.pList.Length-1; i++) {
                        tentacle.sList[i] = new Stick(tentacle.pList[i], tentacle.pList[i+1], 9.25f);
                    }
                    tentacle.cList = new Circle[something.circleAmmount];//Hard-coded bumps here      Make sure to actually change the index they're being put in if copying
                    //base.Logger.LogDebug(tentacle.pList.Length);
                    tentacle.cList[0] = new Circle(tentacle.pList[8], tentacle.pList[9], new Vector2(3f,10f), true, false, 0.4f);
                    tentacle.cList[1] = new Circle(tentacle.pList[19], tentacle.pList[20], new Vector2(0f,6f), true, false, 0.4f);
                    tentacle.cList[2] = new Circle(tentacle.pList[15], tentacle.pList[16], new Vector2(-1.15f, 3.43f), true, false, 1f, scaleX:0.386f, scaleY:0.54f);
                    tentacle.cList[3] = new Circle(tentacle.pList[8], tentacle.pList[9], new Vector2(3f,10f), false, false, 0.3f);
                    tentacle.cList[4] = new Circle(tentacle.pList[19], tentacle.pList[20], new Vector2(0f,6f), false, false, 0.3f);
                    tentacle.cList[5] = new Circle(tentacle.pList[22], tentacle.pList[23], new Vector2(1,5f), false, false, 0.4f, scaleY:0.9f);
                    tentacle.cList[6] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-4f,4f), false, false, 0.55f);
                    tentacle.cList[7] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(5f,12f), false, false, 0.55f);
                    tentacle.cList[8] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-2f,12.3f), false, false, 0.475f);
                    tentacle.cList[9] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(3f,4f), false, false, 0.25f);
                    tentacle.cList[10] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(0.5f,9.1f), false, false, 0.445f);
                    tentacle.cList[11] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(2.25f,-0.6f), false, false, 0.389f);
                    tentacle.cList[12] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-0.9f,-3f), false, false, 0.385f);
                    tentacle.cList[13] = new Circle(tentacle.pList[21], tentacle.pList[22], new Vector2(-2.15f,3.35f), false, false, 1f, scaleX:0.13f, scaleY:0.36f, lightgrayscale: true);
                    tentacle.cList[14] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-0.5f,9.3f), false, true, 0.455f);
                    tentacle.cList[15] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-4f,4f), false, true, 0.35f);
                    tentacle.cList[16] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(5f,12f), false, true, 0.35f);
                    tentacle.cList[17] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(2.25f,-0.6f), false, true, 0.229f);
                    tentacle.cList[18] = new Circle(tentacle.pList[21], tentacle.pList[22], new Vector2(-0.95f,3.1f), false, true, 1f, scaleX:0.23f, scaleY:0.66f);

                }
                //Adds points and sticks to the decorative tentacles
                foreach (var tentacle in something.decorativeTentacles) {
                    tentacle.pList = new Point[something.decorationSegments];
                    for (int i = 0; i < tentacle.pList.Length; i++) {
                        tentacle.pList[i] = new Point(new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-1-i), i==0||i==(tentacle.pList.Length-1)?true:false);
                    }
                    tentacle.sList = new Stick[tentacle.pList.Length-1];
                    for (int i = 0; i < tentacle.pList.Length-1; i++) {
                        tentacle.sList[i] = new Stick(tentacle.pList[i], tentacle.pList[i+1], 5f);
                    }
                    tentacle.decoPushDirection = Vector2.right * Random.Range(-0.7f,0.7f) * 0;  //Curently unused because it doesn't look the best. To enable change the 0 to something.decorativeTentacles[i].decoPushDirection *I think
                }
                self.slugcatStats.runspeedFac = 0.65f;
                self.slugcatStats.corridorClimbSpeedFac = 1.1f;
            }
        };

        On.Player.NewRoom += (orig, self, newRoom) =>
        {
            orig(self, newRoom);
            tenticleStuff.TryGetValue(self, out var something);
            if (something.isRot) {
                for (int i = 0; i < something.tentacles.Length; i++) {
                    for (int j = something.tentacles[i].pList.Length-1; j >= 0; j--) {
                        something.tentacles[i].pList[j].position = self.mainBodyChunk.pos - new Vector2(0,j);
                        something.tentacles[i].pList[j].prevPosition = self.mainBodyChunk.pos - new Vector2(0,j);
                    }
                }
            }
        };

        /*On.Player.EatMeatUpdate += (orig, self, graspIndex) => {
            orig(self, graspIndex);
            self.grasps[graspIndex].grabbedChunk.owner.Destroy();
        };*/

        /*On.DaddyTentacle.Update += (orig, self) => {
            orig(self);
            for(int i = 0; i < self?.room?.PlayersInRoom?.Count; i++) {
                tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var something);
                if (something.isRot) {
                    if(self?.grabChunk?.owner?.GetType() == typeof(Player)) {
                        self.grabChunk = null;
                    }
                }
            }
        };*/

        On.DaddyLongLegs.Update += (orig, self, eu) => {
            for(int i = 0; i < self?.room?.PlayersInRoom?.Count; i++) {
                tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var something);
                if (something.isRot && self.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs) {
                    self.room.PlayersInRoom[i].abstractCreature.tentacleImmune = false;
                }
            }
            orig(self, eu);
            for(int i = 0; i < self?.room?.PlayersInRoom?.Count; i++) {
                tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var something);
                if (something.isRot && self.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs) {
                    for(int j = 0; j < self?.grasps?.Length; j++) {
                        if (self?.grasps[j] != null && self?.grasps[j].grabbedChunk.owner == self.room.PlayersInRoom[i]) {
                            self.grasps[j] = null;
                        }
                    }
                    self.room.PlayersInRoom[i].abstractCreature.tentacleImmune = true;
                }
            }
        };

        On.DaddyCorruption.LittleLeg.Update += (orig, self, eu) => {
            orig(self, eu);
            for(int i = 0; i < self?.room?.PlayersInRoom?.Count; i++) {
                tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var something);
                if (something.isRot) {
                    base.Logger.LogDebug(self?.grabChunk?.owner?.GetType());
                    if(self?.grabChunk?.owner?.GetType() == typeof(Player)) {
                        self.grabChunk = null;
                    }
                }
            }
        };

        On.Player.SpitOutOfShortCut += (orig, self, pos, newRoom, spitOutAllStacks) => {
            orig(self, pos, newRoom, spitOutAllStacks);
            tenticleStuff.TryGetValue(self, out var something);
            if (something.isRot) {
                for (int i = 0; i < something.tentacles.Length; i++) {
                    for (int j = something.tentacles[i].pList.Length-1; j >= 0; j--) {
                        something.tentacles[i].pList[j].position = self.mainBodyChunk.pos - new Vector2(0,j);
                        something.tentacles[i].pList[j].prevPosition = self.mainBodyChunk.pos - new Vector2(0,j);
                    }
                }
            }
        };

        On.Player.Update += (orig, self, eu) =>
        {
            orig(self, eu);
            tenticleStuff.TryGetValue(self, out var something);
            if (something.isRot && self != null) {
                self.scavengerImmunity = 9999;  //Might want to add a system that calculates this based on rep, since you should be able to become friends if you want
                something.overrideControls = Input.GetKey(staticOptions.tentMovementLeft.Value) || Input.GetKey(staticOptions.tentMovementRight.Value) || Input.GetKey(staticOptions.tentMovementDown.Value) || Input.GetKey(staticOptions.tentMovementUp.Value);
                if (something.grabWallCooldown > 0) { //Doesn't do anything right now, need to get it to play nice with the logic first
                    something.grabWallCooldown -= 0.5f;
                }
                //This whole bit controls the lengthening of the tentacles when the player is standing still, or moving too much
                Functions.TentacleRetraction(((Custom.Dist(something.previousPosition, self.mainBodyChunk.pos) > 1f && Custom.Dist(something.previousPosition, self.mainBodyChunk.pos) < 3.5f) || Input.GetKey(staticOptions.tentMovementAutoEnable.Value) || Input.GetKey(staticOptions.tentMovementEnable.Value)) && something.retractionTimer < 60, something.retractionTimer > -20 && !Input.GetKey(staticOptions.tentMovementEnable.Value), something.retractionTimer <= 40 && something.retractionTimer > 0, self, something);

                //Physics for the individual points and Corruption circles
                int numIterations = 10;
                foreach (var tentacle in something.tentacles) {
                    foreach (Point p in tentacle.pList)
                    {
                        if (Array.IndexOf(tentacle.pList, p) == tentacle.pList.Length-1 && ((Input.GetKey(staticOptions.tentMovementEnable.Value) || Input.GetKey(staticOptions.tentMovementAutoEnable.Value)) && something.targetPos[Array.IndexOf(something.tentacles, tentacle)].foundSurface && ((Array.IndexOf(something.tentacles, tentacle) == 0) || something.automateMovement))) {  //If it is the very last point in the list, the tentacle tip
                            p.locked = true;
                        }
                        else {
                            p.locked = false;
                        }
                        if (!p.locked && self.room != null) {
                            Vector2 positionBeforeUpdate = p.position;
                            p.position += (p.position - p.prevPosition) * Random.Range(0.9f,1.1f);
                            p.position += Vector2.down * self.room.gravity * Random.Range(0.15f,0.3f);
                            p.prevPosition = positionBeforeUpdate;
                        }
                        if (Array.IndexOf(tentacle.pList, p) == 0) {
                            p.position = self.mainBodyChunk.pos;
                        }
                    }
                
                    //base.Logger.LogDebug("Offset here");
                    foreach (Circle spot in tentacle.cList) {
                        Vector2 direction = spot.pointB.position - spot.pointA.position;
                        Vector2 dirNormalized = direction.normalized;
                        Vector2 perpendicularVector = Custom.PerpendicularVector(direction);
                        spot.position = spot.pointA.position + (dirNormalized * spot.offset.y) + (perpendicularVector * spot.offset.x);
                    }
                }
                //The same, but for the decorative tentacles which have slightly different parameters to follow.
                foreach (var tentacle in something.decorativeTentacles) {
                    int pointer = Array.IndexOf(something.decorativeTentacles, tentacle);
                    Vector2 direction = self.mainBodyChunk.pos - self.bodyChunks[1].pos;
                    Vector2 dirNormalized = direction.normalized;
                    Vector2 perpendicularVector = Custom.PerpendicularVector(direction);
                    foreach (Point p in tentacle.pList)
                    {
                        if (!p.locked && self.room != null) {
                            Vector2 positionBeforeUpdate = p.position;
                            p.position += (p.position - p.prevPosition) * Random.Range(0.9f,1.1f);
                            p.position += (Vector2.down * self.room.gravity * Random.Range(0.9f,1.1f));
                            p.prevPosition = positionBeforeUpdate;
                        }
                        if (Array.IndexOf(tentacle.pList, p) == 0) {
                            p.locked = true;
                            p.position = self.mainBodyChunk.pos + (dirNormalized * something.randomPosOffest[pointer*2].y) + (perpendicularVector * something.randomPosOffest[pointer*2].x);
                        }
                        if (Array.IndexOf(tentacle.pList, p) == tentacle.pList.Length-1) {
                            p.locked = true;
                            p.position = self.mainBodyChunk.pos + (dirNormalized * something.randomPosOffest[(pointer*2)+1].y) + (perpendicularVector * something.randomPosOffest[(pointer*2)+1].x);
                        }
                    }
                }

                //Physics for the sticks of all tentacles, which affects the points
                for (int i = 0; i < numIterations; i++) {
                    Line[][] totalTentacles = {something.tentacles, something.decorativeTentacles};
                    Functions.StickCalculations(totalTentacles);
                }

                if (Input.GetKey(staticOptions.tentMovementEnable.Value) || Input.GetKey(staticOptions.tentMovementAutoEnable.Value)) {//Something is bugged when first activating this, reminder to figure out why. Needs fixing
                    Functions.PrimTentAndPlayerMovement(something, self, staticOptions);
                    float startPos = Functions.FindPos(something.overrideControls, self, staticOptions);    //Finds the position around the player to start, based on Sine and Cosine intervals of pi/4
                    Functions.TentaclesFindPositionToGoTo(something, self, startPos);
                    Functions.MoveTentacleToPosition(something, self);
                }
                else {
                    something.automateMovement = false;
                }
            }
        };

        On.PlayerGraphics.ctor += (orig, self, ow) =>
        {
            orig(self, ow);
        };
        
        On.PlayerGraphics.ApplyPalette += (orig, self, sLeaser, rCam, palette) => {
            orig(self, sLeaser, rCam, palette);
        };

        On.PlayerGraphics.AddToContainer += (orig, self, sLeaser, rCam, newContainer) => {
            orig(self, sLeaser, rCam, newContainer);
            tenticleStuff.TryGetValue(self.player, out var something);
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
        };

        On.PlayerGraphics.InitiateSprites += (orig, self, sLeaser, rCam) =>
        {
            //base.Logger.LogDebug("Initiating Sprites");
            orig(self, sLeaser, rCam);
            //base.Logger.LogDebug(sLeaser.sprites.Length);
            tenticleStuff.TryGetValue(self.player, out var something);
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
        };
        
        On.PlayerGraphics.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            tenticleStuff.TryGetValue(self.player, out var something);
            if (something.isRot) {
                string name = sLeaser.sprites[9]?.element?.name;
                //base.Logger.LogDebug(self.player.flipDirection);
                if (name != null && name.StartsWith("Face") && something.faceAtlas._elementsByName.TryGetValue("Rot" + name, out var element)) {
                    sLeaser.sprites[9].element = element;
                    //base.Logger.LogDebug(element.name);
                    //base.Logger.LogDebug(sLeaser.sprites[9].scaleX);
                    if(sLeaser.sprites[9].scaleX < 0) {
                        if(element.name == "RotFaceA0" || element.name == "RotFaceA8" || element.name == "RotFaceB0" || element.name == "RotFaceB8" || element.name == "RotFaceStunned") {
                            sLeaser.sprites[9].scaleX = 1f;
                        }
                        else if(element.name.StartsWith("RotFaceA")) {
                            char num = element.name[8];
                            //base.Logger.LogDebug(element.name.Substring(0,7));
                            //base.Logger.LogDebug(num);
                            sLeaser.sprites[9].element = something.faceAtlas._elementsByName[element.name.Substring(0,7)+"E"+num];
                            sLeaser.sprites[9].scaleX = 1f;
                        }
                        else if(element.name.StartsWith("RotFaceB")) {
                            char num = element.name[8];
                            //base.Logger.LogDebug(element.name.Substring(0,7));
                            //base.Logger.LogDebug(num);
                            sLeaser.sprites[9].element = something.faceAtlas._elementsByName[element.name.Substring(0,7)+"F"+num];
                            sLeaser.sprites[9].scaleX = 1f;
                        }
                    }
                }
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
                
                //Set the circle positions on the tentacles and color them
                for (int i = 0; i < something.tentacles.Length; i++) {
                    for (int j = 0; j < something.tentacles[i].cList.Length; j++) {
                        //base.Logger.LogDebug("Drawsprites");
                        //base.Logger.LogDebug(something.tentacles[i].cList[j].position);
                        if (i == 0) {
                            Vector2 vector = (something.tentacles[i].cList[j].pointA.position-something.tentacles[i].cList[j].pointB.position).normalized;
                            bool rotationSide = vector.x < 0;
                            tentacle1Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                            if (something.tentacles[i].cList[j].brightBackground) {
                                tentacle1Circles[j].color = new Color((float)27/255, (float)11/255, (float)253/255);
                            }
                            else {
                                if (something.tentacles[i].cList[j].darkBackground) {
                                    tentacle1Circles[j].color = new Color((float)27/255, (float)11/255, (float)55/255);
                                }
                                else {
                                    tentacle1Circles[j].color = new Color((float)27/255, (float)11/255, (float)153/255);
                                }
                            }
                            tentacle1Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f) + something.tentacles[i].cList[j].rotation;
                            if (something.tentacles[i].cList[j].grayscale) {
                                tentacle1Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                            }
                        }
                        if (i == 1) {
                            Vector2 vector = (something.tentacles[i].cList[j].pointA.position-something.tentacles[i].cList[j].pointB.position).normalized;
                            bool rotationSide = vector.x < 0;
                            tentacle2Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                            //self.player.room.AddObject(new Spark(tentacle2Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                            if (something.tentacles[i].cList[j].brightBackground) {
                                tentacle2Circles[j].color = new Color((float)27/255, (float)11/255, (float)253/255);
                            }
                            else {
                                if (something.tentacles[i].cList[j].darkBackground) {
                                    tentacle2Circles[j].color = new Color((float)27/255, (float)11/255, (float)55/255);
                                }
                                else {
                                    tentacle2Circles[j].color = new Color((float)27/255, (float)11/255, (float)153/255);
                                }
                            }
                            tentacle2Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                            if (something.tentacles[i].cList[j].grayscale) {
                                tentacle2Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                            }
                        }
                        if (i == 2) {
                            Vector2 vector = (something.tentacles[i].cList[j].pointA.position-something.tentacles[i].cList[j].pointB.position).normalized;
                            bool rotationSide = vector.x < 0;
                            tentacle3Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                            //self.player.room.AddObject(new Spark(tentacle3Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                            if (something.tentacles[i].cList[j].brightBackground) {
                                tentacle3Circles[j].color = new Color((float)27/255, (float)11/255, (float)253/255);
                            }
                            else {
                                if (something.tentacles[i].cList[j].darkBackground) {
                                    tentacle3Circles[j].color = new Color((float)27/255, (float)11/255, (float)55/255);
                                }
                                else {
                                    tentacle3Circles[j].color = new Color((float)27/255, (float)11/255, (float)153/255);
                                }
                            }
                            tentacle3Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                            if (something.tentacles[i].cList[j].grayscale) {
                                tentacle3Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                            }
                        }
                        if (i == 3) {
                            Vector2 vector = (something.tentacles[i].cList[j].pointA.position-something.tentacles[i].cList[j].pointB.position).normalized;
                            bool rotationSide = vector.x < 0;
                            tentacle4Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                            //self.player.room.AddObject(new Spark(tentacle4Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                            if (something.tentacles[i].cList[j].brightBackground) {
                                tentacle4Circles[j].color = new Color((float)27/255, (float)11/255, (float)253/255);
                            }
                            else {
                                if (something.tentacles[i].cList[j].darkBackground) {
                                    tentacle4Circles[j].color = new Color((float)27/255, (float)11/255, (float)55/255);
                                }
                                else {
                                    tentacle4Circles[j].color = new Color((float)27/255, (float)11/255, (float)153/255);
                                }
                            }
                            tentacle4Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                            if (something.tentacles[i].cList[j].grayscale) {
                                tentacle4Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                            }
                        }
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
                    if (something.retractionTimer <= -10f && (i < something.initialDecoLegSprite || i >= something.initialLegSprite)) {
                        sLeaser.sprites[i].color = new Color(sLeaser.sprites[i].color.r, sLeaser.sprites[i].color.g, sLeaser.sprites[i].color.b, Mathf.Lerp(0f,1f,something.retractionTimer/10));
                    }
                }

                for (int i = something.initialBodyRotSprite; i < something.initialCircleSprite; i++) {
                    int halfLength = ((something.initialCircleSprite - something.initialBodyRotSprite)/2) + something.initialBodyRotSprite;
                    int k = i - something.initialBodyRotSprite;
                    k -= i<halfLength? 0:halfLength-something.initialBodyRotSprite;
                    //base.Logger.LogDebug("The current i & k indexies are:");
                    //base.Logger.LogDebug(i);
                    //base.Logger.LogDebug(k);
                    Vector2 direction = something.rList[k].chunk2.GetPosition() - something.rList[k].chunk1.GetPosition();
                    Vector2 dirNormalized = direction.normalized;
                    Vector2 perpendicularVector = Custom.PerpendicularVector(direction);
                    sLeaser.sprites[i].SetPosition(something.rList[k].chunk1.GetPosition() + (dirNormalized * something.rList[k].offset.y) + (perpendicularVector * something.rList[k].offset.x));
                    sLeaser.sprites[i].scale = something.rList[k].scale;
                    if (k < i - something.initialBodyRotSprite) {
                        sLeaser.sprites[i].color = new Color((float)27/255, (float)11/255, (float)253/255);
                    }
                    //sLeaser.sprites[i].color = something.rotEyeColor;
                }
            }
        };
        
        /*On.RoomCamera.ctor += (orig, self, game, cameraNumber) => {   //Leftover from when I was doing a slight amount of trolling :3
            orig(self, game, cameraNumber);
            FSprite replacementBackground = new FSprite("bgreplace", false);
            replacementBackground.x = 700f;
            //self.backgroundGraphic = replacementBackground;
            //replacementBackground.anchorX = 350f;
            //self.ReturnFContainer("Foreground").RemoveChild(self.backgroundGraphic);
            //self.backgroundGraphic = new FSprite("bgreplace", false);
            //self.backgroundGraphic.shader = game.rainWorld.Shaders["Background"];
            //self.backgroundGraphic.anchorX = 0f;
            //self.backgroundGraphic.anchorY = 0f;
            //self.backgroundGraphic.scale = 4f;
            self.ReturnFContainer("Shadows").AddChild(replacementBackground);
            replacementBackground.MoveToBack();
        };*/
    }
    private void Init(On.RainWorld.orig_OnModsInit orig, RainWorld self) {
        orig(self);

        if (!init) {
            init = true;
            try {
                //AssetManager.ResolveFilePath("atlases/mane.png");
                Futile.atlasManager.LoadAtlas("atlases/lightgrayscalesprite");
                Futile.atlasManager.LoadAtlas("atlases/grayscalesprite");
                Futile.atlasManager.LoadAtlas("atlases/roteyeeye");
                Futile.atlasManager.LoadAtlas("atlases/roteye");
                Futile.atlasManager.LoadAtlas("atlases/RotFace");
                //Futile.atlasManager.LoadAtlas("atlases/body");
                //Futile.atlasManager.LoadAtlas("atlases/mane");
                //Futile.atlasManager.LoadAtlas("atlases/bgreplace");
                this.Options = new RotCatOptions(this, Logger);
                staticOptions = this.Options;
                MachineConnector.SetRegisteredOI("rotcat", Options);
                configWorking = true;
            } catch (Exception err) {
                base.Logger.LogError(err);
                configWorking = false;
            }
        }
    }
}

public class PlayerEx
{
    public Line tentacleOne;    //These honestly don't need to be here, since they are immediantly grouped into the tentacles array
    public Line tentacleTwo;    //These honestly don't need to be here, since they are immediantly grouped into the tentacles array
    public Line tentacleThree;  //These honestly don't need to be here, since they are immediantly grouped into the tentacles array
    public Line tentacleFour;   //These honestly don't need to be here, since they are immediantly grouped into the tentacles array
    public Line[] tentacles = new Line[4];  //Array that stores the logic bits of the movement tentacles
    public Line[] decorativeTentacles = new Line[2];    //Array that stores the logic bits of the decorative tentacles
    public int initialDecoLegSprite;    //The position of the initial decorative tentacle in the sLeaser array
    public float retractionTimer = 80;
    public Vector2 previousPosition;    //The previous position of the player, used to control if the tentacles should be retracted based on movement
    public int initialLegSprite;    //The position of the initial movement tentacle in the sLeaser array
    public int segments = 25;   //The amount of points in the movement tentacles.Might be aassumed as 10 in some places, can't remember, so changing this might break something
    public int decorationSegments = 10; //The amount of points in the decorative tentacles. Might be aassumed as 10 in some places, can't remember, so changing this might break something
    public Vector2 potentialGrapSpot;
    public int totalCircleSprites;  //Assigned later, keeps track of the total amount of circle sprites on all the tentacles (Is used solely for resizing the sLeaser array)
    public TargetPos[] targetPos = new TargetPos[4] {new TargetPos(), new TargetPos(), new TargetPos(), new TargetPos()};
    public bool automateMovement = false;   //Determines whether the tentacles will guide themselves toward a wall/pole
    public float grabWallCooldown = 0;  //Currently unused, honestly can't remember what I wanted to do with it
    public Vector2[] randomPosOffest;   //Currently not used, applies a force the the 2 decorative tentacles behind the scug
    public bool overrideControls = false;
    public int circleAmmount = 19;  //Change this to alter the amount of circle sprites that can be made
    public int initialCircleSprite;     //Stores the position of the initial tantacle circle sprite in the sprite list
    public int initialBodyRotSprite;    //Stores the position of the initial bodyRot sprite in the sprite list
    public int bodyRotSpriteAmount = 8;     //Change this whenever I change the amount of Rot sprites on the body
    public BodyRot[] rList;     //An array that stores the logic for the body rot sprites
    public Color rotEyeColor;   //Should store and control the color of the X sprites from the slugbase custom color
    public FAtlas faceAtlas;
    public class TargetPos {    //Movement tentacle targeting logic, probably very messing in implementation
        public Vector2 targetPosition = new Vector2(0,0);
        public bool connectionTaken = false;
        public bool foundSurface = false;
        public bool isPole = false;
    }
    public bool isRot = false;  //Is set to true if the Slugrot character is selected, so it doesn't apply anything to non-rot characters
}
public class Functions {
    public static float FindPos(bool flag, Player self, RotCatOptions options) {
        if ((Input.GetKey(options.tentMovementRight.Value) && flag) || (!flag && self.input[0].x == 1)) {
            if ((Input.GetKey(options.tentMovementUp.Value) && flag) || (!flag && self.input[0].y == 1)) {
                return 0;
            }
            else {
                return 7*(float)Math.PI/4;
            }
        }
        else if ((Input.GetKey(options.tentMovementUp.Value) && flag) || (!flag && self.input[0].y == 1)) {
            if ((Input.GetKey(options.tentMovementLeft.Value) && flag) || (!flag && self.input[0].x == -1)) {
                return (float)Math.PI/2;
            }
            else {
                return (float)Math.PI/4;
            }
        }
        else if ((Input.GetKey(options.tentMovementLeft.Value) && flag) || (!flag && self.input[0].x == -1)) {
            if ((Input.GetKey(options.tentMovementDown.Value) && flag) || (!flag && self.input[0].y == -1)) {
                return (float)Math.PI;
            }
            else {
                return 3*(float)Math.PI/4;
            }
        }
        else if ((Input.GetKey(options.tentMovementDown.Value) && flag) || (!flag && self.input[0].y == -1)) {
            if ((Input.GetKey(options.tentMovementRight.Value) && flag) || (!flag && self.input[0].x == 1)) {
                return 3*(float)Math.PI/2;
            }
            else {
                return 5*(float)Math.PI/4;
            }
        }
        return 0;
    }
    public static void StickCalculations(Line[][] totalTentacles) {
        foreach (Line[] tentacleList in totalTentacles) {
            foreach (Line tentacle in tentacleList) {
                foreach (Stick stick in tentacle.sList) {
                    Vector2 stickCenter = (stick.pointA.position + stick.pointB.position)/2;
                    Vector2 stickDir = (stick.pointA.position - stick.pointB.position).normalized;
                    if (!stick.pointA.locked)
                        stick.pointA.position = stickCenter + stickDir * stick.length / 2;
                    if (!stick.pointB.locked)
                        stick.pointB.position = stickCenter - stickDir * stick.length / 2;
                }
            }
        }
    }
    public static void TentacleRetraction(bool condition1, bool condition2, bool condition3, Player self, PlayerEx something) {
        if (condition1) {//Change limits back to 1f and 4.5f once testing is done
            something.retractionTimer += 0.5f;
        }
        else if (condition2) {
            something.retractionTimer -= 0.5f;
        }
        if (condition3) {
            foreach (var tentacle in something.tentacles) {
                foreach (var stick in tentacle.sList)
                {
                    stick.length = Mathf.Lerp(0.15f, 10, something.retractionTimer/40);
                }
            }
        }
        something.previousPosition = self.mainBodyChunk.pos;
    }
    public static void PrimTentAndPlayerMovement(PlayerEx something, Player self, RotCatOptions staticOptions) {
        if (Input.GetKey(staticOptions.tentMovementAutoEnable.Value)) {
            something.automateMovement = true;
        }
        if (self.room != null && !(self.room.GetTile(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position).Solid || self.room.GetTile(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position).AnyBeam) && !something.automateMovement) {
            int upDown = (Input.GetKey(staticOptions.tentMovementUp.Value)? 1:0) + (Input.GetKey(staticOptions.tentMovementDown.Value)? -1:0);
            int rightLeft = (Input.GetKey(staticOptions.tentMovementRight.Value)? 1:0) + (Input.GetKey(staticOptions.tentMovementLeft.Value)? -1:0);
            if (Custom.Dist(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position + new Vector2(3*(something.overrideControls? rightLeft:self.input[0].x), 3*(something.overrideControls? upDown:self.input[0].y)), self.mainBodyChunk.pos) < 300f) {
                something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position += new Vector2(3*(something.overrideControls? rightLeft:self.input[0].x), 3*(something.overrideControls? upDown:self.input[0].y));
            }
        }
        else {
            if (!something.automateMovement) {
                something.tentacles[0].iWantToGoThere = something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position;
            }
            something.automateMovement = true;
            int connectionsToSurface = 0;
            foreach (var tentacle in something.tentacles) {
                if (tentacle.isAttatchedToSurface == 1) {
                    connectionsToSurface += 1;
                }
            }
            if (connectionsToSurface == 0 && !(Input.GetKey(staticOptions.tentMovementAutoEnable.Value))) {
                something.automateMovement = false;
                //self.controller = new Player.NullController();
            }
            self.mainBodyChunk.vel = new Vector2(2.3f*connectionsToSurface*self.input[0].x, 2.3f*connectionsToSurface*self.input[0].y);
            self.airFriction = 0.75f;
            self.customPlayerGravity = 0.25f;
            foreach (var chunk in self.bodyChunks)
            {
                if (chunk != self.mainBodyChunk)
                {
                    //base.Logger.LogDebug(self.room.GetTile(chunk.pos + new Vector2(0,-1f)).Solid);
                    if (self.room != null && !self.room.GetTile(chunk.pos + Vector2.down).Solid)
                    {
                        chunk.vel = Vector2.down * self.customPlayerGravity * self.room.gravity;
                    }
                    else
                    {
                        chunk.vel = new Vector2(0,1);
                        //chunk.pos += new Vector2(0,10);
                    }
                }
            }
        }
    }
    public static void TentaclesFindPositionToGoTo(PlayerEx something, Player self, float startPos) {
        for (int i = 0; i < something.targetPos.Length; i++) {
            if (something.targetPos[i].foundSurface && Custom.Dist(self.mainBodyChunk.pos, something.targetPos[i].targetPosition) >= 250) {
                something.targetPos[i].foundSurface = false;
                something.targetPos[i].connectionTaken = false;
            }
            for (float k = 0; k < 200; k++) {
                for (float j = startPos + (float)Math.PI/8*(i); j < startPos + (float)Math.PI/8*(i+1); j+=((float)Math.PI)/256f) {
                    if (self.room != null && (self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).Solid || self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).AnyBeam) && !something.targetPos[i].foundSurface) {
                        if (self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).AnyBeam) {
                            something.targetPos[i].isPole = true;
                        }
                        else {
                            something.targetPos[i].isPole = false;
                        }
                        something.targetPos[i].targetPosition = new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y) + (something.targetPos[i].isPole? (new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)-self.mainBodyChunk.pos).normalized * 5 : new Vector2(0,0));
                        something.targetPos[i].foundSurface = true;
                    }
                    /*else if (!something.targetPos[i].foundSurface) {
                        something.targetPos[i].targetPosition = self.mainBodyChunk.pos - Vector2.down * 5;
                    }*/
                }
            }
            //base.Logger.LogDebug(something.targetPos[i]);
            //self.room.AddObject(new Spark(something.targetPos[i].targetPosition, new Vector2(-5,5), Color.green, null, 10, 20));
            //self.room.AddObject(new Spark(something.targetPos[i].targetPosition, new Vector2(5,5), Color.green, null, 10, 20));
        }
    }
    public static void MoveTentacleToPosition(PlayerEx something, Player self) {
        for (int i = 0; i < something.tentacles.Length; i++) {
            //base.Logger.LogDebug(something.targetPos[i].isPipe);
            //base.Logger.LogDebug(Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere));
            if (!something.targetPos[i].connectionTaken && something.automateMovement) {
                something.targetPos[i].connectionTaken = true;
                something.tentacles[i].iWantToGoThere = something.targetPos[i].targetPosition;
                //self.room.AddObject(new Spark(something.tentacles[i].iWantToGoThere, new Vector2(5,5), Color.blue, null, 10, 20));  //Testing
            }
            if (Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere) > (something.targetPos[i].isPole? 0f:5f) && something.automateMovement) {//Change value here too
                something.tentacles[i].isAttatchedToSurface = 0;
                Vector2 direction = (something.tentacles[i].iWantToGoThere - something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position);
                something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position += direction / ((Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere) > 5f)? 9f:1f); //Tip, controls speed tentacles move to their target pos
                something.tentacles[i].canPlaySound = true;
                //base.Logger.LogDebug(direction);
            }
            if (self.room != null && Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere) < 15f) {
                something.tentacles[i].isAttatchedToSurface = 1;    //Casually giving the player some lenience
                if (something.tentacles[i].canPlaySound) {
                    self.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Terrain, something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, 1f, 1f);
                    something.tentacles[i].canPlaySound = false;
                }
            }
        }
    }
}

//These bits are for the extra sprite logic/data storage
public class Point
{
    public Vector2 position = new Vector2(700,200);
    public Vector2 prevPosition = new Vector2(701,200);
    public bool locked = false;
    public Point(Vector2 position, bool locked) {
        this.position = position;
        this.locked = locked;
    }
}
public class Stick {
    public Point pointA, pointB;
    public float length;
    public Stick(Point pointA, Point pointB, float length) {
        this.pointA = pointA;
        this.pointB = pointB;
        this.length = length;
    }
}
public class Line {
    public Point[] pList;
    public Stick[] sList;
    public Circle[] cList;
    public Vector2 iWantToGoThere;
    public int isAttatchedToSurface = 0;
    public Vector2 decoPushDirection = new Vector2(0,0);
    public bool canPlaySound = true;
}
public class BodyRot {
    public BodyRot (FSprite chunk1, FSprite chunk2, Vector2 offset, float scale/*, int bodyRotEyePosInSpriteList = null*/) {
        this.chunk1 = chunk1;
        this.chunk2 = chunk2;
        this.offset = offset;
        this.scale = scale;
        /*this.bodyRotEyePosInSpriteList = bodyRotEyePosInSpriteList*/
    }
    public FSprite chunk1;
    public FSprite chunk2;
    public Vector2 offset;
    public float scale;
    //public int bodyRotEyePosInSpriteList;
}
public class Circle {
    public Circle (Point pointA, Point pointB, Vector2 offset, bool background, bool brightBackground, float scale, float scaleX = 1f, float scaleY = 1f, bool lightgrayscale = false, float rotation = 0) {
        this.pointA = pointA;
        this.pointB = pointB;
        this.offset = offset;
        this.darkBackground = background;
        this.scale = scale;
        this.brightBackground = brightBackground;
        this.grayscale = lightgrayscale;
        this.rotation = rotation;
        if (scaleX == 1) {
            this.scaleX = scale;
        }
        else {
            this.scaleX = scaleX;
        }
        if (scaleY == 1) {
            this.scaleY = scale;
        }
        else {
            this.scaleY = scaleY;
        }
    }
    public Point pointA;
    public Point pointB;
    public Vector2 offset;
    public bool darkBackground;
    public bool brightBackground;
    public Vector2 position;
    public float scale;
    public float scaleX;
    public float scaleY;
    public bool grayscale;
    public float rotation;
}