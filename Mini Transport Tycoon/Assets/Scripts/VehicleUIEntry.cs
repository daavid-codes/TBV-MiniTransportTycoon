using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTransportTycoon
{
    public class VehicleUIEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private Button sellButton;
        private Vehicle vehicle;

        public void Setup(Vehicle vehicle)
        {
            this.vehicle = vehicle;
            infoText.text = "ID: " + vehicle.Id.ToString() + "\n" + "Type: " + vehicle.Type.ToString() + "\n" + "100/" + vehicle.GetDurability().ToString() + "\n" + "Speed: " + vehicle.Speed.ToString("F1");
        }

        public void OnSellButtonClicked()
        {
            VehicleManager.Instance.UnregisterVehicle(vehicle);
            vehicle.DestroyVehicle();
        }


    }
}