using UnityEngine;
using System.Collections;
using GestureRecognizer;
using UnityEngine.UI;

// !!!: Drag & drop a MultiStrokeRecognizer prefab on to the scene first from Prefabs folder!!!
public class MultiStrokeDemoEvent : MonoBehaviour {

    [Tooltip("Messages will show up here")]
    public Text messageArea;

    // Subscribe your own method to OnRecognition event 
    void OnEnable() {
        MultiStrokeBehaviour.OnRecognition += OnMultiStrokeRecognition;
    }

    // Unsubscribe when this game object or monobehaviour is disabled.
    // If you don't unsubscribe, this will give an error.
    void OnDisable() {
		MultiStrokeBehaviour.OnRecognition -= OnMultiStrokeRecognition;
    }

    // Unsubscribe when this game object or monobehaviour is destroyed.
    // If you don't unsubscribe, this will give an error.
    void OnDestroy() {
		MultiStrokeBehaviour.OnRecognition -= OnMultiStrokeRecognition;
    }

    /// <summary>
    /// The method to be called on recognition event
    /// </summary>
    /// <param name="r">Recognition result</param>
    /// 
    /// <remarks>
    /// Implement your own method here. This method will be called by MultiStrokeBehaviour
    /// automatically when a multi stroke is recognized. You can write your own script
    /// in this method (kill enemies, shoot a fireball, or cast some spell etc.)
    /// This method's signature should match the signature of OnRecognition event,
    /// so your method should always have one parametre with the type of Result. Example:
    /// 
    /// void MyMethod(Result gestureResult) {
    ///     kill enemy,
    ///     shoot fireball,
    ///     cast spell etc.
    /// }
    /// 
    /// You can decide what to do depending on the name of the multi stroke and its score.
    /// For example, let's say, if the player draws the letter of "e" (let's name the 
    /// gesture "Fireball"), shoot a fireball:
    /// 
    /// void MagicHandler(Result magicMultiStroke) {
    /// 
    ///    if (magicMultiStroke.Name == "Fireball") {
    ///        Instantiate(fireball, transform.position, Quaternion.identity);
    ///    }
    /// 
    /// }
    /// 
    /// !: You can name this method whatever you want, but you should use the same name
    /// when subscribing and unsubscribing. If your method's name is MagicHandler like above,
    /// then:
    /// 
    /// void OnEnable() {
    ///   MultiStrokeBehaviour.OnRecognition += MagicHandler;
    /// }
    /// </remarks>
    void OnMultiStrokeRecognition(Result r) {
        SetMessage("Multistroke is recognized as <color=#ff0000>'" + r.Name + "'</color> with a score of " + r.Score);
    }


    /// <summary>
    /// Shows a message at the bottom of the screen
    /// </summary>
    /// <param name="text">Text to show</param>
    public void SetMessage(string text) {
        messageArea.text = text;
    }
}
