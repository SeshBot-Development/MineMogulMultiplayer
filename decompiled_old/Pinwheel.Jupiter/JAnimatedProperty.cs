using System;
using UnityEngine;

namespace Pinwheel.Jupiter;

[Serializable]
public class JAnimatedProperty
{
	[SerializeField]
	private string name;

	[SerializeField]
	private string displayName;

	[SerializeField]
	private JCurveOrGradient curveOrGradient;

	[SerializeField]
	private AnimationCurve curve;

	[SerializeField]
	private Gradient gradient;

	[SerializeField]
	private JAnimateTarget target;

	public string Name
	{
		get
		{
			if (name == null)
			{
				name = string.Empty;
			}
			return name;
		}
		set
		{
			name = value;
		}
	}

	public string DisplayName
	{
		get
		{
			if (displayName == null)
			{
				displayName = string.Empty;
			}
			return displayName;
		}
		set
		{
			displayName = value;
		}
	}

	public JCurveOrGradient CurveOrGradient
	{
		get
		{
			return curveOrGradient;
		}
		set
		{
			curveOrGradient = value;
		}
	}

	public AnimationCurve Curve
	{
		get
		{
			if (curve == null)
			{
				curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
			}
			return curve;
		}
		set
		{
			curve = value;
		}
	}

	public Gradient Gradient
	{
		get
		{
			if (gradient == null)
			{
				gradient = JUtilities.CreateFullWhiteGradient();
			}
			return gradient;
		}
		set
		{
			gradient = value;
		}
	}

	public JAnimateTarget Target
	{
		get
		{
			return target;
		}
		set
		{
			target = value;
		}
	}

	public float EvaluateFloat(float t)
	{
		return Curve.Evaluate(t);
	}

	public Color EvaluateColor(float t)
	{
		return Gradient.Evaluate(t);
	}

	public static JAnimatedProperty Create(string name, string displayName, JCurveOrGradient curveOrGradient, JAnimateTarget target = JAnimateTarget.Material)
	{
		return new JAnimatedProperty
		{
			name = name,
			displayName = displayName,
			curveOrGradient = curveOrGradient,
			target = target
		};
	}
}
