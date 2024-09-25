using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Cam.AnimationFunctions
{
    internal class AnimationFunction
    {
        public static AnimationCurve CreateEaseCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 0),        // Start at (0, 0) with no tangent
                new Keyframe(0.2f, 1, 5, 0),    // Quick rise to (0.2, 1)
                new Keyframe(0.8f, 0, -5, 0),    // Sharp drop back to (0.8, 0)
                new Keyframe(1, 1, 0, 0)         // Finish at (1, 1) with no tangent
            );
        }
        public static AnimationCurve CreateEaseInCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 2),      // Start at (0, 0) with upward slope
                new Keyframe(1, 1, 1, 0)       // End at (1, 1) with a downward slope
            );
        }

        public static AnimationCurve CreateEaseOutCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 2, 0),      // Start at (0, 0) with an upward slope
                new Keyframe(1, 1, 0, 0)       // End at (1, 1) with a flat slope
            );
        }

        public static AnimationCurve CreateBounceInCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 1),      // Start at (0, 0)
                new Keyframe(0.5f, 1, 3, -2),  // Bounce peak
                new Keyframe(1, 0)               // End at (1, 0)
            );
        }

        public static AnimationCurve CreateBounceOutCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),             // Start at (0, 0)
                new Keyframe(0.5f, 0.5f, 0, -3),// First bounce
                new Keyframe(1, 1)               // End at (1, 1)
            );
        }

        public static AnimationCurve CreateBounceInOutCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),             // Start at (0, 0)
                new Keyframe(0.5f, 1, 0, -1),   // Peak bounce
                new Keyframe(1, 0)               // End at (1, 0)
            );
        }

        public static AnimationCurve CreateDecelerateCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),              // Start at (0, 0)
                new Keyframe(1, 1, 0, 0)         // End at (1, 1) with a flat slope
            );
        }

        public static AnimationCurve CreateElasticInCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),
                new Keyframe(0.5f, 1, 0, -2),    // Elastic peak
                new Keyframe(1, 0)
            );
        }

        public static AnimationCurve CreateElasticOutCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),
                new Keyframe(0.5f, 0.5f, 1, -1), // Elastic bounce
                new Keyframe(1, 1)
            );
        }

        public static AnimationCurve CreateEaseInBackCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 2),        // Start at (0, 0) with upward slope
                new Keyframe(1, 1, -1, 0)        // Overshoot at end
            );
        }

        public static AnimationCurve CreateEaseInCircCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),               // Start at (0, 0)
                new Keyframe(1, 1)                // End at (1, 1)
            );
        }

        public static AnimationCurve CreateEaseInCubicCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 3),        // Start at (0, 0) with a cubic slope
                new Keyframe(1, 1)                // End at (1, 1)
            );
        }

        public static AnimationCurve CreateEaseInExpoCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),
                new Keyframe(1, 1)
            );
        }

        public static AnimationCurve CreateEaseInOutBackCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 2),        // Start at (0, 0) with upward slope
                new Keyframe(0.5f, 1, -1, 0),    // Peak with overshoot
                new Keyframe(1, 0)                // End at (1, 0)
            );
        }

        public static AnimationCurve CreateEaseInOutCircCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),               // Start at (0, 0)
                new Keyframe(1, 1)                // End at (1, 1)
            );
        }

        public static AnimationCurve CreateEaseInOutCubicCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),               // Start at (0, 0)
                new Keyframe(1, 1)                // End at (1, 1)
            );
        }

        public static AnimationCurve CreateEaseInOutQuadCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 2), // Ease In
                new Keyframe(0.5f, 1, 2, 0), // Peak
                new Keyframe(1, 1, 0, 0) // Ease Out
            );
        }

        public static AnimationCurve CreateEaseInOutQuartCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 4), // Ease In
                new Keyframe(0.5f, 1, 4, 0), // Peak
                new Keyframe(1, 1, 0, 0) // Ease Out
            );
        }

        public static AnimationCurve CreateEaseInOutQuintCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 5), // Ease In
                new Keyframe(0.5f, 1, 5, 0), // Peak
                new Keyframe(1, 1, 0, 0) // Ease Out
            );
        }

        public static AnimationCurve CreateEaseInOutSineCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 1), // Ease In
                new Keyframe(0.5f, 1, 1, 0), // Peak
                new Keyframe(1, 1, 0, 0) // Ease Out
            );
        }

        public static AnimationCurve CreateEaseInQuadCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 2), // Ease In
                new Keyframe(1, 1) // End
            );
        }

        public static AnimationCurve CreateEaseInQuartCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 4), // Ease In
                new Keyframe(1, 1) // End
            );
        }

        public static AnimationCurve CreateEaseInQuintCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 5), // Ease In
                new Keyframe(1, 1) // End
            );
        }

        public static AnimationCurve CreateEaseInSineCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 1), // Ease In
                new Keyframe(1, 1) // End
            );
        }

        public static AnimationCurve CreateEaseOutBackCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Start
                new Keyframe(0.3f, 1.5f, 0, -1.5f), // Overshoot
                new Keyframe(1, 1) // End
            );
        }

        public static AnimationCurve CreateEaseOutCircCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Start
                new Keyframe(1, 1, 0, 0) // End
            );
        }

        public static AnimationCurve CreateEaseOutCubicCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Start
                new Keyframe(1, 1, 0, 0) // End
            );
        }

        public static AnimationCurve CreateEaseOutExpoCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Start
                new Keyframe(1, 1, 0, 0) // End
            );
        }

        public static AnimationCurve CreateEaseOutQuadCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Start
                new Keyframe(1, 1, 0, 0) // End
            );
        }

        public static AnimationCurve CreateEaseOutQuartCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Start
                new Keyframe(1, 1, 0, 0) // End
            );
        }

        public static AnimationCurve CreateEaseOutQuintCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Start
                new Keyframe(1, 1, 0, 0) // End
            );
        }

        public static AnimationCurve CreateEaseOutSineCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Start
                new Keyframe(1, 1, 0, 0) // End
            );
        }

        public static AnimationCurve CreateFastEaseInToSlowEaseOut()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 2), // Fast Ease In
                new Keyframe(1, 1, 0, 0) // Slow Ease Out
            );
        }

        public static AnimationCurve CreateFastLinearToSlowEaseIn()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Fast Linear
                new Keyframe(1, 1, 0, 1) // Slow Ease In
            );
        }

        public static AnimationCurve CreateFastOutSlowIn()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 2), // Fast Out
                new Keyframe(1, 1, 0, 1) // Slow In
            );
        }

        public static AnimationCurve CreateLinearToEaseOut()
        {
            return new AnimationCurve(
                new Keyframe(0, 0), // Linear Start
                new Keyframe(1, 1, 0, 0) // Ease Out End
            );
        }

        public static AnimationCurve CreateSlowMiddle()
        {
            return new AnimationCurve(
                new Keyframe(0, 0, 0, 0), // Start
                new Keyframe(0.5f, 0.5f, 0, 0), // Slow Middle
                new Keyframe(1, 1) // End
            );
        }
    }
}
