using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkMark : MonoBehaviour
{
    Camera cameraMain;

    Vector3 worldPosition = Vector3.zero;
    Plane plane = new Plane(Vector3.up, 0);
    PlayerMovement playerScript;
    [SerializeField] GameObject walkMark;
    internal bool transition;

    const float DISTANCE_TO_DISABLE_MARK = 2;

    private void Start()
    {
        walkMark.transform.parent = null;
        cameraMain = Camera.main;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        SetMarkPositionWithRaycast();

        if(!transition && Input.GetKey(KeyCode.Mouse1))
        {
            playerScript.targetMousePos = worldPosition;

            if(walkMark.activeSelf)
                walkMark.SetActive(false);
        }

        if(Input.GetKey(KeyCode.LeftShift) && walkMark.activeSelf)
            walkMark.SetActive(false);

        if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            walkMark.transform.position = worldPosition;
            walkMark.SetActive(true);
        }

        if(walkMark.activeSelf && Vector3.Distance(playerScript.transform.position, walkMark.transform.position) <= DISTANCE_TO_DISABLE_MARK)
            walkMark.SetActive(false);
    }

    void SetMarkPositionWithRaycast()
    {
        float distance;
        Ray ray = cameraMain.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
            worldPosition = ray.GetPoint(distance);

        transform.position = worldPosition;
    }

    public void SetWalkMarkActive(bool _active)
    {
        walkMark.SetActive(_active);
    }

}
