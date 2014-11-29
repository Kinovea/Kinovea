using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// A simple wrapper around a media type and a framerate.
    /// This correspond to one selectable option in the UI.
    /// </summary>
    public class MediaTypeSelection
    {
        public MediaType MediaType { get; private set; }
        public float Framerate { get; private set; }

        public MediaTypeSelection(MediaType mediaType, float framerate)
        {
            this.MediaType = mediaType;
            this.Framerate = framerate;
        }

        public override string ToString()
        {
            return string.Format("{0:0.000}", Framerate);
        }

    }
}
