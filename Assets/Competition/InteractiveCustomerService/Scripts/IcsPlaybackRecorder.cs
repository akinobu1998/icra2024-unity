using SIGVerse.Common;
using SIGVerse.FCSC.Common;
using SIGVerse.ToyotaHSR;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	[RequireComponent(typeof (IcsPlaybackCommon))]
	public class IcsPlaybackRecorder : CompetitionPlaybackRecorder
	{
		//private string environmentName;

		protected override void Awake()
		{
			if(IcsConfig.Instance.info.playbackType != IcsPlaybackCommon.PlaybackTypeRecord)
			{
				DisableRecorder();
			}
			
			base.Awake();

			if (this.willRecord)
			{
				// Robot
				Transform robot = GameObject.FindGameObjectWithTag("Robot").transform;

				this.targetTransforms.Add(robot);

				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.base_footprint       .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.arm_lift_link        .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.arm_flex_link        .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.arm_roll_link        .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.wrist_flex_link      .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.wrist_roll_link      .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.head_pan_link        .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.head_tilt_link       .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.torso_lift_link      .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.hand_motor_dummy_link.ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.hand_l_proximal_link .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.hand_r_proximal_link .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.hand_l_distal_link   .ToString()));
				this.targetTransforms.Add(SIGVerseUtils.FindTransformFromChild(robot, HSRCommon.Link.hand_r_distal_link   .ToString()));
			}
		}

		//protected override List<string> GetDefinitionLines()
		//{
		//	List<string> definitionLines = base.GetDefinitionLines();

		//	// Environment
		//	definitionLines.Add(HandymanPlaybackEnvironmentEventController.GetDefinitionLine(this.environmentName));
			
		//	return definitionLines;
		//}


		public bool Initialize(int numberOfTrials)
		{
			string filePath = string.Format(Application.dataPath + IcsPlaybackCommon.FilePathFormat, numberOfTrials);

			return this.Initialize(filePath);
		}

		//public void SetEnvironmentName(string environmentName)
		//{
		//	this.environmentName = environmentName;
		//}
	}
}
