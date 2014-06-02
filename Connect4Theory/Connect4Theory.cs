//
// Copyright (c) 2014 Fabio Biselli - fabio.biselli.80@gmail.com
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
//    1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
//
//    2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
//
//    3. This notice may not be removed or altered from any source
//    distribution.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using BoardGameCore;

namespace Connect4Theory
{
    /// <summary>
    /// Defines the type of opponent for the single game.
    /// </summary>
    enum Opponent
    {
        // Player VS Player.
        Manual,
        // Player VS AI.
        Sheldon,
        // Not yet defined.
        Empty
    }

    /// <summary>
    /// Defines the first to move for the single game.
    /// </summary>
    enum First
    {
        // The player.
        Me,
        // The AI.
        Opponent,
        // Not yet defined.
        Empty
    }

    /// <summary>
    /// Defines AI agents for the game tests.
    /// </summary>
    enum AI
    {
        // Defined AI.
        Sheldon,
        // Not yet defined.
        Empty
    }

    /// <summary>
    /// Define the state of a standard start/stop button.
    /// </summary>
    enum ButtonState
    {
        Start,
        Stop
    }

    public partial class Connect4Theory : Form
    {
        // Button states.
        ButtonState gameButtonState = ButtonState.Start;
        ButtonState stepButtonState = ButtonState.Start;
        ButtonState streakButtonState = ButtonState.Start;

        // Threads.
        Thread gameThread = null;
        Thread stepThread = null;
        Thread streakThread = null;

        // Delegates.
        delegate void UpdateTextDelegate(Control ctl, string text);
        delegate void UpdateEnabledDelegate(Control crl, bool p);
        delegate void ShowProgressDelegate(int result);
        delegate void UpdateResultsDelegate(int total);
        delegate void InitStatisticsDelegate(int total);
        delegate void UpdateMaximumDelegate(ProgressBar ctl, int total);
        delegate void UpdateSquareDelegate(Label lbl, int kind);

        /* Single Game Mode Data. */

        // The single game core.
        Connect4Core game;

        // The players names.
        string player1;
        string player2;

        // The list of all the single game board square.
        List<Label> gameSquares;

        // The choosen opponent.
        Opponent opponent = Opponent.Empty;

        // The first to move.
        First first = First.Empty;

        // The AI strategy.
        //Connect4Strategy strategy { get; set; }

        /* Step AI Game Mode Data. */

        // The single game core.
        Connect4Core stepGame;

        // The list of all the single game board square.
        List<Label> stepSquares;

        /* Streak AI Game Mode Data. */

        public Connect4Theory()
        {
            InitializeComponent();
            gameSquares = new List<Label>() {
                gameLabel00, gameLabel01, gameLabel02, gameLabel03,
                gameLabel04, gameLabel05, gameLabel06, gameLabel07,
                gameLabel08, gameLabel09, gameLabel10, gameLabel11,
                gameLabel12, gameLabel13, gameLabel14, gameLabel15,
                gameLabel16, gameLabel17, gameLabel18, gameLabel19,
                gameLabel20, gameLabel21, gameLabel22, gameLabel23,
                gameLabel24, gameLabel25, gameLabel26, gameLabel27,
                gameLabel28, gameLabel29, gameLabel30, gameLabel31,
                gameLabel32, gameLabel33, gameLabel34, gameLabel35,
                gameLabel36, gameLabel37, gameLabel38, gameLabel39,
                gameLabel40, gameLabel41
            };
            stepSquares = new List<Label>() {
                stepLabel00, stepLabel01, stepLabel02, stepLabel03,
                stepLabel04, stepLabel05, stepLabel06, stepLabel07,
                stepLabel08, stepLabel09, stepLabel10, stepLabel11,
                stepLabel12, stepLabel13, stepLabel14, stepLabel15,
                stepLabel16, stepLabel17, stepLabel18, stepLabel19,
                stepLabel20, stepLabel21, stepLabel22, stepLabel23,
                stepLabel24, stepLabel25, stepLabel26, stepLabel27,
                stepLabel28, stepLabel29, stepLabel30, stepLabel31,
                stepLabel32, stepLabel33, stepLabel34, stepLabel35,
                stepLabel36, stepLabel37, stepLabel38, stepLabel39,
                stepLabel40, stepLabel41
            };
        }

        /* Delegators helpers. */

        private void UpdateText(Control ctl, string text)
        {
            if (ctl.InvokeRequired)
            {
                UpdateTextDelegate callbackMethod =
                    new UpdateTextDelegate(UpdateText);
                this.Invoke(callbackMethod, ctl, text);
            }
            else
            {
                ctl.Text = text;
            }
        }

        private void UpdateEnabled(Control ctl, bool p)
        {
            if (ctl.InvokeRequired)
            {
                UpdateEnabledDelegate callbackMethod =
                    new UpdateEnabledDelegate(UpdateEnabled);
                this.Invoke(callbackMethod, ctl, p);
            }
            else
            {
                ctl.Enabled = p;
            }
        }

        private void ShowProgress(int result)
        {
            switch (result)
            {
                case -2:
                    progressP1.PerformStep();
                    break;
                case 2:
                    progressP2.PerformStep();
                    break;
                case 1:
                    progressDraws.PerformStep();
                    break;
                default:
                    break;
            }
            totalGameProgress.PerformStep();
        }

        private void UpdateResults(int total)
        {
            /// TODO
            //if (p1Wins > p2Wins)
            //{
            //    UpdateText(messageTestsLabel, aiPlayer1.ToString() + " is the winner!");
            //}
            //else if (p2Wins > p1Wins)
            //{
            //    UpdateText(messageTestsLabel, aiPlayer2.ToString() + " is the winner!");
            //}
            //else
            //{
            //    UpdateText(messageTestsLabel, "The competition is a draw!");
            //}
            //UpdateText(statisticsLabel, printStatistics());
        }

        private void UpdateSquare(Label lbl, int kind)
        {
            if (lbl.InvokeRequired)
            {
                UpdateEnabledDelegate callbackMethod =
                    new UpdateEnabledDelegate(UpdateEnabled);
                this.Invoke(callbackMethod, lbl, kind);
            }
            else
            {
                if (kind == -1)
                {
                    lbl.BackColor = Color.Crimson;
                }
                else if (kind == 1)
                {
                    lbl.BackColor = Color.Gold;
                }
                else
                {
                    lbl.BackColor = Color.LightSteelBlue;
                }
            }
        }

        private void UpdateMaximum(ProgressBar ctl, int total)
        {
            if (ctl.InvokeRequired)
            {
                UpdateMaximumDelegate callbackMethod =
                    new UpdateMaximumDelegate(UpdateMaximum);
                this.Invoke(callbackMethod, ctl, total);
            }
            else
            {
                ctl.Maximum = total;
            }
        }

        /* Class functions and methods. */

        /// <summary>
        /// Switch the state of a given button.
        /// 
        /// Usage:
        /// <code>buttonStateOut = switchButtonState(buttonStateIn);</code>
        /// </summary>
        /// <param name="state">The input button state.</param>
        /// <returns>The opposite input button state.</returns>
        private ButtonState switchButtonState(ButtonState state, Control btn)
        {
            if (state.Equals(ButtonState.Start))
            {
                UpdateText(btn, "Stop");
                return ButtonState.Stop;
            }
            else
            {
                UpdateText(btn, "Start");
                return ButtonState.Start;
            }
        }

        /* Single Game methods. */

        /// <summary>
        /// Start the single game.
        /// </summary>
        private void SingleGameStart()
        {
            stepGame = new Connect4Core();
            foreach (Label square in gameSquares)
            {
                UpdateSquare(square, 0);
            }
            if (opponent.Equals(Opponent.Manual))
            {
                player1 = "Player 1";
                player2 = "Player 2";
                UpdateText(messageGameLabel, "Player 1 make your move!");
            }
            else
            {
                if (first.Equals(First.Me))
                {
                    player1 = "You";
                    player2 = opponent.ToString();
                    UpdateText(messageGameLabel, "Make your moves!");
                }
                else
                {
                    player1 = opponent.ToString();
                    player2 = "You";
                }
                //initializeAI(opponent);
                //aiMove();
            }
        }

        /* Auxiliary Methods. */

        /// <summary>
        /// Given a label representing a square of the game board
        /// this method returns the column index.
        /// </summary>
        /// <param name="label">The board square label.</param>
        /// <returns>The index of the board representation.</returns>
        private int labelToColumnIndex(Label label)
        {
            string labelName = label.Name;
            // gameLabelXY and stepLabelXY keep index on the last two chars.
            char[] toConvert = labelName.ToCharArray(9, 2);
            string toParse = new string(toConvert);
            return (int.Parse(toParse) % 7);
            
        }

    }
}
