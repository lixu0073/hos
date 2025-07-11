using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleUI;
using UnityEngine.UI;
using TMPro;
using System;

namespace Hospital
{
    public class StorageFullPopUp : UIElement
    {
        [SerializeField] private Image MachineImage = null;
        [SerializeField] public TextMeshProUGUI storageInfo;
        [SerializeField] public TextMeshProUGUI storageHeaderInfo;

        [SerializeField] public List<GameObject> tankLvls;

        ElixirStorage elixirStorage;
        ElixirTank elixirTank;

        private bool isTanktStorage = false;
        public bool preserveHover = false;

        public void UpgradeStorage()
        {
            ButtonExit();
            if (AreaMapController.Map.VisitingMode)
            {
                UIController.getHospital.StorageUpgradePopUp.Open(this.isTanktStorage);
            }
            else
            {
                if (!this.isTanktStorage)
                {
                    UIController.getHospital.StorageUpgradePopUp.Open(() =>
                    {
                        Vector3 spawnPosition = Game.Instance.gameState().ElixirStore.transform.position;
                        var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(this.elixirStorage.position.x + this.elixirStorage.actualData.rotationPoint.x, 0, this.elixirStorage.position.y + this.elixirStorage.actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
                        fp.transform.localScale = Vector3.one * 1.4f;
                        fp.SetActive(true);
                        base.Exit();
                    }, false);
                }
                else
                {
                    UIController.getHospital.StorageUpgradePopUp.Open(() =>
                    {
                        var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(this.elixirTank.position.x + this.elixirTank.actualData.rotationPoint.x, 0, this.elixirTank.position.y + this.elixirTank.actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
                        fp.transform.localScale = Vector3.one * 1.4f;
                        fp.SetActive(true);
                        base.Exit();
                    }, true);
                }
            }
        }

        public IEnumerator Open(bool isTanktStorage, Action whenDone = null)
        {
            if (elixirStorage == null)
                elixirStorage = Game.Instance.gameState().ElixirStore;

            if (elixirTank == null)
                elixirTank = Game.Instance.gameState().ElixirTank;

            this.isTanktStorage = isTanktStorage;

            SetMachineImage();

            yield return null; // CV: to force 1-frame delay before rendering the TMPro

            if (this.isTanktStorage)
            {
                storageInfo.text = I2.Loc.ScriptLocalization.Get("TANK_FULL_INFO");
                storageHeaderInfo.text = I2.Loc.ScriptLocalization.Get("TANK_FULL_TITLE");
            }
            else
            {
                storageInfo.text = I2.Loc.ScriptLocalization.Get("STORAGE_FULL_INFO");
                storageHeaderInfo.text = I2.Loc.ScriptLocalization.Get("STORAGE_FULL_TITLE");
            }

            yield return base.Open(true, preserveHover);

            whenDone?.Invoke();
        }

        public void ButtonExit()
        {
            preserveHover = false;
            Exit();
        }

        public void SetMachineImage()
        {
            MachineImage.gameObject.SetActive(false);
            for (int i = 0; i < tankLvls.Count; i++)
                tankLvls[i].gameObject.SetActive(false);

            if (this.isTanktStorage)
            {
                int visualLvl = elixirTank.GetVisualLevel();
                if (visualLvl >= 0 && visualLvl < 5)
                    tankLvls[visualLvl].gameObject.SetActive(true);
                else
                    tankLvls[4].gameObject.SetActive(true);
            }
            else
            {
                MachineImage.gameObject.SetActive(true);
                MachineImage.sprite = elixirStorage.GetCurrentIndicator();
            }
        }
    }
}
