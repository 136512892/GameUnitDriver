using UnityEngine;
using UnityEngine.UI;
using Outline = QuickOutline.Outline;

public class EventResponserSample : MonoBehaviour, IEventResponser
{
    [SerializeField]
    protected Outline outline;
    [SerializeField]
    protected string description;
    [SerializeField]
    private Text descriptionText;

    public void OnEnter()
    {
        outline.enabled = true;
        descriptionText.text = description;
        descriptionText.gameObject.SetActive(true);
    }
    public void OnExit()
    {
        outline.enabled = false;
        descriptionText.gameObject.SetActive(false);
    }
    public void OnStay() 
    {
        descriptionText.rectTransform
            .anchoredPosition3D = Input.mousePosition;
    }
    public void OnClick() { }
}