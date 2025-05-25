extern alias GameScripts;
using GameScripts.Assets.Source.UserManagement;
using UnityEngine;
using UnityEngine.Video;

namespace InertialOuija.Components;
internal class IntroSkip : MonoBehaviour
{
	private Animator _animator;
	private CutsceneSkip _cutsceneSkip;

	void Start()
	{
		var canvas = GameObject.Find("/Canvas");
		_animator = canvas.GetComponent<Animator>();
		_cutsceneSkip = canvas.GetComponent<CutsceneSkip>();

		if (!_animator || !_cutsceneSkip)
		{
			Destroy(gameObject);
			return;
		}

		var videoPlayer = canvas.transform.Find("Video Player")?.GetComponent<VideoPlayer>();
		if (videoPlayer)
		{
			videoPlayer.clip = null;
			videoPlayer.playOnAwake = false;
		}

		// Changing timeScale to also speed up the various delayed actions and not just the animation
		Time.timeScale = 3;
	}

	void Update()
	{
		// Wwise stuff is enabled on a delay and won't be returned by GameObject.Find until then
		// It should already be enabled before this point, just making sure
		if (_cutsceneSkip.enabled && GameObject.Find("/WwiseInit") && GameObject.Find("/WwiseInit/Bank"))
		{
			Log.Debug("Skipping now");
			Time.timeScale = 1;
			_animator.Play(0, 0, _cutsceneSkip.SkipTime / _animator.GetCurrentAnimatorStateInfo(0).length);
			Destroy(gameObject);
		}
	}

	void OnDestroy()
	{
		// Just in case
		Time.timeScale = 1;
	}
}
