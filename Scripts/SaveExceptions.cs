using UnityEngine;
using System;
using System.Collections.Generic;
using Hospital;
using IsoEngine;
using MovementEffects;
using System.Globalization;

/// <summary>
/// 保存错误异常类，当保存游戏数据时发生错误时抛出。
/// </summary>
public class SaveErrorException : Exception
{
    /// <summary>
    /// 构造函数，创建一个SaveErrorException实例。
    /// </summary>
    /// <param name="message">错误消息。</param>
    internal SaveErrorException(string message) : base(message)
    {
        Debug.LogError(message);                
        BaseUIController.ShowCriticalProblemPopup(toLaunchCoroutine: UIController.get, message);

        //Timing.KillCoroutine (ReloadDelayed().GetType());
        //Timing.RunCoroutine (ReloadDelayed ());
    }

    /// <summary>
    /// 延迟重新加载游戏的协程。
    /// </summary>
    IEnumerator<float> ReloadDelayed()
    {
        yield return Timing.WaitForSeconds(1);
        UIController.get.LoadingPopupController.Open(0, 100, 6);
        SaveSynchronizer.Instance.LoadGame();
    }
}

/// <summary>
/// 保存错误测试通过异常类，用于测试目的。
/// </summary>
public class SaveErrorTestPositive : SystemException
{
    /// <summary>
    /// 构造函数，创建一个SaveErrorTestPositive实例。
    /// </summary>
    /// <param name="message">错误消息。</param>
    internal SaveErrorTestPositive(string message) : base(message)
    {
        Debug.LogError(message);
        //do save error actions
    }
}

/// <summary>
/// 无法保存异常类，当游戏无法保存时抛出。
/// </summary>
public class SaveImpossibleException : System.Exception
{
    /// <summary>
    /// 构造函数，创建一个SaveImpossibleException实例。
    /// </summary>
    /// <param name="message">错误消息。</param>
    internal SaveImpossibleException(string message) : base(message)
    {
        Debug.LogWarning(message);
        //	UIController.get.testView.SetSaveStatus (false);
    }
}

/// <summary>
/// 检查器类，包含各种数据验证方法，用于在保存游戏数据前进行检查。
/// </summary>
public class Checkers
{
    #region basic checkers 
    /// <summary>
    /// 检查整数值是否在指定范围内。
    /// </summary>
    /// <param name="value">要检查的整数值。</param>
    /// <param name="min">最小值。</param>
    /// <param name="max">最大值。</param>
    /// <param name="message">错误消息。</param>
    /// <returns>如果有效，则返回原始值。</returns>
    public static int CheckedAmount(int value, int min, int max, string message)
    {
        // CV: value is an int, so Value type that means that is never a reference but a value itself -> Can't never be null
        //if (value == null)
        //    throw new SaveErrorException("Checked value is null");

        if (value < min || value > max)
            throw new SaveErrorException(message + " " + value + " - is invalid");

        return value;
    }

    /// <summary>
    /// 检查长整数值是否在指定范围内。
    /// </summary>
    /// <param name="value">要检查的长整数值。</param>
    /// <param name="min">最小值。</param>
    /// <param name="max">最大值。</param>
    /// <param name="message">错误消息。</param>
    /// <returns>如果有效，则返回原始值。</returns>
    public static long CheckedAmount(long value, long min, long max, string message)
    {
        // CV: value is a long, so Value type that means that is never a reference but a value itself -> Can't never be null
        //if (value == null)
        //    throw new SaveErrorException("Checked value is null");

        if (value < min || value > max)
            throw new SaveErrorException(message + " " + value + " - is invalid");

        return value;
    }

    /// <summary>
    /// 检查字符串是否为空或null。
    /// </summary>
    /// <param name="value">要检查的字符串。</param>
    /// <param name="message">错误消息。</param>
    /// <returns>如果有效，则返回原始字符串。</returns>
    public static string CheckedNullOrEmpty(string value, string message)
    {
        if (string.IsNullOrEmpty(value))
            throw new SaveErrorException(message + " value is null");

        return value;
    }

    /// <summary>
    /// 检查浮点数值是否在指定范围内。
    /// </summary>
    /// <param name="value">要检查的浮点数值。</param>
    /// <param name="min">最小值。</param>
    /// <param name="max">最大值。</param>
    /// <param name="message">错误消息。</param>
    /// <returns>如果有效，则返回原始值。</returns>
    public static float CheckedAmount(float value, float min, float max, string message)
    {
        // CV: value is a float, so Value type that means that is never a reference but a value itself -> Can't never be null
        //if (value == null)
        //    throw new SaveErrorException("Checked value is null");

        if (value < min || value > max)
            throw new SaveErrorException(message + " " + value + " - is invalid");

        return value;
    }

    /// <summary>
    /// 检查教程步骤标签是否有效。
    /// </summary>
    /// <param name="tutorialTag">要检查的教程标签。</param>
    /// <param name="isMaternity">是否为产科教程。</param>
    /// <returns>如果有效，则返回原始标签。</returns>
    public static StepTag CheckedTutorialStepTag(StepTag tutorialTag, bool isMaternity = false)
    {
        // CV: StepTag is an enum, so Value type that means that is never a reference but a value itself -> Can't never be null
        //if (tutorialTag == null)        
        //    throw new SaveErrorException("Checked tutorial tag is null");

        if (isMaternity && tutorialTag != TutorialController.Instance.CurrentTutorialStepTag)
            throw new SaveErrorException("Checked tutorial tag is invalid: " + tutorialTag + ", current: " + TutorialController.Instance.CurrentTutorialStepTag);

        if (!isMaternity && tutorialTag != TutorialSystem.TutorialController.CurrentStep.StepTag)
            throw new SaveErrorException("Checked tutorial tag is invalid: " + tutorialTag + ", current: " + TutorialController.Instance.CurrentTutorialStepTag);

        if (tutorialTag == StepTag.None)
            throw new SaveErrorException("Checked tutorial tag is invalid");

        return tutorialTag;
    }

    /// <summary>
    /// 检查布尔值。
    /// </summary>
    /// <param name="checkedValue">要检查的布尔值。</param>
    /// <returns>原始布尔值。</returns>
    public static bool CheckedBool(bool checkedValue)
    {
        //false and true are possible
        return checkedValue;
    }

    /// <summary>
    /// 检查枚举值是否有效。
    /// </summary>
    /// <param name="enumValue">要检查的枚举值。</param>
    /// <returns>如果有效，则返回原始枚举值。</returns>
    public static Enum CheckedEnumValue(Enum enumValue)
    {
        Type typ = enumValue.GetType();
        var lastEnumVal = Enum.GetValues(typ).Length - 1;

        if ((int)((object)enumValue) > (int)lastEnumVal)
            throw new SaveErrorException("Enum type " + typ + " is invalid");

        return enumValue;
    }

    /// <summary>
    /// 检查经验值是否有效。
    /// </summary>
    /// <param name="experienceAmount">经验值。</param>
    /// <param name="actualLevel">当前等级。</param>
    /// <returns>如果有效，则返回原始经验值。</returns>
    public static int CheckedExperience(int experienceAmount, int actualLevel)
    {
        if (actualLevel > 1)
        {
            if (experienceAmount < 0)
                throw new SaveErrorException("Experience amount of " + experienceAmount + " - is invalid for level " + actualLevel);
        }

        return experienceAmount;
    }

    /// <summary>
    /// 检查医院名称是否有效。
    /// </summary>
    /// <param name="hospitalName">医院名称。</param>
    /// <returns>如果有效，则返回原始医院名称。</returns>
    public static string CheckedHospitalName(string hospitalName)
    {
        if (hospitalName == null)        
            return "My Hospital";

        if (hospitalName.Length < 1 || hospitalName.Length > UIController.getHospital.hospitalNameTab.GetNameCharLimit())        
            throw new SaveErrorException("Name: " + hospitalName + " - is invalid");

        return hospitalName;
    }

    /// <summary>
    /// 检查服务器时间是否有效。
    /// </summary>
    /// <returns>如果有效，则返回服务器时间。</returns>
    public static DateTime CheckedTime()
    {
        DateTime time = ServerTime.Get().GetServerTime();
        if (!ServerTime.Get().isTimeDownloaded || time.Year == 1970)
            throw new SaveErrorException("can't check server time");

        return time;
    }
    #endregion

    #region RotatableObject checkers
    /// <summary>
    /// 检查位置是否有效。
    /// </summary>
    /// <param name="position">要检查的位置。</param>
    /// <param name="objectName">对象名称。</param>
    /// <returns>如果有效，则返回原始位置。</returns>
    public static Vector2i CheckedPosition(Vector2i position, string objectName)
    {
        if (position == null)
            throw new SaveErrorException("Checked position is null");

        //if (position.x < 0 || position.x > 10 || position.y < 0 || position.y > 10) {
        if (position.x < 0 || position.x > AreaMapController.Map.mapConfig.MapSize.x || position.y < 0 || position.y > AreaMapController.Map.mapConfig.MapSize.y)
            throw new SaveErrorException(objectName + position + " - position is invalid");

        return position;
    }

    /// <summary>
    /// 检查实际旋转是否有效。
    /// </summary>
    /// <param name="actualRotation">要检查的旋转。</param>
    /// <param name="objectName">对象名称。</param>
    /// <returns>如果有效，则返回原始旋转。</returns>
    public static Rotation CheckedActualRotation(Rotation actualRotation, string objectName)
    {
        // CV: Hospital.Rotation is an enum, so Value type that means that is never a reference but a value itself -> Can't never be null
        //if (actualRotation == null)        
        //    throw new SaveErrorException("Checked actualRotation is null");

        if (actualRotation != Rotation.North && actualRotation != Rotation.South && actualRotation != Rotation.East && actualRotation != Rotation.West)        
            throw new SaveErrorException(objectName + " " + actualRotation + " - rotation is invalid");

        return actualRotation;
    }

    /// <summary>
    /// 检查可旋转对象的当前状态是否有效。
    /// </summary>
    /// <param name="state">要检查的状态。</param>
    /// <param name="objectName">对象名称。</param>
    /// <returns>如果有效，则返回原始状态。</returns>
    public static RotatableObject.State CheckedState(RotatableObject.State state, string objectName)
    {
        // CV: RotatableObjext.State is an enum, so Value type that means that is never a reference but a value itself -> Can't never be null
        //if (state == null)
        //    throw new SaveErrorException("Checked state is null");

        if (state != RotatableObject.State.building && state != RotatableObject.State.fresh && state != RotatableObject.State.waitingForUser && state != RotatableObject.State.working)
            throw new SaveErrorException(objectName + " " + state + " - state is invalid");

        return state;
    }

    /// <summary>
    /// 检查药品引用是否有效。
    /// </summary>
    /// <param name="medicine">要检查的药品引用。</param>
    /// <param name="objectName">对象名称。</param>
    /// <returns>如果有效，则返回原始药品引用。</returns>
    public static MedicineRef CheckedMedicine(MedicineRef medicine, string objectName)
    {
        if (medicine == null)        
            throw new SaveErrorException("Checked medicine is null");

        if (medicine.type != MedicineType.AdvancedElixir && medicine.type != MedicineType.Balm && medicine.type != MedicineType.BaseElixir && medicine.type != MedicineType.BasePlant && medicine.type != MedicineType.Capsule && medicine.type != MedicineType.Drips && medicine.type != MedicineType.Extract && medicine.type != MedicineType.EyeDrops && medicine.type != MedicineType.FizzyTab && medicine.type != MedicineType.InhaleMist && medicine.type != MedicineType.Jelly && medicine.type != MedicineType.NoseDrops && medicine.type != MedicineType.Pill && medicine.type != MedicineType.Shot && medicine.type != MedicineType.Special && medicine.type != MedicineType.Syrop && medicine.type != MedicineType.Bacteria && medicine.type != MedicineType.Vitamins)
            throw new SaveErrorException(objectName + "collectable " + medicine.type + " - medicine type is invalid");

        if (medicine.id < 0 || medicine.id >= ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines.Count)
            throw new SaveErrorException(objectName + "collectable " + medicine.type + " ID: " + medicine.id + " - medicine ID is invalid");

        return medicine;
    }
    #endregion

    #region patients
    /// <summary>
    /// 检查诊所AI状态字符串是否有效。
    /// </summary>
    /// <param name="AIState">要检查的AI状态字符串。</param>
    /// <returns>如果有效，则返回原始AI状态字符串。</returns>
    public static string CheckedClinicAIState(string AIState)
    {
        if (string.IsNullOrEmpty(AIState))        
            throw new SaveErrorException("Checked AIState is null or empty");

        var state = AIState.Split('!');

        if (state[0] != "GTR" && state[0] != "RCIS" && state[0] != "GTDR" && state[0] != "SOWS" && state[0] != "WIRS" && state[0] != "WFCS" && state[0] != "WAS" && state[0] != "WARS" && state[0] != "HS" && state[0] != "WIDRS" && state[0] != "GHS" && state[0] != "GNFH")
            throw new SaveErrorException(AIState + " - is invalid");

        if (state[0] == "GTR")
        {
            if (state[1] != "Default" && state[1] != "WaitnQueue" && state[1] != "CheckInReception")
                throw new SaveErrorException("ClinicPatientAI reception " + state[1] + " - state is invalid");
        }
        if (state[0] == "RCIS")
        {
            if (float.Parse(state[1], CultureInfo.InvariantCulture) < -1)
                throw new SaveErrorException("Time left " + state[1] + " - is invalid");
        }
        if (state[0] == "GTDR" || state[0] == "SOWS" || state[0] == "WIRS")
        {
            if (int.Parse(state[1]) < -1)
                throw new SaveErrorException("ID: " + state[1] + " - is invalid");
        }

        return AIState;
    }

    /// <summary>
    /// 检查儿童AI状态字符串是否有效。
    /// </summary>
    /// <param name="AIState">要检查的AI状态字符串。</param>
    /// <returns>如果有效，则返回原始AI状态字符串。</returns>
    public static string CheckedChildAIState(string AIState)
    {
        if (string.IsNullOrEmpty(AIState))
            throw new SaveErrorException("Checked AIState is null or empty");

        var state = AIState.Split('!');

        if (state[0] != "SOWS" && state[0] != "GHCS" && state[0] != "GTDR" && state[0] != "WIDRS" && state[0] != "HS" && state[0] != "IS" && state[0] != "WIFOP" && state[0] != "PRS" && state[0] != "PS")
            throw new SaveErrorException(AIState + " - is invalid");

        if (state[0] == "GTDR" || state[0] == "SOWS" || state[0] == "IS" || state[0] == "PRS" || state[0] == "PS")
        {
            if (int.Parse(state[1], System.Globalization.CultureInfo.InvariantCulture) < -1)
                throw new SaveErrorException("ID: " + state[1] + " - is invalid");
        }
        return AIState;
    }

    /// <summary>
    /// 检查病人生物信息字符串是否有效。
    /// </summary>
    /// <param name="BIO">要检查的生物信息字符串。</param>
    /// <param name="objectName">对象名称。</param>
    /// <returns>如果有效，则返回原始生物信息字符串。</returns>
    public static string CheckedPatientBIO(string BIO, string objectName)
    {
        if (string.IsNullOrEmpty(BIO))
            throw new SaveErrorException("Checked BIO is null or empty");

        var bio = BIO.Split('*');

        int race = int.Parse(bio[0], System.Globalization.CultureInfo.InvariantCulture);
        int gender = int.Parse(bio[1], System.Globalization.CultureInfo.InvariantCulture);

        if (race < 0 || race > 2)
            throw new SaveErrorException(objectName + " race: " + bio[0] + " - is invalid");

        if (gender < 0 || gender > 1)
            throw new SaveErrorException(objectName + " gender: " + bio[1] + " - is invalid");

        if (int.Parse(bio[2], System.Globalization.CultureInfo.InvariantCulture) < 1 || int.Parse(bio[2], System.Globalization.CultureInfo.InvariantCulture) > 3)
        {
            if (int.Parse(bio[2], System.Globalization.CultureInfo.InvariantCulture) == 0)
                throw new SaveErrorException(objectName + " type: " + bio[2] + " - is for doctors");

            throw new SaveErrorException(objectName + " type: " + bio[2] + " - is invalid");
        }

        if (int.Parse(bio[4], System.Globalization.CultureInfo.InvariantCulture) < -1)
            throw new SaveErrorException(objectName + " VIPTime: " + bio[4] + " - is invalid");

        if (int.Parse(bio[5], System.Globalization.CultureInfo.InvariantCulture) < 0)
            throw new SaveErrorException(objectName + " Age: " + bio[5] + " - is invalid");

        //check HeadID
        if (int.Parse(bio[8], System.Globalization.CultureInfo.InvariantCulture) < 0 /*|| int.Parse(bio[8]) > MainList.Races[race].Genders[gender].HeadList.Count - 1*/)
        { //need to check it?
            throw new SaveErrorException(objectName + " Head ID: " + bio[8] + " - is invalid");
        }

        //check BodyID
        if (int.Parse(bio[9], System.Globalization.CultureInfo.InvariantCulture) < 0 /*|| int.Parse(bio[9]) > MainList.Races[race].Genders[gender].BodyList.Count - 1*/)
            throw new SaveErrorException(objectName + " Body ID: " + bio[9] + " - is invalid");

        //check LegsID
        if (int.Parse(bio[10], System.Globalization.CultureInfo.InvariantCulture) < 0 /*|| bio[10] > MainList.Races[race].Genders[gender].HeadList.Count - 1*/)
            throw new SaveErrorException(objectName + " Legs ID: " + bio[10] + " - is invalid");
            
        return BIO;
    }
    #endregion

    #region hospitalRoom
    /// <summary>
    /// 检查病床状态是否有效。
    /// </summary>
    /// <param name="status">要检查的病床状态。</param>
    /// <param name="message">错误消息。</param>
    /// <returns>如果有效，则返回原始病床状态。</returns>
    public static HospitalBedController.HospitalBed.BedStatus CheckedBedStatus(HospitalBedController.HospitalBed.BedStatus status, string message)
    {
        if (status != HospitalBedController.HospitalBed.BedStatus.NoExist && status != HospitalBedController.HospitalBed.BedStatus.OccupiedBed && status != HospitalBedController.HospitalBed.BedStatus.WaitForDiagnose && status != HospitalBedController.HospitalBed.BedStatus.WaitForPatient && status != HospitalBedController.HospitalBed.BedStatus.WaitForPatientSpawn)
            throw new SaveErrorException(message + " bed status:" + status + "is invalid");

        return status;
    }

    /// <summary>
    /// 检查病床保存数据是否有效。
    /// </summary>
    /// <param name="time">时间字符串。</param>
    /// <param name="status">状态字符串。</param>
    /// <param name="state">可旋转对象状态。</param>
    /// <param name="isDummy">是否为虚拟对象。</param>
    public static void CheckedBedSave(string time, string status, RotatableObject.State state, bool isDummy)
    {
        if (state != RotatableObject.State.working || isDummy)
            return;

        if (string.IsNullOrEmpty(time) && string.IsNullOrEmpty(status))
            throw new SaveErrorException("bed time ans status are empty");

        if (string.IsNullOrEmpty(time))        
            throw new SaveErrorException("bed time is empty");

        if (string.IsNullOrEmpty(status))        
            throw new SaveErrorException("bed status is empty");
    }

    /// <summary>
    /// 检查医院病人角色状态是否有效。
    /// </summary>
    /// <param name="status">要检查的角色状态。</param>
    /// <param name="message">错误消息。</param>
    /// <returns>如果有效，则返回原始角色状态。</returns>
    public static HospitalPatientAI.CharacterStatus CheckedHospitalPatientStatus(HospitalPatientAI.CharacterStatus status, string message)
    {
        // CV: HospitalPatientAI.CharacterStatus is an enum, so Value type that means that is never a reference but a value itself -> Can't never be null
        //if (status == null)
        //    throw new SaveErrorException("Checked characterStatus is null");

        if (status != HospitalPatientAI.CharacterStatus.Diagnose && status != HospitalPatientAI.CharacterStatus.Healed && status != HospitalPatientAI.CharacterStatus.InQueue && status != HospitalPatientAI.CharacterStatus.None)        
            throw new SaveErrorException(message + "patient status: " + status + "is invalid");

        return status;
    }

    /// <summary>
    /// 检查医院病人状态字符串是否有效。
    /// </summary>
    /// <param name="state">要检查的状态字符串。</param>
    /// <returns>如果有效，则返回原始状态字符串。</returns>
    public static string CheckedHospitalPatientState(string state)
    {
        if (string.IsNullOrEmpty(state))
            throw new SaveErrorException("Checked hospitalPatientState is null or empty");

        var stateSplit = state.Split('!');
        if (stateSplit[0] != "GTCR" && stateSplit[0] != "IB" && stateSplit[0] != "GTDR" && stateSplit[0] != "H" && stateSplit[0] != "RTB" && stateSplit[0] != "GH" && stateSplit[0] != "GTR")
            throw new SaveErrorException(stateSplit[0] + " - state is invalid");

        if (stateSplit[0] == "H")
        {
            if (float.Parse(stateSplit[1], CultureInfo.InvariantCulture) < 0)
                throw new SaveErrorException("H state waitTime: " + stateSplit[1] + " - is invalid");
        }

        return state;
    }
    #endregion

    #region probeTable
    /// <summary>
    /// 检查探测台状态是否有效。
    /// </summary>
    /// <param name="state">要检查的状态。</param>
    /// <returns>如果有效，则返回原始状态。</returns>
    public static TableState CheckedTableState(TableState state)
    {
        // CV: TableState is an enum, so Value type that means that is never a reference but a value itself -> Can't never be null
        //if (state == null)
        //    throw new SaveErrorException("Checked tableState is null");

        if (state != TableState.empty && state != TableState.producing && state != TableState.waitingForUser)
            throw new SaveErrorException("Probe table state: " + state + " - is invalid");

        return state;
    }
    #endregion

    #region plantation
    /// <summary>
    /// 检查种植园地块状态是否有效。
    /// </summary>
    /// <param name="state">要检查的状态。</param>
    /// <param name="GrowingPlant">正在生长的植物引用。</param>
    /// <returns>如果有效，则返回原始状态。</returns>
    public static EPatchState CheckedPatchState(EPatchState state, MedicineRef GrowingPlant)
    {
        // CV: EPatchState is an enum, so Value type that means that is never a reference but a value itself -> Can't ever be null
        //if (state == null)        
        //    throw new SaveErrorException("Checked tableState is null");

        if (state != EPatchState.disabled && state != EPatchState.empty && state != EPatchState.fallow && state != EPatchState.producing && state != EPatchState.renewing && state != EPatchState.waitingForHelp && state != EPatchState.waitingForRenew && state != EPatchState.waitingForUser)
            throw new SaveErrorException("Plantation state: " + state + " - is invalid");

        if (state == EPatchState.waitingForHelp || state == EPatchState.fallow)
        {
            if (GrowingPlant == null)
                throw new SaveErrorException("Plantation state: " + state + " - is invalid because doesn't have GrowingPlant value");
        }

        return state;
    }
    #endregion

    #region VIP
    /// <summary>
    /// 检查VIP定义字符串是否有效。
    /// </summary>
    /// <param name="VIPDefinition">要检查的VIP定义字符串。</param>
    /// <returns>如果有效，则返回原始VIP定义字符串。</returns>
    public static string CheckedVIPDEfinition(string VIPDefinition)
    {
        if (string.IsNullOrEmpty(VIPDefinition))
            throw new SaveErrorException("Checked VIPDefinition is null or empty");

        var definition = VIPDefinition.Split('/');
        if (int.Parse(definition[0]) < 0 || int.Parse(definition[0], System.Globalization.CultureInfo.InvariantCulture) > 1)
            throw new SaveErrorException("VIP gender is invalid");

        if (int.Parse(definition[1]) < 0 || int.Parse(definition[1], System.Globalization.CultureInfo.InvariantCulture) > 2)
            throw new SaveErrorException("VIP race is invalid");

        if (int.Parse(definition[2], System.Globalization.CultureInfo.InvariantCulture) < 0)        
            throw new SaveErrorException("VIP person is invalid");

        return VIPDefinition;
    }

    /// <summary>
    /// 检查VIP是否在医院内。
    /// </summary>
    /// <param name="VIPInside">VIP是否在医院内。</param>
    /// <param name="currentVIP">当前VIP游戏对象。</param>
    public static void CheckInsideVIP(bool VIPInside, GameObject currentVIP)
    {
        if (VIPInside == true && currentVIP == null)
            throw new SaveImpossibleException("currentVIP is null when VIPinside == true");
    }
    #endregion

    #region DoctorRoom
    /// <summary>
    /// 检查治疗数量是否有效。
    /// </summary>
    /// <param name="currentPatient">当前病人AI。</param>
    /// <param name="cureAmount">治疗数量。</param>
    /// <returns>如果有效，则返回病人保存字符串，否则返回空字符串。</returns>
    public static string CheckCureAmount(BasePatientAI currentPatient, int cureAmount)
    {
        if (currentPatient != null && cureAmount < 1)
            throw new SaveImpossibleException("currentPatient != null && cureAmount < 1");

        if (currentPatient != null)
            return currentPatient.SaveToString() + "?";

        return "";
    }
    #endregion

    #region externalRoom
    /// <summary>
    /// 检查外部房间状态是否有效。
    /// </summary>
    /// <param name="state">要检查的状态。</param>
    /// <returns>如果有效，则返回原始状态。</returns>
    public static ExternalRoom.EExternalHouseState CheckedExternalRoomState(ExternalRoom.EExternalHouseState state)
    {
        // CV: ExternalRoom.EExternalHouseState is an enum, so Value type that means that is never a reference but a value itself -> Can't ever be null
        //if (state == null)        
        //    throw new SaveErrorException("Checked tableState is null");

        if (state != ExternalRoom.EExternalHouseState.disabled && state != ExternalRoom.EExternalHouseState.enabled && state != ExternalRoom.EExternalHouseState.renewing && state != ExternalRoom.EExternalHouseState.waitingForRenew && state != ExternalRoom.EExternalHouseState.waitingForUser)
            throw new SaveErrorException("externalRoom state: " + state + " - is invalid");

        return state;
    }
    #endregion

    #region bedsToUnlock
    /// <summary>
    /// 检查待解锁病床数组是否有效。
    /// </summary>
    /// <param name="bedsToUnlock">待解锁病床数组。</param>
    /// <returns>如果有效，则返回原始数组。</returns>
    public static int[] CheckedBedsToUnlock(int[] bedsToUnlock)
    {
        if (bedsToUnlock == null)
            throw new SaveErrorException("Checked bedsToUnlock arr is null");

        for (int i = 0; i < bedsToUnlock.Length; ++i)
        {
            if (bedsToUnlock[i] < 0)
                throw new SaveErrorException("Beds to unlock" + i + ": " + bedsToUnlock[i] + " - is invalid");
        }
        return bedsToUnlock;
    }
    #endregion

    #region saveImpossible Checkers
    /// <summary>
    /// 检查对象是否为虚拟对象，如果是则抛出无法保存异常。
    /// </summary>
    /// <param name="isDummy">是否为虚拟对象。</param>
    /// <param name="objectTag">对象标签。</param>
    public static void CheckedIsNotDummy(bool isDummy, string objectTag)
    {
        if (isDummy)
            throw new SaveImpossibleException(objectTag + " - isDummy, can't save");
    }
    #endregion
}