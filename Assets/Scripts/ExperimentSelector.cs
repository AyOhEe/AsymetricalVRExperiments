using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentSelector : MonoBehaviour
{
    //scroll bar to use for this menu
    public Scrollbar scrollbar;

    //the content to scroll
    public RectTransform content;
    public RectTransform contentEnd;

    //the position of content when the scrollbar value is 0
    public Vector3 noScroll;
    //the position of content when the scrollbar value is 1
    public Vector3 fullScroll;

    // Start is called before the first frame update
    void Start()
    {
        //get the positions at different levels of scroll
        noScroll = content.localPosition;
        fullScroll = contentEnd.localPosition;

        //update the scrollValue every time the scrollbar is moved
        scrollbar.onValueChanged.AddListener((val) => { content.localPosition = Vector3.Lerp(noScroll, fullScroll, val); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
