using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using TanksRebirth.Graphics;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Utilities;

namespace TanksRebirth.GameContent
{
    public class Particle
    {
        public Texture2D Texture;

        public Vector3 position;

        public Color color = Color.White;

        public float Roll;
        public float Pitch;
        public float Yaw;

        public float TextureScale = int.MinValue;
        public float TextureRotation;

        public Vector2 TextureOrigin;

        public float Opacity = 1f;

        public readonly int id;

        public bool FaceTowardsMe;

        public bool is2d;

        public Action<Particle> UniqueBehavior;

        public bool isAddative = true;

        public int lifeTime;

        // NOTE: scale.X is used for 2d scaling.
        public Vector3 Scale;

        public float addativeLightPower;

        /* TODO:
         * Model alpha must be set!
         * 
         * billboard from 'position' to the camera.
         */

        internal Particle(Vector3 position)
        {
            this.position = position;
            int index = Array.IndexOf(ParticleSystem.CurrentParticles, ParticleSystem.CurrentParticles.First(particle => particle == null));

            id = index;

            ParticleSystem.CurrentParticles[index] = this;
        }

        public void Update()
        {
            UniqueBehavior?.Invoke(this);
            lifeTime++;
        }

        public static BasicEffect effect = new(TankGame.Instance.GraphicsDevice);

        internal void Render()
        {
            if (!is2d)
            {
                effect.World = Matrix.CreateScale(Scale) * Matrix.CreateRotationX(Roll) * Matrix.CreateRotationY(Pitch) * Matrix.CreateRotationZ(Yaw) * Matrix.CreateTranslation(position);
                effect.View = TankGame.GameView;
                effect.Projection = TankGame.GameProjection;
                effect.TextureEnabled = true;
                effect.Texture = Texture;
                effect.AmbientLightColor = color.ToVector3() * GameHandler.GameLight.Brightness;
                effect.DiffuseColor = color.ToVector3() * GameHandler.GameLight.Brightness;
                effect.FogColor = color.ToVector3() * GameHandler.GameLight.Brightness;
                effect.EmissiveColor = color.ToVector3() * GameHandler.GameLight.Brightness;
                effect.SpecularColor = color.ToVector3() * GameHandler.GameLight.Brightness;

                effect.Alpha = Opacity;

                effect.SetDefaultGameLighting_IngameEntities(addativeLightPower);

                effect.FogEnabled = false;

                TankGame.spriteBatch.End();
                TankGame.spriteBatch.Begin(SpriteSortMode.Deferred, isAddative ? BlendState.Additive : BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.DepthRead, RasterizerState.CullNone, effect);
                TankGame.spriteBatch.Draw(Texture, Vector2.Zero, null, color * Opacity, TextureRotation, TextureOrigin != default ? TextureOrigin : Texture.Size() / 2, TextureScale == int.MinValue ? Scale.X : TextureScale, default, default);
            }
            else
            {
                TankGame.spriteBatch.End();
                TankGame.spriteBatch.Begin(SpriteSortMode.Deferred, isAddative ? BlendState.Additive : BlendState.NonPremultiplied);
                TankGame.spriteBatch.Draw(Texture, GeometryUtils.ConvertWorldToScreen(Vector3.Zero, Matrix.CreateTranslation(position), TankGame.GameView, TankGame.GameProjection), null, color * Opacity, TextureRotation, TextureOrigin != default ? TextureOrigin : Texture.Size() / 2, TextureScale == int.MinValue ? Scale.X : TextureScale, default, default);
            }
            TankGame.spriteBatch.End();
            TankGame.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        public void Destroy()
        {
            ParticleSystem.CurrentParticles[id] = null;
        }
    }
}