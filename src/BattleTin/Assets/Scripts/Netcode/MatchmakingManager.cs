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
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // === PUBLIC API ===

        public void QuickMatch()
        {
            if (isSearching)
                return;

            matchmakingCts = new CancellationTokenSource();
            QuickMatchAsync(matchmakingCts.Token).Forget();
        }

        private async UniTask QuickMatchAsync(CancellationToken token)
        {
            isSearching = true;

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
                currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(result.Results[0].Id);
                isHost = false;

                await ClientWaitForRelayAsync(token);
            }
            else
            {
                // === CREATE ===
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
            while (currentLobby.Players.Count < 2)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Delay(1000, cancellationToken: token);
                currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            }

            // === RELAY ===
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

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateOptions);

            networkManager.StartHost();
            LoadBattleScene();
        }

        private async UniTask ClientWaitForRelayAsync(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Delay(1000, cancellationToken: token);

                currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

                if (currentLobby.Data[GAME_STARTED_KEY].Value == "1")
                    break;
            }

            var joinCode = currentLobby.Data[RELAY_CODE_KEY].Value;

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
            networkManager.SceneManager.LoadScene(
                "Game",
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }

        public async UniTask LeaveMatchAsync()
        {
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
                }
                catch { }

                currentLobby = null;
            }

            isSearching = false;
        }

        public async UniTask<bool> CheckConnection()
        {
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

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
                return;

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
                }
                catch { }

                currentLobby = null;
            }
        }
    }
}
