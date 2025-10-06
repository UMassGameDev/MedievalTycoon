using Godot;

namespace ComputerysMovementSystem;

[GlobalClass]
public partial class ComputeryPlayer : CharacterBody3D {
	public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	
	public override void _Process(double delta) {
		HandleControllerLook((float)delta);
		
		#if DEBUG
		// TODO: add a real pause menu
		if (InputMapHandler.Pause.IsJustPressed) {
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		}
		#endif
	}
	
	public override void _PhysicsProcess(double delta) {
		HandleMovement((float)delta);
	}

	public override void _EnterTree() {
		SubscribeToCameraInputs();
	}

	public override void _ExitTree() {
		UnsubscribeFromCameraInputs();
	}
}
