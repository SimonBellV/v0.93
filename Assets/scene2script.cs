using UnityEngine;
using UnityEngine.UI;

public class scene2script : MonoBehaviour {
	//public GameObject Data;
	public bool isFormSet;
	private GameObject textA, textB, textX, textY/*для букв-обозначений*/, textBoxA, textBoxB, sliderX, sliderY/*для изменяемых переменных*/, cubeA, cubeB, cubeX, cubeY/*для кубов*/, error;
	public struct data{
		public float a;
		public float b;
		public float x;
		public float y;
	}
	private int fontSize;
	private GameObject Data;//чтобы каждый раз не искать Data
	public data userData;

	void Start () {
		Data = GameObject.Find ("Data");
		isFormSet = false;
		fontSize = Screen.currentResolution.height / 20;//задание размера шрифта
		error = GameObject.Find("Error");//месседжбокс для случая когда нельзя загрузить вариант второй сцены
		error.SetActive (false);
		//задание переменных
		textA = GameObject.Find ("TextA");
		textB = GameObject.Find ("TextB");
		textX = GameObject.Find ("TextX");
		textY = GameObject.Find ("TextY");

		textBoxA = GameObject.Find ("TextBoxA");
		textBoxB = GameObject.Find ("TextBoxB");
		sliderX = GameObject.Find ("SliderX");
		sliderY = GameObject.Find ("SliderY");

		cubeA = GameObject.Find ("CubeA");
		cubeB = GameObject.Find ("CubeB");
		cubeX = GameObject.Find ("CubeX");
		cubeY = GameObject.Find ("CubeY");
		//все объекты предварительно скрываются
		textA.SetActive (false);
		textBoxA.SetActive (false);
		cubeA.SetActive (false);

		textB.SetActive (false);
		textBoxB.SetActive (false);
		cubeB.SetActive (false);

		textX.SetActive (false);
		sliderX.SetActive (false);
		cubeX.SetActive (false);

		textY.SetActive (false);
		sliderY.SetActive (false);
		cubeY.SetActive (false);
	}

	void Update () {
		if (GameObject.Find ("Data").GetComponent<dataScr> ().initialized) {//чтобы не делать лишних проверок до загрузки данных
			if (!isFormSet) {
				userData.a = Data.GetComponent<dataScr> ().a;//сразу извлекаем данные, поскольку мы то поле сразу удаляем, и создаем новое при выходе из программы
				userData.b = Data.GetComponent<dataScr> ().b;
				userData.x = Data.GetComponent<dataScr> ().x;
				userData.y = Data.GetComponent<dataScr> ().y;
				if (Data.GetComponent<dataScr> ().maxScene > Data.GetComponent<dataScr> ().versionOfScene)//проверка на допустимость загрузки варианта сцены
					error.SetActive (true);			
				else {
					Data.GetComponent<dataScr> ().maxScene = Data.GetComponent<dataScr> ().versionOfScene;
					//задание шрифтов и открытие нужных объектов в зависимости от сцены
					if (Data.GetComponent<dataScr> ().versionOfScene == 1) {
						textA.SetActive (true);
						textBoxA.SetActive (true);
						cubeA.SetActive (true);

						textB.SetActive (true);
						textBoxB.SetActive (true);
						cubeB.SetActive (true);

						textA.GetComponent<Text> ().fontSize = fontSize;
						textB.GetComponent<Text> ().fontSize = fontSize;

						textBoxA.GetComponent<InputField> ().text = userData.a.ToString ();
						textBoxB.GetComponent<InputField> ().text = userData.b.ToString ();
					}
					if (Data.GetComponent<dataScr> ().versionOfScene == 2) {
						textA.SetActive (true);
						textBoxA.SetActive (true);
						cubeA.SetActive (true);

						textB.SetActive (true);
						textBoxB.SetActive (true);
						cubeB.SetActive (true);

						textX.SetActive (true);
						sliderX.SetActive (true);
						cubeX.SetActive (true);

						textA.GetComponent<Text> ().fontSize = fontSize;
						textB.GetComponent<Text> ().fontSize = fontSize;
						textX.GetComponent<Text> ().fontSize = fontSize;

						textBoxA.GetComponent<InputField> ().text = userData.a.ToString ();
						textBoxB.GetComponent<InputField> ().text = userData.b.ToString ();
						sliderX.GetComponent<Slider> ().value = userData.x / 50;
					}
					if (Data.GetComponent<dataScr> ().versionOfScene == 3) {
						textX.SetActive (true);
						sliderX.SetActive (true);
						cubeX.SetActive (true);

						textY.SetActive (true);
						sliderY.SetActive (true);
						cubeY.SetActive (true);

						textX.GetComponent<Text> ().fontSize = fontSize;
						textY.GetComponent<Text> ().fontSize = fontSize;

						sliderX.GetComponent<Slider> ().value = userData.x / 50;
						sliderY.GetComponent<Slider> ().value = userData.y / 50;
					}
					isFormSet = true;//то есть мы заходим сюда только один раз, по сути метод Start, только выполняется после того, как значения взяты с сервера
				}
			} else {//здесь будет происходить обновление информации
				if (Data.GetComponent<dataScr> ().versionOfScene == 1) {
					float.TryParse (textBoxA.GetComponent<InputField> ().text, out userData.a);//перевод значений из inputfield в float переменную а
					if (userData.a > 50)//она не может быть больше 50
					userData.a = 50;
					if (userData.a < 0)//и отрицательным
						userData.a = 0;
					cubeA.GetComponent<Transform> ().localScale = new Vector3 (userData.a, userData.a, 0);
					float.TryParse (textBoxB.GetComponent<InputField> ().text, out userData.b);
					if (userData.b > 50)
						userData.b = 50;
					if (userData.b < 0)
						userData.b = 0;
					cubeB.GetComponent<Transform> ().localScale = new Vector3 (userData.b, userData.b, 0);
				}
				if (Data.GetComponent<dataScr> ().versionOfScene == 2) {
					float.TryParse (textBoxA.GetComponent<InputField> ().text, out userData.a);
					if (userData.a > 50)
						userData.a = 50;
					if (userData.a < 0)
						userData.a = 0;
					cubeA.GetComponent<Transform> ().localScale = new Vector3 (userData.a, userData.a, 0);
					float.TryParse (textBoxB.GetComponent<InputField> ().text, out userData.b);
					if (userData.b > 50)
						userData.b = 50;
					if (userData.b < 0)
						userData.b = 0;
					cubeB.GetComponent<Transform> ().localScale = new Vector3 (userData.b, userData.b, 0);
					userData.x = sliderX.GetComponent<Slider> ().value * 50;//поскольку значение меняется от 0 до 1 домножаем на 50
					cubeX.GetComponent<Transform> ().localScale = new Vector3 (userData.x, userData.x, 0);
				}
				if (Data.GetComponent<dataScr> ().versionOfScene == 3) {
					userData.x = sliderX.GetComponent<Slider> ().value * 50;
					cubeX.GetComponent<Transform> ().localScale = new Vector3 (userData.x, userData.x, 0);
					userData.y = sliderY.GetComponent<Slider> ().value * 50;
					cubeY.GetComponent<Transform> ().localScale = new Vector3 (userData.y, userData.y, 0);
				}
			}
		}
	}
}
