using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameControl : MonoBehaviour//,IBeginDragHandler,IDragHandler,IEndDragHandler
{

    //资源加载路径
    public string spritePath = "Sprites/Teriri";

    //拼图碎片
    public Sprite[] sprites = null;

    //拼图物体
    public List<GameObject> gameOBJs = null;

    //拼图控制（以按钮的形式）
    public Button[] buttons = null;

    //携带网格方块自动布局组件的背景
    public Transform bgTransform = null;

    //碎片预制体
    public GameObject puzzlePrefab = null;

    //用于记录空图片的位置
    public RectTransform nullImage = null;

    //用于获取布局方块的大小，便于调节碎片
    private GridLayoutGroup bgLayout = null;

    //用于记录正确的图片名字相加顺序
    private string correctSpritesNameOrder = null;

    //用于确认按钮化的图片碎片是否正确
    private string spritesNameOrder = null;


    private Sprite remainSprite = null;//用于保存被置空的图片，最后游戏结束时现实出来

    //记录摄像机的位置
    private Transform tempParent;


    //此函数用于初始化
    private void Start()
    {
        tempParent = GameObject.Find("Canvas").transform;

        string theLastSpriteName = null;

        //资源加载
        this.sprites = Resources.LoadAll<Sprite>(this.spritePath);
        this.bgLayout = bgTransform.GetComponent<GridLayoutGroup>();

        List<Sprite> tempSprites = new List<Sprite>();

        //记录正确的精灵名字顺序
        for (int i = 0; i < this.sprites.Length; i++)
        {
            tempSprites.Add(this.sprites[i]);
            this.correctSpritesNameOrder += this.sprites[i].name;
            //记录下最后一张碎片的名字
            if (i == this.sprites.Length - 1)
            {
                theLastSpriteName = this.sprites[i].name;
            }
        }

        print("正确的图片名字构成顺序：" + this.correctSpritesNameOrder.ToString());

        int spritesLength = this.sprites.Length;
        //Debug.Log(spritesLength);
        for (int i = spritesLength - 1; i >= 0; i--)
        {
            //实例化预制体
            GameObject btnInstance = Instantiate(this.puzzlePrefab) as GameObject;
            gameOBJs.Add(btnInstance);
            //获取按钮组件
            Button btn = btnInstance.GetComponent<Button>();

            GameControl self = this;//要保存一下当前的this对象,以免出现this指向错误
            btn.onClick.AddListener(delegate () {
                self.BtnOnclick(btnInstance.GetComponent<RectTransform>());
            });

            //随机取图片出来放置，达到打乱的效果
            int randomNumber = Random.Range(0, tempSprites.Count);
            btnInstance.name = tempSprites[randomNumber].name;

            Image btnInstanceImage = btnInstance.GetComponent<Image>();
            if (btnInstance.name == theLastSpriteName)
            {
                //置空，否则打乱
                this.remainSprite = tempSprites[randomNumber];
                btnInstanceImage.sprite = null;
                this.nullImage = btnInstance.GetComponent<RectTransform>();
            }
            else
            {
                //btnInstance.GetComponent<Image>().sprite = tempSprites[randomNumber];
                btnInstanceImage.sprite = tempSprites[randomNumber];
            }

            //Debug.Log(btnInstanceImage);

            tempSprites.Remove(tempSprites[randomNumber]);
            btnInstance.transform.SetParent(bgTransform);
            btnInstance.GetComponent<RectTransform>().localScale = Vector3.one;
            //Debug.Log(randomNumber + ":" + btnInstance.name);
        }

        //this.buttons = this.transform.Find("BackGround").GetComponentsInChildren<Button>();

        ////随机放置一个空图片位置
        //int tempNumber = Random.Range(0, this.buttons.Length);
        //this.remainSprite = buttons[tempNumber].GetComponent<Image>().sprite;//保存被置空的图片
        //this.buttons[tempNumber].GetComponent<Image>().sprite = null;
        //this.nullImage = this.buttons[tempNumber].GetComponent<RectTransform>();

        //设置最右下角的图片为消失的图片
    }

    //根据游戏模式刷新图片
    public void RenewGame() {

        for (int i = 0; i < gameOBJs.Count; i++)
        {
            //随机随两个数让其进行交换
            int randomNumber1 = Random.Range(0, gameOBJs.Count - 1);
            int randomNumber2 = Random.Range(0, gameOBJs.Count - 1);

            if (randomNumber1 != randomNumber2)
            {
                Sprite cacheSprite = gameOBJs[randomNumber1].GetComponent<Image>().sprite;//交换前缓存一下图片
                string cacheString = gameOBJs[randomNumber1].name;

                gameOBJs[randomNumber1].name = gameOBJs[randomNumber2].name;
                gameOBJs[randomNumber2].name = cacheString;

                gameOBJs[randomNumber1].GetComponent<Image>().sprite = gameOBJs[randomNumber2].GetComponent<Image>().sprite;
                gameOBJs[randomNumber2].GetComponent<Image>().sprite = cacheSprite;
            }

            if (gameOBJs[randomNumber1] == nullImage.gameObject)
            {
                this.nullImage = gameOBJs[randomNumber2].GetComponent<RectTransform>();
            }
            else if (gameOBJs[randomNumber2].gameObject == nullImage.gameObject)
            {
                this.nullImage = gameOBJs[randomNumber1].GetComponent<RectTransform>();
            }
        }

        switch (Buttons.state)
        {
            case GameState.easy:
                nullImage.GetComponent<Image>().sprite = this.remainSprite;
                break;
            case GameState.diff:
                nullImage.GetComponent<Image>().sprite = null;
                break;
        }
    }

    //拼图按钮点击事件
    private void BtnOnclick(RectTransform btnRect)
    {
        switch (Buttons.state)
        {
            case GameState.easy:
                Debug.Log("简单模式");
                break;
            case GameState.diff:
                if (Vector2.Distance(btnRect.anchoredPosition, this.nullImage.anchoredPosition)
                <= (this.bgLayout.cellSize.x + this.bgLayout.spacing.x))
                {
                    //Sprite nullImageSprite = this.nullImage.GetComponent<Image>().sprite;
                    Sprite cacheSprite = btnRect.GetComponent<Image>().sprite;//交换前缓存一下图片
                    string cacheString = btnRect.gameObject.name;

                    btnRect.gameObject.name = this.nullImage.gameObject.name;
                    this.nullImage.gameObject.name = cacheString;

                    btnRect.GetComponent<Image>().sprite = this.nullImage.GetComponent<Image>().sprite;
                    this.nullImage.GetComponent<Image>().sprite = cacheSprite;
                    this.nullImage = btnRect;

                    CheckAll();
                }
                break;
        }
    }

    //遍历所有图片名字顺序，判断是否完成拼图，结束游戏
    private void CheckAll()
    {
        this.spritesNameOrder = "";
        this.buttons = this.bgTransform.GetComponentsInChildren<Button>();
        for (int i = 0; i < this.buttons.Length; i++)
        {
            this.spritesNameOrder += this.buttons[i].gameObject.name;
        }

        if (this.spritesNameOrder == this.correctSpritesNameOrder)
        {
            this.nullImage.GetComponent<Image>().sprite = this.remainSprite;//现实被置空的图片
            this.enabled = false;//停止游戏刷新
        }
    }

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    //拖拽开始时记下自己的父物体.
    //     myParent = transform.parent;
    //    //拖拽开始时禁用检测.
    //    cg.blocksRaycasts = false;
    //    this.transform.SetParent(tempParent);
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    throw new System.NotImplementedException();
    //}
}