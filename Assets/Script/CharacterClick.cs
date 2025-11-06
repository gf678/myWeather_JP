using UnityEngine;

public class CharacterClick : MonoBehaviour
{
    public DialogueManager dialogueManager;

    void OnMouseDown()
    {
        Debug.Log("✅ 캐릭터 클릭 감지됨!");
        dialogueManager.OnCharacterClicked();
    }
}