using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleUI;
using TMPro;

namespace Hospital
{
	public class PanaceaStoragePopUp : UIElement
	{
        /*
		#region localizationKeys
		private string TitleKey = "TitlePanaceaStorage";
		private string InfoKey = "PanaceaStorageLvl";
		private string UpgradeKey = "Upgrade";
		#endregion

		#region localizationText
		public TextMeshProUGUI TitleText;
		public TextMeshProUGUI InfoText;
		public TextMeshProUGUI UpgradeText;
		#endregion

		//[SerializeField] private TextMeshProUGUI lvlText = null;
		[SerializeField] private TextMeshProUGUI amountText = null;
		PanaceaStorage currentStorage;
		int localAmount;


		public void Exit()
		{
            base.Exit();
			HospitalAreasMapController.Map.ResetOntouchAction();
        }

		public void Open(int amount, PanaceaStorage storage)
		{
            base.Open();
            currentStorage = storage;
			Refresh(amount);
        }

		private void Refresh(int amount)
		{
			TitleText.text = LanguageManager.Instance.GetTextValue (TitleKey);
			InfoText.text = LanguageManager.Instance.GetTextValue (InfoKey) + " " + currentStorage.actualLevel.ToString ();
			UpgradeText.text = LanguageManager.Instance.GetTextValue (UpgradeKey);
			amountText.text = amount.ToString() + "/" + currentStorage.maximumAmount.ToString();
			localAmount = amount;
		}

		public void Upgrade()
		{
            SoundsController.Instance.PlayObjectUpgrade();
            currentStorage.Upgrade(1);
			InfoText.text = LanguageManager.Instance.GetTextValue (InfoKey) + " " + currentStorage.actualLevel.ToString ();
			amountText.text = localAmount.ToString() + "/" + currentStorage.maximumAmount.ToString();
		}

    */
	}
}

