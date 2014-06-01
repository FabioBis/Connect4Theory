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
        public Connect4Theory()
        {
            InitializeComponent();
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
