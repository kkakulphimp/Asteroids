using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace Asteroids
{
    /// <summary>
    /// This structure is used to determine where to spawn the mirror shapes
    /// </summary>
    public struct BoundProximity
    {
        public bool left;
        public bool right;
        public bool top;
        public bool bottom;
        //Number of intersections this shape had
        public int proxSum;
    }
    abstract class ShapeBase: ICloneable
    {
        //Angle of shape
        protected float m_fRotation;
        //Rate of angle change on tick (affects rock)
        protected float m_fRotationChange;
        //Location of shape
        protected PointF m_pLocation;
        //Component speed of shape
        protected PointF m_pSpeed;
        //Maximum radius used for hit detection
        protected float m_fMaxRadius;

        /// <summary>
        /// Base constructor as follows:
        ///     -The position is the provided point
        ///     -Rotation starts at 0
        ///     -Rotation is random value from -3 to 3
        ///     -Speed is random falue between -2.5 and 2.5
        /// </summary>
        /// <param name="point"></param>
        public ShapeBase(PointF point)
        {
            m_pLocation = point;

        }
        /// <summary>
        /// Use same animation timer for everything
        /// </summary>
        static ShapeBase()
        {
            m_AnimationTimer.Start();
        }
        //Fade animation timer
        protected static Stopwatch m_AnimationTimer = new Stopwatch();
        /// <summary>
        /// Common random number generator
        /// </summary>
        public static Random Random { get; set; }
        /// <summary>
        /// Shape needs to die on check
        /// </summary>
        public bool MarkedForDeath { get; set; }
        /// <summary>
        /// ID used to kill original object if hit mirror
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Angle in degree helper
        /// </summary>
        public float AngleDeg
        {
            get
            {
                return m_fRotation;
            }
            set
            {
                m_fRotation = value;
            }
        }
        /// <summary>
        /// Angle in radians because sometimes it's degrees
        /// </summary>
        public float AngleRad
        {
            get
            {
                return (float)(m_fRotation * Math.PI / 180);
            }
        }
        /// <summary>
        /// Object's location
        /// </summary>
        public PointF Location
        {
            get
            {
                return m_pLocation;
            }
        }
        /// <summary>
        /// Object's fade "timer" which is just a countdown
        /// </summary>
        public int Fade { get; set; }
        /// <summary>
        /// Clone implementation is just a memberwse clone
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
        /// <summary>
        /// Queue of tuple (with path and brush) when the shape is more complex, so far it's just the ship but there is expandability
        /// </summary>
        /// <returns></returns>
        abstract public Queue<Tuple<GraphicsPath, Brush>> GetPath();
        /// <summary>
        /// Render the objects from the queue
        /// </summary>
        /// <param name="graphics"></param>
        public void Render(Graphics graphics)
        {
            Queue<Tuple<GraphicsPath, Brush>> shapes = GetPath();
            while (shapes.Count > 0)
            {
                Tuple<GraphicsPath, Brush> temp = shapes.Dequeue();
                graphics.FillPath(temp.Item2, temp.Item1);
            }
        }
        /// <summary>
        /// Tick shape for moving and keeping it in the playing field
        /// </summary>
        /// <param name="bound"></param>
        public void Tick(Rectangle bound)
        {
            m_pLocation.X += m_pSpeed.X;
            m_pLocation.Y += m_pSpeed.Y;
            m_fRotation += m_fRotationChange;
            if (!(this is Bullet))
            {
                if (m_pLocation.X > bound.Right) m_pLocation.X -= bound.Width;
                if (m_pLocation.X < bound.Left) m_pLocation.X += bound.Width;
                if (m_pLocation.Y > bound.Bottom) m_pLocation.Y -= bound.Height;
                if (m_pLocation.Y < bound.Top) m_pLocation.Y += bound.Height;
            }
            else if (!bound.Contains(new Point((int)m_pLocation.X, (int)m_pLocation.Y)))
            {
                MarkedForDeath = true;
            }
            if (Fade > 0)
                Fade--;
        }
        /// <summary>
        /// Static shape maker that was carried over from pointy pixel penetration
        /// </summary>
        /// <param name="vertices">Vertices on shape</param>
        /// <param name="variance">Variance from max radius</param>
        /// <param name="radius">Max radius</param>
        /// <returns></returns>
        protected static GraphicsPath MakeShape(int vertices, float variance, float radius)
        {
            GraphicsPath gp = new GraphicsPath();

            //Create vertices
            PointF[] pArray = new PointF[vertices];
            for (int i = 0; i < vertices; i++)
            {
                float variation = (float)(Random.NextDouble() * variance);
                float length = radius - variation;
                pArray[i] = new PointF((float)Math.Cos((float)(2 * Math.PI * i / vertices)) * length, (float)Math.Sin((float)(2 * Math.PI * i / vertices)) * length);
            }
            gp.AddPolygon(pArray);
            return gp;
        }
        /// <summary>
        /// Determine whether the two shapes are close enough to warrant checking intersection
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool DistanceTest(ShapeBase a, ShapeBase b)
        {
            return Math.Sqrt(Math.Pow(a.m_pLocation.X - b.m_pLocation.X, 2) + Math.Pow(a.m_pLocation.Y - b.m_pLocation.Y, 2)) < a.m_fMaxRadius + b.m_fMaxRadius;
        }
        /// <summary>
        /// Determine proximity to the playing space
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public BoundProximity EdgeProximity(Rectangle bounds)
        {
            BoundProximity bp;
            bp.left = (m_pLocation.X - m_fMaxRadius < bounds.Left);
            bp.right = (m_pLocation.X + m_fMaxRadius > bounds.Right);
            bp.bottom = (m_pLocation.Y - m_fMaxRadius < bounds.Top);
            bp.top = (m_pLocation.Y + m_fMaxRadius > bounds.Bottom);
            bp.proxSum = (bp.left ? 1 : 0) + (bp.right ? 1 : 0) + (bp.top ? 1 : 0) + (bp.bottom ? 1 : 0);
            return bp;
        }
        /// <summary>
        /// Translate this object by x distance
        /// </summary>
        /// <param name="distance"></param>
        public void TranslateX(float distance)
        {
            m_pLocation.X += distance;
        }
        /// <summary>
        /// Translate the object by y distance
        /// </summary>
        /// <param name="distance"></param>
        public void TranslateY(float distance)
        {
            m_pLocation.Y += distance;
        }
        /// <summary>
        /// Adjsut a linear speed to actual x y component speed
        /// </summary>
        /// <param name="linearSpeed"></param>
        public void SpeedAdjust(float linearSpeed)
        {
            m_pSpeed = new PointF((float)(linearSpeed * Math.Cos(AngleRad)), (float)(linearSpeed * Math.Sin(AngleRad)));
        }
        /// <summary>
        /// Sort helper to make a collection do ship first, then bullet, then rock
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static int SortHelper(ShapeBase thing)
        {
            if (thing is Ship)
                return 0;
            if (thing is Bullet)
                return 1;
            if (thing is Rock)
                return 2;
            return 3;
        }
    }
}
