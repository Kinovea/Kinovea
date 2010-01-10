/*
Copyright © Joan Charmant 2009.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/

using System;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// this class stores a state of a PlayerScreen in order to reinstate it later.
    /// </summary>
    public class MemoPlayerScreen
    {
        #region Properties
        public long SelStart
        {
            get { return m_iSelStart; }
            set { m_iSelStart = value;}
        }
        public long SelEnd
        {
            get { return m_iSelEnd; }
            set { m_iSelEnd = value; }
        }
        #endregion

        #region Members
        private long m_iSelStart;
        private long m_iSelEnd;
        #endregion

        public MemoPlayerScreen(long _iSelStart, long _iSelEnd)
        {
            m_iSelStart = _iSelStart;
            m_iSelEnd = _iSelEnd;
        }

    }
}

