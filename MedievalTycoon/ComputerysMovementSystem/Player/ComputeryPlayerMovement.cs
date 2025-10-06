using Godot;

namespace ComputerysMovementSystem;

public partial class ComputeryPlayer : CharacterBody3D {
	[ExportCategory("Quake Movement")]
	[ExportSubgroup("Friction")]
	[Export] public float MinimumSpeed = 1.0f;
	[Export] public float Friction = 6.0f;
	[ExportSubgroup("Acceleration")]
	[Export] public Curve StrafeCurve = new Curve();
	[Export] public float Acceleration = 30.0f;
	[Export] public float AccelerationAir = 5.0f;
	[Export] public float MaxGainableSpeed = 10.0f;
	[ExportSubgroup("Gravity")]
	[Export] public float MaximumPositiveVerticalVelocityToBeConsideredGrounded = 10f;
	[Export] public float Gravity = 29.4f;
	[ExportSubgroup("Jumping")]
	[Export] public float JumpVelocity = 7.5f;
	[Export] public float CoyoteTime = 0.2f;

	/// <summary>
	/// Does all the movement.
	/// </summary>
	/// <param name="delta"> Time delta. </param>
	private void HandleMovement(float delta) {
		Vector2 inputDirection = InputMapHandler.GetMovementInput();
		Vector3 wishDirection = new Vector3(inputDirection.X, 0, inputDirection.Y).Rotated(Vector3.Up, Camera.GlobalRotation.Y);
		
		Vector3 velocity = Velocity;
		
		CoreMovement(ref velocity, wishDirection, delta);
		
		// add test force
		if (InputMapHandler.Interact.IsJustPressed) { velocity += Camera.Transform.Basis.Z * -20.0f; }
		
		Velocity = velocity;
		MoveAndSlide();
	}
	
	private float _airtime = 0.0f;
	
	/// <summary>
	/// Core Quake style movement logic.
	/// </summary>
	/// <param name="velocity"> Reference to the velocity vector to modify. </param>
	/// <param name="wishDirection"> Direction to accelerate towards. </param>
	/// <param name="delta"> Time delta. </param>
	private void CoreMovement(ref Vector3 velocity, Vector3 wishDirection, float delta) {
		// Ramp Slide: https://www.ryanliptak.com/blog/rampsliding-quake-engine-quirk/
		bool isOnFloor = IsOnFloor();
		bool isGrounded = isOnFloor && velocity.Y <= MaximumPositiveVerticalVelocityToBeConsideredGrounded;
		
		if (isGrounded) {
			_airtime = 0.0f;
			
			QuakeMath.QuakeFriction(ref velocity, MinimumSpeed, Friction, delta);
			QuakeMath.StrafeCurveAcceleration(ref velocity, wishDirection, StrafeCurve, Acceleration, MaxGainableSpeed, delta);
		} else {
			_airtime += delta;
			
			QuakeMath.StrafeCurveAcceleration(ref velocity, wishDirection, StrafeCurve, AccelerationAir, MaxGainableSpeed, delta);
			velocity.Y -= Gravity * delta;
		}

		if (isOnFloor) {
			Vector3 floorNormal = GetFloorNormal();
			float backoff = velocity.Dot(floorNormal);
			if (backoff < 0) {
				velocity -= floorNormal * backoff;
			}
		}

		if ((isGrounded || _airtime <= CoyoteTime) && InputMapHandler.Jump.IsPressed) {
			velocity.Y = JumpVelocity;
			_airtime = CoyoteTime + float.Epsilon; // Prevent double jumping within coyote time
		}
	}
}
