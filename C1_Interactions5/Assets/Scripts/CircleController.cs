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
    private Vector2 touchStartPos;
    private float lastTapTime;
    private float doubleTapThreshold = 0.3f; 
    private bool isDragging = false;

    [Header("Settings")]
    public float speed = 5f;
    public float swipeForce = 300f;

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
        inputActions.TouchControls.PrimaryTouch.canceled += OnPrimaryTouchCanceled;
    }

    void OnDisable()
    {
        inputActions.TouchControls.Disable();
        inputActions.TouchControls.PrimaryTouch.started -= OnPrimaryTouchStarted;
        inputActions.TouchControls.PrimaryTouch.canceled -= OnPrimaryTouchCanceled;
    }

    void OnPrimaryTouchStarted(InputAction.CallbackContext context)
    {
        Vector2 screenPos = inputActions.TouchControls.TouchPosition.ReadValue<Vector2>();
        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

        // Check for double tap
        if (Time.time - lastTapTime < doubleTapThreshold)
        {
            transform.position = worldPos;
            Debug.Log("Double Tap Detected");
            return;
        }
        lastTapTime = Time.time;

        //cek apakah menyentuh collider
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null && hit.transform == transform)
        {
            isDragging = true;
        }
        else
        {
            //Tap biasa
            StopMovement();
            StartCoroutine(MoveToPosition(worldPos));
            Debug.Log("Tap Detected");
        }

        touchStartPos = worldPos;
    }

    void OnPrimaryTouchCanceled(InputAction.CallbackContext context)
    {
        if (isDragging)
        {
            isDragging = false;
            return;
        }

        Vector2 screenEnd = inputActions.TouchControls.TouchPosition.ReadValue<Vector2>();
        Vector2 worldEnd = cam.ScreenToWorldPoint(screenEnd);
        Vector2 swipeDirection = (worldEnd - touchStartPos).normalized;

        float swipeDistance = Vector2.Distance(worldEnd, touchStartPos);
        if (swipeDistance > 0.5f)
        {
            rb.AddForce(swipeDirection * swipeForce);
            Debug.Log("Swipe Detected!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            Vector2 pos = inputActions.TouchControls.TouchPosition.ReadValue<Vector2>();
            Vector2 worldPos = cam.ScreenToWorldPoint(pos);
            transform.position = new Vector3(worldPos.x, worldPos.y, 0);
            Debug.Log("Dragging Detected");
        }
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
