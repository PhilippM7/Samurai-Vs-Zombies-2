using UnityEngine;

// Compatibility stubs for the legacy particle classes that were removed in Unity 2018.3+.
// Legacy particle components cannot exist on any GameObject in Unity 2019, so
// GetComponent/GetComponentsInChildren lookups for these types always return empty
// results and the members below are never invoked at runtime.
public class ParticleEmitter : MonoBehaviour
{
	public bool emit { get; set; }

	public int particleCount
	{
		get { return 0; }
	}

	public void ClearParticles()
	{
	}
}

public class ParticleRenderer : MonoBehaviour
{
}

public class ParticleAnimator : MonoBehaviour
{
}
