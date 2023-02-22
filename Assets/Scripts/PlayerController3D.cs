using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3D : MonoBehaviour
{
     #region Variables

    [Header("Movements Settings")]
    [SerializeField] private float _runningSpeed = 7.5f;
    [SerializeField] private float gravity = 20.0f;

    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float lookSpeed = 1.5f;
    [SerializeField] private float lookXLimit = 85f;
    
    [Header("Head Bobbing")]
    [SerializeField]private Transform _headTransform;
    [SerializeField] float walkingBobbingSpeed = 14f;
    [SerializeField]float bobbingAmount = 0.05f;

    [Header("Booleans")] 
    [SerializeField] private bool _lockCursorOnStart;
    
    //Hidden
    [HideInInspector]public bool canMove = true;
    [HideInInspector]public Vector3 moveDirection = Vector3.zero;

    //Privates
    private float _defaultPosY = 0;
    private float _headBobbingTimer = 0;
    private float _curSpeedX;
    private float _curSpeedY;
    private float _movementDirectionY;
    private float _defaultHeight;

    //Components
    private CharacterController _characterController;
    private float _rotationX = 0;
    
    //Statics
    public static bool _playerIsLocked;

    #endregion
    
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _defaultPosY = _headTransform.localPosition.y;
        _defaultHeight = transform.localScale.y + .001f;

        if(_lockCursorOnStart){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void Update()
    {
        if (!_playerIsLocked)
        {
            Movements();
            CameraMovements();
        }
    }

    void OnMove(InputValue value)
    {
        _vertical = value.Get<Vector2>().y;
        _horizontal = value.Get<Vector2>().x;
    }


    private float _horizontal;
    private float _vertical;
    #region Basic Movements Methods

    void Movements()
    {
        //Player movements
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        if (canMove)
        {
            _curSpeedX = _runningSpeed * _vertical;
            _curSpeedY = _runningSpeed * _horizontal;
        }
        else
        {
            _curSpeedX = 0;
            _curSpeedY = 0;
        }
        _movementDirectionY = moveDirection.y;
        moveDirection = (forward * _curSpeedX) + (right * _curSpeedY);

        moveDirection.y = _movementDirectionY;
        if (!_characterController.isGrounded) { moveDirection.y -= gravity * Time.deltaTime; }
        
        //Move the character 
        _characterController.Move(moveDirection * Time.deltaTime);
        HeadBobbing();
    }

    void HeadBobbing()
    {
        //Head bobbing
        if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {

            _headBobbingTimer += Time.deltaTime * walkingBobbingSpeed * 1.4f;
            _headTransform.localPosition = new Vector3(_headTransform.localPosition.x, _defaultPosY + Mathf.Sin(_headBobbingTimer) * bobbingAmount, _headTransform.localPosition.z);
        }
        else
        {
            _headBobbingTimer = 0;
            _headTransform.localPosition = new Vector3(_headTransform.localPosition.x, Mathf.Lerp(_headTransform.localPosition.y, _defaultPosY, Time.deltaTime * walkingBobbingSpeed), _headTransform.localPosition.z);
        }
    }

    void CameraMovements()
    {
        if (canMove)
        {
            _rotationX += -Input.GetAxisRaw("Mouse Y") * lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    #endregion Methods
    
}
