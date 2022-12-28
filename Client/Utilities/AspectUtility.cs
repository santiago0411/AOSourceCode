using AOClient.Core.Utils;
using UnityEngine;

namespace AOClient.Utilities
{
	public class AspectUtility : MonoBehaviour
	{
		[SerializeField] private float _wantedAspectRatio = 1.3333333f;
		private static float wantedAspectRatio;
		private static Camera cam;
		private static Camera backgroundCam;

		private void Awake()
		{
			cam = GetComponent<Camera>();
			if (!cam)
			{
				cam = Camera.main;
			}
			if (!cam)
			{
				DebugLogger.Error("No camera available");
				return;
			}
			wantedAspectRatio = _wantedAspectRatio;
			SetCamera();
		}

		private static void SetCamera()
		{
			float currentAspectRatio = (float)Screen.width / Screen.height;
			// If the current aspect ratio is already approximately equal to the desired aspect ratio,
			// use a full-screen Rect (in case it was set to something else previously)
			if ((int)(currentAspectRatio * 100) / 100.0f == (int)(wantedAspectRatio * 100) / 100.0f)
			{
				cam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
				if (backgroundCam)
				{
					Destroy(backgroundCam.gameObject);
				}
				return;
			}
			// Pillarbox
			if (currentAspectRatio > wantedAspectRatio)
			{
				float inset = 1.0f - wantedAspectRatio / currentAspectRatio;
				cam.rect = new Rect(inset / 2, 0.0f, 1.0f - inset, 1.0f);
			}
			// Letterbox
			else
			{
				float inset = 1.0f - currentAspectRatio / wantedAspectRatio;
				cam.rect = new Rect(0.0f, inset / 2, 1.0f, 1.0f - inset);
			}
			if (!backgroundCam)
			{
				// Make a new camera behind the normal camera which displays black; otherwise the unused space is undefined
				backgroundCam = new GameObject("BackgroundCam", typeof(Camera)).GetComponent<Camera>();
				backgroundCam.depth = int.MinValue;
				backgroundCam.clearFlags = CameraClearFlags.SolidColor;
				backgroundCam.backgroundColor = Color.black;
				backgroundCam.cullingMask = 0;
			}
		}

		public static int ScreenHeight => (int)(Screen.height * cam.rect.height);

		public static int ScreenWidth => (int)(Screen.width * cam.rect.width);

		public static int XOffset => (int)(Screen.width * cam.rect.x);

		public static int YOffset => (int)(Screen.height * cam.rect.y);

		public static Rect ScreenRect => new Rect(cam.rect.x * Screen.width, cam.rect.y * Screen.height, cam.rect.width * Screen.width, cam.rect.height * Screen.height);

		public static Vector3 MousePosition
		{
			get
			{
				Vector3 mousePos = Input.mousePosition;
				mousePos.y -= (int)(cam.rect.y * Screen.height);
				mousePos.x -= (int)(cam.rect.x * Screen.width);
				return mousePos;
			}
		}

		public static Vector2 GUIMousePosition
		{
			get
			{
				Vector2 mousePos = Event.current.mousePosition;
				mousePos.y = Mathf.Clamp(mousePos.y, cam.rect.y * Screen.height, cam.rect.y * Screen.height + cam.rect.height * Screen.height);
				mousePos.x = Mathf.Clamp(mousePos.x, cam.rect.x * Screen.width, cam.rect.x * Screen.width + cam.rect.width * Screen.width);
				return mousePos;
			}
		}
	}
}