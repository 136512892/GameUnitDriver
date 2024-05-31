using UnityEngine;
using System.Collections;

public class DelayDestroy : MonoBehaviour
{
    [SerializeField]
    private float delay = 3f;

    private IEnumerator Start()
    {
        //延时销毁
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}