using System;
using System.Collections.Generic;
using IsoEngine;

namespace Hospital
{
	[Serializable]
	public class IsoWallData
	{
		public Vector2i wallsfrom;
		public Vector2i wallsTo;
		public WallType InnerWallType = WallType.BrickWalls;
		public WallType OuterWallType = WallType.BrickWalls;
		public int InnerWindowID;
		public int OuterWindowID;
		public int DoorID;
		public List<int> windows;
		//public int doors;
		public List<int> doors=new List<int>();
        public PathType[] doorPathTypes = new PathType[1] { PathType.Default };
        public IsoWallData(Vector2i from, Vector2i to, WallType innerWallID, WallType outerWallID, int innerWindowID, int outerWindowID, int doorID, List<int> doorPosition, List<int> windowPositions, PathType[] doorPathTypes = null)
		{
			wallsfrom = from;
			wallsTo = to;
			InnerWallType = innerWallID;
			OuterWallType = outerWallID;
			InnerWindowID = innerWindowID;
			OuterWindowID = outerWindowID;
			doors = doorPosition;
			DoorID = doorID;
			windows = windowPositions;
            if (doorPathTypes != null)
                this.doorPathTypes = doorPathTypes;
            else            
                this.doorPathTypes = new PathType[1] {PathType.Default};
        }

		public IsoWallData() { }
	}

	public enum HospitalArea
	{
		Clinic = 0,
		Laboratory = 1,
		Hospital = 2,
		Patio = 3,
        MaternityWardClinic = 4,
        MaternityWardPatio = 5,
		Ignore=20,
	}

    public enum HospitalAreaInDrawer
    {
        Clinic = 0,
        Laboratory = 1,
        Patio = 2,
        MaternityClinic = 3,
        MaternityPatio = 4,
        Ignore = 20,
    }

    public enum PathType //matward add some new types for maternity. Optional if so. Because you can use what you have. (Translated from Polish)
    {
        Default = 0,
        GoPlaygroundPath = 1,
        GoEmergencyPath = 2,
        GoHomePath = 3,
        GoWanderingPath = 4,
        GoHealingPath = 5,
        GoReceptionPath = 6,
        GoPatioPath = 7,
        VIPPath = 8,
        NoPath = 9,
    }

    public enum MutalType //MutualType??
    {
        None = 0,
        North = 1,
        South = 2,
        West = 3,
        East = 4,
    }
    public enum Rotation
	{
		North = 0,
		East = 1,
		South = 2,
		West = 3
	}

    public enum DecorationInteractionType
    {
        Default = 0,
        Siting = 1,
        Drinking = 2,
    }

    public enum AIWanderingMode
    {
        Default = 0,
        Walking = 1,
        CanTalk = 2,
        Talk = 3,
    }
}
