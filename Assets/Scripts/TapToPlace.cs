using UnityEngine;

public class TapToPlace : MonoBehaviour
{
    public GameObject couch;
    int state { get; set; }
    bool placing = false;
    Vector3 start_position { get; set; }
    float rotate { get; set; }
    float scale { get; set; }
    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        // On each Select gesture, toggle whether the user is in placing mode.
        placing = !placing;

        // If the user is in placing mode, display the spatial mapping mesh.
        if (placing)
        {
            SpatialMapping.Instance.DrawVisualMeshes = true;
        }
        // If the user is not in placing mode, hide the spatial mapping mesh.
        else
        {
            SpatialMapping.Instance.DrawVisualMeshes = false;
        }
    }

    void ScaleObject()
    {
        print("ScaleObject");
        state = 1;
    }

    void RotateObject()
    {
        print("RotateObject");
        state = 2;
    }

    void SetObject(GameObject source)
    {
        print("OnSelect Couch");
        couch = source;
    }

        void PerformManipulationStart(Vector3 position)
    {
        print("PerformManipulationStart: x=" + position.x + " y=" + position.y + " z=" + position.z);

        start_position = position;

        scale = 0.0f;
        rotate = 0.0f;
    }

    void PerformManipulationUpdated(Vector3 position)
    {
        print("PerformManipulationUpdated: x="+(start_position.x-position.x)+" y="+(start_position.y-position.y)+" z="+(start_position.z-position.z));
        scale = (start_position.y - position.y);
        rotate = (start_position.x - position.x);

        if (state == 1)
        {
            if ((couch.transform.localScale.x + -0.005f) < 0.25f)
            {
                print("Too Small");
            }
            else if ((couch.transform.localScale.x + -0.005f) > 1.5f)
            {
                print("Too Big");
            }
            else
            {
                print("Scaling");
                if (scale > 0.0f)
                {
                    couch.transform.localScale += new Vector3(-0.005f, -0.005f, -0.005f);
                }
                else
                {
                    couch.transform.localScale += new Vector3(0.005f, 0.005f, 0.005f);
                }

            }
        }
        else if (state == 2)
        {
            couch.transform.Rotate(0.0f, rotate, 0.0f, Space.World);
        }
    }

    void PerformManipulationCompleted(Vector3 position)
    {
        print("PerformManipulationCompleted: x=" + position.x + " y=" + position.y + " z=" + position.z);
        
    }

    void PerformManipulationCanceled(Vector3 position)
    {
        print("PerformManipulationCanceled: x=" + position.x + " y=" + position.y + " z=" + position.z);
    }

    // Update is called once per frame
    void Update()
    {
        // If the user is in placing mode,
        // update the placement to match the user's gaze.

        if (placing)
        {
            // Do a raycast into the world that will only hit the Spatial Mapping mesh.
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
                30.0f, SpatialMapping.PhysicsRaycastMask))
            {
                // Move this object's parent object to
                // where the raycast hit the Spatial Mapping mesh.
                this.transform.position = hitInfo.point;

                // Rotate this object's parent object to face the user.
                Quaternion toQuat = Camera.main.transform.localRotation;
                toQuat.x = 0;
                toQuat.z = 0;
                //this.transform.rotation = toQuat;
            }
        }
    }
}
