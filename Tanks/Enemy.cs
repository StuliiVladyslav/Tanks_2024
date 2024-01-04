using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Tanks
{
    public class Enemy
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Health { get; set; }
        public Image Image { get; set; } // Добавьте поле Image
        public EnemyMovement Movement { get; set; }
       
    }
    public class EnemyMovement
    {
        public double Speed { get; set; }
        public double Angle { get; set; }

        public EnemyMovement(double speed, double angle)
        {
            Speed = speed;
            Angle = angle;
        }
    }

}
