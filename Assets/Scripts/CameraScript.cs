using UnityEngine;
using Cinemachine;

public class CameraScript : MonoBehaviour
{
    private CinemachineVirtualCamera _virtualCamera;
    private float _shakeTimer;
    
    private void Awake()
    {
        _shakeTimer = -0.1f;
        
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    //Shake camera using Cinemachine noise 
    public void CameraShake(float intensity, float shakeTime)
    {
        CinemachineBasicMultiChannelPerlin cameraPerlin =
            _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cameraPerlin.m_AmplitudeGain = intensity;
        _shakeTimer = shakeTime;
    }

    private void Update()
    {
        if(_shakeTimer > 0){
            
            _shakeTimer -= Time.deltaTime;
            if (_shakeTimer <= 0f)
            {
                CinemachineBasicMultiChannelPerlin cameraPerlin =
                    _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cameraPerlin.m_AmplitudeGain = 0;
            }
        }
    }
}
