using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    class Bullet: ShapeBase
    {
        //Bullet has single path
        GraphicsPath m_Model;
        //Base bullet scale
        private const float c_Scale = 2;

        public Bullet(PointF point, float angle, float speed)
            : base(point)
        {
            MakeBullet();
            m_fRotation = angle;
            //2 is farthest point on the bullet drawing
            m_fMaxRadius = c_Scale * 2;
            SpeedAdjust(speed);
        }
        public override Queue<Tuple<GraphicsPath, Brush>> GetPath()
        {
            Queue<Tuple<GraphicsPath, Brush>> queue = new Queue<Tuple<GraphicsPath, Brush>>();
            GraphicsPath bulletgr = (GraphicsPath)m_Model.Clone();
            Matrix matrix = new Matrix();
            matrix = new Matrix();
            matrix.Scale(c_Scale, c_Scale, MatrixOrder.Append);
            matrix.Rotate(m_fRotation, MatrixOrder.Append);
            matrix.Translate(m_pLocation.X, m_pLocation.Y, MatrixOrder.Append);
            bulletgr.Transform(matrix);
            queue.Enqueue(new Tuple<GraphicsPath, Brush>(bulletgr, new SolidBrush(Color.Yellow)));
            return queue;
        }

        private void MakeBullet()
        {
            GraphicsPath gp = new GraphicsPath();
            //Ship graphics
            List<PointF> points = new List<PointF>()
            {
                new PointF(2, -1),
                new PointF(0, -1),
                new PointF(0, 1),
                new PointF(2, 1)
            };
            gp.AddPolygon(points.ToArray());
            //Rounded tip
            gp.AddBezier(new PointF(2, 1), new PointF(3, 1), new PointF(3, -1), new PointF(2, -1));
            m_Model = (GraphicsPath)gp.Clone();
        }
    }
}
