using UnityEngine;

/*
 * A simple script which allow the camera to move over a 2D map
 * It handle both mouse and touch input
 * TODO : calculate camera frustum and enforce boudary based on frustrum instead of camera position
 */ 
public class MapCamera2D : MonoBehaviour
{
    private float defaultZ;
    private Vector3 startPos, movePos, currentPos, wantedPos;
    private float currentZoom, wantedZoom;
    private Camera mCamera;
    public float damping = 5.0f;
    public float zoomSpeed = 1.0f;
    public float moveSpeed = 3.0f;
    // The first element is the min, the second is the max
    public float[] xlimit = new float[2] { 0, 100 };
    public float[] ylimit = new float[2] { 0, 100 };
    public float[] zoomLimit = new float[2] { 1, 15 };


    void Awake()
    {
        mCamera = Camera.main;
        this.defaultZ = transform.position.z;  // Distance camera is above map
        this.currentPos = new Vector3(transform.position.x, transform.position.y, this.defaultZ);
        this.wantedPos = new Vector3(this.currentPos.x, this.currentPos.y, this.defaultZ); // Copy the vector
        this.currentZoom = Camera.main.orthographicSize;
        this.wantedZoom = this.currentZoom;
    }

    /*
     * A public function to move the camera with another script.
     * Any move input cancel the movement
     */ 
    public void moveTo(Vector3 target)
    {
        target.x = Mathf.Clamp(target.x, xlimit[0], xlimit[1]);
        target.y = Mathf.Clamp(target.y, ylimit[0], ylimit[1]);
        target.z = this.defaultZ; // avoid Z movement
        this.wantedPos = target;
    }

    /*
     * A public function to zoom the camera with another script.
     * Any zoom input cancel the zoom
     */
    public void zoomTo(float target)
    {
        Debug.Log("In ZoomTo");
        target = Mathf.Clamp(target, zoomLimit[0], zoomLimit[1]);
        this.wantedZoom = target;
    }

    /*
     * Immediatly apply a Zoom to the camera.
     * If the camera was previously zooming through wantedZoom, cancel it.
     */
    private void applyZoom(float diff)
    {
        Debug.Log("In applyZoom");
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - diff, zoomLimit[0], zoomLimit[1]);
        this.currentZoom = Camera.main.orthographicSize;
        this.wantedZoom = this.currentZoom;
        Debug.Log(this.wantedPos);
        Debug.Log(this.currentPos);
    }

    /*
     * Immediatly apply a transform to the camera.
     * If the camera was previously moving through wantedPos, cancel it.
     */
    private void applyTransform(Vector3 newPos)
    {
        newPos.x = Mathf.Clamp(newPos.x, xlimit[0], xlimit[1]);
        newPos.y = Mathf.Clamp(newPos.y, ylimit[0], ylimit[1]);
        transform.position = newPos;
        this.currentPos = newPos;
        this.wantedPos = newPos;
    }

    /*
     * Animate the movement to a certain position with an uniform amount of time
     */
    private void moveCameraToTarget()
    {
        Debug.Log("In moveCameraToTarget");
        transform.position = Vector3.Lerp(transform.position, this.wantedPos, (Time.deltaTime * damping));
        this.currentPos = transform.position;
    }

    /*
     * Animate the movement to a certain position with an uniform amount of time
     */
    private void zoomCameraToTarget()
    {
        Camera.main.orthographicSize = Mathf.Lerp(this.currentZoom, this.wantedZoom, (Time.deltaTime * damping));
        this.currentZoom = Camera.main.orthographicSize;
    }

    void Update()
    {
        // Handle Mouse movement
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            movePos = Input.mousePosition - startPos;
            float magnitude = (movePos).magnitude;
            if (magnitude > 0.1f)
            {
                // To avoid acceleration as the button is pressed, we need to normalize movePos
                movePos.Normalize();
                movePos = movePos * Time.deltaTime * moveSpeed;
                Vector3 tmp = new Vector3(transform.position.x - movePos.x, transform.position.y - movePos.y, defaultZ);
                applyTransform(tmp);
            }
            
        }
        // Handle Touch Movement
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startPos = Input.GetTouch(0).position;
        }
        else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            movePos = new Vector3(Input.GetTouch(0).position.x - startPos.x, Input.GetTouch(0).position.y - startPos.y, startPos.z);
            float magnitude = (movePos).magnitude;
            if (magnitude > 0.1f)
            {
                movePos.Normalize();
                movePos = movePos * Time.deltaTime * moveSpeed;
                Vector3 tmp = new Vector3(transform.position.x - movePos.x, transform.position.y - movePos.y, defaultZ);
                this.applyTransform(tmp);
            }
        }
        else if (Input.touchCount == 2) // pinch to zoom
        {
            Touch tZero = Input.GetTouch(0);
            Touch tOne = Input.GetTouch(1);

            Vector2 zeroDelta = tZero.position - tZero.deltaPosition;
            Vector2 oneDelta = tOne.position - tOne.deltaPosition;
            float initialMagnitude = (zeroDelta - oneDelta).magnitude;
            float currentMagnitude = (tZero.position - tOne.position).magnitude;

            float diff = currentMagnitude - initialMagnitude;
            this.applyZoom(diff * 0.01f * zoomSpeed);
        }
        // Handle mouse wheel zoom
        float mWheel = Input.GetAxis("Mouse ScrollWheel");
        if (mWheel != 0)
        {
            this.applyZoom(mWheel * zoomSpeed);
        }
        // If another script want to move th camera
        if (Vector3.Distance(this.wantedPos, this.currentPos) > 0.1f)
        {
            this.moveCameraToTarget();
        }
        if (Mathf.Abs(this.wantedZoom - this.currentZoom) > 0.1f)
        {
            this.zoomCameraToTarget();
        }
    }
}
