using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RoomController : MonoBehaviour
{
	readonly Vector2 BackRight = new Vector2(1,1);
	readonly Vector2 FrontRight = new Vector2(1,-1);
	readonly Vector2 FrontLeft = new Vector2(-1,-1);
	readonly Vector2 BackLeft = new Vector2(-1,1);
	public bool isRight;
	public bool isFront;

	public IsoEngine.Vector2i StartingPosition;
	public RoomCharacterInfo Room;
	private IsoEngine.PathInfo goPath;
	public PlayerStates state;
	private int index;
	private int startIndex;
	// Use this for initialization
	private Animator anim;
	private IsoEngine.Vector2i startPoint;
	private IsoEngine.Vector2i endPoint;
	public bool operational;
	private int targetwaypoint;
	//private float startTime;
	private float waitTime = 1.0f;
	private float runTime = 0f;
	public enum PlayerStates
	{
		Idle,
		Walking,
		Waiting
	}

	void OnEnable()
	{
		StartingPosition = Room.Rotate(this.transform.GetComponentInParent<IsoEngine.IsoObjectPrefabController>().prefabData.rotation, StartingPosition);
		Debug.Log (StartingPosition.x + " , " + StartingPosition.y);
		transform.localPosition = new Vector3(StartingPosition.x, 0 , StartingPosition.y);
	}

	void Start ()
	{

		goPath = new IsoEngine.PathInfo ();
		anim = transform.GetChild(0).GetComponent<Animator> ();

		operational = true;
		state = PlayerStates.Idle;
		startPoint = new IsoEngine.Vector2i ();

		//startTime = Time.time;
	}

	// Update is called once per frame
	void Update ()
	{
		if (operational) {
			switch (state) {
			case PlayerStates.Idle:
				waitTime = 1.0f;
				startPoint.x = (int)transform.localPosition.x;
				startPoint.y = (int)transform.localPosition.z;
				index = BaseGameState.RandomNumber(0, Room.ListOfPathPoints.Count);

				for(int i=0; i<Room.ListOfPathPoints.Count; ++i)
				{
					IsoEngine.Vector2i temp = Room.Rotate(this.transform.GetComponentInParent<IsoEngine.IsoObjectPrefabController>().prefabData.rotation, Room.ListOfPathPoints[i]);
					if(temp.x == startPoint.x && temp.y == startPoint.y)
					{
						startIndex = i;
					}
				}
				Debug.Log("Start index: " + startIndex);
				//Debug.Log("index: " + index);
				//Debug.Log("Curr doc pos: " + (int)transform.localPosition.x + ", " + (int)transform.localPosition.z);
				goPath = Room.ListOfPaths[Room.ListOfPathPoints.Count * startIndex + index];
				Debug.Log("End point: " + Room.ListOfPathPoints [index].x + ", " + Room.ListOfPathPoints [index].y);
				if (goPath.path.Count == 0) {
					Debug.Log("No path");
					state = PlayerStates.Waiting;
					break;
				}

				targetwaypoint = 0;
				endPoint = Room.Rotate(this.transform.GetComponentInParent<IsoEngine.IsoObjectPrefabController>().prefabData.rotation, goPath.path [targetwaypoint]);
				//Debug.Log("Going to point: " + endPoint.x + ", " + endPoint.y + " Rotation: " + this.transform.GetComponentInParent<IsoEngine.IsoObjectPrefabController>().prefabData.rotation);
				state = PlayerStates.Walking;
				break;

			case PlayerStates.Walking:
				if (runTime < 1.0f) {
					runTime += Time.deltaTime;

					if((startPoint.x - endPoint.x)>0 && (endPoint.y - startPoint.y) ==0){
						isFront=true;
						isRight=false;}
					if((startPoint.x - endPoint.x)<0 && (endPoint.y - startPoint.y) ==0){
						isFront=false;
						isRight=true;}
					if((startPoint.y - endPoint.y)>0 && (endPoint.x - startPoint.x) == 0){
						isFront=true;
						isRight=true;}
					if((startPoint.y - endPoint.y)<0 && (endPoint.x - startPoint.x) == 0){
						isFront=false;
						isRight=false;}
					SetAnimationDirection(isFront, isRight);
                    try {
					    anim.Play(AnimHash.Walk, 0, 0.0f);
                    }catch (Exception e){
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    transform.localPosition = Vector3.Lerp (new Vector3 (startPoint.x, 0, startPoint.y), new Vector3 (endPoint.x, 0, endPoint.y),  runTime / 1.0f);
				}
				else
				{
					//Debug.Log("In next");
					//startTime = Time.time;
					runTime = 0;
					targetwaypoint++;
					if (targetwaypoint >= goPath.path.Count) {
						targetwaypoint = 0;
						//Debug.Log("Change state to waiting");
						state = PlayerStates.Waiting;
                        try
                        {
                            anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
					}
					targetwaypoint = targetwaypoint % goPath.path.Count;

					startPoint = endPoint;
					endPoint = Room.Rotate(this.transform.GetComponentInParent<IsoEngine.IsoObjectPrefabController>().prefabData.rotation, goPath.path [targetwaypoint]);;
					//Debug.Log("Going to point: " + endPoint.x + ", " + endPoint.y + " Rotation: " + this.transform.GetComponentInParent<IsoEngine.IsoObjectPrefabController>().prefabData.rotation);

					//Debug.Log(goPath.path[targetwaypoint].x + ", " + goPath.path[targetwaypoint].y);
						
				}

				break;

			case PlayerStates.Waiting:


				if(waitTime < 3.0f)
				{

					waitTime+= Time.deltaTime;
				}
				else
				{
					//Debug.Log("Change state to idle");
//					goPath.path.Clear();
					state = PlayerStates.Idle;
				}
				break;

			}
		}
	}
	
	void SetAnimationDirection(bool isFront = true, bool isRight = true ){
		
		if(isFront && isRight){
			
			anim.SetFloat("tile_X",FrontRight.x);
			anim.SetFloat("tile_Y",FrontRight.y);
			
		}else if(isFront && !isRight){
			anim.SetFloat("tile_X",FrontLeft.x);
			anim.SetFloat("tile_Y",FrontLeft.y);
			
		}else if(!isFront && isRight){
			
			anim.SetFloat("tile_X",BackRight.x);
			anim.SetFloat("tile_Y",BackRight.y);
			
		}else if(!isFront && !isRight){
			
			anim.SetFloat("tile_X",BackLeft.x);
			anim.SetFloat("tile_Y",BackLeft.y);
		}
	}


}
