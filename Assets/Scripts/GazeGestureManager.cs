using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GazeGestureManager : MonoBehaviour
{
    public static GazeGestureManager Instance { get; private set; }
    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }
    bool spawnedCouch;
    bool spawnedTV;
    public bool IsNavigating { get; private set; }

    public Vector3 NavigationPosition { get; private set; }

    //state 0 = start 
    //state 1 = selected & moving
    //state 2 = spawn object

    public static int object_type;
    public static int state;
    GestureRecognizer recognizer;
    public FurniturePlacer placer;
    public TVPlacer tv_placer;

    public RaycastHit last_hitInfo { get; private set; }

    // Use this for initialization
    void Start()
    {
        object_type = 1;
        Instance = this;
        spawnedCouch = false;
        spawnedTV = false;

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.NavigationX | GestureSettings.NavigationY);
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {

            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null)
            {
                
                FocusedObject.SendMessageUpwards("OnSelect");
                if (state == 0)
                {
                    print("FocusedObject Picked Up");
                    state = 1;
                }
                else if(state == 1)
                {
                    print("FocusedObject Placed");
                    state = 0;
                }else
                {
                    print("Why am i here ");
                }
            }
            else if(FocusedObject == null && ((!spawnedCouch && object_type == 1) || (!spawnedTV && object_type == 2)))
            {
                

                if (object_type == 1)
                {
                    print("Spawn Couch");
                    placer.SpawnNewFurniture(last_hitInfo.point);
                    spawnedCouch = true;
                }
                else if (object_type == 2)
                {
                    print("Spawn TV");
                    tv_placer.SpawnNewFurniture(last_hitInfo.point);
                    spawnedTV = true;
                }
                

                //state = 0;
            }
        };

        recognizer.NavigationStartedEvent += (source, relativePosition, ray) =>
        {

            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null)
            {
                // 2.b: Set IsNavigating to be true.
                IsNavigating = true;

                // 2.b: Set NavigationPosition to be relativePosition.
                NavigationPosition = relativePosition;
                FocusedObject.SendMessageUpwards("SetObject", FocusedObject);
                FocusedObject.SendMessageUpwards("PerformManipulationStart", relativePosition);
                print("NavigationStartedEvent: Got Object");
            }
            else if (FocusedObject == null)
            {
                print("NavigationStartedEvent: No Object");
            }
        };

        recognizer.NavigationUpdatedEvent += (source, relativePosition, ray) =>
        {

            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null)
            {
                // 2.b: Set IsNavigating to be true.
                IsNavigating = true;

                // 2.b: Set NavigationPosition to be relativePosition.
                NavigationPosition = relativePosition;
                
                FocusedObject.SendMessageUpwards("PerformManipulationUpdated", relativePosition);
                print("NavigationUpdatedEvent: Got Object");
            }
            else if (FocusedObject == null)
            {
                print("NavigationUpdatedEvent: No Object");
            }
        };

        recognizer.NavigationCompletedEvent += (source, relativePosition, ray) =>
        {

            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null)
            {
                // 2.b: Set IsNavigating to be true.
                IsNavigating = false;

                // 2.b: Set NavigationPosition to be relativePosition.
                NavigationPosition = relativePosition;

                FocusedObject.SendMessageUpwards("PerformManipulationCompleted", relativePosition);
                print("NavigationCompletedEvent: Got Object");
            }
            else if (FocusedObject == null)
            {
                print("NavigationCompletedEvent: No Object");
            }
        };

        recognizer.NavigationCanceledEvent += (source, relativePosition, ray) =>
        {

            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null)
            {
                // 2.b: Set IsNavigating to be true.
                IsNavigating = false;

                // 2.b: Set NavigationPosition to be relativePosition.
                NavigationPosition = relativePosition;

                FocusedObject.SendMessageUpwards("PerformManipulationCanceled", relativePosition);

                print("NavigationCanceledEvent: Got Object");
            }
            else if (FocusedObject == null)
            {
                print("NavigationCanceledEvent: No Object");
            }
        };

        recognizer.StartCapturingGestures();
    }

    // Update is called once per frame
    void Update()
    {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;



        //Focused on a placed object
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo) && state <= 1)
        {
            if (hitInfo.collider.gameObject.tag == "couch" || hitInfo.collider.gameObject.tag == "tv")
            {
                // If the raycast hit a hologram, use that as the focused object.
                FocusedObject = hitInfo.collider.gameObject;
                //print("Object Mesh");
            }else 
            {
                last_hitInfo = hitInfo;
                FocusedObject = null;
                //print("Spatial Mesh 2");
            }
        }
        //focused on the spatial mesh
        else if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
                10.0f, SpatialMapping.PhysicsRaycastMask) && state == 0 )
        {

            last_hitInfo = hitInfo;
            FocusedObject = null;
            //print("Spatial Mesh 1");
        }
        
        
        //else 
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
            //print("No Mesh");
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
    }
}
