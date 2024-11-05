using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
	[SerializeField] private TMP_InputField nameField;
	[SerializeField] private Button connectButton;
	[SerializeField] private int minNameLength = 1;
	[SerializeField] private int maxNameLength = 12;
	public GameObject[] characters;
	public int selectedCharacter = 0;

	public const string PlayerNameKey = "PlayerName";

	private void Start()
	{
		if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
			return;
		}

		nameField.text = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
		HandleNameChanged();
	}
	public void HandleNameChanged()
	{
		connectButton.interactable =
			nameField.text.Length >= minNameLength &&
			nameField.text.Length <= maxNameLength;
	}

	public void NextCharacter()
	{
		characters[selectedCharacter].SetActive(false);
		selectedCharacter = (selectedCharacter + 1) % characters.Length;
		characters[selectedCharacter].SetActive(true);
	}

	public void PreviousCharacter()
	{
		characters[selectedCharacter].SetActive(false);
		selectedCharacter--;
		if (selectedCharacter < 0)
		{
			selectedCharacter += characters.Length;
		}
		characters[selectedCharacter].SetActive(true);
	}

	public void Connect()
	{
		PlayerPrefs.SetString(PlayerNameKey, nameField.text);
		PlayerPrefs.SetInt("selectedCharacter", selectedCharacter);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}
}
