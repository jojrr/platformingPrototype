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
        // list of all characters - [int: level][list: chunk][Character]
        // used for gametick
        public static List<Character>[][] CharacterList = new List<Character>[TotalLevels][];
        
        public double xVelocity;
        public double yVelocity;
        public bool IsMoving = false;
        public bool IsOnFloor = false;
        public bool HasGravity;
        
        private const int TerminalVelocity = 100;
        private const int MaxXVelocity = 10;
        private int CoyoteTime;
        private const double Gravity = 0.981;

        public Rectangle? xStickTarget;
        public Rectangle? yStickTarget;
        private Entity? xStickEntity;
        private Entity? yStickEntity;

        private Rectangle OverShootRec;
        
        /// <summary>
        /// Array that stores the current collision state of this character.
        /// format [X, Y]
        /// </summary>
        public string[] CollisionState = { "null", "null" };
        private int yCollider = 0;
        private int xCollider = 1;

        // Initalises the jagged array - adds this character to [level][chunk]
        static Character()
        {
            // Loops through all levels and looks at the number of chunks of each level stored in the array
            // [ChunksInLvl] at each according level and initalises the jagged array [CharacterList] accordingly
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
        /// CollisionState[0] and [1] is the assigned Y and X collision value respectively.
        /// </summary>
        /// <param name="collisionTarget"></param>
        /// <returns>Rectangle: the collisionTarget's hitbox</returns>
        private Rectangle IsCollidingWith(Entity collisionTarget)
        {
            Rectangle targetHitbox = collisionTarget.getHitbox();
            Point targetCenter = collisionTarget.getCenter();
            
            // sets collision to null if not longer colliding with the previously colliding hitbox
            if (!Hitbox.IntersectsWith(targetHitbox))
            {
                if (collisionTarget == xStickEntity)
                {
                    xStickTarget = null;
                    xStickEntity = null;
                    CollisionState[xCollider] = "null"; 
                }
                if (collisionTarget == yStickEntity)
                {
                    yStickTarget = null;
                    yStickEntity = null;
                    CollisionState[yCollider] = "null";
                }
            }
            else
            {
                // if this' center is between the left and the right of the hitbox 
                if ((Center.X < targetHitbox.Right) && (Center.X > targetHitbox.Left))
                {
                    // Checks if there is a platform below - considers overshoot
                    if ( (Center.Y <= targetHitbox.Top  ) || (OverShootRec.IntersectsWith(targetHitbox) && (OverShootRec.Top < targetHitbox.Top)))
                    {
                        if (!IsOnFloor) { yVelocity = 0; } // zeros the velocity if the player was previously not on the floor when landing (prevents fling)
                        CollisionState[yCollider] = "bottom";
                        yStickTarget = targetHitbox;
                        yStickEntity = collisionTarget;
                    }
                    // Checks if there is a platform above the player
                    else if ((Center.Y >= targetCenter.Y + targetHitbox.Height/2 - Height/4 ) && (yVelocity <0))
                    {
                        CollisionState[yCollider] = "top";
                        yStickTarget = targetHitbox;
                        yStickEntity = collisionTarget;
                    }
                }

                // Checks if there is a platform to the left/right of the player
                if (Center.X < targetHitbox.Left)
                {
                    CollisionState[xCollider] = "right";
                    xStickTarget = targetHitbox;
                    xStickEntity = collisionTarget;
                }
                else if (Center.X > targetHitbox.Right)
                {
                    CollisionState[xCollider] = "left";
                    xStickTarget = targetHitbox;
                    xStickEntity = collisionTarget;
                }
            }
            
        return targetHitbox;
        
        }

        public void CheckPlatformCollision(Entity target)
        {
            Rectangle targetHitbox =  IsCollidingWith(target);

            if (yStickTarget != null)
            {
                // if platform is above -> set the location to 1 under the platform to prevent getting stuck
                if ( CollisionState[yCollider] == "top")
                {
                    Location.Y = yStickTarget.Value.Bottom + 1; 
                    yVelocity = 0;
                }

                // adds coyote time if there is a platform below the player, and sets the Y value of the player to the platform
                else if ( CollisionState[yCollider] == "bottom" )
                {
                    CoyoteTime = 10; // 100ms (on 10ms timer)
                    Location.Y = yStickTarget.Value.Y - Height;
                }

                // if the player is colliding with a corner, prevents the left/right wall collisions
                if (xStickTarget == yStickTarget)
                {
                    return;
                }
            }

            if (xStickTarget != null)
            {
                if (CollisionState[xCollider] == "right")
                {
                    Location.X = xStickTarget.Value.Left - this.Width;
                }
                else if (CollisionState[xCollider] == "left")
                {
                    Location.X = xStickTarget.Value.Right;
                }
            }


        }

        /// <summary>
        /// Moves the player according to their velocity and checks collision.
        /// also responsible for gravity
        /// </summary>
        public void MoveCharacter(bool isScrolling = false)
        {
            if (HasGravity)
            {
                // if there is no floor beneath -> gravity occurs
                if ( CollisionState[0] != "bottom" )
                {
                    IsOnFloor = false;
                    yVelocity += Gravity; 

                    // Terminal velocity -> only applies downwards
                    if (yVelocity > 0) { yVelocity = Math.Min(yVelocity, TerminalVelocity); }
                }
                // Coyote time ticks down 
                if (CoyoteTime > 0) 
                {
                    CoyoteTime -= 1;
                    IsOnFloor = true; // allows for more responsive jumping
                }
            }

            if (isScrolling == true) 
            {
                if (yStickEntity != null) { CheckPlatformCollision(yStickEntity); }
                updateLocation(Location.X, Location.Y + (int)yVelocity);
            }
            else
            {
                updateLocation(Location.X + (int)xVelocity, Location.Y + (int)yVelocity);
            }
            xVelocity = Math.Min(Math.Abs(xVelocity), MaxXVelocity) * Math.Sign(xVelocity); // stops the player from achieving lightspeed

            SetOverShootRec(); 

            // if not moving horizontally -> gradually decrease horizontal velocity
            if ((!IsMoving) && (Math.Abs(xVelocity) > 0.01)) 
            {
                //xVelocity *= 0.85;
                xVelocity = 0;
            }
        }

        /// <summary>
        /// creates a new rectangle to detect for overshoot above the player's current location.
        /// Rectangle is the size of the player (effectively doubling the player's height)
        /// Only used to detect overshoot incase the player clips into the ground.
        /// </summary>
        private void SetOverShootRec()
        {
            OverShootRec = new Rectangle(Location.X, Location.Y - Height, Width, Height);
        }





        public int GetChunksInLevel(int level)
        {
            return ChunksInLvl[level];
        }


    }
}
