using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public GameObject weatherPanel;  // 날씨 UI Panel

    // 버튼 클릭 시 호출될 함수
    public void OnHokkaidoClicked()
    {
        Debug.Log("北海道 버튼 클릭됨!");

        OnPrefectureClicked();
    }
    public void OnPrefectureClicked()
    {
        Debug.Log("버튼 클릭됨!"); // 콘솔 먼저 확인
        if (weatherPanel)
        {
            weatherPanel.SetActive(true);
        }
    }
    public void OnAomoriClicked() { Debug.Log("青森 버튼 클릭됨!"); }
    public void OnIwateClicked() { Debug.Log("岩手 버튼 클릭됨!"); }
    public void OnMiyagiClicked() { Debug.Log("宮城 버튼 클릭됨!"); }
    public void OnAkitaClicked() { Debug.Log("秋田 버튼 클릭됨!"); }
    public void OnYamagataClicked() { Debug.Log("山形 버튼 클릭됨!"); }
    public void OnFukushimaClicked() { Debug.Log("福島 버튼 클릭됨!"); }

    public void OnIbarakiClicked() { Debug.Log("茨城 버튼 클릭됨!"); }
    public void OnTochigiClicked() { Debug.Log("栃木 버튼 클릭됨!"); }
    public void OnGunmaClicked() { Debug.Log("群馬 버튼 클릭됨!"); }
    public void OnSaitamaClicked() { Debug.Log("埼玉 버튼 클릭됨!"); }
    public void OnChibaClicked() { Debug.Log("千葉 버튼 클릭됨!"); }
    public void OnTokyoClicked() { Debug.Log("東京 버튼 클릭됨!"); }
    public void OnKanagawaClicked() { Debug.Log("神奈川 버튼 클릭됨!"); }

    public void OnNiigataClicked() { Debug.Log("新潟 버튼 클릭됨!"); }
    public void OnToyamaClicked() { Debug.Log("富山 버튼 클릭됨!"); }
    public void OnIshikawaClicked() { Debug.Log("石川 버튼 클릭됨!"); }
    public void OnFukuiClicked() { Debug.Log("福井 버튼 클릭됨!"); }
    public void OnYamanashiClicked() { Debug.Log("山梨 버튼 클릭됨!"); }
    public void OnNaganoClicked() { Debug.Log("長野 버튼 클릭됨!"); }
    public void OnGifuClicked() { Debug.Log("岐阜 버튼 클릭됨!"); }
    public void OnShizuokaClicked() { Debug.Log("静岡 버튼 클릭됨!"); }
    public void OnAichiClicked() { Debug.Log("愛知 버튼 클릭됨!"); }
    public void OnMieClicked() { Debug.Log("三重 버튼 클릭됨!"); }

    public void OnShigaClicked() { Debug.Log("滋賀 버튼 클릭됨!"); }
    public void OnKyotoClicked() { Debug.Log("京都 버튼 클릭됨!"); }
    public void OnOsakaClicked() { Debug.Log("大阪 버튼 클릭됨!"); }
    public void OnHyogoClicked() { Debug.Log("兵庫 버튼 클릭됨!"); }
    public void OnNaraClicked() { Debug.Log("奈良 버튼 클릭됨!"); }
    public void OnWakayamaClicked() { Debug.Log("和歌山 버튼 클릭됨!"); }
    public void OnTottoriClicked() { Debug.Log("鳥取 버튼 클릭됨!"); }
    public void OnShimaneClicked() { Debug.Log("島根 버튼 클릭됨!"); }
    public void OnOkayamaClicked() { Debug.Log("岡山 버튼 클릭됨!"); }
    public void OnHiroshimaClicked() { Debug.Log("広島 버튼 클릭됨!"); }
    public void OnYamaguchiClicked() { Debug.Log("山口 버튼 클릭됨!"); }

    public void OnTokushimaClicked() { Debug.Log("徳島 버튼 클릭됨!"); }
    public void OnKagawaClicked() { Debug.Log("香川 버튼 클릭됨!"); }
    public void OnEhimeClicked() { Debug.Log("愛媛 버튼 클릭됨!"); }
    public void OnKochiClicked() { Debug.Log("高知 버튼 클릭됨!"); }

    public void OnFukuokaClicked() { Debug.Log("福岡 버튼 클릭됨!"); }
    public void OnSagaClicked() { Debug.Log("佐賀 버튼 클릭됨!"); }
    public void OnNagasakiClicked() { Debug.Log("長崎 버튼 클릭됨!"); }
    public void OnKumamotoClicked() { Debug.Log("熊本 버튼 클릭됨!"); }
    public void OnOitaClicked() { Debug.Log("大分 버튼 클릭됨!"); }
    public void OnMiyazakiClicked() { Debug.Log("宮崎 버튼 클릭됨!"); }
    public void OnKagoshimaClicked() { Debug.Log("鹿児島 버튼 클릭됨!"); }
    public void OnOkinawaClicked() { Debug.Log("沖縄 버튼 클릭됨!"); }

}


