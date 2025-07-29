using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CircleController : MonoBehaviour
{
    //Object Variables
    public PlayerInput inputActions;
    private Rigidbody2D rb;
    private Camera cam;
    //Double Tap
    private Vector2 touchStartPos;
    private float lastTapTime;
    private float doubleTapThreshold = 0.3f; // Time in seconds to consider a double tap
    //Dragging
    private bool isDragging = false;

    [Header("Settings")]
    public float speed = 5f;// Adjust this value to change movement speed
    public float swipeForce = 300f; // Adjust this value to change swipe strength

    void Awake()
    {
        inputActions = new PlayerInput();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void OnEnable()
    {
        // Enable the input actions and subscribe to touch events
        inputActions.TouchControls.Enable();
        inputActions.TouchControls.PrimaryTouch.started += OnPrimaryTouchStarted;
        inputActions.TouchControls.PrimaryTouch.canceled += OnPrimaryTouchCanceled;
    }

    void OnDisable()
    {
        // Disable the input actions and unsubscribe from touch events
        inputActions.TouchControls.Disable();
        inputActions.TouchControls.PrimaryTouch.started -= OnPrimaryTouchStarted;
        inputActions.TouchControls.PrimaryTouch.canceled -= OnPrimaryTouchCanceled;
    }

    void OnPrimaryTouchStarted(InputAction.CallbackContext context)
    {
        // Get the touch position in world coordinates
        Vector2 screenPos = inputActions.TouchControls.TouchPosition.ReadValue<Vector2>();
        // Convert screen position to world position
        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

        // Check for double tap
        if (Time.time - lastTapTime < doubleTapThreshold)
        {
            transform.position = worldPos;
            Debug.Log("Double Tap Detected");
            return;
        }
        lastTapTime = Time.time;

        //Collision check for dragging
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null && hit.transform == transform)
        {
            isDragging = true;
        }
        else
        {
            //Tap 
            StopMovement();
            StartCoroutine(MoveToPosition(worldPos));
            Debug.Log("Tap Detected");
        }

        touchStartPos = worldPos; // Save for swipe detection
    }

    void OnPrimaryTouchCanceled(InputAction.CallbackContext context)
    {
        if (isDragging)
        {
            isDragging = false;
            return;
        }
        // Swipe detection
        Vector2 screenEnd = inputActions.TouchControls.TouchPosition.ReadValue<Vector2>();
        // Convert screen end position to world position
        Vector2 worldEnd = cam.ScreenToWorldPoint(screenEnd);
        // Calculate swipe direction
        Vector2 swipeDirection = (worldEnd - touchStartPos).normalized;

        // Check if the swipe distance is significant
        float swipeDistance = Vector2.Distance(worldEnd, touchStartPos);
        if (swipeDistance > 0.5f)
        {
            // Apply force in the swipe direction
            rb.AddForce(swipeDirection * swipeForce);
            Debug.Log("Swipe Detected!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            // Handle dragging
            Vector2 pos = inputActions.TouchControls.TouchPosition.ReadValue<Vector2>();
            Vector2 worldPos = cam.ScreenToWorldPoint(pos);
            transform.position = new Vector3(worldPos.x, worldPos.y, 0);
            Debug.Log("Dragging Detected");
        }
    }

    void StopMovement()
    {
        // Stop all movement and clear velocity
        StopAllCoroutines();
        rb.velocity = Vector2.zero; // Clear any existing velocity
    }

    // Coroutine to move the circle to a target position smoothly
    System.Collections.IEnumerator MoveToPosition(Vector2 target)
    {
        // Smoothly move the circle to the target position
        while (Vector2.Distance(transform.position, target) > 0.05f)
        {
            // Move towards the target position
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }


}
