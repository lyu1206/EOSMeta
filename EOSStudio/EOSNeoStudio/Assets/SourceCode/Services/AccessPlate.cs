using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Eos.Service.AI
{
    using Objects;
    [NoCreated]
    public class AccessPlate : EosObjectBase , ITransform
    {
        private EosHumanoid _humanoid;
        private EosTransform _transform;
        private List<AIEngageObject> _requestobject = new List<AIEngageObject>();
        public EosTransform Transform => _transform;

        public override void OnAncestryChanged()
        {
            if (!(_parent is EosHumanoid humanoid))
                return;
            _humanoid = humanoid;
            _transform = ObjectFactory.CreateInstance<EosTransform>();
            _transform.Create("AccesPlate");
        }
        protected override void OnStartPlay()
        {
            _transform.SetParent(_humanoid.Humanoidroot.Transform);
            _humanoid.OnMoveStateChanged += OwnerMoveStateChaged;
        }
        private void OwnerMoveStateChaged(object sender,bool state)
        {
            _platelist.Clear();
            _requestobject.ForEach(it => it.ResetTrace());
            _requestobject.Clear();
        }
        /*
        public Collider GetExpectAccessPoint(AIEngageObject reqobj)
        {

        }
        */
        public bool GetAccesPoint(AIEngageObject reqhumanoid,out Vector3 accesspoint)
        {
            var result = GetAccesPoint(reqhumanoid.Owner.Humanoidroot.LocalPosition, reqhumanoid.Owner.Radius, out accesspoint);
            if (result)
                _requestobject.Add(reqhumanoid);
            //if (result)
            //    reqhumanoid.Transform.localPosition = accesspoint;
            return result;
            /*
            var aiservice = Ref.Solution.AIService;
            var direction = reqhumanoid.Owner.Humanoidroot.LocalPosition - _humanoid.Humanoidroot.LocalPosition;

            var distance = direction.magnitude;

            direction.Normalize();
            accesspoint = _humanoid.Humanoidroot.LocalPosition + direction * (_humanoid.Radius + reqhumanoid.Owner.Radius* 0.9f);
            var result = Physics.OverlapSphere(accesspoint, reqhumanoid.Owner.Radius, 1 << LayerMask.NameToLayer("AI"));
            var find = false;
            foreach(var it in result)
            {
                if (reqhumanoid.Transform.GetInstanceID() == it.transform.GetInstanceID())
                    continue;

                Debug.DrawRay(_humanoid.Humanoidroot.LocalPosition, direction * distance, Color.yellow);
                var occupiedobject = aiservice.GetAIEngageObjectByInstanceID(it.transform.GetInstanceID());
                var testvec = (occupiedobject.Owner.Humanoidroot.LocalPosition - _humanoid.Humanoidroot.LocalPosition);
                Debug.DrawRay(_humanoid.Humanoidroot.LocalPosition, testvec, Color.white);

                var resultvec = EosUtil.Math.GetSomething(_humanoid.Humanoidroot.LocalPosition, direction * (_humanoid.Radius + reqhumanoid.Owner.Radius), testvec.normalized * (_humanoid.Radius + reqhumanoid.Owner.Radius), 5 + reqhumanoid.Owner.Radius);
                resultvec.Normalize();
                Debug.DrawRay(_humanoid.Humanoidroot.LocalPosition, resultvec * (_humanoid.Radius + reqhumanoid.Owner.Radius), Color.red);
                accesspoint = _humanoid.Humanoidroot.LocalPosition + resultvec * (_humanoid.Radius + reqhumanoid.Owner.Radius);
                find = true;
                break;
            }

            reqhumanoid.Transform.localPosition = accesspoint;
            return true;
             */
        }
        public class AccesPlate
        {
            private Vector3 center;
            private float _angle;
            public float Angle => _angle;
            public Vector3 Position;
            public AccesPlate Left;
            public AccesPlate Right;
            public float Distance;
            public struct PlateRay
            {
                public PlateRay(Vector3 origin_, Vector3 direction_)
                {
                    origin = origin_;
                    direction = direction_;
                }
                public Vector3 endposition => origin + direction;
                public Vector3 origin;
                public Vector3 direction;
            }
            public Vector3 AccessPoint => Position + center * Distance;
            public AccesPlate(Vector3 position , float ownerradius, float reqradius)
            {
                Position = position;
                Distance = reqradius + ownerradius;
                var direction = center = Vector3.forward;
                var cr = Vector3.Cross(direction, Vector3.up);
                var crosspoint = position + direction * (ownerradius + reqradius);
                var p1 = crosspoint + cr * reqradius;
                var p2 = crosspoint - cr * reqradius;
                left = new PlateRay(position, p1 - position);
                right = new PlateRay(position, p2 - position);
                cross = new PlateRay(p1, p2 - p1);
                _angle = Vector3.Angle(left.direction, right.direction);
            }
            public void SetAngle(Vector3 direction)
            {
                var angle = Vector3.Angle(center, direction.normalized);
                var rotate = Quaternion.Euler(0, angle, 0);
                left.direction = rotate * left.direction;
                right.direction = rotate * right.direction;
                cross = new PlateRay(left.endposition, right.endposition - left.endposition);
                center = rotate * center;
            }
            public void AddAngle(AccesPlate plate)
            {
                //SetAngle(plate.center);
                SetAngle(plate.right.direction);
            }
            public void AddAngle(PlateRay axis,PlateRay target)
            {
                var angle = Vector3.Angle(target.direction, axis.direction);
                //SetAngle(plate.center);
                var rotate = Quaternion.Euler(0, angle, 0);
                left.direction = rotate * left.direction;
                right.direction = rotate * right.direction;
                cross = new PlateRay(left.endposition, right.endposition - left.endposition);
                center = rotate * center;
            }
            public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
            {

                Vector3 lineVec3 = linePoint2 - linePoint1;
                Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
                Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

                float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

                //is coplanar, and not parrallel
                if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
                {
                    float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                    intersection = linePoint1 + (lineVec1 * s);
                    return true;
                }
                else
                {
                    intersection = Vector3.zero;
                    return false;
                }
            }
            public bool GetIntersection(AccesPlate plate,out int direction)
            {
                Vector3 intersection;
                var intersect = LineLineIntersection(out intersection, cross.origin, cross.direction, plate.left.origin, plate.center);
                var disfromleft = (left.endposition - intersection).magnitude;
                var disfromright = (right.endposition - intersection).magnitude;
                if (disfromleft > disfromright)
                    direction = 0;// right close
                else
                    direction = 1;//left close
                return intersect;
            }
            //public AccesPlate(Vector3 center, float ownerradius, Vector3 reqPosition, float reqradius)
            //{
            //    var direction = reqPosition - center;
            //    var cr = Vector3.Cross(direction.normalized, Vector3.up);
            //    var crosspoint = center + direction.normalized * (ownerradius + reqradius);
            //    var p1 = crosspoint + cr * reqradius;
            //    var p2 = crosspoint - cr * reqradius;
            //    left = new PlateRay(center, p1 - center);
            //    right = new PlateRay(center, p2 - center);
            //    cross = new PlateRay(p1, p2 - p1);
            //}
#if UNITY_EDITOR
            public void DrawRay()
            {
                Debug.DrawRay(left.origin, left.direction, Color.black);
                Debug.DrawRay(right.origin, right.direction, Color.red);
                Debug.DrawRay(cross.origin, cross.direction, Color.yellow);
                Debug.DrawRay(left.origin, center, Color.green);
                Debug.Break();
            }
#endif
            public PlateRay left;
            public PlateRay right;
            public PlateRay cross;
        }
        List<AccesPlate> _platelist = new List<AccesPlate>();

        public bool GetAccesPoint(Vector3 reqPosition, float reqradius, out Vector3 accesspoint)
        {
            var aiservice = Ref.Solution.AIService;

            var direction = reqPosition - _humanoid.Humanoidroot.LocalPosition;
            //var plate = new AccesPlate(_humanoid.Humanoidroot.LocalPosition, _humanoid.Radius, reqPosition, reqradius);
            var plate = new AccesPlate(_humanoid.Humanoidroot.LocalPosition, _humanoid.Radius, reqradius);
            plate.SetAngle(direction);
            accesspoint = plate.AccessPoint;

            accesspoint = _humanoid.Humanoidroot.LocalPosition;

            if (_platelist.Count == 0)
            {
                _platelist.Add(plate);
            }
            else
            {
                foreach(var it in _platelist)
                {
                    var leftright = 0;
                    if (it.GetIntersection(plate,out leftright))
                    {
                        plate = new AccesPlate(_humanoid.Humanoidroot.LocalPosition, _humanoid.Radius, reqradius);
                        leftright = 1;
                        if (leftright==0)
                            plate.AddAngle(it.right,plate.left);
                        else
                            plate.AddAngle(it.left, plate.right);
                        //it.DrawRay();
                        //plate.DrawRay();
                    }
                }
            }
            return true;
        }

    }
}