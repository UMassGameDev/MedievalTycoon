using Godot;

namespace ComputerysMovementSystem;

/// <summary>
/// Class for Quake style movement math functions.
/// In a separate static class so I can just steal it for other projects and have clean player code.
/// I think the only really confusing thing on how to work this is the reference struct passing.
/// structs are value types, so passing them to a function normally would just pass a copy, and then it would be useless.
/// Could have just had them return a new vector but this is more like the original quake code, if you want it changed lmk.
///
/// Original Quake source code for reference: https://github.com/id-Software/Quake/blob/master/WinQuake/sv_user.c#L190
/// </summary>
public static class QuakeMath {
	
	/// <summary>
	/// Applies Quake style friction to a velocity vector.
	/// </summary>
	/// <param name="velocity"> Reference to the velocity vector to apply friction to. </param>
	/// <param name="minimumSpeed"> Minimum speed to apply friction at. </param>
	/// <param name="friction"> Friction coefficient. </param>
	/// <param name="delta"> Time delta. </param>
	public static void QuakeFriction(ref Vector3 velocity, float minimumSpeed, float friction, float delta) {
		float speed = velocity.Length();
		if (Mathf.IsZeroApprox(speed)) { return; }
		
		float effectiveSpeed = Mathf.Max(speed, minimumSpeed);
		float deceleration = effectiveSpeed * friction * delta;
		
		float newSpeed = speed - deceleration;
		if (newSpeed < 0) { newSpeed = 0; }
		
		float scale = newSpeed / speed;
		
		velocity *= scale;
	}
	
	/// <summary>
	/// Applies Quake style acceleration to a velocity vector.
	/// 
	/// Note that this function limits the horizontal speed a player can gain from purely strafing to maxGainableSpeed, but does limit it from external forces.
	/// This is to prevent bunny hopping from being too powerful while still allowing for rocket jumping and similar mechanics.
	/// </summary>
	/// <param name="velocity"> Reference to the velocity vector to apply acceleration to. </param>
	/// <param name="wishDirection">
	/// Direction to accelerate towards.
	/// 
	/// Input from the input system gives a vector in the range of length 0 to 1 when on controllers,
	/// and DOES NOT need to be normalized since it is used to scale acceleration.
	/// For keyboard input it will be a vector of length 0 or 1.
	/// </param>
	/// <param name="targetSpeed"> Speed to accelerate towards in the wish direction. </param>
	/// <param name="acceleration"> Acceleration coefficient. </param>
	/// <param name="maxGainableSpeed"> Maximum horizontal speed that can be gained from purely strafing. </param>
	/// <param name="delta"> Time delta. </param>
	public static void QuakeAcceleration(ref Vector3 velocity, Vector3 wishDirection, float targetSpeed, float acceleration, float maxGainableSpeed, float delta) {
		// We treat this as if it is speed for quake style movement
		// If you are interested in why this does that this video is dope af: https://youtu.be/v3zT3Z5apaM
		float velocityAlongWishDirection = velocity.Dot(wishDirection);
		
		float speedToAdd = targetSpeed - velocityAlongWishDirection;
		if (speedToAdd <= 0f) { return; }
		
		float scaledAcceleration = acceleration * delta * targetSpeed;
		float effectiveAcceleration = Mathf.Min(speedToAdd, scaledAcceleration);
		Vector3 accelerationVector = wishDirection * effectiveAcceleration;
		
		// Yeah yeah ik it could be done more efficiently but it's 2025 and our movement code will never matter.
		Vector2 horizontalVelocity = new Vector2(velocity.X, velocity.Z);
		Vector2 horizontalAccelerationVector = new Vector2(accelerationVector.X, accelerationVector.Z);
		Vector2 newHorizontalVelocity = horizontalVelocity + horizontalAccelerationVector;
		float currentHorizontalSpeed = horizontalVelocity.Length();
		float newHorizontalSpeed = newHorizontalVelocity.Length();
		if (newHorizontalSpeed > maxGainableSpeed && newHorizontalSpeed > currentHorizontalSpeed) {
			float speed = Mathf.Max(maxGainableSpeed, currentHorizontalSpeed);
			Vector2 clampedNewHorizontalVelocity = newHorizontalVelocity.Normalized() * speed;
			velocity = new Vector3(clampedNewHorizontalVelocity.X, velocity.Y + accelerationVector.Y, clampedNewHorizontalVelocity.Y);
		} else {
			velocity += accelerationVector;
		}
	}
	
	/// <summary>
	/// This is quake type acceleration but with a curve to do strafe acceleration scaling.
	/// </summary>
	/// <param name="velocity"> Reference to the velocity vector to apply acceleration to. </param>
	/// <param name="wishDirection">
	/// Direction to accelerate towards.
	/// 
	/// Input from the input system gives a vector in the range of length 0 to 1 when on controllers,
	/// and DOES NOT need to be normalized since it is used to scale acceleration.
	/// For keyboard input it will be a vector of length 0 or 1.
	/// </param>
	/// <param name="strafeCurve">
	/// Curve to use for strafe acceleration scaling.
	/// 1 is going exactly the same direction.
	/// 0 is going perpendicular.
	/// -1 is going exactly the opposite direction.
	/// This lets you tune how effective strafing is.
	/// </param>
	/// <param name="acceleration"> Base acceleration. </param>
	/// <param name="maxGainableSpeed"> Maximum horizontal speed that can be gained from this method. </param>
	/// <param name="delta"> Time delta. </param>
	public static void StrafeCurveAcceleration(ref Vector3 velocity, Vector3 wishDirection, Curve strafeCurve, float acceleration, float maxGainableSpeed, float delta) {
		float differenceInDirection = velocity.Normalized().Dot(wishDirection.Normalized());
		float strafeMultiplier = strafeCurve.Sample(differenceInDirection);
		float accelerationToApply = acceleration * strafeMultiplier * delta;
		Vector3 accelerationVector = wishDirection * accelerationToApply;
		Vector2 horizontalVelocity = new Vector2(velocity.X, velocity.Z);
		Vector2 horizontalAccelerationVector = new Vector2(accelerationVector.X, accelerationVector.Z);
		Vector2 newHorizontalVelocity = horizontalVelocity + horizontalAccelerationVector;
		float currentHorizontalSpeed = horizontalVelocity.Length();
		float newHorizontalSpeed = newHorizontalVelocity.Length();
		if (newHorizontalSpeed > maxGainableSpeed && newHorizontalSpeed > currentHorizontalSpeed) {
			float speed = Mathf.Max(maxGainableSpeed, currentHorizontalSpeed);
			Vector2 clampedNewHorizontalVelocity = newHorizontalVelocity.Normalized() * speed;
			velocity = new Vector3(clampedNewHorizontalVelocity.X, velocity.Y + accelerationVector.Y, clampedNewHorizontalVelocity.Y);
		} else {
			velocity += accelerationVector;
		}
	}
}