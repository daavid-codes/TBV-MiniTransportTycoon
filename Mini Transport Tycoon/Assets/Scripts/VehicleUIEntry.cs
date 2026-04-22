using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTransportTycoon
{
    public class VehicleUIEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI infoText;

        public void Setup(Vehicle vehicle)
        {
            
            infoText.text = "ID: " + vehicle.Id.ToString() + "\n" + "Type: " + vehicle.Type.ToString() + "\n" + "100/" + vehicle.GetDurability().ToString() + "\n" + "Speed: " + vehicle.Speed.ToString("F1");
        }
    }
}