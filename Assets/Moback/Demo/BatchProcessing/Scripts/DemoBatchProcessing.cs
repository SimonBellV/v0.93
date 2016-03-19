//-----------------------------------------------------------------------
// <copyright file="DemoBatchProcessing.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoBack;

/// <summary>
/// Class for Demo batch processing.
/// </summary>
public class DemoBatchProcessing : MonoBehaviour 
{
    /// <summary>
    /// The buttons.
    /// </summary>
    public Button addButton, showButton, saveButton, updateButton, deleteButton, updatePositionButton, backButton; 
    
    /// <summary>
    /// The text to display the message.
    /// </summary>
    public Text displayText;
    
    /// <summary>
    /// InputFields.
    /// </summary>
    public InputField valueInput, currentValueUpdateInput, valueUpdateInput;
    
    /// <summary>
    /// Scene gameobject.
    /// </summary>
    public GameObject mainScene, updateScene;

    /// <summary>
    /// The name of the demo batch table .
    /// </summary>
    private const string DemoBatchTableName = "DemoBatchTable";

    /// <summary>
    /// The demo batch objects.
    /// </summary>
    private List<MoBackRow> demoBatchObjects = new List<MoBackRow>();
    
    /// <summary>
    /// Start this instance.
    /// </summary>
     public void Start() 
    {
        addButton.onClick.AddListener(OnAddButtonClick);
        showButton.onClick.AddListener(OnShowButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        updateButton.onClick.AddListener(OnUpdateButtonClick);
        deleteButton.onClick.AddListener(OnDeleteButtonClick);
        updatePositionButton.onClick.AddListener(OnUpdatePositionButtonClick);
        backButton.onClick.AddListener(OnBackButtonClick);
    }

    /// <summary>
    /// Raises the add button click event.
    /// </summary>
    private void OnAddButtonClick()
    {
        addButton.enabled = false;

        if (!string.IsNullOrEmpty(valueInput.text))
        {
        	MoBackRow row = new MoBackRow(DemoBatchTableName);
        	row.SetValue("Value", valueInput.text.ToString());
        	demoBatchObjects.Add(row);
            Display(valueInput.text + " Added successfully ");
        	valueInput.text = string.Empty;
        }
        else
        {
            Display("Field Cannot be Blank");
        }

        addButton.enabled = true;
    }

	/// <summary>
	/// Raises the show button click event.
	/// </summary>
	private void OnShowButtonClick()
	{
		showButton.enabled = false;

		if (demoBatchObjects.Count > 0)
		{
			string data = null;
			MoBackRow t = new MoBackRow(DemoBatchTableName);
			for (int i = 0; i < demoBatchObjects.Count; i++)
			{
				string val = demoBatchObjects[i].GetString("Value").ToString();
				if (!string.IsNullOrEmpty(data))
				{
					data += ", " + val;
				}
				else
				{
					data += val;
				}
			}
			Display(data);
		}
		else
		{
			Display("No data to display.");
		}

		showButton.enabled = true;
	}

	/// <summary>
	/// Raises the update button click event.
	/// </summary>
	private void OnUpdateButtonClick()
	{
		if (demoBatchObjects.Count > 0)
		{
			ChangeScene(updateScene);
		}
		else
		{
			Display("No data to Update");
		}
	}

    /// <summary>
    /// Raises the update position button click event.
    /// </summary>
    private void OnUpdatePositionButtonClick()
    {
		updatePositionButton.enabled = false;

        if (!string.IsNullOrEmpty(currentValueUpdateInput.text) && !string.IsNullOrEmpty(valueUpdateInput.text))
        {
			int matchingRows=0;
			foreach(MoBackRow mbRow in demoBatchObjects)
			{
				if( mbRow.GetString("Value").ToString() == currentValueUpdateInput.text )
				{
					mbRow.SetValue("Value", valueUpdateInput.text);
					Display("Object Updated ");
					matchingRows++;
				}
			}
			if(matchingRows<0)
			{
				Display("Invalid Value");
			}
        }
        else
        {
            Display("Fields Cannot be Blank");
        }

        updatePositionButton.enabled = true;
    }

    /// <summary>
    /// Raises the save button click event.
    /// </summary>
    private void OnSaveButtonClick()
    {
        StartCoroutine(SaveMultipleObjects());
    }
    
    /// <summary>
    /// Raises the back button click event.
    /// </summary>
    private void OnBackButtonClick()
    {
        ChangeScene(mainScene);
    }
        
    /// <summary>
    /// Raises the delete button click event.
    /// </summary>
    private void OnDeleteButtonClick()
    {
        StartCoroutine(DeleteMultipleObjects());
    }
    
    /// <summary>
    /// Saves multiple MoBack Objects(Row) with a single call.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    private IEnumerator SaveMultipleObjects()
    {
        if (demoBatchObjects == null || demoBatchObjects.Count <= 0)
        {
            Display("No data in object");
        }
        saveButton.enabled = false;
        MoBackTableInterface demoBatchTable = new MoBackTableInterface(DemoBatchTableName);
        MoBackRequest saveMultipleObjectsRequest = demoBatchTable.RequestB.SaveMultipleObjects(demoBatchObjects); 
        
        yield return saveMultipleObjectsRequest.Run();
        
        if (saveMultipleObjectsRequest.State == MoBackRequest.RequestState.COMPLETED)
        {
            foreach (var item in demoBatchObjects)
            {
                Display("Object saved");
            }
        }
        if (saveMultipleObjectsRequest.State == MoBackRequest.RequestState.ERROR)
        {
            Display(saveMultipleObjectsRequest.errorDetails.Message);
        }
        saveButton.enabled = true;
    }
    
    /// <summary>
    /// Deletes the multiple objects.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    private IEnumerator DeleteMultipleObjects()
    {
        if (demoBatchObjects == null || demoBatchObjects.Count <= 0)
        {
            Debug.LogError("Need to populate demoBatchObjects first");
            Display("Need to populate demoBatchObjects first");
            yield break;
        }
        deleteButton.enabled = false;
        MoBackTableInterface demoBatchTable = new MoBackTableInterface(DemoBatchTableName);
        
        MoBackRequest deleteMultipleObjectsRequest = demoBatchTable.RequestB.DeleteMultipleOjbects(demoBatchObjects);
        
        yield return deleteMultipleObjectsRequest.Run();
        if (deleteMultipleObjectsRequest.State == MoBackRequest.RequestState.COMPLETED)
        {
             Display("Object deleted ");
            demoBatchObjects = new List<MoBackRow>();
        }
        else if (deleteMultipleObjectsRequest.State == MoBackRequest.RequestState.ERROR)
        {
            Display(deleteMultipleObjectsRequest.errorDetails.Message);
        }
        deleteButton.enabled = false;
    }

    /// <summary>
    /// To Display the message on the screen. 
    /// </summary>
    /// <param name="value">Value.</param>
    public void Display(string value)
    {
        displayText.text = value;
    }

    /// <summary>
    /// Switches on and off the different gameObjects in the demo scene.
    /// </summary>
    /// <param name="prompt">A gameObject associated with a scene.</param>
    private void ChangeScene(GameObject prompt)
    {
        mainScene.SetActive(false);
        updateScene.SetActive(false);
        prompt.SetActive(true);
    }
}
