using UnityEngine;
using IsoEngine;

namespace Hospital
{

    public class Doors : MonoBehaviour
    {
        public Vector2i position;
        public Rotation wallRotation;
        public DoorType door_mode = DoorType.Normal;

        [SerializeField]
        Vector3 openMovement = Vector3.zero;

        [SerializeField]
        Vector3 rotation = Vector3.zero;

        [SerializeField]
        bool rotate = false, inverseTexture = false;

        public bool open { get; set; }
        public bool near_door = false;
        public int doorRoomID = -1;

        private Transform tr, tr2;
        private float openPercent = 0, time = 0;
        private Vector3 firstPartPos, secondPartPos, firstEulerRot, secondEulerRot;
        private float delt;
        [HideInInspector]
        public bool display_centerFrame = true;

        public Doors nextDoor;

        public bool istThisNeraDoorToOtherDooor = false;

        public DoorBordersBlocker mutalBorder;

        private bool patient_around = false;
        public bool sameDoor = true;

        protected virtual void Start()
        {

            open = false;
            position.x = (int)transform.position.x;
            position.y = (int)transform.position.z;
            time = 0;
            tr = transform.GetChild(0);
            tr2 = transform.GetChild(1);

            if (near_door)
            {
                if (inverseTexture)
                {
                    if (!display_centerFrame)
                        transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    else
                    {
                        tr.localScale = new Vector3(-1 * tr.localScale.x, tr.localScale.y, tr.localScale.z);
                        tr2.localScale = new Vector3(-1 * tr2.localScale.x, tr2.localScale.y, tr2.localScale.z);
                    }

                    inverseTexture = !inverseTexture;
                }

                InitializeDoor(door_mode);
            }
            else if (door_mode == DoorType.Automatic)
                InitializeDoor(door_mode);

            void SetDoorRoomID(Vector2i doorCheckPos)
            {
                int doorRoomInstanceID = AreaMapController.Map.GetInstanceID(doorCheckPos);
                // FOR STATIC WALLS
                if (doorRoomInstanceID != -1)
                    doorRoomID = AreaMapController.Map.GetObjectID(doorCheckPos);
            }
            switch (wallRotation)
            {
                case Rotation.South:
                case Rotation.West:
                    SetDoorRoomID(new Vector2i(position.x, position.y));
                    break;
                case Rotation.East:
                    SetDoorRoomID(new Vector2i(position.x, position.y + 1));
                    break;
                case Rotation.North:
                    SetDoorRoomID(new Vector2i(position.x + 1, position.y));
                    break;
            }


            firstPartPos = tr.transform.position;
            secondPartPos = tr2.transform.position;

            firstEulerRot = tr.transform.localEulerAngles;
            secondEulerRot = tr2.transform.localEulerAngles;

            // get second wing of the door

            if (sameDoor)
            {
                nextDoor = AreaMapController.Map.GetNearDoorFromMapOfSameRoom(this, 1.5f);

                if (nextDoor != null && nextDoor.nextDoor == null)
                {
                    nextDoor.nextDoor = this;
                }
            }



            if ((doorRoomID == ReferenceHolder.Get().engine.GetMap<Hospital.AreaMapController>().pathBlockerId))
            {
                // fix for static doors west rotation (reception) and playroom east
                if ((nextDoor != null) && (wallRotation == Rotation.West))
                    nextDoor.transform.localScale = new Vector3(-1 * nextDoor.transform.localScale.x, nextDoor.transform.localScale.y, nextDoor.transform.localScale.z);

                if ((mutalBorder != null) && (wallRotation == Rotation.North)) // mutal for north static door of the clinic
                {
                    mutalBorder = GameObject.Instantiate(mutalBorder);
                    mutalBorder.transform.SetParent(transform);

                    if (near_door)
                    {
                        mutalBorder.transform.localPosition = new Vector3(-0.5f, -1f, -1f);
                        mutalBorder.HideLine(3);
                    }
                    else
                    {
                        mutalBorder.transform.localPosition = new Vector3(0.5f, -1f, -1f);
                        mutalBorder.HideLine(1);
                    }

                    mutalBorder.Init();
                    mutalBorder.gameObject.SetActive(false);
                }

            }

            if ((this.gameObject.transform.tag == "Fake") && (mutalBorder != null)) // mutal for fake walls
            {
                mutalBorder = GameObject.Instantiate(mutalBorder);
                mutalBorder.transform.SetParent(transform);
                if (near_door)
                {
                    mutalBorder.transform.localPosition = new Vector3(-0.5f, -1f, 0.1f);
                    mutalBorder.HideLine(3);
                }
                else
                {
                    mutalBorder.transform.localPosition = new Vector3(0.5f, -1f, 0.1f);
                    mutalBorder.HideLine(1);
                }
                mutalBorder.Init();
                mutalBorder.gameObject.SetActive(false);
            }
        }


        protected virtual void InitializeDoor(DoorType dType)
        {
            if (dType == DoorType.Normal)
                openMovement = new Vector3(-openMovement.x, openMovement.y, openMovement.z);
            else
                openMovement = new Vector3(openMovement.x, openMovement.y, openMovement.z);
        }

        protected virtual void OpenDoor(DoorType dType)
        {

            if (tr != null && tr2 != null)
            {
                if (dType == DoorType.Automatic)
                {
                    if (openPercent < 1)
                    {
                        openPercent += delt * 3f; // speed of opening the door

                        if (!near_door)
                        {
                            tr.transform.position = Vector3.Lerp(firstPartPos, firstPartPos + openMovement, openPercent);
                            tr2.transform.position = Vector3.Lerp(secondPartPos, secondPartPos + openMovement, openPercent);
                        }
                        else
                        {
                            tr.transform.position = Vector3.Lerp(firstPartPos, firstPartPos - openMovement, openPercent);
                            tr2.transform.position = Vector3.Lerp(secondPartPos, secondPartPos - openMovement, openPercent);
                        }

                    }
                    else
                    {
                        open = true;
                        time = 0;
                        openPercent = 1;
                        if (!near_door)
                        {
                            tr.transform.position = firstPartPos + openMovement;
                            tr2.transform.position = secondPartPos + openMovement;
                        }
                        else
                        {
                            tr.transform.position = firstPartPos - openMovement;
                            tr2.transform.position = secondPartPos - openMovement;
                        }
                        if (nextDoor != null)
                            nextDoor.openPercent = 1;
                    }
                }
                else if (dType == DoorType.Classic)
                {
                    if (openPercent < 1)
                    {
                        openPercent += delt * 6f; // speed of opening the door

                        if (!near_door)
                        {
                            tr.transform.localEulerAngles = Vector3.Lerp(firstEulerRot, new Vector3(0, 90, 0), openPercent);
                            tr2.transform.localEulerAngles = Vector3.Lerp(secondEulerRot, new Vector3(0, 90, 0), openPercent);
                        }
                        else
                        {
                            tr.transform.localEulerAngles = Vector3.Lerp(firstEulerRot, new Vector3(0, -90, 0), openPercent);
                            tr2.transform.localEulerAngles = Vector3.Lerp(secondEulerRot, new Vector3(0, -90, 0), openPercent);
                        }

                    }
                    else
                    {
                        open = true;
                        time = 0;
                        openPercent = 0;
                        if (!near_door)
                        {
                            tr.transform.localEulerAngles = new Vector3(0, 90, 0);
                            tr2.transform.localEulerAngles = new Vector3(0, 90, 0);
                        }
                        else
                        {
                            tr.transform.localEulerAngles = new Vector3(0, -90, 0);
                            tr2.transform.localEulerAngles = new Vector3(0, -90, 0);
                        }
                    }
                }
            }
        }

        protected virtual void CloseDoor(DoorType dType)
        {
            if (tr != null && tr2 != null)
            {
                //print("closing doors at" + position);
                if (dType == DoorType.Normal)
                {
                    if (rotate)
                    {
                        tr.Rotate(-rotation);
                        tr2.Rotate(-rotation);
                    }
                    tr.Translate(-openMovement);
                    tr2.Translate(-openMovement);
                    if (inverseTexture)
                    {
                        tr.localScale = new Vector3(-1 * tr.localScale.x, tr.localScale.y, tr.localScale.z);
                        tr2.localScale = new Vector3(-1 * tr2.localScale.x, tr2.localScale.y, tr2.localScale.z);
                    }

                    open = false;
                    time = 0;
                }
                else if (dType == DoorType.Automatic)
                {
                    if (openPercent > 0)
                    {
                        openPercent -= delt * 2f;

                        if (CheckPatients())
                        {
                            open = false;
                            //openPercent += time * 1f;
                            OpenDoor(DoorType.Automatic);
                            return;
                        }

                        if (!near_door)
                        {
                            tr.transform.position = Vector3.Lerp(firstPartPos + openMovement, firstPartPos, 1 - openPercent);
                            tr2.transform.position = Vector3.Lerp(secondPartPos + openMovement, secondPartPos, 1 - openPercent);
                        }
                        else
                        {
                            tr.transform.position = Vector3.Lerp(firstPartPos - openMovement, firstPartPos, 1 - openPercent);
                            tr2.transform.position = Vector3.Lerp(secondPartPos - openMovement, secondPartPos, 1 - openPercent);
                        }

                    }
                    else
                    {
                        open = false;
                        time = 0;
                        openPercent = 0;

                        if (!near_door)
                        {
                            tr.transform.position = firstPartPos;
                            tr2.transform.position = secondPartPos;
                        }
                        else
                        {
                            tr.transform.position = firstPartPos;
                            tr2.transform.position = secondPartPos;
                        }

                        istThisNeraDoorToOtherDooor = false;

                        if (nextDoor != null)
                            nextDoor.openPercent = 0;
                    }
                }
                else if (dType == DoorType.Classic)
                {
                    if (openPercent < 1)
                    {
                        openPercent += delt * 3f;

                        if (CheckPatients())
                        {
                            open = false;
                            OpenDoor(DoorType.Classic);
                            return;
                        }

                        if (!near_door)
                        {
                            tr.transform.localEulerAngles = Vector3.Lerp(firstEulerRot + new Vector3(0, 90, 0), firstEulerRot, openPercent);
                            tr2.transform.localEulerAngles = Vector3.Lerp(secondEulerRot + new Vector3(0, 90, 0), secondEulerRot, openPercent);
                        }
                        else
                        {
                            tr.transform.localEulerAngles = Vector3.Lerp(firstEulerRot + new Vector3(0, -90, 0), firstEulerRot, openPercent);
                            tr2.transform.localEulerAngles = Vector3.Lerp(secondEulerRot + new Vector3(0, -90, 0), secondEulerRot, openPercent);
                        }
                    }
                    else
                    {
                        open = false;
                        time = 0;
                        openPercent = 0;

                        if (!near_door)
                        {
                            tr.transform.localEulerAngles = firstEulerRot;
                            tr2.transform.localEulerAngles = secondEulerRot;
                        }
                        else
                        {
                            tr.transform.localEulerAngles = firstEulerRot;
                            tr2.transform.localEulerAngles = secondEulerRot;
                        }
                    }
                }
            }

        }

        void Destroy()
        {
            if (mutalBorder != null)
            {
                GameObject.Destroy(mutalBorder);
            }
            Destroy();
        }
        void Update()
        {
            delt = Time.deltaTime;
            time += delt;

            //	print("checking");
            if (!istThisNeraDoorToOtherDooor || door_mode != DoorType.Automatic)
            {
                if (open)
                {
                    if (time > 2f)
                    {
                        if (!patient_around)
                        {
                            if (nextDoor != null)
                                nextDoor.CloseDoor(door_mode);

                            CloseDoor(door_mode);
                        }
                    }
                }
                else
                {
                    if (time > 0.01f)
                    {
                        if (patient_around)
                        {
                            if (nextDoor != null)
                            {
                                nextDoor.OpenDoor(door_mode);
                            }
                            OpenDoor(door_mode);
                        }
                    }
                }
            }
            else
            {
                time -= delt;
            }
        }

        // Are tiles a and b on opposite sides of the door?
        // NOTE: Assumes that a and b are adjacent to each other
        private bool PointsCrossDoor(Vector2i a, Vector2i b)
        {
            if (wallRotation == Rotation.North || wallRotation == Rotation.South)
                return (a.x == position.x && b.x == position.x + 1 || a.x == position.x + 1 && b.x == position.x)
                    && Mathf.Abs(position.y - a.y) <= 1 && Mathf.Abs(position.y - b.y) <= 1;
            if (wallRotation == Rotation.East || wallRotation == Rotation.West)
                return (a.y == position.y && b.y == position.y + 1 || a.y == position.y + 1 && b.y == position.y)
                    && Mathf.Abs(position.x - a.x) <= 1 && Mathf.Abs(position.x - b.x) <= 1;
            return false;
        }

        public void UpdateWithNearPatient()
        {
            patient_around = CheckPatients();
        }

        public bool CheckPatients()
        {
            if (BasePatientAI.patients != null && BasePatientAI.patients.Count > 0)
            {
                for (int id = 0; id< BasePatientAI.patients.Count; id++)
                {
                    var currentPat = BasePatientAI.patients[id];
                    if (currentPat.isGetingPath() && !currentPat.isMovementStopped())
                    {
                        var pathRange = currentPat.GetPathRange(maxTilesBehind: 1, maxTilesAhead: 1);
                        for (int i = 0; i < pathRange.Count - 1; i++)
                        {
                            if (PointsCrossDoor(pathRange[i], pathRange[i + 1]))
                                return true;
                        }
                    }
                }
            }
            return false;
        }


        public bool CheckPatient(BasePatientAI p, Vector2i curentNodePos, Vector2i beforeNodePos, Vector2i nextNodePos)
        {
            if (nextDoor == null)
                return false;

            var pathRange = p.GetPathRange(maxTilesBehind: 1, maxTilesAhead: 1);
            for (int i = 0; i < pathRange.Count - 1; i++)
            {
                if (PointsCrossDoor(pathRange[i], pathRange[i + 1]))
                    return true;
            }
            return false;
        }

        public enum DoorType
        {
            Normal = 0,
            Automatic = 1,
            Classic = 2,
        }
    }
}
