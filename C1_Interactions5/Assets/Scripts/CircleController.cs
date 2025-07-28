using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CircleController : MonoBehaviour
{
    public PlayerInput inputActions;
    private Rigidbody2D rb;
    private Camera cam;

    [Header("Settings")]
    public float speed = 5f;

    void Awake()
    {
        inputActions = new PlayerInput();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void OnEnable()
    {
        inputActions.TouchControls.Enable();
        inputActions.TouchControls.PrimaryTouch.started += OnPrimaryTouchStarted;
        //inputActions.TouchControls.PrimaryTouch.canceled += OnPrimaryTouchCanceled;
    }

    void OnDisable()
    {
        inputActions.TouchControls.Disable();
        inputActions.TouchControls.PrimaryTouch.started -= OnPrimaryTouchStarted;
        //inputActions.TouchControls.PrimaryTouch.canceled -= OnPrimaryTouchCanceled;
    }

    void OnPrimaryTouchStarted(InputAction.CallbackContext context)
    {
        Vector2 screenPos = inputActions.TouchControls.TouchPosition.ReadValue<Vector2>();
        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

        StopMovement();
        StartCoroutine(MoveToPosition(worldPos));
        Debug.Log("Tap Detected");
    }

    void OnPrimaryTouchCanceled(InputAction.CallbackContext context)
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void StopMovement()
    {
        StopAllCoroutines();
        rb.velocity = Vector2.zero;
    }

    System.Collections.IEnumerator MoveToPosition(Vector2 target)
    {
        while (Vector2.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }


}
