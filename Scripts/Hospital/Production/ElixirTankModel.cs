using IsoEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hospital
{
    public class ElixirTankModel : ElixirTank
    {

        private const string OBJECT_TAG = "EliTank";
        private const char MAIN_SEPARATOR = ';';
        private const char SEPARATOR = '/';
        private const string NULL_TAG = "null";

        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true) { }

        protected override void Init()
        {
            if (!AreaMapController.Map.VisitingMode)
            {
                Game.Instance.gameState().ElixirTank = this;
            }
        }

        public override void SetActualAmount(int amount)
        {
            actualAmount = amount;
        }

        protected override void OnClickWorking() { }

        public override void IsoUpdate() { }

        public override void SetLevel(int lvl, int MaximumLvl = -1)
        {
            actualLvl = lvl;
        }

        [Obsolete]
        public void Save(Save save)
        {
            for (int i = 0; i < save.LaboratoryObjectsData.Count; ++i)
            {
                var laboratoryObjectArray = save.LaboratoryObjectsData[i].Split('$');
                if (laboratoryObjectArray[0] == OBJECT_TAG)
                {
                    save.LaboratoryObjectsData[i] = SaveObject();
                    return;
                }
            }
        }

        public static bool IsElixirTank(string model)
        {
            return model.StartsWith(OBJECT_TAG);
        }

        protected override string SaveToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(position);
            builder.Append(SEPARATOR);
            builder.Append(Checkers.CheckedActualRotation(actualRotation, Tag));
            builder.Append(SEPARATOR);
            builder.Append(Checkers.CheckedState(state, Tag));
            builder.Append(SEPARATOR);
            builder.Append(NULL_TAG);
            builder.Append(MAIN_SEPARATOR);
            builder.Append(Checkers.CheckedAmount(actualLvl, 1, GetMaxLevel(), Tag + " level: "));
            builder.Append(SEPARATOR);
            builder.Append(Checkers.CheckedAmount(actualAmount, 0, int.MaxValue, Tag + " currentAmount: "));
            return builder.ToString();
        }

        public void InitFromSave(Save save, TimePassedObject timePassed)
        {
            foreach (string laboratoryObjectUnparsed in save.LaboratoryObjectsData)
            {
                var laboratoryObjectArray = laboratoryObjectUnparsed.Split('$');
                if (laboratoryObjectArray[0] == OBJECT_TAG)
                {
                    var info = AreaMapController.Map.GetPrefabInfo(OBJECT_TAG);
                    if (info != null)
                    {
                        InitializeFromSave(laboratoryObjectArray[1], info, timePassed);
                        Init();
                    }
                    else
                    {
                        Debug.LogError("CHECK TO FIX");
                    }
                    return;
                }
            }
        }

        protected override void InitializeFromSave(string save, Rotations info, TimePassedObject timePassed)
        {
            var strs = save.Split(MAIN_SEPARATOR);
            var settings = strs[0].Split(SEPARATOR);
            var pos = Vector2i.Parse(settings[0]);
            Rotation rotation = (Rotation)Enum.Parse(typeof(Rotation), settings[1]);
            var stt = (State)Enum.Parse(typeof(State), settings[2]);
            Tag = info.infos.Tag;
            this.info = info;
            position = pos;
            actualRotation = rotation;
            availableInVisitingMode = info.infos.availableInVisitingMode;
            state = stt;
            LoadFromString(save, timePassed);
        }

        protected override void LoadFromString(string save, TimePassedObject timePassed, int ActionsDone = 0)
        {
            var str = save.Split(MAIN_SEPARATOR);
            if (str.Length > 1)
            {
                var p = str[1].Split(SEPARATOR);
                int lvl = int.Parse(p[0], System.Globalization.CultureInfo.InvariantCulture);
                int amount = int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture);
                SetLevel(lvl);
                actualAmount = amount;
            }
            else
            {
                actualLvl = 1;
                actualAmount = 0;
            }

            //Debug.LogError("Sprawdzić to -> case upgrade'u tanka'a");
            /*
            if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    if (GameState.Get().ElixirTankToUpgrade > 0)
                    {
                        Upgrade(GameState.Get().ElixirTankToUpgrade);
                        GameState.Get().ElixirTankToUpgrade = 0;
                    }
                }
            */

        }

    }
}
