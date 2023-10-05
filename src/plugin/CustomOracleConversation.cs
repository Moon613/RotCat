using System.Collections.Generic;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using SlugBase.Features;
using UnityEngine;
using static SlugBase.Features.FeatureTypes;
using static System.Reflection.BindingFlags;
using System;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;
using IL.RWCustom;

namespace Chimeric
{
    public static class ConversationOverrides
    {
        private static ConditionalWeakTable<Oracle, StrongBox<int>> OracleCWT = new ConditionalWeakTable<Oracle, StrongBox<int>>();
        public static readonly GameFeature<bool> CustomConversations = GameBool("CustomConversations");
        public static void Hooks()
        {
            new Hook(typeof(SSOracleRotBehavior).GetMethod("get_EyesClosed", Public | NonPublic | Instance), (Func<SSOracleRotBehavior, bool> orig, SSOracleRotBehavior self) => (self.oracle.stun > 0 && self.oracle.room.game.session is StoryGameSession session && session.saveStateNumber.value == Plugin.ROT_NAME) || orig(self));
            On.Oracle.ctor += OracleCtor;
            On.Oracle.Update += OracleUpdate;

            // On.GhostConversation.AddEvents += GhostOVerride;
            On.MoreSlugcats.SSOracleRotBehavior.RMConversation.AddEvents += PebblesOverride;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonOverride;
        }
        public static void OracleCtor(On.Oracle.orig_ctor orig, Oracle self, AbstractPhysicalObject abstractPhysicalObject, Room room)
        {
            orig(self, abstractPhysicalObject, room);
            OracleCWT.Add(self, new StrongBox<int>(0));
        }
        public static void OracleUpdate(On.Oracle.orig_Update orig, Oracle self, bool eu)
        {
            orig(self, eu);
            if (OracleCWT.TryGetValue(self, out var Counter) && self.oracleBehavior is SSOracleRotBehavior rotBehavior && rotBehavior.conversation?.events != null && rotBehavior.conversation.id == MoreSlugcatsEnums.ConversationID.Pebbles_RM_FirstMeeting) {
                Counter.Value++;
                const int StartPain = 265;
                if (Counter.Value >= StartPain - 35 && Counter.Value <= StartPain) {
                    for (int i = 1; i < 3; i++) {
                        bool lockToHorizontal = Random.Range(0f,1f) > 0.5f;
                        Spark spark = new Spark(new Vector2(lockToHorizontal? (Random.Range(0f, 1f) > 0.5f? 1225:1795) : Random.Range(1225, 1795), !lockToHorizontal? (Random.Range(0f, 1f) > 0.5f? 805:1375) : Random.Range(800,1375)), RWCustom.Custom.rotateVectorDeg(Vector2.down * (i * 2.5f), Random.Range(0f, 360f)), Color.yellow, null, 10, 30);
                        self.room.AddObject(spark);
                        
                        if (i == 1 && Counter.Value % 2 == 0) {
                            lockToHorizontal = Random.Range(0f,1f) > 0.5f;
                            Vector2 startPos = new Vector2(lockToHorizontal? (Random.Range(0f, 1f) > 0.5f? 1225:1795) : Random.Range(1225, 1795), !lockToHorizontal? (Random.Range(0f, 1f) > 0.5f? 805:1375) : Random.Range(800,1375));
                            Vector2 endPos = new Vector2(!lockToHorizontal? (Random.Range(0f, 1f) > 0.5f? 1225:1795) : Random.Range(1225, 1795), lockToHorizontal? (Random.Range(0f, 1f) > 0.5f? 805:1375) : Random.Range(800,1375));
                            LightningBolt lightning = new LightningBolt(startPos, endPos, 0, 0.2f, 1, 0, 0f, false);
                            lightning.intensity = 1f;
                            lightning.color = new Color(Random.Range(0.85f, 1f), Random.Range(0f, 0.196f), Random.Range(0f, 0.456f));
                            self.room.AddObject(lightning);
                            self.room.PlaySound(SoundID.Zapper_Zap, lightning.from, 0.6f, 1f);
                            self.room.PlaySound(SoundID.Zapper_Disrupted_LOOP, lightning.target, 0.6f, 1f);
                        }
                    }
                    self.bodyChunks[0].vel += new Vector2(Random.Range(-3.65f, 3.65f), Random.Range(-1f, 1f));
                }
                if (Counter.Value == StartPain) {
                    self.stun = 300;
                    self.dazed = 300;
                    rotBehavior.AirVoice(SoundID.SS_AI_Talk_4);
                    self.room.PlaySound(SoundID.SL_AI_Pain_1, self.firstChunk, false, 0.7f, 0.5f);
                    self.bodyChunks[1].vel.x -= 5f;
                    self.room.AddObject(new PebblesPanicDisplay(self));
                }
                if (Counter.Value == StartPain + 30) {
                    rotBehavior.AirVoice(SoundID.SS_AI_Talk_1);
                    self.room.PlaySound(SoundID.SL_AI_Pain_2, self.firstChunk, false, 0.87f, 0.45f);
                }
            }
        }
        public static void MoonOverride(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            orig(self);
            if(CustomConversations.TryGet(self.myBehavior.oracle.room.game,out bool value) && value)
            {
                if(self.id == Conversation.ID.MoonFirstPostMarkConversation)
                {
                    self.events = new List<Conversation.DialogueEvent>();
                    switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5)) //this gets the number of neurons left for moon.
                    {
                        case 0:
                            break;
                        case 1:
                            self.events.Add(new Conversation.TextEvent(self, 0, "...", 0));
                            return;
                        case 2:
                            self.events.Add(new Conversation.TextEvent(self, 0, "... 2 NEURONS LEFT!", 0));
                            return;
                        case 3:
                            self.events.Add(new Conversation.TextEvent(self, 0, "3 NEURON CONVO!", 0));
                            return;
                        case 4:
                            self.events.Add(new Conversation.TextEvent(self, 0, "4 NEURON CONVO!!!", 0));
                            return;
                        case 5:
                            self.events.Add(new Conversation.TextEvent(self, 0, "Hewwo Wittle Cweatuwe!", 0));

                            return;

                    }
                }
            }
        }
        public static void PebblesOverride(On.MoreSlugcats.SSOracleRotBehavior.RMConversation.orig_AddEvents orig, SSOracleRotBehavior.RMConversation self)
        {
            orig(self);
            if(CustomConversations.TryGet(self.owner.oracle.room.game,out bool custom) && custom && self.owner.oracle.room.game.session is StoryGameSession session && session.saveState.saveStateNumber.value == Plugin.ROT_NAME) {
                if (self.id == MoreSlugcatsEnums.ConversationID.Pebbles_RM_FirstMeeting) //Pebbles_White is the default convo as far as i can see.
                {
                    self.events = new List<Conversation.DialogueEvent>() {
                        new Conversation.TextEvent(self, 5, "...", 103),
                        new Conversation.TextEvent(self, 5, "So it...<LINE>would seeeeeeeee", 20),
                        new Conversation.TextEvent(self, 0, "...I am sorry, but--", 30),
                        new Conversation.TextEvent(self, 0, "---", 40),
                        new Conversation.TextEvent(self, 0, "------<LINE>..oh...", 15),
                        new Conversation.TextEvent(self, 0, "...you...<LINE>.r..herree...", 15),
                        new Conversation.TextEvent(self, 0, "........", 45)
                    };
                    if (OracleCWT.TryGetValue(self.owner.oracle, out var Counter)) { Counter.Value = 0; }

                    //heres some things you might want!

                    //Make five pebbles wait for the player to be still:
                    // self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 20));

                    // convos for already existing mark
                    // if (self.owner.playerEnteredWithMark)
                    // {
                    //     self.events.Add(new Conversation.TextEvent(self, 0, "You already have a mark!", 0));
                    // }
                    // else
                    // {
                    //     self.events.Add(new Conversation.TextEvent(self, 0, "This conversation requires me giving you the mark!", 0));
                    // }

                    //convo if you've gone thru mem arrays:
                    // if (self.owner.oracle.room.game.IsStorySession && self.owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.memoryArraysFrolicked)
                    // {
                    //     self.events.Add(new Conversation.TextEvent(self, 0, "Pwease don't go in my memowy awways", 0));
                    // }

                    //convo for slugpups in room!
                    // if(ModManager.MSC && self.owner.CheckSlugpupsInRoom()){
                    //     self.events.Add(new Conversation.TextEvent(self, 0, "Scuppies in room!", 0));
                    // }

                    //convo for creauters in room
                    // if (ModManager.MMF && self.owner.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                    // {
                    //     self.events.Add(new Conversation.TextEvent(self, 0, "Creature in room!", 0));
                    // }
                }
            }
        }
        public static void GhostOVerride(On.GhostConversation.orig_AddEvents orig, GhostConversation self)
        {
            orig(self);
            if (CustomConversations.TryGet(self.ghost.room.game,out bool value) && value) //check if this game has custom conversations
            {
                if(self.id == Conversation.ID.Ghost_CC) //check for which ghost this is
                {
                    self.events = new List<Conversation.DialogueEvent>(); //remove all events already existing
                    self.events.Add(new Conversation.TextEvent(self, 0, "This is a test!", 0)); //add in your own text events!
                    self.events.Add(new Conversation.TextEvent(self, 0, "This should be the second text!", 0));
                }
            }
        }
        public class PebblesPanicDisplay : UpdatableAndDeletable
        {
            Oracle oracle;
            private int[] timings;
            private int timer;
            public PebblesPanicDisplay(Oracle oracle)
            {
                this.oracle = oracle;
                timings = new int[] {
                    140,
                    190,
                    210,
                    240,
                    320
                };
                Debug.Log(oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights)?.amount);
                Debug.Log(oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness)?.amount);
                Debug.Log(oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast)?.amount);
            }
            public override void Update(bool eu)
            {
                base.Update(eu);
                timer++;
                if (timer == 1) {
                    if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) == null)
                    {
                        oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.DarkenLights, 0f, false));
                    }
                    if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) == null)
                    {
                        oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Darkness, 0f, false));
                    }
                    if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
                    {
                        oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 0f, false));
                    }
                    oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_Off, 0f, 1f, 1f);
                }
                if (timer < timings[0]) {
                    float t = (float)timer / timings[0];
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0f, 0.6f, t);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0f, 0.4f, t);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0f, 0.3f, t);
                }
                if (timer == timings[0]) {
                    oracle.arm.isActive = false;
                    oracle.setGravity(0.9f);
                    oracle.stun = 9999;
                    // oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Moon_Panic_Attack, 0f, 0.85f, 1f);
                    for (int i = 0; i < oracle.room.game.cameras.Length; i++)
                    {
                        if (oracle.room.game.cameras[i].room == oracle.room && !oracle.room.game.cameras[i].AboutToSwitchRoom)
                        {
                            oracle.room.game.cameras[i].ScreenMovement(null, Vector2.zero, 15f);
                        }
                    }
                }
                if (timer == (timings[1] + timings[2]) / 2) {
                    oracle.arm.isActive = false;
                    // oracle.room.PlaySound((Random.value < 0.5f) ? SoundID.SL_AI_Pain_1 : SoundID.SL_AI_Pain_2, 0f, 0.5f, 1f);
                    // chatLabel = new OracleChatLabel(oracle.oracleBehavior);
                    // chatLabel.pos = new Vector2(485f, 360f);
                    // chatLabel.NewPhrase(99);
                    oracle.setGravity(0.9f);
                    oracle.stun = 9999;
                    // oracle.room.AddObject(chatLabel);
                }
                if (timer > timings[1] && timer < timings[2] && timer % 5 == 0) {
                    oracle.room.ScreenMovement(null, new Vector2(0f, 0f), 2.5f);
                    // for (int j = 0; j < 6; j++)
                    // {
                    //     if (Random.value < 0.5f)
                    //     {
                    //         oracle.room.AddObject(new OraclePanicDisplay.PanicIcon(new Vector2((float)Random.Range(230, 740), (float)Random.Range(100, 620))));
                    //     }
                    // }
                }
                if (timer >= timings[2] && timer <= timings[3]) {
                    oracle.room.ScreenMovement(null, new Vector2(0f, 0f), 1f);
                }
                if (timer == timings[3]) {
                    // chatLabel.Destroy();
                    oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_On, 0f, 1f, 1f);
                }
                if (timer > timings[3]) {
                    float t2 = (float)(timer - timings[3]) / (int)(timings[0]/2.1f);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0.6f, 0f, t2);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0.4f, 0f, t2);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0.3f, 0f, t2);
                }
                if (timer == timings[3] + (int)(timings[0]/2.1f)) {
                    oracle.setGravity(0f);
                    oracle.arm.isActive = true;
                    oracle.stun = 0;
                }
                if (timer > timings[3] + (int)(timings[0]/2.1f) && timer < timings[4]) {
                    oracle.bodyChunks[0].vel += new Vector2(Random.Range(-1.7f, 1.7f), Random.Range(-1.2f, 1.2f));
                    oracle.bodyChunks[0].vel *= 0.3f;
                }
                if (timer >= timings[4]) {
                    oracle.room.roomSettings.effects.RemoveAll(eff => eff.type == RoomSettings.RoomEffect.Type.DarkenLights || eff.type == RoomSettings.RoomEffect.Type.Darkness || eff.type == RoomSettings.RoomEffect.Type.Contrast);
                    Destroy();
                }
            }
        }
    }
}