using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using IsoEngine;
using MovementEffects;

namespace SimpleUI
{
    public delegate void OnEvent();
    public delegate void OnEventWithParam(bool param);

    public enum GiftType
    {  //coin, diamond, exp should be in the same order as in ResourceType
        Coin,
        Diamond,
        Exp,
        Panacea,
        Medicine,   //everything that goes to medicine storage (elixirs, plants, cures)
        Special,    //items for upgrading storage and shovels
        Drawer,     //items that fly to the drawer (decorations, etc)
        Booster,
        Case,
        PositiveEnergy,
        GlobalEventPersonal,
        BloodTest,
        Reputation
    }

    public class GiftSystem : MonoBehaviour
    {

        public GameObject canvas;
        public List<Sprite> particleSprites;
        public List<IGiftable> giftables;       //0 - coin icon; 1 - diamond icon; 2 - Exp star icon; 3 - DrawerButton; 
                                                //4 - StorageIcon (from ElixirStorageCounter) 5 - boosters button
                                                //6 - Kids Room 7 - Return from visiting button, 8 - Global Events Personal goal progress bar icon
        public GameObject collectable;
        public GameObject giftPrefab;
        public GameObject itemUsedPrefab;
        public PickUpParticle[] pickUpParticles;

        public List<Vector3> uiElementsPos = new List<Vector3>();


        public void SetUIElementsPos()
        {
            if (uiElementsPos.Count == 0)
            {
                uiElementsPos.Add(GetGiftablePosition(0));
                uiElementsPos.Add(GetGiftablePosition(1));
            }
        }
        public void CreateGiftParticle(GiftType type, Vector3 worldPos, Vector2 to, int amount, float delay, float duration, Vector3 startScale, Vector3 endScale, Sprite sprite = null, OnEvent start = null, OnEvent end = null, bool setAsLast = true, string bloodTestPatientID = null)
        {
            if (amount == 0)
                return;

            GameObject go;
            go = Instantiate(giftPrefab, new Vector3(-2000, -2000, 0), Quaternion.identity) as GameObject;
            go.transform.SetParent(canvas.transform, false);
            if (setAsLast)
            {
                go.transform.SetAsLastSibling();
            }
            else
            {
                go.transform.SetAsFirstSibling();
            }

            Vector3 worldTo = Vector3.zero;
            go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;

            if (type == GiftType.PositiveEnergy || type == GiftType.BloodTest)
                go.GetComponent<GiftMoving>().StartAnimation(type, worldPos, worldTo, duration, delay, startScale, endScale, "+" + amount, start, end);
            else
                go.GetComponent<GiftMoving>().StartAnimation(type, worldPos, to, duration, delay, startScale, endScale, "+" + amount, start, end);
        }

        public void CreateGiftParticle(GiftType type, Vector3 worldPos, int amount, float delay, float duration, Vector3 startScale, Vector3 endScale, Sprite sprite = null, OnEvent start = null, OnEvent end = null, bool setAsLast = true, string bloodTestPatientID = null)
        {
            if (amount == 0)
                return;

            GameObject go;
            go = Instantiate(giftPrefab, new Vector3(-2000, -2000, 0), Quaternion.identity) as GameObject;
            go.transform.SetParent(canvas.transform, false);
            if (setAsLast)
            {
                go.transform.SetAsLastSibling();
            }
            else
            {
                go.transform.SetAsFirstSibling();
            }


            Vector2 to = Vector2.zero;
            Vector3 worldTo = Vector3.zero;
            switch (type)
            {
                case GiftType.Coin:

                    if (uiElementsPos.Count == 2)
                        to = uiElementsPos[0];
                    else to = GetGiftablePosition(0);

                    if (sprite == null)
                        go.GetComponent<GiftMoving>().giftIcon.sprite = particleSprites[0];
                    else //custom currency sprite from giftbox/vip/iap
                        go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Diamond:

                    if (uiElementsPos.Count == 2)
                        to = uiElementsPos[1];
                    else to = GetGiftablePosition(1);

                    if (sprite == null)
                        go.GetComponent<GiftMoving>().giftIcon.sprite = particleSprites[1];
                    else //custom currency sprite from giftbox/vip/iap
                        go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Exp:
                    to = GetGiftablePosition(2);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = particleSprites[2];
                    break;
                case GiftType.Medicine:
                    to = GetGiftablePosition(4);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Special:
                    to = GetGiftablePosition(4);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Drawer:
                    to = GetGiftablePosition(3);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Panacea:
                    to = GetGiftablePosition(4);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Booster:
                    to = GetGiftablePosition(5);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.PositiveEnergy:
                    worldTo = GetGiftablePosition(6, true);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.BloodTest:
                    worldTo = MaternityWaitingRoomController.Instance.GetMaternityWaitingRoomForPatientID(bloodTestPatientID).GetBloodResultTable().position;
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                default:
                    break;
            }

            if (type == GiftType.PositiveEnergy || type == GiftType.BloodTest)
                go.GetComponent<GiftMoving>().StartAnimation(type, worldPos, worldTo, duration, delay, startScale, endScale, "+" + amount, start, end);
            else
                go.GetComponent<GiftMoving>().StartAnimation(type, worldPos, to, duration, delay, startScale, endScale, "+" + amount, start, end);
        }

        public void CreateGiftParticleUI(GiftType type, Vector2 from, Vector2 to, int amount, float delay, float duration, Vector3 startScale, Vector3 endScale, Sprite sprite = null, OnEvent start = null, OnEvent end = null)
        {
            GameObject go;
            go = Instantiate(giftPrefab, new Vector3(-2000, -2000, 0), Quaternion.identity) as GameObject;
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
            go.GetComponent<GiftMoving>().StartItemMoveAnimation(type, from, to, duration, delay, startScale, endScale, "+" + amount, start, end);
        }

        public void CreateGiftParticleUI(GiftType type, Vector2 from, int amount, float delay, float duration, Vector3 startScale, Vector3 endScale, Sprite sprite = null, OnEvent start = null, OnEvent end = null)
        {
            GameObject go;
            go = Instantiate(giftPrefab, new Vector3(-2000, -2000, 0), Quaternion.identity) as GameObject;
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            Vector2 to = Vector2.zero;
            switch (type)
            {
                case GiftType.Reputation:
                    Transform transform = FindObjectOfType<ReputationSystem.ReputationCounter>().transform;
                    to = transform.GetChild(3).
                        GetComponent<RectTransform>().anchoredPosition + transform.GetComponent<RectTransform>().anchoredPosition;
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Coin:

                    if (uiElementsPos.Count == 2)
                        to = uiElementsPos[0];
                    else to = GetGiftablePosition(0);

                    if (sprite == null)
                        go.GetComponent<GiftMoving>().giftIcon.sprite = particleSprites[0];
                    else //custom currency sprite from giftbox/vip/iap
                        go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Diamond:

                    if (uiElementsPos.Count == 2)
                        to = uiElementsPos[1];
                    else to = GetGiftablePosition(1);

                    if (sprite == null)
                        go.GetComponent<GiftMoving>().giftIcon.sprite = particleSprites[1];
                    else //custom currency sprite from giftbox/vip/iap
                        go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Exp:
                    to = GetGiftablePosition(2);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = particleSprites[2];
                    break;
                case GiftType.Medicine:
                    to = GetGiftablePosition(4);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Special:
                    to = GetGiftablePosition(4);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Drawer:
                    to = GetGiftablePosition(3);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Panacea:
                    to = GetGiftablePosition(4);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.Booster:
                    to = GetGiftablePosition(5);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.PositiveEnergy:
                    to = GetGiftablePosition(6);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                case GiftType.GlobalEventPersonal:
                    to = GetGiftablePosition(8);
                    go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;
                    break;
                default:
                    break;
            }

            go.GetComponent<GiftMoving>().StartItemMoveAnimation(type, from, to, duration, delay, startScale, endScale, "+" + amount, start, end);
        }

        public void CreateItemMoveParticle(GiftType type, Vector2 from, Vector2 to, int amount, float delay, Vector3 startScale, Vector3 endScale, Sprite sprite = null, OnEvent start = null, OnEvent end = null)
        {
            GameObject go;
            go = Instantiate(giftPrefab, new Vector3(-200, -200, 0), Quaternion.identity) as GameObject;
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();
            //go.transform.localScale = scale;
            //go.GetComponent<GiftMoving> ().giftIcon.transform.localScale = startScale;
            go.gameObject.SetActive(true);
            go.GetComponent<GiftMoving>().giftIcon.sprite = sprite;

            if (amount > 0)
                go.GetComponent<GiftMoving>().StartItemMoveAnimation(type, from, to, 1.75f, delay, startScale, endScale, "+" + amount, start, end);
            else
                go.GetComponent<GiftMoving>().StartItemMoveAnimation(type, from, to, 1.75f, delay, startScale, endScale, "", start, end);
        }


        //creates an icon with -X text which flies and fades to the action location showing which resource was spent to complete this action
        //pos is position of action, item will fall to this location
        public void CreateItemUsed(Vector3 pos, int amount, float delay, Sprite sprite = null, bool isWorldSpace = true)
        {
            //Debug.Log("CreateItemUsed pos: " + pos);
            if (amount <= 0)
            {
                Debug.LogWarning("CreateItemUsed amount is <= 0. Returning, we dont want to show -0 something");
                return;
            }

            GameObject go;
            go = Instantiate(itemUsedPrefab);
            go.gameObject.SetActive(true);
            go.GetComponent<Image>().sprite = sprite;

            if (!isWorldSpace)
            {
                go.transform.SetParent(UIController.get.canvas.transform);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
            }
            if (amount == int.MaxValue)
            {
                go.GetComponent<ItemUsed>().StartAnimation(pos, 1.25f, delay, "", isWorldSpace);
            }
            else
            {
                go.GetComponent<ItemUsed>().StartAnimation(pos, 1.25f, delay, "-" + amount, isWorldSpace);
            }
        }

        public Collectable CreateCollectable(Sprite image, MedicineRef medicine, int amount, bool isPositiveEnergy, string patientID)
        {
            GameObject go = Instantiate(collectable);
            go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, 0);
            go.GetComponent<Collectable>().Initialize(image, medicine, amount, isPositiveEnergy, patientID);
            go.SetActive(true);
            return go.GetComponent<Collectable>();
        }

        Vector3 GetGiftablePosition(int id, bool isWorld = false)
        {
            Vector3 worldPos;
            if (Hospital.AreaMapController.Map.VisitingMode && (id == 0 || id == 1 || id == 2))
            {
                worldPos = giftables[7].GetPosition();
            }
            else if (id == 0 || id == 1)
            {
                worldPos = giftables[id].GetTransformPosition();
            }
            else
            {
                worldPos = giftables[id].GetPosition();
            }

            if (isWorld)
            {
                //Debug.LogError("giftablepos = " + worldPos);
                //Debug.LogError("giftable go name = " + giftables[id].gameObject.name);
                //worldPos somehow cuts out z value
                return giftables[id].transform.position;
            }
            else
            {
                return new Vector2((worldPos.x - Screen.width / 2) / canvas.transform.localScale.x, (worldPos.y - Screen.height / 2) / canvas.transform.localScale.y);
            }
        }

        int pickupIndex = 0;
        public void SpawnPickUpParticle(Vector3 pos, bool newCurePatient = false)
        {
            //Debug.LogError("SpawnPickupParticle");
            pickupIndex++;
            if (pickupIndex + 1 > pickUpParticles.Length)
                pickupIndex = 0;

            pickUpParticles[pickupIndex].transform.position = pos;
            pickUpParticles[pickupIndex].FireParticle(newCurePatient);
        }
    }
}