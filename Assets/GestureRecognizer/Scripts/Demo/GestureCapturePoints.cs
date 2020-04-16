using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GestureRecognizer;
using UnityEngine.UI;

public class GestureCapturePoints : MonoBehaviour {

	[Tooltip("Disable or enable gesture recognition")]
	public bool isEnabled = true;

	[Tooltip("Overwrite the XML file in persistent data path")]
	public bool forceCopy = false;

	[Tooltip("Use the faster algorithm, however default (slower) algorithm has a better scoring system")]
	public bool UseProtractor = false;

	[Tooltip("The name of the gesture library to load. Do NOT include '.xml'")]
	public string libraryToLoad = "shapes";

	[Tooltip("A new point will be placed if it is this further than the last point.")]
	public float distanceBetweenPoints = 10f;

	[Tooltip("Minimum amount of points required to recognize a gesture.")]
	public int minimumPointsToRecognize = 10;

	[Tooltip("Material for the line renderer.")]
	public Material lineMaterial;

	[Tooltip("Start thickness of the gesture.")]
	public float startThickness = 0.25f;

	[Tooltip("End thickness of the gesture.")]
	public float endThickness = 0.05f;

    [Tooltip("Start color of the gesture.")]
    public Color startColor = new Color(0, 0.67f, 1f);

	[Tooltip("End color of the gesture.")]
	public Color endColor = new Color(0.48f, 0.83f, 1f);

    [Tooltip("The RectTransform that limits the gesture")]
    public RectTransform drawArea;

    [Tooltip("The InputField that will hold the new gesture name")]
    public InputField newGestureName;

    [Tooltip("Messages will show up here")]
    public Text messageArea;

	// Current platform.
	RuntimePlatform platform;

	// Line renderer component.
	LineRenderer gestureRenderer;

	// The position of the point on the screen.
	Vector3 virtualKeyPosition = Vector2.zero;

	// A new point.
	Vector2 point;

	// List of points that form the gesture.
	List<Vector2> points = new List<Vector2>();

	// Vertex count of the line renderer.
	int vertexCount = 0;

	// Loaded gesture library.
	GestureLibrary gl;

	// Recognized gesture.
	Gesture gesture;

	// Result.
	Result result;


	// Get the platform and apply attributes to line renderer.
	void Awake() {
		platform = Application.platform;
		gestureRenderer = gameObject.AddComponent<LineRenderer>();
		gestureRenderer.SetVertexCount(0);
		gestureRenderer.material = lineMaterial;
		gestureRenderer.SetColors(startColor, endColor);
		gestureRenderer.SetWidth(startThickness, endThickness);
	}


	// Load the library.
	void Start() {
		gl = new GestureLibrary(libraryToLoad, forceCopy);
	}


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

			if (RectTransformUtility.RectangleContainsScreenPoint(drawArea, virtualKeyPosition, Camera.main)) {

				if (Input.GetMouseButtonDown(0)) {
					ClearGesture();
				}

				// It is not necessary to track the touch from this point on,
				// because it is already registered, and GetMouseButton event 
				// also fires on touch devices
				if (Input.GetMouseButton(0)) {

					point = new Vector2(virtualKeyPosition.x, -virtualKeyPosition.y);

					// Register this point only if the point list is empty or current point
					// is far enough than the last point. This ensures that the gesture looks
					// good on the screen. Moreover, it is good to not overpopulate the screen
					// with so much points.
					if (points.Count == 0 ||
						(points.Count > 0 && Vector2.Distance(point, points[points.Count - 1]) > distanceBetweenPoints)) {
						points.Add(point);

						gestureRenderer.SetVertexCount(++vertexCount);
						gestureRenderer.SetPosition(vertexCount - 1, Utility.WorldCoordinateForGesturePoint(virtualKeyPosition));
					}

				}

				// Capture the gesture, recognize it, fire the recognition event,
				// and clear the gesture from the screen.
				if (Input.GetMouseButtonUp(0)) {

					if (points.Count > minimumPointsToRecognize) {
						gesture = new Gesture(points);
						result = gesture.Recognize(gl, UseProtractor);
                        SetMessage("Gesture is recognized as <color=#ff0000>'" + result.Name + "'</color> with a score of " + result.Score);
					}

				} 
			}
		}

    }


    /// <summary>
    /// Adds a gesture to the library
    /// </summary>
    public void AddGesture() {
        Gesture newGesture = new Gesture(points, newGestureName.text);
        gl.AddGesture(newGesture);
        SetMessage(newGestureName.text + " has been added to the library");
    }


    /// <summary>
    /// Shows a message at the bottom of the screen
    /// </summary>
    /// <param name="text"></param>
    public void SetMessage(string text) {
        messageArea.text = text;
    }


	/// <summary>
	/// Remove the gesture from the screen.
	/// </summary>
	void ClearGesture() {
		points.Clear();
		gestureRenderer.SetVertexCount(0);
		vertexCount = 0;
	}
}
