using InertialOuija.Ghosts;
using TMPro;
using UnityEngine;

namespace InertialOuija.Components;

internal class RivalTimeHud : MonoBehaviour
{
	public static RivalTimeHud FastestGhostHud { get; set; }

	private TMP_Text _header;
	private TMP_Text _name;
	private TMP_Text _value;

	public string Header { get; private set; }
	public string Name { get; private set; }
	public string Value { get; private set; }

	protected virtual void Awake()
	{
		_header = transform.Find("HUDTitle Rival Time/Title").GetComponent<TMP_Text>();
		_name = transform.Find("Rival Name/Rival Name").GetComponent<TMP_Text>();
		_value = transform.Find("GoldTarget/Target Text").GetComponent<TMP_Text>();
		UpdateText();
	}

	void OnDestroy()
	{
		if (FastestGhostHud == this)
			FastestGhostHud = null;
	}

	public void SetActive(bool value) => gameObject.SetActive(value);

	public void SetText(string header, string name, string value)
	{
		Header = header;
		Name = name;
		Value = value;
		UpdateText();
	}

	public void SetText(string header, string name, GhostTime value)
	{
		var valueStr = "<mspace=0.7em>" + value.ToString(false, ":");
		SetText(header, name, valueStr);
	}

	private void UpdateText()
	{
		if (_header)
			_header.text = Header;
		if (_name)
			_name.text = Name;
		if (_value)
			_value.text = Value;
	}
}
