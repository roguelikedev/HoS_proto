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
        class MenuItem
        {
            #region constructors
            public MenuItem(string s, Action L)
            {
                rawText = s; Lambda = L;
                lines.Add(rawText);
            }
            public MenuItem(string s, Action L, Color c) : this(s, L) { color = c; }
            #endregion
            #region fields
            public Action Lambda;
            public Color color = STANDARD;
            public bool Active { get { return Lambda != null; } }

            readonly string rawText;
            List<string> lines = new List<string>();
            int __backing_field_for_Width = -1;
            #endregion
            #region helpers
            Point RawTextSize
            {
                get
                {
                    return new Point((int)Engine.Font.MeasureString(rawText).X
                                   , (int)Engine.Font.MeasureString(rawText).Y);
                }
            }
            public override string ToString() { return lines.Count > 1 ? string.Join("\n", lines) : rawText; }
            public static implicit operator string(MenuItem mi) { return mi.ToString(); }

            /// <summary> in pixels. </summary>
            public int Height { get { return RawTextSize.Y * lines.Count; } }
            #endregion

            /// <summary> in pixels. </summary>
            public int Width
            {
                get { return __backing_field_for_Width == -1 ? RawTextSize.X : __backing_field_for_Width; }
                set
                {
                    Debug.Assert(value > 0);
                    __backing_field_for_Width = value;
                    lines.Clear();

                    var words = rawText.Split();
                    var currentLine = "";
                    foreach (var currentWord in words)
                    {
                        var prospectiveLine = currentLine
                                            + (currentLine.Length == 0 ? "" : " ")
                                            + currentWord;
                        if (Engine.Font.MeasureString(prospectiveLine).X > __backing_field_for_Width)
                        {
                            lines.Add(currentLine);
                            currentLine = currentWord;
                        }
                        else currentLine = prospectiveLine;
                    }
                    lines.Add(currentLine);
                }
            }

            public void Draw(int xOrigin, int yOrigin)
            {
                Debug.Assert(Width > 0);
                Engine.DrawAtScreen("lozenge", xOrigin - BORDER_DEPTH, yOrigin
                                             , Width + BORDER_DEPTH * 2, Height, color);
                Engine.WriteAtScreen(this, xOrigin, yOrigin, 1);
            }
        }
        List<MenuItem> contents = new List<MenuItem>();
        MenuItem activeItem;

        #region obligatory data structure operations
        public Menu Add(string text) { return Add(text, Constants.NO_OP); }
        public Menu Add(string text, Action Lambda)
        {
            contents.Add(new MenuItem(text, Lambda));
            return this;
        }
        public Menu Add(string text, Action Lambda, Color color)
        {
            contents.Add(new MenuItem(text, Lambda, color));
            return this;
        }
        public bool AddUnique(string text, Action Lambda)
        {
            var mi = new MenuItem(text, Lambda);
            if (contents.Exists(_mi => mi.ToString() == _mi.ToString() && mi.color == _mi.color)) return false;
            Add(text, Lambda);
            return true;
        }

        public void GoNext()
        {
            if (activeItem == null)
            {
                activeItem = contents.Find(mi => mi.Active);
                if (activeItem == null) return;
            }
            else
            {
                activeItem.color = STANDARD;
                var valid = contents.FindAll(mi => mi.Active);
                var ndx = valid.IndexOf(activeItem) + 1;
                if (ndx == valid.Count) ndx = 0;
                activeItem = valid[ndx];
            }
            activeItem.color = HOVERING;
        }
        public void GoPrev()
        {
            if (activeItem == null)
            {
                activeItem = contents.FindLast(mi => mi.Active);
                if (activeItem == null) return;
            }
            else
            {
                activeItem.color = STANDARD;
                var valid = contents.FindAll(mi => mi.Active);
                var ndx = valid.IndexOf(activeItem) - 1;
                if (ndx < 0) ndx = valid.Count - 1;
                activeItem = valid[ndx];
            }
            activeItem.color = HOVERING;

        }
        public void InvokeCurrent()
        {
            if (activeItem != null) activeItem.Lambda();
        }
        #endregion

        #region view
        static readonly Color STANDARD = Color.Gray, HOVERING = Color.White;
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

        public void Draw()
        {
            if (DrawBox == null) throw new Exception("cannot use parameterless Draw() without knowing where.");
            var dbox = DrawBox();
            if (dbox.X != FLEXIBLE && dbox.Width != FLEXIBLE) goto JUST_DRAW_ALREADY;

            if (dbox.X == FLEXIBLE && dbox.Width != FLEXIBLE)
            {
                var max_x = dbox.Width;
                dbox.Width = MaxLineLength;
                dbox.X = max_x - dbox.Width;
                if (dbox.X < 0)
                {
                    var negativeNumber = dbox.X;
                    dbox.X -= negativeNumber;
                    dbox.Width += negativeNumber;
                    contents.ForEach(mi => mi.Width = dbox.Width);
                }
            }
            else if (dbox.X != FLEXIBLE && dbox.Width == FLEXIBLE)
            {
                dbox.Width = Math.Min(MaxLineLength, Engine.SCREEN_WIDTH_PX - dbox.X);
                contents.ForEach(mi => mi.Width = dbox.Width);
            }
            else
            {
                Debug.Assert(false, "cannot use parameterless Draw() without knowing where.");
            }

        JUST_DRAW_ALREADY:
            Draw(dbox.X, dbox.Y);
        }

        /// <summary> all args are in pixels. </summary>
        void Draw(int xOrigin, int yOrigin)
        {
            if (contents.Count == 0) return;
            contents.ForEach(mi =>
            {
                mi.Draw(xOrigin, yOrigin);
                yOrigin += mi.Height;
            });
        }
        #endregion
    }
}
