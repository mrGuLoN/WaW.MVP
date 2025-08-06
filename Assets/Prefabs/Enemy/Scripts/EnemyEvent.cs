using UnityEngine;

public class EnemyEvent : MonoBehaviour
{
	[SerializeField] private ParticleSystem _lHit, _rHit;
	public void LeftHit()
	{
		_lHit.Play();
	}
	public void RightHit()
	{
		_rHit.Play();
	}
}
