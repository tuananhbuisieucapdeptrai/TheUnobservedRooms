using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class SurveyorVisualRig : MonoBehaviour
    {
        private Transform leftArm, rightArm, leftLeg, rightLeg, headLeft, headRight;
        private Vector3 leftArmBase, rightArmBase, leftLegBase, rightLegBase;
        private float seed;

        public void Configure(Transform lArm, Transform rArm, Transform lLeg, Transform rLeg, Transform lHead, Transform rHead)
        {
            leftArm=lArm; rightArm=rArm; leftLeg=lLeg; rightLeg=rLeg; headLeft=lHead; headRight=rHead;
            leftArmBase=lArm.localEulerAngles; rightArmBase=rArm.localEulerAngles; leftLegBase=lLeg.localEulerAngles; rightLegBase=rLeg.localEulerAngles;
            seed=Random.value * 10f;
        }

        private void Update()
        {
            if (leftArm == null) return;
            var stride = Mathf.Sin(Time.time * 4.1f + seed) * 22f;
            leftArm.localEulerAngles = leftArmBase + new Vector3(stride,0,Mathf.Sin(Time.time*1.7f)*8f);
            rightArm.localEulerAngles = rightArmBase + new Vector3(-stride,0,-Mathf.Sin(Time.time*1.7f)*8f);
            leftLeg.localEulerAngles = leftLegBase + new Vector3(-stride*.45f,0,0);
            rightLeg.localEulerAngles = rightLegBase + new Vector3(stride*.45f,0,0);
            var split = .11f + Mathf.Abs(Mathf.Sin(Time.time * .83f + seed)) * .12f;
            headLeft.localPosition = new Vector3(-split,1.56f,0);
            headRight.localPosition = new Vector3(split,1.56f,0);
            transform.localRotation = Quaternion.Euler(0,0,Mathf.Sin(Time.time*1.31f+seed)*2.5f);
        }
    }
}
