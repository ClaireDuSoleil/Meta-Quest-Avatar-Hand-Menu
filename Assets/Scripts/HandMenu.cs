using Oculus.Interaction;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandMenu : MonoBehaviour
{
    public enum MenuUI
    {
        Menu0 = 0,
        Menu1 = 1,
        Menu2 = 2,
        Menu3 = 3,
        Menu4 = 4,
        Menu5 = 5,
        Menu6 = 6,
    }

    [SerializeField]
    [Tooltip("The camera attached to the user's head.")]
    private Camera headTrackedCamera = null;

    [SerializeField]
    [Tooltip("Oculus's articulated hand-tracking component.")]
    private OVRHand hand = null;

    [SerializeField]
    [Tooltip("A transform at the center of the hand-tracked palm.")]
    private Transform palmTransform = null;

    [SerializeField]
    [Tooltip("The threshold, in degrees, between the user's head and palm to activate/deactivate the Hand Launcher UI")]
    private float showLauncherThreshold = 35f;

    [SerializeField]
    [Tooltip("The Transform used to position/rotate the Hand Launcher UI.")]
    private Transform handLauncherPivot = null;

    [SerializeField]
    private GameObject hingeButton;
    [SerializeField]
    private GameObject scrollingButton;

    [SerializeField]
    private GameObject HandMenuGroup;

    [SerializeField]
    private GameObject FingerTipSphere;

    [SerializeField]
    private GameObject menu0;

    [SerializeField]
    private GameObject localAvatar;


    private List<GameObject> buttonList = new List<GameObject>();
    private GameObject scrollButton;
    private bool useNewButtons = true;
    private bool handLauncherActive = false;
    private int startAngle;
    private int stopAngle;
    private bool isScrolling;
    private const int BUTTON_COUNT = 6;
    private Vector3 smallButtonSize = new Vector3(.25f, .25f, .25f);
    private Vector3 bigButtonSize = new Vector3(.55f, .55f, .55f);

    ButtonControl scrollControl;

    public void OnButtonPressed()
    {
        for (int i = 0; i < buttonList.Count; i++)
        {
            ButtonControl control = buttonList[i].GetComponent<ButtonControl>();
            control.SetCollisionDetection(false);
        }
        Invoke("ResetButtons", 0.75f);
    }

    private void ResetButtons()
    {
        for (int i = 0; i < buttonList.Count; i++)
        {
            ButtonControl control = buttonList[i].GetComponent<ButtonControl>();
            control.SetCollisionDetection(true);
        }
    }

    private bool HandLauncherShouldBeActive
    {
        get
        {
            float angleBetweenHeadAndPalm = Vector3.Angle(-palmTransform.up, headTrackedCamera.transform.forward);
            return Mathf.Abs(angleBetweenHeadAndPalm) < showLauncherThreshold;
        }
    }

    private void Start()
    {
        OnCloseAllMenus();

        scrollButton = Instantiate(scrollingButton);
        scrollButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Scroll";
        scrollControl = scrollButton.GetComponent<ButtonControl>();
        scrollButton.transform.localScale = smallButtonSize;
        scrollControl.SetHandMenu(this);
        scrollControl.SetAnimate(true, 1.04f);
        scrollControl.SetCollisionDetection(true);
        scrollControl.GetEventObject().AddListener(OnScroll);

        for (int i = 0; i < BUTTON_COUNT; i++)
        {
            GameObject button;
            button = Instantiate(hingeButton);
            button.name = i.ToString();
            ButtonControl control = button.GetComponent<ButtonControl>();
            control.SetAnimate(false, 1.2f);
            switch (i)
            {
                case 0:
                    {
                        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Sample\nMenu";
                        control.GetEventObject().AddListener(OnMenu_0);
                        break;
                    }
                case 1:
                    {
                        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Inactive";                        
                        break;
                    }
                case 2:
                    {
                        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Inactive";                        
                        break;
                    }
                case 3:
                    {
                        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Inactive";                        
                        break;
                    }
                case 4:
                    {
                        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Inactive";
                        break;
                    }
                case 5:
                    {
                        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Exit App";
                        control.GetEventObject().AddListener(OnExit);
                        break;
                    }
            }

            button.transform.localScale = smallButtonSize;
            control.SetHandMenu(this);
            control.SetCollisionDetection(true);
            buttonList.Add(button);
        }
    }

    private void Update()
    {
        if (!hand.IsTracked)
        {
            HideHandLauncher();
            return;
        }

        if (handLauncherActive) // Currently active
        {
            if (!HandLauncherShouldBeActive) // Should be inactive
            {
                HideHandLauncher();
            }
            else // Should be active
            {
                RepositionMenu();
            }
        }
        else if (HandLauncherShouldBeActive) // Not active, but it should be
        {
            ShowHandLauncher();
            RepositionMenu();
        }
    }

    private void HideHandLauncher()
    {
        if (!handLauncherActive)
        {
            return;
        }
        for (int i = 0; i < buttonList.Count; i++)
        {
            buttonList[i].SetActive(false);
        }
        scrollButton.SetActive(false);
        handLauncherActive = false;
    }

    private void ShowHandLauncher()
    {
        if (handLauncherActive)
        {
            return;
        }
        handLauncherActive = true;

        scrollButton.SetActive(true);
        for (int i = 0; i < buttonList.Count; i++)
        {
            buttonList[i].SetActive(true);
        }
    }

    private Vector3 calcPointZ(int angle)
    {
        float radius = .045f;
        double radians = (Math.PI / 180) * angle;
        float cx = handLauncherPivot.position.x;
        float cy = handLauncherPivot.position.y;
        float cz = handLauncherPivot.position.z;

        cz += .02f;
        cx += .02f;

        float x = (float)(cx + (radius * Math.Cos(radians)));
        float y = (float)(cy + (radius * Math.Sin(radians)));
        float z = (float)(cz + (radius * Math.Cos(radians)));
        return new Vector3(cx, y, z);
    }

    private Vector3 calcPoint(int angle)
    {
        float radius = .045f;
        double radians = (Math.PI / 180) * angle;
        float cx = handLauncherPivot.position.x;
        float cy = handLauncherPivot.position.y;
        float cz = handLauncherPivot.position.z - .025f;
      
        float x = (float)(cx + (radius * Math.Cos(radians)));
        float y = (float)(cy + (radius * Math.Sin(radians)));
        return new Vector3(x, y, cz);
    }

    private void RepositionMenu()
    {            
        if (isScrolling)
            startAngle += 2;
        if (startAngle >= 360)
            startAngle -= 360;
        if (startAngle == stopAngle)
        {
            scrollControl.SetCollisionDetection(true);
            isScrolling = false;
        }

        float yPadding = -.027f;
        float xPadding = .01f;
        
        scrollButton.transform.SetPositionAndRotation(handLauncherPivot.position + new Vector3(xPadding, yPadding * 4, -.05f), handLauncherPivot.rotation);

        for (int i = 0; i < buttonList.Count; i++)
        {
            int angle = startAngle + i * 60;
            if (angle >= 360)
                angle -= 360;
            
            if (angle == 0)
            {
                buttonList[i].transform.localScale = bigButtonSize;
            }                        
            else
            {
                buttonList[i].transform.localScale = smallButtonSize;
            }            
            buttonList[i].transform.SetPositionAndRotation(calcPoint(angle), handLauncherPivot.rotation);
        }
    }


    private void OnScroll()
    {
        if (isScrolling)
            return;
        isScrolling = true;
        scrollControl.SetCollisionDetection(false);
        stopAngle = startAngle + 60;
        if (stopAngle >= 360)
            stopAngle -= 360;
    }

    private void SetDisplayActive(MenuUI index)
    {
        GameObject display = null;
        OnCloseAllMenus();
        Vector3 pos = new Vector3();
        float distance = 0.425f;
        float displayAngle = 15.0f;
        pos.y = .95f;

        if (index == MenuUI.Menu0)
        {
            display = menu0;
        }

        pos.x = localAvatar.transform.position.x;
        pos.z = localAvatar.transform.position.z + distance;
        display.transform.localRotation = Quaternion.Euler(displayAngle, 0, 0);
        display.transform.position = pos;
        display.SetActive(true);
    }

    public void OnCloseAllMenus()
    {
        menu0.SetActive(false);
    }

    private void OnExit()
    {
        UnityEngine.Application.Quit(0);
    }
   
    private void OnMenu_0()
    {
        SetDisplayActive(MenuUI.Menu0);
    }

    public void ToggleMirror()
    {
        GameObject mirror = HandMenuGroup.transform.Find("Menu0_UI/Quad-Mirror").gameObject;
        mirror.SetActive(!mirror.activeSelf);
    }

    public void ToggleSphere()
    {
        Renderer rend = FingerTipSphere.GetComponent<Renderer>();
        if (rend.enabled)
            rend.enabled = false;
        else
            rend.enabled = true;       
    }
}
