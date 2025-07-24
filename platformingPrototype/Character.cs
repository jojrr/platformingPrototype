using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace platformingPrototype
{
    internal class Character : Entity
    {
        public static List<Character>[][] CharacterList = new List<Character>[TotalLevels][];
        public double xVelocity;
        public double yVelocity;
        public bool IsMoving = false;
        public bool IsOnFloor = false;
        public bool HasGravity = true;
        public bool WallInfront = false;
        
        private const int TerminalVelocity = 10;
        private const double Gravity = 0.2;
        public Rectangle xStickTarget;
        public Rectangle yStickTarget;

        private Rectangle OverShootRec;
        /// <summary>
        /// Array that stores the current collision detection of the
        /// </summary>
        public string[] CollisionState = { "null", "null" };

        // Initalises the jagged array - storing the enemies of each chunk per level.
        static Character()
        {
            // Loops through all levels and looks at the number of chunks of each level stored in the array
            // [ChunksInLvl] at each according level and initalises the jagged array [CharacterList] accordingly.
            for (int level = 0; level < TotalLevels; level++) {
                int chunks = ChunksInLvl[level];
                CharacterList[level] = new List<Character>[chunks];

                //loops through each chunk and adds the list into the dimension of said chunk
                for (int i = 0; i < chunks; i++)
                {
                    CharacterList[level][i] = new List<Character>();
                }
            }
        }

        /// <summary>
        /// Initalises a "character" (entity with velocity and gravity)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="LocatedLevel">The level that the character is located in</param>
        /// <param name="LocatedChunk">The chunk that the character is located in</param>
        /// <param name="xVelocity">default = 0</param>
        /// <param name="yVelocity">default = 0</param>
        /// <param name="flying">default = false</param>
        public Character(Point origin, int width, int height, int LocatedLevel, int LocatedChunk,  double xVelocity = 0, double yVelocity = 0, bool flying = false): base(origin: origin, width: width, height: height) 
        {
            this.xVelocity = xVelocity;
            this.yVelocity = yVelocity;
            HasGravity = !flying;
            SetOverShootRec();
            CharacterList[LocatedLevel][LocatedChunk].Add(this);
        }



        /// <summary>
        /// Checks if the target's hitbox is colliding with this entity's hitbox. 
        /// Returned position is relative to this Entity.
        /// </summary>
        /// <param name="collisionTarget"></param>
        /// <returns>string: "bottom", "top", "side", or (default)"null"</returns>
        private Rectangle IsCollidingWith(Entity collisionTarget)
        {
            Rectangle targetHitbox = collisionTarget.getHitbox();
            Point targetCenter = collisionTarget.getCenter();

            if (!Hitbox.IntersectsWith(targetHitbox))
            {
                if (targetHitbox == xStickTarget)
                {
                    xStickTarget = Hitbox;
                    CollisionState[1] = "null";
                }
                if (targetHitbox == yStickTarget)
                {
                    yStickTarget = Hitbox;
                    CollisionState[0] = "null";
                }
            }
            else
            {
                if ((Center.X < targetHitbox.Right) && (Center.X > targetHitbox.Left))
                {
                    if ( (Center.Y <= targetHitbox.Top  ) || (OverShootRec.IntersectsWith(targetHitbox) && (OverShootRec.Top < targetHitbox.Top)))
                    {
                        if (!IsOnFloor) { yVelocity = 0; }
                        CollisionState[0] = "bottom";
                        yStickTarget = targetHitbox;
                    }
                    else if (Center.Y >= targetCenter.Y + targetHitbox.Height/2 - Height/4 )
                    {
                        CollisionState[0] = "top";
                        yStickTarget = targetHitbox;
                    }
                }


                if (Center.X < targetHitbox.Left)
                {
                    CollisionState[1] = "right";
                    xStickTarget = targetHitbox;
                }
                else if (Center.X > targetHitbox.Right)
                {
                    CollisionState[1] = "left";
                    xStickTarget = targetHitbox;
                }
            }
        return targetHitbox;

        }

        public void CheckPlatformCollision(Entity target)
        {
            Rectangle targetHitbox =  IsCollidingWith(target);
            if (HasGravity)
            {
                if ( CollisionState[0] != "bottom" )
                {
                    IsOnFloor = false;
                    yVelocity += Gravity; 

                    if (yVelocity > 0) { yVelocity = Math.Min(yVelocity, TerminalVelocity); }
                }
                if (Hitbox.IntersectsWith(yStickTarget)) { IsOnFloor = true; }
            }

            if ( CollisionState[0] == "top")
            {
                Location.Y = yStickTarget.Bottom + 1; 
                yVelocity = 0;
            }
            else if ( CollisionState[0] == "bottom" )
            {
                Location.Y = yStickTarget.Y - Height;
            }

            if (xStickTarget == yStickTarget)
            {
                return;
            }

            if (CollisionState[1] == "right")
            {
                Location.X = xStickTarget.Left - this.Width+1;
            }
            else if (CollisionState[1] == "left")
            {
                Location.X = xStickTarget.Right + 1;
            }


        }

        /// <summary>
        /// Moves the player according to their velocity and checks collision.
        /// also responsible for gravity
        /// </summary>
        public void MoveCharacter()
        {
            xVelocity = Math.Min(Math.Abs(xVelocity), 5) * Math.Sign(xVelocity); // stops the player from achieving lightspeed

            updateLocation(Location.X + (int)xVelocity, Location.Y + (int)yVelocity);
            SetOverShootRec();

            if ((!IsMoving) && (Math.Abs(xVelocity) > 0.01)) 
            {
                xVelocity = xVelocity / 1.05;
            }
        }

        private void SetOverShootRec()
        {
            OverShootRec = new Rectangle(Location.X, Location.Y - Height, Width, Height);
        }

    }
}
