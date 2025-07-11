using UnityEngine;
using System.Collections;
using IsoEngine;
using System;
using SimpleUI;
namespace Hospital
{

    //	public class FloraProductionMachine : RotatableWithoutBuilding, ICollectable
    //	{
    //		#region Privates

    //		FloraProductionState floraState = FloraProductionState.nothing;
    //		float timeLeft;
    //		float productionTime;
    //		MedicineRef medicine;
    //		FloraProductionMachineInfo infos;

    //		#endregion

    //		#region Initialization

    //		public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State state = State.fresh, bool shouldDisappear = true)
    //		{
    //			base.Initialize(info, position, rotation,state,shouldDisappear);
    //			infos = (FloraProductionMachineInfo)info.infos;
    //			medicine = infos.producedFlora;
    //			productionTime = ResourcesHolder.Get().GetMedicineInfos(medicine).productionTime;
    //			timeLeft = -1;
    //		}
    //		protected override void LoadFromString(string str)
    //		{
    //			base.LoadFromString(str);
    //			var strs = str.Split('/');
    //			infos = (FloraProductionMachineInfo)info.infos;
    //			medicine = infos.producedFlora;
    //			productionTime = ResourcesHolder.Get().GetMedicineInfos(medicine).productionTime;
    //			timeLeft = float.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture);
    //			floraState = (FloraProductionState)Enum.Parse(typeof(FloraProductionState), strs[0]);
    //			ProbeTableTool.collectable.Add(this);

    //		}
    //		protected override string SaveToString()
    //		{
    //			return base.SaveToString()+";"+floraState+"/"+timeLeft;
    //		}
    //		public override void IsoDestroy()
    //		{
    //			ProbeTableTool.collectable.Remove(this);
    //			base.IsoDestroy();
    //		}

    //		public override void SetAnchored(bool value)
    //		{
    //			base.SetAnchored(value);
    //			if (floraState == FloraProductionState.nothing && value)
    //			{
    //				StartProduction();
    //				ProbeTableTool.collectable.Add(this);
    //			}
    //		}

    //		#endregion

    //		#region Interaction

    //		private void ShowWorkingHover()
    //		{

    //		}

    //		private void ShowCollectingHover()
    //		{

    //		}

    //		protected override void OnClickWorking()
    //		{
    //			base.OnClickWorking();
    //			if (floraState == FloraProductionState.working)
    //			{
    //				ShowWorkingHover();
    //				print("working!" + timeLeft);
    //			}
    //			if(floraState==FloraProductionState.waiting)
    //			{
    //				ShowCollectingHover();
    //			}
    //		}

    //#endregion

    //		public void StartProduction()
    //		{
    //			print("starting production of flora");
    //			floraState = FloraProductionState.working;
    //			timeLeft = productionTime;

    //		}

    //		#region Collecting Tool Interaction

    //		public bool Collect(Vector2i position)
    //		{
    //			//print("collecting");
    //			if (this.position==position && floraState == FloraProductionState.waiting)
    //			{
    //				if(!GameState.Get().CanAddAmount(1))
    //				{
    //					MessageController.instance.ShowMessage("Storage is full!");
    //					StorageFullPopUp.Open();
    //					return;
    //				}
    //				var from = Utils.ScreenToCanvasPosition(ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(new Vector3(position.x, 0, position.y)));
    //				var canvas = UIController.get.canvas;
    //				ElixirStorageCounter.Add();
    //				GameState.Get().AddResource(medicine, 1);
    //                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Medicine, transform.position, 1, .5f, ResourcesHolder.Get().GetSpriteForCure(medicine), null, () => {
    //                    ElixirStorageCounter.Remove();
    //                });
    //                int expRecieved = ResourcesHolder.Get().GetEXPForCure(medicine);
    //                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, transform.position, expRecieved, 0f, null, null, () => {
    //                    GameState.Get().AddResource(ResourceType.Exp, expRecieved);
    //                });
    //                StartProduction();
    //			}
    //		}

    //		#endregion

    //		public override void Update()
    //		{
    //			base.Update();
    //			//odliczanie
    //			if (floraState == FloraProductionState.working)
    //			{
    //				timeLeft -= Time.deltaTime;
    //				SetIndicator();
    //				if (timeLeft < 0)
    //				{
    //					floraState = FloraProductionState.waiting;
    //					timeLeft = -1;
    //				}
    //			}
    //		}

    //		#region Sprite setting

    //		protected void SetIndicator()
    //		{
    //			SetIndicator(1 - (timeLeft / productionTime));
    //		}

    //		protected void SetIndicator(float percent)
    //		{
    //			var p = infos.GetSprite(percent);
    //			if (p != null && isoObj != null)
    //				isoObj.GetGameObject().transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>().sprite = p;
    //		}

    //		#endregion
    //	}
    //	internal enum FloraProductionState
    //	{
    //		nothing,
    //		working,
    //		waiting,

    //	}
}