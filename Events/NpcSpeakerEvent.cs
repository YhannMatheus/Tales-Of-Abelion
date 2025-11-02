using UnityEngine;

public class NpcSpeakerEvent : Event
{
    public string npcName;
    [TextArea(3, 10)]
    public string[] dialogueLines;

    public override void OnClick()
    {

    }
}