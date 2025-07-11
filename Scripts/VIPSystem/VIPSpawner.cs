using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using System;


namespace Hospital
{
    public class VIPSpawner : MonoBehaviour //czy może lepiej to wrzucić do hospital AISpawnera?
    {
        [SerializeField]
        private GameObject VIPPrefab = null;
        public VIPDatabase VIPdatabase = null;
        public GameObject VIP_gfxPrefab = null;
        [SerializeField]
        private VIPSystemManager mVIPSystemManager = null;
        [SerializeField]
        private VipRoom mVipRoom = null;

        [SerializeField]
        private float speed = 1;

        private GameObject Character;
        private BaseCharacterInfo info;

        private SpriteRenderer AvatarHead;
        private SpriteRenderer AvatarBody;
        private SpriteRenderer HeadFront;
        private SpriteRenderer HeadBack;
        private SpriteRenderer TorsoFront;
        private SpriteRenderer TorsoBack;
        private SpriteRenderer LowerBodyFront;
        private SpriteRenderer ArmsL;
        private SpriteRenderer HandsLFront;
        private SpriteRenderer ArmsR;
        private SpriteRenderer HandsRFront;
        private SpriteRenderer ThighL;
        private SpriteRenderer CalfL;
        private SpriteRenderer FootLFront;
        private SpriteRenderer ThighR;
        private SpriteRenderer CalfR;
        private SpriteRenderer FootRFront;
        private SpriteRenderer ApronFL;
        private SpriteRenderer ApronB;
        private SpriteRenderer ApronF;
        private SpriteRenderer ApronFR;

        bool tutorialMode = false;

        public void SpawnVIP(string VIPDefinition)
        {
            GameObject VIP = CreateDefinedVIP(VIPDefinition);
            mVipRoom.currentVip = VIP;
            var AI = VIP.AddComponent<VIPPersonController>();
            HospitalCharacterInfo vipInfo = VIP.GetComponent<HospitalCharacterInfo>();
            vipInfo.SetRequiredMedicines(ReferenceHolder.GetHospital().vipSystemManager.GetRequiredMedicines());

            AI.transform.rotation = Quaternion.Euler(45, 45, 0);

            AI.definition = VIPDefinition;
            AI.speed = speed;
            if (tutorialMode)
                AI.Initialize(ResourcesHolder.GetHospital().VIPspots[5]); // This spot will make Leo spawn close to door
            else
                AI.Initialize(ResourcesHolder.GetHospital().VIPspots[0]); // All other vips will be spawning next to Helicopter
            Character.name = "VIP";
            VIP.transform.SetParent(this.transform);
            Character.SetActive(true);
        }

        [TutorialTriggerable]
        public void SetTutorialMode(bool isOn) { tutorialMode = isOn; }

        private GameObject CreateDefinedVIP(string VIPDefinition)
        {
            Character = Instantiate(VIPPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            info = Character.GetComponent<HospitalCharacterInfo>();
            GameObject VIP = GenerateDefinedVIP(VIPDefinition);
            info.Initialize();
            GameObject gfx = Instantiate(VIP_gfxPrefab);
            gfx.transform.SetParent(((HospitalCharacterInfo)info).VIPWrapper.transform);
            gfx.transform.localPosition = new Vector3(0, 0, 0);
            gfx.transform.localRotation = Quaternion.Euler(0, 0, 0);
            gfx.transform.localScale = new Vector3(1, 1, 1);
            return VIP;
        }

        private GameObject GenerateDefinedVIP(string VIPDefinition)
        {
            var vipdata = VIPDefinition.Split('/');

            HeadFront = Character.transform.GetChild(0).transform.GetChild(0).GetComponent<SpriteRenderer>();
            HeadBack = Character.transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>();
            TorsoFront = Character.transform.GetChild(0).transform.GetChild(2).GetComponent<SpriteRenderer>();
            TorsoBack = Character.transform.GetChild(0).transform.GetChild(3).GetComponent<SpriteRenderer>();
            ArmsR = Character.transform.GetChild(0).transform.GetChild(4).GetComponent<SpriteRenderer>();
            HandsRFront = Character.transform.GetChild(0).transform.GetChild(5).GetComponent<SpriteRenderer>();
            ArmsL = Character.transform.GetChild(0).transform.GetChild(6).GetComponent<SpriteRenderer>();
            HandsLFront = Character.transform.GetChild(0).transform.GetChild(7).GetComponent<SpriteRenderer>();
            LowerBodyFront = Character.transform.GetChild(0).transform.GetChild(8).GetComponent<SpriteRenderer>();
            ThighR = Character.transform.GetChild(0).transform.GetChild(9).GetComponent<SpriteRenderer>();
            CalfR = Character.transform.GetChild(0).transform.GetChild(10).GetComponent<SpriteRenderer>();
            FootRFront = Character.transform.GetChild(0).transform.GetChild(11).GetComponent<SpriteRenderer>();
            ThighL = Character.transform.GetChild(0).transform.GetChild(12).GetComponent<SpriteRenderer>();
            CalfL = Character.transform.GetChild(0).transform.GetChild(13).GetComponent<SpriteRenderer>();
            FootLFront = Character.transform.GetChild(0).transform.GetChild(14).GetComponent<SpriteRenderer>();

            AvatarHead = Character.transform.GetChild(0).transform.GetChild(16).GetComponent<SpriteRenderer>();
            AvatarBody = Character.transform.GetChild(0).transform.GetChild(17).GetComponent<SpriteRenderer>();

            VIPDatabase.VIP_BIO BIO = VIPdatabase.VIPgender[int.Parse(vipdata[0], System.Globalization.CultureInfo.InvariantCulture)].VIPrace[int.Parse(vipdata[1], System.Globalization.CultureInfo.InvariantCulture)].VIPbio[int.Parse(vipdata[2], System.Globalization.CultureInfo.InvariantCulture)];

            info.Name = BIO.VIPinfo.name;
            info.Surname = BIO.VIPinfo.surname;
            info.Age = BIO.VIPinfo.age;
            info.BloodType = BIO.VIPinfo.bloodType;
            info.AvatarBody = BIO.VIPinfo.avatarBody;
            info.AvatarHead = BIO.VIPinfo.avatarHead;
            info.IsVIP = true;
            info.VIPDescription = BIO.VIPinfo.description;
            info.Likes = BIO.VIPinfo.description;
            info.Dislikes = BIO.VIPinfo.description;
            info.VIPTime = mVIPSystemManager.vipCureTImeSeconds;
            info.Race = int.Parse(vipdata[1], System.Globalization.CultureInfo.InvariantCulture);
            info.Sex = int.Parse(vipdata[0], System.Globalization.CultureInfo.InvariantCulture);
            info.Type = 1;

            HeadFront.sprite = BIO.VIPappearance.headFront;
            HeadBack.sprite = BIO.VIPappearance.headBack;

            TorsoFront.sprite = BIO.VIPappearance.torsoFront;
            TorsoBack.sprite = BIO.VIPappearance.torsoBack;
            ArmsL.sprite = BIO.VIPappearance.Arm;
            HandsLFront.sprite = BIO.VIPappearance.HandFrontL;
            ArmsR.sprite = BIO.VIPappearance.Arm;
            HandsRFront.sprite = BIO.VIPappearance.HandFrontR;

            LowerBodyFront.sprite = BIO.VIPappearance.lowerBodyFront;
            ThighL.sprite = BIO.VIPappearance.UpperLeg;
            CalfL.sprite = BIO.VIPappearance.LowerLeg;
            FootLFront.sprite = BIO.VIPappearance.FootFront;
            ThighR.sprite = BIO.VIPappearance.UpperLeg;
            CalfR.sprite = BIO.VIPappearance.LowerLeg;
            FootRFront.sprite = BIO.VIPappearance.FootFront;

            return Character;
        }
        public List<Sprite> GetPajama(string VIPDefinition)
        {

            var vipdata = VIPDefinition.Split('/');

            VIPDatabase.VIP_BIO BIO = VIPdatabase.VIPgender[int.Parse(vipdata[0], System.Globalization.CultureInfo.InvariantCulture)].VIPrace[int.Parse(vipdata[1], System.Globalization.CultureInfo.InvariantCulture)].VIPbio[VIPdatabase.VIPgender[int.Parse(vipdata[0], System.Globalization.CultureInfo.InvariantCulture)].VIPrace[int.Parse(vipdata[1], System.Globalization.CultureInfo.InvariantCulture)].VIPbio.Length - 1];

            List<Sprite> pajama = new List<Sprite>();

            pajama.Add(BIO.VIPappearance.torsoFront);
            pajama.Add(BIO.VIPappearance.torsoBack);
            pajama.Add(BIO.VIPappearance.Arm);
            pajama.Add(BIO.VIPappearance.HandFrontR);
            pajama.Add(BIO.VIPappearance.Arm);
            pajama.Add(BIO.VIPappearance.HandFrontL);
            pajama.Add(BIO.VIPappearance.lowerBodyFront);
            pajama.Add(BIO.VIPappearance.UpperLeg);
            pajama.Add(BIO.VIPappearance.LowerLeg);
            pajama.Add(BIO.VIPappearance.FootFront);
            pajama.Add(BIO.VIPappearance.UpperLeg);
            pajama.Add(BIO.VIPappearance.LowerLeg);
            pajama.Add(BIO.VIPappearance.FootFront);

            return pajama;
        }
        public List<Sprite> GetClothesOn(string VIPDefinition)
        {
            var vipdata = VIPDefinition.Split('/');

            VIPDatabase.VIP_BIO BIO = VIPdatabase.VIPgender[int.Parse(vipdata[0])].VIPrace[int.Parse(vipdata[1])].VIPbio[int.Parse(vipdata[2])];

            List<Sprite> clothes = new List<Sprite>();

            clothes.Add(BIO.VIPappearance.torsoFront);
            clothes.Add(BIO.VIPappearance.torsoBack);
            clothes.Add(BIO.VIPappearance.Arm);
            clothes.Add(BIO.VIPappearance.HandFrontR);
            clothes.Add(BIO.VIPappearance.Arm);
            clothes.Add(BIO.VIPappearance.HandFrontL);
            clothes.Add(BIO.VIPappearance.lowerBodyFront);
            clothes.Add(BIO.VIPappearance.UpperLeg);
            clothes.Add(BIO.VIPappearance.LowerLeg);
            clothes.Add(BIO.VIPappearance.FootFront);
            clothes.Add(BIO.VIPappearance.UpperLeg);
            clothes.Add(BIO.VIPappearance.LowerLeg);
            clothes.Add(BIO.VIPappearance.FootFront);

            return clothes;
        }
    }
}
