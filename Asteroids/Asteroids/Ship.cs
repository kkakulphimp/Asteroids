using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Asteroids
{
    class Ship : ShapeBase
    {
        //Ship's hull
        private GraphicsPath m_Hull;
        //Triangular window
        private GraphicsPath m_Window;
        //Booster at the back end
        private GraphicsPath m_Booster;
        //Ship's color
        private Color m_Color;
        //Base scale factor for a ship
        private const float c_ShipScale = 5;
        //Linear speed of a ship
        private float m_LinearSpeed = 0;
        //Max linear speed
        private const float c_MaxLinearSpeed = 2;
        //Rate of ship boost speed decay
        private const float c_Decay = 0.05f;
        //Alpha for fade
        private const byte c_FadeAlpha = 64;
        //Timer fade flash bit position
        private const int c_FadeFlashBit = 7;
        /// <summary>
        /// Make ship appear at a point with color and initialize fade
        /// </summary>
        /// <param name="point">Location of center point of ship</param>
        /// <param name="color">Ship's hull color</param>
        /// <param name="fade">Fade initialization</param>
        public Ship(PointF point, Color color, int fade)
            :base(point)
        {
            Fade = fade;
            m_fMaxRadius = c_ShipScale * 5;
            m_fRotation = 0;
            m_fRotationChange = 0;
            m_pSpeed = new PointF(0, 0);
            m_Color = color;
            MakeShip();
        }
        /// <summary>
        /// Location of the ship's gun port used by bullet
        /// </summary>
        public PointF GunPortLocation
        {
            get
            {
                float x = (float)(m_pLocation.X + Math.Cos(AngleRad) * c_ShipScale * 5);
                float y = (float)(m_pLocation.Y + Math.Sin(AngleRad) * c_ShipScale * 5);
                return new PointF(x, y);
            }
        }
        /// <summary>
        /// Path returns as a queue starting with hull, window, and boost to make sure they draw in the correct dequeue order
        /// </summary>
        /// <returns></returns>
        public override Queue<Tuple<GraphicsPath,Brush>> GetPath()
        {
            //Fade color if faded
            Color hullColor = Fade > 0 && (m_AnimationTimer.ElapsedMilliseconds & 1 << c_FadeFlashBit) > 0 ? Color.FromArgb(c_FadeAlpha, m_Color.R, m_Color.G, m_Color.B) : m_Color;
            Queue<Tuple<GraphicsPath, Brush>> queue = new Queue<Tuple<GraphicsPath, Brush>>();
            //Get clones for all pieces
            GraphicsPath tempHull = (GraphicsPath)m_Hull.Clone();
            GraphicsPath tempWin = (GraphicsPath)m_Window.Clone();
            GraphicsPath tempBoost = (GraphicsPath)m_Booster.Clone();

            //Do rest of ship
            Matrix mainMatrix = new Matrix();
            mainMatrix.Scale(c_ShipScale, c_ShipScale);
            mainMatrix.Rotate(m_fRotation, MatrixOrder.Append);
            mainMatrix.Translate(m_pLocation.X, m_pLocation.Y, MatrixOrder.Append);
            tempHull.Transform(mainMatrix);
            tempWin.Transform(mainMatrix);
            queue.Enqueue(new Tuple<GraphicsPath, Brush>(tempHull, new SolidBrush(hullColor)));
            queue.Enqueue(new Tuple<GraphicsPath, Brush>(tempWin, new SolidBrush(Color.LightBlue)));
            //Do booster size scaling
            if (m_LinearSpeed > 0)
            {
                Matrix boosterMatrix = new Matrix();
                boosterMatrix.Scale(1, m_LinearSpeed / c_MaxLinearSpeed);
                tempBoost.Transform(boosterMatrix);
                tempBoost.Transform(mainMatrix);
                queue.Enqueue(new Tuple<GraphicsPath, Brush>(tempBoost, new SolidBrush(Color.Goldenrod)));
            }
            
            return queue;
        }
        private void MakeShip()
        {
            GraphicsPath gp = new GraphicsPath();
            //Ship graphics
            List<PointF> points = new List<PointF>()
            {
                new PointF(4,0),
                new PointF(3,-1),
                new PointF(0,-2),
                new PointF(0,-5),
                new PointF(-2, -5),
                new PointF(-3,-2),
                new PointF(-4,-1),
                new PointF(-4, 1),
                new PointF(-3, 2),
                new PointF(-2, 5),
                new PointF(0, 5),
                new PointF(0, 2),
                new PointF(3, 1)
            };
            gp.AddPolygon(points.ToArray());
            m_Hull = (GraphicsPath)gp.Clone();
            gp = new GraphicsPath();
            //Window graphics
            points = new List<PointF>()
            {
                new PointF(2,0),
                new PointF(0,-1),
                new PointF(0,1)
            };
            gp.AddPolygon(points.ToArray());
            m_Window = (GraphicsPath)gp.Clone();
            //Booster graphics
            gp = new GraphicsPath();
            points = new List<PointF>()
            {
                new PointF(-4,-1),
                new PointF(-4, 1),
                new PointF(-6, 0)
            };
            gp.AddPolygon(points.ToArray());
            m_Booster = (GraphicsPath)gp.Clone();
        }
        /// <summary>
        /// Boost will jam on the booster and adjust from linear speed to vector
        /// </summary>
        public void Boost()
        {
            m_LinearSpeed = c_MaxLinearSpeed;
            SpeedAdjust(m_LinearSpeed);
        }
        /// <summary>
        /// Decay slows the ship over time if the boost isn't jammed on
        /// </summary>
        public void Decay()
        {
            if (m_LinearSpeed > 0)
                m_LinearSpeed -= c_Decay;
            SpeedAdjust(m_LinearSpeed);
        }
    }
}
