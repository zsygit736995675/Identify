using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GestureRecognizer;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System;

public class MultiStrokeCapturePoints : MonoBehaviour {

	[Tooltip("Disable or enable gesture recognition")]
	public bool isEnabled = true;

	[Tooltip("Overwrite the XML file in persistent data path")]
	public bool forceCopy = false;

	[Tooltip("The name of the gesture library to load. Do NOT include '.xml'")]
	public string libraryToLoad = "multistroke_shapes";

	[Tooltip("A new point will be placed if it is this further than the last point.")]
	public float distanceBetweenPoints = 10f;

	[Tooltip("Minimum amount of points required to recognize a multistroke.")]
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
    public string newMultiStrokeName;

    [Tooltip("Messages will show up here")]
    public Text messageArea;

    // Current platform.
    RuntimePlatform platform;

	// Line renderer component.
	LineRenderer currentStrokeRenderer;

	// The position of the point on the screen.
	Vector3 virtualKeyPosition = Vector2.zero;

	// A new point.
	Vector2 point = Vector2.zero;

	// Last added point.
	Vector2 lastPoint = Vector2.zero;

	// Vertex count of the line renderer.
	int vertexCount = 0;

	// Last stroke's ID.
	int lastStrokeID = -1;

	// Loaded multiStroke library.
	MultiStrokeLibrary ml;

	// Captured points
	List<MultiStrokePoint> multiStrokePoints;

	// Recognized multiStroke.
	MultiStroke multiStroke;

	// Result.
	Result result;

	// Strokes.
	List<GameObject> strokes;

	// If a multistroke is recognized, we will clear strokes on the screen OnMouseButtonDown
	bool isRecognized;


	// Get the platform.
	void Awake() {
		platform = Application.platform;
	}


	// Load the library.
	void Start() {
		ml = new MultiStrokeLibrary(libraryToLoad, forceCopy);
		strokes = new List<GameObject>();
		multiStrokePoints = new List<MultiStrokePoint>();
	}


    void Update() {

		if (isEnabled) {

			// If it is a touch device, get the touch position
			// if it is not, get the mouse position
			if (GestureRecognizer.Utility.IsTouchDevice()) {
				if (Input.touchCount > 0) {
					virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
				}
			} else {
				if (Input.GetMouseButton(0)) {
					virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
				}
			}

			if (RectTransformUtility.RectangleContainsScreenPoint(drawArea, virtualKeyPosition, Camera.main)) {
				// It is not necessary to track the touch from this point on,
				// because it is already registered, and GetMouseButton event 
				// also fires on touch devices
				if (Input.GetMouseButtonDown(0)) {

					if (isRecognized) {
						ClearMultiStroke();
					}

					point = Vector2.zero;
					lastPoint = Vector2.zero;
					AddStroke();
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
					if (Vector2.Distance(point, lastPoint) > distanceBetweenPoints) {
						multiStrokePoints.Add(new MultiStrokePoint(point, lastStrokeID));
						lastPoint = point;

						currentStrokeRenderer.SetVertexCount(++vertexCount);
						currentStrokeRenderer.SetPosition(vertexCount - 1, GestureRecognizer.Utility.WorldCoordinateForGesturePoint(virtualKeyPosition));
					}

				}

				// Capture the gesture, recognize it, fire the recognition event,
				// and clear the gesture from the screen.
				if (Input.GetMouseButtonDown(1)) {
					Recognize();
				} 
			}
		}

    }


    /// <summary>
    /// Add multistroke to the library
    /// </summary>
    public void AddMultiStroke() {
        multiStroke = new MultiStroke(multiStrokePoints.ToArray(), newMultiStrokeName);
        ml.AddMultiStroke(multiStroke);
        SetMessage(newMultiStrokeName + " has been added to the library");

        id = System.Guid.NewGuid().ToString("N");
        newMultiStrokeName = text + ":" + id;
        SetMessage(newMultiStrokeName);
    }
    

    /// <summary>
    /// Recognize drawn gesture
    /// </summary>
	public void Recognize() {

		if (multiStrokePoints.Count > minimumPointsToRecognize) {
			multiStroke = new MultiStroke(multiStrokePoints.ToArray());

			result = multiStroke.Recognize(ml);
			isRecognized = true;
            string[] names = result.Name.Split(':');
            SetMessage("MultiStroke is recognized as <color=#ff0000>'" + names[0] + "'</color> with a score of " + result.Score);
        }
	}


	/// <summary>
	/// Remove the gesture from the screen.
	/// </summary>
	public void ClearMultiStroke() {

		isRecognized = false;
		vertexCount = 0;
		lastStrokeID = -1;
		multiStrokePoints.Clear();

		for (int i = strokes.Count - 1; i >= 0; i--) {
			Destroy(strokes[i]);
		}

		strokes.Clear();
        SetMessage("");
	}


	/// <summary>
	/// Add a new stroke to gesture
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


    /// <summary>
    /// Shows a message at the bottom of the screen
    /// </summary>
    /// <param name="text"></param>
    public void SetMessage(string text) {
        messageArea.text = text;
    }

    string id ;
    string text;
    public void ToogleChange(Toggle toggle)
    {
        if (toggle.isOn)
        {
            text = toggle.transform.GetComponentInChildren<Text>().text;
            id=System.Guid.NewGuid().ToString("N");
            newMultiStrokeName = text + ":" + id;
            SetMessage(newMultiStrokeName);
        }
    }

    public void Save()
    {
#if !UNITY_WEBPLAYER && !UNITY_EDITOR
       byte[] bytes = File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "multistroke_shapes.xml"));
#else
        byte[] bytes = File.ReadAllBytes(Path.Combine(Path.Combine(Application.dataPath, "GestureRecognizer/Resources"), "multistroke_shapes.xml"));
#endif
        StartCoroutine( UploadFile("http:///upload", bytes));
    }

    private IEnumerator UploadFile(string url, byte[] bytes)
    {
        WWWForm form = new WWWForm();

        string fileName =  System.Guid.NewGuid().ToString("N")+ GetTimeStamp()+"multistroke_shapes.xml";

        form.AddBinaryData("file", bytes, fileName, "");

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                SetMessage("error:"+ www.error);
            }
            else
            {
               SetMessage("upload complete!");
            }
        }
    }

    public static long GetTimeStamp(bool bflag = true)
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long ret;
        if (bflag)
            ret = Convert.ToInt64(ts.TotalSeconds);
        else
            ret = Convert.ToInt64(ts.TotalMilliseconds);
        return ret;
    }

}
