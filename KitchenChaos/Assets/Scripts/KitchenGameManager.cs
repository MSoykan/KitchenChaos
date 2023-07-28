using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour {



    public static KitchenGameManager Instance { get; private set; }


    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnPaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnPaused;

    private enum State {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool localPlayerReady;
    //private float waitingToStartTimer = 1f;
    private NetworkVariable<float> countDownToStartTimer = new NetworkVariable<float>(1f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(300f);
    private float gamePlayingTimerMax = 300f;
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private bool autoTestGamePauseState;

    private void Awake() {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
    }
    private void Start() {
        GameInput.instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.instance.OnInteractAction += GameInput_OnInteractAction;

        //DEBUG TRIGGER GAME START AUTOMATICALLY
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if(IsServer){
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        autoTestGamePauseState = true;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue) {
        if (isGamePaused.Value) {
            Time.timeScale = 0f;

            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else {
            Time.timeScale = 1f;

            OnMultiplayerGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue) {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e) {
        if (state.Value == State.WaitingToStart) {
            localPlayerReady = true;

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default) {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;


        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) {
                //This player is not ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady) {
            state.Value = State.CountdownToStart;
        }

        Debug.Log("AllClientsReady: " + allClientsReady);
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e) {
        TogglePauseGame();
    }




    private void Update() {
        if (!IsServer) {
            return;
        }
        switch (state.Value) {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countDownToStartTimer.Value -= Time.deltaTime;
                if (countDownToStartTimer.Value < 0f) {
                    state.Value = State.GamePlaying;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f) {
                    state.Value = State.GameOver;
                }
                break;
            case State.GameOver:

                break;
        }
    }

    private void LateUpdate() {
        if (autoTestGamePauseState) {
            autoTestGamePauseState = false;
            TestGamePausedState();
        }
    }

    public bool IsGamePlaying() {
        return state.Value == State.GamePlaying;
    }

    public bool IsCountDownToStartActive() {
        return state.Value == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer() {
        return countDownToStartTimer.Value;
    }

    public bool IsGameOver() {
        return state.Value == State.GameOver;
    }

    public bool IsLocalPlayerReady() {
        return localPlayerReady;
    }

    public float GetGamePlayingTimerNormalized() {
        return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
    }

    public void TogglePauseGame() {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused) {
            PauseGameServerRpc();

            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else {
            UnPauseGameServerRpc();

            OnLocalGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default) {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnPauseGameServerRpc(ServerRpcParams serverRpcParams = default) {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
        
        TestGamePausedState();
    }

    private void TestGamePausedState() {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId]) {
                //This player is paused
                isGamePaused.Value = true;
                return;
            }
        }

        // All players are unpaused 
        isGamePaused.Value = false;
    }
}
