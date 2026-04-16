using UnityEngine;
using TMPro;

namespace MiniTransportTycoon
{
    public class TopBarManager : MonoBehaviour
    {
    public TextMeshProUGUI cityNameText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI dateText;

    public void UpdateTopBar(string cityName, int money, string date)
    {
        cityNameText.text = cityName;
        moneyText.text = $"${money:N0}";
        dateText.text = date;
    }
    }
}