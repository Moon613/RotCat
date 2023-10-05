using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using static Pom.Pom;

namespace Chimeric;
public class POMDarkness
{
	internal class Vignette : CosmeticSprite
	{
		private readonly PlacedObject placedObject;
		public Vignette(PlacedObject pObj, Room room) {
			this.room = room;
			this.placedObject = pObj;
			room.AddObject(this);
		}
		public override void Update(bool eu) {
			base.Update(eu);
			// Debug.Log("POM Object is here");
		}
		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White", true);
			sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["Red"];
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Bloom"));
			sLeaser.sprites[0].MoveToFront();
		}
		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

			ManagedData objData = (ManagedData)placedObject.data;
			Vector2 vector = placedObject.pos-rCam.pos;
			sLeaser.sprites[0]._localVertices[0].Set(objData.GetValue<Vector2>("TopRight").x+vector.x,objData.GetValue<Vector2>("TopRight").y+vector.y);
			sLeaser.sprites[0]._localVertices[1].Set(objData.GetValue<Vector2>("TopLeft").x+vector.x,objData.GetValue<Vector2>("TopLeft").y+vector.y);
			sLeaser.sprites[0]._localVertices[2].Set(objData.GetValue<Vector2>("BottomLeft").x+vector.x,objData.GetValue<Vector2>("BottomLeft").y+vector.y);
			sLeaser.sprites[0]._localVertices[3].Set(objData.GetValue<Vector2>("BottomRight").x+vector.x,objData.GetValue<Vector2>("BottomRight").y+vector.y);

			if (objData.GetValue<bool>("FollowPlayer")) {
				Player player = room.PlayersInRoom.Find(p => p.playerState.playerNumber==0);
				Debug.Log($"Thing: {placedObject.pos}, {player.mainBodyChunk.pos}");
				Vector2 vector1 = new Vector2((player.mainBodyChunk.pos.x-(placedObject.pos.x-objData.GetValue<Vector2>("BottomLeft").x)) / Vector2.Distance(objData.GetValue<Vector2>("BottomLeft"), objData.GetValue<Vector2>("BottomRight")), (player.mainBodyChunk.pos.y-(placedObject.pos.y+objData.GetValue<Vector2>("BottomLeft").y)) / Vector2.Distance(objData.GetValue<Vector2>("BottomLeft"), objData.GetValue<Vector2>("TopLeft")));
				Debug.Log($"Thing2: {vector1}");
				Shader.SetGlobalVector("_VignettePlayerPos", vector1);
			}
			else {
				Shader.SetGlobalVector("_VignettePlayerPos", new Vector2(objData.GetValue<float>("CenterX"), objData.GetValue<float>("CenterY")));
			}

			sLeaser.sprites[0].color = new Color(objData.GetValue<float>("OuterColorR"),objData.GetValue<float>("OuterColorG"),objData.GetValue<float>("OuterColorB"),objData.GetValue<float>("OuterColorA"));
			Shader.SetGlobalColor("_VignetteInnerColor", new Color(objData.GetValue<float>("InnerColorR"), objData.GetValue<float>("InnerColorG"), objData.GetValue<float>("InnerColorB"), objData.GetValue<float>("InnerColorA")));
		}
	}
    internal static void RegisterDarkness() {
		List<ManagedField> fields = new List<ManagedField> {
			new FloatField("OuterColorR", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Outer Red"),
			new FloatField("OuterColorG", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Outer Green"),
			new FloatField("OuterColorB", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Outer Blue"),
			new FloatField("OuterColorA", 0f, 1f, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Outer Alpha"),
			
			new FloatField("InnerColorR", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Inner Red"),
			new FloatField("InnerColorG", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Inner Green"),
			new FloatField("InnerColorB", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Inner Blue"),
			new FloatField("InnerColorA", 0f, 1f, 0f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Inner Alpha"),
			
			new FloatField("CenterX", 0f, 1f, 0.5f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Vignette X"),
			new FloatField("CenterY", 0f, 1f, 0.5f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Vignette Y"),

			new Vector2Field("TopLeft", new Vector2(-40,40), Vector2Field.VectorReprType.line, "Top Left Corner"),
			new Vector2Field("TopRight", new Vector2(40,40), Vector2Field.VectorReprType.line, "Top Right Corner"),
			new Vector2Field("BottomLeft", new Vector2(-40,-40), Vector2Field.VectorReprType.line, "Bottom Left Corner"),
			new Vector2Field("BottomRight", new Vector2(40,-40), Vector2Field.VectorReprType.line, "Bottom Right Corner"),

			new BooleanField("FollowPlayer", false, ManagedFieldWithPanel.ControlType.button, "Track Player")
		};
        RegisterFullyManagedObjectType(fields.ToArray(), typeof(Vignette), "Vignette", "Chimeric");
    }
}