// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;

namespace CompositionAnimatedControlDemo;

public sealed class PhysicsSimulation
{
    private readonly float _logicalWidth;
    private readonly float _logicalHeight;

    private readonly Random _rnd = new(1);
    private float _accumulator;
    private float _gravityY;
    private World _world = null!;
    private Body[] _balls = [];
    private float[] _radii = [];

    public PhysicsSimulation(float logicalWidth, float logicalHeight, float initialGravityY)
    {
        _logicalWidth = logicalWidth;
        _logicalHeight = logicalHeight;
        _gravityY = initialGravityY;
        BuildWorld();
    }

    public void Tick(float deltaSeconds)
    {
        const float fixedStep = 1f / 240f;
        _accumulator += deltaSeconds;
        while (_accumulator >= fixedStep)
        {
            Step(fixedStep);
            _accumulator -= fixedStep;
        }
    }

    public void SetGravity(float gravityY)
    {
        _gravityY = gravityY;
        _world.Gravity = new Vector2(0, _gravityY);
    }

    public void Restart()
    {
        _accumulator = 0f;
        BuildWorld();
    }

    public void ForEachBall(Action<Vector2, float> callback)
    {
        for (int i = 0; i < _balls.Length; i++)
        {
            var pos = _balls[i].GetPosition();
            var r = _radii[i];
            callback(pos, r);
        }
    }

    private void Step(float dt)
    {
        _world.Gravity = new Vector2(0, _gravityY);
        _world.Step(dt, 8, 3);
    }

    private void CreateStaticBox(Vector2 center, Vector2 size)
    {
        var body = _world.CreateBody(new BodyDef { type = BodyType.Static, position = center });
        var box = new PolygonShape();
        box.SetAsBox(size.X * 0.5f, size.Y * 0.5f, Vector2.Zero, 0f);
        var fd = new FixtureDef
        {
            shape = box,
            density = 0f
        };
        body.CreateFixture(fd);
    }

    private void BuildWorld()
    {
        _world = new World(new Vector2(0, _gravityY));

        // Bounds
        CreateStaticBox(new Vector2(_logicalWidth / 2f, 0.1f), new Vector2(_logicalWidth, 0.2f)); // floor
        CreateStaticBox(new Vector2(_logicalWidth / 2f, _logicalHeight - 0.1f), new Vector2(_logicalWidth, 0.2f)); // ceiling
        CreateStaticBox(new Vector2(0.1f, _logicalHeight / 2f), new Vector2(0.2f, _logicalHeight)); // left
        CreateStaticBox(new Vector2(_logicalWidth - 0.1f, _logicalHeight / 2f), new Vector2(0.2f, _logicalHeight)); // right

        // Balls
        _balls = new Body[12];
        _radii = new float[_balls.Length];
        for (int i = 0; i < _balls.Length; i++)
        {
            var r = 0.25f + (float)_rnd.NextDouble() * 0.25f;
            var body = _world.CreateBody(new BodyDef
            {
                type = BodyType.Dynamic,
                position = new Vector2(1.5f + (float)_rnd.NextDouble() * (_logicalWidth - 3f), _logicalHeight - 1f - i * 0.5f)
            });
            var circle = new CircleShape { Radius = r };
            var fd = new FixtureDef
            {
                shape = circle,
                density = 1f,
                restitution = 0.8f,
                friction = 0.02f
            };
            body.CreateFixture(fd);
            _balls[i] = body;
            _radii[i] = r;
        }
    }
}
