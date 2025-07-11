using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Hospital
{
	public class MedicineLockedTooltip : SimpleUI.Tooltip
	{
		[SerializeField]
		private TextMeshProUGUI unlockedText = null;

		private void Initialize(MedicineRef medicine)
		{
			base.Initialize();
			var rh = ResourcesHolder.Get();
			gameObject.SetActive(true);
			unlockedText.text = I2.Loc.ScriptLocalization.Get("UNLOCK_AT_LEVEL") + " " + rh.GetMedicineInfos(medicine).minimumLevel;
			SetPosition();

		}

		public static MedicineLockedTooltip Instance { get; private set; }
		public static MedicineLockedTooltip Open(MedicineRef medicine)
		{

			if (Instance == null)
				Instantiate();
			Instance.Initialize(medicine);

			return Instance;

		}
		private static void Instantiate()
		{
			Instance = GameObject.Instantiate(ResourcesHolder.Get().MedicineLockedTooltipPrefab).GetComponent<MedicineLockedTooltip>();
			Instance.gameObject.transform.SetParent(UIController.get.canvas.transform);
			Instance.transform.localScale = Vector3.one;
		}
	}
}