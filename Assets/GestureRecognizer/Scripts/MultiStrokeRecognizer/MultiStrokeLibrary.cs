using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using System.Globalization;

namespace GestureRecognizer {
	public class MultiStrokeLibrary {

		string libraryName;
		string libraryFilename;
		string persistentLibraryPath;
		string resourcesPath;
		string xmlContents;
		XmlDocument multiStrokeLibrary = new XmlDocument();
		List<MultiStroke> library = new List<MultiStroke>();

		public List<MultiStroke> Library { get { return library; } }


		public MultiStrokeLibrary(string libraryName, bool forceCopy = false) {
            this.libraryName = libraryName;
            this.libraryFilename = libraryName + ".xml";
            this.persistentLibraryPath = Path.Combine(Application.persistentDataPath, libraryFilename);
            this.resourcesPath = Path.Combine(Path.Combine(Application.dataPath, "GestureRecognizer/Resources"), libraryFilename);
            CopyToPersistentPath(forceCopy);
            LoadLibrary();
		}


		public void LoadLibrary() {

			// Uses the XML file in resources folder if it is webplayer or the editor.
			string xmlContents = "";
			string floatSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

#if !UNITY_WEBPLAYER && !UNITY_EDITOR
            xmlContents = FileTools.Read(persistentLibraryPath);
#else
			xmlContents = Resources.Load<TextAsset>(libraryName).text;
#endif

			multiStrokeLibrary.LoadXml(xmlContents);


			// Get "gesture" elements
			XmlNodeList xmlMultiStrokeList = multiStrokeLibrary.GetElementsByTagName("multistroke");

			// Parse "gesture" elements and add them to library
			foreach (XmlNode xmlMultiStrokeNode in xmlMultiStrokeList) {

				string multiStrokeName = xmlMultiStrokeNode.Attributes.GetNamedItem("name").Value;
				XmlNodeList xmlPoints = xmlMultiStrokeNode.ChildNodes;
				List<MultiStrokePoint> multiStrokePoints = new List<MultiStrokePoint>();

				foreach (XmlNode point in xmlPoints) {

					float x = (float)System.Convert.ToDouble(point.Attributes.GetNamedItem("x").Value.Replace(",", floatSeparator).Replace(".", floatSeparator));
					float y = (float)System.Convert.ToDouble(point.Attributes.GetNamedItem("y").Value.Replace(",", floatSeparator).Replace(".", floatSeparator));
					int multiStrokeID = (int)System.Convert.ToDouble(point.Attributes.GetNamedItem("id").Value);
					multiStrokePoints.Add(new MultiStrokePoint(x, y, multiStrokeID));
				}

				MultiStroke multiStroke = new MultiStroke(multiStrokePoints.ToArray(), multiStrokeName);
				library.Add(multiStroke);
			}
		}


		public bool AddMultiStroke(MultiStroke multiStroke) {

			// Create the xml node to add to the xml file
			XmlElement rootElement = multiStrokeLibrary.DocumentElement;
			XmlElement multiStrokeNode = multiStrokeLibrary.CreateElement("multistroke");
			multiStrokeNode.SetAttribute("name", multiStroke.Name);

			foreach (MultiStrokePoint m in multiStroke.Points) {
				XmlElement multiStrokePoint = multiStrokeLibrary.CreateElement("point");
				multiStrokePoint.SetAttribute("x", m.Point.x.ToString());
				multiStrokePoint.SetAttribute("y", m.Point.y.ToString());
				multiStrokePoint.SetAttribute("id", m.StrokeID.ToString());

				multiStrokeNode.AppendChild(multiStrokePoint);
			}

			// Append the node to xml file contents
			rootElement.AppendChild(multiStrokeNode);

			try {

				// Add the new gesture to the list of gestures
				this.Library.Add(multiStroke);

				// Save the file if it is not the web player, because
				// web player cannot have write permissions.
#if !UNITY_WEBPLAYER && !UNITY_EDITOR
                FileTools.Write(persistentLibraryPath, multiStrokeLibrary.OuterXml);
#elif !UNITY_WEBPLAYER && UNITY_EDITOR
				FileTools.Write(resourcesPath, multiStrokeLibrary.OuterXml);
#endif

				return true;
			} catch (Exception e) {
				Debug.Log(e.Message);
				return false;
			}

		}


		void CopyToPersistentPath(bool forceCopy) {

#if !UNITY_WEBPLAYER && !UNITY_EDITOR
            if (!FileTools.Exists(persistentLibraryPath) || (FileTools.Exists(persistentLibraryPath) && forceCopy)) {
                string fileContents = Resources.Load<TextAsset>(libraryName).text;
                FileTools.Write(persistentLibraryPath, fileContents);
            }
#endif

		}

	} 
}
