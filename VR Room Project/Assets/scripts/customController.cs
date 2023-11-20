using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR.Input;
using XRController = UnityEngine.InputSystem.XR.XRController;

public class customController : MonoBehaviour
{
    [SerializeField] InputActionReference primaryInputActionReference;
    [SerializeField] InputActionReference secondaryInputActionReference;
    [SerializeField] private InputActionProperty open;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private TMP_InputField mainText;
    [SerializeField] private Canvas wheel;

    private bool selected = false;
    private bool tracking = false;
    private Vector2 lastAxis = Vector2.zero;
    private Vector2 axis;

    // Start is called before the first frame update
    void Start()
    {
        wheel.enabled = false;
        primaryInputActionReference.action.performed += OnBackspace;
        secondaryInputActionReference.action.performed += OnEnter;
    }

    // Update is called once per frame
    void Update()
    {
        if (!selected)
            return;
        axis = open.action?.ReadValue<Vector2>() ?? Vector2.zero;
        // Debugger.Instance.LogIt($"Axis: {axis.ToString()}; Last Axis: {lastAxis.ToString()}");
        if (axis == Vector2.zero && lastAxis != Vector2.zero)
        {
            OnClose();
            tracking = false;
        }
        else if (axis != Vector2.zero && lastAxis == Vector2.zero)
        {
            tracking = true;
        }
        else if (tracking && ((Mathf.Abs(axis.x) > 0.75 && Mathf.Abs(axis.y) < 0.25) || (Mathf.Abs(axis.y) > 0.75 && Mathf.Abs(axis.x) < 0.25)))
        {
            tracking = false;
            OnOpen();
        }
        else if (axis != Vector2.zero && lastAxis != Vector2.zero)
        {
            // OnHover();
            wheel.transform.position = transform.position;
            var transform1 = wheel.transform;
            var diff = Vector3.SignedAngle(transform.up, transform1.up, transform1.forward);
            // Debugger.Instance.LogIt(diff.ToString(CultureInfo.CurrentCulture));
        }
        else
        {
        }

        lastAxis = axis;
    }

    private void OnBackspace(InputAction.CallbackContext c)
    {
        if (!selected)
            return;

        mainCanvas.GetComponent<Keyboard>().DeleteChar();
    }

    private void OnEnter(InputAction.CallbackContext c)
    {
        if (!selected)
            return;

        mainText.onSubmit.Invoke("");
    }


    public void OnSelect()
    {
        Debugger.Instance.LogIt("Here");
        selected = !selected;
    }

    public void OnOpen()
    {
        if (!selected)
            return;
        var transform1 = transform;
        wheel.transform.SetPositionAndRotation(transform1.position, transform1.rotation);
        wheel.enabled = true;
        if (wheel.TryGetComponent<LeftWheelController>(out var left))
            left.OnSelectRow(axis);
        if (wheel.TryGetComponent<RightWheelController>(out var right))
            right.OnSelectRow(axis);
    }

    private void OnClose()
    {
        var transform1 = wheel.transform;
        var diff = Vector3.SignedAngle(transform.up, transform1.up, transform1.forward);
        Debugger.Instance.LogIt(diff.ToString(CultureInfo.CurrentCulture));
        if (wheel.TryGetComponent<LeftWheelController>(out var left))
            left.OnSelect(diff);
        if (wheel.TryGetComponent<RightWheelController>(out var right))
            right.OnSelect(diff);
        wheel.enabled = false;
    }
}