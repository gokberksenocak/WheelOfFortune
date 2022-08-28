using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [Header("CONTENT OF SLICES")]
    [Tooltip("Zone 1 icin cark dilimlerinin icerigini buradan ayarlayabilirsiniz.")]
    [SerializeField] private Sprite[] pieceIcons = new Sprite[6]; // cark dilimlerinin sprite'lari

    [Header("BUTTONS")] //Butonlar bir liste veya dizide toplanabilirdi ama kodu yazmayan kisinin rahat anlamasi acisindan boyle kullanildi
    [SerializeField] private Button SpinButton;
    [SerializeField] private Button Res_Button;
    [SerializeField] private Button CollectButton;
    [SerializeField] private Button CollectButton2;
    [SerializeField] private Button AgainButton;
    [SerializeField] private Button ContinueButton;
    [SerializeField] private Button BackButton;
    public Button ReviveButton;

    [Header("PANELS")]
    public GameObject RestartPanel;
    [SerializeField] private GameObject RewardPanel; //kazanilan odullerin gosterilecegi panel
    [SerializeField] private GameObject QuesPan; // soru paneli, cekilmek istenildiginden emin olup olmamak icin
    [SerializeField] private GameObject Zone30Panel;

    [Header("SPRITES")]
    [SerializeField] private Image[] Icons; //loot panelinde gosterilecek olan, carktan toplanilan odullerin ikonlari
    [SerializeField] private GameObject[] LastIcons; // en sonda collect isleminden sonra gelecek paneldeki gosterilecek ikonlar
    [SerializeField] private Sprite[] rewards; //cark dilimlerinin random olarak secilecek sprite'lari
    [SerializeField] private Sprite[] SuperZoneRewards; //superzone'a ulasilirsa random olarak secilecek odul sprite'i
    [SerializeField] private Sprite bomb; 
    [SerializeField] private SpriteAtlas atlas;

    [Header("GAMEOBJECTS AND COMPONENTS")]
    [SerializeField] private GameObject[] pieces; //carkin 6 ayri dilimi
    [SerializeField] private TextMeshProUGUI text_zone; //kacinci zone'da olundugunu gosteren text
    [SerializeField] private GameObject stick; // odul secici cubuk
    [SerializeField] private Animator anim_zone; //zone textinin animatoru

    [Header("VARIABLES")]
    [SerializeField] private string[] spritenames; //sprite atlasin sprite yakalamasi icin yapildi
    [SerializeField] private int turn; //kacinci zone'da olundugunu hesaplayan deger
    private int[] numbers = new int[9]; //loot paneldeki adet hesaplamasi icin kullanildi
    private int[] count = new int[6]; //cark diliminin spritelarini random secmek icin kullanildi
    private List<int> liste = new List<int>(); //ayni zone'da cark diliminde kullanilacak spritelari sadece bir kez secmek icin kullanildi
    private int k; // loot paneldeki adet hesaplamasi icin kullanildi
    private int f; // safe zone'lar haric her zone'da bir adet bomba sprite'i secilmesi icin kullanildi
    private int i = 0; //random reward belirlemek icin kullanildi
    private int s; // superzone odul sprite'ini random secmek icin kullanildi
    private bool clicked = false; // dondurme butonuna basilip basilmadigini anlamak icin kullanildi
    private bool assign = false; // cark dilimlerine sprite atamasinin yapilip yapilmadigini anlamak icin kullanildi
    public bool finish = false; // carkin bomba dilimine gelip gelmedigini kontrol etmek icin kullanildi
    private bool allow = true; // update metodundaki superzone panel dongusunden cikilmak icin kullanildi
    private float speed; // speed ve duration carkin donme isleminin hizini ve suresini belirlemek icin kullanildi.
    private float duration;
    private RaycastHit2D _hit;

    [Header("SOUND OBJECTS")]
    [SerializeField] private AudioClip[] sounds; //ses efektleri
    [SerializeField] private AudioSource source;
   
    void Start()
    {
        SlicesContent();
        turn = 1;
        duration = Random.Range(3f, 5.3f);
        speed = Random.Range(150f, 350f);
        text_zone.GetComponent<Material>();
        s = Random.Range(0, 6);
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = 1;
        }
    }
    void Update()
    {
        if (Input.touchCount > 0)
        {
            OnValidate();
        }
        if (clicked)
        {
            StartSpin();
        }
        if (turn == 30 && allow)
        {
            Zone30Panel.SetActive(true);
            Zone30Panel.transform.GetChild(1).GetComponent<Image>().sprite = SuperZoneRewards[s];
        }
        else if (turn == 30 && !allow)
        {
            stick.GetComponent<SpriteRenderer>().sprite= atlas.GetSprite(spritenames[0]);
            text_zone.color = Color.yellow;
        }
        else if (turn % 5 == 0 && turn!=30)
        {
            text_zone.color = Color.green;
            stick.GetComponent<SpriteRenderer>().sprite = atlas.GetSprite(spritenames[1]);
            if (!assign)
            {
                RandomRewards();
            }
        }
        else
        {
            stick.GetComponent<SpriteRenderer>().sprite = atlas.GetSprite(spritenames[2]);
            text_zone.color = Color.white;
            if (!assign && turn !=1)
            {
                RandomRewards();
                OneBomb();
            }
            if (turn == 34)
            {
                Last();
            }
        }
    }
    public void OnValidate()
    {
            SpinButton.onClick.AddListener(ClickControl);
            Res_Button.onClick.AddListener(RestartGame);
            CollectButton.onClick.AddListener(Question);
            CollectButton2.onClick.AddListener(Last);
            BackButton.onClick.AddListener(Returning);
            AgainButton.onClick.AddListener(RestartGame);
            ContinueButton.onClick.AddListener(Close30);  
    }
    private void StartSpin()
    {
        SpinButton.interactable = false;
        CollectButton.interactable = false;
        anim_zone.SetBool("back", true);
        transform.Rotate(new Vector3(0f, 0f, -1f) * speed * Time.deltaTime);
        Invoke("BacktoNormal", duration);
        Invoke("BacktoNormal2", duration + 1.5f);
    }
    private void BacktoNormal()
    {
        transform.Rotate(Vector3.zero);
        gameObject.GetComponent<AudioSource>().mute = true;
        clicked = false;
    }
    private void BacktoNormal2()
    {
        assign = false;
        SpinButton.interactable = true;
        CollectButton.interactable = true;
        PickIcon();
        duration = Random.Range(3f, 6f);
        speed = Random.Range(150f, 300f);
        CancelInvoke();
    }
    private void PickIcon() // cikan ikona gore yanma veya devam etme
    {
        _hit = Physics2D.Raycast(stick.transform.position, stick.transform.TransformDirection(Vector2.down), 1f);
        if (_hit)
        {
            if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "ui_card_icon_death")
            {
                finish = true;
                StartCoroutine("GameOver");
            }
            else
            {
                gameObject.GetComponent<AudioSource>().mute = false;
                source.PlayOneShot(sounds[3]);
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "ui_icon_aviator_glasses_easter") Looting();
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "ui_icon_baseball_cap_easter") Looting();
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_icon_cash") Looting();
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "ui_icon_render_cons_grenade_m67") Looting();
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_icon_gold") Looting();
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_Icons_Armor_Points") Looting();
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_Icons_Pistol_Points") Looting();
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_Icons_Rifle_Points") Looting();
                if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_Icons_Sniper_Points") Looting();
            }
        }      
    }
    private void Looting() // loot panelinde kazanilan odulun ikonunu acma islemi ve siradaki zone'a gecme
    {
        turn++;
        text_zone.text = turn.ToString();
        anim_zone.Play("Zone_ani");
        for (int i = 0; i < Icons.Length; i++)
        {
            if (_hit.transform.GetComponent<SpriteRenderer>().sprite == Icons[i].sprite)
            {
                Choose();
                Icons[i].gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "x" + k.ToString();
                break;
            }
            else if (_hit.transform.GetComponent<SpriteRenderer>().sprite != Icons[i].sprite && Icons[i].sprite == null)
            {
                Icons[i].sprite = _hit.transform.GetComponent<SpriteRenderer>().sprite;
                Icons[i].GetComponent<Image>().color = new Color(255, 255, 255, 1);
                break;
            }
        }
    }
    private void Choose() // loot panelindeki odul sayisi hesaplama islemi
    {
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "ui_icon_aviator_glasses_easter")
        {
            numbers[0]++;
            k = numbers[0];
        }
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "ui_icon_baseball_cap_easter")
        {
            numbers[1]++;
            k = numbers[1];
        }
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_icon_cash")
        {
            numbers[2]++;
            k = numbers[2];
        }
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "ui_icon_render_cons_grenade_m67")
        {
            numbers[3]++;
            k = numbers[3];
        }
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_icon_gold")
        {
            numbers[4]++;
            k = numbers[4];
        }
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_Icons_Armor_Points")
        {
            numbers[5]++;
            k = numbers[5];
        }
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_Icons_Pistol_Points")
        {
            numbers[6]++;
            k = numbers[6];
        }
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_Icons_Rifle_Points")
        {
            numbers[7]++;
            k = numbers[7];
        }
        if (_hit.transform.GetComponent<SpriteRenderer>().sprite.name == "UI_Icons_Sniper_Points")
        {
            numbers[8]++;
            k = numbers[8];
        }
    }
    private void RandomRewards() // her cark dilimine farklý odulun getirilmesi islemi
    {
        if (!finish)
        {
            count[0] = Random.Range(0, 9);
            liste.Add(count[0]);
            while (i < 5)
            {
                count[i] = Random.Range(0, 9);
                if (!liste.Contains(count[i]))
                {
                    liste.Add(count[i]);
                    i++;
                }
                if (liste.Count == 6)
                {
                    break;
                }
            }
            pieces[0].GetComponent<SpriteRenderer>().sprite = rewards[liste[0]];
            pieces[1].GetComponent<SpriteRenderer>().sprite = rewards[liste[1]];
            pieces[2].GetComponent<SpriteRenderer>().sprite = rewards[liste[2]];
            pieces[3].GetComponent<SpriteRenderer>().sprite = rewards[liste[3]];
            pieces[4].GetComponent<SpriteRenderer>().sprite = rewards[liste[4]];
            pieces[5].GetComponent<SpriteRenderer>().sprite = rewards[liste[5]];
            assign = true;
            i = 0;
            liste.Clear();
        }
    }
    private void OneBomb()
    {
        if (!finish)
        {
            f = Random.Range(0, 6);
            pieces[f].GetComponent<SpriteRenderer>().sprite = bomb;
            assign = true;
        }
    }
    private void ClickControl()
    {
        clicked = true;
        source.PlayOneShot(sounds[4]);
    }
    private IEnumerator GameOver()
    {
        yield return new WaitForSecondsRealtime(.025f);
        gameObject.GetComponent<AudioSource>().mute = false;
        RestartPanel.SetActive(true);
        RestartPanel.transform.DOMove(QuesPan.transform.position,.025f);
        source.PlayOneShot(sounds[0]);
    }
    private void Last() // reward panel ile kazanilan odullerin gösterimi
    {
        RewardPanel.SetActive(true);
        source.PlayOneShot(sounds[5]);
        for (int i = 0; i < LastIcons.Length; i++)
        {
            if (Icons[i].sprite != null)
            {
                LastIcons[i].SetActive(true);
                LastIcons[i].GetComponent<Image>().sprite = Icons[i].sprite;
                LastIcons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Icons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
            }
            else
            {
                break;
            }
        }
        RewardPanel.transform.DOMove(QuesPan.transform.position, .5f);
    }
    private void RestartGame()
    {
        source.PlayOneShot(sounds[2]);
        SceneManager.LoadScene(0);
        CancelInvoke();
    }
    private void Close30()
    {
        source.PlayOneShot(sounds[2]);
        allow = false;
        Zone30Panel.SetActive(false);
    }
    private void Question()
    {
        source.PlayOneShot(sounds[2]);
        QuesPan.SetActive(true);
    }
    private void Returning()
    {
        source.PlayOneShot(sounds[1]);
        QuesPan.SetActive(false);
    }
    private void SlicesContent()
    {
        pieces[0].GetComponent<SpriteRenderer>().sprite = pieceIcons[0];
        pieces[1].GetComponent<SpriteRenderer>().sprite = pieceIcons[1];
        pieces[2].GetComponent<SpriteRenderer>().sprite = pieceIcons[2];
        pieces[3].GetComponent<SpriteRenderer>().sprite = pieceIcons[3];
        pieces[4].GetComponent<SpriteRenderer>().sprite = pieceIcons[4];
        pieces[5].GetComponent<SpriteRenderer>().sprite = pieceIcons[5];
    }
}