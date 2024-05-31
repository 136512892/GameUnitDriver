using UnityEngine;

public class MathfExample : MonoBehaviour
{
    private void Start()
    {
        //60°的余弦值
        Debug.Log(Mathf.Cos(60f * Mathf.Deg2Rad)); //0.5
        //0.5的反余弦值
        Debug.Log(Mathf.Acos(0.5f) * Mathf.Rad2Deg); //60

        Debug.Log(Mathf.Round(2.5f)); //2
        Debug.Log(Mathf.Round(3.5f)); //4
    }
}