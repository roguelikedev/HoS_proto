using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HoS_proto;
using System.Collections.Generic;
using System.Diagnostics;

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
            vertices[ndx].Color = Color.Black;

            ndx++;
        }

        public void End()
        {
            if (ndx == 0) return;

            //Comparison<VertexPositionColor> RightSort = (a, b) =>
            //    {
            //        if (a.Position.X != b.Position.X)
            //        {
            //            return (int)(a.Position.X - b.Position.X);
            //        }
            //        else return (int)(a.Position.Y - b.Position.Y);
            //    };


            //for (var lcv = 0; lcv < ndx; lcv += VERTS_PER_TRIANGLE)
            //{
            //    var tmp = new List<VertexPositionColor>(vertices).GetRange(lcv, VERTS_PER_TRIANGLE);

            //    tmp.Sort(RightSort);
            //    tmp.ToArray().CopyTo(vertices, lcv);
            //}

            // submit the draw call to the graphics card
            device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0,
                ndx / VERTS_PER_TRIANGLE);

            ndx = 0;
        }
    }

    public class Menu
    {
        static readonly Color STANDARD = Color.CornflowerBlue, HOVERING = Color.GreenYellow;
        class MenuItem
        {
            public Action Lambda;
            public Color color = STANDARD;
            public string name;
            public override string ToString() { return name; }
            
            public MenuItem(string name, Action Lambda) { this.name = name; this.Lambda = Lambda; }
        }
        List<MenuItem> contents = new List<MenuItem>();
        int activeIndex = -1;
        public Func<Rectangle> DrawBox = null;

        public void Draw()
        {
            if (DrawBox == null) throw new Exception("cannot use parameterless Draw() without knowing where.");
            var dbox = DrawBox();
            Draw(dbox.X, dbox.Y, dbox.Width, dbox.Height);
        }

        public void Add(string name, Action Lambda)
        {
            contents.Add(new MenuItem(name, Lambda));
        }
        public void Expand(string name, Action Lambda)
        {


        }

        public void GoNext()
        {
            if (activeIndex != -1)
            {
                contents[activeIndex].color = STANDARD;
            }
            activeIndex++;
            Debug.Assert(activeIndex >= 0 && activeIndex <= contents.Count);

            if (activeIndex == contents.Count)
            {
                Debug.Assert(activeIndex != 0);
                activeIndex = 0;
            }

            contents[activeIndex].color = HOVERING;
        }
        public void GoPrev()
        {
            if (activeIndex != -1)
            {
                contents[activeIndex].color = STANDARD;
            }
            activeIndex--;
            Debug.Assert(activeIndex >= -1 && activeIndex < contents.Count - 1);

            if (activeIndex == -1) activeIndex = contents.Count - 1;

            if (activeIndex != -1) contents[activeIndex].color = HOVERING;
        }
        public void Select()
        {
            contents[activeIndex].Lambda();
        }

        /// <summary> all args are in pixels. </summary>
        /// <param name="width"> pass -1 to use default width. </param>
        /// <param name="height"> pass -1 to use default height. </param>
        public void Draw(int xOrigin, int yOrigin, int width, int height)
        {
            if (contents.Count == 0) return;
            #region assign defaults
            {
                Func<string, Point> Size = str =>
                {
                    var _rval = Engine.Font.MeasureString(str);
                    return new Point((int)_rval.X, (int)_rval.Y);
                };
                if (height == -1) height = Size("|").Y;
                if (width == -1)
                {
                    contents.ForEach(mi => width = Math.Max(width, Size(mi.name).X));
                }
            }
            if (height == -1 || width == -1) throw new Exception("incomplete case analysis.");
            #endregion

            //height /= contents.Count;
            contents.ForEach(mi =>
            {
                var nudge = 16;
                Engine.DrawAtScreen("lozenge", xOrigin - nudge, yOrigin, width + nudge * 2, height, mi.color);
                Engine.WriteAtScreen(mi.name, xOrigin, yOrigin, 1);
                yOrigin += height;
            });
        }

        public void Draw(int xOrigin, int yOrigin)
        {
            Draw(xOrigin, yOrigin, -1, -1);
        }
    }
}
