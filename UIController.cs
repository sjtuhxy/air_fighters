using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public JetController jetController;

    public Text v_valueText;
    public Text heightText;
    public Text pitchdotText;
    public Text yawdottext;
    public Text angletext;
    public Text vdottext;
    public Text vtext;
    public Text rollangletext;
    public Text tangload;
    public RectTransform crosshairs;

    private void Start()
    {
        crosshairs.gameObject.SetActive(false);
    }

    void Update()
    {
        if (jetController == null)
        {
            Debug.LogError("Jet controller is missing");
            return;
        }

        v_valueText.text = $"v_value: {jetController.v_value:n0}";
        heightText.text = $"Height: {jetController.height:n0}";
        pitchdotText.text = $"pitchdot: {jetController.pitchdot:n0}";
        yawdottext.text = $"yawdot:{jetController.yawdot:n0}";
        angletext.text = $"pitch,yaw,roll:{jetController.angle:n0}";
        vdottext.text = $"vdot:{jetController.vdot:n0}";
        vtext.text = $"v:{jetController.v:n0}";
        rollangletext.text = $"rollangle:{jetController.rollangle:n0}";
        tangload.text = $"tangload:{jetController.tangentialoverload:n0}";

        if (jetController.showCrosshairs)
        {
            jetController.showCrosshairs = false;
            if (!crosshairs.gameObject.activeSelf) crosshairs.gameObject.SetActive(true);
        }

        crosshairs.position = jetController.crosshairPosition;
    }
}
