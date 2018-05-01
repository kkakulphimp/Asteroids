/*
 *Description:  This file handles all came functionality
 *              It becomes instantiated by the main form thread
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using InputLibrary;
using System.Media;
using System.Diagnostics;
using AxWMPLib;
using System.IO;

namespace Asteroids
{
    /// <summary>
    /// Game's operational states
    /// </summary>
    public enum GameState
    {
        //Menu is the first thing the user sees
        Menu,
        //Game has started
        Started,
        //Game is paused from started
        Paused,
        //User has lost all lives
        Gameover
    }
    class GameManager
    {
        private List<ShapeBase> m_ShapeList = new List<ShapeBase>();
        private Ship m_Ship;
        private const int c_FadeIterations = 100;
        /// <summary>
        /// Game operates within the rectangle's parameters
        /// </summary>
        public Rectangle GameRectangle { get; set; }
        /// <summary>
        /// Game's RNG
        /// </summary>
        public static Random Random { get; set; }
        private GameState m_CurrentState = GameState.Menu;
        /// <summary>
        /// Player 1's input
        /// </summary>
        public InputAbstraction PlayerInput { get; set; }
        //Handle button and state transitions to only trigger on one press
        private bool m_PauseTransition = false;
        private bool m_StateTransition = false;
        private bool m_FireTransition = false;
        //Fonts used by game
        private Font m_BigFont = new Font("Impact", 100);
        private Font m_MediumFont = new Font("Impact", 50);
        private Font m_SmallFont = new Font("Impact", 25);
        //Stringformat for when I want the text centered within rectangle
        private StringFormat centeredString = new StringFormat();
        //Player's score
        private int m_Score = 0;
        //Number of lives left
        private int m_Lives = 3;
        //Number of bullets on screen to ensure 
        private int m_BulletsOnScreen = 0;
        //Game sounds for specific actions
        private SoundPlayer m_ExplosionSound = new SoundPlayer();
        private SoundPlayer m_LaserSound = new SoundPlayer();
        private SoundPlayer m_WowSound1 = new SoundPlayer();
        private SoundPlayer m_WowSound2 = new SoundPlayer();
        private SoundPlayer m_Confirmation = new SoundPlayer();
        //Game music gets played separately to play sounds simultaneously
        private AxWindowsMediaPlayer m_MediaPlayer = new AxWindowsMediaPlayer();
        //This is the location where the files get saved
        private string m_EpicURL = "sounds/bensound-epic.mp3";
        private string m_SlowURL = "sounds/bensound-slowmotion.mp3";
        //Handle the time elapsed between asteroid spawns
        private Stopwatch m_GameTime = new Stopwatch();
        //Asteroids to spawn at the next spawn iteration, will increase per iteration
        private int m_SpawnCount = 1;
        private int m_SpawnDelay = 30000; //ms per spawn
        //Color which will change
        private Color m_PromptColor = Color.Red;
        //Stars to be drawn on the play area
        private List<Star> m_StarList = new List<Star>();
        private const int c_StarCount = 50;
        private const int c_MediumRocksPerLarge = 2;
        private const int c_SmallRocksPerMedium = 1;
        //Life per 10000 points variables
        private int m_LifeCounter = 1;
        private const int c_PointsPerLife = 10000;

        /// <summary>
        /// Main game code
        /// </summary>
        /// <param name="rectangle"></param>
        public GameManager(Rectangle rectangle)
        {
            GameRectangle = rectangle;
            centeredString.Alignment = StringAlignment.Center;
            //Sound effects from freesound.org
            m_LaserSound.Stream = Properties.Resources.laser;
            m_ExplosionSound.Stream = Properties.Resources.explosion;
            m_WowSound1.Stream = Properties.Resources.wow1;
            m_WowSound2.Stream = Properties.Resources.wow2;
            m_Confirmation.Stream = Properties.Resources.confirmation;
            m_MediaPlayer.CreateControl();
            m_MediaPlayer.Visible = false;
            //Royalty Free Music from Bensound
            m_MediaPlayer.URL = m_SlowURL;
            m_MediaPlayer.settings.setMode("loop", true);
            //m_MediaPlayer.URL = @"../sounds/bensound-slowmotion.mp3";
            for (int i = 0; i < c_StarCount; i++)
            {
                m_StarList.Add(new Star(new PointF(Random.Next(0, GameRectangle.Width), Random.Next(0, GameRectangle.Height))));
            }
        }
        /// <summary>
        /// Tick does one iteration of the game mechanics and rendering
        /// </summary>
        /// <param name="graphics"></param>
        public void Tick(Graphics graphics)
        {
            m_StarList.ForEach(q => q.Render(graphics));
            switch (m_CurrentState)
            {
                //Menu state just shows title and a prompt to begin
                case GameState.Menu:
                    graphics.DrawString("Asteroids", m_BigFont, new SolidBrush(Color.LightGray), GameRectangle, centeredString);
                    m_PromptColor = ColorAnimation(m_PromptColor);
                    graphics.DrawString($"\n\n\nPress {(PlayerInput.m_connected ? "Start":"P")} to begin!", m_MediumFont, new SolidBrush(m_PromptColor), GameRectangle, centeredString);
                    if (PlayerInput.m_start && !m_StateTransition)
                    {
                        //Initialize game
                        m_CurrentState = GameState.Started;
                        m_Lives = 3;
                        m_Score = 0;
                        m_Ship = new Ship(new PointF(GameRectangle.Width / 2, GameRectangle.Height / 2), Color.Red, c_FadeIterations);
                        m_ShapeList.Add(m_Ship);
                        m_GameTime.Restart();
                        m_SpawnCount = 1;
                        m_LifeCounter = 1;
                        for (int i = 0; i < 5; i++)
                        {
                            m_ShapeList.Add(new Rock(new PointF(Random.Next(0, GameRectangle.Width), Random.Next(0, GameRectangle.Height)), Size.Large, c_FadeIterations));
                        }
                        m_StateTransition = true;
                        m_Confirmation.Play();
                        m_MediaPlayer.URL = m_EpicURL;
                    }
                    if (m_StateTransition && !PlayerInput.m_start)
                        m_StateTransition = false;
                    break;
                //Once started the game has the centered ship as well as a set number of asteroids
                case GameState.Started:
                    //Player actions start here
                    //Pause handle
                    if (PlayerInput.m_start && !m_PauseTransition && !m_StateTransition)
                    {
                        m_CurrentState = GameState.Paused;
                        m_PauseTransition = true;
                        m_Confirmation.Play();
                    }
                    if (m_PauseTransition && !PlayerInput.m_start)
                    {
                        m_PauseTransition = false;
                    }
                    if (m_StateTransition && !PlayerInput.m_start)
                    {
                        m_StateTransition = false;
                    }
                    //Fire Handle
                    if (PlayerInput.m_a && !m_FireTransition && m_BulletsOnScreen < 8)
                    {
                        m_LaserSound.Play();
                        m_ShapeList.Add(new Bullet(m_Ship.GunPortLocation, m_Ship.AngleDeg, 10));
                        m_FireTransition = true;
                    }
                    if (m_FireTransition && !PlayerInput.m_a)
                    {
                        m_FireTransition = false;
                    }
                    //Boost Handle
                    if (PlayerInput.m_b)
                    {
                        m_Ship.Boost();
                    }
                    else
                    {
                        m_Ship.Decay();
                    }
                    //Rotate Ship
                    m_Ship.AngleDeg += PlayerInput.m_xDir * 5;

                    //Add score text
                    graphics.DrawString($"Score: {m_Score.ToString()}\nLives: {m_Lives}", m_SmallFont, new SolidBrush(Color.LightGray), GameRectangle);
                    //Do all shape movement based on bounds
                    m_ShapeList.ForEach(q => q.Tick(GameRectangle));

                    //Reset bullets on screen
                    m_BulletsOnScreen = 0;
                    List<ShapeBase> mirrorList = new List<ShapeBase>();
                    //Label the rocks for their duplicates
                    int identifier = 0;
                    //Main mirror code entry here
                    foreach (ShapeBase thing in m_ShapeList)
                    {
                        if (!(thing is Bullet))
                        {
                            thing.ID = identifier++;
                            //Find closest edges for mirroring
                            BoundProximity bp = thing.EdgeProximity(GameRectangle);
                            if (bp.proxSum.Equals(2))
                            {
                                ShapeBase[] cornerMirrors = new ShapeBase[3];
                                //Make three clones
                                for (int i = 0; i < cornerMirrors.Length; i++)
                                {
                                    cornerMirrors[i] = (ShapeBase)thing.Clone();
                                }
                                if (bp.left)
                                {
                                    /*     Three projections are needed in a corner
                                     *     
                                     *     o______o      ______
                                     *    |      |      |x     |o
                                     *    |      |      |      |
                                     *    |x_____|o     |__ ___|
                                     *                   o      o
                                     */
                                    cornerMirrors[0].TranslateX(GameRectangle.Width);
                                    cornerMirrors[2].TranslateX(GameRectangle.Width);
                                    if (bp.top)
                                    {
                                        cornerMirrors[1].TranslateY(-GameRectangle.Height);
                                        cornerMirrors[2].TranslateY(-GameRectangle.Height);
                                    }
                                    else
                                    {
                                        cornerMirrors[1].TranslateY(GameRectangle.Height);
                                        cornerMirrors[2].TranslateY(GameRectangle.Height);
                                    }
                                }
                                else
                                {
                                    /*     
                                     *   o______o        ______
                                     *    |      |     o|     x|
                                     *    |      |      |      |
                                     *   o|_____x|      |_____ |
                                     *                 o      o
                                     */
                                    cornerMirrors[0].TranslateX(-GameRectangle.Width);
                                    cornerMirrors[2].TranslateX(-GameRectangle.Width);
                                    if (bp.top)
                                    {
                                        cornerMirrors[1].TranslateY(-GameRectangle.Height);
                                        cornerMirrors[2].TranslateY(-GameRectangle.Height);
                                    }
                                    else
                                    {
                                        cornerMirrors[1].TranslateY(GameRectangle.Height);
                                        cornerMirrors[2].TranslateY(GameRectangle.Height);
                                    }
                                }
                                mirrorList.AddRange(cornerMirrors);
                            }
                            else if (bp.proxSum.Equals(1))
                            {
                                //Regular mirrors without two potential intersections
                                ShapeBase mirror = (ShapeBase)thing.Clone();
                                if (bp.left) mirror.TranslateX(GameRectangle.Width);
                                if (bp.right) mirror.TranslateX(-GameRectangle.Width);
                                if (bp.top) mirror.TranslateY(-GameRectangle.Height);
                                if (bp.bottom) mirror.TranslateY(GameRectangle.Height);
                                mirrorList.Add(mirror);
                            }
                        }
                        else
                        {
                            m_BulletsOnScreen++;
                        }
                    }
                    //Do collision checks
                    List<ShapeBase> all = new List<ShapeBase>();
                    List<Tuple<ShapeBase, ShapeBase>> closePairs = new List<Tuple<ShapeBase, ShapeBase>>();
                    all.AddRange(m_ShapeList);
                    all.AddRange(mirrorList);
                    //Sort the collection by type (ship > bullet > rock) to make tuple comparisons easier
                    all = (from q in all orderby ShapeBase.SortHelper(q) select q).ToList();
                    //Iterate each object with others
                    for (int i = 0; i < all.Count; i++)
                    {
                        for (int j = i+1; j < all.Count; j++)
                        {
                            //Only different type pairs in proximity matter
                            if (all[i].GetType() != all[j].GetType() && ShapeBase.DistanceTest(all[i], all[j]))
                            {
                                closePairs.Add(new Tuple<ShapeBase, ShapeBase>(all[i], all[j]));
                            }
                        }
                    }
                    foreach (Tuple<ShapeBase,ShapeBase> thing in closePairs)
                    {
                        //Rock to anything else interaction
                        if (thing.Item2 is Rock)
                        {
                            //Get bullet or ship into region
                            Region region = new Region(thing.Item1.GetPath().Dequeue().Item1);
                            //Intersect with rock
                            region.Intersect(thing.Item2.GetPath().Dequeue().Item1);
                            if (!region.IsEmpty(graphics))
                            {
                                //Bullet - Rock interaction
                                if (thing.Item1 is Bullet)
                                {
                                    //Play explosion sound
                                    m_ExplosionSound.Play();
                                    //Bullet is the same regardless of collection
                                    thing.Item1.MarkedForDeath = true;
                                    //Pull the offending rock
                                    Rock deadRock = (Rock)m_ShapeList.Find(q => q.ID.Equals(thing.Item2.ID));
                                    //Split rocks, one large into two mediums
                                    if (deadRock.Size.Equals(Size.Large))
                                    {
                                        for (int i = 0; i < 2; i++)
                                            m_ShapeList.Add(new Rock(deadRock.Location, Size.Medium, 0));
                                        m_Score += 100;
                                    }
                                    //One medium into three smalls
                                    else if (deadRock.Size.Equals(Size.Medium))
                                    {
                                        for (int i = 0; i < 3; i++)
                                            m_ShapeList.Add(new Rock(deadRock.Location, Size.Small, 0));
                                        m_Score += 200;
                                    }
                                    else
                                        m_Score += 300;

                                    deadRock.MarkedForDeath = true;
                                }
                                //Other case is a pair of unfaded ship and rock, then the player dun goofed and got hit
                                else if (thing.Item1.Fade.Equals(0) && thing.Item2.Fade.Equals(0))
                                {
                                    //Play explosion sound
                                    m_ExplosionSound.Play();
                                    //Reset fade
                                    m_Ship.Fade = c_FadeIterations;
                                    m_Lives--;
                                    //Determine gameover state
                                    if (m_Lives.Equals(0))
                                    {
                                        m_CurrentState = GameState.Gameover;
                                        m_WowSound1.Play();
                                    }
                                        
                                }
                            }
                        }
                    }
                    //This code was in here before but I forgot to commit it, handles points per life
                    if (m_Score / ((float)c_PointsPerLife * m_LifeCounter) > 1)
                    {
                        m_Lives++;
                        m_LifeCounter++;
                        m_WowSound2.Play();
                    }

                    //Do all renders on collections, remove dead things
                    mirrorList.ForEach(q => q.Render(graphics));
                    m_ShapeList.RemoveAll(q => q.MarkedForDeath);
                    m_ShapeList.ForEach(q => q.Render(graphics));
                    if (m_GameTime.ElapsedMilliseconds > m_SpawnDelay)
                    {
                        //Timer gets +1 rock on ever add, start at one
                        m_GameTime.Restart();
                        for (int i = 0; i < m_SpawnCount; i++)
                        {
                            m_ShapeList.Add(new Rock(new PointF(Random.Next(0, GameRectangle.Width), Random.Next(0, GameRectangle.Height)), Size.Large, c_FadeIterations));
                        }
                        m_SpawnCount++;
                    }
                    break;
                case GameState.Paused:
                    //Unpause handle
                    if (m_PauseTransition && !PlayerInput.m_start)
                    {
                        m_PauseTransition = false;
                    }
                    if (PlayerInput.m_start && !m_PauseTransition)
                    {
                        m_CurrentState = GameState.Started;
                        m_PauseTransition = true;
                        m_Confirmation.Play();
                    }
                    //Pause screen
                    graphics.DrawString("Asteroids", m_BigFont, new SolidBrush(Color.LightGray), GameRectangle, centeredString);
                    graphics.DrawString("\n\n\nPaused", m_MediumFont, new SolidBrush(Color.Red), GameRectangle, centeredString);
                    break;
                //Game over shows the final score and a prompt to return to the menu screen
                case GameState.Gameover:
                    if (PlayerInput.m_start)
                    {
                        m_CurrentState = GameState.Menu;
                        m_StateTransition = true;
                        m_MediaPlayer.URL = m_SlowURL;
                        m_Confirmation.Play();
                    }
                    m_ShapeList.Clear();
                    //Show the final score and prompt the user to continue
                    graphics.DrawString($"Final score: {m_Score}", m_MediumFont, new SolidBrush(Color.Red), GameRectangle, centeredString);
                    m_PromptColor = ColorAnimation(m_PromptColor);
                    graphics.DrawString($"\n\nPress {(PlayerInput.m_connected ? "Start" : "P")} to continue", m_MediumFont, new SolidBrush(ColorAnimation(m_PromptColor)), GameRectangle, centeredString);
                    break;
            }
        }
        /// <summary>
        /// This is my little color looper animation code that I used on my javascript before
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private Color ColorAnimation(Color col)
        {
            byte red = col.R;
            byte green = col.G;
            byte blue = col.B;
            if (red == 255 && green == 0)
                blue += 5;
            if (blue == 255 && green == 0)
                red -= 5;
            if (blue == 255 && red == 0)
                green += 5;
            if (green == 255 && red == 0)
                blue -= 5;
            if (green == 255 && blue == 0)
                red += 5;
            if (red == 255 && blue == 0)
                green -= 5;
            return Color.FromArgb(255, red, green, blue);
        }
    }
}
