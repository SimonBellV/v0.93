//-----------------------------------------------------------------------
// <copyright file="DemoRelations.cs" company="moBack">
// Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoBack;
using UnityEngine.UI;

/// <summary>
/// Demo relations controld the demo for showing users how relations work in moback.
/// </summary>
public class DemoRelations : MonoBehaviour
{
    public InputField parentInput, childInput;
    public Button saveNameButton, addItemButton, createRelationButton, getRelationButton, removeRelationButton, getRelationPointer, backButton_1, backButton_2;
    public GameObject inputNameScene, inputItemScene, relationScene;
    public Text displayText;

    private const string DemoRelationTableName = "Demo_Relations";
    private const string ChildRelationTableName = "Child_RelationTable";
    private const string DemoRelationColumn = "RelationColumn";
    private const string ItemTypeColumn = "Type";

    private MoBackRow playerRow = new MoBackRow(DemoRelationTableName);
    private List<MoBackRow> items = new List<MoBackRow>();

    /// <summary>
    /// Start this instance.
    /// </summary>
    public void Start()
    {
        saveNameButton.onClick.AddListener(OnSavenameButtonClick);
        addItemButton.onClick.AddListener(OnAddItemButtonClick);
        createRelationButton.onClick.AddListener(OnCreateRelationButtonClick);
        getRelationButton.onClick.AddListener(OnGetRelationButtonClick);
        removeRelationButton.onClick.AddListener(OnRemoveRelatiomClick);
        getRelationPointer.onClick.AddListener(OnGetRelationPointerClick);
        backButton_1.onClick.AddListener(OnBackButtonlClick_1);
        backButton_2.onClick.AddListener(OnBackButtonlClick_2);
        createRelationButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Handles displaying results.
    /// </summary>
    /// <param name="value">Value of the string to be displayed.</param>
    public void Display(string value)
    {
        displayText.text = value;
    }

    /// <summary>
    /// Raises the back buttonl click_1 event.
    /// </summary>
    private void OnBackButtonlClick_1()
    {
        ChangeScene(inputNameScene);
    }

    /// <summary>
    /// Raises the back buttonl click_2 event.
    /// </summary>
    private void OnBackButtonlClick_2()
    {
        ChangeScene(inputItemScene);
    }

    /// <summary>
    /// Raises the get relation button click event.
    /// </summary>
    private void OnGetRelationButtonClick()
    {
        StartCoroutine(GetRelation());
    }

    /// <summary>
    /// Raises the savename button click event.
    /// </summary>
    private void OnSavenameButtonClick()
    {
        StartCoroutine(SavePlayerName());
    }

    /// <summary>
    /// Raises the remove relatiom click event.
    /// </summary>
    private void OnRemoveRelatiomClick()
    {
        StartCoroutine(RemoveRelation());
    }

    /// <summary>
    /// Raises the add item button click event.
    /// </summary>
    private void OnAddItemButtonClick()
    {
        MoBackRow t = new MoBackRow(ChildRelationTableName);
        t.SetValue(ItemTypeColumn, childInput.text);
        items.Add(t);
        Display(childInput.text + " has been added to child array");
        childInput.text = string.Empty;
        if (items.Count >= 1)
        {
            createRelationButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Raises the get relation pointer click event.
    /// </summary>
    private void OnGetRelationPointerClick()
    {
        StartCoroutine(GetRelationPointer());
    }

    /// <summary>
    /// Raises the create relation button click event.
    /// </summary>
    private void OnCreateRelationButtonClick()
    {
        StartCoroutine(CreateRelation());
    }

    /// <summary>
    /// Gets the relation pointer.
    /// </summary>
    /// <returns>The relation pointer.</returns>
    private IEnumerator GetRelationPointer()
    {
        MoBackRequest<List<MoBackPointer>> getRelationPointers = playerRow.GetAllPointerObjects(DemoRelationColumn);
        yield return getRelationPointers.Run();
        
        if (getRelationPointers.State != MoBackRequest.RequestState.COMPLETED)
        {
            Debug.LogError("Can't get relation pointers. " + getRelationPointers.errorDetails.Message);
            yield break;
        }    
        string data = "Object ID:";
        foreach (var itemdata in getRelationPointers.ResponseValue) 
        {
            Debug.Log("Object ID: " + itemdata.objectID);
            data += ", " + itemdata.objectID;
        }
        Display(data);
    }

    /// <summary>
    /// Removes the relation.
    /// </summary>
    /// <returns>The relation.</returns>
    private IEnumerator RemoveRelation()
    {
        MoBackRequest removeRelations = playerRow.RemoveRelations(DemoRelationColumn, items.ToArray());
        yield return removeRelations.Run();
        if (removeRelations.State != MoBackRequest.RequestState.COMPLETED)
        {
            Debug.LogError("Can't remove relation objects. " + removeRelations.errorDetails.Message);
        }
        else
        {
            Display("Relation removed");
        }
    }

    /// <summary>
    /// Gets the relation.
    /// </summary>
    /// <returns>The relation.</returns>
    private IEnumerator GetRelation()
    {
      MoBackRequest<List<MoBackRow>> getRelationObjects = playerRow.GetAllRelationObjects(DemoRelationColumn);
      yield return getRelationObjects.Run();
      if (getRelationObjects.State != MoBackRequest.RequestState.COMPLETED)
      {
           Debug.LogError("Can't get relation objects. " + getRelationObjects.errorDetails.Message);
           yield break;
      }   
        string data = "My relations: ";
        Debug.Log("My relations: ");
        foreach (var item in getRelationObjects.ResponseValue) 
        {
                    Debug.Log(string.Format("Item: {0}", item.GetValue(ItemTypeColumn)));
                   data += ", " + item.GetValue(ItemTypeColumn);
        }
           Display(data);
    }

    /// <summary>
    /// Creates the relation.
    /// </summary>
    /// <returns>The relation.</returns>
    private IEnumerator CreateRelation()
    {
        MoBackTableInterface childTable = new MoBackTableInterface(ChildRelationTableName);
        
        MoBackRequest saveOjbectsRequest = childTable.RequestB.SaveMultipleObjects(items.ToList());
        
        yield return saveOjbectsRequest.Run();
        
        MoBackRequest createRelationRequest = playerRow.AddRelations(DemoRelationColumn, items.ToArray());
        yield return createRelationRequest.Run();
        
        if (createRelationRequest.State != MoBackRequest.RequestState.COMPLETED)
        {
            Debug.LogError("Can't create relation. " + createRelationRequest.errorDetails.Message);
            yield break;
        }
        else
        {
            Display("Relation Created");
            ChangeScene(relationScene);
        }
    }

    /// <summary>
    /// Saves the name of the player.
    /// </summary>
    /// <returns>The player name.</returns>
   private IEnumerator SavePlayerName()
    {
        playerRow.SetValue("PlayerName", parentInput.text);
        
        MoBackRequest savePlayerInfoRequest = playerRow.Save();
        yield return savePlayerInfoRequest.Run();
        
        if (savePlayerInfoRequest.State != MoBackRequest.RequestState.COMPLETED)
        {
            Debug.LogError("Can't save player info " + savePlayerInfoRequest.errorDetails.Message);
            yield break;
        }
        else
        {
            ChangeScene(inputItemScene);
            Display(parentInput.text + " saved to " + DemoRelationTableName + "Table");
        }
    }

    /// <summary>
    /// Changes the scene.
    /// </summary>
    /// <param name="obj">Object.</param>
    private void ChangeScene(GameObject obj)
    {
        inputNameScene.SetActive(false);
        inputItemScene.SetActive(false);
        relationScene.SetActive(false);
        obj.SetActive(true);
    }
}
