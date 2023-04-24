using System;

//namespace RotCat;

public static class Enums
{
    public static SSOracleBehavior.Action EmptyAction;
    public static SSOracleBehavior.SubBehavior.SubBehavID EmptySubBehaviorID;
    public static Conversation.ID EmptyConversationID;
    
    public static Conversation.ID PebblesFirstMeetID;
    public static Conversation.ID PebblesSecondMeetID;
    
    public static SSOracleBehavior.Action PebblesFirstMeetAction;
    public static SSOracleBehavior.Action PebblesSecondMeetAction;

    public static Conversation.ID MoonFirstMeet;
    
    public static void Register()
    {
        PebblesFirstMeetID = new Conversation.ID("hello_there_id");
        PebblesSecondMeetID = new Conversation.ID("bye_there_id", true);
        
        EmptyAction = new SSOracleBehavior.Action("action_empty", true);
        EmptySubBehaviorID = new SSOracleBehavior.SubBehavior.SubBehavID("subbheaviourid_empty", true);
        EmptyConversationID = new Conversation.ID("conversationid_empty", true);
    }
}

public class FunctionEvent : Conversation.DialogueEvent
{
    private Action func;

    public FunctionEvent(Conversation owner, Action func) : base(owner, 0)
    {
        this.func = func;
    }

    public override void Activate()
    {
        base.Activate();
        func();
    }
}

public class NullPebblesSubBehaviour : SSOracleBehavior.SubBehavior
{
    public NullPebblesSubBehaviour() : base(null, Enums.EmptySubBehaviorID) { }
}

public class NullPebblesConversationBehaviour : SSOracleBehavior.ConversationBehavior
{
    public NullPebblesConversationBehaviour() : base(null, Enums.EmptySubBehaviorID, Enums.EmptyConversationID) { }
}

public abstract class CustomPebblesConversation : SSOracleBehavior.PebblesConversation
{
    public class CustomPebblesConversationBehaviour : SSOracleBehavior.ConversationBehavior
    {
        public OracleChatLabel ChatLabel;

        public CustomPebblesConversation BoundConversation;

        public override bool CurrentlyCommunicating => base.CurrentlyCommunicating || !this.ChatLabel.finishedShowingMessage;

        public CustomPebblesConversationBehaviour(SSOracleBehavior owner, CustomPebblesConversation boundTo) : base(owner, Enums.EmptySubBehaviorID, Enums.EmptyConversationID)
        {
            BoundConversation = boundTo;
            this.ChatLabel = new OracleChatLabel(owner);
            this.oracle.room.AddObject(this.ChatLabel);
            this.ChatLabel.Hide();
            if (!ModManager.MMF || !owner.oracle.room.game.IsStorySession || !owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.memoryArraysFrolicked || this.oracle.room.world.rainCycle.timer <= this.oracle.room.world.rainCycle.cycleLength / 4) return;
            this.oracle.room.world.rainCycle.timer = this.oracle.room.world.rainCycle.cycleLength / 4;
            this.oracle.room.world.rainCycle.dayNightCounter = 0;
        }

        public CustomPebblesConversationBehaviour(SSOracleBehavior owner, SubBehavID subBehavID, ID convID, CustomPebblesConversation boundTo) : base(owner, subBehavID, convID)
        {
            BoundConversation = boundTo;
            this.ChatLabel = new OracleChatLabel(owner);
            this.oracle.room.AddObject(this.ChatLabel);
            this.ChatLabel.Hide();
            if (!ModManager.MMF || !owner.oracle.room.game.IsStorySession || !owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.memoryArraysFrolicked || this.oracle.room.world.rainCycle.timer <= this.oracle.room.world.rainCycle.cycleLength / 4) return;
            this.oracle.room.world.rainCycle.timer = this.oracle.room.world.rainCycle.cycleLength / 4;
            this.oracle.room.world.rainCycle.dayNightCounter = 0;
        }

        public override void Update()
        {
            base.Update();
            
            if (this.owner.conversation != null && this.owner.conversation.id == this.convoID && this.owner.conversation.slatedForDeletion)
            {
                this.owner.conversation = null;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            this.ChatLabel.Hide();
            base.Deactivate();
        }
    }

    public CustomPebblesConversationBehaviour BoundConversationBehaviour;

    protected CustomPebblesConversation(SSOracleBehavior owner, ID conversationID) : base(owner, null, conversationID, owner.dialogBox)
    {
        BoundConversationBehaviour = new CustomPebblesConversationBehaviour(owner, this);
        base.convBehav = BoundConversationBehaviour;
    }

    protected void Speak(string text, int initialWait = 0, int textLinger = 5)
    {
        this.events.Add(new TextEvent(this, initialWait, Translate(text), textLinger));
    }

    protected void PauseEvent(int pauseFrames)
    {
        this.events.Add(new PauseAndWaitForStillEvent(this, this.convBehav, pauseFrames));
    }

    protected void Wait(int waitTime)
    {
        this.events.Add(new WaitEvent(this, waitTime));
    }

    protected void DoSpecialEvent(string eventName, int initialWait = 5)
    {
        this.events.Add(new SpecialEvent(this, initialWait, eventName));
    }

    protected void FuncEvent(Action func)
    {
        this.events.Add(new FunctionEvent(this, func));
    }

    public override void Update()
    {
        age++;
        if (waitForStill)
        {
            if (!convBehav.CurrentlyCommunicating && convBehav.communicationPause > 0) { convBehav.communicationPause--; }

            if (!convBehav.CurrentlyCommunicating && convBehav.communicationPause < 1 && owner.allStillCounter > 20) { waitForStill = false; }
        }
        else { base.Update(); }
    }
}

public abstract class CustomMoonConversation : SLOracleBehaviorHasMark.MoonConversation
{
    protected CustomMoonConversation(ID id, OracleBehavior oracleBehaviour, SLOracleBehaviorHasMark.MiscItemType describeItem) : base(id, oracleBehaviour, describeItem)
    {
        
    }
    
    protected void Speak(string text, int initialWait = 0, int textLinger = 5)
    {
        this.events.Add(new TextEvent(this, initialWait, Translate(text), textLinger));
    }

    protected void Wait(int waitTime)
    {
        this.events.Add(new WaitEvent(this, waitTime));
    }

    protected void DoSpecialEvent(string eventName, int initialWait = 5)
    {
        this.events.Add(new SpecialEvent(this, initialWait, eventName));
    }

    protected void FuncEvent(Action func)
    {
        this.events.Add(new FunctionEvent(this, func));
    }
}