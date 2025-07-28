using System.Diagnostics.Metrics;
using System.Text.Json.Serialization;

namespace platformingPrototype
{
    public partial class Form1 : Form
    {
        Character playerBox = new(
            origin: new Point(750, 250),
            width: 50,
            height: 50,
            LocatedLevel: 0,
            LocatedChunk: 0,
            yVelocity: -0.2);

         Platform box2 = new(
            origin: new Point(1, 650),
            width: 5400,
            height: 550,
            LocatedLevel: 0,
            LocatedChunk: 0);

         Platform box3 = new(
            origin: new Point(300, 200),
            width: 400,
            height: 175,
            LocatedLevel: 0,
            LocatedChunk: 0);

         Platform box4 = new(
            origin: new Point(1000, 400),
            width: 200,
            height: 300,
            LocatedLevel: 0,
            LocatedChunk: 0);


        Rectangle viewPort;
        string onWorldBoundary = "left";

        bool movingLeft = false;
        bool movingRight = false;
        bool jumping = false;

        bool scrollRight = false;
        bool scrollLeft = false;

        int jumpVelocity = -22;
        double xAccel = 2;

        int CurrentLevel;
        List<int> LoadedChunks;
        int AllChunks;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CurrentLevel = 0;
            LoadedChunks = [0];
            AllChunks = box2.getChunksInLvl(CurrentLevel); 
            int windowWidth = this.Width;
            int windowHeight = this.Height;
            viewPort = new Rectangle( new Point(-5,0), new Size(windowWidth+10, windowHeight ) );
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (playerBox.IsOnFloor && jumping) { playerBox.yVelocity = jumpVelocity; playerBox.IsOnFloor = false; }
            if (movingLeft) { playerBox.xVelocity -= xAccel; }
            if (movingRight) { playerBox.xVelocity += xAccel; }

            if (!movingLeft && !movingRight)
            {
                playerBox.IsMoving = false;
            }
            else
            {
                playerBox.IsMoving = true;
            }



            foreach (int chunk1 in LoadedChunks)
            {
                foreach (Character chara in Character.CharacterList[CurrentLevel][chunk1])
                {

                    if (viewPort.Left < box2.getHitbox().Left) { onWorldBoundary = "left"; }
                    else if (viewPort.Right > box2.getHitbox().Right) { onWorldBoundary = "right"; }
                    else { onWorldBoundary = "null"; }

                    if (playerBox.getCenter().X < 500) { scrollLeft = true; }
                    if (playerBox.getCenter().X > 1300) { scrollRight = true; }

                    bool isScrolling = (scrollLeft || scrollRight);

                    if ((scrollLeft && movingRight) || (scrollRight && movingLeft))
                    {
                        isScrolling = false;
                    }

                    if (onWorldBoundary == "null")
                    {
                        if (scrollRight && movingRight) { ScrollPlatform(currentLevel: CurrentLevel, velocity: -chara.xVelocity); isScrolling = true; }
                        else if (scrollLeft && movingLeft) { ScrollPlatform(currentLevel: CurrentLevel, velocity: -chara.xVelocity); isScrolling = true; }
                        else
                            isScrolling = false;
                    }
                    else if (onWorldBoundary == "left")
                    {
                        if (scrollRight && movingRight)
                            ScrollPlatform(currentLevel: CurrentLevel, velocity: -chara.xVelocity);
                        else
                            isScrolling = false;
                    }

                    else if (onWorldBoundary == "right")
                    {
                        if (scrollLeft && movingLeft)
                            ScrollPlatform(currentLevel: CurrentLevel, velocity: -chara.xVelocity);
                        else
                            isScrolling = false;
                    }

                    foreach (int chunk2 in LoadedChunks)
                    {
                        foreach (Platform plat in Platform.PlatformList[CurrentLevel][chunk2])
                        {
                            chara.CheckPlatformCollision(plat);
                        }
                    }

                    chara.MoveCharacter(isScrolling: isScrolling);
                }
            }


            if (playerBox.getCenter().X > 500)
            {
                if (!LoadedChunks.Contains(1)) { LoadedChunks.Add(1); }
                ;
            }
            else { LoadedChunks.Remove(1); }


            label5.Text = (playerBox.CollisionState[0]).ToString();
            label4.Text = (playerBox.CollisionState[1]).ToString();
            label1.Text = (playerBox.getCenter()).ToString();
            label2.Text = (box2.getCenter()).ToString();
            label3.Text = (onWorldBoundary).ToString();
            GC.Collect();
            this.Refresh();
        }





        public void ScrollPlatform(int currentLevel, double velocity)
        {
            for (int i = 0; i < AllChunks; i++)
            {
                foreach (Platform plat in Platform.PlatformList[currentLevel][i])
                {
                    plat.updateLocation(plat.getPoint().X + (int)velocity, plat.getPoint().Y);
                }
            }
        }





        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Pen bluePen = new Pen(Color.Blue, 3);
            Pen redPen = new Pen(Color.Red, 3);
            foreach (int chunk in LoadedChunks)
            {
                foreach (Character chara in Character.CharacterList[CurrentLevel][chunk])
                {
                    e.Graphics.DrawRectangle(bluePen, chara.getHitbox());
                }
                foreach (Platform plat in Platform.PlatformList[CurrentLevel][chunk])
                {
                    e.Graphics.DrawRectangle(redPen, plat.getHitbox());
                }
            }
        }





        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Rectangle playerBoxHitbox = playerBox.getHitbox();
            if (e.KeyCode == Keys.A)
            {
                movingRight = false;
                movingLeft = true;
            }
            if (e.KeyCode == Keys.D)
            {
                movingLeft=false;
                movingRight = true;
            }
            if (e.KeyCode == Keys.W)
            {
                jumping = true;
            }
        }





        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                movingLeft = false;
            }
            if (e.KeyCode == Keys.D)
            {
                movingRight = false;
            }
            if (e.KeyCode == Keys.W)
            {
                jumping = false;
            }
        }
    }
}
