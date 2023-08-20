using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public class OpenPosePerson
    {
        // Matches OpenPose format 1.3.

        public IList<int> person_id { get; set; }
        public IList<float> pose_keypoints_2d { get; set; }
        public IList<float> face_keypoints_2d { get; set; }
        public IList<float> hand_left_keypoints_2d { get; set; }
        public IList<float> hand_right_keypoints_2d { get; set; }
        public IList<float> pose_keypoints_3d { get; set; }
        public IList<float> face_keypoints_3d { get; set; }
        public IList<float> hand_left_keypoints_3d { get; set; }
        public IList<float> hand_right_keypoints_3d { get; set; }
    }
}
