using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Hospital
{
	public class MedicineTooltip : SimpleUI.Tooltip
	{
		[SerializeField]
		private List<Image> PrerequisiteImages = null;
		[SerializeField]
		private TextMeshProUGUI medicineName = null;
		[SerializeField]
		private TextMeshProUGUI medicineAmount = null;
        [SerializeField]
        private TextMeshProUGUI textTipSourceRoom = null;
        [SerializeField]
		private List<TextMeshProUGUI> PrerequisiteAmounts = null;
		[SerializeField]
		private TextMeshProUGUI productionTime = null;
		[SerializeField]
		private Color avaiable = Color.black;
		[SerializeField]
		private Color unavaiable = Color.red;
        [SerializeField]
        private Image storageImage = null;
        [SerializeField]
        private Sprite storageSprite = null;
        [SerializeField]
        private Sprite tankSprite = null;

        private bool boosterActive = false;
		[SerializeField] GameObject boosterImg = null;
        

		private void Initialize(MedicineRef medicine, bool showPrerequisites)
		{
            //gameObject.GetComponent<RectTransform>().sizeDelta = defaultSize;
			boosterActive = (HospitalAreasMapController.HospitalMap.boosterManager.boosterActive && ResourcesHolder.Get ().boosterDatabase.boosters [HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].boosterType == BoosterType.Action && ResourcesHolder.Get ().boosterDatabase.boosters [HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].boosterTarget == BoosterTarget.Lab);
           /* if (boosterActive) {
            }*/

            base.Initialize();
			var rh = ResourcesHolder.Get();
			gameObject.SetActive(true);
			medicineName.text = rh.GetNameForCure(medicine);
			productionTime.text = UIController.GetFormattedTime((int) rh.GetMedicineInfos(medicine).ProductionTime);
			var p = GameState.Get().GetCureCount(medicine);
			medicineAmount.text = p.ToString();
			medicineAmount.color = p > 0 ? avaiable : unavaiable;

			boosterImg.SetActive (boosterActive);

  			
            textTipSourceRoom.gameObject.SetActive(false);
            var prereq = ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines[medicine.id].Prerequisities;
			int i = 0;

			if (!boosterActive) {
				if (showPrerequisites) {
					for (i = 0; i < prereq.Count; i++) {
						PrerequisiteImages [i].sprite = prereq [i].medicine.image;
						var am = GameState.Get ().GetCureCount (prereq [i].medicine.GetMedicineRef ());
						PrerequisiteAmounts [i].text = am + "/" + prereq [i].amount;
						PrerequisiteAmounts [i].color = prereq [i].amount > am ? unavaiable : avaiable;
						PrerequisiteImages [i].gameObject.SetActive (true);
						PrerequisiteAmounts [i].gameObject.SetActive (true);
					}
					for (; i < PrerequisiteImages.Count; i++) {
						PrerequisiteImages [i].gameObject.SetActive (false);
						PrerequisiteAmounts [i].gameObject.SetActive (false);
					}
				} else {


					PrerequisiteImages [0].sprite = ResourcesHolder.Get ().GetSpriteForCure (medicine);
					var am = GameState.Get ().GetCureCount (medicine);
					PrerequisiteAmounts [0].text = "x1";
					PrerequisiteAmounts [0].color = 1 > am ? Color.red : Color.white;
					for (i = 1; i < PrerequisiteImages.Count; i++) {
						PrerequisiteImages [i].gameObject.SetActive (false);
						PrerequisiteAmounts [i].gameObject.SetActive (false);
					}
				}
			} else {


				for (i = 0; i < PrerequisiteImages.Count; i++) {
					PrerequisiteImages [i].gameObject.SetActive (false);
					PrerequisiteAmounts [i].gameObject.SetActive (false);
				}

                boosterImg.GetComponent<Image>().sprite = ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].icon;

            }

            if (showPrerequisites && prereq.Count > 2)
                rt.sizeDelta = new Vector2(250, 185);
            else if (boosterActive)
                rt.sizeDelta = new Vector2(250, 185);
            else
                rt.sizeDelta = new Vector2(250, 145);


            SetPosition();
		}

        private void Initialize(MedicineRef medicine)
        {
            rt.sizeDelta = new Vector2(250, 120);

            base.Initialize();
			boosterImg.SetActive (false);
            var rh = ResourcesHolder.Get();
            gameObject.SetActive(true);
            medicineName.text = rh.GetNameForCure(medicine);
            productionTime.text = "";

            if (medicine.type == MedicineType.BaseElixir)
                textTipSourceRoom.text = I2.Loc.ScriptLocalization.Get("MADE_IN_TUBES");
            else
                textTipSourceRoom.text = I2.Loc.ScriptLocalization.Get("MADE_IN_MIXER");

            textTipSourceRoom.gameObject.SetActive(true);
            for (int i = 0; i < PrerequisiteImages.Count; i++)
            {
                PrerequisiteImages[i].gameObject.SetActive(false);
                PrerequisiteAmounts[i].gameObject.SetActive(false);
            }

            var p = GameState.Get().GetCureCount(medicine);
            medicineAmount.text = p.ToString();
			medicineAmount.color = p > 0 ? avaiable : unavaiable;

            SetPosition();
        }

        public static MedicineTooltip Instance { get; private set; }
		public static MedicineTooltip Open(MedicineRef medicine,bool showPrerequisites)
		{
            if (Instance == null)
            {
                Instantiate();
            }
            Instance.SetStorageImage(medicine);
            Instance.Initialize(medicine,showPrerequisites);

			return Instance;

		}
        public static MedicineTooltip Open(MedicineRef medicine)
        {

            if (Instance == null)
            {
                Instantiate();
            }
            Instance.SetStorageImage(medicine);
            Instance.Initialize(medicine);

            return Instance;

        }

        private void SetStorageImage(MedicineRef medicine)
        {
            if (storageImage == null || storageSprite == null || tankSprite == null) return;

            bool isTankStorageItem = ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines[medicine.id].isTankStorageItem;
            storageImage.sprite = (isTankStorageItem) ? tankSprite : storageSprite;
        }

        private static void Instantiate()
		{
			Instance = GameObject.Instantiate(ResourcesHolder.Get().MedicineTooltipPrefab).GetComponent<MedicineTooltip>();
			Instance.gameObject.transform.SetParent(UIController.get.canvas.transform);
			Instance.transform.localScale = Vector3.one;
		}
	}
}