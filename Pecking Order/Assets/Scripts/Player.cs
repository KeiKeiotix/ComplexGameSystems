using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 20f)]
    [Tooltip("Movement speed of the Player")]
    float moveSpeed = 5;

    [SerializeField]
    [Range(0f, 20f)]
    [Tooltip("Rotation speed of the Player")]
    float rotationSpeed = 5;

    [SerializeField]
    [Range(0f, 10f)]
    [Tooltip("Jump height of the Player")]
    float jumpHeight = 1;

    [SerializeField]
    InputAction movement;



    Rigidbody rb;

    private float vertical
    {
        get
        {
            Keyboard keyboard = Keyboard.current;
            float vertical = 0; 
            if (keyboard.wKey.isPressed) { vertical += 1; }

            if (keyboard.sKey.isPressed) { vertical -= 1; }

            return vertical;
        }
    }

    private float horizontal
    {
        get
        {
            Keyboard keyboard = Keyboard.current;
            float horizontal = 0;
            if (keyboard.dKey.isPressed) { horizontal += 1; }

            if (keyboard.aKey.isPressed) { horizontal -= 1; }

            return horizontal;
        }
    }

    private bool isMoving
    {
        get
        {
            return (vertical != 0 || horizontal != 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {

        Jump();

        if (!isMoving) { return; }

        Vector3 posChange = Vector3.zero;
        posChange.x = horizontal;
        posChange.z = vertical;
        posChange = posChange.normalized * moveSpeed * Time.deltaTime;

        transform.position += posChange;
    }

    void FixedUpdate()
    {
        //_animator.SetBool(IsMovingParemeterID, IsMoving);
    }

    private void Jump ()
    {
        Vector3 jumpForce = Vector3.zero;
        Keyboard keyboard = Keyboard.current;

        if (keyboard.spaceKey.isPressed && rb.velocity.y <= 0)
        {
            jumpForce.y = Mathf.Sqrt(-2 * jumpHeight * Physics.gravity.y) - rb.velocity.y;

            rb.velocity += jumpForce;
        }
        
     
    }


}
