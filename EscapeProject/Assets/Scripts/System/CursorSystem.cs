using UnityEngine;

public class CursorSystem : MonoBehaviour
{
    [SerializeField] private Texture2D[] CursorTextures;
    [SerializeField] private ScriptController _scriptController;
    public void DefaultCursor() { Cursor.SetCursor(CursorTextures[0], new Vector2(0, 0), CursorMode.Auto); }
    public void GOCursor() {
        if (_scriptController.IsChatting) return;
        Cursor.SetCursor(CursorTextures[1], new Vector2(0, 0), CursorMode.Auto); 
    }
    public void UICursor() {
        if (_scriptController.IsChatting) return;
        Cursor.SetCursor(CursorTextures[2], new Vector2(0, 0), CursorMode.Auto); 
    }
}
