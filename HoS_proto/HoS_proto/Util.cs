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
        #region fields
        VertexPositionColor[] vertices = new VertexPositionColor[6000];

        int ndx = 0;
        GraphicsDevice device;
        const int VERTS_PER_TRIANGLE = 3;
        BasicEffect basicEffect;
        #endregion

        #region OpenGL overhead
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
        #endregion

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
        public const int BORDER_DEPTH = 16;
        /// <summary> MULTIPLE GOTCHA ALERT:
        /// if Draw(x,y,w,h) gets Menu.FLEXIBLE as an arg or Draw() is called
        /// after DrawBox has been assigned a Lambda which evaluates to a
        /// rectangle having Menu.FLEXIBLE as one or more of x,y,w,h, the
        /// relevant dimension or axis of origin will be altered as necessary
        /// to make the menu text fit.
        /// 
        /// if a rectangle's x or y values are Menu.FLEXIBLE, width or height
        /// (respectively) are assumed to be /positions/ representing rightmost
        /// or bottommost (respectively) pixel of that rectangle.
        /// </summary>
        public const int FLEXIBLE = int.MinValue;

        class MenuItem
        {
            public Action Lambda;
            public Color color = STANDARD;
            public readonly Menu container;
            public MenuItem(string s, Action L, Menu c) { rawText = s; Lambda = L; container = c; }
            
            readonly string rawText;
            Point RawTextSize
            {
                get { return new Point((int)Engine.Font.MeasureString(rawText).X
                                     , (int)Engine.Font.MeasureString(rawText).Y); }
            }
            List<string> lines = new List<string>();

            public override string ToString() { return lines.Count > 1? string.Join("\n", lines) : rawText; }
            public static implicit operator string(MenuItem mi) { return mi.ToString(); }

            /// <summary> in pixels. </summary>
            public int Height { get { return RawTextSize.Y * lines.Count; } }

            int width = -1;
            /// <summary> in pixels. </summary>
            public int Width
            {
                get { return width == -1? RawTextSize.X : width; }
                set
                {
                    width = value;
                    var words = rawText.Split();
                    var currentLine = "";
                    foreach (var currentWord in words)
                    {
                        if (Engine.Font.MeasureString(currentLine + " " + currentWord).X > width)
                        {
                            lines.Add(currentLine);
                            currentLine = "";
                        }
                        currentLine += currentWord;
                    }
                    lines.Add(currentLine);
                }
            }

            public void Draw(int xOrigin, int yOrigin)
            {
                Engine.DrawAtScreen("lozenge", xOrigin - BORDER_DEPTH, yOrigin
                                             , Width + BORDER_DEPTH * 2, Height, color);
                Engine.WriteAtScreen(this, xOrigin, yOrigin, 1);
            }
        }
        List<MenuItem> contents = new List<MenuItem>();
        int activeIndex = -1;

        /// <summary> assigning to this permits use of parameterless Draw().
        /// see also Menu.FLEXIBLE. </summary>
        public Func<Rectangle> DrawBox = null;
        public int MaxLineLength
        {
            get
            {
                var rval = 0;
                contents.ForEach(mi => rval = Math.Max(rval, mi.Width));
                return rval;
            }
        }

        #region obligatory data structure operations
        public void Add(string name, Action Lambda)
        {
            contents.Add(new MenuItem(name, Lambda, this));
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
        #endregion

        public void Draw()
        {
            if (DrawBox == null) throw new Exception("cannot use parameterless Draw() without knowing where.");
            var dbox = DrawBox();
            if (dbox.X == FLEXIBLE)
            {
                Debug.Assert(dbox.Width != FLEXIBLE);
                dbox.X = dbox.Width - MaxLineLength;
                dbox.Width = MaxLineLength;
            }
            if (dbox.X < 0)
            {
                var underlap = dbox.X;
                dbox.X = 0;
                dbox.Width += underlap;
                contents.ForEach(mi => mi.Width += underlap);
            }

            Draw(dbox.X, dbox.Y, dbox.Width, dbox.Height);
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
                if (height == FLEXIBLE) height = Size("|").Y;
                if (width == FLEXIBLE)
                {
                    contents.ForEach(mi => width = Math.Max(width, mi.Width));
                }
            }
            #endregion

            //height /= contents.Count;
            contents.ForEach(mi =>
            {
                mi.Draw(xOrigin, yOrigin);
                yOrigin += mi.Height;
            });
        }
    }
}
