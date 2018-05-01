/*
 * Assignment:  Lab 2 - Asteroids
 * Written by:  Karun Kakulphimp
 * Date:        2018-03-14
 * Description: This program is the classic asteroids game, it features
 *              GDI objects and uses regions to detect collisions. In terms
 *              of game mechanics it will produce a #asteroids++ at a fixed
 *              amount of time increasing difficulty with number of asteroids
 *              added.
 *              keyboard                        xbox controller
 *              z to fire                       a to fire
 *              x to boost                      b to boost
 *              left and right arrows to move   left analog stick to move
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InputLibrary;

namespace Asteroids
{
    public partial class Asteroids : Form
    {
        //Game manager class handles all game operations
        private GameManager m_GameManager;
        private const int c_Timing = 17; //ms per frame
        //Game's main timer
        private Timer m_GameTimer;
        private static Random Random = new Random();
        public Asteroids()
        {
            InitializeComponent();
            //Same random for all classes
            GameManager.Random = Random;
            ShapeBase.Random = Random;
            //Game manager gets initial play area rectangle
            m_GameManager = new GameManager(ClientRectangle);
            m_GameTimer = new Timer();
            m_GameTimer.Interval = c_Timing;
            m_GameTimer.Enabled = true;
            m_GameTimer.Tick += GameTick;
            //Initialize the input abstration for player 1
            m_GameManager.PlayerInput = new InputAbstraction(0);
            //Pass key events to the player input class
            KeyDown += m_GameManager.PlayerInput.KeyboardDown;
            KeyUp += m_GameManager.PlayerInput.KeyboardUp;
        }
        /// <summary>
        /// Handle window graphics by passing off the backbuffer reference to the game manager's tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameTick(object sender, EventArgs e)
        {
            using (BufferedGraphicsContext bgc = new BufferedGraphicsContext())
            {
                using (BufferedGraphics backbuff = bgc.Allocate(CreateGraphics(), ClientRectangle))
                {
                    //Send the backbuffer graphics to the game manager
                    m_GameManager.Tick(backbuff.Graphics);
                    backbuff.Render();
                }
            }
        }
        /// <summary>
        /// Adjust game rectangle whenever the user resizes the form control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Asteroids_SizeChanged(object sender, EventArgs e)
        {
            m_GameManager.GameRectangle = ClientRectangle;
        }
    }
}
