using System;
using System.Linq;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Chimeric {
    public static class Functions {
        ///<summary> Find the position for tentacles to start finding connection points at </summary>
        public static float FindPos(bool flag, Player self) {
            if ((Input.GetKey(ChimericOptions.TentMovementRight.Value) && flag) || (!flag && self.input[0].x == 1)) {
                if ((Input.GetKey(ChimericOptions.TentMovementUp.Value) && flag) || (!flag && self.input[0].y == 1)) {
                    return 0;
                }
                else {
                    return 7*(float)Math.PI/4;
                }
            }
            else if ((Input.GetKey(ChimericOptions.TentMovementUp.Value) && flag) || (!flag && self.input[0].y == 1)) {
                if ((Input.GetKey(ChimericOptions.TentMovementLeft.Value) && flag) || (!flag && self.input[0].x == -1)) {
                    return (float)Math.PI/2;
                }
                else {
                    return (float)Math.PI/4;
                }
            }
            else if ((Input.GetKey(ChimericOptions.TentMovementLeft.Value) && flag) || (!flag && self.input[0].x == -1)) {
                if ((Input.GetKey(ChimericOptions.TentMovementDown.Value) && flag) || (!flag && self.input[0].y == -1)) {
                    return (float)Math.PI;
                }
                else {
                    return 3*(float)Math.PI/4;
                }
            }
            else if ((Input.GetKey(ChimericOptions.TentMovementDown.Value) && flag) || (!flag && self.input[0].y == -1)) {
                if ((Input.GetKey(ChimericOptions.TentMovementRight.Value) && flag) || (!flag && self.input[0].x == 1)) {
                    return 3*(float)Math.PI/2;
                }
                else {
                    return 5*(float)Math.PI/4;
                }
            }
            return 0;
        }
        public static void StickCalculations(Tentacle[][] totalTentacles, Player self) {
            foreach (Tentacle[] tentacleList in totalTentacles) {
                foreach (Tentacle tentacle in tentacleList) {
                    foreach (PointConnection stick in tentacle.sList) {
                        stick.Update(self);
                    }
                }
            }
        }
        public static void TentacleRetraction(Player self, PlayerEx something) {
            if (Input.GetKey(ChimericOptions.TentMovementAutoEnable.Value) && something.retractionTimer < 40) {
                something.retractionTimer += 5;
            }

            //bool notTooFarNotTooClose = (Custom.Dist(something.previousPosition, self.mainBodyChunk.pos) > 1f && Custom.Dist(something.previousPosition, self.mainBodyChunk.pos) < 3.5f);

            if ((self.dead || /*notTooFarNotTooClose ||*/ Input.GetKey(ChimericOptions.TentMovementEnable.Value)) && something.retractionTimer < 60) {//Change limits back to 1f and 3.5f once testing is done  //nolol
                something.retractionTimer += 0.5f;
            }
            else if (something.retractionTimer > 7/*-20*/ && !Input.GetKey(ChimericOptions.TentMovementEnable.Value) && !self.dead) {
                something.retractionTimer -= 0.5f;
            }
            foreach (var tentacle in something.tentacles) {
                foreach (var stick in tentacle.sList) {
                    if (something.automateMovement) {
                        stick.length = Vector2.Distance(tentacle.pList[tentacle.pList.Length-1].pos, tentacle.pList[0].pos)/36f;
                        //Debug.Log("Using distance");
                    }
                    else if (something.retractionTimer > 0 && something.retractionTimer <= 40 ) {
                        stick.length = Mathf.Lerp(0.15f, 10, something.retractionTimer/40);
                        //Debug.Log("Using retraction");
                    }
                }
            }
            foreach (Tentacle tentacle in something.decorativeTentacles) {
                int pointer = Array.IndexOf(something.decorativeTentacles, tentacle);
                Vector2 direction = self.mainBodyChunk.pos - self.bodyChunks[1].pos;
                Vector2 dirNormalized = direction.normalized;
                Vector2 perpendicularVector = Custom.PerpendicularVector(direction);
                foreach (var p in tentacle.pList) {
                    if (!p.locked && self.room != null) {
                        Vector2 positionBeforeUpdate = p.pos;
                        p.pos += (p.pos - p.lastPos) * Random.Range(0.9f,1.1f);
                        p.pos += Vector2.down * self.room.gravity * Random.Range(0.9f,1.1f);
                        p.lastPos = positionBeforeUpdate;
                    }
                    if (Array.IndexOf(tentacle.pList, p) == 0) {
                        p.locked = true;
                        p.pos = self.mainBodyChunk.pos + (dirNormalized * something.randomPosOffest[pointer*2].y) + (perpendicularVector * something.randomPosOffest[pointer*2].x);
                    }
                    if (Array.IndexOf(tentacle.pList, p) == tentacle.pList.Length-1) {
                        p.locked = true;
                        p.pos = self.mainBodyChunk.pos + (dirNormalized * something.randomPosOffest[(pointer*2)+1].y) + (perpendicularVector * something.randomPosOffest[(pointer*2)+1].x);
                    }
                }
            }
            // something.previousPosition = self.mainBodyChunk.pos;
        }
        public static void PrimaryTentacleAndPlayerMovement(PlayerEx something, Player self) {
            if (Input.GetKey(ChimericOptions.TentMovementAutoEnable.Value) && !Input.GetKey(ChimericOptions.TentMovementEnable.Value)) {
                something.automateMovement = true;
            }
            if (!something.automateMovement && self.room != null && !(self.room.GetTile(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].pos).Solid || self.room.GetTile(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].pos).AnyBeam)) {
                
                something.tentacles[0].foundSurface = true;
                
                int upDown = (Input.GetKey(ChimericOptions.TentMovementUp.Value)? 1:0) + (Input.GetKey(ChimericOptions.TentMovementDown.Value)? -1:0);
                int rightLeft = (Input.GetKey(ChimericOptions.TentMovementRight.Value)? 1:0) + (Input.GetKey(ChimericOptions.TentMovementLeft.Value)? -1:0);
                
                float dist = Custom.Dist(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].pos + new Vector2(3*(something.overrideControls? rightLeft:self.input[0].x), 3*(something.overrideControls? upDown:self.input[0].y)), self.mainBodyChunk.pos);
                if (dist < 300f) {
                    float weight = 0f;
                    if (something.stuckCreature != null && something.stuckCreature.PhysObject.realizedObject is Creature crit) { weight = crit.TotalMass/5f*self.room.gravity; Debug.Log(crit.TotalMass); }
                    something.tentacles[0].pList[something.tentacles[0].pList.Length-1].pos += new Vector2(3f*(something.overrideControls? rightLeft:self.input[0].x), 6f*(something.overrideControls? upDown:self.input[0].y) - Mathf.Min(weight, 5f));
                }
                if (/*Input.GetKey(ChimericOptions.GrabButton.Value)*/ self.input[0].pckp && something.stuckCreature == null && self.Consious && something.swimTimer == 0) {   // Think I'll remove the grab bind and use the pickup keybind from vanilla
                    for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++) {
                        foreach (Creature.Grasp grasp in self.grasps) { // skip this creature if it is already being grabbed by the player's normal grasp
                            if (grasp != null && grasp.grabbedChunk.owner.abstractPhysicalObject == self.room.abstractRoom.creatures[i]) {
                                goto Skip;
                            }
                        }
                        for (int j = 0; j < self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length; j++) {
                            if (Custom.DistLess(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].pos, self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos, 15f) && self.room.abstractRoom.creatures[i].realizedCreature != self) {
                                something.stuckCreature = new AbstractOnTentacleStick(self.abstractCreature, self.room.abstractRoom.creatures[i], j);
                                self.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Creature, self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos, 1f, 1f);
                                something.stuckCreature.ChangeOverlap(false);
                                something.swimTimer = 40;
                                goto Escape;
                            }
                        }
                        Skip:;
                    }
                    Escape:;
                }
                /*else if (dist > 305f) {
                    something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position = Vector2.MoveTowards(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position, self.mainBodyChunk.pos, 1*self.mainBodyChunk.vel.magnitude);
                }*/
            }
            else {
                //self.canJump = 0;
                //self.wantToJump = 0;
                self.airFriction = 0.85f;
                self.customPlayerGravity = 0.2f;
                if (self.feetStuckPos != null && !Input.GetKey(ChimericOptions.TentMovementAutoEnable.Value)) {  //Stop feet from magnetising to the ground
                    self.bodyChunks[1].pos = self.feetStuckPos.Value+Vector2.up*2f;
                }
                if (!something.automateMovement) {  //If the tentacle is making first contact, make it go to that position
                    something.tentacles[0].iWantToGoThere = something.tentacles[0].pList[something.tentacles[0].pList.Length-1].pos;
                    something.tentacles[0].foundSurface = true;
                    something.tentacles[0].hasConnectionSpot = true;
                }
                something.automateMovement = true;
                int connectionsToSurface = 0;   //Get how many tentacles are attached to the terrain/poles
                foreach (var tentacle in something.tentacles) {
                    if (tentacle.isAttatchedToSurface == 1) {
                        connectionsToSurface += 1;
                    }
                }
                if (connectionsToSurface == 0 && !Input.GetKey(ChimericOptions.TentMovementAutoEnable.Value)) {
                    something.automateMovement = false;
                }

                #region BodyChunkMovements
                //Debug.Log(something.timer);
                if (connectionsToSurface == 0 && Input.GetKey(ChimericOptions.TentMovementAutoEnable.Value) && !Input.GetKey(ChimericOptions.TentMovementEnable.Value)) {
                    self.customPlayerGravity = 1f;
                    //self.mainBodyChunk.vel -= new Vector2(0f, 0.2f);
                }
                if (Input.GetKey(ChimericOptions.TentMovementEnable.Value) && connectionsToSurface != 0) {
                    const float timerAddSub = 0.07f;
                    if ((self.input[0].x != 0 || self.input[0].y != 0) && something.timer < 1f) {
                        something.timer += timerAddSub;
                    }
                    else if (something.timer > 0f) {
                        something.timer -= timerAddSub;
                    }
                    // Rotund world stuff affecting tentacle movement
                    float rotundFactor = Plugin.rotund? (self.TotalMass >= 1.7f? self.TotalMass : 1f) : 1f;
                    self.mainBodyChunk.vel = Vector2.Lerp(new Vector2(0,0.82f*(self.room==null? 0 : self.room.gravity)-((rotundFactor-1)*1.15f)), new Vector2(2.3f*connectionsToSurface*self.input[0].x/rotundFactor, 2.3f*connectionsToSurface*self.input[0].y-(rotundFactor-1)*1.15f), something.timer);
                    foreach (var chunk in self.bodyChunks)
                    {
                        if (chunk != self.mainBodyChunk)
                        {
                            //base.Logger.LogDebug(self.room.GetTile(chunk.pos + new Vector2(0,-1f)).Solid);
                            if (self.room != null && !self.room.GetTile(chunk.pos + Vector2.down).Solid)
                            {
                                chunk.vel = Vector2.down * self.customPlayerGravity * self.room.gravity / self.airFriction;
                            }
                            else
                            {
                                chunk.vel = new Vector2(0, 0.84f);
                            }
                        }
                    }
                }
                #endregion
            }
        }
        public static void TentaclesFindPositionToGoTo(PlayerEx something, Player self, float startPos) {
            int numerations = (self.room != null && self.room.game.IsStorySession && (self.room.world.region.name=="RM" || self.room.world.region.name=="SS" || self.room.world.region.name=="DM"))? 100 : self.room?.abstractRoom.name == "SB_L01"? 5 : 200;
            float multiple = numerations==100? 2f : numerations==5? 40f : 1f;
            for (int i = 0; i < something.tentacles.Length; i++) {
                if (something.tentacles[i].foundSurface && (Custom.Dist(self.mainBodyChunk.pos, something.tentacles[i].targetPosition) >= 250 || Custom.Dist(self.mainBodyChunk.pos, something.tentacles[i].iWantToGoThere) >= 250) && something.tentacles[i].hasConnectionSpot) {
                    something.tentacles[i].foundSurface = false;
                    something.tentacles[i].hasConnectionSpot = false;
                }
                // These two for loops make a part of a circle, where k is the radius and j is the relative angle
                for (float k = 0; k < numerations; k++) {
                    for (float j = startPos + (float)Math.PI/8*(i); j < startPos + (float)Math.PI/8*(i+1); j+=((float)Math.PI/256f)*multiple) {
                        Vector2 position = new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y);
                        var tile = self.room?.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y));
                        if (!something.tentacles[i].foundSurface && self.room != null && tile != null && (tile.Solid || tile.AnyBeam)) {
                            if (tile.AnyBeam) {
                                something.tentacles[i].isPole = true;
                            }
                            else {
                                something.tentacles[i].isPole = false;
                            }   //These two are technically the same I think
                            //something.targetPos[i].isPole = (self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).AnyBeam);
                            something.tentacles[i].targetPosition = position + (something.tentacles[i].isPole? (position-self.mainBodyChunk.pos).normalized * 5 : new Vector2(0,0));
                            something.tentacles[i].foundSurface = true;
                            goto End;   // If this tentacle found a valid position, skip doing the rest of the math and go to the next one
                        }
                        /*else if (!something.targetPos[i].foundSurface && self.room != null && (tile == null || (!tile.Solid && !tile.AnyBeam))) {
                            something.targetPos[i].foundSurface = false;
                            something.targetPos[i].targetPosition = self.mainBodyChunk.pos - Vector2.down * 5;
                        }*/
                    }
                }
                End:;
            }
        }
        public static void MoveTentacleToPosition(PlayerEx something, Player self) {
            for (int i = 0; i < something.tentacles.Length; i++) {
                //base.Logger.LogDebug(something.targetPos[i].isPipe);
                //base.Logger.LogDebug(Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere));
                if (!something.tentacles[i].hasConnectionSpot && something.automateMovement) {
                    something.tentacles[i].hasConnectionSpot = true;
                    //Debug.Log($"Please god help me here: {something.tentacles[i].iWantToGoThere} and {something.targetPos[i].targetPosition}");
                    something.tentacles[i].iWantToGoThere = something.tentacles[i].targetPosition;
                    //self.room.AddObject(new Spark(something.tentacles[i].iWantToGoThere, new Vector2(5,5), Color.blue, null, 10, 20));  //Testing
                }
                if (Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].pos, something.tentacles[i].iWantToGoThere) > (something.tentacles[i].isPole? 5f:5f/*Can be adjusted maybe, rn it plays multiple times for poles*/) && something.automateMovement) {
                    something.tentacles[i].isAttatchedToSurface = 0;
                    Vector2 direction = something.tentacles[i].iWantToGoThere - something.tentacles[i].pList[something.tentacles[i].pList.Length-1].pos;
                    
                    something.tentacles[i].pList[something.tentacles[i].pList.Length-1].pos += direction / ((Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].pos, something.tentacles[i].iWantToGoThere) > 5f)? 9f:1f); //Tentacle Tip, controls speed tentacles move to their target pos
                    
                    something.tentacles[i].canPlaySound = true;
                    //base.Logger.LogDebug(direction);
                }
                if (self.room != null && Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].pos, something.tentacles[i].iWantToGoThere) < 15f) {    //Casually giving the player some lenience
                    something.tentacles[i].isAttatchedToSurface = 1;
                    if (something.tentacles[i].canPlaySound) {
                        self.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Terrain, something.tentacles[i].pList[something.tentacles[i].pList.Length-1].pos, 1f, 1f);
                        something.tentacles[i].canPlaySound = false;
                    }
                }
            }
        }
        ///<summary> Replaced the normal face with Slugrot's face </summary>
        public static void DrawFace(PlayerEx something, RoomCamera.SpriteLeaser sLeaser, string? name) {
            if (name != null && name != null && name.StartsWith("Face") && something.faceAtlas._elementsByName.TryGetValue("Rot" + name, out var element)) {
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
        }
        public static void DrawTentacleCircles(PlayerGraphics self, PlayerEx something, Vector2 camPos, FSprite[] tentacle1Circles, FSprite[] tentacle2Circles, FSprite[] tentacle3Circles, FSprite[] tentacle4Circles) {
            Color rotBulbColor = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2);
            //Set the circle positions on the tentacles and color them
            for (int i = 0; i < something.tentacles.Length; i++) {
                for (int j = 0; j < something.tentacles[i].cList.Length; j++) {
                    //base.Logger.LogDebug("Drawsprites");
                    //base.Logger.LogDebug(something.tentacles[i].cList[j].position);
                    // Works, but could do with a rewrite to be less ugly.
                    if (i == 0) {
                        Vector2 vector = (something.tentacles[i].cList[j].pointA.pos-something.tentacles[i].cList[j].pointB.pos).normalized;
                        bool rotationSide = vector.x < 0;
                        tentacle1Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                        tentacle1Circles[j].color = GetColor(rotBulbColor, something.tentacles[i].cList[j].brightBackground, something.tentacles[i].cList[j].darkBackground);
                        tentacle1Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f) + something.tentacles[i].cList[j].rotation;
                        if (something.tentacles[i].cList[j].grayscale) {
                            tentacle1Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                        }
                    }
                    if (i == 1) {
                        Vector2 vector = (something.tentacles[i].cList[j].pointA.pos-something.tentacles[i].cList[j].pointB.pos).normalized;
                        bool rotationSide = vector.x < 0;
                        tentacle2Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                        //self.player.room.AddObject(new Spark(tentacle2Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                        tentacle2Circles[j].color = GetColor(rotBulbColor, something.tentacles[i].cList[j].brightBackground, something.tentacles[i].cList[j].darkBackground);
                        tentacle2Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                        if (something.tentacles[i].cList[j].grayscale) {
                            tentacle2Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                        }
                    }
                    if (i == 2) {
                        Vector2 vector = (something.tentacles[i].cList[j].pointA.pos-something.tentacles[i].cList[j].pointB.pos).normalized;
                        bool rotationSide = vector.x < 0;
                        tentacle3Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                        //self.player.room.AddObject(new Spark(tentacle3Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                        tentacle3Circles[j].color = GetColor(rotBulbColor, something.tentacles[i].cList[j].brightBackground, something.tentacles[i].cList[j].darkBackground);
                        tentacle3Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                        if (something.tentacles[i].cList[j].grayscale) {
                            tentacle3Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                        }
                    }
                    if (i == 3) {
                        Vector2 vector = (something.tentacles[i].cList[j].pointA.pos-something.tentacles[i].cList[j].pointB.pos).normalized;
                        bool rotationSide = vector.x < 0;
                        tentacle4Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                        //self.player.room.AddObject(new Spark(tentacle4Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                        tentacle4Circles[j].color = GetColor(rotBulbColor, something.tentacles[i].cList[j].brightBackground, something.tentacles[i].cList[j].darkBackground);
                        tentacle4Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                        if (something.tentacles[i].cList[j].grayscale) {
                            tentacle4Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                        }
                    }
                }
            }
        }
        ///<summary> Update the color of the vignette sprite, and use inputs to do math to determine the center position. r + g are replaced, b + a are passed through </summary>
        public static void UpdateVignette(RainWorld game, Player self, Vector2 camPos, Color newOuterColor, Color? newInnerColor = null, bool visible = true) {
            if (Plugin.vignetteEffect != null) {
                // Set the position to center the Vignette Effect on.
                float xVar = (self.mainBodyChunk.pos.x-camPos.x)/(game.screenSize.x);
                float yVar = (self.mainBodyChunk.pos.y-camPos.y)/(game.screenSize.y)/(86f/48f)+0.22f;    //Math numbers are gotten by doing the best thing ever, complete guesswork!
                Shader.SetGlobalVector("_VignettePlayerPos", new Vector2(xVar, yVar));

                Plugin.vignetteEffect.color = newOuterColor;
                if (newInnerColor is Color newestColor) { Shader.SetGlobalColor("_VignetteInnerColor", newestColor); }
                Plugin.vignetteEffect.isVisible = visible;
                //Debug.Log($"Update Vignette. rVar: {rVar} gVar: {gVar} bodyX: {self.mainBodyChunk.pos.x-camPos.x} bodyY: {self.mainBodyChunk.pos.y-camPos.y}");
            }
            else {
                Debug.LogWarning("VignetteEffect was null!");
            }
        }
        ///<summary> Hard-replacement of the Vignette color </summary>
        public static void UpdateVignette(Color newOuterColor, Color? newInnerColor = null, bool visible = true) {
            if (Plugin.vignetteEffect != null) {
                Plugin.vignetteEffect.color = newOuterColor;
                if (newInnerColor is Color newestColor) { Shader.SetGlobalColor("_VignetteInnerColor", newestColor); }
                Plugin.vignetteEffect.isVisible = visible;
                //Debug.Log($"Default Color is: {Shader.GetGlobalColor("_InputColorA")}");
            }
            else {
                Debug.LogWarning("VignetteEffect was null!");
            }
        }
        public static Color GetColor(Color rotEyeColor, bool brightBackground, bool darkBackground) {
            float r = rotEyeColor.r, g = rotEyeColor.g, b = rotEyeColor.b;
            if (brightBackground) {
                return rotEyeColor; //Default value: 27/255, 11/255, 253/255
            }
            else if (darkBackground) {
                if (r > g && r > b) {
                    r /= 5f;
                }
                else if (g > r && g > b) {
                    g /= 5f;
                }
                else if (b > r && b > g) {
                    b /= 5f;
                }
                return new Color(r, g, b); //new Color((float)27/255, (float)11/255, (float)55/255);
            }
            else {
                if (r > g && r > b) {
                    r /= 1.64f;
                }
                else if (g > r && g > b) {
                    g /= 1.64f;
                }
                else if (b > r && b > g) {
                    b /= 1.64f;
                }
                return new Color(r, g, b); //new Color((float)27/255, (float)11/255, (float)153/255);
            }
        }
        //Thanks Niko
        public static Vector2 RotateAroundPoint(Vector2 center, Vector2 offset, float degrees)
        {
            offset += center;
            float radians = (float)(degrees * Math.PI / 180.0);
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            float x = (offset.x - center.x) * cos - (offset.y - center.y) * sin + center.x;
            float y = (offset.x - center.x) * sin + (offset.y - center.y) * cos + center.y;
            return new Vector2(x, y);
        }
    }
}