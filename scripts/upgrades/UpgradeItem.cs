// UpgradeItem.cs - Individual shop item display
using Godot;
using System;

public partial class UpgradeItem : Control
{
	[Export] public Label NameLabel { get; set; }
	[Export] public Label DescriptionLabel { get; set; }
	[Export] public Label CostLabel { get; set; }
	[Export] public Label LevelLabel { get; set; }
	[Export] public Button BuyButton { get; set; }
	
	public string UpgradeId { get; private set; }
	private PlayerUpgrade _upgradeInfo;
	private ShopManager _shopManager;
	
	public override void _Ready()
	{
		BuyButton.Pressed += OnBuyButtonPressed;
	}
	
	public void SetUpgradeInfo(PlayerUpgrade upgrade)
	{
		_upgradeInfo = upgrade;
		UpgradeId = upgrade.Id;
		
		NameLabel.Text = upgrade.Name;
		DescriptionLabel.Text = upgrade.Description;
		
		UpdateLevel(upgrade.CurrentLevel);
	}
	
	public void SetShopManager(ShopManager shopManager)
	{
		_shopManager = shopManager;
	}
	
	public void UpdateLevel(int level)
	{
		int nextLevel = level + 1;
		LevelLabel.Text = $"Level: {level}/{_upgradeInfo.MaxLevel}";
		
		// Update cost and button state
		if (level >= _upgradeInfo.MaxLevel)
		{
			CostLabel.Text = "MAX LEVEL";
			BuyButton.Disabled = true;
		}
		else
		{
			int cost = _upgradeInfo.BaseCost * nextLevel;
			CostLabel.Text = $"Cost: {cost}";
			BuyButton.Disabled = false;
		}
	}
	
	private void OnBuyButtonPressed()
	{
		if (_shopManager != null)
		{
			_shopManager.PurchaseUpgrade(UpgradeId);
		}
	}
}
