using SIGVerse.Common;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.ToyotaHSR;
using Joint = SIGVerse.ToyotaHSR.HSRCommon.Joint;
using Link  = SIGVerse.ToyotaHSR.HSRCommon.Link;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public class IcsRobotArmController : MonoBehaviour
	{
		public float jointVel = 0.1f;
		public float jointAngVel = 60f;

		// ----------------------------------------
		private float armLiftLinkIniPosZ;

		private Dictionary<Joint, Transform> linkMap = new Dictionary<Joint, Transform>();
		private Dictionary<Joint, float> destinationMap = new Dictionary<Joint, float>();

		void Awake()
		{
			this.linkMap[Joint.arm_lift_joint]   = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_lift_link  .ToString());
			this.linkMap[Joint.torso_lift_joint] = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.torso_lift_link.ToString());
			this.linkMap[Joint.arm_flex_joint]   = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.arm_flex_link  .ToString());
			this.linkMap[Joint.wrist_flex_joint] = SIGVerseUtils.FindTransformFromChild(this.transform.root, Link.wrist_flex_link.ToString());
		}

		void Start()
		{
			this.armLiftLinkIniPosZ   = this.linkMap[Joint.arm_lift_joint].localPosition.z;

			this.destinationMap[Joint.arm_lift_joint]   = this.linkMap[Joint.arm_lift_joint]  .localPosition.z - this.armLiftLinkIniPosZ;
			this.destinationMap[Joint.arm_flex_joint]   = HSRCommon.GetNormalizedJointEulerAngle(this.linkMap[Joint.arm_flex_joint]  .localEulerAngles.y, Joint.arm_flex_joint);
			this.destinationMap[Joint.wrist_flex_joint] = HSRCommon.GetNormalizedJointEulerAngle(this.linkMap[Joint.wrist_flex_joint].localEulerAngles.y, Joint.wrist_flex_joint);
		}

		void Update()
		{
			if(!IsReachedDestination())
			{
				MovePosZ();
				MoveRotY(Joint.arm_flex_joint);
				MoveRotY(Joint.wrist_flex_joint);
			}
		}

		private void MovePosZ()
		{
			// Move arm_lift_joint
			Joint joint = Joint.arm_lift_joint;

			float delta = Mathf.Clamp(GetDiffArmLink(), -this.jointVel*Time.deltaTime, +this.jointVel*Time.deltaTime);

			float previousPos = this.linkMap[joint].localPosition.z;

			this.linkMap[joint].localPosition = new Vector3(0, 0, HSRCommon.GetClampedPosition(this.linkMap[joint].localPosition.z-this.armLiftLinkIniPosZ+delta, joint)+this.armLiftLinkIniPosZ);

			float armDiff = this.linkMap[joint].localPosition.z - previousPos;

			// Move torso_lift_joint
			this.linkMap[Joint.torso_lift_joint].localPosition = new Vector3(0, 0, this.linkMap[Joint.torso_lift_joint].localPosition.z+armDiff/2.0f);
		}

		private void MoveRotY(Joint joint)
		{
			float delta = Mathf.Clamp(GetDiffRotY(joint), -this.jointAngVel*Time.deltaTime, +this.jointAngVel*Time.deltaTime);
			this.linkMap[joint].localRotation = Quaternion.Euler(0, HSRCommon.GetNormalizedJointEulerAngle(this.linkMap[joint].localEulerAngles.y+delta, joint), 0);
		}

		public bool IsReachedDestination()
		{
			if(Mathf.Abs(GetDiffArmLink())    > 0.01f){ return false; }
			if(Mathf.Abs(GetDiffRotY(Joint.arm_flex_joint)) > 0.01f){ return false; }
			if(Mathf.Abs(GetDiffRotY(Joint.wrist_flex_joint)) > 0.01f){ return false; }

			return true;
		}

		private float GetDiffArmLink()
		{
			return this.destinationMap[Joint.arm_lift_joint] - (this.linkMap[Joint.arm_lift_joint].localPosition.z - this.armLiftLinkIniPosZ);
		}

		private float GetDiffRotY(Joint joint)
		{
			return this.destinationMap[joint] - HSRCommon.GetNormalizedJointEulerAngle(this.linkMap[joint].localEulerAngles.y, joint);
		}

		public void UpArmHigh()
		{
			SetDestination(0.69f, -60f, -30f);
		}

		public void UpArm()
		{
			SetDestination(0.2f, -60f, -30f);
		}

		public void DownArmLittle()
		{
			SetDestination(0.0f, -60f, -30f);
		}

		public void DownArm()
		{
			SetDestination(0.0f, -120f, +30f);
		}

		public void ResetArm()
		{
			SetDestination(0.0f, 0.0f, -90.0f);
		}

		private void SetDestination(float armLiftVal, float armFlexVal, float wristFlexVal)
		{
			this.destinationMap[Joint.arm_lift_joint]   = armLiftVal;
			this.destinationMap[Joint.arm_flex_joint]   = armFlexVal;
			this.destinationMap[Joint.wrist_flex_joint] = wristFlexVal;
		}
	}
}

