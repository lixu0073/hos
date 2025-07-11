using UnityEngine;
using System.Collections;

public interface IPersonCloudController {
	void SetCloudState (CloudsManager.CloudState cloudState);
	void SetCloudMessageType (CloudsManager.MessageType messageType);

	void ExpandCloud ();
	void ColapseCloud ();
	void CloudColapsed ();

	void SmallCloudClick ();
	void BigCloudClick ();
}
