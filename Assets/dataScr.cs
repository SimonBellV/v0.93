using UnityEngine;
using System.Collections;
using MoBack;
using System.Collections.Generic;

public class dataScr : MonoBehaviour {
	public string currentID;
	public int versionOfScene, rowPosition, maxScene;
	public float a, b, x, y;
	public bool initialized;
	List<MoBackRow> rows;
	MoBackTableInterface exampleTable;
	void Start(){
		currentID = "";
		maxScene = 1;
		rowPosition = -1;
	}

	public IEnumerator Data(){//здесь происходит поиск данных по ID и при + результате внесение их в переменные для дальнейшей работы
		exampleTable = new MoBackTableInterface ("PlayersData");
		MoBackRequest<List<MoBackRow>> tableContentsFetch = exampleTable.GetRows ();
		yield return tableContentsFetch.Run ();
		if (tableContentsFetch.State == MoBackRequest.RequestState.COMPLETED) {
			rows = tableContentsFetch.ResponseValue;
			for (int i = 0; i < rows.Count; i++)
				if (rows [i].GetString ("IDcode") == currentID) {
					rowPosition = i;
					MoBackRequest response1 = exampleTable.DeleteObject (rows [rowPosition].GetString ("objectId"));
					yield return response1.Run ();
					a = rows [i].GetFloat ("aValue");
					b = rows [i].GetFloat ("bValue");
					x = rows [i].GetFloat ("xValue");
					y = rows [i].GetFloat ("yValue");
					maxScene = rows [i].GetInt ("maxScene");
					break;
				}
		} else 
			Debug.Log (tableContentsFetch.errorDetails);

		initialized = true;
	}

	void OnApplicationQuit()//по завершению данных сохраняем данные
	{
		StartCoroutine ("UpdateRow");
	}

	IEnumerator UpdateRow()
	{
		MoBackRow newRow = new MoBackRow ("PlayersData");
		newRow.SetValue ("IDcode", currentID);
		newRow.SetValue ("aValue", GameObject.Find("Main").GetComponent<scene2script>().userData.a);
		newRow.SetValue ("bValue", GameObject.Find("Main").GetComponent<scene2script>().userData.b);
		newRow.SetValue ("xValue", GameObject.Find("Main").GetComponent<scene2script>().userData.x);
		newRow.SetValue ("yValue", GameObject.Find("Main").GetComponent<scene2script>().userData.y);
		newRow.SetValue ("maxScene", maxScene);
		MoBackRequest response = newRow.Save ();
		yield return response.Run ();
	}
}
