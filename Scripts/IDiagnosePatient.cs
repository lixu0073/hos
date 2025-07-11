using System;
using UnityEngine;

/// <summary>
/// 诊断病人接口，定义了病人诊断和治疗过程中所需的方法。
/// </summary>
public interface IDiagnosePatient {
	
	/// <summary>
	/// 获取角色精灵信息。
	/// </summary>
	BaseCharacterInfo GetSprites ();

	/// <summary>
	/// 获取诊断所需时间。
	/// </summary>
	float GetDiagnoseTime ();

	/// <summary>
	/// 获取队列ID。
	/// </summary>
	int GetQueueID();

	/// <summary>
	/// 设置队列ID。
	/// </summary>
	/// <param name="Value">队列ID。</param>
	void SetQueueID(int Value);

	/// <summary>
	/// 获取AI组件。
	/// </summary>
	Component GetAI();

	/// <summary>
	/// 将病人状态设置为进入诊断室。
	/// </summary>
	/// <param name="room">诊断室实例。</param>
	void StateToDiagRoom(DiagnosticRoom room);
    
	/// <summary>
	/// 检查治疗是否完成。
	/// </summary>
	bool DoneHealing ();

	/// <summary>
	/// 获取病人是否回家。
	/// </summary>
	bool GetGoHome ();

	/// <summary>
	/// 通知病人状态变化。
	/// </summary>
	/// <param name="id">通知ID。</param>
	/// <param name="parameters">通知参数。</param>
	void Notify (int id, object parameters);

	/// <summary>
	/// 停止诊断。
	/// </summary>
	/// <param name="room">诊断室实例。</param>
	void StopDiagnose (DiagnosticRoom room);

	/// <summary>
	/// 设置病人状态为诊断中。
	/// </summary>
	void SetStateDiagnose ();

	/// <summary>
	/// 设置病人状态为排队中。
	/// </summary>
	void SetStateInQueue();

    /// <summary>
    /// 从其他病人那里添加细菌。
    /// </summary>
    void AddBacteriaFromOtherPatient();

    /// <summary>
    /// 获取医院角色信息。
    /// </summary>
    HospitalCharacterInfo GetHospitalCharacterInfo();

	/// <summary>
	/// 检查病人是否处于回家状态。
	/// </summary>
	bool IsGoHomeState();

	/// <summary>
	/// 获取病人是否回家（另一个同名方法，可能用于不同上下文）。
	/// </summary>
	bool goHomeGet ();

    /// <summary>
    /// 加速诊断或治疗过程。
    /// </summary>
    /// <param name="time">加速后的时间。</param>
    /// <param name="timePassed">已过去的时间。</param>
    /// <param name="isTaken">是否已加速。</param>
    /// <returns>是否成功加速。</returns>
    bool SpeedUp(out float time, float timePassed, out bool isTaken);
}