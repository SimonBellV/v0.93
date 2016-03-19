//-----------------------------------------------------------------------
// <copyright file="DemoMoBackFile.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.IO;
using MoBack;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles all the requests and responses made to and from the server.
/// </summary>
using System;


public class DemoMoBackFile : MonoBehaviour 
{

    /// <summary>
    /// The name of the table of the server.
    /// </summary>
    private const string DemoTableName = "MoBackFileTable";

    /// <summary>
	/// The MobackFile object .
	/// </summary>
    private MoBackFile demoMoBackFile;

	/// <summary>
	/// MobackFile data.
	/// </summary>
	private byte[] data;

	private WWW reader;

    /// <summary>
	/// Object of MobackUser class 
	/// </summary>
    private  MoBackUser demoUser;

	/// <summary>
	/// MobackUser credentials.
	/// </summary>
    const string demoMobackUser = "demoMoBackUser";
    const string demoMoBackPassword = "demoMoBackPassword";
    const string demoMoBackEmail = "demo@moback.com";
    
    const string demoFileName = "image.png";
    
    const string requiredLoginMessage = "Need to login to use MoBack File Manager";
    
    public Button uploadFileButton, downloadFileButton, deleteFileButton;
    
    public Text displayText;

    /// <summary>
    /// Start this instance.
    /// </summary>
    public void Start()
    {
        // In order to upload file, we need a session token which can be obtain by using MoBackUser to log in.
        // For FileManager demo, we will auto generate a user and login.
        StartCoroutine(LoginUser());

#if UNITY_ANDROID
		// Get target file from android streaming assests folder.
		try{
			StartCoroutine (AndroidFilePathAccessor (demoFileName));
		}
		catch(Exception ex)
		{ 
			Debug.Log("Android Exception :" + ex.Message); 
		}
#endif

        // Assign button's events.
        uploadFileButton.onClick.AddListener(OnUploadFileButtonClick);
        downloadFileButton.onClick.AddListener(OnDownloadFileButtonClick);
        deleteFileButton.onClick.AddListener(OnDeleteFileButtonClick);
    }
    
    /// <summary>
    /// Raises the upload file button click event.
    /// </summary>
    private void OnUploadFileButtonClick()
    {
		// Check to see if user is logged in.
        if (!demoUser.IsLoggedIn)
        {
            Debug.LogError(requiredLoginMessage);
            return;
        }

#if UNITY_EDITOR
		try 
		{
			// Explorer window prompt to select image.
			var path = EditorUtility.OpenFilePanel("Load Image File", "", "");
			byte[] data = System.IO.File.ReadAllBytes(path);
			
			// Create a Moback File Object.
            demoMoBackFile = new MoBackFile(DemoTableName, data, demoFileName);
		}
		catch (Exception e)
		{
			// If no file selected, return.
			DisplayMessage("No File Selected.");
			Debug.LogError(e.Message);
			return;
		}
#else
        demoMoBackFile = CreateMoBackFileObject(demoFileName);
#endif

		StartCoroutine(UploadFile(demoMoBackFile));
    }
	
    /// <summary>
    /// Raises the download file button click event.
    /// </summary>
    private void OnDownloadFileButtonClick()
    {
        if (!demoUser.IsLoggedIn)
        {
            Debug.LogError(requiredLoginMessage);
            return;
        }

        StartCoroutine(DownloadFile(demoMoBackFile));
    }
    
    /// <summary>
    /// Raises the delete file button click event.
    /// </summary>
    private void OnDeleteFileButtonClick()
    {
        if (!demoUser.IsLoggedIn)
        {
            Debug.LogError(requiredLoginMessage);
            return;
        }
        
        StartCoroutine(DeleteFile(demoMoBackFile));
    }
    
    
    /// <summary>
    /// Logins the user.
    /// </summary>
    /// <returns>The user.</returns>
    /// <param name="name">Name.</param>
    /// <param name="password">Password.</param>
    public IEnumerator LoginUser()
    {
        // Disable all buttons while signing up and logging in.
        uploadFileButton.enabled = false;
        downloadFileButton.enabled = false;
        deleteFileButton.enabled = false;
        
        // Create a new user.
        demoUser = new MoBackUser(demoMobackUser, demoMoBackPassword, demoMoBackEmail);
        
        // Sign up for the demo user.
        MoBackRequest signUpRequest = demoUser.SignUp();
        yield return signUpRequest.Run();
        
        MoBackRequest currentLoginRequest = demoUser.Login();
        yield return currentLoginRequest.Run();
        
        if (currentLoginRequest.State == MoBackRequest.RequestState.COMPLETED)
        {
            if (demoUser.IsLoggedIn)
            {
                // enable all button when logging complete.
                uploadFileButton.enabled = true;
                downloadFileButton.enabled = true;
                deleteFileButton.enabled = true;
                Debug.Log("Login Complete");
            }
            else
            {
                Debug.Log("Can't log in");
            }
            
            DisplayMessage(currentLoginRequest.message);
        }
        else if (currentLoginRequest.State == MoBackRequest.RequestState.ERROR)
        {
            DisplayMessage(currentLoginRequest.errorDetails.Message);
        }
    }
    
    /// <summary>
    /// Uploads the file.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    /// <param name="name">Name of the file to upload from StreamingAssets folder.</param>
    public IEnumerator UploadFile(MoBackFile mobackFile)
    {
        if (mobackFile == null)
        {
            Debug.LogError("moback file is null");
            yield break;
        }
        
		// Prevent multiple click while uploading.
		uploadFileButton.enabled = false;
        
		// Run the request.
		MoBackRequest uploadFileRequest = mobackFile.UploadFile();
        yield return uploadFileRequest.Run();

        if (uploadFileRequest.State == MoBackRequest.RequestState.COMPLETED)
        {
            // We need to store the file's url (which is received after complete uploading the file) to a row, otherwise, we can't access it.
			MoBackRequest saveFileToTableRequest = mobackFile.Save();
            yield return saveFileToTableRequest.Run();
            
            if (saveFileToTableRequest.State == MoBackRequest.RequestState.COMPLETED)
            {
                DisplayMessage("Finish save file to table");
                
            }
            else if (saveFileToTableRequest.State == MoBackRequest.RequestState.ERROR)
            {
                Debug.LogError("Can't create a new MoBack object on server");
            }
        }
        else if (uploadFileRequest.State == MoBackRequest.RequestState.ERROR)
        {
            Debug.LogError("can't upload file");
        }
        
        uploadFileButton.enabled = true;
    }
    
    /// <summary>
    /// Downloads the file from Moback server.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    /// <param name="url">URL of the server to download the file.</param>
    public IEnumerator DownloadFile(MoBackFile mobackFile)
    {        
        if (mobackFile == null)
        {
            Debug.LogError("moback file is null");
            yield break;
        }
        
        if (string.IsNullOrEmpty(mobackFile.url))
        {
            DisplayMessage("The url is invalid, or the file is already deleted");
            yield break;
        }
        
        downloadFileButton.enabled = false;
        
        yield return mobackFile.Download();
        
        if (mobackFile.Status == MoBackRequest.RequestState.ERROR)
        {
            Debug.LogError("can't download");
            yield break;
        }
        
        // For this demo we will write a downloaded file to persistentDataPath.
        string writePath = Path.Combine(Application.persistentDataPath, mobackFile.Name);
        File.WriteAllBytes(writePath, mobackFile.Data);
        DisplayMessage(string.Format("File Name: {0}. \n Write to path: {1}", mobackFile.Name, writePath));
        
        downloadFileButton.enabled = true;
    }
    
    /// <summary>
    /// Deletes the file from the server.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    /// <param name="name">Name.</param>
    public IEnumerator DeleteFile(MoBackFile moBackFile)
    {
        if (moBackFile == null)
        {
            Debug.LogError("moback file is null");
            yield break;
        }
        
        deleteFileButton.enabled = false;
        
        MoBackRequest deleteFileRequest = moBackFile.DeleteFile();
        yield return deleteFileRequest.Run();
        
        if (deleteFileRequest.State == MoBackRequest.RequestState.COMPLETED)
        {
            // Since we stored the file's url on a row in a table. When delete the file, we need to remove the row as well.
            DisplayMessage("Delete File Success");
            MoBackRequest deleteMoBackObject = moBackFile.DeleteMoBackObject();
            yield return deleteMoBackObject.Run();
            
            if (deleteMoBackObject.State == MoBackRequest.RequestState.COMPLETED)
            {
                DisplayMessage("Delete object");
            }
            else if (deleteMoBackObject.State == MoBackRequest.RequestState.ERROR)
            {
				deleteFileButton.enabled = true;
                DisplayMessage("Unable to delete the MoBack row associated with the file: " + moBackFile.Name);
            }
        }
        else if (deleteFileRequest.State == MoBackRequest.RequestState.ERROR)
        {
            DisplayMessage("Unable to delete the file: " + moBackFile.Name); 
        }
        
        deleteFileButton.enabled = true;
    }

	/// <summary>
	/// Creates a MobackFile with a given file name.
	/// </summary>
	/// <returns>The mo back file object.</returns>
	/// <param name="localFileName">Local file name.</param>
	private MoBackFile CreateMoBackFileObject(string localFileName)
	{
		// Set path for Android platform.
		if (Application.platform == RuntimePlatform.Android)
		{
            if (data != null)
            {
			    return new MoBackFile(DemoTableName, data, localFileName);
            }
            else
            {
                Debug.LogError("bytes array is null. Probably can't read demo file from Jar file");
                return null;
            }
		}
		else
		{
			string path = System.IO.Path.Combine(Application.streamingAssetsPath, localFileName);
			data = System.IO.File.ReadAllBytes(path);
			return new MoBackFile(DemoTableName, data, localFileName);
		}
	}

	/// <summary>
	/// Accessor or android files.
	/// </summary>
	/// <returns>Data to read bytes.</returns>
	/// <param name="localFileName">Local file name.</param>
	private IEnumerator AndroidFilePathAccessor(string localFileName)
	{
		string fromPath = System.IO.Path.Combine(Application.streamingAssetsPath, localFileName);
        		
		WWW read = new WWW(fromPath);
		yield return read;
        
		if (read.isDone) 
		{
			data = read.bytes;
		}
	}

	/// <summary>
	/// Displays the message.
	/// </summary>
	/// <param name="msg">Message.</param>
	private void DisplayMessage(string msg)
	{
		displayText.text = msg;
	}
}
