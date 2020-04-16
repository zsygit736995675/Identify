using UnityEngine;
using System.Collections.Generic;
using GestureRecognizer;

public class MultiStrokeBehaviour : MonoBehaviour {

    /// <summary>
    /// Disable or enable multi stroke recognition
    /// </summary>
	public bool isEnabled = true;

    /// <summary>
    /// Overwrite the XML file in persistent data path
    /// </summary>
	public bool forceCopy = false;

    /// <summary>
    /// The name of the multi stroke library to load. Do NOT include '.xml'
    /// </summary>
	public string libraryToLoad = "multistroke_shapes";

    /// <summary>
    /// A new point will be placed if it is this further than the last point.
    /// </summary>
	public float distanceBetweenPoints = 10f;

    /// <summary>
    /// Minimum amount of points required to recognize a multistroke.
    /// </summary>
	public int minimumPointsToRecognize = 10;

    /// <summary>
    /// Material for the line renderer.
    /// </summary>
	public Material lineMaterial;

    /// <summary>
    /// Start thickness of the multi stroke.
    /// </summary>
	public float startThickness = 0.5f;

    /// <summary>
    /// End thickness of the multi stroke.
    /// </summary>
	public float endThickness = 0.05f;

    /// <summary>
    /// Start color of the multi stroke.
    /// </summary>
	public Color startColor = Color.red;

    /// <summary>
    /// End color of the multi stroke.
    /// </summary>
	public Color endColor = Color.white;

    /// <summary>
    /// Limits gesture drawing to a specific area
    /// </summary>
    public GestureLimitType gestureLimitType;

    /// <summary>
    /// RectTransform to limit gesture
    /// </summary>
    public RectTransform gestureLimitRectBounds;

    /// <summary>
    /// Rect of the gestureLimitRectBounds
    /// </summary>
    Rect gestureLimitRect;

    /// <summary>
    /// Parent canvas of RectTransform to limit gesture.
    /// Set the pivot to bottom-left corner
    /// </summary>
    Canvas parentCanvas;

    /// <summary>
    /// Current platform.
    /// </summary>
    RuntimePlatform platform;

    /// <summary>
    /// Line renderer component.
    /// </summary>
    LineRenderer currentStrokeRenderer;

    /// <summary>
    /// The position of the point on the screen.
    /// </summary>
    Vector3 virtualKeyPosition = Vector2.zero;

    /// <summary>
    /// A new point.
    /// </summary>
    Vector2 point = Vector2.zero;

    /// <summary>
    /// Last added point.
    /// </summary>
    Vector2 lastPoint = Vector2.zero;

    /// <summary>
    /// Vertex count of the line renderer.
    /// </summary>
    int vertexCount = 0;

    /// <summary>
    /// Last stroke's ID.
    /// </summary>
    int lastStrokeID = -1;

    /// <summary>
    /// Loaded multiStroke library.
    /// </summary>
    MultiStrokeLibrary ml;

    /// <summary>
    /// Captured points
    /// </summary>
    List<MultiStrokePoint> multiStrokePoints = new List<MultiStrokePoint>();

    /// <summary>
    /// Recognized multiStroke.
    /// </summary>
    MultiStroke multiStroke;

    /// <summary>
    /// Result.
    /// </summary>
    Result result;

    /// <summary>
    /// Strokes.
    /// </summary>
    List<GameObject> strokes = new List<GameObject>();

	/// <summary>
	/// This is the event to subscribe to.
	/// </summary>
	/// <param name="r">Result of the recognition</param>
	public delegate void MultiStrokeEvent(Result r);
	public static event MultiStrokeEvent OnRecognition;


	// Get the platform
	void Awake() {
		platform = Application.platform;
	}


	// Load the library.
	void Start() {
		ml = new MultiStrokeLibrary(libraryToLoad, forceCopy);
		strokes = new List<GameObject>();

        if (gestureLimitType == GestureLimitType.RectBoundsClamp) {
            parentCanvas = gestureLimitRectBounds.GetComponentInParent<Canvas>();
            gestureLimitRect = RectTransformUtility.PixelAdjustRect(gestureLimitRectBounds, parentCanvas);
            gestureLimitRect.position += new Vector2(gestureLimitRectBounds.position.x, gestureLimitRectBounds.position.y);
        }
    }


	// Track user input and fire OnRecognition event when necessary.
	void Update() {

		// Track user input if GestureRecognition is enabled.
		if (isEnabled) {

			// If it is a touch device, get the touch position
			// if it is not, get the mouse position
			if (Utility.IsTouchDevice()) {
				if (Input.touchCount > 0) {
					virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
				}
			} else {
				if (Input.GetMouseButton(0)) {
					virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
				}
			}

			// It is not necessary to track the touch from this point on,
			// because it is already registered, and GetMouseButton event 
			// also fires on touch devices
			if (Input.GetMouseButtonDown(0)) {
				point = Vector2.zero;
				lastPoint = Vector2.zero;
				AddStroke();
			}

			// It is not necessary to track the touch from this point on,
			// because it is already registered, and GetMouseButton event 
			// also fires on touch devices
			if (Input.GetMouseButton(0)) {

                switch (gestureLimitType) {

                    case GestureLimitType.None:
                        RegisterPoint();
                        break;

                    case GestureLimitType.RectBoundsIgnore:
                        if (RectTransformUtility.RectangleContainsScreenPoint(gestureLimitRectBounds, virtualKeyPosition, null)) {
                            RegisterPoint();
                        }
                        break;

                    case GestureLimitType.RectBoundsClamp:
                        virtualKeyPosition = Utility.ClampPointToRect(virtualKeyPosition, gestureLimitRect);
                        RegisterPoint();
                        break;
                }

            }

            // Capture the multi stroke, recognize it, fire the recognition event,
            // and clear the multi stroke from the screen.
            if (Input.GetMouseButtonDown(1)) {

				if (multiStrokePoints.Count > minimumPointsToRecognize) {
					multiStroke = new MultiStroke(multiStrokePoints.ToArray());
					result = multiStroke.Recognize(ml);

					if (OnRecognition != null) {
						OnRecognition(result);
					}
				}

				ClearGesture();
			}
		}

	}


    /// <summary>
    /// Register this point only if the point list is empty or current point
    /// is far enough than the last point. This ensures that the multi stroke looks
    /// good on the screen. Moreover, it is good to not overpopulate the screen
    /// with so much points.
    /// </summary>
    void RegisterPoint() {
        point = new Vector2(virtualKeyPosition.x, -virtualKeyPosition.y);
        
        if (Vector2.Distance(point, lastPoint) > distanceBetweenPoints) {
            multiStrokePoints.Add(new MultiStrokePoint(point.x, point.y, lastStrokeID));
            lastPoint = point;

            currentStrokeRenderer.SetVertexCount(++vertexCount);
            currentStrokeRenderer.SetPosition(vertexCount - 1, Utility.WorldCoordinateForGesturePoint(virtualKeyPosition));
        }
    }


    /// <summary>
    /// Remove the multi stroke from the screen.
    /// </summary>
    void ClearGesture() {

		vertexCount = 0;
		lastStrokeID = -1;
		multiStrokePoints.Clear();

		for (int i = strokes.Count - 1; i >= 0; i--) {
			Destroy(strokes[i]);
		}

		strokes.Clear();
	}


	/// <summary>
	/// Add a new stroke to multi stroke
	/// </summary>
	void AddStroke() {
		lastStrokeID++;
		vertexCount = 0;
		GameObject newStroke = new GameObject();
		newStroke.name = "Stroke " + lastStrokeID;
		newStroke.transform.parent = this.transform;
		currentStrokeRenderer = newStroke.AddComponent<LineRenderer>();
		currentStrokeRenderer.SetVertexCount(0);
		currentStrokeRenderer.material = lineMaterial;
		currentStrokeRenderer.SetColors(startColor, endColor);
		currentStrokeRenderer.SetWidth(startThickness, endThickness);
		strokes.Add(newStroke);
	}
}
