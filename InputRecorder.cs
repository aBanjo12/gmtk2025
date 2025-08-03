using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gmtk2025;

public partial class InputRecorder : Node
{
    private List<KeyEvent> _recordedEvents = new();
    private float _recordStartTime;
    private bool _isRecording = false;
    
    private static readonly Key[] TrackedKeys = { Key.W, Key.A, Key.S, Key.D };

    public override void _Ready()
    {
        GD.Print("InputRecorder Ready");
        (GetTree().Root.GetNode<Node>("GlobalEvents") as GlobalEvents).Connect(GlobalEvents.SignalName.Fire, new Callable(this, nameof(GunShot)));
    }

    public void StartRecording()
    {
        _recordedEvents.Clear();
        _recordStartTime = Time.GetTicksMsec() / 1000f;
        _isRecording = true;
    }

    public void StopRecording()
    {
        _isRecording = false;
    }
    
    public List<KeyEvent> StopRecordingGetEvents()
    {
        _isRecording = false;
        return _recordedEvents.ToList();
    }

    public void GunShot(Vector2 position)
    {
        float currentTime = Time.GetTicksMsec() / 1000f;
        float relativeTime = currentTime - _recordStartTime;
        _recordedEvents.Add(new KeyEvent(KeyEvent.EventType.MouseClick, relativeTime, Key.None, position));
        GD.Print("gun at " + relativeTime);
    }

    public List<KeyEvent> GetRecordedEvents() => _recordedEvents;

    public override void _Input(InputEvent @event)
    {
        if (!_isRecording) return;

        float currentTime = Time.GetTicksMsec() / 1000f;
        float relativeTime = currentTime - _recordStartTime;

        // Key events: W, A, S, D only
        if (@event is InputEventKey keyEvent && !keyEvent.Echo)
        {
            if (TrackedKeys.Contains(keyEvent.Keycode))
            {
                var eventType = keyEvent.Pressed ? KeyEvent.EventType.KeyDown : KeyEvent.EventType.KeyUp;
                GD.Print($"keyevent {eventType} at {relativeTime} key {keyEvent.Keycode}");
                _recordedEvents.Add(new KeyEvent(eventType, relativeTime, keyEvent.Keycode));
            }
        }
    }
}