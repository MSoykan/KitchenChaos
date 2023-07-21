using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent {

    public static event EventHandler OnAnyPlayerSpawned;

    public static void ResetStaticData() {
        OnAnyPlayerSpawned = null;
    }
    public static Player LocalInstance { get; private set; }

    public event EventHandler OnTakeSomething;
    public event EventHandler<OnSelectedCounterChangeEventArgs> OnSelectedCounterChange;

    public class OnSelectedCounterChangeEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;


    private bool isWalking;
    private Vector3 lastInteractDir;
    private Vector3 lastMoveDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private void Awake() {
        //if (Instance != null) {
        //    Debug.LogError("There is more than one player instance");
        //}
        LocalInstance = this;
        gameInput = GameInput.instance;
    }

    private void Start() {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }


    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e) {
        if (!KitchenGameManager.Instance.IsGamePlaying()) { return; }
        if (selectedCounter != null) {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
        if (!KitchenGameManager.Instance.IsGamePlaying()) { return; }
        if (selectedCounter != null) {
            selectedCounter.Interact(this);
        }
    }

    private void Update() {
        if (!IsOwner) {
            return;
        }
        HandleMovementServerAuth();
        //HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking() {
        return isWalking;
    }

    private void HandleInteractions() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero) {
            lastInteractDir = moveDir;
        }
        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit rayCastHit, interactDistance, counterLayerMask)) {
            if (rayCastHit.transform.TryGetComponent(out BaseCounter baseCounter)) {
                // Has ClearCounter 
                //clearCounter.Interact();
                if (baseCounter != selectedCounter) {
                    SetSelectedCounter(baseCounter);
                }
            }
            else {
                SetSelectedCounter(null);
            }
        }
        else {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovementServerAuth() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        HandleMovementServerRpc(inputVector);
    }

    [ServerRpc(RequireOwnership = true)]
    private void HandleMovementServerRpc(Vector2 inputVector) {
        Debug.Log("OWNERSHIP ALO");
        if (inputVector == Vector2.zero) return;

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);


        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove) {
            //Cannot move in this direction

            //Attempt only x movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            if (canMove) {
                //Can move only on the X
                moveDir = moveDirX;
            }
            else {
                //Cannot move only on the X

                //Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove) {
                    //Can move only on the Z
                    moveDir = moveDirZ;
                }
                else {

                }
            }
        }

        if (canMove) {
            transform.position += moveDir * Time.deltaTime * moveSpeed;
        }


        isWalking = moveDir != Vector3.zero;
        float rotateSpeed = 10f;
        if (moveDir == Vector3.zero) return;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }


    private void HandleMovement() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        if (inputVector == Vector2.zero) return;

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);


        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove) {
            //Cannot move in this direction

            //Attempt only x movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            if (canMove) {
                //Can move only on the X
                moveDir = moveDirX;
            }
            else {
                //Cannot move only on the X

                //Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove) {
                    //Can move only on the Z
                    moveDir = moveDirZ;
                }
                else {

                }
            }
        }

        if (canMove) {
            transform.position += moveDir * Time.deltaTime * moveSpeed;
        }


        isWalking = moveDir != Vector3.zero;
        float rotateSpeed = 10f;
        if (moveDir == Vector3.zero) return;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);

    }

    private void SetSelectedCounter(BaseCounter selectedCounter) {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChange?.Invoke(this, new OnSelectedCounterChangeEventArgs {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform() {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnTakeSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    public void ClearKitchenObject() {
        kitchenObject = null;
    }

    public bool HasKitchenObject() {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject() {
        return NetworkObject;
    }
}
