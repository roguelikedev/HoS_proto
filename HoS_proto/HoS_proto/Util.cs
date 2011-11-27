using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HoS_proto;

namespace Util
{
    public class TriangleDrawer
    {
        VertexPositionColor[] vertices = new VertexPositionColor[6000];

        int ndx = 0;
        GraphicsDevice device;
        const int VERTS_PER_TRIANGLE = 3;
        BasicEffect basicEffect;

        public TriangleDrawer(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            device = graphicsDevice;

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;

            // projection uses CreateOrthographicOffCenter to create 2d projection
            // matrix with 0,0 in the upper left.
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
                (0, graphicsDevice.Viewport.Width,
                graphicsDevice.Viewport.Height, 0,
                0, 1);
        }

        public void Begin()
        {
            // tell our basic effect to begin.
            basicEffect.CurrentTechnique.Passes[0].Apply();
        }

        public void AddVertex(Vector2 vertex)
        {
            vertices[ndx].Position = new Vector3(vertex, 0);
            vertices[ndx].Color = Color.White;

            ndx++;
        }

        public void End()
        {
            if (ndx == 0) return;

            // submit the draw call to the graphics card
            device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0,
                ndx / VERTS_PER_TRIANGLE);

            ndx = 0;
        }
    }
}
