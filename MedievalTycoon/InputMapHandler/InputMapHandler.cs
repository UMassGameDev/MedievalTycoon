using System;

namespace Godot;
public partial class InputMapHandler : Node {
    /// <summary>
    /// Radians per unit of mouse movement.
    /// Set to -0.1 degree per pixel of mouse movement.
    /// </summary>
    public const float RadiansPerMouseUnit = -0.00174533f;

    /// <summary>
    /// Is the player currently using a controller
    /// </summary>
    /// <returns> True if using a controller, false if using mouse and keyboard </returns>
    public static bool IsUsingController() => _usingController;
    private static bool _usingController = false;
    
    /// <summary>
    /// Look event is only for mouse input, and you will need to check in process if using a controller for the controllers look input
    /// Vector2 where X is left/right and Y is up/down
    /// </summary>
    public static event Action<Vector2> LookEvent;
    
    /// <summary>
    /// Event for when a number key (0-9) is pressed
    /// 1 = 0, 2 = 1, ..., 9 = 8, 0 = 9 (so that we can use it with arrays n shit)
    /// 
    /// Includes the number pad keys
    /// </summary>
    public static event Action<int> NumbersEvent;
    
    public override void _Input(InputEvent @event) {
        switch (@event) {
            case InputEventMouse: {
                _usingController = false;
            
                if (@event is InputEventMouseMotion mouseMotion) {
                    LookEvent?.Invoke(mouseMotion.ScreenRelative); // Read the about the diffrence between this and relative: https://docs.godotengine.org/en/4.4/classes/class_inputeventmousemotion.html#class-inputeventmousemotion-property-screen-relative
                    return; // Mouse motion is handled separately to all of this
                }

                break;
            }
            case InputEventKey keyEvent: {
                _usingController = false;
                
                if (keyEvent.IsPressed() && !keyEvent.IsEcho()) {
                    // This is soooooooooo cursed lol, but it's by far the best way I could find to do this.
                    // I don't want to have 10 actions for the number keys, so instead I just check if a number key was pressed here.
                    // This also makes the code handling the number key presses much simpler since the action has a int parameter.
                    if (keyEvent.Keycode >= Key.Key0 && keyEvent.Keycode <= Key.Key9) {
                        int numberPressed = (int)(keyEvent.Keycode - Key.Key1);
                        if (numberPressed == -1) numberPressed = 9;
                        
                        NumbersEvent?.Invoke(numberPressed);
                    } else if (keyEvent.Keycode >= Key.Kp0 && keyEvent.Keycode <= Key.Kp9) {
                        int numberPressed = (int)(keyEvent.Keycode - Key.Kp1);
                        if (numberPressed == -1) numberPressed = 9;
                        
                        NumbersEvent?.Invoke(numberPressed);
                    }
                }
                
                break;
            }
            case InputEventJoypadButton or InputEventJoypadMotion: {
                _usingController = true;
                break;
            }
        }

        HandleInputMap(@event);
    }
    
    /// <summary>
    /// Returns a vector2 representing the movement input from the player
    /// </summary>
    /// <returns> Vector2 where X is left/right and Y is forwards/backwards </returns>
    public static Vector2 GetMovementInput() => Input.GetVector(Left.Action, Right.Action, Forwards.Action, Backwards.Action);
    
    /// <summary>
    /// Returns a vector2 representing the look input from a controller
    /// </summary>
    /// <returns> Vector2 where X is left/right and Y is up/down </returns>
    public static Vector2 GetControllerLookInput() => Input.GetVector(LookLeft.Action, LookRight.Action, LookUp.Action, LookDown.Action);
}