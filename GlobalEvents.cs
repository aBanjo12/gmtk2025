using Godot;

namespace gmtk2025;

public partial class GlobalEvents : Node
{
	[Signal]
	public delegate void FireEventHandler(Vector2 position);

	[Signal]
	public delegate void StartRecordingEventHandler();
	
	[Signal]
	public delegate void StopRecordingEventHandler();

	public void EmitFireEvent(Vector2 position)
	{
		EmitSignal(SignalName.Fire, position);
	}

	public void EmitStartRecordingEvent()
	{
		EmitSignalStartRecording();
	}

	public void EmitStopRecordingEvent()
	{
		EmitSignalStopRecording();
	}
	
	
}
