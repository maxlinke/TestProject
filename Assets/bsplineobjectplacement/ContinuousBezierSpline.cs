using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineTools {

    public class ContinuousBezierSpline : BezierSpline {

        public override int DEFAULT_LENGTH_CALC_ITERATIONS => throw new System.NotImplementedException();
        public override int DEFAULT_NEXT_T_ITERATIONS => throw new System.NotImplementedException();
        public override int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION => throw new System.NotImplementedException();

        protected override bool PointsChangedSinceLastRecalculation()
        {
            throw new System.NotImplementedException();
        }

        public override void ApplyScale () {
            throw new System.NotImplementedException();
        }

        public override Vector3 BezierDerivative (float t) {
            throw new System.NotImplementedException();
        }

        public override Vector3 BezierPoint (float t) {
            throw new System.NotImplementedException();
        }

        public override void ReverseDirection () {
            throw new System.NotImplementedException();
        }

        public override Vector3 SecondDerivative (float t) {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<Vector3> GetWorldSpaceControlPoints () {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<Vector3> GetWorldSpaceEndPoints () {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<(Vector3, Vector3)> GetWorldSpaceHandleLines () {
            throw new System.NotImplementedException();
        }
    }

}