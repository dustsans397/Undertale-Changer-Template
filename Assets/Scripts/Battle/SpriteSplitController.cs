using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Contains a Pool.
/// Implement the fragmentation effect of sprites.
/// Line by line from top to bottom.
/// Effective immediately after enabling this script
/// </summary>
public class SpriteSplitController : MonoBehaviour
{
    Queue<GameObject> available = new Queue<GameObject>();//Object pool
    Texture2D map;
    GameObject Mask;
    public int poolCount;
    public List<Color> colorExclude;
    public Vector2 startPos;//The particle calculates the relative coordinates of the upper left corner of the image
    public float speed;//Particle generation speed
    void Awake()
    {
        map = GetComponent<SpriteRenderer>().sprite.texture;
        Mask = transform.Find("Mask").gameObject;
    }
    void OnEnable()
    {
        startPos = new Vector2(-map.width / 2 * 0.05f, map.height / 2 * 0.05f);
        if (map.width % 2 == 0)
            startPos += new Vector2(0.025f, 0);
        if (map.height % 2 == 0)
            startPos -= new Vector2(0, 0.025f);

        Mask.transform.localScale = new Vector2(map.width, map.height);
        Mask.transform.localPosition = new Vector3(0, 0.05f * map.height);
        StartCoroutine(SummonPixel());
       
    }
    IEnumerator SummonPixel()
    {
        for (int y = map.height - 1; y >= 0; y--)
        {
            for (int x = 0; x < map.width; x++)
            {
                bool skip = false;
                Color color = map.GetPixel(x, y);
                for (int i = 0; i < colorExclude.Count; i++)
                {
                    if (color == colorExclude[i])
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip)
                {
                    continue;
                }
                else
                {

                    GameObject obj = GetFromPool();
                    obj.GetComponent<SpriteRenderer>().color = color;
                    obj.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;

                    obj.transform.localPosition = startPos + new Vector2(x * 0.05f, -(map.height - y - 1) * 0.05f);
               
                }

            }
            Mask.transform.localPosition -= new Vector3(0, 0.05f);
            yield return new WaitForSeconds(speed);
        }
    }

    //-----Object Pool Section-----

    /// <summary>
    /// Initialize/Fill Object Pool
    /// </summary>
    public void FillPool()
    {
        for (int i = 0; i < poolCount; i++)
        {
            var newObj = Instantiate(Resources.Load<GameObject>("Template/Square Template"), transform);
            ReturnPool(newObj);
        }
    }
    /// <summary>
    /// Return Object Pool
    /// </summary>
    public void ReturnPool(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform);
        available.Enqueue(gameObject);
    }
    /// <summary>
    /// Obtaining Square
    /// </summary>
    public GameObject GetFromPool()
    {
        if (available.Count == 0)
            FillPool();

        var square = available.Dequeue();

        square.SetActive(true);
        return square;
    }


}
