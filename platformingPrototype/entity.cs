using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace platformingPrototype
{
    /// <summary>
    /// produces the hitbox and responsible for collision detection
    /// </summary>
    internal class Entity
    {
        protected Point Location;
        protected Point Center;
        protected Size Size;
        protected int
            Width = 0,
            Height = 0;
        protected Rectangle Hitbox;

        protected const int TotalLevels = 1;
        protected static readonly int[] ChunksInLvl = new int[TotalLevels] { 2 };

        /// <summary>
        /// Checks if the target's hitbox is colliding with this entity's hitbox. 
        /// Returned position is relative to this Entity.
        /// </summary>
        /// <param name="collisionTarget"></param>
        /// <returns>string: "bottom", "top", "side", or (default)"null"</returns>
        public string IsCollidingWith(Entity collisionTarget)
        {
            Rectangle targetHitbox = collisionTarget.getHitbox();
            Point targetCenter = collisionTarget.getCenter();

            if (Hitbox.IntersectsWith(targetHitbox))
            {
                if (Center.Y <= targetCenter.Y - targetHitbox.Height/2)
                {
                    return "bottom";
                }
                else if (Center.Y >= targetCenter.Y + targetHitbox.Height/2 - Height/4 )
                {
                    return "top";
                }

                else return "side";
            }
            else return "null";
        }

        /// <summary>
        /// returns the hitbox as a rectangle
        /// </summary>
        /// <returns>hitbox of type rectangle</returns>
        public Rectangle getHitbox()
        {
            return Hitbox;
        }


        /// <summary>
        /// Creates a hitbox at specified paramters
        /// </summary>
        /// <param name="origin">the point of the top-left of the rectangle</param>
        /// <param name="width">width of the rectangle</param>
        /// <param name="height">height of the rectangle</param>
        public Entity(Point origin, int width, int height)
        {
            Location = origin;
            Width = width;
            Height = height;
            Size = new Size( width, height);
            Hitbox = new Rectangle(origin, Size); 
            Center = new Point (Hitbox.X + Width/2, Hitbox.Y + Height/2);
        }

        /// <summary>
        /// Assigns a new position to the top-left of the hitbox
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void updateLocation(int x, int y)
        {
            Location = new Point(x, y);
            Hitbox = new Rectangle(Location, Size);
            Center = new Point (Location.X + Width/2, Location.Y + Height/2);
        }

        /// <summary>
        /// returns the point of the center
        /// </summary>
        /// <returns></returns>
        public Point getCenter()
        {
            return Center;
        }

        /// <summary>
        /// returns the point of the top-left 
        /// </summary>
        /// <returns></returns>
        public Point getLocation()
        {
            return Location;
        }
    }
}
