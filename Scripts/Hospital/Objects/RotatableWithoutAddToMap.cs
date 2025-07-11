using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using System.Linq;
using MovementEffects;
using SimpleUI;

namespace Hospital
{
	public class RotatableWithoutAddToMap : RotatableObject
	{
        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, _state, true);
        }

        protected override void AddToMap()
        {
            state = State.fresh;
            base.AddToMapTemporary();
        }

        public override void MoveTo(int x, int y)
        {
            base.MoveToTemporaryObject(x, y);
        }

        public override void MoveTo(int x, int y, Rotation beforeRotation)
        {
            base.MoveToTemporaryObject(x, y);
        }


        public override bool isTemporaryObject()
        {
            return true;
        }

    }
}