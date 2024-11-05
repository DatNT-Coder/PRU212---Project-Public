using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
	[SerializeField] private ClientSingleton clientPrefab;
	[SerializeField] private HostSingleton hostPrefab;
	[SerializeField] private ServerSingleton serverPrefab;
	NetworkObject playerPrefab;
	[SerializeField] private List<NetworkObject> playerPrefabs;
	private NetworkVariable<int> playerCharacterIndex = new NetworkVariable<int>();
	private ApplicationData appData;
	private const string GameSceneName = "Game";
	private async void Start()
	{
		DontDestroyOnLoad(gameObject);

		// Retrieve selected character index from PlayerPrefs
		int selectedCharacter = PlayerPrefs.GetInt("selectedCharacter");
		playerCharacterIndex.Value = selectedCharacter;

		await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
	}

	private async Task LaunchInMode(bool isDedicatedServer)
	{
		if (isDedicatedServer)
		{
			Application.targetFrameRate = 60;
			appData = new ApplicationData();

			ServerSingleton serverSingleton = Instantiate(serverPrefab);

			StartCoroutine(LoadGameSceneAsync(serverSingleton));

		}
		else
		{
			playerPrefab = playerPrefabs[playerCharacterIndex.Value];

			HostSingleton hostSingleton = Instantiate(hostPrefab);
			hostSingleton.CreateHost(playerPrefab);

			ClientSingleton clientSingleton = Instantiate(clientPrefab);
			bool authenticated = await clientSingleton.CreateClient();

			if (authenticated)
			{
				clientSingleton.GameManager.GoToMenu();
			}
		}
	}

	private IEnumerator LoadGameSceneAsync(ServerSingleton serverSingleton)
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GameSceneName);

		while (!asyncOperation.isDone)
		{
			yield return null;
		}

		playerPrefab = playerPrefabs[playerCharacterIndex.Value];

		Task createServerTask = serverSingleton.CreateServer(playerPrefab);
		yield return new WaitUntil(() => createServerTask.IsCompleted);

		Task startServerTask = serverSingleton.GameManager.StartGameServerAsync();
		yield return new WaitUntil(() => startServerTask.IsCompleted);
	}

}