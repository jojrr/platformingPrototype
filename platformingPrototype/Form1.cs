using System.Diagnostics.Metrics;
using System.Text.Json.Serialization;

namespace platformingPrototype
{
    public partial class Form1 : Form
    {
        Character playerBox = new(
            origin: new Point(20, 250),
            width: 50,
            height: 50,
            LocatedLevel: 0,
            LocatedChunk: 0,
            yVelocity: -0.2);

        readonly Platform box2 = new(
            origin: new Point(0, 650),
            width: 1400,
            height: 550,
            LocatedLevel: 0,
            LocatedChunk: 0);

        readonly Platform box3 = new(
            origin: new Point(300, 200),
            width: 400,
            height: 175,
            LocatedLevel: 0,
            LocatedChunk: 1);

        readonly Platform box4 = new(
            origin: new Point(1000, 400),
            width: 200,
            height: 300,
            LocatedLevel: 0,
            LocatedChunk: 1);

        bool movingLeft = false;
        bool movingRight = false;
        bool jumping = false;

        int jumpVelocity = -22;
        double xAccel = 2;

        int CurrentLevel;
        List<int> LoadedChunks;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CurrentLevel = 0;
            LoadedChunks = [0];
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (playerBox.IsOnFloor && jumping) { playerBox.yVelocity = jumpVelocity; playerBox.IsOnFloor = false; }
            if (!playerBox.WallInfront)
            {
                if (movingLeft) { playerBox.xVelocity -= xAccel; }
                if (movingRight) { playerBox.xVelocity += xAccel; }
            }
            if (!movingLeft  && !movingRight)
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
                    foreach (int chunk2 in LoadedChunks)
                    {
                        foreach (Platform plat in Platform.PlatformList[CurrentLevel][chunk2])
                        {
                            chara.CheckPlatformCollision(plat);
                        }
                    }
                    chara.MoveCharacter();
                }
            }

            if (playerBox.getCenter().X > 500){
                if (!LoadedChunks.Contains(1)) { LoadedChunks.Add(1); };
            }
            else { LoadedChunks.Remove(1); }

                label5.Text = (playerBox.CollisionState[0]).ToString();
            label4.Text = (playerBox.CollisionState[1]).ToString();
            label1.Text = (playerBox.getCenter()).ToString();
            label2.Text = (box2.getCenter()).ToString();
            label3.Text = Convert.ToString(playerBox.xStickTarget);
            GC.Collect();
            this.Refresh();
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
