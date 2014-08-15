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
        //ButtonState stepButtonState = ButtonState.Start;
        //ButtonState streakButtonState = ButtonState.Start;

        // Threads.
        Thread gameThread = null;
        //Thread stepThread = null;
        //Thread streakThread = null;

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
        BoardGameStrategy gameStrategy { get; set; }

        // Control GUI board flag.
        bool isGameBoardAvailabe = false;

        /* Step AI Game Mode Data. */

        // The step by step opponents.
        //AI stepPlayer1 = AI.Empty;
        //AI stepPlayer2 = AI.Empty;

        // The step by step AI strategies.
        //BoardGameStrategy stepStrategy1 { get; set; }
        //BoardGameStrategy stepStrategy2 { get; set; }

        // The step game core.
        //Connect4Core stepGame;

        // The list of all the single game board square.
        List<Label> stepSquares;

        /* Streak AI Game Mode Data. */

        // The choosen opponents.
        AI streakPlayer1 = AI.Empty;
        AI streakPlayer2 = AI.Empty;

        // The AI strategies.
        //BoardGameStrategy streakStrategy1 { get; set; }
        //BoardGameStrategy streakStrategy2 { get; set; }

        // The number of matches to play.
        int totalMatches = 0;
        int matchesLeft = 0;

        // Statistics.
        int draws = 0;
        int p1Wins = 0;
        int p2Wins = 0;

        // Is the streak ended?
        //bool streakEnded = false;

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
            if (p1Wins > p2Wins)
            {
                UpdateText(messageStreakLabel, streakPlayer1.ToString() + " is the winner!");
            }
            else if (p2Wins > p1Wins)
            {
                UpdateText(messageStreakLabel, streakPlayer2.ToString() + " is the winner!");
            }
            else
            {
                UpdateText(messageStreakLabel, "The competition is a draw!");
            }
            UpdateText(statisticsLabel, printStatistics());
        }

        private string printStatistics()
        {
            int matchesPlayed = totalMatches - matchesLeft;
            return
                "Game Streak Statistics\n" +
                "----------------------\n" +
                "Total matches played: " + matchesPlayed.ToString() +
                ".\n" +
                "Player 1 (" + streakPlayer1.ToString() + ") won " + p1Wins.ToString() +
                " matches " + "(" + ((float)p1Wins) / matchesPlayed * 100 + "%).\n" +
                "Player 2 (" + streakPlayer2.ToString() + ") won " + p2Wins.ToString() +
                " matches " + "(" + ((float)p2Wins) / matchesPlayed * 100 + "%).\n" +
                "Total draws " + draws +
                " (" + ((float)draws) / matchesPlayed * 100 + "%).";
        }

        /// <summary>
        /// Updates the square graphics.
        /// </summary>
        /// <param name="lbl">The label to change.</param>
        /// <param name="kind">The kind of the new label: -1 red, 1 yellow
        /// and 0 back to none.</param>
        private void UpdateSquare(Label lbl, int kind)
        {
            if (lbl.InvokeRequired)
            {
                UpdateSquareDelegate callbackMethod =
                    new UpdateSquareDelegate(UpdateSquare);
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
        private void singleGameStart()
        {
            game = new Connect4Core();
            foreach (Label square in gameSquares)
            {
                UpdateSquare(square, 0);
            }
            if (opponent.Equals(Opponent.Manual))
            {
                player1 = "Player 1";
                player2 = "Player 2";
                UpdateText(messageGameLabel, "Player 1 make your move!");
                isGameBoardAvailabe = true;
            }
            else
            {
                gameStrategy = initializeAI(opponent);
                if (first.Equals(First.Me))
                {
                    player1 = "You";
                    player2 = opponent.ToString();
                    UpdateText(messageGameLabel, "Make your moves!");
                    isGameBoardAvailabe = true;
                }
                else
                {
                    player1 = opponent.ToString();
                    player2 = "You";
                    aiMove();
                }
            }
        }

        private void aiMove()
        {
            // The AI make his move, return the related square index.
            int column = gameStrategy.OwnMove();
            if (game.CheckAndMove(column))
            {
                // The Ai move is valid, update the board.
                int square = game.GetLastSquareMove();
                Label label = indexToGameLabel(square);
                UpdateSquare(label, game.GetLastTurn());
                switch (game.CheckForWinner())
                {
                    case -2:
                        UpdateText(messageGameLabel, player1 + " won!");
                        isGameBoardAvailabe = false;
                        UpdateText(singleGameButton, "Reset");
                        break;
                    case 2:
                        UpdateText(messageGameLabel, player2 + " won!");
                        isGameBoardAvailabe = false;
                        UpdateText(singleGameButton, "Reset");
                        break;
                    case 1:
                        UpdateText(messageGameLabel, "This match is a draw!");
                        isGameBoardAvailabe = false;
                        UpdateText(singleGameButton, "Reset");
                        break;
                    default:
                        UpdateText(messageGameLabel, "Make your moves!");
                        isGameBoardAvailabe = true; 
                        break;
                }
            }
            else
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Initialize the AI strategy.
        /// Given an opponent, this method load the related
        /// strategy. The integer parameter "square" represent the
        /// first move of the player.
        /// </summary>
        /// <param name="opponent">The current opponent AI.</param>
        private BoardGameStrategy initializeAI(Opponent opponent)
        {
            BoardGameStrategy result = null;
            switch (opponent)
            {
                case Opponent.Manual:
                    // Player VS player, nothing to load.
                    break;
                case Opponent.Sheldon:
                    result = initSheldonAI();
                    break;
                case Opponent.Empty:
                    // Nothing to load.
                    break;
                default:
                    break;
            }
            return result;
        }

        private BoardGameStrategy initSheldonAI()
        {
            if (first.Equals(First.Me))
            {
                // Not enough memory to generate this strategy.
                // return new Connect4AlphaBetaStrategy(2);
                return new Connect4AlphaBetaLimitedStrategy(2);
            }
            else
            {
                // Not enough memory to generate this strategy.
                // return new Connect4AlphaBetaStrategy(1);
                return new Connect4AlphaBetaLimitedStrategy(1);
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

        private Label indexToGameLabel(int square)
        {
            return gameSquares.ElementAt(square);
        }

        private Label indexToStepLabel(int square)
        {
            return stepSquares.ElementAt(square);
        }

        /// <summary>
        /// Reset data and gui elements for the single game mode.
        /// </summary>
        private void singleGameReset()
        {
            isGameBoardAvailabe = false;
            clearBoard();
            UpdateText(messageGameLabel, "");
            player1 = "";
            player2 = "";
            return;
        }

        /// <summary>
        /// Clear the game board GUI.
        /// </summary>
        private void clearBoard()
        {
            foreach (Label square in gameSquares)
            {
                UpdateSquare(square, 0);
            }
        }

        /// <summary>
        /// Returns the player name who moves last.
        /// </summary>
        private string getLastPlayer()
        {
            int turn = game.GetLastTurn();
            if (turn == -1)
            {
                return player1;
            }
            else
            {
                return player2;
            }
        }

        /// <summary>
        /// Return the player name who moves next.
        /// </summary>
        /// <returns></returns>
        private string getNextPlayer()
        {
            int turn = game.GetNextTurn();
            if (turn == -1)
            {
                return player1;
            }
            else
            {
                return player2;
            }
        }

        /* Event handlers for the single game mode. */

        /// <summary>
        /// The user clicked a label to perform his move.
        /// </summary>
        private void gameLabel_Click(object sender, EventArgs e)
        {
            if (isGameBoardAvailabe)
            {
                Label clickedLabel = sender as Label;
                int column = labelToColumnIndex(clickedLabel);
                if (column < 0 || column > 6)
                {
                    MessageBox.Show("Error", "An Error occourred! We're Sorry...");
                }
                else if (game.CheckAndMove(column))
                {
                    // The move is sound, set the symbol on the board.
                    int index = game.GetLastSquareMove();
                    Label toUpdateLabel = gameSquares.ElementAt<Label>(index);
                    UpdateSquare(toUpdateLabel, game.GetLastTurn());
                    if (game.CheckVictory())
                    {
                        UpdateText(messageGameLabel, getLastPlayer() + " won!");
                    }
                    else if (game.GameOver())
                    {
                        UpdateText(messageGameLabel, "This match is a draw!");
                    }
                    else
                    {
                        if (opponent.Equals(Opponent.Manual))
                        {
                            UpdateText(messageGameLabel, getNextPlayer() +
                                " make your move!");
                        }
                        else
                        {
                            // Update the AI with the user move.
                            gameStrategy.OpponentMove(column);
                            // Let the AI play.
                            isGameBoardAvailabe = false;
                            aiMove();
                        }
                    }
                }
                else
                {
                    UpdateText(messageGameLabel, "The column is full, try again!");
                }
            }
        }

        /// <summary>
        /// The player select the opponent in the single game.
        /// </summary>
        private void selectOpponentBox_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (Enum.TryParse<Opponent>(selectOpponentBox.SelectedItem.ToString(),
                    true, out opponent))
            {
                switch (opponent)
                {
                    case Opponent.Manual:
                        UpdateText(descriptionLabel, "Player VS Player mode. ");
                        break;
                    case Opponent.Sheldon:
                        UpdateText(descriptionLabel, "You know, Sheldon is too"
                            + " smart for anyone. You can just draw a match"
                            + " against him, if You are good enough! ");
                        break;
                    //case Opponent.Penny:
                    //    UpdateText(descriptionLabel, "Ok, Penny is not the best"
                    //    + " player, but don't underestimate her. ");
                    //    break;
                    //case Opponent.Stuart:
                    //    UpdateText(descriptionLabel, "Stuart is not the smarter"
                    //    + " player, more aggressive than Penny, but still beatable. ");
                    //    break;
                    default:
                        UpdateText(messageGameLabel, "A problem occoured,"
                        + " please chose the opponent again");
                        break;
                }
            }
        }

        /// <summary>
        /// The player select who move first in the next single game.
        /// </summary>
        private void firstToMoveBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (firstToMoveBox.SelectedItem != null)
            {
                if (Enum.TryParse<First>(firstToMoveBox.SelectedItem.ToString(),
                    true, out first))
                {
                    switch (first)
                    {
                        case First.Me:
                            break;
                        case First.Opponent:
                            break;
                        default:
                            first = First.Empty;
                            UpdateText(messageGameLabel,
                                "A problem occoured, please choose" +
                                " the first to move again.");
                            break;
                    }
                }
            }
            else
            {
                first = First.Empty;
            }
        }

        /// <summary>
        /// The player click the button to start the single game.
        /// </summary>
        private void singleGameButton_Click(object sender, EventArgs e)
        {
            if (opponent.Equals(Opponent.Empty))
            {
                UpdateText(messageGameLabel, "Please select an Opponent.");
            }
            else if (opponent.Equals(Opponent.Manual))
            {
                handleSingleGameStart();
            }
            else if (first.Equals(First.Empty))
            {
                UpdateText(messageGameLabel, "Please select who move first.");
            }
            else
            {
                handleSingleGameStart();
            }

        }

        private void handleSingleGameStart()
        {
            if (gameButtonState.Equals(ButtonState.Start))
            {
                gameButtonState = switchButtonState(
                    gameButtonState,
                    singleGameButton);
                gameThread = new Thread(new ThreadStart(singleGameStart));
                gameThread.Start();
            }
            else
            {
                gameButtonState = switchButtonState(
                    gameButtonState,
                    singleGameButton);
                singleGameReset();
                gameThread.Abort();
            }
        }

    }
}
