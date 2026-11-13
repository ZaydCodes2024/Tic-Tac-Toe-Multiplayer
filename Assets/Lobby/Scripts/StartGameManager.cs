using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameManager : MonoBehaviour {


    private void Awake() {
        DontDestroyOnLoad(gameObject);
        if (FindObjectsByType<StartGameManager>(FindObjectsSortMode.None).Length > 1) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        LobbyManager.Instance.OnLobbyStartGame += LobbyManager_OnLobbyStartGame;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void LobbyManager_OnLobbyStartGame(object sender, LobbyManager.LobbyEventArgs e) {
        // Start Game!
        if (LobbyManager.IsHost) {
            CreateRelay();
        } else {
            JoinRelay(LobbyManager.RelayJoinCode);
        }
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e) {
        // Kicked (or lobby deleted by host) while in the game scene
        if (SceneManager.GetActiveScene().name == LobbyManager.SCENE_GAME) {
            ReturnToMainMenu();
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        if (LobbyManager.IsHost) {
            // The opponent left mid-game; delete the lobby and return to the menu
            if (clientId != NetworkManager.ServerClientId) {
                DeleteJoinedLobby();
                ReturnToMainMenu();
            }
        } else {
            // The host disconnected us
            ReturnToMainMenu();
        }
    }

    private async void DeleteJoinedLobby() {
        var joinedLobby = LobbyManager.Instance.GetJoinedLobby();
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            } catch (Unity.Services.Lobbies.LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    private void ReturnToMainMenu() {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) {
            NetworkManager.Singleton.Shutdown();
        }

        LobbyManager.Instance.ResetLobbyState();
        SceneManager.LoadScene(LobbyManager.SCENE_MAIN_MENU);
    }

    public void StartHost() {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient() {
        NetworkManager.Singleton.StartClient();
    }


    private async void CreateRelay() {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Allocated Relay JoinCode: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

            StartHost();

            LobbyManager.Instance.SetRelayJoinCode(joinCode);
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    private async void JoinRelay(string joinCode) {
        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

            StartClient();
        } catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

}
