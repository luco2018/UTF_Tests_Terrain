using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour
{
	[System.Serializable]
	public class TrampleProperties
	{
		public enum Interaction { GoThrough, Fall, Evade }
		public enum EffectType { None, Normal, Bare, Pine, OnlyContact }
		public enum EffectSize { Small, Normal }

		public Interaction interactionWithLightVehicle = Interaction.Fall;
		public Interaction interactionWithHeavyVehicle = Interaction.Fall;
		public EffectType effectType = EffectType.Normal;
		public EffectSize effectSize = EffectSize.Normal;
	}

	public TrampleProperties trampleProperties;
}
