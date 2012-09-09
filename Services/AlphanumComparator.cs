/*
 * The Alphanum Algorithm is an improved sorting algorithm for strings
 * containing numbers.  Instead of sorting numbers in ASCII order like
 * a standard sort, this algorithm sorts numbers in numeric order.
 *
 * The Alphanum Algorithm is discussed at http://www.DaveKoelle.com
 *
 * Based on the Java implementation of Dave Koelle's Alphanum algorithm.
 * Contributed by Jonathan Ruckwood <jonathan.ruckwood@gmail.com>
 * 
 * Adapted by Dominik Hurnaus <dominik.hurnaus@gmail.com> to 
 *   - correctly sort words where one word starts with another word
 *   - have slightly better performance
 * 
 * [Joan Charmant] - Compare numerical chunks one char at a time to avoid
 * Int32 overflow.
 * Made it specific to sort filenames. Strings passed should be full path.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

/* 
 * Please compare against the latest Java version at http://www.DaveKoelle.com
 * to see the most recent modifications 
 */
namespace Kinovea.Services
{
    public class AlphanumComparator : IComparer<string>
    {
        private enum ChunkType {Alphanumeric, Numeric};
        
        private bool InChunk(char ch, char otherCh)
        {
            ChunkType type = ChunkType.Alphanumeric;

            if (char.IsDigit(otherCh))
            {
                type = ChunkType.Numeric;
            }

            if ((type == ChunkType.Alphanumeric && char.IsDigit(ch))
                || (type == ChunkType.Numeric && !char.IsDigit(ch)))
            {
                return false;
            }

            return true;
        }

        public int Compare(string x, string y)
        {
            String s1 = Path.GetFileNameWithoutExtension(x);
            String s2 = Path.GetFileNameWithoutExtension(y);

            if (s1 == null || s2 == null)
                return 0;

            int thisMarker = 0;
            int thatMarker = 0;

            while ((thisMarker < s1.Length) || (thatMarker < s2.Length))
            {
                if (thisMarker >= s1.Length)
                    return -1;
                else if (thatMarker >= s2.Length)
                    return 1;
                
                // Build next chunks for each string.
                char thisCh = s1[thisMarker];
                char thatCh = s2[thatMarker];

                StringBuilder thisChunk = new StringBuilder();
                while ((thisMarker < s1.Length) && (thisChunk.Length==0 || InChunk(thisCh, thisChunk[0])))
                {
                    thisChunk.Append(thisCh);
                    thisMarker++;

                    if (thisMarker < s1.Length)
                    {
                        thisCh = s1[thisMarker];
                    }
                }
                
                StringBuilder thatChunk = new StringBuilder();
                while ((thatMarker < s2.Length) && (thatChunk.Length==0 || InChunk(thatCh, thatChunk[0])))
                {
                    thatChunk.Append(thatCh);
                    thatMarker++;

                    if (thatMarker < s2.Length)
                    {
                        thatCh = s2[thatMarker];
                    }
                }

                // If both chunks contain numeric characters, sort them numerically
                int result = 0;
                if (char.IsDigit(thisChunk[0]) && char.IsDigit(thatChunk[0]))
                {
                    int thisChunkLength = thisChunk.Length;
                    result = thisChunkLength - thatChunk.Length;
                    
                    // If equal, the first different number counts.
                    if(result == 0)
                    {
                        for (int i = 0; i < thisChunkLength; i++)
                        {
                            result = thisChunk[i] - thatChunk[i];
                            if (result != 0)
                                return result;
                        }
                    }
                }
                else
                {
                    result = thisChunk.ToString().CompareTo(thatChunk.ToString());
                }

                if (result != 0)
                    return result;
            }

            return 0;
        }
    }
}
