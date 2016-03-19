//-----------------------------------------------------------------------
// <copyright file="DemoMoBackUser.cs" company="moBack">
// Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoBack;

/// <summary>
/// Class handele all the request and response for MoBack User Demo.
/// </summary>
public class DemoMoBackUser : MonoBehaviour 
{
	/// <summary>
	/// The Text to display message.
	/// </summary>
	public Text messageText;
	
    // <summary>
    // The buttons.
    // </summary>
	public Button loginButton, newUserButton, resetPasswordButton, signUpButton, inviteFriendButton, userInfoButton, signOutButton, deleteUserButton, getUserInfoButton, updateUserInfoButton, sendInviteButton, backButton;

	// <summary>
	// The InputFields.
	// </summary>
	public InputField loginUsernameField, loginPasswordField, signUpEmailField, signUpUsernameField, signUpPasswordField, signUpFirstNameField, signUpLastNameField, signUpAgeField, userInfoFirstNameField, userInfoLastNameField, userInfoAgeField, inviteEmailField;
   
	/// <summary>
	/// The scene prompt gameobjects.
	/// </summary>
	public GameObject loginPrompt, signUpPrompt, signedInPrompt, userInfoPrompt, inviteFriendPrompt;

	/// <summary>
	/// The current scene.
	/// </summary>
	private GameObject currentPrompt;

	/// <summary>
	/// The previous scene.
	/// </summary>
	private GameObject previousPrompt;

    /// <summary>
    /// The age column name.
    /// </summary>
    private string ageColumn = "Age";

    /// <summary>
    /// The first name column name.
    /// </summary>
    private string firstNameColumn = "First Name";

    /// <summary>
    /// The last name column name.
    /// </summary>
    private string lastNameColumn = "Last Name";
    
    /// <summary>
    /// The MobackUser object.
    /// </summary>
    private MoBackUser demoUser;

   /// <summary>
   /// Start this instance.
   /// </summary>
    public void Start()
    {
		ChangeScene (loginPrompt);
        loginButton.onClick.AddListener(OnLoginButtonPress);
		newUserButton.onClick.AddListener (OnNewUserButtonPress);
		signUpButton.onClick.AddListener(OnSignUpButtonPress);
		backButton.onClick.AddListener (OnBackButtonPress);
		resetPasswordButton.onClick.AddListener (OnResetPasswordButtonPress);
		inviteFriendButton.onClick.AddListener (OnInviteFriendButtonPress);
		userInfoButton.onClick.AddListener (OnUserInfoButtonPress);
		signOutButton.onClick.AddListener (OnSignOutButtonPress);
		deleteUserButton.onClick.AddListener (OnDeleteUserButtonPress);
		getUserInfoButton.onClick.AddListener (OnGetUserInfoButtonPress);
		updateUserInfoButton.onClick.AddListener (OnUpdateUserInfoButtonPress);
		sendInviteButton.onClick.AddListener (OnSendInviteButtonPress);
    }

    /// <summary>
    /// Raises the Login button click event.
    /// </summary>
	private void OnLoginButtonPress()
    {
		if( !string.IsNullOrEmpty(loginUsernameField.text) &&  !string.IsNullOrEmpty(loginPasswordField.text))
		{
			demoUser = new MoBackUser(loginUsernameField.text, loginPasswordField.text, null);
        	StartCoroutine(Login());
		}
		else
		{
			Display("Username and Password Fields Cannot Be Left Empty.");
		}
    }

    /// <summary>
    /// Raises the sign up demo button click event.
    /// </summary>
	private void OnSignUpButtonPress()
    {
		if( !string.IsNullOrEmpty(signUpUsernameField.text) && !string.IsNullOrEmpty(signUpPasswordField.text) && !string.IsNullOrEmpty(signUpEmailField.text) && !string.IsNullOrEmpty(signUpFirstNameField.text) && !string.IsNullOrEmpty(signUpLastNameField.text))
		{
			demoUser = new MoBackUser(signUpUsernameField.text, signUpPasswordField.text, signUpEmailField.text);
        	StartCoroutine(SignUp());
		}
		else
		{
			Display("Fields Cannot Be Left Empty");
		}
    }

	/// <summary>
	/// Raises the reset password button press event.
	/// </summary>
	private void OnResetPasswordButtonPress()
	{
		if(!string.IsNullOrEmpty(signUpEmailField.text))
		{
			demoUser = new MoBackUser(signUpUsernameField.text, null, signUpEmailField.text);
			StartCoroutine(ResetPassword());
		}
		else
		{
			Display("Email Field Cannot Be Left Blank");
		}
	}

    /// <summary>
    /// Raises the invite friend demo button press event.
    /// </summary>
    private void OnSendInviteButtonPress()
    {
		if(!string.IsNullOrEmpty(inviteEmailField.text))
		{
        	StartCoroutine(InviteFriend());
		}
		else
		{
			Display("Fields Cannot Be Left Empty");
		}
    }
  
	/// <summary>
	/// Raises the delete user button press event.
	/// </summary>
	private void OnDeleteUserButtonPress()
	{
		StartCoroutine(DeleteUser());
	}
	
	/// <summary>
	/// Raises the get user info demo button press event.
	/// </summary>
	private void OnGetUserInfoButtonPress()
	{
		StartCoroutine(GetUserInfo());
	}
	
	/// <summary>
	/// Raises the update user info demo button press event.
	/// </summary>
	private void OnUpdateUserInfoButtonPress()
	{
		StartCoroutine(UpdateUserInfo());
	}

	/// <summary>
	/// Raises the sign out button click event.
	/// </summary>
	private void OnSignOutButtonPress()
	{
		demoUser = null;
		ChangeScene (loginPrompt);
	}

	/// <summary>
	/// Raises the user info button click event.
	/// </summary>
	private void OnUserInfoButtonPress()
	{
		ChangeScene (userInfoPrompt);
	}

	/// <summary>
	/// Raises the new user button click event.
	/// </summary>
	private void OnNewUserButtonPress()
	{
		ChangeScene (signUpPrompt);
	}

	/// <summary>
	/// Raises the invite friend button click event.
	/// </summary>
	private void OnInviteFriendButtonPress()
	{
		ChangeScene (inviteFriendPrompt);
	}

	/// <summary>
	/// Raises the back button click event.
	/// </summary>
	private void OnBackButtonPress()
	{
		ChangeScene (previousPrompt);
	}

	/// <summary>
	/// Login this instance.
	/// </summary>
	/// <returns>A Unity coroutine IEnumerator.</returns>
	private IEnumerator Login()
	{
		MoBackRequest loginRequest = demoUser.Login();
		yield return loginRequest.Run();
		// If login request can't complete -> exit.
		if (loginRequest.State != MoBackRequest.RequestState.COMPLETED)
		{
			Debug.LogError("Login request error: " + loginRequest.message);
			Display(loginRequest.message);
			yield break;
		}
		else 
		{
			Display("User Login Success");
		}
		
		// If login request is complete, but we can't loggin. Mostly due to invalid credential (wrong password or username) -> exit.
		if (!demoUser.IsLoggedIn)
		{
			Debug.LogError("invalid credential?");
			Display(loginRequest.message);
			yield break;
		}
		else
		{
			ChangeScene(signedInPrompt);
		}
	}

    /// <summary>
    /// Signs up.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    private IEnumerator SignUp()
    {
        // Add custom values if any
        demoUser.SetValue(firstNameColumn, signUpFirstNameField.text);
        demoUser.SetValue(lastNameColumn, signUpLastNameField.text);
		demoUser.SetValue(ageColumn, signUpAgeField.text);

        // Create signup request.
        MoBackRequest signUpNewUserRequest = demoUser.SignUp();
        
        // Send SignUp request.
        yield return signUpNewUserRequest.Run();
        
        // If request can't complete, exit.
        if (signUpNewUserRequest.State != MoBackRequest.RequestState.COMPLETED)
        {
            Debug.LogError("Error: Account creation unsuccessful.");
            Display("Account creation unsuccessful");
            yield break;
        }
        else
        {
			ChangeScene(signedInPrompt);
            Display("User account creation successful");
        }
        
        // If signup request is complete, but we can't sign up. Mostly because there is already an existence account. 
        // We give a error message here.
        if (!demoUser.CompleteSignedUp)
        {
            Debug.LogError("Error: User already exists. New column won't be stored on the server.");
            Display("User already exists - New column won't be stored on the server");
        }
    }

	/// <summary>
	/// Resets the password.
	/// </summary>
	/// <returns>A Unity coroutine IEnumerator.</returns>
	private IEnumerator ResetPassword()
	{
		resetPasswordButton.enabled = false;
		MoBackRequest resetThisUserPasswordRequest = demoUser.ResetPassword();
		yield return resetThisUserPasswordRequest.Run();
		resetPasswordButton.enabled = true;
		if (resetThisUserPasswordRequest.State == MoBackRequest.RequestState.COMPLETED)
		{
			Display("Password request sent");
		}
		else
		{
			Display(resetThisUserPasswordRequest.errorDetails.Message);
		}
	}

	/// <summary>
	/// Invites the friend.
	/// </summary>
	/// <returns>A Unity coroutine IEnumerator.</returns>
	private IEnumerator InviteFriend()
	{
		sendInviteButton.enabled = false;
		MoBackRequest sendInvitationRequest = demoUser.SendInvitation(inviteEmailField.text);
		yield return sendInvitationRequest.Run();
		sendInviteButton.enabled = true;
		if (sendInvitationRequest.State == MoBackRequest.RequestState.COMPLETED)
		{
			Display("Invitation request sent");
		}
		else
		{
			Display(sendInvitationRequest.errorDetails.Message);
		}
	}

	/// <summary>
    /// Deleteuser this instance.
    /// </summary>
   /// <returns>A Unity coroutine IEnumerator.</returns>
    private IEnumerator DeleteUser()
    {
        deleteUserButton.enabled = false;
        MoBackRequest deleteCurrentUser = demoUser.DeleteCurrentUser();
        yield return deleteCurrentUser.Run();
        deleteUserButton.enabled = true;
        if (deleteCurrentUser.State == MoBackRequest.RequestState.COMPLETED)
        {
			ChangeScene(loginPrompt);
            Display("Deleted current user");
        }
        else
        {
            Display(deleteCurrentUser.errorDetails.Message);
        }
    }

	/// <summary>
	/// Gets the user info.
	/// </summary>
	/// <returns>A Unity coroutine IEnumerator.</returns>
	private IEnumerator GetUserInfo()
	{
		// Check if user is signed in.
		if (!demoUser.IsLoggedIn) 
		{
			MoBackRequest loginNewUser = demoUser.Login();
			
			yield return loginNewUser.Run();
			
			if (loginNewUser.State != MoBackRequest.RequestState.COMPLETED)
			{
				Debug.LogError("Error: Login attempt unsuccessful.");
				Display("Login attempt unsuccessful");
				yield break;
			}
		}
		
		getUserInfoButton.enabled = false;
		MoBackRequest getUserInfo = demoUser.GetUserInfo();
		yield return getUserInfo.Run();
		getUserInfoButton.enabled = true;
		if (getUserInfo.State != MoBackRequest.RequestState.COMPLETED)
		{
			Debug.LogError("Error: User info request unsuccessful.");
            Display("User info request unsuccessful");
			Debug.Log(getUserInfo.errorDetails);
			yield break;
		}
		
		// Extract values from server.
		string firstName = demoUser.GetString(firstNameColumn);
		string lastName = demoUser.GetString(lastNameColumn);
		string age = demoUser.GetString(ageColumn);
		
		//Create a debug text.
		string debugString = string.Format("Name: {0} {1} \nAge: {2}", firstName, lastName, age );
		
		Debug.Log(debugString);
		Display(debugString);
		
	}

    /// <summary>
    /// Updates the user info.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    private IEnumerator UpdateUserInfo()
    {
		updateUserInfoButton.enabled = false;

		// Check if user is signed in.
		if (!demoUser.IsLoggedIn) 
		{
			MoBackRequest loginNewUser = demoUser.Login();
			
			yield return loginNewUser.Run();
			
			if (loginNewUser.State != MoBackRequest.RequestState.COMPLETED)
			{
				Debug.LogError("Error: Login attempt unsuccessful.");
                Display("Login attempt unsuccessful");
				yield break;
			}
		}

		// Check is new value is entered in field.
		if(string.IsNullOrEmpty(userInfoFirstNameField.text) && string.IsNullOrEmpty(userInfoLastNameField.text) && string.IsNullOrEmpty(userInfoAgeField.text))
		{
			updateUserInfoButton.enabled = true;

			Display("No New Value Entered.");
			yield break;
		}
		else 
		{
			// Set new value entered in field.
			if(!string.IsNullOrEmpty(userInfoFirstNameField.text)) { demoUser.SetValue(firstNameColumn, userInfoFirstNameField.text);}
			if(!string.IsNullOrEmpty(userInfoLastNameField.text)) { demoUser.SetValue(lastNameColumn, userInfoLastNameField.text);}
			if(!string.IsNullOrEmpty(userInfoAgeField.text)) { demoUser.SetValue(ageColumn, userInfoAgeField.text);}
		}

        // Update to server.
        MoBackRequest updateUserInfoRequest = demoUser.UpdateUserInfo();
        yield return updateUserInfoRequest.Run();
        
        if (updateUserInfoRequest.State != MoBackRequest.RequestState.COMPLETED)
        {
            Debug.LogError("Error: Update user info request unsuccessful.");
            Display("Update user info request unsuccessful");
        }
        else
        {
            Display("User information updated");
        }
        updateUserInfoButton.enabled = true;
    }

	/// <summary>
	/// Switches on and off the different gameObjects in the demo scene depending on the current active scene.
	/// </summary>
	/// <param name="prompt">A gameObject associated with a scene.</param>
	private void ChangeScene(GameObject prompt)
	{
		if(currentPrompt!=null) 
		{ 
			currentPrompt.SetActive(false);
			previousPrompt = currentPrompt;
		}
		prompt.SetActive(true);
		currentPrompt = prompt;
		
		if (currentPrompt.name == "LoginPrompt" || currentPrompt.name == "SignedInPrompt") 
		{
			backButton.interactable = false;
			Display(string.Empty);
		} 
		else 
		{
			backButton.interactable = true;
		}
	}

    /// <summary>
    /// To Display the message on the screen. 
    /// </summary>
    /// <param name="value">Value.</param>
    public void Display(string value)
    {
        messageText.text = value;
    }
}
