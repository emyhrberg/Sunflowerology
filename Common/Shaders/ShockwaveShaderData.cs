using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;

namespace Sunflowerology.Common.Shaders
{
    public class ShockwaveShaderData : ScreenShaderData
    {
        public const string ShockwaveSceneFilterName = "Sunflowerology:Shockwave";
        public Vector2 WaveCentre { get; set; }
        public Vector3 WaveParams { get; set; } = new Vector3(1f, 0.7f, 0.055f);

        public ShockwaveShaderData(Asset<Effect> shader, string passName)
            : base(shader, passName)
        {
        }

        public override void Apply()
        {
            Shader.Parameters["uWaveCentre"].SetValue(WaveCentre);
            Shader.Parameters["uWaveParams"].SetValue(WaveParams);
            base.Apply();
        }
    }
}
