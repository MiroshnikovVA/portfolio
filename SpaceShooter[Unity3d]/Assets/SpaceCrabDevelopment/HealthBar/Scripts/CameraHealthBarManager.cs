using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;

namespace SpaceCrabDevelopment.HealthBar {

	[AddComponentMenu("SpaceCrabDevelopment/Camera/CameraHealthBarManager")]
	[RequireComponent(typeof(Camera))]
	public class CameraHealthBarManager : MonoBehaviour {

		[Tooltip("Список все юнитов, заполняется автоматически самими юнитами")]
		public System.Collections.Generic.List<HealthBar> AllUnits = new System.Collections.Generic.List<HealthBar>();

		Camera _camera;
		Colors _colors;

		static private CameraHealthBarManager _instance;
		static public CameraHealthBarManager Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<CameraHealthBarManager>();
				}
				return _instance;
			}
		}

		void Start() {
			_camera = GetComponent<Camera>();
			_colors = new Colors();
		}

		private class Colors {
			public Colors() {
				GreenColor = new Texture2D(1, 1);
				GreenColor.SetPixel(0, 0, Color.green);
				GreenColor.Apply();

				WhiteColor = new Texture2D(1, 1);
				WhiteColor.SetPixel(0, 0, Color.white);
				WhiteColor.Apply();

				WhiteColorAlfa = new Texture2D(1, 1);
				WhiteColorAlfa.SetPixel(0, 0, new Color(0.9f, 1f, 0.9f, 0.4f));
				WhiteColorAlfa.Apply();

				RedColor = new Texture2D(1, 1);
				RedColor.SetPixel(0, 0, Color.red);
				RedColor.Apply();

				RedColorAlfa = new Texture2D(1, 1);
				RedColorAlfa.SetPixel(0, 0, new Color(0.9f, 0.1f, 0f, 0.4f));
				RedColorAlfa.Apply();

				CyanColor = new Texture2D(1, 1);
				CyanColor.SetPixel(0, 0, Color.cyan);
				CyanColor.Apply();

				YellowColor = new Texture2D(1, 1);
				YellowColor.SetPixel(0, 0, Color.yellow);
				YellowColor.Apply();

				AlphaGreenColor = new Texture2D(1, 1);
				AlphaGreenColor.SetPixel(0, 0, new Color(0, 1f, 0, 0.1f));
				AlphaGreenColor.Apply();
			}
			public Texture2D GreenColor;
			public Texture2D WhiteColor;
			public Texture2D YellowColor;
			public Texture2D AlphaGreenColor;
			public Texture2D RedColor;
			public Texture2D CyanColor;
			public Texture2D WhiteColorAlfa;
			public Texture2D RedColorAlfa;

		}

		void OnGUI() {
			foreach (var unit in AllUnits) {
				if (unit.IsActive)	DrawUnitFrame(unit, false);
				else if (unit.IsRecentlyDamaged())DrawUnitFrame(unit, true);
			}
		}

		Rect _rect11 = new Rect(0, 0, 1, 1);

		void DrawHitpoints(float hp, float minX, float maxX, float minY, float resources) {
			maxX = Mathf.Min(GetComponent<Camera>().pixelRect.max.x, maxX);
			if (minX > GetComponent<Camera>().pixelRect.max.x) return;
			var color = _colors.GreenColor;
			if (hp < 0.666f) color = _colors.YellowColor;
			if (hp < 0.333f) color = _colors.RedColor;
			int hpY = 4;
			if (resources > 0) {
				GUI.DrawTextureWithTexCoords(new Rect(minX, minY - 4, (maxX - minX) * resources, 2), _colors.CyanColor, _rect11);
				hpY = 8;
			}
			GUI.DrawTextureWithTexCoords(new Rect(minX, minY - hpY, (maxX - minX) * hp, 2), color, _rect11);
		}
		void DrawrСorners(float minX, float maxX, float minY, float maxY, Texture frameColor) {

			const float cornerLength = 10;
			const float cornerWeight = 2;

			//Обрубаем справа рамку
			maxX = Mathf.Min(_camera.pixelRect.max.x, maxX);
			if (minX > _camera.pixelRect.max.x) return;

			//вверхняя полоска
			GUI.DrawTextureWithTexCoords(new Rect(minX, minY, cornerLength, cornerWeight), frameColor, _rect11);
			GUI.DrawTextureWithTexCoords(new Rect(maxX - cornerLength, minY, cornerLength, cornerWeight), frameColor, _rect11);

			//нижняя полоска
			GUI.DrawTextureWithTexCoords(new Rect(minX, maxY - cornerWeight, cornerLength, cornerWeight), frameColor, _rect11);
			GUI.DrawTextureWithTexCoords(new Rect(maxX - cornerLength, maxY - cornerWeight, cornerLength, cornerWeight), frameColor, _rect11);

			//левая полоска
			GUI.DrawTextureWithTexCoords(new Rect(minX, minY, cornerWeight, cornerLength), frameColor, _rect11);
			GUI.DrawTextureWithTexCoords(new Rect(minX, maxY - cornerLength, cornerWeight, cornerLength), frameColor, _rect11);

			//правая полоска
			GUI.DrawTextureWithTexCoords(new Rect(maxX - cornerWeight, minY, cornerWeight, cornerLength), frameColor, _rect11);
			GUI.DrawTextureWithTexCoords(new Rect(maxX - cornerWeight, maxY - cornerLength, cornerWeight, cornerLength), frameColor, _rect11);
		}
		void DrawUnitFrame(HealthBar unit, bool onlyHP) {
			if (unit.BoundaryPoints.Length > 0) {
				var ScrinPoint = this.GetComponent<Camera>().WorldToScreenPoint(unit.BoundaryPoints[0].transform.position);
				var y = GetComponent<Camera>().pixelHeight - ScrinPoint.y;
				var x = ScrinPoint.x;
				var minX = x - 8;
				var maxX = x + 8;
				var minY = y - 8;
				var maxY = y + 8;
				foreach (var p in unit.BoundaryPoints) {
					ScrinPoint = this.GetComponent<Camera>().WorldToScreenPoint(p.transform.position);
					y = GetComponent<Camera>().pixelHeight - ScrinPoint.y;
					x = ScrinPoint.x;
					if (x > maxX) maxX = x;
					else if (x < minX) minX = x;
					if (y > maxY) maxY = y;
					else if (y < minY) minY = y;
				}

				Texture2D color;
				if (!onlyHP) {
					color = !unit.IsEnemy ? _colors.WhiteColor : _colors.RedColor;
				}
				else {
					color = !unit.IsEnemy ? _colors.WhiteColorAlfa : _colors.RedColorAlfa;
				}
				DrawrСorners(minX, maxX, minY, maxY, color);
				DrawHitpoints(unit.Health, minX, maxX, minY, unit.Resource);
			}
		}
	}
}
