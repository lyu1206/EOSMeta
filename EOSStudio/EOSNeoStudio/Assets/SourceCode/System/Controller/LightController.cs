using UnityEngine;
using UnityEngine.Rendering;

namespace Eos.Ore
{
	[ExecuteInEditMode]
	public class LightController : OreBase
	{

		[SerializeField]
		private Color _ambientSkyColor;
		[SerializeField]
		private Color _ambientEquatorColor;
		[SerializeField]
		private Color _ambientGroundColor;
		[SerializeField]
		private AmbientMode _ambientMode;
		
		
		private Color _originalAmbientSkyColor;
		private Color _originalAmbientEquatorColor;
		private Color _originalAmbientGroundColor;
		private AmbientMode _originalAmbientMode;
		
		private void OnEnable()
		{
			SetColor();
		}

		private void SetColor()
		{
			_originalAmbientSkyColor = RenderSettings.ambientSkyColor;
			_originalAmbientEquatorColor = RenderSettings.ambientEquatorColor;
			_originalAmbientGroundColor = RenderSettings.ambientGroundColor;
			_originalAmbientMode = RenderSettings.ambientMode;

			RenderSettings.ambientSkyColor = _ambientSkyColor;
			RenderSettings.ambientEquatorColor = _ambientEquatorColor;
			RenderSettings.ambientGroundColor = _ambientGroundColor;
			RenderSettings.ambientMode = _ambientMode;
		}

		private void OnDisable()
		{
			RollbackColor();
		}

		private void RollbackColor()
		{
			RenderSettings.ambientSkyColor = _originalAmbientSkyColor;
			RenderSettings.ambientEquatorColor = _originalAmbientEquatorColor;
			RenderSettings.ambientGroundColor = _originalAmbientGroundColor;
			RenderSettings.ambientMode = _originalAmbientMode;
		}

		public void UpdateColor()
		{
			SetColor();
		}
	}
}