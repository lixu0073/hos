using UnityEngine;
using System.Collections;

/// <summary>
/// iOS平台专用警告框控制器，提供iOS原生对话框显示功能。
/// 通过KTAlertView插件实现iOS原生警告对话框的显示和回调处理。
/// </summary>
public class IOSAlert : MonoBehaviour {
#if UNITY_IPHONE
	static IOSAlert Instance;
    Coroutine _registerForAlertView;


    void Awake(){
		Instance = this;
	}
	
	void OnEnable () {
		_registerForAlertView = StartCoroutine(RegisterForAlertView());
	}
	void OnDisable () {
		KTAlertView.sharedInstance.AlertViewReturned -= AlertViewDelegate;
        if (_registerForAlertView != null) StopCoroutine(_registerForAlertView);
	}
	IEnumerator RegisterForAlertView () {
		yield return new WaitForSeconds(.5f);
		KTAlertView.sharedInstance.AlertViewReturned += AlertViewDelegate;
	}
	
	public void ShowAlert(string title, string message, string buttonName){
		string[] buttons = new string[] {""};
		KTAlertView.sharedInstance.ShowAlertViewCS(title, message, buttonName, buttons, 10);
	}
	
	public void ShowAlert(string title, string message, string buttonName, int tag){
		string[] buttons = new string[] {""};
		KTAlertView.sharedInstance.ShowAlertViewCS(title, message, buttonName, buttons, tag);
	}
	
	void AlertViewDelegate (int tag, int clickedIndex) {
		print ("CS Alertview tag= " + tag +" clicked= " + clickedIndex);
		if (tag == 99) {
			Application.Quit();
		}
	}
	
	public static IOSAlert GetInstance(){
		return Instance;
	}
#endif
}