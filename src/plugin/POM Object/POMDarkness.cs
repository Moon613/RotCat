using DevInterface;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using RWCustom;
using static Pom.Pom;
using System;

namespace Chimeric
{
public static class POMDarkness
{
	// Juuuuust an object, yet, we can place it. Data and UI are generated automatically
	public class Dark : UpdatableAndDeletable
	{
		private readonly PlacedObject placedObject;

		public Dark(PlacedObject pObj, Room room)
		{
			this.room = room;
			this.placedObject = pObj;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.clock % 100 == 0)
				Debug.Log("Dark vf1.x is " + ((ManagedData)placedObject.data).GetValue<Vector2>("vf1").x); // This is how you access those fields you created when using ManagedData directly.
		}
	}
    internal static void RegisterDarkness()
    {
		// Registers a type with a loooooot of fields
		List<ManagedField> fields = new List<ManagedField>
		{
			new FloatField("f1", 0f, 1f, 0.2f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Float Slider"),
			new FloatField("f2", 0f, 1f, 0.5f, 0.1f, ManagedFieldWithPanel.ControlType.button, "Float Button"),
			new FloatField("f3", 0f, 1f, 0.8f, 0.1f, ManagedFieldWithPanel.ControlType.arrows, "Float Arrows"),
			new FloatField("f4", 0f, 1f, 0.8f, 0.1f, ManagedFieldWithPanel.ControlType.text, "Float Text"),

			new BooleanField("b1", false, ManagedFieldWithPanel.ControlType.slider, "Bool Slider"),
			new BooleanField("b2", true, ManagedFieldWithPanel.ControlType.button, "Bool Button"),
			new BooleanField("b3", false, ManagedFieldWithPanel.ControlType.arrows, "Bool Arrows"),
			new BooleanField("b4", true, ManagedFieldWithPanel.ControlType.text, "Bool Text"),

			new ExtEnumField<PlacedObject.Type>("e1", PlacedObject.Type.None, new PlacedObject.Type[] { PlacedObject.Type.BlueToken, PlacedObject.Type.GoldToken }, ManagedFieldWithPanel.ControlType.slider, "Enum Slider"),
			new ExtEnumField<PlacedObject.Type>("e2", PlacedObject.Type.Mushroom, null, ManagedFieldWithPanel.ControlType.button, "Enum Button"),
			new ExtEnumField<PlacedObject.Type>("e3", PlacedObject.Type.SuperStructureFuses, null, ManagedFieldWithPanel.ControlType.arrows, "Enum Arrows"),
			new ExtEnumField<PlacedObject.Type>("e4", PlacedObject.Type.GhostSpot, null, ManagedFieldWithPanel.ControlType.text, "Enum Text"),

			new IntegerField("i1", 0, 10, 1, ManagedFieldWithPanel.ControlType.slider, "Integer Slider"),
			new IntegerField("i2", 0, 10, 2, ManagedFieldWithPanel.ControlType.button, "Integer Button"),
			new IntegerField("i3", 0, 10, 3, ManagedFieldWithPanel.ControlType.arrows, "Integer Arrows"),
			new IntegerField("i4", 0, 10, 3, ManagedFieldWithPanel.ControlType.text, "Integer Text"),

			new StringField("str1", "your text here", "String"),

			new Vector2Field("vf1", Vector2.one, Vector2Field.VectorReprType.line),
			new Vector2Field("vf2", Vector2.one, Vector2Field.VectorReprType.circle),
			new Vector2Field("vf3", Vector2.one, Vector2Field.VectorReprType.rect),

			new IntVector2Field("ivf1", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.line),
			new IntVector2Field("ivf2", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.tile),
			new IntVector2Field("ivf3", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.fourdir),
			new IntVector2Field("ivf4", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.eightdir),
			new IntVector2Field("ivf5", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.rect)
		};

		try {
        	RegisterFullyManagedObjectType(fields.ToArray(), typeof(Dark), "Darkness", "POM examples");
		} catch (Exception err) {
			Debug.LogError($"Uh oh error:\n{err}");
			Debug.LogError($"And {fields.ToArray()}");
		}
    }
}
}