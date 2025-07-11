using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainList : MonoBehaviour
{
    public List<Race> Races;


    [System.Serializable]
    public class MultiContainer
    {
        public Sprite front;
        public Sprite back;
    }


    [System.Serializable]
    public class Body
    {
        public Sprite Avatar;
        public MultiContainer BodyContainer;
        public Arms Arms;
        public MultiContainer PregnantBodyContainer;
    }

    [System.Serializable]
    public class Legs
    {
        public Sprite LowerBodyFront;
        public Sprite Thigh;
        public Sprite Calf;
        public Sprite FootFront;
        public Sprite FootBack;
    }

    [System.Serializable]
    public class Arms
    {
        public Sprite Arm;
        public Sprite HandLeft;
        public Sprite HandRight;
    }

    [System.Serializable]
    public class Head
    {
        public Sprite Avatar;
        public Sprite EyeLids;
        public MultiContainer HeadContainer;
    }

    [System.Serializable]
    public class Apron
    {
        public Sprite ApronFrontLeft;
        public Sprite ApronFront;
        public Sprite ApronFrontRight;
        public Sprite ApronBack;
    }

    [System.Serializable]
    public struct Gender
    {
        public string Name;
        public List<Head> HeadList;
        public List<Body> BodyList;
        public List<Legs> LegsList;
        public List<Apron> ApronList;
    }
    [System.Serializable]
    public class Race
    {
        public string Name;
        public List<Gender> Genders;
    }
}
