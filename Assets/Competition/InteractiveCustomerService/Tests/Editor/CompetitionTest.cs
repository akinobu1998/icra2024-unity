using UnityEngine;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public class CompetitionTest
	{
		[Test]
		public void ScoreTest()
		{
			MethodInfo getSpeechCntDeduction = typeof(Score).GetMethod("GetSpeechCountDeduction", BindingFlags.NonPublic | BindingFlags.Static);
			
			Assert.AreEqual(+0, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)0 }));
			Assert.AreEqual(+0, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)1 }));
			Assert.AreEqual(+0, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)4 }));
			Assert.AreEqual(+0, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)5 }));
			Assert.AreEqual(-1, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)6 }));
			Assert.AreEqual(-1, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)7 }));
			Assert.AreEqual(-1, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)10 }));
			Assert.AreEqual(-1, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)20 }));
			Assert.AreEqual(-1, getSpeechCntDeduction.Invoke(null, new object[] { (ushort)100 }));
//			Assert.Throws<System.Reflection.TargetInvocationException>( () => getSpeechCntDeduction.Invoke(null, new object[] { -1 }) );

			MethodInfo getSpeechCntBonus = typeof(Score).GetMethod("GetSpeechCountBonus", BindingFlags.NonPublic | BindingFlags.Static);
			
			Assert.AreEqual(+10, getSpeechCntBonus.Invoke(null, new object[] { (ushort)0 }));
			Assert.AreEqual(+ 8, getSpeechCntBonus.Invoke(null, new object[] { (ushort)1 }));
			Assert.AreEqual(+ 6, getSpeechCntBonus.Invoke(null, new object[] { (ushort)2 }));
			Assert.AreEqual(+ 4, getSpeechCntBonus.Invoke(null, new object[] { (ushort)3 }));
			Assert.AreEqual(+ 2, getSpeechCntBonus.Invoke(null, new object[] { (ushort)4 }));
			Assert.AreEqual(+ 0, getSpeechCntBonus.Invoke(null, new object[] { (ushort)5 }));
			Assert.AreEqual(+ 0, getSpeechCntBonus.Invoke(null, new object[] { (ushort)6 }));
			Assert.AreEqual(+ 0, getSpeechCntBonus.Invoke(null, new object[] { (ushort)7 }));
			Assert.AreEqual(+ 0, getSpeechCntBonus.Invoke(null, new object[] { (ushort)10 }));
//			Assert.Throws<System.Reflection.TargetInvocationException>( () => getSpeechCntDeduction.Invoke(null, new object[] { -1 }) );
		}
	}
}
