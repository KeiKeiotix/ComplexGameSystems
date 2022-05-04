using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public GameObject target;

    public float cameraSense = 2;
    public float camMaxDist = 5;
    public float camMovebackSpeed = 5;

    float currentDistance;


    //toggles to turn on and off camera move.
    bool clickLastUpdate = false;
    bool moveCamToggle = false;

    // Start is called before the first frame update
    void Start()
    {
        currentDistance = camMaxDist;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(clickLastUpdate + " " + moveCamToggle);
        if (Input.GetMouseButton(1) && !clickLastUpdate)
        {
            clickLastUpdate = true;
            if (moveCamToggle)
            {
                moveCamToggle = false;
            } else
            {
                moveCamToggle = true;
            }
        } else if (!Input.GetMouseButton(1) && clickLastUpdate)
        {
            clickLastUpdate= false;
        }


        // right drag rotates the camera
        if (moveCamToggle)
        {
            Vector3 angles = transform.eulerAngles;

            float dx = -Input.GetAxis("Mouse Y");
            float dy = Input.GetAxis("Mouse X");

            // look up and down by rotating around X-axis
            angles.x = Mathf.Clamp(angles.x + dx * cameraSense, 0, 70);

            // spin the camera round
            angles.y += dy * cameraSense;
            transform.eulerAngles = angles;
        }

        RaycastHit hit;
        if (Physics.Raycast(target.transform.position, -transform.forward, out hit, camMaxDist))
        {
            currentDistance = hit.distance;
        }
        else
        {
            currentDistance = Mathf.MoveTowards(currentDistance, camMaxDist, Time.deltaTime * camMovebackSpeed);
        }


        // look at the target point
        transform.position = GetTargetPos() - currentDistance * transform.forward;
    }

    Vector3 GetTargetPos()
    {
        return target.transform.position;
    }
}
