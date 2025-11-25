using System.Collections.Generic;
using Godot;

namespace Builder.Components.External
{
    public enum Face
    {
        North,
        South,
        East,
        West
    }

    public class SnapPoint_Directional
    {
        public SnapPoint_External North { get; set; }
        public SnapPoint_External South { get; set; }
        public SnapPoint_External East { get; set; }
        public SnapPoint_External West { get; set; }

        public SnapPoint_External this[Face face]
        {
            get => face switch
            {
                Face.North => North,
                Face.South => South,
                Face.East => East,
                Face.West => West,
                _ => null,
            };
            set
            {
                switch (face)
                {
                    case Face.North: North = value; break;
                    case Face.South: South = value; break;
                    case Face.East: East = value; break;
                    case Face.West: West = value; break;
                }
            }
        }

        public IEnumerable<SnapPoint_External> All => new[] { North, South, East, West };

        public bool TryGet(Face face, out SnapPoint_External snapPoint)
        {
            snapPoint = this[face];
            return snapPoint != null;
        }

        public Face? Opposite(Face face) =>
            face switch
            {
                Face.North => Face.South,
                Face.South => Face.North,
                Face.East => Face.West,
                Face.West => Face.East,
                _ => null,
            };
    }
}