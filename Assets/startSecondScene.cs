using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class startSecondScene : MonoBehaviour {
	public int currentHeight, currentWidth;
	public float xProp;
	public Vector2 clickPosition;
	public GameObject data;
	public void OnGUI()
	{
		currentHeight = Screen.currentResolution.height;
		currentWidth = Screen.currentResolution.width;
		xProp = 200.0f * currentWidth / currentHeight;
		/*if (GUI.Button (new Rect (currentWidth/2, currentHeight/2, currentWidth/6, currentHeight/8), "Version 1")) {
			data.gameObject.GetComponent<dataScript> ().parameter = 1;
			SceneManager.LoadScene ("second");
		}
		if (GUI.Button (new Rect (xProp - 50, 150, currentWidth/6, currentHeight/8), "Version 2"))
			Debug.Log ("Clicked VER2");
		if (GUI.Button (new Rect (xProp - 50, 250, currentWidth/6, currentHeight/8), "Version 3"))
			Debug.Log ("Clicked VER3");*/
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0))
			clickPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
	}
}
