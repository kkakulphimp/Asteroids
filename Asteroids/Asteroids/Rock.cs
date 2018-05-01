/*
 *Description: Main code for rock which is derived from shapebase 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Asteroids
{
    //Different rock sizes
    public enum Size
    {
        Small = 1,
        Medium = 2,
        Large = 3
    }
    class Rock : ShapeBase
    {
        //Single model per asteroid
        private GraphicsPath m_model;
        //Variance of the distance for a vertex
        private float m_fVariance = (float)0.5;
        //Smallest number of vertices
        private const int c_minVertices = 4;
        //Greatest number of vertices
        private const int c_maxVertices = 12;
        //Base object scale value
        private const float c_SizeScaler = 10;
        /// <summary>
        /// Asteroid's enum size
        /// </summary>
        public Size Size { get; private set; }
        /// <summary>
        /// Only constructor used by asteroid
        /// </summary>
        /// <param name="point">Spawn location</param>
        /// <param name="size">Enum size</param>
        /// <param name="fade">fade initialization</param>
        public Rock(PointF point, Size size, int fade)
            : base(point)
        {
            Size = size;
            Fade = fade;
            m_fMaxRadius = c_SizeScaler * (int)size;
            m_fVariance = m_fMaxRadius / 2;
            m_fRotation = (float)(Random.NextDouble() * 360);
            m_fRotationChange = (float)Random.NextDouble() * 6 - 3;
            m_pSpeed = new PointF(
                (float)(Random.NextDouble() * 5 - 2.5),
                (float)(Random.NextDouble() * 5 - 2.5));
            m_model = MakeShape(Random.Next(c_minVertices, c_maxVertices), m_fVariance, m_fMaxRadius);
        }
        /// <summary>
        /// Path for asteroid
        /// </summary>
        /// <returns></returns>
        public override Queue<Tuple<GraphicsPath,Brush>> GetPath()
        {
            //Fade color if faded, will check if bit 8 is set or not set on the timer
            Color drawcolor = Fade > 0 && (m_AnimationTimer.ElapsedMilliseconds & 1 << 7) > 0 ? Color.FromArgb(64, Color.Gray.R, Color.Gray.G, Color.Gray.B) : Color.Gray;
            Queue<Tuple<GraphicsPath, Brush>> queue = new Queue<Tuple<GraphicsPath, Brush>>();
            GraphicsPath path = (GraphicsPath)m_model.Clone();
            Matrix matrix = new Matrix();
            matrix.Rotate(m_fRotation);
            matrix.Translate(m_pLocation.X, m_pLocation.Y, MatrixOrder.Append);
            path.Transform(matrix);
            queue.Enqueue(new Tuple<GraphicsPath, Brush>(path, new SolidBrush(drawcolor)));
            return queue;
        }
    }
}
