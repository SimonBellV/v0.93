//-----------------------------------------------------------------------
// <copyright file="DemoMenuHandler.cs" company="moBack">   
// Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MoBack;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles all the menu items in the demo scene.
/// </summary>
public class DemoMenuHandler : MonoBehaviour
{
    public GameObject scene, sceneTable, sceneGeoPoint, sceneArray;

    public Button backButton;

    public InputField inputScore;

    /// <summary>
    /// The display text field.
    /// </summary>
    public Text displayText;

    /// <summary>
    /// The request manager instance of the RequestManager class.
    /// </summary>
    public DemoRequestManager requestManager;

    #region GeoPoint
    /// <summary>
    /// InputField to get the GeoPoint data from the user for the MoBackGeoPoint test.
    /// </summary>
    public InputField geo1X, geo1Y, geo2X, geo2Y;
    #endregion

    #region ArrayScene
    /// <summary>
    /// InputField to get input from the user for the MoBackArray test.
    /// </summary>
    public InputField arrayInput;
    #endregion

    /// <summary>
    /// The current scene.
    /// </summary>
    private GameObject currentScene;

   /// <summary>
   /// Start this instance. Find and assign all of the gameObjects required.
   /// </summary>
    public void Start()
    {
        currentScene = scene;
        backButton.interactable = false;
    }

    /// <summary>
    /// Buttons events are triggered by this function.
    /// </summary>
    /// <param name="name">Name of the button event.</param>
    public void ButtonEvents(string name)
    {
        switch (name)
        {
        case "TestTable":
            ChangeScene(sceneTable);
            break;
        case "TestGeoPoint":
            ChangeScene(sceneGeoPoint);
            break; 
        case "TestMobackArray":
            ChangeScene(sceneArray);
            break; 
        case "backButton":
            ChangeScene(scene);
            break;
        case "SubmitScore":
            StartCoroutine(requestManager.SubmitScore(int.Parse(inputScore.text)));
            break;
        case "FetchScore":
            StartCoroutine(requestManager.FetchScore());
            break;
        case "DisplayDistance":
            requestManager.DisplayDistance(float.Parse(geo1X.text), float.Parse(geo1Y.text), float.Parse(geo2X.text), float.Parse(geo2Y.text));
            break;
        case "PushToArray":
            requestManager.AddToArray(arrayInput.text);
            break;
        case "ReplaceAt":
            requestManager.ReplaceAt(arrayInput.text);
            break;
        case "Replace":
            requestManager.Replace(arrayInput.text);
            break;
        }
    }
    
    /// <summary>
    /// Handles displaying results.
    /// </summary>
    /// <param name="value">Value of the string to be displayed.</param>
    public void Display(string value)
    {
        this.displayText.text = value;
    }

    /// <summary>
    /// Switches on and off the different gameObjects in the demo scene depending on the current active scene.
    /// </summary>
    /// <param name="obj">A gameObject associated with a scene.</param>
    private void ChangeScene(GameObject obj)
    {
        currentScene.SetActive(false);
        obj.SetActive(true);
        currentScene = obj;

        if (currentScene.name == "Scene: Main") 
        {
            backButton.interactable = false;
            Display(string.Empty);
        } 
        else 
        {
            backButton.interactable = true;
        }
    }
}
