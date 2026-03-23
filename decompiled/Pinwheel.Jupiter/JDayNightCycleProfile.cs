using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Pinwheel.Jupiter;

[CreateAssetMenu(menuName = "Jupiter/Day Night Cycle Profile")]
public class JDayNightCycleProfile : ScriptableObject
{
	private static Dictionary<string, int> propertyRemap;

	private static Dictionary<string, PropertyInfo> scriptPropertyRemap;

	[SerializeField]
	private List<JAnimatedProperty> animatedProperties;

	private static Dictionary<string, int> PropertyRemap
	{
		get
		{
			if (propertyRemap == null)
			{
				propertyRemap = new Dictionary<string, int>();
			}
			return propertyRemap;
		}
		set
		{
			propertyRemap = value;
		}
	}

	private static Dictionary<string, PropertyInfo> ScriptPropertyRemap
	{
		get
		{
			if (scriptPropertyRemap == null)
			{
				scriptPropertyRemap = new Dictionary<string, PropertyInfo>();
			}
			return scriptPropertyRemap;
		}
		set
		{
			scriptPropertyRemap = value;
		}
	}

	public List<JAnimatedProperty> AnimatedProperties
	{
		get
		{
			if (animatedProperties == null)
			{
				animatedProperties = new List<JAnimatedProperty>();
			}
			return animatedProperties;
		}
		set
		{
			animatedProperties = value;
		}
	}

	static JDayNightCycleProfile()
	{
		InitPropertyRemap();
	}

	private static void InitPropertyRemap()
	{
		PropertyRemap.Clear();
		ScriptPropertyRemap.Clear();
		PropertyInfo[] properties = typeof(JSkyProfile).GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetCustomAttribute(typeof(JAnimatableAttribute), inherit: false) is JAnimatableAttribute)
			{
				string text = propertyInfo.Name;
				int value = Shader.PropertyToID("_" + text);
				PropertyRemap.Add(text, value);
				ScriptPropertyRemap.Add(text, propertyInfo);
			}
		}
	}

	public void AddProperty(JAnimatedProperty p, bool setDefaultValue = true)
	{
		if (setDefaultValue)
		{
			JDayNightCycleProfile defaultDayNightCycleProfile = JJupiterSettings.Instance.DefaultDayNightCycleProfile;
			if (defaultDayNightCycleProfile != null)
			{
				JAnimatedProperty jAnimatedProperty = defaultDayNightCycleProfile.AnimatedProperties.Find((JAnimatedProperty jAnimatedProperty2) => jAnimatedProperty2.Name != null && jAnimatedProperty2.Name.Equals(p.Name));
				if (jAnimatedProperty != null)
				{
					p.Curve = jAnimatedProperty.Curve;
					p.Gradient = jAnimatedProperty.Gradient;
				}
			}
		}
		AnimatedProperties.Add(p);
	}

	public void Animate(JSky sky, float t)
	{
		CheckDefaultProfileAndThrow(sky.Profile);
		for (int i = 0; i < AnimatedProperties.Count; i++)
		{
			JAnimatedProperty jAnimatedProperty = AnimatedProperties[i];
			if (jAnimatedProperty.Target == JAnimateTarget.Material)
			{
				int value = 0;
				if (PropertyRemap.TryGetValue(jAnimatedProperty.Name, out value))
				{
					if (jAnimatedProperty.CurveOrGradient == JCurveOrGradient.Curve)
					{
						sky.Profile.Material.SetFloat(value, jAnimatedProperty.EvaluateFloat(t));
					}
					else
					{
						sky.Profile.Material.SetColor(value, jAnimatedProperty.EvaluateColor(t));
					}
				}
			}
			else
			{
				if (jAnimatedProperty.Target != JAnimateTarget.SkyProfile)
				{
					continue;
				}
				PropertyInfo value2 = null;
				if (ScriptPropertyRemap.TryGetValue(jAnimatedProperty.Name, out value2))
				{
					if (jAnimatedProperty.CurveOrGradient == JCurveOrGradient.Curve)
					{
						value2.SetValue(sky.Profile, jAnimatedProperty.EvaluateFloat(t));
					}
					else
					{
						value2.SetValue(sky.Profile, jAnimatedProperty.EvaluateColor(t));
					}
				}
			}
		}
	}

	private void CheckDefaultProfileAndThrow(JSkyProfile p)
	{
		if (p == null || (!(p == JJupiterSettings.Instance.DefaultProfileSunnyDay) && !(p == JJupiterSettings.Instance.DefaultProfileStarryNight)))
		{
			return;
		}
		throw new ArgumentException("Animating default sky profile is prohibited. You must create a new profile for your sky to animate it.");
	}

	public bool ContainProperty(string propertyName)
	{
		return AnimatedProperties.Exists((JAnimatedProperty p) => p.Name.Equals(propertyName));
	}
}
