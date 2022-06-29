using UnityEngine;

//게임 오브젝트를 지속적으로 회전하는 스크립트
public class Rotator : MonoBehaviour {
    public float fRotationSpeed = 60f;//회전속도

    private void Update() 
    {
        transform.Rotate(0f, fRotationSpeed * Time.deltaTime, 0f);
    }
}