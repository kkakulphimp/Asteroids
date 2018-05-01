using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    class Star :ShapeBase
    {
        //Size of the star
        private const int c_StarSize = 2;
        /// <summary>
        /// Build a star at point
        /// </summary>
        /// <param name="p"></param>
        public Star(PointF p)
            :base(p)
        {
        }
        /// <summary>
        /// Return ellipse of star
        /// </summary>
        /// <returns></returns>
        public override Queue<Tuple<GraphicsPath, Brush>> GetPath()
        {
            GraphicsPath gp = new GraphicsPath();
            Queue<Tuple<GraphicsPath, Brush>> queue = new Queue<Tuple<GraphicsPath, Brush>>();
            gp.AddEllipse(m_pLocation.X, m_pLocation.Y, c_StarSize, c_StarSize);
            queue.Enqueue(new Tuple<GraphicsPath, Brush>(gp, new SolidBrush(Color.White)));
            return queue;
        }
    }
}
