using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Bomberman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class Bomb
    {
        // Bomb related variable
        public double BombExplodeTime;
        public int BombRange;
        public object BombTag;
        public int PosX, PosY;
        public Rect B_Hit;
        // Bomb Name, Bomb Explosion Time, Bomb Range, Row for Bomb, Column for Bomb
        public Bomb(object BombTag, double BombExplodeTime, int BombRange, int PosY, int PosX, Rect B_Hit)
        {
            this.BombTag = BombTag;
            this.BombExplodeTime = BombExplodeTime;
            this.BombRange = BombRange;
            this.PosX = PosX;
            this.PosY = PosY;
            this.B_Hit = B_Hit;
        }
    }

    public class ExplosionVar
    {
        public double ExplosionTime;
        public int PositionX;
        public int PositionY;
        public Rectangle canvas;
        public ExplosionVar(double E, int Y, int X, Rectangle canvas)
        {
            this.ExplosionTime = E;
            this.PositionX = X;
            this.PositionY = Y;
            this.canvas = canvas;
        }
    }
    
    public class Enemy
    {
        public string type;
        public int health;
        public int EposX, EposY;
        public bool[] dir = new bool[4]; // 0 - Up // 1 - Down // 2 - Left // 3 - Right
        public Rect E_Hit;
        public Rectangle canvas;
        public Enemy(string type, int EposY, int EposX, bool[] dir, Rect E_Hit, Rectangle canvas)
        {
            this.type = type;
            this.EposX = EposX;
            this.EposY = EposY;
            this.health = 1;
            this.dir = dir;
            this.E_Hit = E_Hit;
            this.canvas = canvas;
        }
    }

    public class Item
    {
        public string name;
        public int position;

        public Item(string name, int position)
        {
            this.name = name;
            this.position = position;
        }

    }
    public class PowerUp : Item
    {
        public PowerUp(string name, int position) : base(name, position) { }
    }
    public class EndItems : Item
    {
        public EndItems(string name, int position) : base(name, position) { }
    }

    public partial class MainWindow : Window
    {
        private DispatcherTimer GameTimer = new DispatcherTimer();
        private double GameTime;
        private int Ticks;
        // Player Direction
        public enum Direction
        {
            Idle, Up, Left, Down, Right
        }
        private Direction Movement; 
        private bool UpEnable, LeftEnable, DownEnable, RightEnable;

        public int Player_Row, Player_Column;
        public int min_X, min_Y, max_X, max_Y; // Minimum and Maximum Coordinates of Canvas
        public int Speed; // Player Speed
        private bool Player_Status; // Status - true = alive, false = dead
        public bool GetKey, ReachDoor;

        // Bomb default setup
        public int BombRemains;
        public int BombExploded; // Numbers of bomb exploded
        public List<Bomb> BombBackPack = new List<Bomb>();
        public int MaxBombPowerUp, MaxRangePowerUp, PowerUpTotal;//Bomb PowerUp


        private List<Rectangle> BombTracking = new List<Rectangle>(); //Canvas Control of the bomb
        private List<ExplosionVar> _ExplosionVar = new List<ExplosionVar>(); // All Control of the explosion 

        // PowerUp saving (representation in coordinates) 
        public PowerUp PGeneral;
        public EndItems EGeneral;
        public List<Item> ItemList = new List<Item>(); // formula -> y * 100 + x 
        public bool GetPowerUp1, GetPowerUp2, trigger1, trigger2;
        private int type, rate;

        // Map setup 
        private Random maprand = new Random();
        private int[] StartPosition = new int[2]; // (x,y) of the starting position
        private int BrickCount, BrickBreak;
        private bool KeyExist, DoorExist;

        //Enemies
        private List<Enemy> EnemyList = new List<Enemy>();

        //Display Variable setup
        public int score;

        //End Game
        private bool wincondition;
        //Map
        // 0 - Empty                  5 - Brick (PowerUp2)                      10 - Starting Position
        // 1 - Wall                   6 - Bomb                                  11 - Key
        // 2 - Empty (Bricks remain)  7 - Bomb Explosion -> Die if hit player   12 - Door
        // 3 - Brick                  8 - Explosion Occurs
        // 4 - Brick (PowerUp1)       9 - /  

        protected int[,] map = new int[21, 29]  // (y,x)
            {
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,10,10,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,10,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            };
        public MainWindow()
        {
            InitializeComponent();
            GameTimer.Tick += GameLoop;
            GameTimer.Interval = TimeSpan.FromMilliseconds(20);
            MainCanvas.Focus();
            GameSetup();
        }
        
        public void Move(int row, int col, bool R, bool L, bool D, bool U, Direction M) 
        {
            if (map[row, col + 1] == 1 || map[row, col + 1] == 3 || map[row, col + 1] == 6)
                R = false;
            else
                R = true;

            if (map[row + 1, col] == 1 || map[row + 1, col] == 3 || map[row + 1, col] == 6)
                D = false;
            else
                D = true;

            if (map[row, col - 1] == 1 || map[row, col - 1] == 3 || map[row, col - 1] == 6)
                L = false;
            else
                L = true;

            if (map[row - 1, col] == 1 || map[row - 1, col] == 3 || map[row - 1, col] == 6)
                U = false;
            else
                U = true;
            if (M != Direction.Idle)
            {
                switch (M)
                {
                    case Direction.Left:
                        if (L)
                            Canvas.SetLeft(Player, Canvas.GetLeft(Player) - Speed);
                        break;

                    case Direction.Right:
                        if (R)
                            Canvas.SetLeft(Player, Canvas.GetLeft(Player) + Speed);
                        break;

                    case Direction.Up:
                        if (U)
                            Canvas.SetTop(Player, Canvas.GetTop(Player) - Speed);
                        break;

                    case Direction.Down:
                        if (D)
                            Canvas.SetTop(Player, Canvas.GetTop(Player) + Speed);
                        break;
                }
            }
        }
        public void EMove(Enemy E)
        {
            // U D L R
            Random changedir = new Random();
            int cd = changedir.Next(0, 100);
            bool[] es = new bool[4];
            int row = E.EposY;
            int col = E.EposX;

            if (cd >= 10)
            {
                if (E.type == "BrainMob")
                {
                    if (E.dir[0] || E.dir[1])
                    {
                        E.dir[0] = E.dir[1] = false;
                        E.dir[cd % 2 + 2] = true;
                    }
                    else
                    {
                        E.dir[2] = E.dir[3] = false;
                        E.dir[cd % 2] = true;
                    }
                }
            }

            if (E.dir[0])
            {
                if (map[row - 1, col] == 0 || map[row - 1, col] == 2)
                {
                    Canvas.SetTop(E.canvas, Canvas.GetTop(E.canvas) - Speed);
                    E.EposY -= 1;
                }
                else
                {
                    E.dir[0] = false;
                    E.dir[1] = true;
                }
            }
            else if (E.dir[1])
            {
                if (map[row + 1, col] == 0 || map[row + 1, col] == 2)
                {
                    Canvas.SetTop(E.canvas, Canvas.GetTop(E.canvas) + Speed);
                    E.EposY += 1;
                }
                else
                {
                    E.dir[1] = false;
                    E.dir[0] = true;
                }
            }
            else if (E.dir[2])
            {
                if (map[row, col - 1] == 0 || map[row, col - 1] == 2)
                {
                    Canvas.SetLeft(E.canvas, Canvas.GetLeft(E.canvas) - Speed);
                    E.EposX -= 1;
                }
                else
                {
                    E.dir[2] = false;
                    E.dir[3] = true;
                }
            } else if (E.dir[3])
            {
                if (map[row, col + 1] == 0 || map[row, col + 1] == 2)
                {
                    Canvas.SetLeft(E.canvas, Canvas.GetLeft(E.canvas) + Speed);
                    E.EposX += 1;
                }
                else
                {
                    E.dir[3] = false;
                    E.dir[2] = true;
                }
            }

            E.E_Hit = new Rect(min_Y + E.EposY * 25 + 5, E.EposX * 25 + 5, 15, 15);
        }
        private void GameSetup()
        {
            Movement = Direction.Idle;
            max_X = (int)Window.Height;
            max_Y = (int)Window.Width;
            min_X = 5; 
            min_Y = 10;
            Speed = 25;
      
            //numberic value reset
            
            BombExploded = 0; 
            MaxBombPowerUp = 0;
            MaxRangePowerUp = 0;
            BombRemains = 1;
            PowerUpTotal = 0;

            score = 0;
            UpEnable = LeftEnable = false;
            DownEnable = RightEnable = true;
            Player_Row = 1;
            Player_Column = 1;

            GetPowerUp1 = GetPowerUp2 = false;
            trigger1 = trigger2 = true;
            Player_Status = true;
            rate = 0;
            Ticks = 0;

            BrickCount = 2;
            BrickBreak = 0;
            KeyExist = false;
            DoorExist = false;
            GetKey = false;
            ReachDoor = false;

            wincondition = false;

            MapInit();
            ImageBrush Player_Image = new ImageBrush();
            Player_Image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/player.png"));
            Player.Fill = Player_Image;

            GameTimer.Start();
        }

        public void MapInit()
        {
            // int empty = 0; // Use for counting slots 
            // 609 slots in total, 356 not solid wall
            StartPosition[0] = 7; 
            StartPosition[1] = 13;

            for (int i = 0; i < 21; i++)
                for (int j = 0; j < 29; j++)
                {
                    if (map[i, j] == 0)
                    {
                        int r = maprand.Next(0, 356);
                        if (r > 200)
                        {
                            map[i, j] = 3;
                            BrickCount++;
                        }
                        else if (r < 18)
                        {
                            Enemy _Create;
                            ImageBrush Enemy_Image = new ImageBrush();
                            Rectangle _enemy = new Rectangle()
                            {
                                Width = 15,
                                Height = 15,
                            };
                            Rect en = new Rect()
                            {
                                X = i * Speed + 7,
                                Y = j * Speed + 13,
                                Width = 10,
                                Height = 10
                            };
                            Random e = new Random();
                            int num = e.Next(0, 100) % 3;
                            if (r % 2 == 0)
                            {

                                bool[] temp = new bool[4] { false, false, false, false };
                                if (map[i - 1, j] == 0) temp[0] = true;
                                else if (map[i + 1, j] == 0) temp[1] = true;
                                else if (map[i, j - 1] == 0) temp[2] = true;
                                else if (map[i, j + 1] == 0) temp[3] = true;

                                _Create = new Enemy("StupidMob", i, j, temp, en, _enemy);
                                Enemy_Image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/stupidmob.png"));
                                _enemy.Fill = Enemy_Image;
                            }
                            else
                            {

                                bool[] temp = new bool[4] { false, false, false, false };
                                if (map[i - 1, j] == 0) temp[0] = true;
                                else if (map[i + 1, j] == 0) temp[1] = true;
                                else if (map[i, j - 1] == 0) temp[2] = true;
                                else if (map[i, j + 1] == 0) temp[3] = true;

                                _Create = new Enemy("BrainMob", i, j, temp, en, _enemy);
                                Enemy_Image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/brainmob.png"));
                                _enemy.Fill = Enemy_Image;
                            }
                            Canvas.SetTop(_enemy, min_Y + i * 25 + 5);
                            Canvas.SetLeft(_enemy, min_X + j * 25 + 5);
                            Canvas.SetZIndex(_enemy, -1);
                            EnemyList.Add(_Create);
                            MainCanvas.Children.Add(_enemy);
                        }
                    }

                    if (map[i, j] == 1)
                    {
                        Rectangle _wall = new Rectangle()
                        {
                            Width = 25,
                            Height = 25,
                            Tag = "Wall",
                            Stroke = Brushes.Black,
                            StrokeThickness = 2,
                            Fill = Brushes.Gray,
                        };

                        // Add to a canvas for example
                        Canvas.SetTop(_wall, min_Y + i * 25);
                        Canvas.SetLeft(_wall, min_X + j * 25);
                        Canvas.SetZIndex(_wall, -5);
                        MainCanvas.Children.Add(_wall);
                    }

                    if (map[i, j] == 3)
                    {
                        ImageBrush Brick_Image = new ImageBrush();
                        Brick_Image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/brick.png"));

                        Rectangle _brick = new Rectangle()
                        {
                            Width = 25,
                            Height = 25,
                            Tag = "Brick" + i * 200 + j,
                            Fill = Brick_Image,
                        };

                        Canvas.SetTop(_brick, StartPosition[1] + i * 25 - 1);
                        Canvas.SetLeft(_brick, StartPosition[0] + j * 25 - 1);
                        Canvas.SetZIndex(_brick, -5);
                        MainCanvas.Children.Add(_brick);
                    }
                }
        }
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
        private void PressKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
                Movement = Direction.Up;

            if (e.Key == Key.A)
                Movement = Direction.Left;

            if (e.Key == Key.S)
                Movement = Direction.Down;

            if (e.Key == Key.D)
                Movement = Direction.Right;

            if (e.Key == Key.Space)
            {
                //Bomb
                ImageBrush Bomb_Image = new ImageBrush();
                Bomb_Image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/bomb.png"));
                //Brush BodyBrush = new SolidColorBrush(Colors.Blue);

                Rectangle _bomb = new Rectangle()
                {
                    Tag = "Bomb" + Convert.ToString(BombRemains),
                    Height = 20,
                    Width = 20,
                    Fill = Bomb_Image
                };

                if (BombRemains > 0 && (map[Player_Row, Player_Column] == 0 || map[Player_Row, Player_Column] == 10 || map[Player_Row, Player_Column] == 2))
                {
                    Canvas.SetTop(_bomb, Canvas.GetTop(Player) + Player.Height / 10);
                    Canvas.SetLeft(_bomb, Canvas.GetLeft(Player) + Player.Width / 10);
                    MainCanvas.Children.Add(_bomb);
                    Rect box = new Rect()
                    {
                        X = Player_Row * Speed + 7,
                        Y = Player_Column * Speed + 13,
                        Width = 25,
                        Height = 25
                    };

                    Bomb _Bomb = new Bomb(_bomb.Tag, GameTime + 2f, MaxRangePowerUp + 1, Player_Row, Player_Column, box);
                    BombBackPack.Add(_Bomb);
                    BombTracking.Add(_bomb);

                    map[_Bomb.PosY, _Bomb.PosX] = 6;
                    BombRemains -= 1;
                }
            }
        }
        public void ICreate(Item i, int x, int y)
        {
            i.position = y * 100 + x;
            ItemList.Add(i);
        }
        public void IUse(Item n)
        {

            switch (n.name)
            {                    
                case "MaxBomb":
                    MaxBombPowerUp += 1;
                    score += 6;
                    BombRemains += 1;
                    ItemList.Remove(n);
                    mapUpdate(Player_Row, Player_Column, 0);
                    break;
                case "MaxRange":
                    MaxRangePowerUp += 1;
                    score += 6;
                    ItemList.Remove(n);
                    mapUpdate(Player_Row, Player_Column, 0);
                    break;
                case "Key":
                    score += 750;
                    GetKey = true;
                    ImageBrush Key_Image = new ImageBrush();
                    Key_Image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/key.png"));
                    KeyTag.Fill = Key_Image;
                    ItemList.Remove(n);
                    mapUpdate(Player_Row, Player_Column, 0);
                    break;
                case "Door":
                    EndGameCheck(Player_Status, wincondition);
                    break;
            }
        }

        //private void ReleaseKey(object sender, KeyEventArgs e) {  }
        public void mapUpdate(int ey, int ex, int AD) //Add or Remove powerup
        {
            Rectangle _empty = new Rectangle()
            {
                Height = 25.25,
                Width = 25,
                Fill = Brushes.Green
            };

            Canvas.SetTop(_empty, ey * Speed + 11);
            Canvas.SetLeft(_empty, ex * Speed + 6);

            if (AD == 0)
            {
                Canvas.SetZIndex(_empty, -2);
                MainCanvas.Children.Add(_empty);
                map[ey, ex] = 2;
            }
            if (AD == 1)
            {
                BrickBreak++;
                Canvas.SetZIndex(_empty, -3);
                MainCanvas.Children.Add(_empty);
                type = maprand.Next(0, 100);
                rate = (6 * PowerUpTotal) + 12;
                if (rate > 72)
                    rate = rate * (rate / (6 * (PowerUpTotal + 1)));
                if (rate > 99)
                    rate = 100;
                if (type < ((double)BrickBreak/BrickCount) * 100) //spawn key or door
                {
                    ImageBrush Door_Image = new ImageBrush();
                    Door_Image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/door.png"));
                    ImageBrush Key_Image = new ImageBrush();
                    Key_Image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/key.png"));
                    
                    if ((type >= 98 && !DoorExist) || (95 < ((double)BrickBreak / BrickCount) * 100 && !DoorExist)) //spawn door (must)
                    {
                        DoorExist = true;
                        map[ey, ex] = 12;
                        Rectangle _door = new Rectangle()
                        {
                            Width = 15,
                            Height = 15,
                            Fill = Door_Image
                        };
                        Canvas.SetTop(_door, ey * Speed + 15);
                        Canvas.SetLeft(_door, ex * Speed + 9);
                        Canvas.SetZIndex(_door, -2);
                        MainCanvas.Children.Add(_door);

                        EGeneral = new EndItems("Door", 0);
                        ICreate(EGeneral, ex, ey);
                    }
                   
                    if ((type % 4 == 1 && !KeyExist) || (85 < ((double)BrickBreak / BrickCount) * 100 && !KeyExist))
                    { 
                        KeyExist = true;
                        map[ey, ex] = 11;
                        Rectangle _key = new Rectangle()
                        {
                            Width = 15,
                            Height = 15,
                            Fill = Key_Image
                        };
                        Canvas.SetTop(_key, ey * Speed + 15);
                        Canvas.SetLeft(_key, ex * Speed + 9);
                        Canvas.SetZIndex(_key, -2);
                        MainCanvas.Children.Add(_key);

                        EGeneral = new EndItems("Key", 0);
                        ICreate(EGeneral, ex, ey);
                    }
                }
                else //possible to spawn power ups
                {
                    if (type > rate)
                    {
                        if (type % 2 == 0)
                        {
                            PowerUpTotal++;
                            ImageBrush ItemBlastRaadius_Image = new ImageBrush();
                            ItemBlastRaadius_Image.ImageSource =
                                new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/ItemBlastRadius.png"));

                            Rectangle _powerup1 = new Rectangle()
                            {
                                Width = 15,
                                Height = 15,
                                Fill = ItemBlastRaadius_Image
                            };

                            Canvas.SetTop(_powerup1, ey * Speed + 15);
                            Canvas.SetLeft(_powerup1, ex * Speed + 9);
                            Canvas.SetZIndex(_powerup1, -2);
                            MainCanvas.Children.Add(_powerup1);

                            PGeneral = new PowerUp("MaxRange", 0);
                            ICreate(PGeneral, ex, ey);
                        }
                        else
                        {
                            PowerUpTotal++;
                            ImageBrush ItemExtraBomb_Image = new ImageBrush();
                            ItemExtraBomb_Image.ImageSource =
                                new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/ItemExtraBomb.png"));

                            Rectangle _powerup2 = new Rectangle()
                            {
                                Width = 15,
                                Height = 15,
                                Fill = ItemExtraBomb_Image
                            };

                            Canvas.SetTop(_powerup2, ey * Speed + 15);
                            Canvas.SetLeft(_powerup2, ex * Speed + 9);
                            Canvas.SetZIndex(_powerup2, -2);
                            MainCanvas.Children.Add(_powerup2);

                            PGeneral = new PowerUp("MaxBomb", 0);
                            ICreate(PGeneral, ex, ey);
                        }
                    }

                }
            }
        }
        private void Explosion(int ex, int ey, Direction dir, int range) // (y, x), true implment hitbox, false remove it 
        {
            //Explosion shown here
            if (range <= 0 || ex < 0 || ey < 0 || ex > 29 || ey > 21)
                return;
            switch (dir)
            {
                case (Direction.Up):
                    ey -= 1;
                    break;
                case (Direction.Down):
                    ey += 1;
                    break;
                case (Direction.Left):
                    ex -= 1;
                    break;
                case (Direction.Right):
                    ex += 1;
                    break;
                default: //Idle
                    break;
            }

            if (map[ey, ex] == 1)
                return;

            

            Rectangle ExplosionAnimation = new Rectangle()
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red, 
            };

            Rect detect = new Rect()
            {
                X = ey * Speed + 7,
                Y = ex * Speed + 13,
                Width = 15,
                Height = 15
            };


            for (int i = 0; i < BombBackPack.Count; i++)
            {
                if (detect.IntersectsWith(BombBackPack[i].B_Hit))
                {
                    map[BombBackPack[i].PosY, BombBackPack[i].PosX] = 0;
                    score += 5;
                    BombBackPack[i].BombExplodeTime = 0f;
                }
            }

            if (ItemList != null)
            {
                for (int i = 0; i < ItemList.Count; i++)
                {
                    if (ItemList[i].position == ey * 100 + ex && ItemList[i] is PowerUp)
                    {
                        ItemList.RemoveAt(i);
                        score += 1;
                        mapUpdate(ey, ex, 0);
                    }
                }
            }

            if (EnemyList != null)
            {
                for (int i = 0; i < EnemyList.Count; i++)
                {
                    if (detect.IntersectsWith(EnemyList[i].E_Hit))
                    {
                        EnemyList[i].canvas.Fill = Brushes.Green;
                        EnemyList.RemoveAt(i);
                        score += 50;
                        mapUpdate(ey, ex, 0);
                    }
                }
            }
            Canvas.SetTop(ExplosionAnimation, ey * Speed + 13);
            Canvas.SetLeft(ExplosionAnimation, ex * Speed + 7);
            Canvas.SetZIndex(ExplosionAnimation, 0);
            MainCanvas.Children.Add(ExplosionAnimation);
            _ExplosionVar.Add(new ExplosionVar(GameTime + 1f, ey, ex, ExplosionAnimation));



            if (map[ey, ex] == 3)
            {
                mapUpdate(ey, ex, 1);

                score += 2;
            }
            else 
            { 
                map[ey, ex] = 8; 
            }

            Explosion(ex, ey, dir, range - 1);
        }
        private void Explode()
        {
            //Bomb Management 
            for (int i = 0; i < BombBackPack.Count; i++)
                if (GameTime >= BombBackPack[i].BombExplodeTime)
                    if (BombTracking.Count != 0)
                    {
                        MainCanvas.Children.Remove(BombTracking[BombExploded]);
                        if (Player_Row == BombBackPack[i].PosY && Player_Column == BombBackPack[i].PosX)
                        {
                            Player_Status = false;
                        }
                        map[BombBackPack[i].PosY, BombBackPack[i].PosX] = 0;

                        Explosion(BombBackPack[i].PosX, BombBackPack[i].PosY, Direction.Idle, 1);
                        Explosion(BombBackPack[i].PosX, BombBackPack[i].PosY, Direction.Up, BombBackPack[i].BombRange);
                        Explosion(BombBackPack[i].PosX, BombBackPack[i].PosY, Direction.Down, BombBackPack[i].BombRange);
                        Explosion(BombBackPack[i].PosX, BombBackPack[i].PosY, Direction.Left, BombBackPack[i].BombRange);
                        Explosion(BombBackPack[i].PosX, BombBackPack[i].PosY, Direction.Right, BombBackPack[i].BombRange);

                        BombBackPack.RemoveAt(i);
                        BombExploded += 1;
                        score += 10;
                        BombRemains += 1;
                    }

            for (int i = 0; i < _ExplosionVar.Count; i++)
                if (GameTime >= _ExplosionVar[i].ExplosionTime)
                    if (_ExplosionVar.Count != 0)
                    {
                        MainCanvas.Children.Remove(_ExplosionVar[i].canvas);
                        map[_ExplosionVar[i].PositionY, _ExplosionVar[i].PositionX] = 2;
                        _ExplosionVar.RemoveAt(i);
                    }

        }
        private void Messageblockcheck(MessageBoxResult r)
        {
            if (r == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
            else
            {
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }
        private void EndGameCheck(bool Player_Status, bool wincondition)
        {
            if (wincondition)
            {
                score += 5000;
                ScoreTag.Text = "Score: " + (int)score;
                MessageBoxResult ret = MessageBox.Show($"Time: {(int)GameTime}\nScore: {score}\nPress Yes to quit.\nPress No to restart.",
                "You Win!", MessageBoxButton.YesNo);
                Messageblockcheck(ret); 
            }
            if (!Player_Status)
            {
                ImageBrush Dead_Image = new ImageBrush();
                Dead_Image.ImageSource =
                    new BitmapImage(new Uri("pack://application:,,,/Properties/Sprites/dead.png"));

                Rectangle _dead = new Rectangle()
                {
                    Width = 20,
                    Height = 20,
                    Fill = Dead_Image,
                };

                Canvas.SetTop(_dead, min_Y + Player_Row * 25);
                Canvas.SetLeft(_dead, min_X + Player_Column * 25);
                Canvas.SetZIndex(_dead, 99);
                MainCanvas.Children.Add(_dead);

                MessageBoxResult ret = MessageBox.Show($"Time: {(int)GameTime}\nScore: {score}\nPress Yes to quit.\nPress No to restart.",
                    "You Dead!", MessageBoxButton.YesNo);
                Messageblockcheck(ret);
            }
        }
        private void GameLoop(object sender, EventArgs e)
        {

            GameTime += 0.02; // Default 20 mili second for one tick
            Rect PlayerHitBox = new Rect(Player_Row * 25 + 7, Player_Column * 25 + 13, Player.Width, Player.Height);
            Ticks++;
            for (int i = 0; i < EnemyList.Count; i++)
            {

                EnemyList[i].E_Hit = new Rect(min_Y + EnemyList[i].EposY * 25 + 5, min_X + EnemyList[i].EposX * 25 + 5, 15, 15);
                if (PlayerHitBox.IntersectsWith(EnemyList[i].E_Hit))
                {
                    Player_Status = false;
                }

                if (Ticks % 30 == 0)
                    EMove(EnemyList[i]);
            }


            if (map[Player_Row, Player_Column] == 8)
            {
                    Player_Status = false;
            }



            Player_Column = ((int)Canvas.GetLeft(Player) - 7) / Speed;
            Player_Row = ((int)Canvas.GetTop(Player) - 13) / Speed;
           
            Move(Player_Row, Player_Column, RightEnable, LeftEnable, DownEnable, UpEnable, Movement);

            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].position == Player_Row * 100 + Player_Column)
                { 
                    IUse(ItemList[i]);
                }
            }

            if (KeyExist && DoorExist)
                wincondition = true;

            Explode();
            // Numberic stuff update here
            TimerTag.Text = "Timer: " + (int)GameTime;
            ScoreTag.Text = "Score: " + (int)score;
            PowerUpTag.Text = "Bomb Range: " + ((int)MaxRangePowerUp + 1) + "\nBomb Available: " + (BombRemains);
            // End of each loop
            // MapUpdate();
            EndGameCheck(Player_Status, false);
            Movement = Direction.Idle;
        }
    }
}
