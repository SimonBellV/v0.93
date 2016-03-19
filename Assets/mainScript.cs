using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class mainScript : MonoBehaviour {
	private GameObject inputFieldGo;
	public string currentID;
	public GameObject Data;
	private InputField inputFieldCo;
	private bool initialized = false;
	// Use this for initialization
	void Start () {
		currentID = "";//задание начального значения для проверки чтобы если что не перейти в след. сцену без ID
		inputFieldGo = GameObject.Find ("TextBox");
		inputFieldCo = inputFieldGo.GetComponent<InputField> ();//необходимо для извлечения данных из InputField
		GameObject.Find ("Version1").GetComponentInChildren<Text> ().fontSize = Screen.currentResolution.height/ 20;//задание шрифтов
		GameObject.Find ("Version2").GetComponentInChildren<Text> ().fontSize = Screen.currentResolution.height/ 20;
		GameObject.Find ("Version3").GetComponentInChildren<Text> ().fontSize = Screen.currentResolution.height/ 20;
	}

	void Update () {
		if (!inputFieldGo.GetComponent<InputField> ().isFocused)
			currentID = inputFieldCo.text;//пока inputfield в фокусе идет обновление данных
		if (currentID != "" && !initialized)
			initialized = true;
	}

	public void startSceneOne()
	{
		GetComponentInChildren<AudioSource> ().Play();//при нажатии на кнопку издать звук
		if (currentID != ""){
			Data.GetComponent<dataScr> ().currentID = currentID;//задать в контейнер введенный ID
			Data.GetComponent<dataScr> ().StartCoroutine (Data.GetComponent<dataScr> ().Data ());//обращение к серверу за данными, асинхронный процесс
			Data.GetComponent<dataScr> ().versionOfScene = 1;
			DontDestroyOnLoad (Data);//не удалять объект Data при загрузке
			DontDestroyOnLoad(GameObject.Find("moBack"));
			SceneManager.LoadScene ("second");//загрузить вторую сцену
		}
	}

	public void startSceneTwo()
	{
		GetComponentInChildren<AudioSource> ().Play ();
		if (currentID != "") {
			Data.GetComponent<dataScr> ().currentID = currentID;
			Data.GetComponent<dataScr> ().StartCoroutine (Data.GetComponent<dataScr> ().Data ());//обращение к серверу за данными, асинхронный процесс
			Data.GetComponent<dataScr> ().versionOfScene = 2;
			DontDestroyOnLoad (Data);
			DontDestroyOnLoad (GameObject.Find ("moBack"));
			SceneManager.LoadScene ("second");
		}
	}
	public void startSceneThree()
	{
		GetComponentInChildren<AudioSource> ().Play();
		if (currentID != "") {
			Data.GetComponent<dataScr> ().currentID = currentID;
			Data.GetComponent<dataScr> ().StartCoroutine (Data.GetComponent<dataScr> ().Data ());//обращение к серверу за данными, асинхронный процесс
			Data.GetComponent<dataScr> ().versionOfScene = 3;
			DontDestroyOnLoad (Data);
			DontDestroyOnLoad(GameObject.Find("moBack"));
			SceneManager.LoadScene ("second");
		}
	}
}
	
