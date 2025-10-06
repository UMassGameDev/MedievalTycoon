// TODO: rework this like entirely

using Godot;

namespace ComputerysMovementSystem;

public partial class ComputeryPlayer : CharacterBody3D {
    [ExportCategory("Camera")]
    [Export] public Camera3D Camera;
    [Export(PropertyHint.Range, "0, 180, 0.1, radians_as_degrees")] private float _maxVerticalCameraRotation = 1.5708f;
    [Export(PropertyHint.Range, "-180, 0, 0.1, radians_as_degrees")] private float _minVerticalCameraRotation = -1.5708f;
    [Export] public float XSensitivity = 1.0f;
    [Export] public float YSensitivity = 1.0f;
    Vector2 _cameraRotation = Vector2.Zero;
    
    private void SubscribeToCameraInputs() { InputMapHandler.LookEvent += MouseLookInput; }
    private void UnsubscribeFromCameraInputs() { InputMapHandler.LookEvent -= MouseLookInput; }
    
    private void HandleControllerLook(float delta) {
        if (InputMapHandler.IsUsingController()) {
            // TODO: This probably does not work well, so somebody with a controller should test it
            LookInput(InputMapHandler.GetControllerLookInput() * delta);
        }
    }
    
    private void MouseLookInput(Vector2 relative) {
        LookInput(relative * InputMapHandler.RadiansPerMouseUnit);
    }
    
    private void LookInput(Vector2 relative) {
        _cameraRotation += new Vector2(relative.X * XSensitivity, relative.Y * YSensitivity);
        _cameraRotation.Y = Mathf.Clamp(_cameraRotation.Y, _minVerticalCameraRotation, _maxVerticalCameraRotation);

        Transform3D cameraTransform = Camera.Transform;
        cameraTransform.Basis = Basis.Identity;
        Camera.Transform = cameraTransform;
        
        Camera.RotateObjectLocal(Vector3.Up, _cameraRotation.X);
        Camera.RotateObjectLocal(Vector3.Right, _cameraRotation.Y);
    }
}
