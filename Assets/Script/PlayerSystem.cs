using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public partial struct PlayerSystem : ISystem
{
    // Because OnUpdate accesses a managed object (the camera), we cannot Burst compile 
    // this method, so we don't use the [BurstCompile] attribute here.
    
    public void OnUpdate(ref SystemState state)
    {
        // Get input using the new Input System directly
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;
        
        float horizontal = 0f;
        float vertical = 0f;
        
        // Check keyboard input
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
        }
        
        // Check gamepad input
        if (gamepad != null)
        {
            horizontal += gamepad.leftStick.x.ReadValue();
            vertical += gamepad.leftStick.y.ReadValue();
        }
        
        var movement = new float3(horizontal, 0, vertical);
        movement *= SystemAPI.Time.DeltaTime * 5f; // Add speed multiplier

        foreach (var playerTransform in 
                SystemAPI.Query<RefRW<LocalTransform>>()
                    .WithAll<Player>())
        {
            // move the player tank
            playerTransform.ValueRW.Position += movement;

            // move the camera to follow the player
            var cameraTransform = Camera.main.transform;
            cameraTransform.position = playerTransform.ValueRO.Position;
            cameraTransform.position -= 10.0f * (Vector3)playerTransform.ValueRO.Forward();  // move the camera back from the player
            cameraTransform.position += new Vector3(0, 5f, 0);  // raise the camera by an offset
            cameraTransform.LookAt(playerTransform.ValueRO.Position);  // look at the player
        }
    }
}