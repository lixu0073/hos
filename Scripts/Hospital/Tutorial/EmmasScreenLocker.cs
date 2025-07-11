using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using IsoEngine;

public class EmmasScreenLocker : MonoBehaviour
{
    /*
    private Vector2 pos = Vector2.zero;
    private Vector3 anchorPoint;
    private RectTransform emmasInGameTransform;

	private float scaleValue = 1f;

	private InGamePopupPosition _mode;
	private Vector3 _position;

    private bool isEmmaStatic = true;  // jesli chcemy miec dynamiczną emme to tutaj false

    private int text_buble_widht = 0;
    private int text_buble_height = 0;

    public RectTransform EmmasInGameTextCloud;
    public RectTransform EmmasInGameImage;

    private enum State
    {
        idle,
        creation
    }

    void Start()
    {
        emmasInGameTransform = transform.GetChild(0).GetComponent<RectTransform>();

        if (isEmmaStatic)
        {
            //transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            scaleValue = 1f;


        }
        else scaleValue = transform.localScale.x;

    }
    private State state = State.idle;

    void Update()
    {
        if (isEmmaStatic) UpdateEmmaStaticPos();
    }

    void LateUpdate()
    {
        if (_mode == InGamePopupPosition.UI_OwnPosition) {
            return;
        }

        if (!isEmmaStatic)
        {
            switch (state)
            {
                case State.creation:
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(UIController.get.canvas.transform as RectTransform, ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(anchorPoint), UIController.get.canvas.worldCamera, out pos);
                    transform.position = UIController.get.canvas.transform.TransformPoint(pos);
                    UpdateEmmaScale();
                    break;
                case State.idle:
                    break;
            }
		}

    }


    public Vector2 GetPosition(InGamePopupPosition position)
    {
        Vector2 newPosition = Vector3.zero;
        switch (position)
        {
            case InGamePopupPosition.UI_OwnPosition:
                newPosition = new Vector2(TutorialController.Instance.GetCurrentStepData().OwnPopupPosition.x, TutorialController.Instance.GetCurrentStepData().OwnPopupPosition.y);           
                break;
            case InGamePopupPosition.UI_DefaultPosition:
                newPosition = Vector2.zero;
                break;
            default:
                break;
        }
        return newPosition;
    }



    public void LockEmma()
    {
        if (!isEmmaStatic)
        {

            Vector2 anchor_move = GetPosition(_mode);
            if (_mode == InGamePopupPosition.UI_OwnPosition)
            {
                GetComponent<RectTransform>().anchoredPosition = GetPosition(_mode);
                return;
            }


            if (TutorialController.Instance.GetCurrentStepData().NeedsOpenAnimation)
            {
                anchorPoint = ReferenceHolder.Get().engine.MainCamera.RayCast(new Vector2(_position.x, _position.y));
                GetComponent<RectTransform>().anchoredPosition = GetPosition(_mode);

                anchorPoint = ReferenceHolder.Get().engine.MainCamera.RayCast(new Vector2(_position.x + anchor_move.x, _position.y + anchor_move.y));

                state = State.creation;
                UpdateEmmaScale();

            }
            else {
                state = State.idle;
            }
        }


    }

	public void UnlockOnEmma()
	{
        Debug.LogWarning("UnlockOnEmma()");
		//gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;
	}
		
	public void UpdateEmmaScale()
	{
		scaleValue = 1f-Mathf.Clamp (ReferenceHolder.Get ().engine.MainCamera.GetZoom () / ReferenceHolder.Get().engine.MainCamera.GetMaxZoom(), 0.1f, 0.6f);
		transform.localScale = new Vector3(scaleValue,scaleValue,scaleValue);

	}

    public void InitializePopupValues(Vector3 position, InGamePopupPosition mode)
	{
		_mode = mode;
		_position = position;
	}

    public void UpdateEmmaStaticPos()
    {
        //Debug.LogWarning(TutorialController.Instance.GetCurrentStepData().OwnPopupPosition.y);
        text_buble_widht = (int)(EmmasInGameTextCloud.rect.width + EmmasInGameImage.rect.width);
        text_buble_height = (int)EmmasInGameImage.rect.height;

        emmasInGameTransform.anchoredPosition = new Vector2(-text_buble_widht / 2 + TutorialController.Instance.GetCurrentStepData().OwnPopupPosition.x, text_buble_height / 2 + TutorialController.Instance.GetCurrentStepData().OwnPopupPosition.y);
    }

    */
}