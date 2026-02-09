

using System.Collections.Generic;
using System.Linq;

using UnityEngine;


public class SortOrder : MonoBehaviour
{

    public List<GameObject> _decorSprites;
    private GameObject _previousClosestSprite;

    private void Start()
    {
 
        GameObject[] decors = GameObject.FindGameObjectsWithTag("Decor");

        foreach (var decor in decors)
        {
            _decorSprites.Add(decor);
        }
    }
    private void Update()
    {
        var playerY = transform.position.y;

        var closestBelow = _decorSprites
            .Where(sprite => sprite.transform.position.y < playerY)
            .OrderBy(sprite => Vector3.Distance(sprite.transform.position, transform.position))
            .FirstOrDefault();

        if (_previousClosestSprite != null && _previousClosestSprite != closestBelow)
        {
            var decor = _previousClosestSprite.GetComponent<SpriteRenderer>();
                decor.sortingOrder = 0;
            var color = decor.color;
            color.a = 1f;
            decor.color = color;
        }

    
        if (closestBelow != null)
        {

            closestBelow.GetComponent<SpriteRenderer>().sortingOrder = 2;
            var color = closestBelow.GetComponent<SpriteRenderer>().color;
            color.a = 0.5f;
            closestBelow.GetComponent<SpriteRenderer>().color = color;
            _previousClosestSprite = closestBelow;
        }
    }
}
