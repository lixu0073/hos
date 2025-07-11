using UnityEngine;
using System.Collections;

public class DoorBordersBlocker : MonoBehaviour
{
    [SerializeField]
    private LineRenderer line1 = null;
    [SerializeField]
    private LineRenderer line2 = null;
    [SerializeField]
    private LineRenderer line3 = null;
    [SerializeField]
    private LineRenderer line4 = null;

    private int mode = 0;

    public void SetBorderSize(int x, int y)
	{
            line1.SetPositions(new Vector3[] { new Vector3(0, 0, 0), new Vector3(x, 0, 0) });
            line2.SetPositions(new Vector3[] { new Vector3(x, 0, -0.05f), new Vector3(x, 0, y + 0.05f) });
            line3.SetPositions(new Vector3[] { new Vector3(x, 0, y), new Vector3(0, 0, y) });
            line4.SetPositions(new Vector3[] { new Vector3(0, 0, y + 0.05f), new Vector3(0, 0, -0.05f) });

    }
	public void SetBorderColor(int i)
	{
		    Material mat = ResourcesHolder.Get().mutalPathBorder;

            line1.sharedMaterial = line2.material = line3.material = line4.material = mat;

            line1.sharedMaterial.mainTextureScale = new Vector2(3, 1);
            line3.sharedMaterial.mainTextureScale = new Vector2(3, 1);

            line2.sharedMaterial.mainTextureScale = new Vector2(3, 1);
            line4.sharedMaterial.mainTextureScale = new Vector2(3, 1);
        //	print("setting color " + i);
    }

    public int GetMode()
    {
        return mode;
    }

    public void Init()
    {
        gameObject.SetActive(true);
        SetBorderSize(1, 1);
        SetBorderColor(3);
    }

    public void HideLine(int id)
    {
        switch(id)
        {
            case 1:
                line1.enabled = false;
                break;
            case 2:
                line2.enabled = false;
                break;
            case 3:
                line3.enabled = false;
                break;
            case 4:
                line4.enabled = false;
                break;
        }
    }

}
