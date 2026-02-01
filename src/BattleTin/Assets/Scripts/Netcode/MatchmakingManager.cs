using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode
{
    public class MatchmakingManager : MonoBehaviour
    {
        const string RELAY_CODE_KEY = "relayCode";
        const string GAME_STARTED_KEY = "started";

        public static MatchmakingManager Instance { get; private set; }

        [SerializeField] private UnityTransport transport;
        [SerializeField] private NetworkManager networkManager;

        private Lobby currentLobby;
        private bool isSearching;
        private CancellationTokenSource matchmakingCts;

        private bool isHost;

        public int LocalTeamId => networkManager.IsHost ? 0 : 1;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServicesAsync().Forget();
        }

        private async UniTask InitializeServicesAsync()
        {
            Debug.Log("Initializing Unity Services");
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Signing in anonymously");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Unity Services Initialized");
        }

        // === PUBLIC API ===

        public void QuickMatch()
        {
            if (isSearching)
            {
                Debug.Log("Already searching for a match");
                return;
            }

            Debug.Log("Starting Quick Match");
            matchmakingCts = new CancellationTokenSource();
            QuickMatchAsync(matchmakingCts.Token).Forget();
        }

        private async UniTask QuickMatchAsync(CancellationToken token)
        {
            isSearching = true;
            Debug.Log("Searching for available lobbies");

            // === QUERY LOBBIES ===
            var queryOptions = new QueryLobbiesOptions
            {
                Count = 1,
                Filters = new List<QueryFilter>
                {
                    new(
                        QueryFilter.FieldOptions.AvailableSlots,
                        "0",
                        QueryFilter.OpOptions.GT
                    )
                }
            };

            var result = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            token.ThrowIfCancellationRequested();

            if (result.Results.Count > 0)
            {
                // === JOIN ===
                Debug.Log($"Joining lobby: {result.Results[0].Id}");
                currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(result.Results[0].Id);
                isHost = false;

                await ClientWaitForRelayAsync(token);
            }
            else
            {
                // === CREATE ===
                Debug.Log("No available lobbies, creating a new lobby");
                var options = new CreateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { RELAY_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Member, "") },
                        { GAME_STARTED_KEY, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                    }
                };

                currentLobby = await LobbyService.Instance.CreateLobbyAsync("QuickMatch", 2, options);
                isHost = true;

                await HostWaitForClientAsync(token);
            }
        }

        private async UniTask HostWaitForClientAsync(CancellationToken token)
        {
            Debug.Log("Waiting for client to join");
            while (currentLobby.Players.Count < 2)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Delay(1000, cancellationToken: token);
                currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            }

            // === RELAY ===
            Debug.Log("Creating Relay allocation");
            var allocation = await RelayService.Instance.CreateAllocationAsync(1);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            var updateOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { RELAY_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                    { GAME_STARTED_KEY, new DataObject(DataObject.VisibilityOptions.Member, "1") }
                }
            };

            Debug.Log("Updating lobby with join code");
            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateOptions);

            networkManager.StartHost();
            LoadBattleScene();
        }

        private async UniTask ClientWaitForRelayAsync(CancellationToken token)
        {
            Debug.Log("Client waiting for relay to be started");
            while (true)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Delay(1000, cancellationToken: token);

                currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

                if (currentLobby.Data[GAME_STARTED_KEY].Value == "1")
                    break;
            }

            var joinCode = currentLobby.Data[RELAY_CODE_KEY].Value;
            Debug.Log($"Joining allocation with join code: {joinCode}");

            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            networkManager.StartClient();
        }

        private void LoadBattleScene()
        {
            Debug.Log("Loading battle scene");
            networkManager.SceneManager.LoadScene(
                "Game",
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }

        public async UniTask LeaveMatchAsync()
        {
            Debug.Log("Leaving match");
            matchmakingCts?.Cancel();

            if (networkManager.IsListening)
                networkManager.Shutdown();

            if (currentLobby != null)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(
                        currentLobby.Id,
                        AuthenticationService.Instance.PlayerId
                    );
                    Debug.Log("Player removed from lobby");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while removing player from lobby: {e}");
                }

                currentLobby = null;
            }

            isSearching = false;
        }

        public async UniTask<bool> CheckConnection()
        {
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("Checking sign-in status - signing in anonymously");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                Debug.Log("Connection check successful");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Connection check failed: {e}");
                return false;
            }
        }

        public async UniTask CancelMatchmakingAsync()
        {
            if (!isSearching)
            {
                Debug.Log("Matchmaking was not in progress");
                return;
            }

            Debug.Log("Cancelling matchmaking");
            matchmakingCts?.Cancel();
            matchmakingCts?.Dispose();
            matchmakingCts = null;

            isSearching = false;

            if (currentLobby != null)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(
                        currentLobby.Id,
                        AuthenticationService.Instance.PlayerId
                    );
                    Debug.Log("Player removed from lobby after cancelling matchmaking");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while removing player from lobby: {e}");
                }

                currentLobby = null;
            }
        }
    }
}
