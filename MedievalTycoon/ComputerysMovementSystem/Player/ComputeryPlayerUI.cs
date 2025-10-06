using Godot;

namespace ComputerysMovementSystem;

public partial class ComputeryPlayer : CharacterBody3D {
    [ExportCategory("UI")]
    [Export] private Label _healthLabel;
    [Export] private Label _speedLabel;
}
