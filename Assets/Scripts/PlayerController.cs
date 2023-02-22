using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("--------------------------------------------------------")]
    [Header("Move Parameters")]
    [SerializeField] float speed;
    
    [Header("Jump Parameters")]
    [SerializeField] float jumpTime;
    [SerializeField] int jumpForce;
    [SerializeField] float jumpMultiplier;
    [SerializeField] float fallMultiplier;
    [SerializeField] float coyoteTime;
    [SerializeField] float jumpBufferTime;

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

    [Header("-------------------- Graphics ------------------------")]
    [SerializeField] private Transform graphics;
    [SerializeField] private Transform bodyGraphics;
    [Space]
    [SerializeField] private Vector3 bodySquashScale;
    [SerializeField] private float bodySquashTime;
    [Space]
    [SerializeField] private Vector3 bodyJumpScale;
    [SerializeField] private float bodyJumpTime;

    [SerializeField] private ParticleSystem runParticle;
    [SerializeField] private ParticleSystem landParticle;

    [Header("-------------------- Camera ------------------------")] 
    [SerializeField] private float landCameraShakeIntensity;
    [SerializeField] private float landCameraShakeTime;
    [Space]
    [SerializeField] private float jumpCameraShakeIntensity;
    [SerializeField] private float jumpCameraShakeTime;

    private bool _isJumping;
    private float _jumpCounter;
    private float _coyoteCounter;
    private float _jumpBufferCounter;
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
    
    private Vector3 _originalBodyScale;
    private CameraScript _cameraScript;

    private bool passOne; //utilisé pour ne faire qu'une fois les anims d'attérissages

    public delegate void PlayerDel();
    private PlayerDel _charaDeath;
        

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _vecGravity = new Vector2(0, -Physics2D.gravity.y);
        _jumpCounter = 0;
        _baseGravity = Physics2D.gravity;
        _trailRenderer = transform.GetComponentInChildren<TrailRenderer>();
        _trailNormalValue = _trailRenderer.time;
        _originalBodyScale = bodyGraphics.localScale;
        _cameraScript = GameObject.FindGameObjectWithTag("Camera").GetComponent<CameraScript>();
        
        _charaDeath += Death;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsGrounded())
        {
            _coyoteCounter = coyoteTime;
            if (!runParticle.isPlaying) runParticle.Play();
        }
        else
        {
            _coyoteCounter -= Time.deltaTime;
            runParticle.Stop();
        }

        _jumpBufferCounter -= Time.deltaTime;

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
                
                bodyGraphics.DOScale(bodyJumpScale, bodyJumpTime)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        bodyGraphics.DOScale(_originalBodyScale, bodyJumpTime)
                            .SetEase(Ease.OutBounce);
                    });
            }
            else
            {
                //Jump
                
                if(_coyoteCounter>0 && _jumpBufferCounter>0)
                {
                    _cameraScript.CameraShake(jumpCameraShakeIntensity, jumpCameraShakeTime);
                    bodyGraphics.DOScale(bodyJumpScale, bodyJumpTime)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            bodyGraphics.DOScale(_originalBodyScale, bodyJumpTime)
                                .SetEase(Ease.OutBounce);
                        });
                    
                    _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
                    _isJumping = true;
                    
                    _jumpCounter = 0;
                    _jumpBufferCounter = 0;
                }
                if(_rb.velocity.y > 0 && _isJumping)
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

    void OnJump() //fonction lancé 2 fois, à l'appuie et quand on lache le bouton de saut
    {
        _jumpButtonState = !_jumpButtonState;
        if (_jumpButtonState) //quand Appuie sur le bouton
        {
            _jumpBufferCounter = jumpBufferTime;
        }
        else if(!_jumpButtonState) //quand relache le bouton
        {
            _isJumping = false;
            
            _coyoteCounter = 0;
            _jumpCounter = 0;
            
            if (_rb.velocity.y>0)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * .6f);
            }
        }
    }
    
    void OnDash() //fonction lancé 2 fois, à l'appuie et quand on lache le bouton de Dash
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

    void OnReset()
    {
        Debug.Log("Reset");
        ReloadScene();
    }

    bool IsGrounded()
    {
        bool ret = Physics2D.OverlapCapsule(groundCheck.position, new Vector2(1f, 0.15f), CapsuleDirection2D.Horizontal,
            0, groundLayer);
        if(ret)
        {
            if(passOne)
            {
                passOne = false;
                landParticle.Play();
                _cameraScript.CameraShake(landCameraShakeIntensity, landCameraShakeTime);
                bodyGraphics.DOScale(bodySquashScale, bodySquashTime)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        bodyGraphics.DOScale(_originalBodyScale, bodySquashTime)
                            .SetEase(Ease.OutBounce);
                    });
            }
        }
        else
        {
            passOne = true;
        }
        return ret;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("DeathZone"))
        {
            _charaDeath();
        }

        if (other.CompareTag("Finish"))
        {
            Debug.Log("C'est gagné");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        if (other.CompareTag("Enemy"))
        {
            if (_isDashing)
            {
                other.GetComponent<SpriteRenderer>().DOFade(0f, 0.1f).OnComplete((() =>
                {
                    other.GetComponent<SpriteRenderer>().DOFade(1f, 1f).SetEase(Ease.InOutCubic);
                }));
            }
            else
            {
                _charaDeath();
            }
        }
    }

    private void Death()
    {
        Debug.Log("Ah il est oh sol!");
        ReloadScene();
    }

    private void ReloadScene()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
