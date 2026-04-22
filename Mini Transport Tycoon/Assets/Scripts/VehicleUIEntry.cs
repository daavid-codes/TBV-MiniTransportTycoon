using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTransportTycoon
{
    public class VehicleUIEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private Button sellButton;
        [SerializeField] private TextMeshProUGUI  maintenanceButtonText;
        [SerializeField] private TextMeshProUGUI sellButtonText;

        [SerializeField] private Button maintenanceButton;
        private Vehicle vehicle;

        public void Setup(Vehicle vehicle)
        {
            this.vehicle = vehicle;
            infoText.text = "ID: " + vehicle.Id.ToString() + "\n" + "Type: " + vehicle.Type.ToString() + "\n" + "100/" + vehicle.GetDurability().ToString() + "\n" + "Speed: " + vehicle.Speed.ToString("F1");
            maintenanceButtonText.text = vehicle.MaintenanceCost.ToString() + "$";
            sellButtonText.text = ((int)(vehicle.Cost * 0.01f * vehicle.GetDurability())).ToString() + "$";

        }

        public void OnSellButtonClicked()
        {
        GameData gameData = GameData.Instance;
        if (gameData != null)
        {
            gameData.Money += (int)(vehicle.Cost * 0.01f * vehicle.GetDurability());
            vehicle.DestroyVehicle();
            VehicleManager.Instance.UnregisterVehicle(vehicle);
            
        }
        }

        public void OnMaintenanceButtonClicked()
        {
            GameData gameData = GameData.Instance;
            if (gameData != null)
            {
                gameData.Money -= (100 - vehicle.GetDurability()) * 10;
                VehicleManager.Instance.RepairVehicle(vehicle);
                
            }
        }


    }
}