using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Move Parameters")]
    [SerializeField] float speed;
    
    [Header("Jump Parameters")]
    [SerializeField] float jumpTime;
    [SerializeField] int jumpForce;
    [SerializeField] float jumpMultiplier;
    [SerializeField] float fallMultiplier;
    
    [Header("Dash Parameters")]
    [SerializeField] float dashTime;
    [SerializeField] int dashForce;
    [SerializeField] float dashFallMultiplier;
    [SerializeField] private GameObject dashChargeParticle;
    [SerializeField] private GameObject dashDirArrow;
    [SerializeField] private float trailDashValue;
    
    [Header("GroundCheck Parameters")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    [Header("Graphics")]
    [SerializeField] private Transform graphics;
    
    private bool _isJumping;
    private float _jumpCounter;
    private bool _isDashCharging;
    private float _dashCounter;
    private bool _hasDashed;
    private bool _isDashing;
    private bool _jumpButtonState = false;//false == released, true == pushed
    private bool _dashButtonState = false;//false == released, true == pushed
    private TrailRenderer _trailRenderer;
    private float _trailNormalValue;
    
    private Rigidbody2D _rb;
    private Vector2 _move;
    private Vector2 _baseGravity;
    private Vector2 _vecGravity;

    public delegate void PlayerDel();
    public PlayerDel CharaDeath;
        

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _vecGravity = new Vector2(0, -Physics2D.gravity.y);
        _jumpCounter = 0;
        _baseGravity = Physics2D.gravity;
        _trailRenderer = transform.GetComponentInChildren<TrailRenderer>();
        _trailNormalValue = _trailRenderer.time;
        
        CharaDeath += Death;
    }

    // Update is called once per frame
    void Update()
    {
        if(_rb.velocity.y <= 0 && IsGrounded())
            _hasDashed = false;
        if (_rb.velocity.y <= 0)
        {
            _isDashing = false;
            if(_trailRenderer.time > _trailNormalValue)
            {
                _trailRenderer.time -=  Time.deltaTime;
            }
             
        }
        
        //Dash
        if(_isDashCharging && !_hasDashed)
        {
            dashChargeParticle.SetActive(true);
            dashDirArrow.SetActive(true);

            float angle = -Mathf.Atan2(_move.x, _move.y)* Mathf.Rad2Deg;
            
            dashDirArrow.transform.rotation = Quaternion.Euler(0,0, angle);
            
            _rb.velocity = _vecGravity * dashFallMultiplier * Time.deltaTime;
            
            _dashCounter += Time.deltaTime;
            if (_dashCounter > dashTime) _dashCounter = dashTime;
            
            float t = (_dashCounter / dashTime) *2;
            dashDirArrow.transform.localScale = new Vector3(1 + t, 1 + t, 1 + t);
        }
        else
        {
            if (!_isDashCharging && _dashCounter > 0)
            {
                _trailRenderer.time = trailDashValue;
                _isDashing = true;
                float t = _dashCounter / dashTime;
                Physics2D.gravity = _baseGravity;

                Vector2 dashDir = Vector2.up;
                if(_move.normalized != Vector2.zero)
                    dashDir = _move.normalized;
                
                _rb.velocity = dashDir * dashForce * t;
                _hasDashed = true;
                _dashCounter = 0;
            }
            else
            {
                //Jump
                if (_rb.velocity.y > 0 && _isJumping)
                {
                    _jumpCounter += Time.deltaTime;
                    if (_jumpCounter > jumpTime) _isJumping = false;

                    float t = _jumpCounter / jumpTime;
                    float currentJumpM = jumpMultiplier;

                    if (t > .5f)
                        currentJumpM = jumpMultiplier * (1 - t);

                    _rb.velocity += _vecGravity * currentJumpM * Time.deltaTime;
                }

                if (_rb.velocity.y < 0)
                {
                    _rb.velocity -= _vecGravity * fallMultiplier * Time.deltaTime;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if(_isDashCharging)
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            return;
        }
        if(_isDashing) return;
        //Movement
        _rb.velocity = new Vector2(_move.x * speed, _rb.velocity.y);
        if (_rb.velocity.x > 0.01) graphics.localScale = new Vector3(1,1,1);
        if (_rb.velocity.x < -0.01) graphics.localScale = new Vector3(-1,1,1);
    }

    void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }

    void OnJump()
    {
        _jumpButtonState = !_jumpButtonState;
        if (_jumpButtonState)
        {
            if(IsGrounded())
            {
                _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
                _isJumping = true;
                _jumpCounter = 0;
            }
        }
        else if(!_jumpButtonState)
        {
            _isJumping = false;

            _jumpCounter = 0;
            
            if (_rb.velocity.y>0)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * .6f);
            }
        }
    }
    
    void OnDash()
    {
        _dashButtonState= !_dashButtonState;
        if(_dashButtonState)
        {
            _isDashCharging = true;
            _dashCounter = 0;
        }
        else if(!_dashButtonState)
        {
            _isDashCharging = false;
            dashChargeParticle.SetActive(false);
            dashDirArrow.SetActive(false);
        }
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(1f, 0.15f), CapsuleDirection2D.Horizontal,0,groundLayer);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("DeathZone"))
        {
            CharaDeath();
        }
    }

    private void Death()
    {
        Debug.Log("Ah il est oh sol!");
    }
}
