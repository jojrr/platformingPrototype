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
        public Rectangle stickTarget;
        public string PlatformCollision = "null";

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
            CharacterList[LocatedLevel][LocatedChunk].Add(this);
            stickTarget = Hitbox;
        }

        public void CheckPlatformCollision(Entity target)
        {
            PlatformCollision = IsCollidingWith(target);
            Rectangle targetHitbox = target.getHitbox();
            if (HasGravity)
            {
                if ( (PlatformCollision!="bottom") && (yVelocity != 0) )
                {
                    IsOnFloor = false;

                    if (PlatformCollision == "top")
                    {
                        yVelocity = 3;
                    }
                    yVelocity += 0.3;
                    if (yVelocity > 0) { yVelocity = Math.Min(yVelocity, TerminalVelocity); }
                }
                else 
                {
                    stickTarget = targetHitbox;
                    yVelocity = 0;
                    IsOnFloor = true;
                    this.updateLocation(Location.X, stickTarget.Y - this.Height);
                }
            }

            if (PlatformCollision == "side")
            {
                xVelocity = -xVelocity;
                WallInfront = true;
            }
            if (IsOnFloor) { WallInfront = false; }
        }

        /// <summary>
        /// Moves the player according to their velocity and checks collision.
        /// also responsible for gravity
        /// </summary>
        public void MoveCharacter()
        {
            xVelocity = Math.Min(Math.Abs(xVelocity), 5) * Math.Sign(xVelocity); // stops the player from achieving lightspeed
            updateLocation(Location.X + (int)xVelocity, Location.Y + (int)yVelocity);
            if ((!IsMoving) && (Math.Abs(xVelocity) > 0.01)) 
            {
                xVelocity = xVelocity / 1.28;
            }
        }

    }
}
