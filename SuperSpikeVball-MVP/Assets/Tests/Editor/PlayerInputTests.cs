using NUnit.Framework;
using UnityEngine;
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
    public void MoveAction_Exists()     => Assert.IsNotNull(actions.Player.Move,  "Move action should be defined");
    [Test]
    public void JumpAction_Exists()     => Assert.IsNotNull(actions.Player.Jump,  "Jump action should be defined");
    [Test]
    public void BumpAction_Exists()     => Assert.IsNotNull(actions.Player.Bump,  "Bump action should be defined");
    [Test]
    public void SetAction_Exists()      => Assert.IsNotNull(actions.Player.Set,   "Set action should be defined");
    [Test]
    public void SpikeAction_Exists()    => Assert.IsNotNull(actions.Player.Spike, "Spike action should be defined");

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

public class BallControllerTests
{
    private GameObject    ballObj;
    private BallController ballCtrl;

    [SetUp]
    public void Setup()
    {
        ballObj  = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballCtrl = ballObj.AddComponent<BallController>();
        var rb   = ballObj.GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    [Test]
    public void Launch_Default_SetsCorrectVelocity()
    {
        ballCtrl.initialSpeed     = 10f;
        ballCtrl.initialDirection = new Vector3(1, 1, 0);
        Vector3 expected = ballCtrl.initialDirection.normalized * 10f;

        ballCtrl.Launch();
        Vector3 actual = ballObj.GetComponent<Rigidbody>().velocity;

        float delta = 1e-4f;
        Assert.AreEqual(expected.x, actual.x, delta, "X velocity mismatch");
        Assert.AreEqual(expected.y, actual.y, delta, "Y velocity mismatch");
        Assert.AreEqual(expected.z, actual.z, delta, "Z velocity mismatch");
    }

    [Test]
    public void Launch_Custom_SetsCorrectVelocity()
    {
        Vector3 dir   = new Vector3(0, 1, 0);
        float   speed = 5f;
        Vector3 expected = dir.normalized * speed;

        ballCtrl.Launch(dir, speed);
        Vector3 actual = ballObj.GetComponent<Rigidbody>().velocity;

        float delta = 1e-4f;
        Assert.AreEqual(expected.x, actual.x, delta, "X velocity mismatch");
        Assert.AreEqual(expected.y, actual.y, delta, "Y velocity mismatch");
        Assert.AreEqual(expected.z, actual.z, delta, "Z velocity mismatch");
    }
}
