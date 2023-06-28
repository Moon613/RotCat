using System.Diagnostics.CodeAnalysis;

namespace Chimeric
{
    public sealed class CreatureTemplateType {
        [AllowNull] public static CreatureTemplate.Type BabyAquapede = new(nameof(BabyAquapede), true);
    }
    public static class SandboxUnlockID {
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID BabyAquapede = new(nameof(BabyAquapede), true);
    }
    public static class ConversationID {
        public static void RegisterValues() {
            ConversationID.PebblesMeetRot = new Conversation.ID("PebblesMeetRot", true);
        }
        [AllowNull] public static Conversation.ID PebblesMeetRot;
    }
    public static class SoundEnums
    {
        public static void RegisterValues() {
            SoundEnums.Silence = new SoundID("Silence", true);
        }
        [AllowNull] public static SoundID Silence;
    }
}