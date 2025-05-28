extern alias GameScripts;

using GameScripts.Assets.Source.CarModel;
using GameScripts.Assets.Source.Messaging;
using GameScripts.Assets.Source.Messaging.Messages;
using InertialOuija.Ghosts;

namespace InertialOuija.Components;

internal class PersonalBestHud : RivalTimeHud, IReceiveMessages<LapTimeMessage>
{
	private GhostTime? _personalBest;

	protected override void Awake()
	{
		base.Awake();
		MessagingCenter.RegisterReceiver(this);
		UpdatePersonalBest();
	}

	void OnDestroy()
	{
		MessagingCenter.DeregisterReceiver(this);
	}

	private void UpdatePersonalBest(GhostTime? personalBestTime)
	{
		_personalBest = personalBestTime;
		if (personalBestTime != null)
		{
			SetText("personal best", GameData.SteamUser.Name, personalBestTime.Value);
			SetActive(true);
		}
		else
		{
			SetActive(false);
		}
	}

	private void UpdatePersonalBest() =>
		UpdatePersonalBest(ExternalGhostManager.Ghosts.GetPersonalBestTime());

	void IReceiveMessages<LapTimeMessage>.HandleMessage(LapTimeMessage message)
	{
		var props = message.Racer.GetComponentInChildren<CarVisualProperties>();
		if (props.CarId == 0 && (_personalBest is null || message.LapTime < _personalBest.Value.TimeInSeconds))
			UpdatePersonalBest(new(message.LapTime));
	}
}
