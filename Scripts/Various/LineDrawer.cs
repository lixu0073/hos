using UnityEngine;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
	public List<Hospital.RotatableObject> objs;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
	}

	public Material lineMat;// = new Material("Shader \"Lines/Colored Blended\" {" + "SubShader { Pass { " + "    Blend SrcAlpha OneMinusSrcAlpha " + "    ZWrite Off Cull Off Fog { Mode Off } " + "    BindChannels {" + "      Bind \"vertex\", vertex Bind \"color\", color }" + "} } }");

	private void DrawObject(Vector2 position, int sizex, int sizey)
	{

		GL.Color(new Color(0f, 0f, 0f, 1f));

		GL.Begin(GL.LINES);
		GL.Vertex3(position.x, 0f, position.y);
		GL.Vertex3(position.x + sizex, 0f, position.y);
		GL.End();

		GL.Begin(GL.LINES);
		GL.Vertex3(position.x + sizex, 0f, position.y);
		GL.Vertex3(position.x + sizex, 0f, position.y + sizey);
		GL.End();

		GL.Begin(GL.LINES);
		GL.Vertex3(position.x + sizex, 0f, position.y + sizey);
		GL.Vertex3(position.x, 0f, position.y + sizey);
		GL.End();

		GL.Begin(GL.LINES);
		GL.Vertex3(position.x, 0f, position.y + sizey);
		GL.Vertex3(position.x, 0f, position.y);
		GL.End();
	}
	void OnPostRender()
	{
		var mesh = new Mesh();
		mesh.triangles[5] = 3;
		if (objs == null)
			return;
		lineMat.SetPass(0);
		foreach (var obj in objs)
			DrawObject(new Vector2(obj.position.x - 0.5f, obj.position.y - 0.5f), obj.actualData.tilesX, obj.actualData.tilesY);
	}
}
