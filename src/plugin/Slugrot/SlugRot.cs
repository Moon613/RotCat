using UnityEngine;
using RWCustom;
using System;
using Random = UnityEngine.Random;
using SlugBase;
using System.Runtime.CompilerServices;

namespace Chimeric
{
    public class SlugRot
    {
        private static ConditionalWeakTable<RainWorldGame, StrongBox<int>> SpawnFlagTimer = new ConditionalWeakTable<RainWorldGame, StrongBox<int>>();
        public static void Apply() {
            On.Player.Update += PlayerUpdate;
            On.Player.Die += PlayerDie;
            On.RainWorldGame.ctor += RainWorldGame_ctor;
        }
        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager) {
            orig(self, manager);
            if (self.session is StoryGameSession currentSession && currentSession.game.StoryCharacter.value == Plugin.ROT_NAME) {
                SpawnFlagTimer.Add(self, new StrongBox<int>(0));
            }
        }
        public static void PlayerUpdate(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            #region Cycle 0 spawn intro for campaign
            if (self.room != null && self.room.game.session is StoryGameSession && self.room.game.rainWorld.progression?.currentSaveState.saveStateNumber.value == Plugin.ROT_NAME && self.room.game.rainWorld.progression.currentSaveState.cycleNumber == 0 && SpawnFlagTimer.TryGetValue(self.room.game, out var localTimer) && localTimer.Value <= 320) {
                self.SuperHardSetPosition(self.room.MiddleOfTile(29 + 4*self.playerState.playerNumber, localTimer.Value==0? 75 : 120));
                foreach (BodyChunk chunk in self.bodyChunks) {
                    chunk.vel = localTimer.Value < 320? Vector2.zero : new Vector2(0f, -7.5f);
                }
                self.Stun(170);
                if (self.playerState.playerNumber == 0) { localTimer.Value++; }
            }
            #endregion
            if (Plugin.playerCWT.TryGetValue(self, out var something) && something.isRot) {
                //Debug.Log(self.mainBodyChunk.pos);
                if (Input.GetKey(KeyCode.Y)) {
                    // SlugBase.Assets.CustomScene.SetSelectMenuScene((self.abstractCreature.world.game.session as StoryGameSession)?.saveState, new ("slugrot_sleep"));
                    // Debug.Log("Set new menu");
                    Debug.Log($"Position: {self.mainBodyChunk.pos}");
                }
                self.scavengerImmunity = 9999;  //Might want to add a system that calculates this based on rep, since you should be able to become friends if you want
                something.overrideControls = Input.GetKey(ChimericOptions.TentMovementLeft.Value) || Input.GetKey(ChimericOptions.TentMovementRight.Value) || Input.GetKey(ChimericOptions.TentMovementDown.Value) || Input.GetKey(ChimericOptions.TentMovementUp.Value);
                if (something.grabWallCooldown > 0) { //Doesn't do anything right now, need to get it to play nice with the logic first     //New note, this will probably go unused.
                    something.grabWallCooldown -= 0.5f;
                }
                if (something.hearingCooldown > 0) {
                    something.hearingCooldown--;
                }
                if (something.smolHearingCooldown > 0) {
                    something.smolHearingCooldown--;
                }
                if (something.swimTimer > 0) {
                    something.swimTimer--;
                    Debug.Log($"Lowered rotcat swimTimer {something.swimTimer}");
                }
                if (something.stuckCreature != null) {
                    //Debug.Log($"Caught Creature is {something.stuckCreature.PhysObject}");
                    something.stuckCreature.Update(self.evenUpdate);
                    if (/*Input.GetKey(ChimericOptions.GrabButton.Value)*/ self.input[0].pckp && something.stuckCreature != null && something.stuckCreature?.PhysObject != null && self.Consious && something.swimTimer == 0) {
                        something.stuckCreature.ChangeOverlap(true);
                        something.stuckCreature.Deactivate();
                        #pragma warning disable CS8625
                        something.stuckCreature.PhysObject = null;
                        something.stuckCreature.Player = null;
                        #pragma warning restore CS8625
                        something.stuckCreature = null;
                        something.swimTimer += 10;
                        self.room?.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Release_Creature, something.tentacles[0].EndOfTentaclePos);
                        Debug.Log("Dropped creature");
                    }
                }
                //This whole bit controls the length of the tentacles
                Functions.TentacleRetraction(self, something);

                if ((Input.GetKey(ChimericOptions.TentMovementEnable.Value) || Input.GetKey(ChimericOptions.TentMovementAutoEnable.Value)) && !self.dead) {
                    Functions.PrimaryTentacleAndPlayerMovement(something, self);
                    float startPos = Functions.FindPos(something.overrideControls, self);    //Finds the position around the player to start, based on Sine and Cosine intervals of pi/4
                    Functions.TentaclesFindPositionToGoTo(something, self, startPos);
                    Functions.MoveTentacleToPosition(something, self);
                }
                else {
                    something.automateMovement = false;
                }
                #region What to do if the player is not dead but also not using tentacles for movement
                if (!something.automateMovement && !self.dead) {
                    for (int i = (Input.GetKey(ChimericOptions.TentMovementEnable.Value) || Input.GetKey(ChimericOptions.TentMovementAutoEnable.Value))?1:0; i < something.tentacles.Length; i++) {
                        float xPos=0, yPos=20;
                        switch (i)
                        {
                            case 0:
                                xPos = -20 - (something.stuckCreature?.PhysObject == null? 0f : something.stuckCreature.PhysObject.realizedObject.bodyChunks[something.stuckCreature.ConnectedChunk].rad - 10);
                                yPos = 17 + (something.stuckCreature?.PhysObject == null? 0f : something.stuckCreature.PhysObject.realizedObject.bodyChunks[something.stuckCreature.ConnectedChunk].rad + 10);
                                break;
                            case 1:
                                xPos = 18;
                                yPos = -2;
                                break;
                            case 2:
                                xPos = 15;
                                yPos = 19;
                                break;
                            case 3:
                                xPos = -17;
                                yPos = -4;
                                break;
                        }
                        if (self.bodyMode == Player.BodyModeIndex.CorridorClimb) { xPos *= 0.1f; yPos = (i==0||i==2)? yPos-13:yPos; }
                        something.tentacles[i].pList[something.tentacles[i].TipIndex-1].pos = Vector2.Lerp(
                        Functions.RotateAroundPoint(self.mainBodyChunk.pos, new Vector2(xPos, yPos), -Custom.VecToDeg(self.mainBodyChunk.Rotation)), 
                        Functions.RotateAroundPoint(self.mainBodyChunk.lastPos, new Vector2(xPos, yPos), -Custom.VecToDeg(self.mainBodyChunk.Rotation)), 0.5f);
                    }
                }
                #endregion

                //Physics for the individual points and Rot Bulbs
                foreach (var tentacle in something.tentacles) {
                    foreach (Point p in tentacle.pList)
                    {
                        p.Update(something, self, tentacle);
                    }
                
                    //base.Logger.LogDebug("Offset here");
                    foreach (Circle spot in tentacle.cList) {
                        spot.Update();
                    }
                }
                //Physics for the sticks of all tentacles, which affects the points
                int numIterations = self.room?.abstractRoom.name == "SB_L01" ? 1 : 10;
                for (int i = 0; i < numIterations; i++) {
                    Tentacle[][] totalTentacles = new Tentacle[][]{something.tentacles, something.decorativeTentacles};
                    Functions.StickCalculations(totalTentacles, self);
                }
            }
        }
        public static void RotCtor(Player self, PlayerEx something) {
            self.slugcatStats.runspeedFac = 0.65f;
            self.slugcatStats.corridorClimbSpeedFac = 1.2f;
            //self.abstractCreature.tentacleImmune = true;
            something.totalCircleSprites = something.circleAmmount * 4;
            something.tentacles[0] = new Tentacle();
            something.tentacles[1] = new Tentacle();
            something.tentacles[2] = new Tentacle();
            something.tentacles[3] = new Tentacle();
            something.decorativeTentacles[0] = new Tentacle();
            something.decorativeTentacles[1] = new Tentacle();
            something.randomPosOffest = new Vector2[something.decorativeTentacles.Length*2];
            for (int i = 0; i < something.randomPosOffest.Length; i++) {    //Decides where to place the ends of the decorative tentacles
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
                    tentacle.pList[i] = new Point(self.graphicsModule, new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-1-i), i==0);
                    tentacle.pList[i].lastPos = new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-i);
                }
                tentacle.sList = new PointConnection[something.segments-1];
                for (int i = 0; i < tentacle.TipIndex-1; i++) {
                    tentacle.sList[i] = new PointConnection(tentacle.pList[i], tentacle.pList[i+1], 9.25f);
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
                for (int i = 0; i < tentacle.TipIndex; i++) {
                    tentacle.pList[i] = new Point(self.graphicsModule, new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-1-i), i==0||i==(tentacle.TipIndex-1)?true:false);
                }
                tentacle.sList = new PointConnection[tentacle.TipIndex-1];
                for (int i = 0; i < tentacle.TipIndex-1; i++) {
                    tentacle.sList[i] = new PointConnection(tentacle.pList[i], tentacle.pList[i+1], 5f);
                }
                tentacle.decoPushDirection = Vector2.right * Random.Range(-0.7f,0.7f) * 0;  //Curently unused because it doesn't look the best. To enable change the 0 to something.decorativeTentacles[i].decoPushDirection *I think
            }
        }
        public static void PlayerDie(On.Player.orig_Die orig, Player self) {
            if (self != null && Plugin.playerCWT.TryGetValue(self, out var something) && something.isRot && something.stuckCreature != null) {
                something.stuckCreature.ChangeOverlap(true);
                something.stuckCreature.Deactivate();
                #pragma warning disable CS8625
                something.stuckCreature.PhysObject = null;
                something.stuckCreature.Player = null;
                #pragma warning restore CS8625
                something.stuckCreature = null;
            }
            orig(self);
        }
    }
}