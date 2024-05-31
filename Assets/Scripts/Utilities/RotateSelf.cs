using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    [SerializeField]
    private Vector3 axis = Vector3.right;

    [SerializeField]
    private Vector3 offset = Vector3.zero;

    [SerializeField]
    private float speed = 5f;

    private void Update()
    {
        transform.RotateAround(
            transform.position + offset,
            transform.rotation * axis, 
            speed * Time.deltaTime);
    }
}
