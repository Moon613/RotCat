

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Chimeric;
public class PlayerEx
{
    public Tentacle[] tentacles = new Tentacle[4];  //Array that stores the logic bits of the movement tentacles
    public Tentacle[] decorativeTentacles = new Tentacle[2];    //Array that stores the logic bits of the decorative tentacles
    public int initialDecoLegSprite;    //The position of the initial decorative tentacle in the sLeaser array
    public int endOfsLeaser;
    public float retractionTimer = 80;
    public Vector2 previousPosition;    //The previous position of the player, used to control if the tentacles should be retracted based on movement
    public int initialLegSprite;    //The position of the initial movement tentacle in the sLeaser array
    public int segments = 25;   //The amount of points in the movement tentacles.Might be aassumed as 10 in some places, can't remember, so changing this might break something
    public int decorationSegments = 10; //The amount of points in the decorative tentacles. Might be aassumed as 10 in some places, can't remember, so changing this might break something
    public Vector2 potentialGrapSpot;   //IT'S NOT USED LMAOOOOOO
    public int totalCircleSprites;  //Assigned later, keeps track of the total amount of circle sprites on all the tentacles (Is used solely for resizing the sLeaser array)
    public bool automateMovement = false;   //Determines whether the tentacles will guide themselves toward a wall/pole
    public float grabWallCooldown = 0;  //Currently unused, honestly can't remember what I wanted to do with it
    [AllowNull] public Vector2[] randomPosOffest;   //Currently not used, applies a force the the 2 decorative tentacles behind the scug
    public bool overrideControls = false;   //If the player starts using custom movement keys instead of the default scug movement, this is set to true and false otherwise
    public int circleAmmount = 19;  //Change this to alter the amount of circle sprites that can be made
    public int initialCircleSprite;     //Stores the position of the initial tantacle circle sprite in the sprite list
    public int initialBodyRotSprite;    //Stores the position of the initial bodyRot sprite in the sprite list
    public int bodyRotSpriteAmount = 8;     //Change this whenever I change the amount of Rot sprites on the body
    [AllowNull] public BodyRot[] rList;     //An array that stores the logic for the body rot sprites
    public Color rotEyeColor;   //Should store and control the color of the X sprites from the slugbase custom color
    [AllowNull] public FAtlas faceAtlas;
    public bool eating = false;
    public float timer = 0f;
    public int hearingCooldown = 0;
    public int smolHearingCooldown = 0;
    public bool canPlayShockSound = true;
    public int swimTimer = 0;
    public float slowX = 0;
    public float slowY = 0;
    public Vector2 prevInput = Vector2.zero;
    public int initialFinSprite;
    public List<Fin> fList = new List<Fin>();
    public int timeInWater;
    public int timeInWaterUpTo80;
    [AllowNull] public AbstractOnTentacleStick stuckCreature;
    public bool crawlToRoll;
    public bool isRot = false;  //Is set to true if the Slugrot character is selected, so it doesn't apply anything to non-rot characters
    public bool isDynamo = false;
    public bool isDragon = false;
}
public class SparkEx {
    public bool isHearingSpark;
    public SparkEx(bool flag) {
        this.isHearingSpark = flag;
    }
}
public class CreatureEx {
    public bool isBeingEaten;
    public bool redrawRotSprites;
    public bool addNewSprite;
    public int numOfCosmeticSprites;
    public int maxNumOfSprites;
    public bool shouldFearDynamo = false;
    public int fearTime = -1;
    public List<EatingRot> yummersRotting = new List<EatingRot>();
}
///<summary>The pivot points for the tentacles, where they can bend. To be replaced with tailSegments</summary>
public class Point : BodyPart
{
    public bool locked = false;
    public Point(GraphicsModule ow, Vector2 position, bool locked) : base (ow) {
        this.pos = position;
        this.vel = Vector2.zero;
        this.locked = locked;
        this.rad = 2f;
        this.surfaceFric = 0.5f;
        this.airFriction = 0.5f;
    }
    public void Update(PlayerEx something, Player self, Tentacle tentacle) {
        base.Update();
        if (Array.IndexOf(tentacle.pList, this) == tentacle.pList.Length-1 && (
                (Input.GetKey(ChimericOptions.tentMovementEnable.Value) || Input.GetKey(ChimericOptions.tentMovementAutoEnable.Value)) && 
                something.tentacles[Array.IndexOf(something.tentacles, tentacle)].foundSurface && 
                ((Array.IndexOf(something.tentacles, tentacle) == 0) || something.automateMovement))) {  //If it is the very last point in the list, the tentacle tip
            this.locked = true;
        }
        else {
            this.locked = false;
        }
        if (!this.locked && self.room != null) {
            Vector2 positionBeforeUpdate = this.pos;
            this.pos += (this.pos - this.lastPos) * Random.Range(0.9f,1.1f);
            float distToWaterSurface = this.pos.y - self.room.FloatWaterLevel(this.pos.x) - 2f;  // Adjust positions, maybe subtract a bit extra
            this.pos += Vector2.down * self.room.gravity * Random.Range(0.15f,0.3f) * (distToWaterSurface < 0? Mathf.Clamp(distToWaterSurface/1.5f, -0.6f, -0.1f) : 1f);
            this.PushOutOfTerrain(self.room, this.pos);
            this.lastPos = positionBeforeUpdate;
        }
        if (Array.IndexOf(tentacle.pList, this) == 0) {
            this.pos = self.mainBodyChunk.pos;
        }
    }
}
///<summary>The connections between points</summary>
public class PointConnection {
    public Point pointA, pointB;
    public float length;
    public PointConnection(Point pointA, Point pointB, float length) {
        this.pointA = pointA;
        this.pointB = pointB;
        this.length = length;
    }
    public void Update(Player self) {
        Vector2 stickCenter = (this.pointA.pos + this.pointB.pos)/2;
        Vector2 stickDir = (this.pointA.pos - this.pointB.pos).normalized;
        if (!this.pointA.locked)
            this.pointA.pos = stickCenter + stickDir * this.length / 2;
            //this.pointA.PushOutOfTerrain(self.room, this.pointA.pos);
        if (!this.pointB.locked)
            this.pointB.pos = stickCenter - stickDir * this.length / 2;
            //this.pointB.PushOutOfTerrain(self.room, this.pointB.pos);
        }
}
public class Tentacle {
    [AllowNull] public Point[] pList;
    [AllowNull] public PointConnection[] sList;
    [AllowNull] public Circle[] cList;
    public Vector2 iWantToGoThere;
    public int isAttatchedToSurface = 0;
    public Vector2 decoPushDirection = new Vector2(0,0);
    public bool canPlaySound = true;
    public Vector2 targetPosition = new Vector2(0,0);
    public bool hasConnectionSpot = false;
    public bool foundSurface = true;
    public bool isPole = false;
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
    public void Update() {
        Vector2 direction = this.pointB.pos - this.pointA.pos;
        Vector2 dirNormalized = direction.normalized;
        Vector2 perpendicularVector = Custom.PerpendicularVector(direction);
        this.position = this.pointA.pos + (dirNormalized * this.offset.y) + (perpendicularVector * this.offset.x);
    }
    public Point pointA;
    public Point pointB;
    public Vector2 offset;
    public bool darkBackground;
    public bool brightBackground;
    public Vector2 position;
    public Vector2 newPosition;
    public float scale;
    public float scaleX;
    public float scaleY;
    public bool grayscale;
    public float rotation;
}
public class Fin {
    ///<summary>For creating fin sprites and holding their data. BodyChunk constructor. Negative rotation makes them angled down more, and positive is up. The X and Y scales are the other, because the initial sprite is vertical. swimRange is how much the fins rotate in either direction while swimming</summary>
    public Fin(FSprite connectedSprite, Vector2 posOffset, float additionalRotation, float scaleX, float scaleY, bool flipped = false, float foldRotation = -80f, List<float>? swimRange = null, float swimCycle = Mathf.PI/6f, float startSwimCycle = Mathf.PI/6f) {
        this.connectedSprite = connectedSprite;
        this.posOffset = posOffset;
        this.additionalRotation = additionalRotation;
        this.startAdditionalRotation = additionalRotation;
        this.flipped = flipped;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        this.foldRotation = foldRotation;
        if (swimRange == null) {
            this.swimRange = new List<float>(){0, 0};
        }
        else if (swimRange != null) {
            this.swimRange.AddRange(swimRange);
        }
        this.swimCycle = swimCycle;
        this.startSwimCycle = startSwimCycle;
    }
    ///<summary>For creating fin sprites and holding their data. tailSegment constructor, alternate version for attatching sprites to the tail. When adding offset to the tailSegments, y is inverted (negative is up and positive is down). Negative rotation makes them angled down more, and positive is up. The X and Y scales are the other, because the initial sprite is vertical. swimRange is how much the fins rotate in either direction while swimming</summary>
    public Fin(TailSegment connectedTailSegment, Vector2 posOffset, float additionalRotation, float scaleX, float scaleY, bool flipped = false, float foldRotation = -80f, List<float>? swimRange = null, float swimCycle = Mathf.PI/6f, float startSwimCycle = Mathf.PI/6f) {
        this.connectedTailSegment = connectedTailSegment;
        this.posOffset = posOffset;
        this.additionalRotation = additionalRotation;
        this.startAdditionalRotation = additionalRotation;
        this.flipped = flipped;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        this.foldRotation = foldRotation;
        if (swimRange == null) {
            this.swimRange = new List<float>(){0, 0};
        }
        else if (swimRange != null) {
            this.swimRange.AddRange(swimRange);
        }
        this.swimCycle = swimCycle;
        this.startSwimCycle = startSwimCycle;
    }
    [AllowNull] public TailSegment connectedTailSegment;
    [AllowNull] public FSprite connectedSprite;
    public Vector2 posOffset;
    public float additionalRotation;
    public bool flipped;
    public float scaleX;
    public float scaleY;
    public float startAdditionalRotation;
    public float foldRotation;
    public List<float> swimRange = new List<float>{};
    public float swimCycle;
    public float startSwimCycle;
    public float corriderTimer;
}
public class AbstractOnTentacleStick : AbstractPhysicalObject.AbstractObjectStick
{
    [AllowNull] public AbstractPhysicalObject Player
    {
        get
        {
            return this.A;
        }
        set
        {
            this.A = value;
        }
    }
    [AllowNull] public AbstractPhysicalObject PhysObject
    {
        get
        {
            return this.B;
        }
        set
        {
            this.B = value;
        }
    }
    public int ConnectedChunk;
    public AbstractOnTentacleStick(AbstractPhysicalObject player, AbstractPhysicalObject creature, int connectedChunk) : base(player, creature)
    {
        this.ConnectedChunk = connectedChunk;
    }
    public override string SaveToString(int roomIndex)
    {
        return string.Concat(new string[]
        {
            roomIndex.ToString(),
            "<stkA>sprOnBackStick<stkA>",
            this.A.ID.ToString(),
            "<stkA>",
            this.B.ID.ToString()
        });
    }
    public void Update(bool eu) {
        Creature? crit = (this.PhysObject.realizedObject as Creature);
        Player? player = (this.Player.realizedObject as Player);
        if (crit != null && player != null) {
            if (Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                crit.bodyChunks[this.ConnectedChunk].MoveFromOutsideMyUpdate(eu, something.tentacles[0].pList[something.tentacles[0].pList.Length-1].pos);
            }
            foreach (var chunk in crit.bodyChunks) {
                chunk.vel = Vector2.down;
            }
        }
    }
    public void ChangeOverlap(bool newOverlap)
    {
        Creature? crit = (this.PhysObject.realizedObject as Creature);
        Player? player = (this.Player.realizedObject as Player);
        //crit.CollideWithObjects = newOverlap;
        //crit.canBeHitByWeapons = newOverlap;
        if (crit != null) {
            crit.GoThroughFloors = newOverlap;
        }
        /*if (crit.graphicsModule == null || player.room == null)
        {
            return;
        }
        for (int i = 0; i < player.room.game.cameras.Length; i++)
        {
            player.room.game.cameras[i].MoveObjectToContainer(crit.graphicsModule, player.room.game.cameras[i].ReturnFContainer((!newOverlap) ? "Background" : "Midground"));
        }*/
    }
}