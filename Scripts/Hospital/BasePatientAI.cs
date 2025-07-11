using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using System;
using TMPro;

namespace Hospital
{
    public class BasePatientAI : ComponentController, IPathRequester
    {
        // Fields

        private GameObject HappyGameObject = null;
        private int CurrentWalkingAnimationHash = 0;
        private int CurrentStandIdleAnimationHash = 0;
        private int CurrentLayInBedAnimation = 0;
        Coroutine _randomBedIdleAnimations;

        public static List<BasePatientAI> patients;
        static BasePatientAI()
        {
            patients = new List<BasePatientAI>();
        }

        public static void ResetAllPatients()
        {
            if (patients != null && patients.Count > 0)
                for (int i = 0; i < patients.Count; i++)
                {
                    if (patients[i] != null)
                        patients[i].IsoDestroy();
                }

            patients.Clear();
        }

        public BaseCharacterInfo sprites
        {
            get;
            protected set;
        }
        protected bool ListeningForPath;

        public float speed = 1.3f;
        public bool isKid = false;
        [HideInInspector] public StateManager walkingStateManager;

        private BaseWalkingState nextState;

        [HideInInspector] public Animator anim;
        protected bool isRight = false;
        protected bool isFront = false;
        readonly Vector2 BackRight = new Vector2(1, 1);
        readonly Vector2 FrontRight = new Vector2(1, -1);
        readonly Vector2 FrontLeft = new Vector2(-1, -1);
        readonly Vector2 BackLeft = new Vector2(-1, 1);

        protected Vector2i destTilePos;
        private Vector2i positionTilePos;

        public Vector2i position
        {
            get
            {
                return positionTilePos;
            }

            internal set
            {
                if (positionTilePos == value)
                    return;

                Vector2i old = positionTilePos;
                positionTilePos = value;
                if (ReferenceHolder.Get().engine.GetMap<PFMapController>() != null)
                    ReferenceHolder.Get().engine.GetMap<PFMapController>().UpdatePatientPosition(this, old, Vector2i.zero);
            }
        }

        public Vector2i destinationTilePos
        {
            get
            {
                return destTilePos;
            }

            private set
            {
                if (destTilePos == value)
                    return;

                Vector2i old = destTilePos;
                destTilePos = value;
                if (destTilePos != position && ReferenceHolder.Get().engine.GetMap<PFMapController>() != null)
                    ReferenceHolder.Get().engine.GetMap<PFMapController>().UpdatePatientPosition(this, Vector2i.zero, old);
            }
        }

        private void OnDisable()
        {
            if (_randomBedIdleAnimations != null)
            {
                try { 
                    StopCoroutine(_randomBedIdleAnimations);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
            }
        }

        public bool isGetingPath()
        {
            return ListeningForPath;
        }

        public void abortPath()
        {
            ListeningForPath = false;
        }

        public string debugState;

        public void SetDefaultWalkingAnimation(int hash)
        {
            CurrentWalkingAnimationHash = hash;
        }

        public void SetDefaultStandIdleAnimation(int hash)
        {
            CurrentStandIdleAnimationHash = hash;
        }

        public void SetDefaultLayInBedAnimation(int hash)
        {
            CurrentLayInBedAnimation = hash;
        }

        public void PlayWalkingAnimation()
        {
            try
            {
                anim.Play(CurrentWalkingAnimationHash,0);//, 0, 0.0f);
            }
            catch (Exception e) {
                Debug.LogWarning("Animator - exception:  " + e.Message);
            }
        }

        public void PlayStandIdleAnimation()
        {
            try {
                anim.Play(CurrentStandIdleAnimationHash, 0, 0.0f);
            }
            catch (Exception e) {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }

        public void PlayLayInBedAnimation()
        {
            try {
                anim.Play(CurrentLayInBedAnimation, 0, 0.0f);
            }
            catch (Exception e) {
                Debug.LogWarning("Animator - exception: : " + e.Message);
            }
        }

        public virtual string SaveToString()
        {
            return "";
        }

        public virtual void Initialize(Vector2i pos)
        {
            walkingStateManager = new StateManager();
            transform.position = new Vector3(pos.x, 0, pos.y);
            position = pos;
            CurrentWalkingAnimationHash = AnimHash.Walk;
            CurrentStandIdleAnimationHash = AnimHash.Stand_Idle;
            CurrentLayInBedAnimation = AnimHash.Bed_Idle;

            sprites = GetComponent<BaseCharacterInfo>();
            if (!(this is DoctorController)/* && !(this is VIPPersonController)*/)
                patients.Add(this);
            anim = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
            ListeningForPath = false;

            if (sprites is ClinicCharacterInfo)
            {
                if (this is ChildPatientAI)
                {
                    ((ClinicCharacterInfo)sprites).SetPositiveEnergyIcon(!AreaMapController.Map.VisitingMode);
                }
                else
                    ((ClinicCharacterInfo)sprites).SetPositiveEnergyIcon(false);
            }
        }
        public virtual void Initialize(RotatableObject room, string info)
        {

        }
        public virtual void Initialize(string info, int timeFromSave)
        {

        }

        public void AddHappyEffect(RotatableObject room)
        {
            if (HappyGameObject != null || this == null || gameObject == null)
            {
                return;
            }
            if (room is DoctorRoom)
            {
                HappyGameObject = Instantiate(ResourcesHolder.GetHospital().PatientHappyPrefab);
            }
            else
            {
                HappyGameObject = Instantiate(ResourcesHolder.GetHospital().PatientDiagnosePrefab);
            }
            HappyGameObject.transform.SetParent(gameObject.transform);
            HappyGameObject.transform.localPosition = GetHappyEffectPosition(room);
            HappyGameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            HappyGameObject.transform.localScale = new Vector3(1, 1, 1);
        }

        public void RemoveHappyEffect()
        {
            if (HappyGameObject != null)
            {
                GameObject.Destroy(HappyGameObject);
                HappyGameObject = null;
            }
        }

        bool isoDestuction;

        public override void IsoDestroy()
        {
            if (this == null) return; //fix for random error "The object of type 'ChildPatientAI' has been destroyed but you are still trying to access it."

            if (!isoDestuction) StartCoroutine(IsoDestruction());
        }

        IEnumerator IsoDestruction()
        {
            isoDestuction = true;
            GeneralCharacterReset();

            var nurseController = this.GetComponent<NurseCloudController>();
            var doctorController = this.GetComponent<DoctorCloudController>();
            var hospitalPatientController = this.GetComponent<HospitalPatientCloudController>();
            var clinicPatientController = this.GetComponent<ClinicPatientCloudController>();

            if (nurseController != null)
            {
                PutCharacterBackToPooler(CharactersList.instance.nurses, ref CharactersList.instance.nursesTaken, CharactersList.instance.nurseName);
            }
            else if (doctorController != null)
            {
                PutCharacterBackToPooler(CharactersList.instance.doctors, ref CharactersList.instance.doctorsTaken, CharactersList.instance.doctorName);
            }
            else if (hospitalPatientController != null)
            {
                PutCharacterBackToPooler(CharactersList.instance.hospitalPatients, ref CharactersList.instance.hospitalTaken, CharactersList.instance.hospitalPatientName);
                ResetCharacterInfo();
                Destroy(this, 1.0f); //deleting unused script with delay so that it won't delete itself before deactivation
            }
            else if (clinicPatientController != null)
            {
                PutCharacterBackToPooler(CharactersList.instance.clinicPatients, ref CharactersList.instance.clinicTaken, CharactersList.instance.clinicPatientName);
                ResetCharacterInfo();
                Destroy(this, 1.0f); //deleting unused script with delay so that it won't delete itself before deactivation
            }

            //maybe let's try calling this on the main thread?
            try {
                anim.Play(AnimHash.Stand_Idle, 0, 0.0f); //reset the animation to an idle state
            }
            catch (Exception e) {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }


            yield return null; //have a delay before deactivation

            if (!WasCharacterTakenByCreator(nurseController, doctorController, hospitalPatientController, clinicPatientController)) this.gameObject.SetActive(false);

            isoDestuction = false;
        }

        private void GeneralCharacterReset()
        {
            ListeningForPath = false;
            patients.Remove(this);
            RemoveHappyEffect();

            //deactivate hearts
            Transform hearts = this.transform.Find("ParticlesHeartsCured");
            if (hearts != null) hearts.gameObject.SetActive(false);

            //reset local scale for kids, so if a character is reused as an adult, it'll have a proper size
            if (isKid) this.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        //ClinicCharacterInfo & HospitalCharacterInfo share info variables in BaseCharacterInfo
        protected virtual void ResetCharacterInfo()
        {
            BaseCharacterInfo characterInfo = this.GetComponent<BaseCharacterInfo>();
            if (characterInfo == null) return;

            characterInfo.IsVIP = false;
            characterInfo.RequiresDiagnosis = false;
            characterInfo.HelpRequested = false;
            characterInfo.HasBacteria = false;
            characterInfo.BacteriaGlobalTime = 0;
            characterInfo.AWS_InfectionTime = 0;
        }

        private void PutCharacterBackToPooler(List<GameObject> characterPooler, ref List<bool> characterTaken, string characterName)
        {
            int index = characterPooler.IndexOf(this.gameObject);
            if (index != -1) characterTaken[index] = false;
            this.name = string.Format("{0}_{1}", CharactersList.instance.hospitalPatientName, index);
        }

        private bool WasCharacterTakenByCreator(NurseCloudController nurseController,
                                                DoctorCloudController doctorController,
                                                HospitalPatientCloudController hospitalPatientController,
                                                ClinicPatientCloudController clinicPatientController)
        {
            bool taken = false;
            if (nurseController != null)
            {
                int index = CharactersList.instance.nurses.IndexOf(this.gameObject);
                if (index != -1) taken = CharactersList.instance.nursesTaken[index];
            }
            else if (doctorController != null)
            {
                int index = CharactersList.instance.doctors.IndexOf(this.gameObject);
                if (index != -1) taken = CharactersList.instance.doctorsTaken[index];
            }
            else if (hospitalPatientController != null)
            {
                int index = CharactersList.instance.hospitalPatients.IndexOf(this.gameObject);
                if (index != -1) taken = CharactersList.instance.hospitalTaken[index];
            }
            else if (clinicPatientController != null)
            {
                int index = CharactersList.instance.clinicPatients.IndexOf(this.gameObject);
                if (index != -1) taken = CharactersList.instance.clinicTaken[index];
            }
            return taken;
        }

        public virtual void Notify(int id, object parameters = null)
        {

        }

        public void SetRandomHurrayAnimation()
        {
            var r = BaseGameState.RandomNumber(0, 10) / 10f;
            if (r > .8f)
            {
                try
                {
                    anim.Play(AnimHash.Hurray2, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
            else if (r > .6f)
            {
                try
                {
                    anim.Play(AnimHash.Hurray3, 0, 0.0f);
                }
                catch (Exception e) {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
            else if (r > .4f)
            {
                try
                {
                    anim.Play(AnimHash.Hurray4, 0, 0.0f);
                }
                catch (Exception e) {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
            else if (r > .2f)
            {
                try
                {
                    anim.Play(AnimHash.Hurray5, 0, 0.0f);
                }
                catch (Exception e) {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
            else {
                try
                {
                    anim.Play(AnimHash.Hurray, 0, 0.0f);
                }
                catch (Exception e) {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
        }

        public void SetRandomStandAnimation()
        {
            float r = BaseGameState.RandomNumber(0, 10) / 10f;

            if (r >= .9f)
            {
                try { 
                    anim.Play(AnimHash.Stand_Sneeze, 0, 0.0f);
                    SoundsController.Instance.PlayCough();
                }catch (Exception e){
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }else if (r >= .6f){
                try{ 
                    anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                }catch(Exception e){
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
        }else if (r >= .3f){
                try{ 
                    anim.Play(AnimHash.Stand_Read, 0, 0.0f);
                }catch(Exception e){
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }else{
                try{ 
                    anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                }catch (Exception e){
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
    }
        }

        public void SetRandomSitAnimation()
        {
            float r = BaseGameState.RandomNumber(0, 10) / 10f;
            /*
            if (r >= .6f)
            {
                anim.SetBool("listen", false);
                anim.SetBool("relaxed", false);
            }
            else if (r >= .1f)
            {
                anim.SetBool("listen", true);
                anim.SetBool("relaxed", false);
            }
            else
            {
                anim.SetBool("listen", false);
                anim.SetBool("relaxed", true);
            }
            */
            anim.speed = BaseGameState.RandomNumber(5, 10) / 10f;
        }

        private Vector3 GetHappyEffectPosition(RotatableObject room)
        {
            Vector3 defaultPosition = new Vector3(0.42f, 0.21f, -1f);
            if (room is DoctorRoom)
            {
                DoctorRoom temp = room as DoctorRoom;
                if (temp.actualData.rotation == Rotation.North || temp.actualData.rotation == Rotation.South)
                {
                    return new Vector3(-0.33f, 0.21f, -1f);
                }
            }
            else if (room is DiagnosticRoom)
            {
                defaultPosition = new Vector3(0.1f, 0.21f, -1);
                DiagnosticRoom temp = room as DiagnosticRoom;
            }
            return defaultPosition;
        }

        protected void SetHealingAnimation(RotatableObject room, int kids = -1)
        {
            if (this == null)
                return;

            if (room is DoctorRoom)
            {
                DoctorRoom temp = room as DoctorRoom;
                switch (temp.actualData.rotation)
                {
                    case Rotation.North:
                        isFront = true;
                        isRight = true;
                        break;

                    case Rotation.South:
                        isFront = true;
                        isRight = true;
                        break;

                    case Rotation.East:
                        isFront = true;
                        isRight = false;
                        break;

                    case Rotation.West:
                        isFront = true;
                        isRight = false;
                        break;
                }
                SetAnimationDirection(isFront, isRight);



                if (kids == 0)
                    transform.position = temp.GetMachinePosition();
                else if (kids == 1)
                    transform.position = temp.GetMachinePosition(1);
                else
                    transform.position = temp.GetMachineObject().transform.GetChild(0).transform.position;

                PlayDoctorHealingAnimation(temp);

            }
            else if (room is DiagnosticRoom)
            {
                DiagnosticRoom temp = room as DiagnosticRoom;
                switch (temp.actualData.rotation)
                {
                    case Rotation.North:
                        isFront = true;
                        isRight = true;
                        break;

                    case Rotation.South:
                        isFront = true;
                        isRight = true;
                        break;

                    case Rotation.East:
                        isFront = true;
                        isRight = false;
                        break;

                    case Rotation.West:
                        isFront = true;
                        isRight = false;
                        break;
                }
                SetAnimationDirection(isFront, isRight);

                transform.position = temp.GetMachinePositionForPatient();
                //transform.position = temp.GetMachineObject().transform.GetChild(0).transform.position;

                PlayDiagnosticHealingAnimation(temp);
            }

        }

        protected virtual void LayInBed(RotatableObject room, GameObject bed)
        {
            HospitalRoom temp = room as HospitalRoom;
            switch (temp.actualData.rotation)
            {
                case Rotation.North:
                    isFront = true;
                    isRight = true;
                    break;

                case Rotation.South:
                    isFront = true;
                    isRight = true;
                    break;

                case Rotation.East:
                    isFront = true;
                    isRight = false;
                    break;

                case Rotation.West:
                    isFront = true;
                    isRight = false;
                    break;
            }

            if (bed != null)
            {
                gameObject.SetActive(true);
                transform.position = bed.transform.GetChild(0).transform.position;
                try { 
                    anim.Play(AnimHash.Bed_Idle, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            _randomBedIdleAnimations = StartCoroutine(RandomBedIdleAnimations());
                SetAnimationDirection(isFront, isRight);
            }
            else
            {
                if (gameObject.activeInHierarchy)
                {
                    Transform animatorT = gameObject.transform.GetChild(0);
                    animatorT.GetChild(8).gameObject.SetActive(true);
                    animatorT.GetChild(9).gameObject.SetActive(true);
                    animatorT.GetChild(10).gameObject.SetActive(true);
                    animatorT.GetChild(11).gameObject.SetActive(true);
                    animatorT.GetChild(12).gameObject.SetActive(true);
                    animatorT.GetChild(13).gameObject.SetActive(true);
                    animatorT.GetChild(14).gameObject.SetActive(true);
                    gameObject.SetActive(false);
                }
            }
        }

        IEnumerator RandomBedIdleAnimations()
        {
            yield return new WaitForSeconds(BaseGameState.RandomFloat(3f, 6f));

            while (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == AnimHash.Bed_Idle)
            {
                float r = UnityEngine.Random.value;
                if (r >= .75f)
                {
                    try { 
                    anim.Play(AnimHash.Bed_Handsmacking, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                yield return new WaitForSeconds(BaseGameState.RandomFloat(6f, 8f));
                }
                else if (r >= .35f)
                {
                    try {
                    anim.Play(AnimHash.Bed_Reading, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                yield return new WaitForSeconds(BaseGameState.RandomFloat(15f, 18f));
                }
                else
                {
                    yield return new WaitForSeconds(BaseGameState.RandomFloat(3f, 8f));
                }
            };
        }

        protected void StartDoctorWaitingSitAnimation(RotatableObject room)
        {
            if (this == null)
                return;

            switch (room.actualData.rotation)
            {
                case Rotation.North:
                    transform.position += BaseResourcesHolder.ChairOffsetNorth;
                    isFront = false;
                    isRight = true;
                    break;

                case Rotation.South:
                    transform.position += BaseResourcesHolder.ChairOffsetSouth;
                    isFront = true;
                    isRight = false;
                    break;

                case Rotation.East:
                    transform.position += BaseResourcesHolder.ChairOffsetEast;
                    isFront = true;
                    isRight = true;
                    break;

                case Rotation.West:
                    transform.position += BaseResourcesHolder.ChairOffsetWest;
                    isFront = false;
                    isRight = false;
                    break;
            }
            SetAnimationDirection(isFront, isRight);
            try { 
            anim.Play(AnimHash.Sit_Idle, 0, 0.0f);
            }catch (Exception e) {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
}

        public Rotation GetRotation()
        {
            if (isFront && isRight)
                return Rotation.North;
            else
                return Rotation.East;
        }

        private void PlayDiagnosticHealingAnimation(RotatableObject room)
        {
            DiagnosticRoom temp = room as DiagnosticRoom;
            switch (temp.ReturnMachineType())
            {
                case HospitalDataHolder.DiagRoomType.LungTesting:
                    try
                    {
                        anim.Play(AnimHash.Treatment_Head, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case HospitalDataHolder.DiagRoomType.Laser:
                    try {
                    anim.Play(AnimHash.Stand_Unmoving, 0, 0.0f);
                    }
                    catch (Exception e) {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case HospitalDataHolder.DiagRoomType.MRI:
                    try
                    {
                        anim.Play(AnimHash.Stand_Unmoving, 0, 0.0f);
                    }
                    catch (Exception e) {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case HospitalDataHolder.DiagRoomType.UltraSound:
                    try
                    {
                        anim.Play(AnimHash.Stand_Unmoving, 0, 0.0f);
                    }
                    catch (Exception e) {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case HospitalDataHolder.DiagRoomType.XRay:
                    try
                    {
                        anim.Play(AnimHash.Stand_Unmoving, 0, 0.0f);
                    }
                    catch (Exception e) {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
            }
        }

        private void PlayDoctorHealingAnimation(DoctorRoom room)
        {
            switch (room.ReturnMachineType())
            {
                case DoctorMachineType.Blue:
                    try { 
                        anim.SetBool("relaxed", true);
                        anim.Play(AnimHash.Sit_Relaxed, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case DoctorMachineType.Green:
                    try { 
                    anim.Play(AnimHash.Treatment_Green, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case DoctorMachineType.Pink:
                    try { 
                    anim.Play(AnimHash.Treatment_Green, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case DoctorMachineType.Purple:
                    try { 
                    anim.Play(AnimHash.Treatment_Purple, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case DoctorMachineType.Red:
                    try { 
                    anim.SetBool("relaxed", false);
                    anim.Play(AnimHash.Sit_Machinetreatment, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case DoctorMachineType.SkyBlue:
                    try { 
                    anim.SetBool("relaxed", false);
                    anim.Play(AnimHash.Sit_Machinetreatment, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case DoctorMachineType.SunnyYellow:
                    try { 
                    anim.Play(AnimHash.Treatment_Head, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case DoctorMachineType.White:
                    try { 
                    anim.SetBool("relaxed", false);
                    anim.Play(AnimHash.Sit_Machinetreatment, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                case DoctorMachineType.Yellow:
                    try { 
                    anim.SetBool("relaxed", true);
                    anim.Play(AnimHash.Sit_Relaxed, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
                default:
                    try { 
                    anim.Play(AnimHash.Treatment_Green, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    break;
            }
        }

        protected void CheckAnimationDirection(Vector3 actual, Vector3 next)
        {
            isRight = true;
            isFront = true;

            if ((actual.x - next.x) > 0)
            {
                isFront = true;
                isRight = false;
            }
            else if ((actual.x - next.x) < 0)
            {
                isFront = false;
                isRight = true;
            }

            if ((actual.z - next.z) > 0)
            {
                isFront = true;
                isRight = true;
            }
            else if ((actual.z - next.z) < 0)
            {
                isFront = false;
                isRight = false;
            }

            if (actual.z == next.z)
            {
                if ((actual.x - next.x) < 0)
                {
                    isRight = true;
                    isFront = false;
                }

                if ((actual.x - next.x) > 0)
                {
                    isRight = false;
                    isFront = true;
                }
            }
        }

        protected void SetAnimationDirection(bool isFront = true, bool isRight = true)
        {
            if (this == null || anim == null)
                return;

            if (isFront && isRight)
            {
                anim.SetFloat("tile_X", FrontRight.x);
                anim.SetFloat("tile_Y", FrontRight.y);
            }
            else if (isFront && !isRight)
            {
                anim.SetFloat("tile_X", FrontLeft.x);
                anim.SetFloat("tile_Y", FrontLeft.y);
            }
            else if (!isFront && isRight)
            {
                anim.SetFloat("tile_X", BackRight.x);
                anim.SetFloat("tile_Y", BackRight.y);
            }
            else if (!isFront && !isRight)
            {
                anim.SetFloat("tile_X", BackLeft.x);
                anim.SetFloat("tile_Y", BackLeft.y);
            }

        }
        public virtual ClinicPatientData GetData()
        {
            ClinicPatientData data = new ClinicPatientData();

            data.position = position;
            data.controller = typeof(BasePatientAI);

            return data;
        }
        public void TeleportTo(Vector2i pos)
        {
            position = pos;
            transform.position = new Vector3(pos.x, 0, pos.y);
        }

        public void TeleportTo(Vector3 pos)
        {
            transform.position = pos;
        }

        protected virtual void Update()
        {
            if (walkingStateManager != null)
                walkingStateManager.Update();

            /* // DEBUG STATE MAKE GAME SLOW
            if (walkingStateManager.State != null)
                debugState = walkingStateManager.State.ToString();
            else
                debugState = "NULL";
            */
        }


        public void GoTo(Vector2i pos, PathType pathType)
        {
            // ReferenceHolder.Get().engine.AddTask(() =>  // Stand animation for every beginning path
            //  {
            try { 
            SetAnimationDirection(isFront, isRight);

            anim.Play(CurrentStandIdleAnimationHash, 0, 0.0f);
            }
            catch (Exception e) {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
    //  });

    // pos = Vector2i.zero;
    ((PFMapController)ReferenceHolder.Get().engine.Map).OrderPath(this, 0, position, 0, pos, pathType);
            destinationTilePos = pos;
            ListeningForPath = true;
        }

        public void GoTo(QueueableSpot.SpotData spot)
        {
            //print("ret ppl going to death at: " + spot.X+" "+spot.Y);
            engineController.GetMap<PFMapController>().OrderPath(this, 0, position, spot.LevelID, new Vector2i(spot.X, spot.Y), PathType.Default);
            ListeningForPath = true;
        }

        protected virtual void ReachedDestination()
        {
        }

        public void StopMovement()
        {
            if (walkingStateManager.State is BaseWalkingState)
            {
                walkingStateManager.State.OnExit();
                walkingStateManager.State = null;
            }
        }

        public bool isMovementStopped()
        {
            if (walkingStateManager.State is BaseWalkingState)
                return false;
            else return true;
        }

        // If in walking state, return a range of tiles around the current tile in the path
        public List<Vector2i> GetPathRange(int maxTilesBehind, int maxTilesAhead)
        {
            var walkingState = walkingStateManager.State as BaseWalkingState;
            return walkingState?.GetPathRange(maxTilesBehind, maxTilesAhead);
        }

        public void SetPath(InterLevelPathInfo path)
        {
            if (!ListeningForPath)
                return;

            ReferenceHolder.Get().engine.AddTask(() =>
            {
                if (this == null || gameObject == null)
                    return;
                if (walkingStateManager.State == null)
                {
                    walkingStateManager.State = new BaseWalkingState(this, path, speed);
                }
                else
                {
                    nextState = new BaseWalkingState(this, path, speed);
                }
            });
        }

        public StateManager GetWalkingStateManager()
        {
            return walkingStateManager;
        }

        #region States

        public class BaseWalkingState : IState
        {
            protected BasePatientAI parent;

            private InterLevelPathInfo pathInfo;
            public float movingSpeed = 0;

            public BaseWalkingState(BasePatientAI parent, InterLevelPathInfo pathInfo, float speed)
            {
                this.parent = parent;
                this.pathInfo = pathInfo;
                this.movingSpeed = speed;
            }
            public virtual string SaveToString()
            {
                return "";
            }

            private Vector3 actualTile;
            private Vector3 nextTile;
            private Vector3 actualTilePosition;
            private Vector3 nextTilePosition;
            private int actualTileNumber;

            private List<int> nodesIdNearDoor;

            public void OnEnter()
            {
                if (pathInfo == null)
                {
                    parent.CheckAnimationDirection(actualTile, nextTile);
                    parent.SetAnimationDirection(parent.isFront, parent.isRight);

                    parent.ReachedDestination();
                    parent.walkingStateManager.State = null;
                    return;
                }

                if (pathInfo.paths[0].path.Count == 1)
                {
                    parent.ReachedDestination();
                    parent.walkingStateManager.State = null;

                    return;
                }

                actualTilePosition = actualTile = parent.transform.position;
                //doorPos = HospitalAreasMapController.Map.CheckIsPositionInDoorNearAndGetCenterOfDoor(pathInfo.paths[0].path[actualTileNumber], 1);
                nodesIdNearDoor = AreaMapController.Map.CheckIsAnyDoorOnPathAndInWhichPostion(parent, pathInfo.paths[0].path);


                if (nodesIdNearDoor.Contains(0))
                    doorPos = AreaMapController.Map.CheckIsPositionInDoorNearAndGetCenterOfDoor(pathInfo.paths[0].path[actualTileNumber], parent.transform.position);

                var p = pathInfo.paths[0].path[1];
                nextTile = new Vector3(p.x, 0, p.y);

                nextTilePosition = nextTile;
                actualTileNumber = 1;
                travelPercent = 0.0f;

                parent.CheckAnimationDirection(actualTile, nextTile);
                parent.SetAnimationDirection(parent.isFront, parent.isRight);
            }

            public void Notify(int id, object parameters)
            {

            }

            private float travelPercent = 0;
            private float currentMovementSpeed = 0;
            private Vector3 doorPos;
            private bool wasDoor = false;

            public void OnUpdate()
            {
                if (!parent.ListeningForPath)
                {
                    return;
                }

                if (((int)actualTile.x != (int)nextTile.x && (int)actualTile.z == (int)nextTile.z) || ((int)actualTile.z != (int)nextTile.z) && (int)actualTile.x == (int)nextTile.x)
                    currentMovementSpeed = movingSpeed;
                else currentMovementSpeed = movingSpeed * 0.75f;

                float calcedPathDistBeetwenStates = 1;

                var absX = (((int)actualTilePosition.x - (int)nextTilePosition.x) ^ (((int)actualTilePosition.x - (int)nextTilePosition.x) >> 31)) - (((int)actualTilePosition.x - (int)nextTilePosition.x) >> 31);//Mathf.Abs(actualTilePosition.x - nextTilePosition.x);
                var absZ = (((int)actualTilePosition.z - (int)nextTilePosition.z) ^ (((int)actualTilePosition.z - (int)nextTilePosition.z) >> 31)) - (((int)actualTilePosition.z - (int)nextTilePosition.z) >> 31);//Mathf.Abs(actualTilePosition.z - nextTilePosition.z);

                if (absX > 0.1)
                    calcedPathDistBeetwenStates = absX;

                if (calcedPathDistBeetwenStates < absZ)
                    calcedPathDistBeetwenStates = absZ;

                travelPercent += Time.deltaTime * (currentMovementSpeed / calcedPathDistBeetwenStates);

                if (actualTilePosition != nextTilePosition)
                {
                    if (parent.isKid)
                    {
                        try { 
                            parent.anim.Play(AnimHash.Walk_Kid,0);//, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }

                    }else { parent.PlayWalkingAnimation(); }

                    if (actualTileNumber < pathInfo.paths[0].path.Count)
                    {
                        if (doorPos != Vector3.zero && (pathInfo.paths[0].path.Count - actualTileNumber > 5 || (actualTileNumber > 5 && pathInfo.paths[0].path.Count - actualTileNumber > 2) || parent.GetType() == typeof(HospitalPatientAI)))
                        {
                            parent.transform.position = Vector3.Lerp(actualTilePosition, doorPos, travelPercent);

                            if (!wasDoor)
                            {
                                nextTilePosition = doorPos;
                                actualTileNumber++;
                                wasDoor = true;
                            }
                        }
                        else
                        {
                            parent.transform.position = Vector3.Lerp(actualTilePosition, nextTilePosition, travelPercent);
                            parent.CheckAnimationDirection(actualTile, nextTile);
                        }
                    }
                    else
                    {
                        parent.transform.position = Vector3.Lerp(actualTilePosition, nextTilePosition, travelPercent);
                        parent.CheckAnimationDirection(actualTile, nextTile);
                    }

                    parent.SetAnimationDirection(parent.isFront, parent.isRight);
                }
                else parent.PlayStandIdleAnimation();


                if (travelPercent > 1.0f)
                {
                    // reset next state if exist

                    if (parent.nextState != null)
                    {
                        parent.walkingStateManager.State = parent.nextState;
                        parent.nextState = null;
                        return;
                    }

                    actualTileNumber += 1;

                    if (doorPos != Vector3.zero)
                    {
                        doorPos = Vector3.zero;
                        wasDoor = false;
                    }

                    // if next step numbers is over paths count number and stop update

                    if (pathInfo.paths[0].path == null || actualTileNumber >= pathInfo.paths[0].path.Count)
                    {
                        parent.transform.position = nextTilePosition;
                        parent.position = new Vector2i((int)(nextTile.x), (int)(nextTile.z));
                        parent.ReachedDestination();
                        parent.walkingStateManager.State = null;
                        return;
                    }

                    // set next tile as current
                    //doorPos = HospitalAreasMapController.Map.CheckIsPositionInDoorNearAndGetCenterOfDoor(pathInfo.paths[0].path[actualTileNumber], parent.transform.position, 1);

                    actualTile = nextTile;
                    actualTilePosition = nextTilePosition;

                    parent.transform.position = nextTilePosition;
                    parent.position = new Vector2i((int)actualTilePosition.x, (int)actualTilePosition.z);

                    // Getting info about next tile

                    if (nodesIdNearDoor.Contains(actualTileNumber))
                    {
                        //Debug.LogWarning(actualTileNumber);
                        doorPos = AreaMapController.Map.CheckIsPositionInDoorNearAndGetCenterOfDoor(pathInfo.paths[0].path[actualTileNumber], parent.transform.position);
                    }

                    nextTile = new Vector3(pathInfo.paths[0].path[actualTileNumber].x, 0, pathInfo.paths[0].path[actualTileNumber].y);

                    if (pathInfo.paths[0].path.Count - 1 == actualTileNumber)
                        nextTilePosition = nextTile;
                    else
                        nextTilePosition = (new Vector3(nextTile.x + (nextTile.z - actualTile.z) * 0.1f, 0, nextTile.z + (actualTile.x - nextTile.x) * 0.1f));

                    parent.CheckAnimationDirection(actualTile, nextTile);
                    parent.SetAnimationDirection(parent.isFront, parent.isRight);
                    travelPercent = 0;
                }
            }

            public void OnExit()
            {

                if (travelPercent > 0.5f)
                {
                    parent.position = new Vector2i((int)nextTile.x, (int)nextTile.z);
                }
            }

            // Return a range of tiles around the current tile in the path
            public List<Vector2i> GetPathRange(int maxTilesBehind, int maxTilesAhead)
            {
                if (pathInfo == null || pathInfo.paths == null || pathInfo.paths.Count == 0)
                    return null;
                var path = pathInfo.paths[0].path;
                int startIndex = Math.Max(actualTileNumber - maxTilesBehind, 0);
                int count = Math.Min(actualTileNumber - startIndex + maxTilesAhead + 1, path.Count - startIndex);
                return path.GetRange(startIndex, count);
            }
        }
        #endregion States

        public void ShowNamePatientcured(Transform floatingInfo, HospitalCharacterInfo info)
        {
            if (!AreaMapController.Map.VisitingMode)
            {
                if (!info.Name.Contains("_"))
                {
                    floatingInfo.GetComponent<TextMeshPro>().text = info.Name + " " + info.Surname + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                }
                else
                {
                    floatingInfo.GetComponent<TextMeshPro>().text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + info.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + info.Surname) + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                }
                //floatingInfo.GetComponent<TextMeshPro> ().SetText (info.Name + " " + info.Surname + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!");
                floatingInfo.gameObject.SetActive(true);
            }
        }
    }
    public class ClinicPatientData
    {
        public Vector2i position;
        public System.Type controller;
    }


}