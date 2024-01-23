using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonControl : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onPressed;
    [SerializeField]
    private HandMenu handMenu;    
    [SerializeField]
    private bool animatePress = false;
    [SerializeField]
    private bool isCheckbox = false;
    [SerializeField]
    private Sprite checkedSprite = null;
    [SerializeField]
    private Sprite uncheckedSprite = null;
    [SerializeField]
    private Image checkboxImage = null;

    bool isPressed = false;
    int counter = 0;   
    bool detectCollisions = false;
    bool checkedState = false;
    Vector3 originalScale;

    public void SetChecked(bool b)
    {
        checkedState = b;
        SetImage();
    }

    public void SetHandMenu(HandMenu m)
    {
        handMenu = m;
    }
    public UnityEvent GetEventObject()
    {
        return onPressed;
    }

    public void SetAnimate(bool b)
    {
        animatePress = b;
    }

    public bool GetCheckedState()
    {
        return checkedState;
    }

    public void ToggleCheckedState()
    {
        checkedState = !checkedState;
        SetImage();
    }

    public void SetCollisionDetection(bool b)
    {        
        if (b)
            detectCollisions = true;
        else
            detectCollisions = false;
    }

    private void SetImage()
    {
        if (checkedState)
        {
            checkboxImage.sprite = checkedSprite;
        }
        else
        {
            checkboxImage.sprite = uncheckedSprite;
        }
    }

    private void Start()
    {
        isPressed = false;
        SetCollisionDetection(true);
        SetAnimate(true);
        originalScale = transform.localScale;         
    }

    private void Update()
    {
        if (isPressed && animatePress)
        {           
            float factor = 1.02f;
            Vector3 scale = gameObject.transform.localScale;
            scale = new Vector3(scale.x * factor, scale.y * factor, scale.z * factor);
            gameObject.transform.localScale = scale;
            counter++;
            if (counter > 10)
            {
                isPressed = false;
                counter = 0;
                gameObject.transform.localScale = originalScale;                
            }
        }
    }   

    private void OnCollisionEnter(Collision collision)
    {
        if (!detectCollisions)
            return;
        if (collision.gameObject.name == "FingerTipSphere")
        {           
            //this is necessary if button is part of a group 
            //disable other buttons so they don't get triggered
            //if they are close together
            if (handMenu != null)
            {
                handMenu.OnButtonPressed();
            }            
            isPressed = true;
            onPressed.Invoke();
            SetCollisionDetection(false);
            Invoke("ResetCollisionDetection", .75f);
        }
    }
    private void ResetCollisionDetection()
    {
        SetCollisionDetection(true);
    }
   
}
