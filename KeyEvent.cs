using Godot;

public class KeyEvent
{
    public enum EventType { KeyDown, KeyUp, MouseClick }

    public EventType Type;
    public Key KeyCode; // Only used for W/A/S/D
    public Vector2 RelativePosition; // Only used for MouseClick
    public float Timestamp;

    public KeyEvent(EventType type, float timestamp, Key keyCode = Key.None, Vector2 relativePosition = new Vector2())
    {
        Type = type;
        Timestamp = timestamp;
        KeyCode = keyCode;
        RelativePosition = relativePosition;
    }
}