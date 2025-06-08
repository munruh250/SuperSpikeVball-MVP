using NUnit.Framework;
using UnityEngine.InputSystem;

public class PlayerInputTests
{
    private PlayerInputActions actions;

    [SetUp]
    public void Setup()
    {
        actions = new PlayerInputActions();
    }

    [Test]
    public void MoveAction_Exists()
    {
        Assert.IsNotNull(actions.Player.Move, "Move action should be defined");
    }

    [Test]
    public void JumpAction_Exists()
    {
        Assert.IsNotNull(actions.Player.Jump, "Jump action should be defined");
    }

    [Test]
    public void BumpAction_Exists()
    {
        Assert.IsNotNull(actions.Player.Bump, "Bump action should be defined");
    }

    [Test]
    public void SetAction_Exists()
    {
        Assert.IsNotNull(actions.Player.Set, "Set action should be defined");
    }

    [Test]
    public void SpikeAction_Exists()
    {
        Assert.IsNotNull(actions.Player.Spike, "Spike action should be defined");
    }

    [Test]
    public void MoveAction_DefaultBinding_IsLeftStick()
    {
        var binding = actions.Player.Move.bindings[0];
        Assert.AreEqual("<Gamepad>/leftStick", binding.path);
    }

    [Test]
    public void JumpAction_DefaultBinding_IsButtonWest()
    {
        var binding = actions.Player.Jump.bindings[0];
        Assert.AreEqual("<Gamepad>/buttonWest", binding.path);
    }

    [Test]
    public void BumpAction_DefaultBinding_IsButtonSouth()
    {
        var binding = actions.Player.Bump.bindings[0];
        Assert.AreEqual("<Gamepad>/buttonSouth", binding.path);
    }

    [Test]
    public void SetAction_DefaultBinding_IsButtonEast()
    {
        var binding = actions.Player.Set.bindings[0];
        Assert.AreEqual("<Gamepad>/buttonEast", binding.path);
    }

    [Test]
    public void SpikeAction_DefaultBinding_IsButtonNorth()
    {
        var binding = actions.Player.Spike.bindings[0];
        Assert.AreEqual("<Gamepad>/buttonNorth", binding.path);
    }
}
