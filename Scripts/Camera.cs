using Godot;
using System;
using System.Drawing;

public partial class Camera : Camera3D
{
    float camZoomDelta = 1; 
    float camRecenterTimer = 0;
    [Export] Node3D target;
    Node3D pivot;
    Node3D parent;

    const float clickSensitivity = 1f;
    const float altClickSensitivty = 0.01f;
    const float middleClickSensitivity = 0.01f;
    const float joyStickRotateSensitivity = 0.05f;
    const float joystickMoveSensitivity = 5.0f;
    const float startingFOV = 70;
    const float startingSize = 20;
    const float scrollUpMult = 0.75f;
    const float scrollDownMult = 1.25f;
    const float minSize = 0.1f;
    const float maxSize = 200;
    const float minFov = 1;
    const float maxFov = 180;
    const float startingYRot = 45;
    const float startingXRot = -30;
    const float recenterSpeed = 3;

    public override void _Ready()
    {       
        pivot = GetParent() as Node3D;
        parent = pivot.GetParent() as Node3D;
        if(Projection == ProjectionType.Orthogonal)
        {
            Size = startingSize;
            camZoomDelta = startingSize;
        }
        else
        {
            Fov = startingFOV;
            camZoomDelta = startingFOV;
        }
        parent.Rotation = new Vector3(0, Mathf.DegToRad(startingYRot), 0);
        pivot.Rotation = new Vector3(Mathf.DegToRad(startingXRot),0,0);
        if(target != null)
        {
            parent.GlobalPosition = target.GlobalPosition;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (Current)
        {
            if (Input.IsActionJustPressed("Scroll Up"))
            {
                if(Projection == ProjectionType.Orthogonal)
                {
                    camZoomDelta = Size * scrollUpMult;
                }
                else
                {
                    camZoomDelta = Fov * scrollUpMult;
                }
            }
            if (Input.IsActionJustPressed("Scroll Down"))
            {
                if(Projection == ProjectionType.Orthogonal)
                {
                    camZoomDelta = Size * scrollDownMult;
                }
                else
                {
                    camZoomDelta = Fov * scrollDownMult;
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Current)
        {
            if(Projection == ProjectionType.Orthogonal)
            {
                Size = Mathf.Clamp(Mathf.Lerp(Size, camZoomDelta,(float)delta*20),minSize, maxSize);
            }
            else
            {
                Fov = Mathf.Clamp(Mathf.Lerp(Fov, camZoomDelta,(float)delta*20),minFov, maxFov);
                Position = new Vector3(Position.X,Position.Y,Fov);
            }
        }

        //All this is for controllers
        Vector2 joyInput = Input.GetVector("Camera Left", "Camera Right", "Camera Up", "Camera Down");
        if(joyInput != Vector2.Zero)
        {
            if (Input.IsActionPressed("Action"))
            {
                Vector2 size = DisplayServer.ScreenGetSize();
                if(Projection == ProjectionType.Orthogonal)
                {
                    MoveCamera(new Vector2(joyInput.X * joystickMoveSensitivity, -joyInput.Y * joystickMoveSensitivity),Size);
                }
                else
                {
                    MoveCamera(new Vector2(joyInput.X * joystickMoveSensitivity, -joyInput.Y * joystickMoveSensitivity),Fov);
                }
            }
            else
            {
                parent.RotateY(joyInput.X * joyStickRotateSensitivity);
                pivot.RotateX(joyInput.Y* joyStickRotateSensitivity);
                pivot.RotationDegrees = new Vector3(Mathf.Clamp(pivot.RotationDegrees.X,-90,0),0,0);
            }
        }

        if (Input.IsActionJustPressed("Middle Action"))
        {
            camRecenterTimer = 1.0f;
        }

        if(target != null && camRecenterTimer > 0)
        {
            parent.GlobalPosition = parent.GlobalPosition.Lerp(target.GlobalPosition + new Vector3(0,1,0),1 - camRecenterTimer);
            camRecenterTimer = Mathf.Max(0,camRecenterTimer -((float)delta*recenterSpeed));
        }
    }

    public override void _UnhandledInput(InputEvent currentEvent)
    {
        if (currentEvent is InputEventMouseMotion motion)
        {
            if(Input.IsActionPressed("Alt Action"))
            {
                parent.RotateY(-motion.Relative.X * altClickSensitivty);
                pivot.RotateX(-motion.Relative.Y * middleClickSensitivity);
                pivot.RotationDegrees = new Vector3(Mathf.Clamp(pivot.RotationDegrees.X,-90,0),0,0);
            }
            if (Input.IsActionPressed("Action"))
            {
                if(Projection == ProjectionType.Orthogonal)
                {
                    MoveCamera(new Vector2(-motion.Relative.X * clickSensitivity, motion.Relative.Y * clickSensitivity),Size);
                }
                else
                {
                    MoveCamera(new Vector2(-motion.Relative.X * clickSensitivity, motion.Relative.Y * clickSensitivity),Fov);
                }
            }
        }   
    }

    void MoveCamera(Vector2 input, float fov)
    {
        Vector2 size = DisplayServer.ScreenGetSize();
        parent.Position += new Vector3(input.X,input.Y, 0).Rotated(Vector3.Up, GlobalRotation.Y) * fov / Mathf.Min(size.X,size.Y);
    }
}